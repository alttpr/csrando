import { beforeAll, beforeEach, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import { eq } from "drizzle-orm";
import {
  rawMetadataFixture,
  loadMetadataFixture,
  defaultFormStateFixture,
} from "../fixtures/metadata";
import { normalizeConfig, type NormalizedConfig } from "$lib/config/normalize";
import {
  configurationPresets,
  configurationPresetRevisions,
  seeds,
  userPresetPreferences,
  userSeeds,
  users,
} from "$lib/server/db/schema";

const randomizeCreateMock = vi.fn(async (_request: unknown) => ({
  seed: 999,
  worlds: { "0": { ipsPatch: "cGF0Y2g=" } },
  spoilerLog: { meta: { seed: "999" } },
}));

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
  randomizeApi: {
    create: randomizeCreateMock,
  },
}));

type DbModule = typeof import("$lib/server/db");
type RandomizeRoute = typeof import("../../src/routes/api/randomize/+server");
type PresetsRoute = typeof import("../../src/routes/api/presets/+server");
type SeedModule = typeof import("$lib/server/presets/seed-official");

let db: DbModule["db"];
let randomize: RandomizeRoute;
let presetsRoute: PresetsRoute;
let seedModule: SeedModule;

const alice: User = {
  id: "byp-alice-0000001",
  username: "byp-alice",
  githubId: null,
  isAdmin: false,
};
const bob: User = {
  id: "byp-bob-000000001",
  username: "byp-bob",
  githubId: null,
  isAdmin: false,
};

// API-key style principal: user present, no session.
function locals(user: User | null) {
  return { user, session: null };
}

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

function jsonRequest(url: string, body: unknown): Request {
  return new Request(url, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
}

async function createPreset(user: User, name: string) {
  const response = await presetsRoute.POST({
    request: jsonRequest("http://localhost/api/presets", {
      configId: "combo",
      name,
      settings: defaultSettings(),
    }),
    locals: locals(user),
  } as never);
  return await response.json();
}

async function generateByPreset(user: User | null, body: unknown) {
  const response = await randomize.POST({
    request: jsonRequest("http://localhost/api/randomize", body),
    locals: locals(user),
  } as never);
  return await response.json();
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  randomize = await import("../../src/routes/api/randomize/+server");
  presetsRoute = await import("../../src/routes/api/presets/+server");
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
  vi.clearAllMocks();
  await db.delete(userSeeds);
  await db.delete(seeds);
  await db.delete(userPresetPreferences);
  await db.delete(configurationPresetRevisions);
  await db.delete(configurationPresets);
  seedModule.resetSeedStateForTests();
});

describe("POST /api/randomize with PresetId (external tools)", () => {
  it("generates a seed from an owned preset with full attribution", async () => {
    const created = await createPreset(alice, "Bot preset");

    const body = await generateByPreset(alice, {
      PresetId: created.preset.id,
    });
    expect(body.id).toBeTruthy();

    const rows = await db
      .select()
      .from(seeds)
      .where(eq(seeds.id, body.id))
      .limit(1);
    const row = rows[0];
    expect(row.presetId).toBe(created.preset.id);
    expect(row.presetRevisionId).toBe(created.revision.id);
    expect(row.differedFromRevision).toBe(false);
    expect(row.settingsSnapshot).toEqual(created.revision.settings);

    // The generator got a fully expanded payload, without the Preset block.
    const forwarded = randomizeCreateMock.mock.calls[0][0] as Record<
      string,
      unknown
    >;
    expect(forwarded).not.toHaveProperty("Preset");
    expect(forwarded).not.toHaveProperty("PresetId");
    const worldConfig = (forwarded.Configs as Record<string, unknown>[])[0];
    expect(worldConfig.Game).toBe("Combo");
    expect(worldConfig).toHaveProperty("Alttpr");

    // Seed history + last-used bookkeeping for the key's user.
    const links = await db
      .select()
      .from(userSeeds)
      .where(eq(userSeeds.seedId, body.id));
    expect(links[0]?.userId).toBe(alice.id);
  });

  it("honors Seed and IncludeSpoiler options", async () => {
    const created = await createPreset(alice, "Race preset");
    await generateByPreset(alice, {
      PresetId: created.preset.id,
      Seed: 42,
      IncludeSpoiler: false,
    });
    const forwarded = randomizeCreateMock.mock.calls[0][0] as Record<
      string,
      unknown
    >;
    expect(forwarded.Seed).toBe(42);
    // Spoilers are always generated for admin diagnostics; visibility is
    // controlled by the stored options.
    expect(forwarded.IncludeSpoiler).toBe(true);
  });

  it("allows anonymous generation from an official preset slug", async () => {
    const list = await (
      await presetsRoute.GET({
        url: new URL("http://localhost/api/presets?configId=combo"),
        locals: locals(null),
      } as never)
    ).json();

    const recommended = list.officials.find(
      (preset: { id: string; slug: string | null }) =>
        preset.id === list.recommendedId,
    );
    expect(recommended?.slug).toBe("recommended");

    const body = await generateByPreset(null, {
      PresetId: "Recommended",
    });
    expect(body.id).toBeTruthy();
    const rows = await db
      .select()
      .from(seeds)
      .where(eq(seeds.id, body.id))
      .limit(1);
    expect(rows[0].presetId).toBe(recommended.id);
  });

  it("rejects foreign private presets and unknown ids with 404", async () => {
    const secret = await createPreset(bob, "Bob private");
    await expect(
      generateByPreset(alice, { PresetId: secret.preset.id }),
    ).rejects.toMatchObject({ status: 404 });
    await expect(
      generateByPreset(alice, { PresetId: "does-not-exist" }),
    ).rejects.toMatchObject({ status: 404 });
    expect(randomizeCreateMock).not.toHaveBeenCalled();
  });

  it("can generate from an older revision explicitly", async () => {
    const created = await createPreset(alice, "Versioned");
    // Add a second revision directly so the current revision moves on.
    const revisionsRoute = await import(
      "../../src/routes/api/presets/[id]/revisions/+server"
    );
    const settings = defaultSettings();
    settings.perGame.Alttpr.Swords = "Assured";
    await revisionsRoute.POST({
      params: { id: created.preset.id },
      request: jsonRequest(
        `http://localhost/api/presets/${created.preset.id}/revisions`,
        { settings, baseRevisionId: created.revision.id },
      ),
      locals: locals(alice),
    } as never);

    const body = await generateByPreset(alice, {
      PresetId: created.preset.id,
      RevisionId: created.revision.id,
    });
    const rows = await db
      .select()
      .from(seeds)
      .where(eq(seeds.id, body.id))
      .limit(1);
    expect(rows[0].presetRevisionId).toBe(created.revision.id);
    const snapshot = rows[0].settingsSnapshot as NormalizedConfig;
    expect(snapshot.perGame.Alttpr.Swords).toBe("Randomized");
  });
});
