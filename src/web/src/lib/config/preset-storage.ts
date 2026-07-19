import { CONFIG_SCHEMA_VERSION, MAX_RECENT_PRESETS } from "./constants";
import type { NormalizedConfig } from "./normalize";

// Browser-local persistence for the config page: an unsaved-work draft,
// recently used presets, and the logged-out last-used preset. All reads and
// writes are best-effort (SSR-safe, quota/corruption tolerant) — this is a
// convenience layer, never presented as saved data.

export interface ConfigDraft {
  savedAt: number;
  configSchemaVersion: number;
  settings: NormalizedConfig;
  // The preset the draft diverged from, when there was one.
  presetId: string | null;
  presetRevisionId: string | null;
}

function storage(): Storage | null {
  try {
    if (typeof localStorage === "undefined") return null;
    return localStorage;
  } catch {
    return null;
  }
}

function draftKey(configId: string): string {
  return `csrando:config-draft:${configId}`;
}

function recentKey(configId: string): string {
  return `csrando:recent-presets:${configId}`;
}

function lastUsedKey(configId: string): string {
  return `csrando:last-preset:${configId}`;
}

export function loadDraft(configId: string): ConfigDraft | null {
  const store = storage();
  if (!store) return null;
  try {
    const raw = store.getItem(draftKey(configId));
    if (!raw) return null;
    const parsed = JSON.parse(raw) as Partial<ConfigDraft>;
    if (
      typeof parsed !== "object" ||
      parsed === null ||
      typeof parsed.savedAt !== "number" ||
      typeof parsed.configSchemaVersion !== "number" ||
      typeof parsed.settings !== "object" ||
      parsed.settings === null
    ) {
      return null;
    }
    return {
      savedAt: parsed.savedAt,
      configSchemaVersion: parsed.configSchemaVersion,
      settings: parsed.settings as NormalizedConfig,
      presetId: typeof parsed.presetId === "string" ? parsed.presetId : null,
      presetRevisionId:
        typeof parsed.presetRevisionId === "string"
          ? parsed.presetRevisionId
          : null,
    };
  } catch {
    return null;
  }
}

export function saveDraft(
  configId: string,
  draft: Omit<ConfigDraft, "savedAt" | "configSchemaVersion">,
): void {
  const store = storage();
  if (!store) return;
  try {
    const full: ConfigDraft = {
      ...draft,
      savedAt: Date.now(),
      configSchemaVersion: CONFIG_SCHEMA_VERSION,
    };
    store.setItem(draftKey(configId), JSON.stringify(full));
  } catch {
    // Quota exceeded or storage unavailable — drafts are best-effort.
  }
}

export function clearDraft(configId: string): void {
  try {
    storage()?.removeItem(draftKey(configId));
  } catch {
    // ignore
  }
}

export function loadRecentPresets(configId: string): string[] {
  const store = storage();
  if (!store) return [];
  try {
    const raw = store.getItem(recentKey(configId));
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) return [];
    return parsed.filter((id): id is string => typeof id === "string");
  } catch {
    return [];
  }
}

export function recordRecentPreset(configId: string, presetId: string): void {
  const store = storage();
  if (!store) return;
  try {
    const current = loadRecentPresets(configId).filter((id) => id !== presetId);
    current.unshift(presetId);
    store.setItem(
      recentKey(configId),
      JSON.stringify(current.slice(0, MAX_RECENT_PRESETS)),
    );
    store.setItem(lastUsedKey(configId), presetId);
  } catch {
    // ignore
  }
}

export function forgetPreset(configId: string, presetId: string): void {
  const store = storage();
  if (!store) return;
  try {
    const current = loadRecentPresets(configId).filter((id) => id !== presetId);
    store.setItem(recentKey(configId), JSON.stringify(current));
    if (store.getItem(lastUsedKey(configId)) === presetId) {
      store.removeItem(lastUsedKey(configId));
    }
  } catch {
    // ignore
  }
}

export function loadLocalLastUsedPreset(configId: string): string | null {
  const store = storage();
  if (!store) return null;
  try {
    return store.getItem(lastUsedKey(configId));
  } catch {
    return null;
  }
}
