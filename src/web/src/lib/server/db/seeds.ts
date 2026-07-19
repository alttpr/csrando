import { db } from "$lib/server/db";
import { eq } from "drizzle-orm";
import { seeds } from "./schema";

// The normalized configuration a seed was generated with, for loading back
// into the config page. Older seeds predate the snapshot columns and return
// null. Never throws.
export async function getSeedSettingsSnapshot(seedId: string): Promise<{
  seedId: string;
  settings: unknown;
  configSchemaVersion: number;
} | null> {
  if (!seedId) return null;
  try {
    const rows = await db
      .select({
        settingsSnapshot: seeds.settingsSnapshot,
        configSchemaVersion: seeds.configSchemaVersion,
      })
      .from(seeds)
      .where(eq(seeds.id, seedId))
      .limit(1);
    const row = rows[0];
    if (!row?.settingsSnapshot || !row.configSchemaVersion) return null;
    return {
      seedId,
      settings: row.settingsSnapshot,
      configSchemaVersion: row.configSchemaVersion,
    };
  } catch (e) {
    console.error("Failed to load seed settings snapshot:", e);
    return null;
  }
}
