import { describe, expect, it } from "vitest";
import {
  resolveProfileReference,
  resolveStartupSelection,
} from "$lib/config/profile-selection";
import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
import type { ConfigDraft } from "$lib/config/profile-storage";
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
    profileId: null,
    profileRevisionId: null,
    ...overrides,
  };
}

const base = {
  queryProfileId: null,
  defaultProfileId: null,
  lastUsedProfileId: null,
  localLastUsedProfileId: null,
  recommendedId: "official-1",
  knownProfileIds: ["official-1", "official-2", "mine-1", "mine-2"],
};

describe("resolveProfileReference", () => {
  const official = {
    id: "official-1",
    slug: "recommended",
  } as Parameters<typeof resolveProfileReference>[1][number];

  it("resolves an official slug to its internal profile id", () => {
    expect(resolveProfileReference("recommended", [official])).toBe(
      "official-1",
    );
  });

  it("preserves ids and unknown references", () => {
    expect(resolveProfileReference("official-1", [official])).toBe(
      "official-1",
    );
    expect(resolveProfileReference("unknown", [official])).toBe("unknown");
    expect(resolveProfileReference(null, [official])).toBeNull();
  });
});

describe("resolveStartupSelection", () => {
  it("prefers an explicit profile link over everything", () => {
    expect(
      resolveStartupSelection(
        { ...base, queryProfileId: "mine-2", defaultProfileId: "mine-1" },
        draft(),
      ),
    ).toEqual({ kind: "profile", profileId: "mine-2" });
  });

  it("ignores unknown profile references", () => {
    expect(
      resolveStartupSelection({ ...base, queryProfileId: "nope" }, null),
    ).toEqual({ kind: "profile", profileId: "official-1" });
  });

  it("recovers a compatible draft before profile preferences", () => {
    const d = draft({ profileId: "mine-1" });
    expect(
      resolveStartupSelection({ ...base, defaultProfileId: "mine-2" }, d),
    ).toEqual({ kind: "draft", draft: d });
  });

  it("skips drafts saved with a newer schema version", () => {
    const d = draft({ configSchemaVersion: CONFIG_SCHEMA_VERSION + 1 });
    expect(
      resolveStartupSelection({ ...base, defaultProfileId: "mine-1" }, d),
    ).toEqual({ kind: "profile", profileId: "mine-1" });
  });

  it("selects default, then last-used, then local last-used, then recommended", () => {
    expect(
      resolveStartupSelection(
        { ...base, defaultProfileId: "mine-1", lastUsedProfileId: "mine-2" },
        null,
      ),
    ).toEqual({ kind: "profile", profileId: "mine-1" });

    expect(
      resolveStartupSelection({ ...base, lastUsedProfileId: "mine-2" }, null),
    ).toEqual({ kind: "profile", profileId: "mine-2" });

    expect(
      resolveStartupSelection(
        { ...base, localLastUsedProfileId: "official-2" },
        null,
      ),
    ).toEqual({ kind: "profile", profileId: "official-2" });

    expect(resolveStartupSelection(base, null)).toEqual({
      kind: "profile",
      profileId: "official-1",
    });
  });

  it("skips preferences pointing at profiles that no longer exist", () => {
    expect(
      resolveStartupSelection(
        { ...base, defaultProfileId: "deleted", lastUsedProfileId: "mine-1" },
        null,
      ),
    ).toEqual({ kind: "profile", profileId: "mine-1" });
  });

  it("falls back to plain defaults when nothing is available", () => {
    expect(
      resolveStartupSelection(
        { knownProfileIds: [], recommendedId: null },
        null,
      ),
    ).toEqual({ kind: "defaults" });
  });
});
