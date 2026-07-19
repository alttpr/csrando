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
  userPresetFavorites,
  userPresetPreferences,
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
type PreferencesRoute =
  typeof import("../../src/routes/api/user/preset-preferences/+server");
type FavoritesRoute =
  typeof import("../../src/routes/api/user/preset-favorites/+server");
type SeedModule = typeof import("$lib/server/presets/seed-official");

let db: DbModule["db"];
let listAndCreate: PresetsRoute;
let preferences: PreferencesRoute;
let favorites: FavoritesRoute;
let seedModule: SeedModule;

const alice: User = {
  id: "pref-alice-000001",
  username: "pref-alice",
  githubId: null,
  isAdmin: false,
};
const bob: User = {
  id: "pref-bob-00000001",
  username: "pref-bob",
  githubId: null,
  isAdmin: false,
};

function locals(user: User | null) {
  return { user, session: null };
}

function jsonRequest(method: string, body: unknown): Request {
  return new Request("http://localhost/api/user/preset-preferences", {
    method,
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
}

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

async function createPreset(user: User, name: string) {
  const response = await listAndCreate.POST({
    request: jsonRequest("POST", {
      configId: "combo",
      name,
      settings: defaultSettings(),
    }),
    locals: locals(user),
  } as never);
  return await response.json();
}

async function putPreferences(user: User | null, body: unknown) {
  const response = await preferences.PUT({
    request: jsonRequest("PUT", body),
    locals: locals(user),
  } as never);
  return await response.json();
}

async function putFavorite(user: User | null, body: unknown) {
  const response = await favorites.PUT({
    request: jsonRequest("PUT", body),
    locals: locals(user),
  } as never);
  return await response.json();
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  listAndCreate = await import("../../src/routes/api/presets/+server");
  preferences = await import(
    "../../src/routes/api/user/preset-preferences/+server"
  );
  favorites = await import(
    "../../src/routes/api/user/preset-favorites/+server"
  );
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
  await db.delete(userPresetFavorites);
  await db.delete(userPresetPreferences);
  await db.delete(configurationPresetRevisions);
  await db.delete(configurationPresets);
  seedModule.resetSeedStateForTests();
});

describe("PUT /api/user/preset-preferences", () => {
  it("requires authentication", async () => {
    await expect(
      putPreferences(null, { defaultPresetId: "x" }),
    ).rejects.toMatchObject({ status: 401 });
  });

  it("upserts default and last-used independently", async () => {
    const a = await createPreset(alice, "A");
    const b = await createPreset(alice, "B");

    let result = await putPreferences(alice, {
      defaultPresetId: a.preset.id,
    });
    expect(result.preferences.defaultPresetId).toBe(a.preset.id);
    expect(result.preferences.lastUsedPresetId).toBeNull();

    result = await putPreferences(alice, { lastUsedPresetId: b.preset.id });
    expect(result.preferences.defaultPresetId).toBe(a.preset.id);
    expect(result.preferences.lastUsedPresetId).toBe(b.preset.id);

    result = await putPreferences(alice, { defaultPresetId: null });
    expect(result.preferences.defaultPresetId).toBeNull();
    expect(result.preferences.lastUsedPresetId).toBe(b.preset.id);
  });

  it("rejects a default pointing at another user's private preset", async () => {
    const secret = await createPreset(bob, "Bob secret");
    await expect(
      putPreferences(alice, { defaultPresetId: secret.preset.id }),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("allows an official preset as default", async () => {
    const list = await (
      await listAndCreate.GET({
        url: new URL("http://localhost/api/presets?configId=combo"),
        locals: locals(alice),
      } as never)
    ).json();
    const result = await putPreferences(alice, {
      defaultPresetId: list.recommendedId,
    });
    expect(result.preferences.defaultPresetId).toBe(list.recommendedId);
  });
});

describe("PUT /api/user/preset-favorites", () => {
  it("adds a favorite exactly once and can remove it", async () => {
    const created = await createPreset(alice, "Pinned");

    await putFavorite(alice, {
      presetId: created.preset.id,
      favorited: true,
    });
    await putFavorite(alice, {
      presetId: created.preset.id,
      favorited: true,
      displayOrder: 3,
    });

    let list = await (
      await listAndCreate.GET({
        url: new URL("http://localhost/api/presets?configId=combo"),
        locals: locals(alice),
      } as never)
    ).json();
    expect(list.preferences.favorites).toEqual([
      { presetId: created.preset.id, displayOrder: 3 },
    ]);

    await putFavorite(alice, {
      presetId: created.preset.id,
      favorited: false,
    });
    list = await (
      await listAndCreate.GET({
        url: new URL("http://localhost/api/presets?configId=combo"),
        locals: locals(alice),
      } as never)
    ).json();
    expect(list.preferences.favorites).toEqual([]);
  });

  it("cannot favorite another user's private preset", async () => {
    const secret = await createPreset(bob, "Bob only");
    await expect(
      putFavorite(alice, { presetId: secret.preset.id, favorited: true }),
    ).rejects.toMatchObject({ status: 404 });
  });
});
