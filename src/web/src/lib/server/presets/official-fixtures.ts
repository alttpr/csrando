// Fixtures for administrator-curated official presets that are seeded
// automatically when missing (idempotent, keyed by slug).
//
// Fixtures intentionally contain NO settings values: each preset's settings
// snapshot is computed at seed time from the generator's own metadata
// defaults, so we never invent game-design values here. To curate different
// settings for a preset, seed it and then save a new revision through the
// admin surface (or add a `transform` below once real curated values exist).
//
// To add a new official preset:
// 1. Add an entry here with a unique slug.
// 2. Restart the server (or hit the presets API) — seeding runs on boot and
//    lazily from the presets list endpoint.
// 3. Curate name/description/tags/settings via the admin actions in the UI.
import type { NormalizedConfig } from "$lib/config/normalize";

export interface OfficialPresetFixture {
  slug: string;
  configId: string;
  name: string;
  description: string;
  gameTags: string[];
  difficultyTag: string | null;
  isRecommended: boolean;
  featured: boolean;
  displayOrder: number;
  // Optional hook to adjust the metadata-default settings for this preset.
  // Must only touch values that exist in the metadata.
  transform?: (defaults: NormalizedConfig) => NormalizedConfig;
}

export const OFFICIAL_PRESET_FIXTURES: OfficialPresetFixture[] = [
  {
    slug: "recommended",
    configId: "combo",
    name: "Recommended",
    description:
      "The recommended starting configuration: all games with their default settings.",
    gameTags: [],
    difficultyTag: "Beginner",
    isRecommended: true,
    featured: true,
    displayOrder: 0,
  },
];
