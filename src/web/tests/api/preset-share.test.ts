import { beforeAll, beforeEach, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import {
  rawMetadataFixture,
  loadMetadataFixture,
  defaultFormStateFixture,
} from "../fixtures/metadata";
import { normalizeConfig, type NormalizedConfig } from "$lib/config/normalize";
import {
  configurationPresets,
  configurationPresetRevisions,
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
type PresetsRoute = typeof import("../../src/routes/api/presets/+server");
type ShareRoute =
  typeof import("../../src/routes/api/presets/[id]/share/+server");
type RevisionsRoute =
  typeof import("../../src/routes/api/presets/[id]/revisions/+server");
type PresetRoute = typeof import("../../src/routes/api/presets/[id]/+server");
type ServiceModule = typeof import("$lib/server/presets/service");
type SeedModule = typeof import("$lib/server/presets/seed-official");

let db: DbModule["db"];
let presetsRoute: PresetsRoute;
let shareRoute: ShareRoute;
let revisionsRoute: RevisionsRoute;
let byId: PresetRoute;
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
  return new Request("http://localhost/api/presets", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
}

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

async function createPreset(user: User, name: string) {
  const response = await presetsRoute.POST({
    request: jsonRequest({
      configId: "combo",
      name,
      settings: defaultSettings(),
    }),
    locals: locals(user),
  } as never);
  return await response.json();
}

async function share(user: User | null, presetId: string) {
  const response = await shareRoute.POST({
    params: { id: presetId },
    locals: locals(user),
  } as never);
  return await response.json();
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  presetsRoute = await import("../../src/routes/api/presets/+server");
  shareRoute = await import("../../src/routes/api/presets/[id]/share/+server");
  revisionsRoute = await import(
    "../../src/routes/api/presets/[id]/revisions/+server"
  );
  byId = await import("../../src/routes/api/presets/[id]/+server");
  service = await import("$lib/server/presets/service");
  seedModule = await import("$lib/server/presets/seed-official");

  for (const user of [alice, bob]) {
    await db.insert(users).values({
      id: user.id,
      username: user.username,
      isAdmin: user.isAdmin,
    });
  }
});

beforeEach(async () => {
  await db.delete(configurationPresetRevisions);
  await db.delete(configurationPresets);
  seedModule.resetSeedStateForTests();
});

describe("preset share links", () => {
  it("creates a stable token for the owner and resolves it", async () => {
    const created = await createPreset(alice, "Shareable");

    const first = await share(alice, created.preset.id);
    expect(first.token).toMatch(/^[0-9A-Za-z]{24}$/);
    // Idempotent: sharing again returns the same token.
    const second = await share(alice, created.preset.id);
    expect(second.token).toBe(first.token);

    const shared = await service.getSharedPreset(first.token);
    expect(shared).not.toBeNull();
    expect(shared!.preset.id).toBe(created.preset.id);
    expect(shared!.preset.name).toBe("Shareable");
    expect(shared!.revision.id).toBe(created.revision.id);
  });

  it("requires authentication and ownership", async () => {
    const created = await createPreset(alice, "Private share");
    await expect(share(null, created.preset.id)).rejects.toMatchObject({
      status: 401,
    });
    await expect(share(bob, created.preset.id)).rejects.toMatchObject({
      status: 404,
    });
    await expect(
      shareRoute.DELETE({
        params: { id: created.preset.id },
        locals: locals(bob),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("shares the current revision, following updates", async () => {
    const created = await createPreset(alice, "Living");
    const { token } = await share(alice, created.preset.id);

    const settings = defaultSettings();
    settings.perGame.Alttpr.Swords = "Assured";
    await revisionsRoute.POST({
      params: { id: created.preset.id },
      request: jsonRequest({
        settings,
        baseRevisionId: created.revision.id,
      }),
      locals: locals(alice),
    } as never);

    const shared = await service.getSharedPreset(token);
    expect(
      (shared!.revision.settings as NormalizedConfig).perGame.Alttpr.Swords,
    ).toBe("Assured");
  });

  it("stops resolving after revocation or deletion", async () => {
    const created = await createPreset(alice, "Revocable");
    const { token } = await share(alice, created.preset.id);

    const response = await shareRoute.DELETE({
      params: { id: created.preset.id },
      locals: locals(alice),
    } as never);
    expect((await response.json()).ok).toBe(true);
    expect(await service.getSharedPreset(token)).toBeNull();

    // Re-sharing issues a fresh token; deleting the preset kills it too.
    const { token: token2 } = await share(alice, created.preset.id);
    expect(token2).not.toBe(token);
    await byId.DELETE({
      params: { id: created.preset.id },
      locals: locals(alice),
    } as never);
    expect(await service.getSharedPreset(token2)).toBeNull();
  });

  it("never resolves unknown tokens and does not leak preset access", async () => {
    expect(
      await service.getSharedPreset("not-a-real-token00000000"),
    ).toBeNull();
    expect(await service.getSharedPreset("")).toBeNull();

    // Knowing the token does not open the regular preset API for others.
    const created = await createPreset(alice, "Token only");
    await share(alice, created.preset.id);
    await expect(
      byId.GET({
        params: { id: created.preset.id },
        url: new URL(`http://localhost/api/presets/${created.preset.id}`),
        locals: locals(bob),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });
});
