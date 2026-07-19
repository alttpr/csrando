import { describe, expect, it } from "vitest";
import { presetConfigPath, seedSettingsPath } from "$lib/config/preset-links";

const official = {
  presetId: "internal-preset-id",
  slug: "recommended",
  configId: "combo",
};

describe("preset links", () => {
  it("uses an official preset's readable slug", () => {
    expect(presetConfigPath(official)).toBe("/config/combo/recommended");
  });

  it("opens an accessible unchanged preset directly", () => {
    expect(
      seedSettingsPath({
        seedId: "seed-id",
        settingsConfigId: "combo",
        preset: official,
        differedFromRevision: false,
      }),
    ).toBe("/config/combo/recommended");
  });

  it("uses the seed snapshot when the preset was modified", () => {
    expect(
      seedSettingsPath({
        seedId: "seed-id",
        settingsConfigId: "combo",
        preset: official,
        differedFromRevision: true,
      }),
    ).toBe("/config/combo?fromSeed=seed-id");
  });

  it("uses the seed snapshot when the preset is inaccessible or absent", () => {
    expect(
      seedSettingsPath({
        seedId: "seed-id",
        settingsConfigId: "combo",
        preset: null,
        differedFromRevision: false,
      }),
    ).toBe("/config/combo?fromSeed=seed-id");
  });
});
