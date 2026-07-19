import { describe, expect, it } from "vitest";
import {
  CONFIG_MIGRATIONS,
  ConfigMigrationError,
  migrateConfig,
  type ConfigMigration,
} from "$lib/config/migrations";
import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
import type { NormalizedConfig } from "$lib/config/normalize";

const baseConfig: NormalizedConfig = {
  selectedGames: ["Alttpr"],
  global: { Game: "Combo" },
  perGame: { Alttpr: { Swords: "Randomized" } },
};

describe("migrateConfig", () => {
  it("returns the config unchanged when it is already current", () => {
    const result = migrateConfig(baseConfig, CONFIG_SCHEMA_VERSION);
    expect(result.settings).toBe(baseConfig);
    expect(result.fromVersion).toBe(CONFIG_SCHEMA_VERSION);
    expect(result.toVersion).toBe(CONFIG_SCHEMA_VERSION);
    expect(result.appliedSteps).toEqual([]);
  });

  it("rejects versions newer than the application supports", () => {
    expect(() =>
      migrateConfig(baseConfig, CONFIG_SCHEMA_VERSION + 1),
    ).toThrowError(ConfigMigrationError);
  });

  it("rejects invalid versions", () => {
    expect(() => migrateConfig(baseConfig, 0)).toThrowError(
      ConfigMigrationError,
    );
    expect(() => migrateConfig(baseConfig, 1.5)).toThrowError(
      ConfigMigrationError,
    );
  });

  it("applies migrations sequentially and reports each step", () => {
    const migrations: ConfigMigration[] = [
      {
        from: 1,
        to: 2,
        description: "rename Swords",
        migrate(settings) {
          const { Swords, ...rest } = settings.perGame.Alttpr;
          return {
            ...settings,
            perGame: {
              ...settings.perGame,
              Alttpr: { ...rest, SwordMode: Swords },
            },
          };
        },
      },
      {
        from: 2,
        to: 3,
        description: "add difficulty",
        migrate(settings) {
          return {
            ...settings,
            perGame: {
              ...settings.perGame,
              Alttpr: { ...settings.perGame.Alttpr, Difficulty: "Normal" },
            },
          };
        },
      },
    ];

    const result = migrateConfig(baseConfig, 1, migrations, 3);
    expect(result.toVersion).toBe(3);
    expect(result.appliedSteps).toEqual(["rename Swords", "add difficulty"]);
    expect(result.settings.perGame.Alttpr).toEqual({
      SwordMode: "Randomized",
      Difficulty: "Normal",
    });
    // Original input must never be mutated.
    expect(baseConfig.perGame.Alttpr).toEqual({ Swords: "Randomized" });
  });

  it("fails loudly when a migration step is missing", () => {
    const migrations: ConfigMigration[] = [
      {
        from: 2,
        to: 3,
        description: "later step only",
        migrate: (s) => s,
      },
    ];
    expect(() => migrateConfig(baseConfig, 1, migrations, 3)).toThrowError(
      /No migration path/,
    );
  });

  it("ships with a valid (possibly empty) registered migration chain", () => {
    // Every registered migration must advance by exactly one contiguous step.
    const sorted = [...CONFIG_MIGRATIONS].sort((a, b) => a.from - b.from);
    for (const migration of sorted) {
      expect(migration.to).toBe(migration.from + 1);
      expect(migration.to).toBeLessThanOrEqual(CONFIG_SCHEMA_VERSION);
    }
  });
});
