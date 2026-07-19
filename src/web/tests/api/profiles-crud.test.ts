import { beforeAll, beforeEach, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import {
  rawMetadataFixture,
  loadMetadataFixture,
  defaultFormStateFixture,
} from "../fixtures/metadata";
import { normalizeConfig, type NormalizedConfig } from "$lib/config/normalize";
import {
  MAX_SETTINGS_JSON_BYTES,
  MAX_USER_PROFILES,
} from "$lib/config/constants";
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
type ProfileRoute = typeof import("../../src/routes/api/profiles/[id]/+server");
type DuplicateRoute =
  typeof import("../../src/routes/api/profiles/[id]/duplicate/+server");
type SeedModule = typeof import("$lib/server/profiles/seed-official");

let db: DbModule["db"];
let listAndCreate: ProfilesRoute;
let byId: ProfileRoute;
let duplicate: DuplicateRoute;
let seedModule: SeedModule;

const alice: User = {
  id: "user-alice-000001",
  username: "alice",
  githubId: null,
  isAdmin: false,
};
const bob: User = {
  id: "user-bob-00000001",
  username: "bob",
  githubId: null,
  isAdmin: false,
};
const admin: User = {
  id: "user-admin-000001",
  username: "admin",
  githubId: null,
  isAdmin: true,
};

function locals(user: User | null) {
  return { user, session: null };
}

function jsonRequest(method: string, body?: unknown): Request {
  return new Request("http://localhost/api/profiles", {
    method,
    headers: { "content-type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
}

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

async function listProfiles(user: User | null) {
  const response = await listAndCreate.GET({
    url: new URL("http://localhost/api/profiles?configId=combo"),
    locals: locals(user),
  } as never);
  return await response.json();
}

async function createProfile(user: User | null, body: unknown) {
  const response = await listAndCreate.POST({
    request: jsonRequest("POST", body),
    locals: locals(user),
  } as never);
  return await response.json();
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  listAndCreate = await import("../../src/routes/api/profiles/+server");
  byId = await import("../../src/routes/api/profiles/[id]/+server");
  duplicate = await import(
    "../../src/routes/api/profiles/[id]/duplicate/+server"
  );
  seedModule = await import("$lib/server/profiles/seed-official");

  for (const user of [alice, bob, admin]) {
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

describe("GET /api/profiles", () => {
  it("lists seeded official presets for anonymous users", async () => {
    const body = await listProfiles(null);
    expect(body.officials.length).toBeGreaterThan(0);
    const recommended = body.officials.find(
      (p: { slug: string }) => p.slug === "recommended",
    );
    expect(recommended).toBeTruthy();
    expect(recommended.scope).toBe("official");
    expect(recommended.selectedGames.length).toBeGreaterThan(0);
    expect(body.recommendedId).toBe(recommended.id);
    expect(body.mine).toEqual([]);
    expect(body.preferences).toBeNull();
  });

  it("requires a configId", async () => {
    await expect(
      listAndCreate.GET({
        url: new URL("http://localhost/api/profiles"),
        locals: locals(null),
      } as never),
    ).rejects.toMatchObject({ status: 400 });
  });

  it("matches profiles regardless of configId casing (page-server bootstrap)", async () => {
    // The config page load passes the backend's canonical id, which may be
    // capitalized (e.g. "Combo"); stored config ids are lowercase.
    const { listProfilesFor } = await import("$lib/server/profiles/service");
    await createProfile(alice, {
      configId: "combo",
      name: "Case test",
      settings: defaultSettings(),
    });

    const result = await listProfilesFor("Combo", alice);
    expect(result.officials.map((p) => p.slug)).toContain("recommended");
    expect(result.mine.map((p) => p.name)).toContain("Case test");
  });

  it("only shows a user's own profiles in mine", async () => {
    await createProfile(alice, {
      configId: "combo",
      name: "Alice profile",
      settings: defaultSettings(),
    });

    const forAlice = await listProfiles(alice);
    expect(forAlice.mine.map((p: { name: string }) => p.name)).toEqual([
      "Alice profile",
    ]);
    expect(forAlice.preferences).not.toBeNull();

    const forBob = await listProfiles(bob);
    expect(forBob.mine).toEqual([]);
  });
});

describe("POST /api/profiles", () => {
  it("requires authentication", async () => {
    await expect(
      createProfile(null, {
        configId: "combo",
        name: "anon",
        settings: defaultSettings(),
      }),
    ).rejects.toMatchObject({ status: 401 });
  });

  it("creates a user profile with revision 1", async () => {
    const body = await createProfile(alice, {
      configId: "combo",
      name: "My Racing Setup",
      description: "fast seeds",
      settings: defaultSettings(),
    });
    expect(body.profile.scope).toBe("user");
    expect(body.profile.name).toBe("My Racing Setup");
    expect(body.profile.currentRevisionId).toBe(body.revision.id);
    expect(body.revision.revisionNumber).toBe(1);
    expect(body.revision.configSchemaVersion).toBeGreaterThanOrEqual(1);
  });

  it("supports setAsDefault and favorite on create", async () => {
    const body = await createProfile(alice, {
      configId: "combo",
      name: "Defaulted",
      settings: defaultSettings(),
      setAsDefault: true,
      favorite: true,
    });
    const list = await listProfiles(alice);
    expect(list.preferences.defaultProfileId).toBe(body.profile.id);
    expect(
      list.preferences.favorites.map((f: { profileId: string }) => f.profileId),
    ).toContain(body.profile.id);
  });

  it("rejects invalid names", async () => {
    await expect(
      createProfile(alice, {
        configId: "combo",
        name: "",
        settings: defaultSettings(),
      }),
    ).rejects.toMatchObject({ status: 400 });
    await expect(
      createProfile(alice, {
        configId: "combo",
        name: "x".repeat(61),
        settings: defaultSettings(),
      }),
    ).rejects.toMatchObject({ status: 400 });
  });

  it("rejects duplicate names case-insensitively", async () => {
    await createProfile(alice, {
      configId: "combo",
      name: "Weekly Race",
      settings: defaultSettings(),
    });
    await expect(
      createProfile(alice, {
        configId: "combo",
        name: "weekly race",
        settings: defaultSettings(),
      }),
    ).rejects.toMatchObject({
      status: 400,
      body: { fieldErrors: { name: expect.any(String) } },
    });
  });

  it("rejects settings with unknown or invalid values", async () => {
    const settings = defaultSettings();
    (settings.perGame.Alttpr as Record<string, unknown>).NotARealSetting = 1;
    await expect(
      createProfile(alice, { configId: "combo", name: "bad", settings }),
    ).rejects.toMatchObject({
      status: 400,
      body: { fieldErrors: { "Alttpr.NotARealSetting": expect.any(String) } },
    });

    const invalid = defaultSettings();
    invalid.perGame.Alttpr.Swords = "NoSuchSword";
    await expect(
      createProfile(alice, {
        configId: "combo",
        name: "bad2",
        settings: invalid,
      }),
    ).rejects.toMatchObject({ status: 400 });
  });

  it("rejects oversized settings payloads", async () => {
    const settings = defaultSettings();
    settings.perGame.Alttpr.Notes = "x".repeat(MAX_SETTINGS_JSON_BYTES);
    await expect(
      createProfile(alice, { configId: "combo", name: "big", settings }),
    ).rejects.toMatchObject({ status: 400 });
  });

  it("enforces the per-user profile limit", async () => {
    const settings = defaultSettings();
    const values = [];
    const now = new Date();
    for (let i = 0; i < MAX_USER_PROFILES; i++) {
      values.push({
        id: `limit-profile-${String(i).padStart(6, "0")}`,
        ownerUserId: bob.id,
        scope: "user" as const,
        configId: "combo",
        name: `Filler ${i}`,
        createdAt: now,
        updatedAt: now,
      });
    }
    await db.insert(configurationProfiles).values(values);

    await expect(
      createProfile(bob, {
        configId: "combo",
        name: "One too many",
        settings,
      }),
    ).rejects.toMatchObject({ status: 400 });
  });

  it("rejects official scope for non-admins and allows it for admins", async () => {
    await expect(
      createProfile(alice, {
        configId: "combo",
        name: "Sneaky official",
        settings: defaultSettings(),
        scope: "official",
      }),
    ).rejects.toMatchObject({ status: 403 });

    const body = await createProfile(admin, {
      configId: "combo",
      name: "Tournament",
      settings: defaultSettings(),
      scope: "official",
      slug: "tournament",
      difficultyTag: "Expert",
    });
    expect(body.profile.scope).toBe("official");
    expect(body.profile.slug).toBe("tournament");

    const list = await listProfiles(null);
    expect(
      list.officials.map((p: { slug: string | null }) => p.slug),
    ).toContain("tournament");
  });
});

describe("PATCH /api/profiles/[id]", () => {
  it("lets owners rename and blocks other users", async () => {
    const created = await createProfile(alice, {
      configId: "combo",
      name: "Old Name",
      settings: defaultSettings(),
    });

    const response = await byId.PATCH({
      params: { id: created.profile.id },
      request: jsonRequest("PATCH", { name: "New Name" }),
      locals: locals(alice),
    } as never);
    const body = await response.json();
    expect(body.profile.name).toBe("New Name");

    await expect(
      byId.PATCH({
        params: { id: created.profile.id },
        request: jsonRequest("PATCH", { name: "Bob was here" }),
        locals: locals(bob),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("blocks curation fields on user profiles", async () => {
    const created = await createProfile(alice, {
      configId: "combo",
      name: "Mine",
      settings: defaultSettings(),
    });
    await expect(
      byId.PATCH({
        params: { id: created.profile.id },
        request: jsonRequest("PATCH", { isRecommended: true }),
        locals: locals(alice),
      } as never),
    ).rejects.toMatchObject({ status: 400 });
  });

  it("blocks non-admin updates to official presets", async () => {
    const list = await listProfiles(null);
    const official = list.officials[0];
    await expect(
      byId.PATCH({
        params: { id: official.id },
        request: jsonRequest("PATCH", { name: "Hacked" }),
        locals: locals(alice),
      } as never),
    ).rejects.toMatchObject({ status: 403 });
  });

  it("keeps a single recommended official preset", async () => {
    const seeded = (await listProfiles(null)).officials.find(
      (p: { slug: string | null }) => p.slug === "recommended",
    );
    const second = await createProfile(admin, {
      configId: "combo",
      name: "Hard Mode",
      settings: defaultSettings(),
      scope: "official",
      slug: "hard-mode",
    });

    await byId.PATCH({
      params: { id: second.profile.id },
      request: jsonRequest("PATCH", { isRecommended: true }),
      locals: locals(admin),
    } as never);

    const list = await listProfiles(null);
    const recommendedNow = list.officials.filter(
      (p: { isRecommended: boolean }) => p.isRecommended,
    );
    expect(recommendedNow).toHaveLength(1);
    expect(recommendedNow[0].id).toBe(second.profile.id);
    expect(list.recommendedId).toBe(second.profile.id);
    expect(
      list.officials.find((p: { id: string }) => p.id === seeded.id)
        .isRecommended,
    ).toBe(false);
  });

  it("archives an official preset out of the list but keeps it readable by id", async () => {
    const created = await createProfile(admin, {
      configId: "combo",
      name: "Old preset",
      settings: defaultSettings(),
      scope: "official",
      slug: "old-preset",
    });
    await byId.PATCH({
      params: { id: created.profile.id },
      request: jsonRequest("PATCH", { archived: true }),
      locals: locals(admin),
    } as never);

    const list = await listProfiles(null);
    expect(list.officials.map((p: { id: string }) => p.id)).not.toContain(
      created.profile.id,
    );

    const response = await byId.GET({
      params: { id: created.profile.id },
      url: new URL(`http://localhost/api/profiles/${created.profile.id}`),
      locals: locals(null),
    } as never);
    const body = await response.json();
    expect(body.profile.archived).toBe(true);
  });
});

describe("DELETE /api/profiles/[id]", () => {
  it("soft-deletes an owned profile and hides it everywhere", async () => {
    const created = await createProfile(alice, {
      configId: "combo",
      name: "Doomed",
      settings: defaultSettings(),
      setAsDefault: true,
    });

    const response = await byId.DELETE({
      params: { id: created.profile.id },
      locals: locals(alice),
    } as never);
    expect((await response.json()).ok).toBe(true);

    const list = await listProfiles(alice);
    expect(list.mine).toEqual([]);
    expect(list.preferences.defaultProfileId).toBeNull();

    await expect(
      byId.GET({
        params: { id: created.profile.id },
        url: new URL(`http://localhost/api/profiles/${created.profile.id}`),
        locals: locals(alice),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("blocks deleting another user's profile", async () => {
    const created = await createProfile(alice, {
      configId: "combo",
      name: "Safe",
      settings: defaultSettings(),
    });
    await expect(
      byId.DELETE({
        params: { id: created.profile.id },
        locals: locals(bob),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });
});

describe("POST /api/profiles/[id]/duplicate", () => {
  it("copies an official preset into an independent user profile", async () => {
    const list = await listProfiles(null);
    const official = list.officials[0];

    const response = await duplicate.POST({
      params: { id: official.id },
      request: jsonRequest("POST", {}),
      locals: locals(alice),
    } as never);
    const copy = await response.json();

    expect(copy.profile.scope).toBe("user");
    expect(copy.profile.slug).toBeNull();
    expect(copy.revision.settings).toEqual(
      (
        await (
          await byId.GET({
            params: { id: official.id },
            url: new URL(`http://localhost/api/profiles/${official.id}`),
            locals: locals(null),
          } as never)
        ).json()
      ).revision.settings,
    );

    // Mutating the copy must not touch the official preset.
    await byId.PATCH({
      params: { id: copy.profile.id },
      request: jsonRequest("PATCH", { name: "Customized" }),
      locals: locals(alice),
    } as never);
    const officialAfter = await (
      await byId.GET({
        params: { id: official.id },
        url: new URL(`http://localhost/api/profiles/${official.id}`),
        locals: locals(null),
      } as never)
    ).json();
    expect(officialAfter.profile.name).toBe(official.name);
  });

  it("auto-suffixes duplicate names", async () => {
    const list = await listProfiles(null);
    const official = list.officials[0];
    const first = await (
      await duplicate.POST({
        params: { id: official.id },
        request: jsonRequest("POST", {}),
        locals: locals(alice),
      } as never)
    ).json();
    const second = await (
      await duplicate.POST({
        params: { id: official.id },
        request: jsonRequest("POST", {}),
        locals: locals(alice),
      } as never)
    ).json();
    expect(first.profile.name).toBe(official.name);
    expect(second.profile.name).toBe(`${official.name} (copy)`);
  });
});
