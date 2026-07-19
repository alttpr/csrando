import { beforeAll, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import {
  rawMetadataFixture,
  loadMetadataFixture,
  defaultFormStateFixture,
} from "../fixtures/metadata";
import { normalizeConfig, type NormalizedConfig } from "$lib/config/normalize";
import { seeds, users } from "$lib/server/db/schema";

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
type ServiceModule = typeof import("$lib/server/profiles/service");
type SeedsModule = typeof import("$lib/server/db/seeds");

let db: DbModule["db"];
let service: ServiceModule;
let seedsModule: SeedsModule;

const alice: User = {
  id: "attr-alice-00001",
  username: "attr-alice",
  githubId: null,
  isAdmin: false,
};
const bob: User = {
  id: "attr-bob-0000001",
  username: "attr-bob",
  githubId: null,
  isAdmin: false,
};
const admin: User = {
  id: "attr-admin-00001",
  username: "attr-admin",
  githubId: null,
  isAdmin: true,
};

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  service = await import("$lib/server/profiles/service");
  seedsModule = await import("$lib/server/db/seeds");

  for (const user of [alice, bob, admin]) {
    await db.insert(users).values({
      id: user.id,
      username: user.username,
      isAdmin: user.isAdmin,
    });
  }
});

describe("getSeedAttribution", () => {
  it("shows official presets to everyone, including anonymous viewers", async () => {
    const { profile, revision } = await service.createProfile(admin, {
      configId: "combo",
      name: "Attribution Official",
      settings: defaultSettings(),
      scope: "official",
      slug: "attr-official",
    });

    const anon = await service.getSeedAttribution(
      profile.id,
      revision.id,
      null,
    );
    expect(anon).toEqual({
      profileId: profile.id,
      slug: "attr-official",
      name: "Attribution Official",
      scope: "official",
      configId: "combo",
      revisionNumber: revision.revisionNumber,
      deleted: false,
    });

    const other = await service.getSeedAttribution(
      profile.id,
      revision.id,
      bob,
    );
    expect(other?.profileId).toBe(profile.id);
  });

  it("shows private profiles only to their owner", async () => {
    const { profile, revision } = await service.createProfile(alice, {
      configId: "combo",
      name: "Alice Private",
      settings: defaultSettings(),
    });

    const owner = await service.getSeedAttribution(
      profile.id,
      revision.id,
      alice,
    );
    expect(owner?.name).toBe("Alice Private");
    expect(owner?.profileId).toBe(profile.id);
    expect(owner?.slug).toBeNull();
    expect(owner?.revisionNumber).toBe(revision.revisionNumber);

    expect(
      await service.getSeedAttribution(profile.id, revision.id, bob),
    ).toBeNull();
    expect(
      await service.getSeedAttribution(profile.id, revision.id, null),
    ).toBeNull();
  });

  it("keeps the name but drops the link for deleted profiles", async () => {
    const { profile, revision } = await service.createProfile(alice, {
      configId: "combo",
      name: "Alice Deleted",
      settings: defaultSettings(),
    });
    await service.softDeleteProfile(alice, profile.id);

    const attribution = await service.getSeedAttribution(
      profile.id,
      revision.id,
      alice,
    );
    expect(attribution).toMatchObject({
      profileId: null,
      name: "Alice Deleted",
      deleted: true,
    });
  });

  it("returns null for unknown profiles and ignores foreign revision ids", async () => {
    expect(
      await service.getSeedAttribution("does-not-exist-1", null, alice),
    ).toBeNull();

    const first = await service.createProfile(alice, {
      configId: "combo",
      name: "Alice Revision Guard",
      settings: defaultSettings(),
    });
    const second = await service.createProfile(alice, {
      configId: "combo",
      name: "Alice Other Profile",
      settings: defaultSettings(),
    });

    // A revision id belonging to a different profile must not resolve.
    const attribution = await service.getSeedAttribution(
      first.profile.id,
      second.revision.id,
      alice,
    );
    expect(attribution?.revisionNumber).toBeNull();
  });
});

describe("getSeedSettingsSnapshot", () => {
  it("returns the stored snapshot for a seed", async () => {
    const settings = defaultSettings();
    await db.insert(seeds).values({
      id: "attr-seed-00001",
      options: {},
      patchData: {},
      placementInfo: {},
      settingsSnapshot: settings,
      configSchemaVersion: 1,
    });

    const snapshot =
      await seedsModule.getSeedSettingsSnapshot("attr-seed-00001");
    expect(snapshot).toEqual({
      seedId: "attr-seed-00001",
      settings,
      configSchemaVersion: 1,
    });
  });

  it("returns null for unknown seeds and seeds without a snapshot", async () => {
    expect(await seedsModule.getSeedSettingsSnapshot("nope")).toBeNull();

    await db.insert(seeds).values({
      id: "attr-seed-legacy",
      options: {},
      patchData: {},
      placementInfo: {},
    });
    expect(
      await seedsModule.getSeedSettingsSnapshot("attr-seed-legacy"),
    ).toBeNull();
  });
});
