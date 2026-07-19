import { error } from "@sveltejs/kit";
import { and, asc, eq, isNull, sql } from "drizzle-orm";
import { customAlphabet } from "nanoid";
import type { User } from "lucia";
import { db } from "$lib/server/db";
import {
  configurationProfiles,
  configurationProfileRevisions,
  userProfileFavorites,
  userProfilePreferences,
} from "$lib/server/db/schema";
import { generateId } from "$lib/utils/id";
import {
  CONFIG_SCHEMA_VERSION,
  MAX_USER_PROFILES,
} from "$lib/config/constants";
import type { NormalizedConfig } from "$lib/config/normalize";
import type {
  ProfileListResponseDto,
  ProfilePreferencesDto,
  ProfileRevisionDto,
  ProfileSummaryDto,
} from "$lib/schemas/profiles";
import { ensureOfficialProfilesSeeded } from "./seed-official";

type ProfileRow = typeof configurationProfiles.$inferSelect;
type RevisionRow = typeof configurationProfileRevisions.$inferSelect;

function toIso(value: unknown): string | null {
  if (value instanceof Date) return value.toISOString();
  if (typeof value === "string" || typeof value === "number") {
    const d = new Date(value);
    return Number.isNaN(d.getTime()) ? null : d.toISOString();
  }
  return null;
}

function selectedGamesOf(revision: RevisionRow | null | undefined): string[] {
  const settings = revision?.settings as Partial<NormalizedConfig> | undefined;
  return Array.isArray(settings?.selectedGames)
    ? settings.selectedGames.filter((g): g is string => typeof g === "string")
    : [];
}

export function toProfileSummary(
  profile: ProfileRow,
  revision?: RevisionRow | null,
): ProfileSummaryDto {
  return {
    id: profile.id,
    scope: profile.scope,
    slug: profile.slug,
    configId: profile.configId,
    name: profile.name,
    description: profile.description,
    currentRevisionId: profile.currentRevisionId,
    revisionNumber: revision?.revisionNumber ?? null,
    configSchemaVersion: revision?.configSchemaVersion ?? null,
    selectedGames: selectedGamesOf(revision),
    gameTags: Array.isArray(profile.gameTags)
      ? (profile.gameTags as string[])
      : null,
    difficultyTag: profile.difficultyTag,
    isRecommended: profile.isRecommended,
    featured: profile.featured,
    archived: profile.archived,
    displayOrder: profile.displayOrder,
    createdAt: toIso(profile.createdAt),
    updatedAt: toIso(profile.updatedAt),
  };
}

export function toRevisionDto(revision: RevisionRow): ProfileRevisionDto {
  return {
    id: revision.id,
    profileId: revision.profileId,
    revisionNumber: revision.revisionNumber,
    configSchemaVersion: revision.configSchemaVersion,
    settings: revision.settings,
    changeSummary: revision.changeSummary,
    createdAt: toIso(revision.createdAt),
  };
}

async function listWithCurrentRevision(where: ReturnType<typeof and>) {
  const rows = await db
    .select()
    .from(configurationProfiles)
    .leftJoin(
      configurationProfileRevisions,
      eq(
        configurationProfiles.currentRevisionId,
        configurationProfileRevisions.id,
      ),
    )
    .where(where)
    .orderBy(
      asc(configurationProfiles.displayOrder),
      asc(configurationProfiles.name),
    );
  return rows.map((row) =>
    toProfileSummary(
      row.configuration_profile,
      row.configuration_profile_revision,
    ),
  );
}

export function pickRecommended(officials: ProfileSummaryDto[]): string | null {
  const active = officials.filter((p) => !p.archived);
  const recommended = active.find((p) => p.isRecommended);
  if (recommended) return recommended.id;
  // Deterministic fallback: lowest display order, then lowest slug/name.
  const sorted = [...active].sort(
    (a, b) =>
      a.displayOrder - b.displayOrder ||
      (a.slug ?? a.name).localeCompare(b.slug ?? b.name),
  );
  return sorted[0]?.id ?? null;
}

export async function getPreferences(
  user: User,
): Promise<ProfilePreferencesDto> {
  const prefRows = await db
    .select()
    .from(userProfilePreferences)
    .where(eq(userProfilePreferences.userId, user.id))
    .limit(1);
  const favorites = await db
    .select({
      profileId: userProfileFavorites.profileId,
      displayOrder: userProfileFavorites.displayOrder,
    })
    .from(userProfileFavorites)
    .where(eq(userProfileFavorites.userId, user.id))
    .orderBy(asc(userProfileFavorites.displayOrder));
  return {
    defaultProfileId: prefRows[0]?.defaultProfileId ?? null,
    lastUsedProfileId: prefRows[0]?.lastUsedProfileId ?? null,
    favorites,
  };
}

export async function listProfilesFor(
  configId: string,
  user: User | null,
): Promise<ProfileListResponseDto> {
  // Config ids are stored lowercase; callers may pass the backend's
  // canonical casing (e.g. "Combo").
  configId = configId.toLowerCase();
  await ensureOfficialProfilesSeeded();

  const officials = await listWithCurrentRevision(
    and(
      eq(configurationProfiles.scope, "official"),
      eq(configurationProfiles.configId, configId),
      eq(configurationProfiles.archived, false),
      isNull(configurationProfiles.deletedAt),
    ),
  );

  const mine = user
    ? await listWithCurrentRevision(
        and(
          eq(configurationProfiles.scope, "user"),
          eq(configurationProfiles.ownerUserId, user.id),
          eq(configurationProfiles.configId, configId),
          isNull(configurationProfiles.deletedAt),
        ),
      )
    : [];

  return {
    officials,
    mine,
    preferences: user ? await getPreferences(user) : null,
    recommendedId: pickRecommended(officials),
    configSchemaVersion: CONFIG_SCHEMA_VERSION,
  };
}

// All of a user's private profiles across every config page (management view).
export async function listUserProfiles(
  user: User,
): Promise<ProfileSummaryDto[]> {
  return await listWithCurrentRevision(
    and(
      eq(configurationProfiles.scope, "user"),
      eq(configurationProfiles.ownerUserId, user.id),
      isNull(configurationProfiles.deletedAt),
    ),
  );
}

async function getProfileRow(id: string): Promise<ProfileRow | null> {
  const rows = await db
    .select()
    .from(configurationProfiles)
    .where(eq(configurationProfiles.id, id))
    .limit(1);
  return rows[0] ?? null;
}

// Read authorization: official profiles are readable by everyone (archived
// ones stay readable by id so an already-selected profile degrades
// gracefully); private profiles only by their owner. Foreign or deleted
// profiles surface as 404 to avoid leaking existence.
export async function getReadableProfile(
  id: string,
  user: User | null,
): Promise<ProfileRow> {
  const profile = await getProfileRow(id);
  if (!profile || profile.deletedAt) {
    throw error(404, { message: "Profile not found" });
  }
  if (profile.scope === "user" && profile.ownerUserId !== user?.id) {
    throw error(404, { message: "Profile not found" });
  }
  return profile;
}

// Write authorization: owners mutate their private profiles; only admins
// mutate official presets. Scope/owner always come from the stored row.
async function getMutableProfile(id: string, user: User): Promise<ProfileRow> {
  const profile = await getProfileRow(id);
  if (!profile || profile.deletedAt) {
    throw error(404, { message: "Profile not found" });
  }
  if (profile.scope === "official") {
    if (!user.isAdmin) {
      throw error(403, {
        message: "Only administrators can modify official presets",
      });
    }
    return profile;
  }
  if (profile.ownerUserId !== user.id) {
    throw error(404, { message: "Profile not found" });
  }
  return profile;
}

export async function getProfileWithRevision(
  id: string,
  user: User | null,
  revisionId?: string | null,
): Promise<{ profile: ProfileRow; revision: RevisionRow }> {
  const profile = await getReadableProfile(id, user);
  const targetRevisionId = revisionId ?? profile.currentRevisionId;
  if (!targetRevisionId) {
    throw error(404, { message: "Profile has no revision" });
  }
  const rows = await db
    .select()
    .from(configurationProfileRevisions)
    .where(
      and(
        eq(configurationProfileRevisions.id, targetRevisionId),
        eq(configurationProfileRevisions.profileId, profile.id),
      ),
    )
    .limit(1);
  if (!rows[0]) {
    throw error(404, { message: "Profile revision not found" });
  }
  return { profile, revision: rows[0] };
}

async function assertNameAvailable(
  user: User,
  configId: string,
  name: string,
  excludeProfileId?: string,
): Promise<void> {
  const rows = await db
    .select({ id: configurationProfiles.id })
    .from(configurationProfiles)
    .where(
      and(
        eq(configurationProfiles.scope, "user"),
        eq(configurationProfiles.ownerUserId, user.id),
        eq(configurationProfiles.configId, configId),
        isNull(configurationProfiles.deletedAt),
        sql`lower(${configurationProfiles.name}) = ${name.toLowerCase()}`,
      ),
    );
  const conflict = rows.find((row) => row.id !== excludeProfileId);
  if (conflict) {
    throw error(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors: { name: "You already have a profile with this name" },
    });
  }
}

async function isNameTaken(
  user: User,
  configId: string,
  name: string,
): Promise<boolean> {
  try {
    await assertNameAvailable(user, configId, name);
    return false;
  } catch {
    return true;
  }
}

export interface CreateProfileInput {
  configId: string;
  name: string;
  description?: string | null;
  settings: NormalizedConfig;
  configSchemaVersion?: number;
  changeSummary?: string | null;
  scope?: "official" | "user";
  slug?: string | null;
  gameTags?: string[] | null;
  difficultyTag?: string | null;
  isRecommended?: boolean;
  featured?: boolean;
  displayOrder?: number;
}

export async function createProfile(
  user: User,
  input: CreateProfileInput,
): Promise<{ profile: ProfileRow; revision: RevisionRow }> {
  const scope = input.scope === "official" ? "official" : "user";
  // Store config ids lowercase regardless of caller casing.
  input = { ...input, configId: input.configId.toLowerCase() };
  if (scope === "official" && !user.isAdmin) {
    throw error(403, {
      message: "Only administrators can create official presets",
    });
  }

  if (scope === "user") {
    const countRows = await db
      .select({ count: sql<number>`count(*)` })
      .from(configurationProfiles)
      .where(
        and(
          eq(configurationProfiles.scope, "user"),
          eq(configurationProfiles.ownerUserId, user.id),
          isNull(configurationProfiles.deletedAt),
        ),
      );
    if ((countRows[0]?.count ?? 0) >= MAX_USER_PROFILES) {
      throw error(400, {
        message: `You have reached the maximum of ${MAX_USER_PROFILES} saved profiles. Delete one to save another.`,
      });
    }
    await assertNameAvailable(user, input.configId, input.name);
  }

  const now = new Date();
  const profileId = generateId();
  const revisionId = generateId();

  db.transaction((tx) => {
    tx.insert(configurationProfiles)
      .values({
        id: profileId,
        ownerUserId: scope === "user" ? user.id : null,
        scope,
        slug: scope === "official" ? (input.slug ?? null) : null,
        configId: input.configId,
        name: input.name,
        description: input.description ?? null,
        currentRevisionId: null,
        gameTags: input.gameTags ?? null,
        difficultyTag: input.difficultyTag ?? null,
        isRecommended: scope === "official" && !!input.isRecommended,
        featured: scope === "official" && !!input.featured,
        displayOrder: input.displayOrder ?? 0,
        createdAt: now,
        updatedAt: now,
      })
      .run();
    tx.insert(configurationProfileRevisions)
      .values({
        id: revisionId,
        profileId,
        revisionNumber: 1,
        configSchemaVersion: input.configSchemaVersion ?? CONFIG_SCHEMA_VERSION,
        settings: input.settings,
        changeSummary: input.changeSummary ?? null,
        createdBy: user.id,
        createdAt: now,
        publishedAt: scope === "official" ? now : null,
      })
      .run();
    tx.update(configurationProfiles)
      .set({ currentRevisionId: revisionId })
      .where(eq(configurationProfiles.id, profileId))
      .run();
    if (scope === "official" && input.isRecommended) {
      tx.update(configurationProfiles)
        .set({ isRecommended: false })
        .where(
          and(
            eq(configurationProfiles.scope, "official"),
            eq(configurationProfiles.configId, input.configId),
            sql`${configurationProfiles.id} <> ${profileId}`,
          ),
        )
        .run();
    }
  });

  const profile = await getProfileRow(profileId);
  const revision = await db
    .select()
    .from(configurationProfileRevisions)
    .where(eq(configurationProfileRevisions.id, revisionId))
    .limit(1);
  return { profile: profile!, revision: revision[0] };
}

export async function createRevision(
  user: User,
  profileId: string,
  input: {
    settings: NormalizedConfig;
    changeSummary?: string | null;
    baseRevisionId: string | null;
  },
): Promise<{ profile: ProfileRow; revision: RevisionRow }> {
  const profile = await getMutableProfile(profileId, user);

  const now = new Date();
  const revisionId = generateId();

  db.transaction((tx) => {
    // Re-read inside the write transaction. SQLite serializes writers, so two
    // saves based on the same revision cannot both pass this check.
    const current = tx
      .select({
        currentRevisionId: configurationProfiles.currentRevisionId,
      })
      .from(configurationProfiles)
      .where(eq(configurationProfiles.id, profile.id))
      .get();
    if (
      !current ||
      (input.baseRevisionId ?? null) !== (current.currentRevisionId ?? null)
    ) {
      throw error(409, {
        message:
          "This profile was updated elsewhere. Reload it before saving to avoid overwriting the newer version.",
      });
    }

    const maxRow = tx
      .select({
        max: sql<number>`coalesce(max(${configurationProfileRevisions.revisionNumber}), 0)`,
      })
      .from(configurationProfileRevisions)
      .where(eq(configurationProfileRevisions.profileId, profile.id))
      .get();
    const nextRevisionNumber = (maxRow?.max ?? 0) + 1;

    tx.insert(configurationProfileRevisions)
      .values({
        id: revisionId,
        profileId: profile.id,
        revisionNumber: nextRevisionNumber,
        configSchemaVersion: CONFIG_SCHEMA_VERSION,
        settings: input.settings,
        changeSummary: input.changeSummary ?? null,
        createdBy: user.id,
        createdAt: now,
        publishedAt: profile.scope === "official" ? now : null,
      })
      .run();
    tx.update(configurationProfiles)
      .set({ currentRevisionId: revisionId, updatedAt: now })
      .where(eq(configurationProfiles.id, profile.id))
      .run();
  });

  const updated = await getProfileRow(profile.id);
  const revisionRows = await db
    .select()
    .from(configurationProfileRevisions)
    .where(eq(configurationProfileRevisions.id, revisionId))
    .limit(1);
  return { profile: updated!, revision: revisionRows[0] };
}

export interface UpdateProfileMetaInput {
  name?: string;
  description?: string | null;
  archived?: boolean;
  slug?: string;
  gameTags?: string[];
  difficultyTag?: string | null;
  isRecommended?: boolean;
  featured?: boolean;
  displayOrder?: number;
}

export async function updateProfileMeta(
  user: User,
  profileId: string,
  patch: UpdateProfileMetaInput,
): Promise<ProfileRow> {
  const profile = await getMutableProfile(profileId, user);

  const curationKeys: Array<keyof UpdateProfileMetaInput> = [
    "archived",
    "slug",
    "gameTags",
    "difficultyTag",
    "isRecommended",
    "featured",
    "displayOrder",
  ];
  if (
    profile.scope === "user" &&
    curationKeys.some((key) => patch[key] !== undefined)
  ) {
    throw error(400, {
      message: "These fields can only be set on official presets",
    });
  }

  if (patch.name !== undefined && profile.scope === "user") {
    await assertNameAvailable(user, profile.configId, patch.name, profile.id);
  }

  const set: Partial<ProfileRow> = { updatedAt: new Date() };
  if (patch.name !== undefined) set.name = patch.name;
  if (patch.description !== undefined) set.description = patch.description;
  if (profile.scope === "official") {
    if (patch.archived !== undefined) set.archived = patch.archived;
    if (patch.slug !== undefined) set.slug = patch.slug;
    if (patch.gameTags !== undefined) set.gameTags = patch.gameTags;
    if (patch.difficultyTag !== undefined)
      set.difficultyTag = patch.difficultyTag;
    if (patch.isRecommended !== undefined)
      set.isRecommended = patch.isRecommended;
    if (patch.featured !== undefined) set.featured = patch.featured;
    if (patch.displayOrder !== undefined) set.displayOrder = patch.displayOrder;
  }

  db.transaction((tx) => {
    tx.update(configurationProfiles)
      .set(set)
      .where(eq(configurationProfiles.id, profile.id))
      .run();
    // Keep the single-recommended invariant per config page.
    if (profile.scope === "official" && patch.isRecommended) {
      tx.update(configurationProfiles)
        .set({ isRecommended: false })
        .where(
          and(
            eq(configurationProfiles.scope, "official"),
            eq(configurationProfiles.configId, profile.configId),
            sql`${configurationProfiles.id} <> ${profile.id}`,
          ),
        )
        .run();
    }
  });

  return (await getProfileRow(profile.id))!;
}

export async function softDeleteProfile(
  user: User,
  profileId: string,
): Promise<void> {
  const profile = await getMutableProfile(profileId, user);
  const now = new Date();
  db.transaction((tx) => {
    tx.update(configurationProfiles)
      .set({ deletedAt: now, updatedAt: now })
      .where(eq(configurationProfiles.id, profile.id))
      .run();
    tx.update(userProfilePreferences)
      .set({ defaultProfileId: null, updatedAt: now })
      .where(eq(userProfilePreferences.defaultProfileId, profile.id))
      .run();
    tx.update(userProfilePreferences)
      .set({ lastUsedProfileId: null, updatedAt: now })
      .where(eq(userProfilePreferences.lastUsedProfileId, profile.id))
      .run();
    tx.delete(userProfileFavorites)
      .where(eq(userProfileFavorites.profileId, profile.id))
      .run();
  });
}

export async function setPreferences(
  user: User,
  input: {
    defaultProfileId?: string | null;
    lastUsedProfileId?: string | null;
  },
): Promise<ProfilePreferencesDto> {
  for (const id of [input.defaultProfileId, input.lastUsedProfileId]) {
    if (id) {
      // Throws 404 when the profile is not readable by this user.
      await getReadableProfile(id, user);
    }
  }

  const now = new Date();
  const values: typeof userProfilePreferences.$inferInsert = {
    userId: user.id,
    updatedAt: now,
  };
  const set: Partial<typeof userProfilePreferences.$inferInsert> = {
    updatedAt: now,
  };
  if (input.defaultProfileId !== undefined) {
    values.defaultProfileId = input.defaultProfileId;
    set.defaultProfileId = input.defaultProfileId;
  }
  if (input.lastUsedProfileId !== undefined) {
    values.lastUsedProfileId = input.lastUsedProfileId;
    set.lastUsedProfileId = input.lastUsedProfileId;
  }

  await db
    .insert(userProfilePreferences)
    .values(values)
    .onConflictDoUpdate({ target: userProfilePreferences.userId, set });

  return await getPreferences(user);
}

export async function setFavorite(
  user: User,
  profileId: string,
  favorited: boolean,
  displayOrder?: number,
): Promise<void> {
  if (favorited) {
    await getReadableProfile(profileId, user);
    await db
      .insert(userProfileFavorites)
      .values({
        userId: user.id,
        profileId,
        displayOrder: displayOrder ?? 0,
        createdAt: new Date(),
      })
      .onConflictDoUpdate({
        target: [userProfileFavorites.userId, userProfileFavorites.profileId],
        set: { displayOrder: displayOrder ?? 0 },
      });
  } else {
    await db
      .delete(userProfileFavorites)
      .where(
        and(
          eq(userProfileFavorites.userId, user.id),
          eq(userProfileFavorites.profileId, profileId),
        ),
      );
  }
}

// --- Share links ---

// Capability tokens: anyone holding the token may load the profile's current
// revision (read-only). Distinct from ids so links can be revoked.
const generateShareToken = customAlphabet(
  "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
  24,
);

export async function ensureShareToken(
  user: User,
  profileId: string,
): Promise<string> {
  const profile = await getMutableProfile(profileId, user);
  if (profile.shareToken) return profile.shareToken;
  const token = generateShareToken();
  await db
    .update(configurationProfiles)
    .set({ shareToken: token, updatedAt: new Date() })
    .where(eq(configurationProfiles.id, profile.id));
  return token;
}

export async function clearShareToken(
  user: User,
  profileId: string,
): Promise<void> {
  const profile = await getMutableProfile(profileId, user);
  await db
    .update(configurationProfiles)
    .set({ shareToken: null, updatedAt: new Date() })
    .where(eq(configurationProfiles.id, profile.id));
}

// Resolve a share token to the profile's current revision. Returns null for
// unknown, revoked, deleted or revision-less profiles — never throws.
export async function getSharedProfile(
  token: string,
): Promise<{ profile: ProfileRow; revision: RevisionRow } | null> {
  if (!token) return null;
  const rows = await db
    .select()
    .from(configurationProfiles)
    .leftJoin(
      configurationProfileRevisions,
      eq(
        configurationProfiles.currentRevisionId,
        configurationProfileRevisions.id,
      ),
    )
    .where(
      and(
        eq(configurationProfiles.shareToken, token),
        isNull(configurationProfiles.deletedAt),
      ),
    )
    .limit(1);
  const row = rows[0];
  if (!row || !row.configuration_profile_revision) return null;
  return {
    profile: row.configuration_profile,
    revision: row.configuration_profile_revision,
  };
}

// All official presets for the admin panel, including archived ones (but not
// soft-deleted).
export async function listOfficialProfilesForAdmin(): Promise<
  ProfileSummaryDto[]
> {
  return await listWithCurrentRevision(
    and(
      eq(configurationProfiles.scope, "official"),
      isNull(configurationProfiles.deletedAt),
    ),
  );
}

async function assertNewOfficialSlug(rawSlug: string): Promise<string> {
  const slug = rawSlug.trim().toLowerCase();
  if (!/^[a-z0-9-]+$/.test(slug)) {
    throw error(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors: {
        slug: "Slug must be lowercase letters, digits and dashes",
      },
    });
  }
  const existing = await db
    .select({ id: configurationProfiles.id })
    .from(configurationProfiles)
    .where(eq(configurationProfiles.slug, slug))
    .limit(1);
  if (existing[0]) {
    throw error(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors: { slug: "An official preset with this slug already exists" },
    });
  }
  return slug;
}

interface PromoteInput {
  slug: string;
  name?: string;
  description?: string | null;
}

function promoteCopy(
  user: User,
  source: ProfileRow,
  revision: RevisionRow,
  input: PromoteInput,
  slug: string,
): Promise<{ profile: ProfileRow; revision: RevisionRow }> {
  return createProfile(user, {
    configId: source.configId,
    name: input.name?.trim() || source.name,
    description:
      input.description !== undefined ? input.description : source.description,
    settings: revision.settings as NormalizedConfig,
    configSchemaVersion: revision.configSchemaVersion,
    changeSummary: `Promoted from ${source.name}`,
    scope: "official",
    slug,
  });
}

// Promote a readable profile into a new official preset: the current revision
// is copied into a brand-new official profile; the source is untouched.
export async function promoteProfileToOfficial(
  user: User,
  profileId: string,
  input: PromoteInput,
): Promise<{ profile: ProfileRow; revision: RevisionRow }> {
  if (!user.isAdmin) {
    throw error(403, {
      message: "Only administrators can create official presets",
    });
  }
  const { profile: source, revision } = await getProfileWithRevision(
    profileId,
    user,
  );
  const slug = await assertNewOfficialSlug(input.slug);
  return await promoteCopy(user, source, revision, input, slug);
}

// Promote through a share link: lets admins turn any user's shared profile
// into an official preset without ever gaining access to the profile itself.
export async function promoteSharedProfileToOfficial(
  user: User,
  token: string,
  input: PromoteInput,
): Promise<{ profile: ProfileRow; revision: RevisionRow }> {
  if (!user.isAdmin) {
    throw error(403, {
      message: "Only administrators can create official presets",
    });
  }
  const shared = await getSharedProfile(token);
  if (!shared) {
    throw error(404, { message: "This share link is no longer valid" });
  }
  const slug = await assertNewOfficialSlug(input.slug);
  return await promoteCopy(user, shared.profile, shared.revision, input, slug);
}

// What the seed page may say about the profile a seed was generated from.
// `profileId` is only set when the viewer is allowed to open the profile on
// the config page (officials for everyone, private profiles for their owner).
export interface SeedAttributionDto {
  profileId: string | null;
  slug: string | null;
  name: string;
  scope: "official" | "user";
  configId: string;
  revisionNumber: number | null;
  deleted: boolean;
}

// Resolve seed attribution for display. Never throws: seeds outlive profiles
// (attribution columns have no FKs), so unknown profiles resolve to null and
// deleted ones keep their name but lose the link. Private profiles are only
// revealed to their owner — everyone else sees no attribution at all.
export async function getSeedAttribution(
  profileId: string,
  revisionId: string | null,
  user: User | null,
): Promise<SeedAttributionDto | null> {
  const profile = await getProfileRow(profileId);
  if (!profile) return null;
  if (profile.scope === "user" && profile.ownerUserId !== user?.id) {
    return null;
  }

  let revisionNumber: number | null = null;
  if (revisionId) {
    const rows = await db
      .select({
        revisionNumber: configurationProfileRevisions.revisionNumber,
      })
      .from(configurationProfileRevisions)
      .where(
        and(
          eq(configurationProfileRevisions.id, revisionId),
          eq(configurationProfileRevisions.profileId, profile.id),
        ),
      )
      .limit(1);
    revisionNumber = rows[0]?.revisionNumber ?? null;
  }

  const deleted = !!profile.deletedAt;
  return {
    profileId: deleted ? null : profile.id,
    slug: profile.scope === "official" ? profile.slug : null,
    name: profile.name,
    scope: profile.scope,
    configId: profile.configId,
    revisionNumber,
    deleted,
  };
}

// Copy a readable profile (typically an official preset) into an independent
// private profile. The settings snapshot is copied verbatim, including its
// schema version — no live inheritance.
export async function duplicateProfile(
  user: User,
  profileId: string,
  requestedName?: string,
): Promise<{ profile: ProfileRow; revision: RevisionRow }> {
  const { profile: source, revision } = await getProfileWithRevision(
    profileId,
    user,
  );

  let name = requestedName ?? source.name;
  if (await isNameTaken(user, source.configId, name)) {
    const base = name;
    let candidate = `${base} (copy)`;
    for (
      let i = 2;
      (await isNameTaken(user, source.configId, candidate)) && i < 20;
      i++
    ) {
      candidate = `${base} (copy ${i})`;
    }
    name = candidate;
  }

  return await createProfile(user, {
    configId: source.configId,
    name,
    description: source.description,
    settings: revision.settings as NormalizedConfig,
    configSchemaVersion: revision.configSchemaVersion,
    changeSummary: `Copied from ${source.name}`,
    scope: "user",
  });
}
