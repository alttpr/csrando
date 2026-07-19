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
type ProfileRoute = typeof import("../../src/routes/api/profiles/[id]/+server");
type RevisionsRoute =
  typeof import("../../src/routes/api/profiles/[id]/revisions/+server");
type SeedModule = typeof import("$lib/server/profiles/seed-official");

let db: DbModule["db"];
let listAndCreate: ProfilesRoute;
let byId: ProfileRoute;
let revisions: RevisionsRoute;
let seedModule: SeedModule;

const alice: User = {
  id: "rev-alice-0000001",
  username: "rev-alice",
  githubId: null,
  isAdmin: false,
};
const bob: User = {
  id: "rev-bob-000000001",
  username: "rev-bob",
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
  const response = await listAndCreate.POST({
    request: jsonRequest({
      configId: "combo",
      name,
      settings: defaultSettings(),
    }),
    locals: locals(user),
  } as never);
  return await response.json();
}

async function postRevision(user: User, profileId: string, body: unknown) {
  const response = await revisions.POST({
    params: { id: profileId },
    request: jsonRequest(body),
    locals: locals(user),
  } as never);
  return await response.json();
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  listAndCreate = await import("../../src/routes/api/profiles/+server");
  byId = await import("../../src/routes/api/profiles/[id]/+server");
  revisions = await import(
    "../../src/routes/api/profiles/[id]/revisions/+server"
  );
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

describe("POST /api/profiles/[id]/revisions", () => {
  it("creates a new revision and moves currentRevisionId", async () => {
    const created = await createProfile(alice, "Evolving");
    const settings = defaultSettings();
    settings.perGame.Alttpr.Swords = "Assured";

    const body = await postRevision(alice, created.profile.id, {
      settings,
      changeSummary: "assured swords",
      baseRevisionId: created.revision.id,
    });

    expect(body.revision.revisionNumber).toBe(2);
    expect(body.profile.currentRevisionId).toBe(body.revision.id);
    expect(
      (body.revision.settings as NormalizedConfig).perGame.Alttpr.Swords,
    ).toBe("Assured");
  });

  it("rejects stale base revisions with 409", async () => {
    const created = await createProfile(alice, "Contended");
    const settings = defaultSettings();
    await postRevision(alice, created.profile.id, {
      settings,
      baseRevisionId: created.revision.id,
    });

    // Second save from a stale tab still pointing at revision 1.
    await expect(
      postRevision(alice, created.profile.id, {
        settings,
        baseRevisionId: created.revision.id,
      }),
    ).rejects.toMatchObject({ status: 409 });
  });

  it("keeps older revisions immutable and readable", async () => {
    const created = await createProfile(alice, "History");
    const settings = defaultSettings();
    settings.perGame.Alttpr.KeyShuffle = true;
    await postRevision(alice, created.profile.id, {
      settings,
      baseRevisionId: created.revision.id,
    });

    const response = await byId.GET({
      params: { id: created.profile.id },
      url: new URL(
        `http://localhost/api/profiles/${created.profile.id}?revision=${created.revision.id}`,
      ),
      locals: locals(alice),
    } as never);
    const original = await response.json();
    expect(original.revision.id).toBe(created.revision.id);
    expect(original.revision.revisionNumber).toBe(1);
    expect(
      (original.revision.settings as NormalizedConfig).perGame.Alttpr
        .KeyShuffle,
    ).toBe(false);
  });

  it("rejects revisions on another user's profile", async () => {
    const created = await createProfile(alice, "Private");
    await expect(
      postRevision(bob, created.profile.id, {
        settings: defaultSettings(),
        baseRevisionId: created.revision.id,
      }),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("rejects non-admin revisions on official presets", async () => {
    const list = await (
      await listAndCreate.GET({
        url: new URL("http://localhost/api/profiles?configId=combo"),
        locals: locals(null),
      } as never)
    ).json();
    const official = list.officials[0];
    await expect(
      postRevision(alice, official.id, {
        settings: defaultSettings(),
        baseRevisionId: official.currentRevisionId,
      }),
    ).rejects.toMatchObject({ status: 403 });
  });

  it("rejects invalid settings with field errors", async () => {
    const created = await createProfile(alice, "Validated");
    const settings = defaultSettings();
    settings.perGame.Alttpr.Crystals = 999;
    await expect(
      postRevision(alice, created.profile.id, {
        settings,
        baseRevisionId: created.revision.id,
      }),
    ).rejects.toMatchObject({
      status: 400,
      body: { fieldErrors: { "Alttpr.Crystals": expect.any(String) } },
    });
  });
});
