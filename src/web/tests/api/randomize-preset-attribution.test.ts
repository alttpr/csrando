import { beforeAll, beforeEach, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import { eq } from "drizzle-orm";
import {
  rawMetadataFixture,
  loadMetadataFixture,
  defaultFormStateFixture,
} from "../fixtures/metadata";
import {
  buildRandomizePayload,
  normalizeConfig,
  type NormalizedConfig,
} from "$lib/config/normalize";
import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
import {
  configurationPresets,
  configurationPresetRevisions,
  seeds,
  userPresetPreferences,
  userSeeds,
  users,
} from "$lib/server/db/schema";

const randomizeCreateMock = vi.fn(
  async (
    _request: unknown,
  ): Promise<{
    seed: number;
    worlds: Record<string, { ipsPatch?: string; bpsPatch?: string }>;
    spoilerLog: Record<string, Record<string, string>>;
  }> => ({
    seed: 12345,
    worlds: { "0": { ipsPatch: "cGF0Y2g=" } },
    spoilerLog: { meta: { seed: "12345" } },
  }),
);

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
  id: "attr-alice-000001",
  username: "attr-alice",
  githubId: null,
  isAdmin: false,
};

function locals(user: User | null) {
  return { user, session: null };
}

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

function randomizeRequest(body: unknown): Request {
  return new Request("http://localhost/api/randomize", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
}

async function createPreset(user: User, name: string) {
  const response = await presetsRoute.POST({
    request: new Request("http://localhost/api/presets", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({
        configId: "combo",
        name,
        settings: defaultSettings(),
      }),
    }),
    locals: locals(user),
  } as never);
  return await response.json();
}

function payloadFor(settings: NormalizedConfig) {
  return buildRandomizePayload(settings, loadMetadataFixture(), {
    seed: 0,
    includeSpoiler: true,
  });
}

const basePayload = payloadFor(defaultSettings());

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  randomize = await import("../../src/routes/api/randomize/+server");
  presetsRoute = await import("../../src/routes/api/presets/+server");
  seedModule = await import("$lib/server/presets/seed-official");

  await db.insert(users).values({
    id: alice.id,
    username: alice.username,
    isAdmin: false,
  });
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

async function getSeedRow(id: string) {
  const rows = await db.select().from(seeds).where(eq(seeds.id, id)).limit(1);
  return rows[0];
}

describe("POST /api/randomize request handling", () => {
  it("returns 400 for malformed JSON", async () => {
    const request = new Request("http://localhost/api/randomize", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: "{",
    });

    await expect(
      randomize.POST({ request, locals: locals(null) } as never),
    ).rejects.toMatchObject({ status: 400 });
    expect(randomizeCreateMock).not.toHaveBeenCalled();
  });

  it("allows the backend to choose a randomizer when Game is omitted", async () => {
    const payload = structuredClone(basePayload);
    delete payload.Configs[0].Game;

    const response = await randomize.POST({
      request: randomizeRequest(payload),
      locals: locals(null),
    } as never);

    expect((await response.json()).id).toBeTruthy();
    expect(randomizeCreateMock).toHaveBeenCalledWith(
      payload,
      expect.any(AbortSignal),
    );
  });

  it("accepts and stores a BPS-only world response", async () => {
    randomizeCreateMock.mockResolvedValueOnce({
      seed: 12345,
      worlds: { "0": { bpsPatch: "YnBzLXBhdGNo" } },
      spoilerLog: { meta: { seed: "12345" } },
    });

    const response = await randomize.POST({
      request: randomizeRequest(basePayload),
      locals: locals(null),
    } as never);
    const body = await response.json();

    expect((await getSeedRow(body.id)).patchData).toBe("YnBzLXBhdGNo");
  });
});

describe("POST /api/randomize with preset attribution", () => {
  it("stores attribution for a clean preset configuration", async () => {
    const created = await createPreset(alice, "Clean run");
    const snapshot = created.revision.settings as NormalizedConfig;

    const response = await randomize.POST({
      request: randomizeRequest({
        ...basePayload,
        Preset: {
          presetId: created.preset.id,
          presetRevisionId: created.revision.id,
          settingsSnapshot: snapshot,
          configSchemaVersion: CONFIG_SCHEMA_VERSION,
        },
      }),
      locals: locals(alice),
    } as never);
    const body = await response.json();

    const row = await getSeedRow(body.id);
    expect(row.presetId).toBe(created.preset.id);
    expect(row.presetRevisionId).toBe(created.revision.id);
    expect(row.differedFromRevision).toBe(false);
    expect(row.configSchemaVersion).toBe(CONFIG_SCHEMA_VERSION);
    expect(row.settingsSnapshot).toEqual(snapshot);

    // last-used bookkeeping for the logged-in user
    const prefs = await db
      .select()
      .from(userPresetPreferences)
      .where(eq(userPresetPreferences.userId, alice.id));
    expect(prefs[0]?.lastUsedPresetId).toBe(created.preset.id);
  });

  it("computes the differed flag server-side for modified configurations", async () => {
    const created = await createPreset(alice, "Modified run");
    const snapshot = structuredClone(
      created.revision.settings,
    ) as NormalizedConfig;
    snapshot.perGame.Alttpr.Swords = "Assured";
    const modifiedPayload = payloadFor(snapshot);

    const response = await randomize.POST({
      request: randomizeRequest({
        ...modifiedPayload,
        Preset: {
          presetId: created.preset.id,
          presetRevisionId: created.revision.id,
          settingsSnapshot: snapshot,
          configSchemaVersion: CONFIG_SCHEMA_VERSION,
          // A lying client flag must be ignored; there is no such field, but
          // even resending the original snapshot as "clean" cannot happen —
          // the server compares content.
        },
      }),
      locals: locals(alice),
    } as never);
    const body = await response.json();

    const row = await getSeedRow(body.id);
    expect(row.differedFromRevision).toBe(true);
    expect(row.settingsSnapshot).toEqual(snapshot);
  });

  it("stores a snapshot without preset ids for custom configurations", async () => {
    const snapshot = defaultSettings();
    const response = await randomize.POST({
      request: randomizeRequest({
        ...basePayload,
        Preset: {
          presetId: null,
          presetRevisionId: null,
          settingsSnapshot: snapshot,
          configSchemaVersion: CONFIG_SCHEMA_VERSION,
        },
      }),
      locals: locals(null),
    } as never);
    const body = await response.json();

    const row = await getSeedRow(body.id);
    expect(row.presetId).toBeNull();
    expect(row.presetRevisionId).toBeNull();
    expect(row.differedFromRevision).toBeNull();
    expect(row.settingsSnapshot).toEqual(snapshot);
  });

  it("nulls attribution when the referenced revision no longer exists", async () => {
    const snapshot = defaultSettings();
    const response = await randomize.POST({
      request: randomizeRequest({
        ...basePayload,
        Preset: {
          presetId: "gone-preset-0001",
          presetRevisionId: "gone-revision-01",
          settingsSnapshot: snapshot,
          configSchemaVersion: CONFIG_SCHEMA_VERSION,
        },
      }),
      locals: locals(null),
    } as never);
    const body = await response.json();

    const row = await getSeedRow(body.id);
    expect(row.presetId).toBeNull();
    expect(row.presetRevisionId).toBeNull();
    expect(row.settingsSnapshot).toEqual(snapshot);
  });

  it("drops spoofed attribution when the snapshot does not match Configs", async () => {
    const created = await createPreset(alice, "Spoof target");
    const snapshot = structuredClone(
      created.revision.settings,
    ) as NormalizedConfig;
    snapshot.perGame.Alttpr.Swords = "Assured";

    const response = await randomize.POST({
      request: randomizeRequest({
        ...basePayload,
        Preset: {
          presetId: created.preset.id,
          presetRevisionId: created.revision.id,
          settingsSnapshot: snapshot,
          configSchemaVersion: CONFIG_SCHEMA_VERSION,
        },
      }),
      locals: locals(alice),
    } as never);
    const row = await getSeedRow((await response.json()).id);
    expect(row.presetId).toBeNull();
    expect(row.presetRevisionId).toBeNull();
    expect(row.settingsSnapshot).toBeNull();
  });

  it("strips the Preset block before calling the generator backend", async () => {
    const created = await createPreset(alice, "Strip test");
    await randomize.POST({
      request: randomizeRequest({
        ...basePayload,
        Preset: {
          presetId: created.preset.id,
          presetRevisionId: created.revision.id,
          settingsSnapshot: created.revision.settings,
          configSchemaVersion: CONFIG_SCHEMA_VERSION,
        },
      }),
      locals: locals(alice),
    } as never);

    expect(randomizeCreateMock).toHaveBeenCalledTimes(1);
    const forwarded = randomizeCreateMock.mock.calls[0][0] as Record<
      string,
      unknown
    >;
    expect(forwarded).not.toHaveProperty("Preset");
    expect(forwarded.Configs).toEqual(basePayload.Configs);
  });

  it("stores no attribution columns and keeps options exact without a Preset block", async () => {
    const response = await randomize.POST({
      request: randomizeRequest(basePayload),
      locals: locals(null),
    } as never);
    const body = await response.json();
    const row = await getSeedRow(body.id);
    expect(row.presetId).toBeNull();
    expect(row.settingsSnapshot).toBeNull();
    expect(row.options).toEqual(basePayload);
  });
});
