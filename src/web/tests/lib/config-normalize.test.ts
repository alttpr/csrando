import { describe, expect, it } from "vitest";
import type { Metadata } from "$lib/types";
import {
  buildRandomizePayload,
  configsEqual,
  getDefaultValue,
  hydrateFormState,
  normalizeConfig,
  stableStringify,
  type FormStateSnapshot,
} from "$lib/config/normalize";
import {
  defaultFormStateFixture as defaultFormState,
  loadMetadataFixture as loadMetadata,
  rawMetadataFixture,
} from "../fixtures/metadata";
import { parseMetadata } from "$lib/schemas/metadata";

// Verbatim copy of the config page's former inline submit serialization,
// kept here as the parity oracle for buildRandomizePayload.
function legacyBuildPayload(
  formValues: {
    global: Record<string, unknown>;
    perGame: Record<string, Record<string, unknown>>;
  },
  selectedGames: string[],
  metadata: Metadata,
  includeSpoiler: boolean,
) {
  const isRandomSelection = (value: unknown) =>
    typeof value === "string" && value.trim().toLowerCase() === "randompick";

  const filterNonNullValues = (obj: unknown) => {
    const o = obj as Record<string, unknown> | undefined;
    if (!o) {
      return {};
    }
    const entries: Array<[string, unknown]> = [];
    for (const [key, value] of Object.entries(o)) {
      if (value === null || isRandomSelection(value)) {
        continue;
      }

      if (Array.isArray(value)) {
        const sanitizedArray = value.filter(
          (item) => item != null && !isRandomSelection(item),
        );
        if (sanitizedArray.length === 0) {
          continue;
        }
        entries.push([key, sanitizedArray]);
        continue;
      }

      if (typeof value === "string") {
        const trimmed = value.trim();
        if (!trimmed || trimmed.toLowerCase() === "randompick") {
          continue;
        }
        entries.push([key, trimmed]);
        continue;
      }

      entries.push([key, value]);
    }
    return Object.fromEntries(entries);
  };

  const gameSettings: { [key: string]: { [key: string]: unknown } } = {};

  for (const gameId of selectedGames) {
    const currentGameOptions = formValues.perGame[gameId];

    if (currentGameOptions) {
      const validGameOptions = filterNonNullValues(currentGameOptions);

      if (Object.keys(validGameOptions).length > 0) {
        gameSettings[gameId] = validGameOptions;
      }
    }
  }

  if (metadata?.gameSettings) {
    for (const [gameKey, gameMeta] of Object.entries(metadata.gameSettings)) {
      for (const setting of gameMeta.settings) {
        if (setting.type === "Slider" && setting.optionsFor) {
          const currentVal = (gameSettings[gameKey] || {})[setting.key];
          if (currentVal === undefined) {
            const from = setting.range.from ?? 0;
            const to = setting.range.to;
            const arr = Array.from(
              { length: to - from + 1 },
              (_, i) => i + from,
            );
            if (!gameSettings[gameKey]) gameSettings[gameKey] = {};
            gameSettings[gameKey][setting.key] = arr;
          } else if (typeof currentVal === "number") {
            gameSettings[gameKey][setting.key] = [currentVal];
          }
        }
      }
    }
  }

  let globalGameTarget = (formValues.global["Game"] as string) || "";
  if (!globalGameTarget && metadata?.settings) {
    const gameSetting = metadata.settings.find((s) => s.key === "Game");
    if (
      gameSetting &&
      "default" in gameSetting &&
      typeof (gameSetting as { default?: unknown }).default === "string"
    ) {
      globalGameTarget = (gameSetting as { default?: string })
        .default as string;
    }
  }
  if (!globalGameTarget) globalGameTarget = "Alttpr";

  const worldConfig: Record<string, unknown> = {
    Language: (formValues.global["Language"] as string) || "en",
  };
  if (!isRandomSelection(globalGameTarget)) {
    worldConfig.Game = globalGameTarget;
  }

  const worldGameKeys = new Set<string>();
  for (const gameKey of selectedGames) {
    worldGameKeys.add(gameKey);
  }

  for (const gameKey of worldGameKeys) {
    const perGame =
      gameSettings[gameKey] ?? filterNonNullValues(formValues.perGame[gameKey]);
    worldConfig[gameKey] = perGame || {};
  }

  if ((globalGameTarget || "").toLowerCase() === "combo") {
    worldConfig["Combo"] =
      filterNonNullValues(formValues.perGame["Combo"]) || {};
  }

  return {
    Seed: 0,
    IncludeSpoiler: includeSpoiler,
    Configs: [worldConfig],
  };
}

function expectParity(form: FormStateSnapshot, metadata: Metadata) {
  const legacy = legacyBuildPayload(
    { global: form.global, perGame: form.perGame },
    form.selectedGames,
    metadata,
    true,
  );
  const snapshot = normalizeConfig(form, metadata);
  const modern = buildRandomizePayload(snapshot, metadata, {
    includeSpoiler: true,
  });
  expect(stableStringify(modern)).toBe(stableStringify(legacy));
}

describe("buildRandomizePayload parity with legacy inline submit logic", () => {
  it("matches for the pure defaults form state", () => {
    const metadata = loadMetadata();
    expectParity(defaultFormState(metadata), metadata);
  });

  it("matches for a modified form state (choices, toggles, inputs)", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.perGame.Alttpr.Swords = "Assured";
    form.perGame.Alttpr.KeyShuffle = true;
    form.perGame.Alttpr.Crystals = 3;
    form.perGame.Alttpr.Notes = "  padded note  ";
    // Pre-sorted array: array order in the payload is intentionally canonical.
    form.perGame.Alttpr.Glitches = ["Major", "None"];
    expectParity(form, metadata);
  });

  it("matches when RandomPick values are selected", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.perGame.Alttpr.Swords = "RandomPick";
    expectParity(form, metadata);
  });

  it("matches when the global Game target is RandomPick", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.global.Game = "RandomPick";
    const snapshot = normalizeConfig(form, metadata);
    const payload = buildRandomizePayload(snapshot, metadata, {
      includeSpoiler: true,
    });
    expect(payload.Configs[0]).not.toHaveProperty("Game");
    expectParity(form, metadata);
  });

  it("matches with a deselected game", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.selectedGames = form.selectedGames.filter((g) => g !== "Sm");
    const snapshot = normalizeConfig(form, metadata);
    const payload = buildRandomizePayload(snapshot, metadata, {
      includeSpoiler: true,
    });
    expect(payload.Configs[0]).not.toHaveProperty("Sm");
    expectParity(form, metadata);
  });

  it("expands optionsFor sliders to a single-value array when picked", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.perGame.Sm.BossTokenCount = 2;
    const snapshot = normalizeConfig(form, metadata);
    const payload = buildRandomizePayload(snapshot, metadata, {
      includeSpoiler: true,
    });
    expect(
      (payload.Configs[0].Sm as Record<string, unknown>).BossTokenCount,
    ).toEqual([2]);
    expectParity(form, metadata);
  });

  it("includes the Combo block when the target is combo", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    const snapshot = normalizeConfig(form, metadata);
    const payload = buildRandomizePayload(snapshot, metadata, {
      includeSpoiler: true,
    });
    expect(payload.Configs[0].Game).toBe("Combo");
    expect(payload.Configs[0]).toHaveProperty("Combo");
    expectParity(form, metadata);
  });

  it("honors the includeSpoiler and seed options", () => {
    const metadata = loadMetadata();
    const snapshot = normalizeConfig(defaultFormState(metadata), metadata);
    const payload = buildRandomizePayload(snapshot, metadata, {
      includeSpoiler: false,
      seed: 1234,
    });
    expect(payload.IncludeSpoiler).toBe(false);
    expect(payload.Seed).toBe(1234);
  });
});

describe("normalizeConfig", () => {
  it("fills missing keys with metadata defaults", () => {
    const metadata = loadMetadata();
    const normalized = normalizeConfig(
      { selectedGames: ["Alttpr"], global: {}, perGame: {} },
      metadata,
    );
    expect(normalized.global.Game).toBe("Combo");
    expect(normalized.perGame.Alttpr.Swords).toBe("Randomized");
    expect(normalized.perGame.Alttpr.Crystals).toBe(7);
    expect(normalized.perGame.Alttpr.Extra).toBeNull();
  });

  it("sorts selectedGames and MultipleChoice arrays deterministically", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.selectedGames = ["Sm", "Combo", "Alttpr"];
    form.perGame.Alttpr.Glitches = ["OverworldGlitches", "Major", "Major"];
    const normalized = normalizeConfig(form, metadata);
    expect(normalized.selectedGames).toEqual(["Alttpr", "Combo", "Sm"]);
    expect(normalized.perGame.Alttpr.Glitches).toEqual([
      "Major",
      "OverworldGlitches",
    ]);
  });

  it("drops unknown settings and games", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.global.NotARealSetting = "x";
    form.perGame.Alttpr.Bogus = 42;
    form.perGame.NotAGame = { Foo: 1 };
    form.selectedGames.push("NotAGame");
    const normalized = normalizeConfig(form, metadata);
    expect(normalized.global).not.toHaveProperty("NotARealSetting");
    expect(normalized.perGame.Alttpr).not.toHaveProperty("Bogus");
    expect(normalized.perGame).not.toHaveProperty("NotAGame");
    expect(normalized.selectedGames).not.toContain("NotAGame");
  });

  it("trims strings and coerces numeric slider strings", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.perGame.Alttpr.Notes = "  hello  ";
    form.perGame.Alttpr.Crystals = "5";
    const normalized = normalizeConfig(form, metadata);
    expect(normalized.perGame.Alttpr.Notes).toBe("hello");
    expect(normalized.perGame.Alttpr.Crystals).toBe(5);
  });

  it("is semantically stable: two equivalent form states normalize equal", () => {
    const metadata = loadMetadata();
    const a = defaultFormState(metadata);
    const b = defaultFormState(metadata);
    b.selectedGames = [...b.selectedGames].reverse();
    b.perGame = Object.fromEntries(Object.entries(b.perGame).reverse());
    expect(
      configsEqual(normalizeConfig(a, metadata), normalizeConfig(b, metadata)),
    ).toBe(true);
  });
});

describe("stableStringify / configsEqual", () => {
  it("is independent of object key insertion order", () => {
    expect(stableStringify({ a: 1, b: { c: 2, d: 3 } })).toBe(
      stableStringify({ b: { d: 3, c: 2 }, a: 1 }),
    );
  });

  it("distinguishes different values and array orders", () => {
    expect(configsEqual({ a: 1 }, { a: 2 })).toBe(false);
    expect(configsEqual({ a: [1, 2] }, { a: [2, 1] })).toBe(false);
  });

  it("drops undefined properties like JSON.stringify does", () => {
    expect(stableStringify({ a: 1, b: undefined })).toBe(
      stableStringify({ a: 1 }),
    );
  });
});

describe("hydrateFormState", () => {
  it("round-trips a normalized config without reports", () => {
    const metadata = loadMetadata();
    const form = defaultFormState(metadata);
    form.perGame.Alttpr.Swords = "Assured";
    form.perGame.Alttpr.KeyShuffle = true;
    const normalized = normalizeConfig(form, metadata);
    const { form: hydrated, report } = hydrateFormState(normalized, metadata);
    expect(report.removedKeys).toEqual([]);
    expect(report.resetKeys).toEqual([]);
    expect(configsEqual(normalizeConfig(hydrated, metadata), normalized)).toBe(
      true,
    );
  });

  it("reports settings that no longer exist", () => {
    const metadata = loadMetadata();
    const normalized = normalizeConfig(defaultFormState(metadata), metadata);
    const stored = {
      ...normalized,
      global: { ...normalized.global, LegacySetting: "x" },
      perGame: {
        ...normalized.perGame,
        Alttpr: { ...normalized.perGame.Alttpr, OldOption: true },
        RemovedGame: { Foo: 1 },
      },
      selectedGames: [...normalized.selectedGames, "RemovedGame"],
    };
    const { report } = hydrateFormState(stored, metadata);
    expect(report.removedKeys).toContain("global.LegacySetting");
    expect(report.removedKeys).toContain("Alttpr.OldOption");
    expect(report.removedKeys).toContain("RemovedGame.*");
    expect(report.removedKeys).toContain("selectedGames.RemovedGame");
  });

  it("resets invalid values to defaults and reports them", () => {
    const metadata = loadMetadata();
    const normalized = normalizeConfig(defaultFormState(metadata), metadata);
    const stored = {
      ...normalized,
      perGame: {
        ...normalized.perGame,
        Alttpr: {
          ...normalized.perGame.Alttpr,
          Swords: "NoSuchValue",
          Crystals: 999,
          KeyShuffle: "yes",
        },
      },
    };
    const { form, report } = hydrateFormState(stored, metadata);
    expect(form.perGame.Alttpr.Swords).toBe("Randomized");
    expect(form.perGame.Alttpr.Crystals).toBe(7);
    expect(form.perGame.Alttpr.KeyShuffle).toBe(false);
    expect(report.resetKeys).toEqual(
      expect.arrayContaining([
        "Alttpr.Swords",
        "Alttpr.Crystals",
        "Alttpr.KeyShuffle",
      ]),
    );
  });

  it("falls back to all games when the stored selection is unusable", () => {
    const metadata = loadMetadata();
    const normalized = normalizeConfig(defaultFormState(metadata), metadata);
    const stored = { ...normalized, selectedGames: ["RemovedGame"] };
    const { form, activeTab } = hydrateFormState(stored, metadata);
    expect(form.selectedGames.length).toBeGreaterThan(0);
    expect(activeTab).toBe(
      form.selectedGames.includes("Alttpr") ? "Alttpr" : form.selectedGames[0],
    );
  });
});

describe("getDefaultValue", () => {
  it("returns the default key when it exists in values", () => {
    const metadata = loadMetadata();
    const game = metadata.settings.find((s) => s.key === "Game");
    expect(getDefaultValue(game!)).toBe("Combo");
  });
});

describe("onlyWithGames game-selection gate", () => {
  // The shared fixture stays gate-free (its parity oracle predates the gate);
  // this local variant adds a Wip setting only meaningful on Sm-only seeds.
  function loadGatedMetadata(): Metadata {
    const raw = structuredClone(rawMetadataFixture) as {
      gameSettings: Record<string, { settings: unknown[] }>;
    };
    raw.gameSettings.Sm.settings.push({
      key: "TieredItems",
      name: "Tiered Items",
      type: "SingleChoice",
      values: { Off: "Off", On: "On" },
      default: "Off",
      visibility: "Wip",
      onlyWithGames: ["Sm"],
    });
    const parsed = parseMetadata(raw);
    if (!parsed.success) throw new Error("gated fixture metadata failed to parse");
    return parsed.data;
  }

  it("omits the gated setting when the selection includes another game", () => {
    const metadata = loadGatedMetadata();
    const form = defaultFormState(metadata);
    form.perGame.Sm.TieredItems = "On";
    const snapshot = normalizeConfig(form, metadata);
    const payload = buildRandomizePayload(snapshot, metadata, {
      includeSpoiler: true,
    });
    expect(payload.Configs[0].Sm as Record<string, unknown>).not.toHaveProperty(
      "TieredItems",
    );
  });

  it("keeps the gated setting when only allowed games are selected", () => {
    const metadata = loadGatedMetadata();
    const form = defaultFormState(metadata);
    form.selectedGames = ["Sm"];
    form.perGame.Sm.TieredItems = "On";
    const snapshot = normalizeConfig(form, metadata);
    const payload = buildRandomizePayload(snapshot, metadata, {
      includeSpoiler: true,
    });
    expect(
      (payload.Configs[0].Sm as Record<string, unknown>).TieredItems,
    ).toBe("On");
  });

  it("preserves the stored value across normalization while gated off", () => {
    const metadata = loadGatedMetadata();
    const form = defaultFormState(metadata);
    form.perGame.Sm.TieredItems = "On";
    // Selection includes Alttpr and Combo: the payload drops the setting but the
    // canonical config keeps it, so re-narrowing the selection restores it.
    const normalized = normalizeConfig(form, metadata);
    expect(normalized.perGame.Sm.TieredItems).toBe("On");
  });
});
