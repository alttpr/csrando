import { beforeAll, beforeEach, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import {
  rawMetadataFixture,
  loadMetadataFixture,
  defaultFormStateFixture,
} from "../fixtures/metadata";
import { normalizeConfig, type NormalizedConfig } from "$lib/config/normalize";
import {
  configurationProfiles,
  configurationProfileRevisions,
  users,
} from "$lib/server/db/schema";

vi.mock("$env/dynamic/private", () => ({
  env: new Proxy(
    {},
    { get: (_target, key: string) => process.env[key] ?? undefined },
  ),
}));

vi.mock("$app/environment", () => ({
  browser: false,
  dev: true,
  building: false,
  version: "test",
}));

vi.mock("$lib/services/api", () => ({
  metadataApi: {
    resolveCanonicalId: vi.fn(async (id: string) => id),
    getById: vi.fn(async () => rawMetadataFixture),
  },
}));

type DbModule = typeof import("$lib/server/db");
type ProfilesRoute = typeof import("../../src/routes/api/profiles/+server");
type ShareRoute =
  typeof import("../../src/routes/api/profiles/[id]/share/+server");
type RevisionsRoute =
  typeof import("../../src/routes/api/profiles/[id]/revisions/+server");
type ProfileRoute = typeof import("../../src/routes/api/profiles/[id]/+server");
type ServiceModule = typeof import("$lib/server/profiles/service");
type SeedModule = typeof import("$lib/server/profiles/seed-official");

let db: DbModule["db"];
let profilesRoute: ProfilesRoute;
let shareRoute: ShareRoute;
let revisionsRoute: RevisionsRoute;
let byId: ProfileRoute;
let service: ServiceModule;
let seedModule: SeedModule;

const alice: User = {
  id: "share-alice-00001",
  username: "share-alice",
  githubId: null,
  isAdmin: false,
};
const bob: User = {
  id: "share-bob-0000001",
  username: "share-bob",
  githubId: null,
  isAdmin: false,
};

function locals(user: User | null) {
  return { user, session: null };
}

function jsonRequest(body: unknown): Request {
  return new Request("http://localhost/api/profiles", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
}

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

async function createProfile(user: User, name: string) {
  const response = await profilesRoute.POST({
    request: jsonRequest({
      configId: "combo",
      name,
      settings: defaultSettings(),
    }),
    locals: locals(user),
  } as never);
  return await response.json();
}

async function share(user: User | null, profileId: string) {
  const response = await shareRoute.POST({
    params: { id: profileId },
    locals: locals(user),
  } as never);
  return await response.json();
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  profilesRoute = await import("../../src/routes/api/profiles/+server");
  shareRoute = await import("../../src/routes/api/profiles/[id]/share/+server");
  revisionsRoute = await import(
    "../../src/routes/api/profiles/[id]/revisions/+server"
  );
  byId = await import("../../src/routes/api/profiles/[id]/+server");
  service = await import("$lib/server/profiles/service");
  seedModule = await import("$lib/server/profiles/seed-official");

  for (const user of [alice, bob]) {
    await db.insert(users).values({
      id: user.id,
      username: user.username,
      isAdmin: user.isAdmin,
    });
  }
});

beforeEach(async () => {
  await db.delete(configurationProfileRevisions);
  await db.delete(configurationProfiles);
  seedModule.resetSeedStateForTests();
});

describe("profile share links", () => {
  it("creates a stable token for the owner and resolves it", async () => {
    const created = await createProfile(alice, "Shareable");

    const first = await share(alice, created.profile.id);
    expect(first.token).toMatch(/^[0-9A-Za-z]{24}$/);
    // Idempotent: sharing again returns the same token.
    const second = await share(alice, created.profile.id);
    expect(second.token).toBe(first.token);

    const shared = await service.getSharedProfile(first.token);
    expect(shared).not.toBeNull();
    expect(shared!.profile.id).toBe(created.profile.id);
    expect(shared!.profile.name).toBe("Shareable");
    expect(shared!.revision.id).toBe(created.revision.id);
  });

  it("requires authentication and ownership", async () => {
    const created = await createProfile(alice, "Private share");
    await expect(share(null, created.profile.id)).rejects.toMatchObject({
      status: 401,
    });
    await expect(share(bob, created.profile.id)).rejects.toMatchObject({
      status: 404,
    });
    await expect(
      shareRoute.DELETE({
        params: { id: created.profile.id },
        locals: locals(bob),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("shares the current revision, following updates", async () => {
    const created = await createProfile(alice, "Living");
    const { token } = await share(alice, created.profile.id);

    const settings = defaultSettings();
    settings.perGame.Alttpr.Swords = "Assured";
    await revisionsRoute.POST({
      params: { id: created.profile.id },
      request: jsonRequest({
        settings,
        baseRevisionId: created.revision.id,
      }),
      locals: locals(alice),
    } as never);

    const shared = await service.getSharedProfile(token);
    expect(
      (shared!.revision.settings as NormalizedConfig).perGame.Alttpr.Swords,
    ).toBe("Assured");
  });

  it("stops resolving after revocation or deletion", async () => {
    const created = await createProfile(alice, "Revocable");
    const { token } = await share(alice, created.profile.id);

    const response = await shareRoute.DELETE({
      params: { id: created.profile.id },
      locals: locals(alice),
    } as never);
    expect((await response.json()).ok).toBe(true);
    expect(await service.getSharedProfile(token)).toBeNull();

    // Re-sharing issues a fresh token; deleting the profile kills it too.
    const { token: token2 } = await share(alice, created.profile.id);
    expect(token2).not.toBe(token);
    await byId.DELETE({
      params: { id: created.profile.id },
      locals: locals(alice),
    } as never);
    expect(await service.getSharedProfile(token2)).toBeNull();
  });

  it("never resolves unknown tokens and does not leak profile access", async () => {
    expect(
      await service.getSharedProfile("not-a-real-token00000000"),
    ).toBeNull();
    expect(await service.getSharedProfile("")).toBeNull();

    // Knowing the token does not open the regular profile API for others.
    const created = await createProfile(alice, "Token only");
    await share(alice, created.profile.id);
    await expect(
      byId.GET({
        params: { id: created.profile.id },
        url: new URL(`http://localhost/api/profiles/${created.profile.id}`),
        locals: locals(bob),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });
});
