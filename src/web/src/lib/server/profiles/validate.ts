import { parseMetadata } from "$lib/schemas/metadata";
import { NormalizedConfigSchema } from "$lib/schemas/profiles";
import { MAX_SETTINGS_JSON_BYTES } from "$lib/config/constants";
import {
  hydrateFormState,
  normalizeConfig,
  type NormalizedConfig,
} from "$lib/config/normalize";
import { getActiveRandomizerVersionFor } from "$lib/server/db/randomizer";
import { metadataApi } from "$lib/services/api";
import type { Metadata } from "$lib/types";

export interface SettingsValidationResult {
  ok: boolean;
  // Canonicalized settings safe to persist (only when ok).
  settings?: NormalizedConfig;
  message?: string;
  fieldErrors?: Record<string, string>;
}

// Resolve the generator metadata used to validate stored settings. Prefers the
// active randomizer version snapshot (no network); falls back to the live
// backend metadata; returns null when neither is available (e.g. dev without
// a backend), in which case only shape/size validation applies.
export async function getValidationMetadata(
  configId: string,
): Promise<Metadata | null> {
  try {
    const version = await getActiveRandomizerVersionFor(configId);
    if (version?.optionsMetadata) {
      const parsed = parseMetadata(version.optionsMetadata);
      if (parsed.success) return parsed.data;
      console.error(
        `Stored options metadata for '${configId}' failed to parse; falling back to live metadata`,
      );
    }
  } catch (err) {
    console.error(`Failed to load randomizer version for '${configId}':`, err);
  }

  try {
    const canonical = await metadataApi.resolveCanonicalId(configId);
    const raw = await metadataApi.getById(canonical);
    const parsed = parseMetadata(raw);
    if (parsed.success) return parsed.data;
  } catch {
    // Backend unavailable; shape-only validation.
  }
  return null;
}

// Validate and canonicalize a submitted settings payload using the real
// generator metadata. Rejects unknown or invalid setting values instead of
// silently dropping them.
export function validateSettings(
  raw: unknown,
  metadata: Metadata | null,
): SettingsValidationResult {
  const shape = NormalizedConfigSchema.safeParse(raw);
  if (!shape.success) {
    return { ok: false, message: "Invalid settings payload" };
  }

  const size = JSON.stringify(shape.data).length;
  if (size > MAX_SETTINGS_JSON_BYTES) {
    return {
      ok: false,
      message: `Settings payload is too large (${size} bytes, max ${MAX_SETTINGS_JSON_BYTES})`,
    };
  }

  const candidate = shape.data as NormalizedConfig;

  if (!metadata) {
    console.warn(
      "No generator metadata available; storing settings with shape-only validation",
    );
    return { ok: true, settings: candidate };
  }

  const { report } = hydrateFormState(candidate, metadata);
  if (report.removedKeys.length > 0 || report.resetKeys.length > 0) {
    const fieldErrors: Record<string, string> = {};
    for (const key of report.removedKeys) {
      fieldErrors[key] = "Unknown setting";
    }
    for (const key of report.resetKeys) {
      fieldErrors[key] = "Invalid value";
    }
    return {
      ok: false,
      message: "Settings contain unknown or invalid values",
      fieldErrors,
    };
  }

  return { ok: true, settings: normalizeConfig(candidate, metadata) };
}
