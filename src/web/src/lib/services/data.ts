import type { Seed } from "$lib/types";
import type {
  PresetDetailResponseDto,
  PresetListResponseDto,
  PresetPreferencesDto,
} from "$lib/schemas/presets";
import type { NormalizedConfig } from "$lib/config/normalize";

type JsonFetchInit = RequestInit & { errorMessage?: string };

// Error carrying the HTTP status and per-field validation errors so callers
// can react to conflicts (409) and highlight form fields.
export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly fieldErrors?: Record<string, string>,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

async function fetchJson<T>(
  input: RequestInfo | URL,
  init: JsonFetchInit = {},
): Promise<T> {
  const { errorMessage, ...fetchInit } = init;
  const response = await fetch(input, fetchInit);

  if (!response.ok) {
    let message: string | undefined;
    let fieldErrors: Record<string, string> | undefined;
    try {
      const data = (await response.json()) as {
        message?: string;
        error?: string;
        fieldErrors?: Record<string, string>;
      };
      message = data.message || data.error;
      fieldErrors = data.fieldErrors;
    } catch {
      try {
        message = await response.text();
      } catch {
        // ignore secondary failure
      }
    }
    throw new ApiError(
      message?.trim() ||
        errorMessage ||
        `Request failed with ${response.status}`,
      response.status,
      fieldErrors,
    );
  }

  try {
    return (await response.json()) as T;
  } catch {
    throw new Error(
      `Failed to parse response from ${
        typeof input === "string" ? input : input.toString()
      }`,
    );
  }
}

export async function fetchSeed(id: string): Promise<Seed> {
  return await fetchJson<Seed>(`/api/seed/${id}`, {
    errorMessage: `Failed to fetch seed with ID ${id}`,
  });
}

export async function fetchUserSeeds(): Promise<Seed[]> {
  return await fetchJson<Seed[]>("/api/user/seeds", {
    errorMessage: "Failed to fetch user seeds",
  });
}

export interface SeedPresetAttribution {
  presetId: string | null;
  presetRevisionId: string | null;
  settingsSnapshot: NormalizedConfig;
  configSchemaVersion: number;
}

export async function createSeed(
  options: unknown,
  preset?: SeedPresetAttribution,
): Promise<{ id: string }> {
  const body = preset
    ? { ...(options as Record<string, unknown>), Preset: preset }
    : options;
  return await fetchJson<{ id: string }>("/api/randomize", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
    errorMessage: "Failed to create seed",
  });
}

// --- Seed preset API client ---

function jsonInit(method: string, body: unknown): JsonFetchInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}

export async function fetchPresets(
  configId: string,
): Promise<PresetListResponseDto> {
  return await fetchJson<PresetListResponseDto>(
    `/api/presets?configId=${encodeURIComponent(configId)}`,
    { errorMessage: "Failed to load presets" },
  );
}

export async function fetchPreset(
  id: string,
  revisionId?: string | null,
): Promise<PresetDetailResponseDto> {
  const suffix = revisionId
    ? `?revision=${encodeURIComponent(revisionId)}`
    : "";
  return await fetchJson<PresetDetailResponseDto>(
    `/api/presets/${encodeURIComponent(id)}${suffix}`,
    { errorMessage: "Failed to load the preset" },
  );
}

export interface CreatePresetPayload {
  configId: string;
  name: string;
  description?: string;
  settings: NormalizedConfig;
  changeSummary?: string;
  setAsDefault?: boolean;
  favorite?: boolean;
  // Admin only: create a globally visible official preset.
  scope?: "official" | "user";
  slug?: string;
  isRecommended?: boolean;
}

export async function createPreset(
  payload: CreatePresetPayload,
): Promise<PresetDetailResponseDto> {
  return await fetchJson<PresetDetailResponseDto>("/api/presets", {
    ...jsonInit("POST", payload),
    errorMessage: "Failed to save the preset",
  });
}

export async function createPresetRevision(
  presetId: string,
  payload: {
    settings: NormalizedConfig;
    changeSummary?: string;
    baseRevisionId: string | null;
  },
): Promise<PresetDetailResponseDto> {
  return await fetchJson<PresetDetailResponseDto>(
    `/api/presets/${encodeURIComponent(presetId)}/revisions`,
    {
      ...jsonInit("POST", payload),
      errorMessage: "Failed to save the preset",
    },
  );
}

export async function duplicatePreset(
  presetId: string,
  name?: string,
): Promise<PresetDetailResponseDto> {
  return await fetchJson<PresetDetailResponseDto>(
    `/api/presets/${encodeURIComponent(presetId)}/duplicate`,
    {
      ...jsonInit("POST", name ? { name } : {}),
      errorMessage: "Failed to copy the preset",
    },
  );
}

export async function patchPreset(
  presetId: string,
  patch: Record<string, unknown>,
): Promise<{ preset: PresetListResponseDto["officials"][number] }> {
  return await fetchJson(`/api/presets/${encodeURIComponent(presetId)}`, {
    ...jsonInit("PATCH", patch),
    errorMessage: "Failed to update the preset",
  });
}

// Admin: promote a preset received through a share link.
export async function promoteSharedPreset(input: {
  token: string;
  slug: string;
  name?: string;
  description?: string | null;
}): Promise<{ preset: PresetListResponseDto["officials"][number] }> {
  return await fetchJson("/api/presets/promote-shared", {
    ...jsonInit("POST", input),
    errorMessage: "Failed to promote the shared preset",
  });
}

// Admin: copy a preset's current revision into a new official preset.
export async function promotePreset(
  presetId: string,
  input: { slug: string; name?: string; description?: string | null },
): Promise<{ preset: PresetListResponseDto["officials"][number] }> {
  return await fetchJson(
    `/api/presets/${encodeURIComponent(presetId)}/promote`,
    {
      ...jsonInit("POST", input),
      errorMessage: "Failed to promote the preset",
    },
  );
}

export async function ensurePresetShare(
  presetId: string,
): Promise<{ token: string }> {
  return await fetchJson(`/api/presets/${encodeURIComponent(presetId)}/share`, {
    method: "POST",
    errorMessage: "Failed to create the share link",
  });
}

export async function revokePresetShare(presetId: string): Promise<void> {
  await fetchJson(`/api/presets/${encodeURIComponent(presetId)}/share`, {
    method: "DELETE",
    errorMessage: "Failed to disable the share link",
  });
}

export async function deletePreset(presetId: string): Promise<void> {
  await fetchJson(`/api/presets/${encodeURIComponent(presetId)}`, {
    method: "DELETE",
    errorMessage: "Failed to delete the preset",
  });
}

export async function savePresetPreferences(preferences: {
  defaultPresetId?: string | null;
  lastUsedPresetId?: string | null;
}): Promise<{ preferences: PresetPreferencesDto }> {
  return await fetchJson("/api/user/preset-preferences", {
    ...jsonInit("PUT", preferences),
    errorMessage: "Failed to save preferences",
  });
}

// --- API keys ---

export interface ApiKeyDto {
  id: string;
  name: string;
  tokenPrefix: string;
  createdAt: string | null;
  lastUsedAt: string | null;
}

export async function fetchApiKeys(): Promise<{ keys: ApiKeyDto[] }> {
  return await fetchJson("/api/user/api-keys", {
    errorMessage: "Failed to load API keys",
  });
}

export async function createApiKeyRequest(
  name: string,
): Promise<{ key: ApiKeyDto; secret: string }> {
  return await fetchJson("/api/user/api-keys", {
    ...jsonInit("POST", { name }),
    errorMessage: "Failed to create the API key",
  });
}

export async function revokeApiKeyRequest(id: string): Promise<void> {
  await fetchJson(`/api/user/api-keys/${encodeURIComponent(id)}`, {
    method: "DELETE",
    errorMessage: "Failed to revoke the API key",
  });
}

export async function savePresetFavorite(
  presetId: string,
  favorited: boolean,
  displayOrder?: number,
): Promise<void> {
  await fetchJson("/api/user/preset-favorites", {
    ...jsonInit("PUT", { presetId, favorited, displayOrder }),
    errorMessage: "Failed to update favorite",
  });
}
