import { describe, expect, it } from "vitest";
import {
  resolvePresetReference,
  resolveStartupSelection,
} from "$lib/config/preset-selection";
import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
import type { ConfigDraft } from "$lib/config/preset-storage";
import type { NormalizedConfig } from "$lib/config/normalize";

const settings: NormalizedConfig = {
  selectedGames: ["Alttpr"],
  global: {},
  perGame: {},
};

function draft(overrides: Partial<ConfigDraft> = {}): ConfigDraft {
  return {
    savedAt: Date.now(),
    configSchemaVersion: CONFIG_SCHEMA_VERSION,
    settings,
    presetId: null,
    presetRevisionId: null,
    ...overrides,
  };
}

const base = {
  queryPresetId: null,
  defaultPresetId: null,
  lastUsedPresetId: null,
  localLastUsedPresetId: null,
  recommendedId: "official-1",
  knownPresetIds: ["official-1", "official-2", "mine-1", "mine-2"],
};

describe("resolvePresetReference", () => {
  const official = {
    id: "official-1",
    slug: "recommended",
  } as Parameters<typeof resolvePresetReference>[1][number];

  it("resolves an official slug to its internal preset id", () => {
    expect(resolvePresetReference("recommended", [official])).toBe(
      "official-1",
    );
  });

  it("preserves ids and unknown references", () => {
    expect(resolvePresetReference("official-1", [official])).toBe("official-1");
    expect(resolvePresetReference("unknown", [official])).toBe("unknown");
    expect(resolvePresetReference(null, [official])).toBeNull();
  });
});

describe("resolveStartupSelection", () => {
  it("prefers an explicit preset link over everything", () => {
    expect(
      resolveStartupSelection(
        { ...base, queryPresetId: "mine-2", defaultPresetId: "mine-1" },
        draft(),
      ),
    ).toEqual({ kind: "preset", presetId: "mine-2" });
  });

  it("ignores unknown preset references", () => {
    expect(
      resolveStartupSelection({ ...base, queryPresetId: "nope" }, null),
    ).toEqual({ kind: "preset", presetId: "official-1" });
  });

  it("recovers a compatible draft before preset preferences", () => {
    const d = draft({ presetId: "mine-1" });
    expect(
      resolveStartupSelection({ ...base, defaultPresetId: "mine-2" }, d),
    ).toEqual({ kind: "draft", draft: d });
  });

  it("skips drafts saved with a newer schema version", () => {
    const d = draft({ configSchemaVersion: CONFIG_SCHEMA_VERSION + 1 });
    expect(
      resolveStartupSelection({ ...base, defaultPresetId: "mine-1" }, d),
    ).toEqual({ kind: "preset", presetId: "mine-1" });
  });

  it("selects default, then last-used, then local last-used, then recommended", () => {
    expect(
      resolveStartupSelection(
        { ...base, defaultPresetId: "mine-1", lastUsedPresetId: "mine-2" },
        null,
      ),
    ).toEqual({ kind: "preset", presetId: "mine-1" });

    expect(
      resolveStartupSelection({ ...base, lastUsedPresetId: "mine-2" }, null),
    ).toEqual({ kind: "preset", presetId: "mine-2" });

    expect(
      resolveStartupSelection(
        { ...base, localLastUsedPresetId: "official-2" },
        null,
      ),
    ).toEqual({ kind: "preset", presetId: "official-2" });

    expect(resolveStartupSelection(base, null)).toEqual({
      kind: "preset",
      presetId: "official-1",
    });
  });

  it("skips preferences pointing at presets that no longer exist", () => {
    expect(
      resolveStartupSelection(
        { ...base, defaultPresetId: "deleted", lastUsedPresetId: "mine-1" },
        null,
      ),
    ).toEqual({ kind: "preset", presetId: "mine-1" });
  });

  it("falls back to plain defaults when nothing is available", () => {
    expect(
      resolveStartupSelection(
        { knownPresetIds: [], recommendedId: null },
        null,
      ),
    ).toEqual({ kind: "defaults" });
  });
});
