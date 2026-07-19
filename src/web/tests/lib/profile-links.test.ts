import { describe, expect, it } from "vitest";
import { profileConfigPath, seedSettingsPath } from "$lib/config/profile-links";

const official = {
  profileId: "internal-profile-id",
  slug: "recommended",
  configId: "combo",
};

describe("profile links", () => {
  it("uses an official profile's readable slug", () => {
    expect(profileConfigPath(official)).toBe("/config/combo/recommended");
  });

  it("opens an accessible unchanged profile directly", () => {
    expect(
      seedSettingsPath({
        seedId: "seed-id",
        settingsConfigId: "combo",
        profile: official,
        differedFromRevision: false,
      }),
    ).toBe("/config/combo/recommended");
  });

  it("uses the seed snapshot when the profile was modified", () => {
    expect(
      seedSettingsPath({
        seedId: "seed-id",
        settingsConfigId: "combo",
        profile: official,
        differedFromRevision: true,
      }),
    ).toBe("/config/combo?fromSeed=seed-id");
  });

  it("uses the seed snapshot when the profile is inaccessible or absent", () => {
    expect(
      seedSettingsPath({
        seedId: "seed-id",
        settingsConfigId: "combo",
        profile: null,
        differedFromRevision: false,
      }),
    ).toBe("/config/combo?fromSeed=seed-id");
  });
});
