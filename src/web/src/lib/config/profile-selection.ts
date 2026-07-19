import { CONFIG_SCHEMA_VERSION } from "./constants";
import type { ConfigDraft } from "./profile-storage";
import type { ProfileSummaryDto } from "$lib/schemas/profiles";

// Public official-profile links use readable slugs. Convert those back to the
// internal id used by profile state after the visible profile list loads.
export function resolveProfileReference(
  reference: string | null,
  officials: ProfileSummaryDto[],
): string | null {
  if (!reference) return null;
  return (
    officials.find((profile) => profile.slug === reference)?.id ?? reference
  );
}

// Pure startup-selection resolution for the config page.
//
// Order: explicit profile link > recoverable local draft > the user's
// default profile > last-used profile (server preference, else local) >
// recommended official preset > plain metadata defaults.

export interface StartupBootstrap {
  // Profile id resolved from a path or legacy query reference, if any.
  queryProfileId?: string | null;
  // Server-side preferences (authenticated users).
  defaultProfileId?: string | null;
  lastUsedProfileId?: string | null;
  // Logged-out fallback recorded in localStorage.
  localLastUsedProfileId?: string | null;
  recommendedId?: string | null;
  // Every profile id the user can currently load (officials + own).
  knownProfileIds: Iterable<string>;
}

export type StartupSelection =
  | { kind: "draft"; draft: ConfigDraft }
  | { kind: "profile"; profileId: string }
  | { kind: "defaults" };

export function resolveStartupSelection(
  bootstrap: StartupBootstrap,
  draft: ConfigDraft | null,
): StartupSelection {
  const known = new Set(bootstrap.knownProfileIds);

  if (bootstrap.queryProfileId && known.has(bootstrap.queryProfileId)) {
    return { kind: "profile", profileId: bootstrap.queryProfileId };
  }

  if (draft && draft.configSchemaVersion <= CONFIG_SCHEMA_VERSION) {
    return { kind: "draft", draft };
  }

  for (const candidate of [
    bootstrap.defaultProfileId,
    bootstrap.lastUsedProfileId,
    bootstrap.localLastUsedProfileId,
    bootstrap.recommendedId,
  ]) {
    if (candidate && known.has(candidate)) {
      return { kind: "profile", profileId: candidate };
    }
  }

  return { kind: "defaults" };
}
