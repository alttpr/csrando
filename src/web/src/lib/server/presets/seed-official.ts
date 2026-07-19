import { and, eq } from "drizzle-orm";
import { db } from "$lib/server/db";
import {
  configurationPresets,
  configurationPresetRevisions,
} from "$lib/server/db/schema";
import { generateId } from "$lib/utils/id";
import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
import { hydrateFormState } from "$lib/config/normalize";
import { OFFICIAL_PRESET_FIXTURES } from "./official-fixtures";
import { getValidationMetadata } from "./validate";

// Avoid hammering the metadata source when it is unavailable: retry lazily at
// most once per minute until every fixture has been seeded.
const RETRY_INTERVAL_MS = 60_000;
let allSeeded = false;
let lastAttempt = 0;

async function fixtureExists(slug: string): Promise<boolean> {
  const rows = await db
    .select({ id: configurationPresets.id })
    .from(configurationPresets)
    .where(
      and(
        eq(configurationPresets.scope, "official"),
        eq(configurationPresets.slug, slug),
      ),
    )
    .limit(1);
  return rows.length > 0;
}

// Seed missing official presets from fixtures. Idempotent (keyed by slug) and
// safe to call at boot and lazily from request paths; never throws.
export async function seedOfficialPresets(): Promise<void> {
  let pending = false;

  for (const fixture of OFFICIAL_PRESET_FIXTURES) {
    try {
      if (await fixtureExists(fixture.slug)) continue;

      const metadata = await getValidationMetadata(fixture.configId);
      if (!metadata) {
        console.warn(
          `Skipping official preset '${fixture.slug}': no generator metadata available yet`,
        );
        pending = true;
        continue;
      }

      // Settings are the generator's own defaults (all games selected),
      // optionally adjusted by the fixture.
      const { form } = hydrateFormState({}, metadata);
      const settings = fixture.transform ? fixture.transform(form) : form;

      const now = new Date();
      const presetId = generateId();
      const revisionId = generateId();

      db.transaction((tx) => {
        tx.insert(configurationPresets)
          .values({
            id: presetId,
            ownerUserId: null,
            scope: "official",
            slug: fixture.slug,
            configId: fixture.configId,
            name: fixture.name,
            description: fixture.description,
            currentRevisionId: null,
            gameTags: fixture.gameTags,
            difficultyTag: fixture.difficultyTag,
            isRecommended: fixture.isRecommended,
            featured: fixture.featured,
            displayOrder: fixture.displayOrder,
            createdAt: now,
            updatedAt: now,
          })
          .run();
        tx.insert(configurationPresetRevisions)
          .values({
            id: revisionId,
            presetId,
            revisionNumber: 1,
            configSchemaVersion: CONFIG_SCHEMA_VERSION,
            settings,
            changeSummary: "Initial preset",
            createdBy: null,
            createdAt: now,
            publishedAt: now,
          })
          .run();
        tx.update(configurationPresets)
          .set({ currentRevisionId: revisionId })
          .where(eq(configurationPresets.id, presetId))
          .run();
      });

      console.log(`Seeded official preset '${fixture.slug}'`);
    } catch (err) {
      console.error(`Failed to seed official preset '${fixture.slug}':`, err);
      pending = true;
    }
  }

  allSeeded = !pending;
}

export async function ensureOfficialPresetsSeeded(): Promise<void> {
  if (allSeeded) return;
  const now = Date.now();
  if (now - lastAttempt < RETRY_INTERVAL_MS) return;
  lastAttempt = now;
  await seedOfficialPresets();
}

// Test hook: reset the lazy-seeding memoization.
export function resetSeedStateForTests(): void {
  allSeeded = false;
  lastAttempt = 0;
}
