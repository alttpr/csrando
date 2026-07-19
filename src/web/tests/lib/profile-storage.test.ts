import { beforeEach, describe, expect, it } from "vitest";
import {
  clearDraft,
  forgetProfile,
  loadDraft,
  loadLocalLastUsedProfile,
  loadRecentProfiles,
  recordRecentProfile,
  saveDraft,
} from "$lib/config/profile-storage";
import {
  CONFIG_SCHEMA_VERSION,
  MAX_RECENT_PROFILES,
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
      profileId: "p1",
      profileRevisionId: "r1",
    });
    const draft = loadDraft("combo");
    expect(draft).not.toBeNull();
    expect(draft!.settings).toEqual(settings);
    expect(draft!.profileId).toBe("p1");
    expect(draft!.profileRevisionId).toBe("r1");
    expect(draft!.configSchemaVersion).toBe(CONFIG_SCHEMA_VERSION);
    expect(draft!.savedAt).toBeGreaterThan(0);
  });

  it("is scoped per configId", () => {
    saveDraft("combo", { settings, profileId: null, profileRevisionId: null });
    expect(loadDraft("alttpr")).toBeNull();
  });

  it("clears drafts", () => {
    saveDraft("combo", { settings, profileId: null, profileRevisionId: null });
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

describe("recently used profiles", () => {
  it("records most-recent-first without duplicates and caps the list", () => {
    for (let i = 0; i < MAX_RECENT_PROFILES + 3; i++) {
      recordRecentProfile("combo", `profile-${i}`);
    }
    recordRecentProfile("combo", "profile-3");

    const recents = loadRecentProfiles("combo");
    expect(recents[0]).toBe("profile-3");
    expect(recents.length).toBeLessThanOrEqual(MAX_RECENT_PROFILES);
    expect(new Set(recents).size).toBe(recents.length);
  });

  it("tracks the local last-used profile", () => {
    recordRecentProfile("combo", "profile-a");
    recordRecentProfile("combo", "profile-b");
    expect(loadLocalLastUsedProfile("combo")).toBe("profile-b");
  });

  it("forgets deleted profiles", () => {
    recordRecentProfile("combo", "doomed");
    forgetProfile("combo", "doomed");
    expect(loadRecentProfiles("combo")).not.toContain("doomed");
    expect(loadLocalLastUsedProfile("combo")).toBeNull();
  });

  it("tolerates corrupt recents", () => {
    localStorage.setItem("csrando:recent-profiles:combo", '{"nope":1}');
    expect(loadRecentProfiles("combo")).toEqual([]);
  });
});
