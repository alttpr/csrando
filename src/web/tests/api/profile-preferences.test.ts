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
  userProfileFavorites,
  userProfilePreferences,
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
type PreferencesRoute =
  typeof import("../../src/routes/api/user/profile-preferences/+server");
type FavoritesRoute =
  typeof import("../../src/routes/api/user/profile-favorites/+server");
type SeedModule = typeof import("$lib/server/profiles/seed-official");

let db: DbModule["db"];
let listAndCreate: ProfilesRoute;
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
  return new Request("http://localhost/api/user/profile-preferences", {
    method,
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
  listAndCreate = await import("../../src/routes/api/profiles/+server");
  preferences = await import(
    "../../src/routes/api/user/profile-preferences/+server"
  );
  favorites = await import(
    "../../src/routes/api/user/profile-favorites/+server"
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
  await db.delete(userProfileFavorites);
  await db.delete(userProfilePreferences);
  await db.delete(configurationProfileRevisions);
  await db.delete(configurationProfiles);
  seedModule.resetSeedStateForTests();
});

describe("PUT /api/user/profile-preferences", () => {
  it("requires authentication", async () => {
    await expect(
      putPreferences(null, { defaultProfileId: "x" }),
    ).rejects.toMatchObject({ status: 401 });
  });

  it("upserts default and last-used independently", async () => {
    const a = await createProfile(alice, "A");
    const b = await createProfile(alice, "B");

    let result = await putPreferences(alice, {
      defaultProfileId: a.profile.id,
    });
    expect(result.preferences.defaultProfileId).toBe(a.profile.id);
    expect(result.preferences.lastUsedProfileId).toBeNull();

    result = await putPreferences(alice, { lastUsedProfileId: b.profile.id });
    expect(result.preferences.defaultProfileId).toBe(a.profile.id);
    expect(result.preferences.lastUsedProfileId).toBe(b.profile.id);

    result = await putPreferences(alice, { defaultProfileId: null });
    expect(result.preferences.defaultProfileId).toBeNull();
    expect(result.preferences.lastUsedProfileId).toBe(b.profile.id);
  });

  it("rejects a default pointing at another user's private profile", async () => {
    const secret = await createProfile(bob, "Bob secret");
    await expect(
      putPreferences(alice, { defaultProfileId: secret.profile.id }),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("allows an official preset as default", async () => {
    const list = await (
      await listAndCreate.GET({
        url: new URL("http://localhost/api/profiles?configId=combo"),
        locals: locals(alice),
      } as never)
    ).json();
    const result = await putPreferences(alice, {
      defaultProfileId: list.recommendedId,
    });
    expect(result.preferences.defaultProfileId).toBe(list.recommendedId);
  });
});

describe("PUT /api/user/profile-favorites", () => {
  it("adds a favorite exactly once and can remove it", async () => {
    const created = await createProfile(alice, "Pinned");

    await putFavorite(alice, {
      profileId: created.profile.id,
      favorited: true,
    });
    await putFavorite(alice, {
      profileId: created.profile.id,
      favorited: true,
      displayOrder: 3,
    });

    let list = await (
      await listAndCreate.GET({
        url: new URL("http://localhost/api/profiles?configId=combo"),
        locals: locals(alice),
      } as never)
    ).json();
    expect(list.preferences.favorites).toEqual([
      { profileId: created.profile.id, displayOrder: 3 },
    ]);

    await putFavorite(alice, {
      profileId: created.profile.id,
      favorited: false,
    });
    list = await (
      await listAndCreate.GET({
        url: new URL("http://localhost/api/profiles?configId=combo"),
        locals: locals(alice),
      } as never)
    ).json();
    expect(list.preferences.favorites).toEqual([]);
  });

  it("cannot favorite another user's private profile", async () => {
    const secret = await createProfile(bob, "Bob only");
    await expect(
      putFavorite(alice, { profileId: secret.profile.id, favorited: true }),
    ).rejects.toMatchObject({ status: 404 });
  });
});
