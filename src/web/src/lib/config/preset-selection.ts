import { CONFIG_SCHEMA_VERSION } from "./constants";
import type { ConfigDraft } from "./preset-storage";
import type { PresetSummaryDto } from "$lib/schemas/presets";

// Public official-preset links use readable slugs. Convert those back to the
// internal id used by preset state after the visible preset list loads.
export function resolvePresetReference(
  reference: string | null,
  officials: PresetSummaryDto[],
): string | null {
  if (!reference) return null;
  return officials.find((preset) => preset.slug === reference)?.id ?? reference;
}

// Pure startup-selection resolution for the config page.
//
// Order: explicit preset link > recoverable local draft > the user's
// default preset > last-used preset (server preference, else local) >
// recommended official preset > plain metadata defaults.

export interface StartupBootstrap {
  // Preset id resolved from a path or legacy query reference, if any.
  queryPresetId?: string | null;
  // Server-side preferences (authenticated users).
  defaultPresetId?: string | null;
  lastUsedPresetId?: string | null;
  // Logged-out fallback recorded in localStorage.
  localLastUsedPresetId?: string | null;
  recommendedId?: string | null;
  // Every preset id the user can currently load (officials + own).
  knownPresetIds: Iterable<string>;
}

export type StartupSelection =
  | { kind: "draft"; draft: ConfigDraft }
  | { kind: "preset"; presetId: string }
  | { kind: "defaults" };

export function resolveStartupSelection(
  bootstrap: StartupBootstrap,
  draft: ConfigDraft | null,
): StartupSelection {
  const known = new Set(bootstrap.knownPresetIds);

  if (bootstrap.queryPresetId && known.has(bootstrap.queryPresetId)) {
    return { kind: "preset", presetId: bootstrap.queryPresetId };
  }

  if (draft && draft.configSchemaVersion <= CONFIG_SCHEMA_VERSION) {
    return { kind: "draft", draft };
  }

  for (const candidate of [
    bootstrap.defaultPresetId,
    bootstrap.lastUsedPresetId,
    bootstrap.localLastUsedPresetId,
    bootstrap.recommendedId,
  ]) {
    if (candidate && known.has(candidate)) {
      return { kind: "preset", presetId: candidate };
    }
  }

  return { kind: "defaults" };
}
