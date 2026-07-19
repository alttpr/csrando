import type { Seed } from "$lib/types";
import type {
  ProfileDetailResponseDto,
  ProfileListResponseDto,
  ProfilePreferencesDto,
} from "$lib/schemas/profiles";
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

export interface SeedProfileAttribution {
  profileId: string | null;
  profileRevisionId: string | null;
  settingsSnapshot: NormalizedConfig;
  configSchemaVersion: number;
}

export async function createSeed(
  options: unknown,
  profile?: SeedProfileAttribution,
): Promise<{ id: string }> {
  const body = profile
    ? { ...(options as Record<string, unknown>), Profile: profile }
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

// --- Seed profile API client ---

function jsonInit(method: string, body: unknown): JsonFetchInit {
  return {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  };
}

export async function fetchProfiles(
  configId: string,
): Promise<ProfileListResponseDto> {
  return await fetchJson<ProfileListResponseDto>(
    `/api/profiles?configId=${encodeURIComponent(configId)}`,
    { errorMessage: "Failed to load profiles" },
  );
}

export async function fetchProfile(
  id: string,
  revisionId?: string | null,
): Promise<ProfileDetailResponseDto> {
  const suffix = revisionId
    ? `?revision=${encodeURIComponent(revisionId)}`
    : "";
  return await fetchJson<ProfileDetailResponseDto>(
    `/api/profiles/${encodeURIComponent(id)}${suffix}`,
    { errorMessage: "Failed to load the profile" },
  );
}

export interface CreateProfilePayload {
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

export async function createProfile(
  payload: CreateProfilePayload,
): Promise<ProfileDetailResponseDto> {
  return await fetchJson<ProfileDetailResponseDto>("/api/profiles", {
    ...jsonInit("POST", payload),
    errorMessage: "Failed to save the profile",
  });
}

export async function createProfileRevision(
  profileId: string,
  payload: {
    settings: NormalizedConfig;
    changeSummary?: string;
    baseRevisionId: string | null;
  },
): Promise<ProfileDetailResponseDto> {
  return await fetchJson<ProfileDetailResponseDto>(
    `/api/profiles/${encodeURIComponent(profileId)}/revisions`,
    {
      ...jsonInit("POST", payload),
      errorMessage: "Failed to save the profile",
    },
  );
}

export async function duplicateProfile(
  profileId: string,
  name?: string,
): Promise<ProfileDetailResponseDto> {
  return await fetchJson<ProfileDetailResponseDto>(
    `/api/profiles/${encodeURIComponent(profileId)}/duplicate`,
    {
      ...jsonInit("POST", name ? { name } : {}),
      errorMessage: "Failed to copy the profile",
    },
  );
}

export async function patchProfile(
  profileId: string,
  patch: Record<string, unknown>,
): Promise<{ profile: ProfileListResponseDto["officials"][number] }> {
  return await fetchJson(`/api/profiles/${encodeURIComponent(profileId)}`, {
    ...jsonInit("PATCH", patch),
    errorMessage: "Failed to update the profile",
  });
}

// Admin: promote a profile received through a share link.
export async function promoteSharedProfile(input: {
  token: string;
  slug: string;
  name?: string;
  description?: string | null;
}): Promise<{ profile: ProfileListResponseDto["officials"][number] }> {
  return await fetchJson("/api/profiles/promote-shared", {
    ...jsonInit("POST", input),
    errorMessage: "Failed to promote the shared profile",
  });
}

// Admin: copy a profile's current revision into a new official preset.
export async function promoteProfile(
  profileId: string,
  input: { slug: string; name?: string; description?: string | null },
): Promise<{ profile: ProfileListResponseDto["officials"][number] }> {
  return await fetchJson(
    `/api/profiles/${encodeURIComponent(profileId)}/promote`,
    {
      ...jsonInit("POST", input),
      errorMessage: "Failed to promote the profile",
    },
  );
}

export async function ensureProfileShare(
  profileId: string,
): Promise<{ token: string }> {
  return await fetchJson(
    `/api/profiles/${encodeURIComponent(profileId)}/share`,
    {
      method: "POST",
      errorMessage: "Failed to create the share link",
    },
  );
}

export async function revokeProfileShare(profileId: string): Promise<void> {
  await fetchJson(`/api/profiles/${encodeURIComponent(profileId)}/share`, {
    method: "DELETE",
    errorMessage: "Failed to disable the share link",
  });
}

export async function deleteProfile(profileId: string): Promise<void> {
  await fetchJson(`/api/profiles/${encodeURIComponent(profileId)}`, {
    method: "DELETE",
    errorMessage: "Failed to delete the profile",
  });
}

export async function saveProfilePreferences(preferences: {
  defaultProfileId?: string | null;
  lastUsedProfileId?: string | null;
}): Promise<{ preferences: ProfilePreferencesDto }> {
  return await fetchJson("/api/user/profile-preferences", {
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

export async function saveProfileFavorite(
  profileId: string,
  favorited: boolean,
  displayOrder?: number,
): Promise<void> {
  await fetchJson("/api/user/profile-favorites", {
    ...jsonInit("PUT", { profileId, favorited, displayOrder }),
    errorMessage: "Failed to update favorite",
  });
}
