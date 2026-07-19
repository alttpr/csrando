import { CONFIG_SCHEMA_VERSION } from "./constants";
import type { NormalizedConfig } from "./normalize";

// Sequential, versioned migrations for stored seed configurations. When a
// setting changes shape in a future release, bump CONFIG_SCHEMA_VERSION and
// append a migration step here; older profile revisions and drafts are then
// upgraded on load.
export interface ConfigMigration {
  from: number;
  to: number;
  // Human-readable summary shown to the user when the migration changes
  // or drops settings.
  description: string;
  migrate(settings: NormalizedConfig): NormalizedConfig;
}

export const CONFIG_MIGRATIONS: ConfigMigration[] = [
  // Example shape for a future migration:
  // {
  //   from: 1,
  //   to: 2,
  //   description: "Renamed Alttpr.SwordMode to Alttpr.Swords",
  //   migrate(settings) { ...return a new object, never mutate... },
  // },
];

export interface MigrationResult {
  settings: NormalizedConfig;
  fromVersion: number;
  toVersion: number;
  // Descriptions of each applied step, for surfacing to the user.
  appliedSteps: string[];
}

export class ConfigMigrationError extends Error {
  constructor(
    message: string,
    public readonly fromVersion: number,
  ) {
    super(message);
    this.name = "ConfigMigrationError";
  }
}

export function migrateConfig(
  settings: NormalizedConfig,
  fromVersion: number,
  migrations: ConfigMigration[] = CONFIG_MIGRATIONS,
  targetVersion: number = CONFIG_SCHEMA_VERSION,
): MigrationResult {
  if (!Number.isInteger(fromVersion) || fromVersion < 1) {
    throw new ConfigMigrationError(
      `Invalid configuration schema version: ${fromVersion}`,
      fromVersion,
    );
  }
  if (fromVersion > targetVersion) {
    throw new ConfigMigrationError(
      `Configuration schema version ${fromVersion} is newer than this application supports (${targetVersion})`,
      fromVersion,
    );
  }

  let current = settings;
  let version = fromVersion;
  const appliedSteps: string[] = [];

  while (version < targetVersion) {
    const step = migrations.find((m) => m.from === version);
    if (!step) {
      throw new ConfigMigrationError(
        `No migration path from configuration schema version ${version}`,
        fromVersion,
      );
    }
    current = step.migrate(current);
    appliedSteps.push(step.description);
    version = step.to;
  }

  return {
    settings: current,
    fromVersion,
    toVersion: version,
    appliedSteps,
  };
}
