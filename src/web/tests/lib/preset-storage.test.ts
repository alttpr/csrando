import { beforeEach, describe, expect, it } from "vitest";
import {
  clearDraft,
  forgetPreset,
  loadDraft,
  loadLocalLastUsedPreset,
  loadRecentPresets,
  recordRecentPreset,
  saveDraft,
} from "$lib/config/preset-storage";
import {
  CONFIG_SCHEMA_VERSION,
  MAX_RECENT_PRESETS,
} from "$lib/config/constants";
import type { NormalizedConfig } from "$lib/config/normalize";

const settings: NormalizedConfig = {
  selectedGames: ["Alttpr"],
  global: { Game: "Combo" },
  perGame: { Alttpr: { Swords: "Randomized" } },
};

beforeEach(() => {
  localStorage.clear();
});

describe("config draft storage", () => {
  it("round-trips a draft with the current schema version", () => {
    saveDraft("combo", {
      settings,
      presetId: "p1",
      presetRevisionId: "r1",
    });
    const draft = loadDraft("combo");
    expect(draft).not.toBeNull();
    expect(draft!.settings).toEqual(settings);
    expect(draft!.presetId).toBe("p1");
    expect(draft!.presetRevisionId).toBe("r1");
    expect(draft!.configSchemaVersion).toBe(CONFIG_SCHEMA_VERSION);
    expect(draft!.savedAt).toBeGreaterThan(0);
  });

  it("is scoped per configId", () => {
    saveDraft("combo", { settings, presetId: null, presetRevisionId: null });
    expect(loadDraft("alttpr")).toBeNull();
  });

  it("clears drafts", () => {
    saveDraft("combo", { settings, presetId: null, presetRevisionId: null });
    clearDraft("combo");
    expect(loadDraft("combo")).toBeNull();
  });

  it("tolerates corrupt JSON", () => {
    localStorage.setItem("csrando:config-draft:combo", "{not json");
    expect(loadDraft("combo")).toBeNull();
    localStorage.setItem("csrando:config-draft:combo", '{"savedAt":"x"}');
    expect(loadDraft("combo")).toBeNull();
  });
});

describe("recently used presets", () => {
  it("records most-recent-first without duplicates and caps the list", () => {
    for (let i = 0; i < MAX_RECENT_PRESETS + 3; i++) {
      recordRecentPreset("combo", `preset-${i}`);
    }
    recordRecentPreset("combo", "preset-3");

    const recents = loadRecentPresets("combo");
    expect(recents[0]).toBe("preset-3");
    expect(recents.length).toBeLessThanOrEqual(MAX_RECENT_PRESETS);
    expect(new Set(recents).size).toBe(recents.length);
  });

  it("tracks the local last-used preset", () => {
    recordRecentPreset("combo", "preset-a");
    recordRecentPreset("combo", "preset-b");
    expect(loadLocalLastUsedPreset("combo")).toBe("preset-b");
  });

  it("forgets deleted presets", () => {
    recordRecentPreset("combo", "doomed");
    forgetPreset("combo", "doomed");
    expect(loadRecentPresets("combo")).not.toContain("doomed");
    expect(loadLocalLastUsedPreset("combo")).toBeNull();
  });

  it("tolerates corrupt recents", () => {
    localStorage.setItem("csrando:recent-presets:combo", '{"nope":1}');
    expect(loadRecentPresets("combo")).toEqual([]);
  });
});
