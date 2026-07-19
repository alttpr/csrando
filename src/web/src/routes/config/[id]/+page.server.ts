import type { Metadata } from "$lib/types";
import type { User } from "lucia";
import * as m from "$lib/paraglide/messages";
import { metadataApi } from "$lib/services/api";
import { parseMetadata } from "$lib/schemas/metadata";
import {
  getSharedProfile,
  listProfilesFor,
} from "$lib/server/profiles/service";
import { getSeedSettingsSnapshot } from "$lib/server/db/seeds";
import type { ProfileListResponseDto } from "$lib/schemas/profiles";
import { resolveProfileReference } from "$lib/config/profile-selection";

// Define our own types since we can't access ./$types
interface Params {
  id: string;
  profile?: string;
}

interface LoadEvent {
  params: Params;
  locals: { user: User | null };
  url: URL;
}

// Settings snapshot delivered through a ?share= capability link. Carries no
// profile ids: the recipient gets a read-only copy, not the profile itself.
export interface SharedProfilePayload {
  name: string;
  description: string | null;
  settings: unknown;
  configSchemaVersion: number;
}

// Settings snapshot of a generated seed, opened through ?fromSeed=. Loads as
// an unowned Custom configuration, exactly like a share link.
export interface SeedSettingsPayload {
  seedId: string;
  settings: unknown;
  configSchemaVersion: number;
}

interface LoadResult {
  metadata: Metadata | null;
  error: string | null;
  configId: string;
  profileBootstrap: ProfileListResponseDto | null;
  queryProfileId: string | null;
  sharedProfile: SharedProfilePayload | null;
  // A ?share= token was present but did not resolve (revoked/deleted).
  sharedInvalid: boolean;
  seedSettings: SeedSettingsPayload | null;
  // A ?fromSeed= id was present but had no stored settings snapshot.
  seedSettingsInvalid: boolean;
}

async function loadSharedProfile(url: URL): Promise<{
  sharedProfile: SharedProfilePayload | null;
  sharedInvalid: boolean;
}> {
  const token = url.searchParams.get("share");
  if (!token) return { sharedProfile: null, sharedInvalid: false };
  try {
    const shared = await getSharedProfile(token);
    if (!shared) return { sharedProfile: null, sharedInvalid: true };
    return {
      sharedProfile: {
        name: shared.profile.name,
        description: shared.profile.description,
        settings: shared.revision.settings,
        configSchemaVersion: shared.revision.configSchemaVersion,
      },
      sharedInvalid: false,
    };
  } catch (e) {
    console.error("Failed to resolve shared profile link:", e);
    return { sharedProfile: null, sharedInvalid: true };
  }
}

async function loadSeedSettings(url: URL): Promise<{
  seedSettings: SeedSettingsPayload | null;
  seedSettingsInvalid: boolean;
}> {
  const seedId = url.searchParams.get("fromSeed");
  if (!seedId) return { seedSettings: null, seedSettingsInvalid: false };
  const snapshot = await getSeedSettingsSnapshot(seedId);
  if (!snapshot) return { seedSettings: null, seedSettingsInvalid: true };
  return { seedSettings: snapshot, seedSettingsInvalid: false };
}

async function loadProfileBootstrap(
  configId: string,
  user: User | null,
): Promise<ProfileListResponseDto | null> {
  try {
    return await listProfilesFor(configId, user);
  } catch (e) {
    // Profiles are an enhancement; the config page must work without them.
    console.error(`Failed to load profiles for '${configId}':`, e);
    return null;
  }
}

export const load = async ({
  params,
  locals,
  url,
}: LoadEvent): Promise<LoadResult> => {
  const id = (params.id || "").toLowerCase();
  // Path-style profile links are canonical; retain query support for links
  // created before `/config/{config}/{profile}` was introduced.
  const queryProfileId = params.profile ?? url.searchParams.get("profile");
  const shared = await loadSharedProfile(url);
  const fromSeed = await loadSeedSettings(url);
  if (!id) {
    return {
      metadata: null,
      error: "Invalid ID provided.",
      configId: id,
      profileBootstrap: null,
      queryProfileId,
      ...shared,
      ...fromSeed,
    };
  }

  try {
    const canonical = await metadataApi.resolveCanonicalId(id);
    // Profiles use lowercase config ids everywhere; the canonical id keeps
    // the backend's casing (e.g. "Combo") for metadata fetches only.
    const profileConfigId = canonical.toLowerCase();
    const raw = await metadataApi.getById(canonical);

    const parsed = parseMetadata(raw);
    if (!parsed.success) {
      console.error("Invalid metadata payload:", parsed.error.flatten());
      return {
        metadata: null,
        error: m.config_metadata_fetch_error(),
        configId: profileConfigId,
        profileBootstrap: null,
        queryProfileId,
        ...shared,
        ...fromSeed,
      };
    }
    const metadata = parsed.data;

    if (!metadata) {
      return {
        metadata: null,
        error: m.config_no_metadata(),
        configId: profileConfigId,
        profileBootstrap: null,
        queryProfileId,
        ...shared,
        ...fromSeed,
      };
    }

    const profileBootstrap = await loadProfileBootstrap(
      profileConfigId,
      locals.user,
    );

    return {
      metadata,
      error: null,
      configId: profileConfigId,
      profileBootstrap,
      queryProfileId: resolveProfileReference(
        queryProfileId,
        profileBootstrap?.officials ?? [],
      ),
      ...shared,
      ...fromSeed,
    };
  } catch (e: unknown) {
    console.error(`Exception while fetching metadata for ID ${id}:`, e);

    // The error handling from our API service will provide proper error messages
    let errorMsg = m.config_metadata_fetch_error();
    if ((e as { status?: number }).status === 404) {
      errorMsg = `Metadata not found for ID: ${id}.`;
    } else if ((e as { body?: { message?: string } }).body?.message) {
      errorMsg = (e as { body?: { message?: string } }).body?.message as string;
    } else if ((e as { message?: string }).message) {
      errorMsg = (e as { message?: string }).message as string;
    }

    return {
      metadata: null,
      error: errorMsg,
      configId: id,
      profileBootstrap: null,
      queryProfileId,
      ...shared,
      ...fromSeed,
    };
  }
};
