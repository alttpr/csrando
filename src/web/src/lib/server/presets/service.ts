import { error } from "@sveltejs/kit";
import { and, asc, eq, isNull, sql } from "drizzle-orm";
import { customAlphabet } from "nanoid";
import type { User } from "lucia";
import { db } from "$lib/server/db";
import {
  configurationPresets,
  configurationPresetRevisions,
  userPresetFavorites,
  userPresetPreferences,
} from "$lib/server/db/schema";
import { generateId } from "$lib/utils/id";
import { CONFIG_SCHEMA_VERSION, MAX_USER_PRESETS } from "$lib/config/constants";
import type { NormalizedConfig } from "$lib/config/normalize";
import type {
  PresetListResponseDto,
  PresetPreferencesDto,
  PresetRevisionDto,
  PresetSummaryDto,
} from "$lib/schemas/presets";
import { ensureOfficialPresetsSeeded } from "./seed-official";

type PresetRow = typeof configurationPresets.$inferSelect;
type RevisionRow = typeof configurationPresetRevisions.$inferSelect;

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

export function toPresetSummary(
  preset: PresetRow,
  revision?: RevisionRow | null,
): PresetSummaryDto {
  return {
    id: preset.id,
    scope: preset.scope,
    slug: preset.slug,
    configId: preset.configId,
    name: preset.name,
    description: preset.description,
    currentRevisionId: preset.currentRevisionId,
    revisionNumber: revision?.revisionNumber ?? null,
    configSchemaVersion: revision?.configSchemaVersion ?? null,
    selectedGames: selectedGamesOf(revision),
    gameTags: Array.isArray(preset.gameTags)
      ? (preset.gameTags as string[])
      : null,
    difficultyTag: preset.difficultyTag,
    isRecommended: preset.isRecommended,
    featured: preset.featured,
    archived: preset.archived,
    displayOrder: preset.displayOrder,
    createdAt: toIso(preset.createdAt),
    updatedAt: toIso(preset.updatedAt),
  };
}

export function toRevisionDto(revision: RevisionRow): PresetRevisionDto {
  return {
    id: revision.id,
    presetId: revision.presetId,
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
    .from(configurationPresets)
    .leftJoin(
      configurationPresetRevisions,
      eq(
        configurationPresets.currentRevisionId,
        configurationPresetRevisions.id,
      ),
    )
    .where(where)
    .orderBy(
      asc(configurationPresets.displayOrder),
      asc(configurationPresets.name),
    );
  return rows.map((row) =>
    toPresetSummary(
      row.configuration_preset,
      row.configuration_preset_revision,
    ),
  );
}

export function pickRecommended(officials: PresetSummaryDto[]): string | null {
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
): Promise<PresetPreferencesDto> {
  const prefRows = await db
    .select()
    .from(userPresetPreferences)
    .where(eq(userPresetPreferences.userId, user.id))
    .limit(1);
  const favorites = await db
    .select({
      presetId: userPresetFavorites.presetId,
      displayOrder: userPresetFavorites.displayOrder,
    })
    .from(userPresetFavorites)
    .where(eq(userPresetFavorites.userId, user.id))
    .orderBy(asc(userPresetFavorites.displayOrder));
  return {
    defaultPresetId: prefRows[0]?.defaultPresetId ?? null,
    lastUsedPresetId: prefRows[0]?.lastUsedPresetId ?? null,
    favorites,
  };
}

export async function listPresetsFor(
  configId: string,
  user: User | null,
): Promise<PresetListResponseDto> {
  // Config ids are stored lowercase; callers may pass the backend's
  // canonical casing (e.g. "Combo").
  configId = configId.toLowerCase();
  await ensureOfficialPresetsSeeded();

  const officials = await listWithCurrentRevision(
    and(
      eq(configurationPresets.scope, "official"),
      eq(configurationPresets.configId, configId),
      eq(configurationPresets.archived, false),
      isNull(configurationPresets.deletedAt),
    ),
  );

  const mine = user
    ? await listWithCurrentRevision(
        and(
          eq(configurationPresets.scope, "user"),
          eq(configurationPresets.ownerUserId, user.id),
          eq(configurationPresets.configId, configId),
          isNull(configurationPresets.deletedAt),
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

// All of a user's private presets across every config page (management view).
export async function listUserPresets(user: User): Promise<PresetSummaryDto[]> {
  return await listWithCurrentRevision(
    and(
      eq(configurationPresets.scope, "user"),
      eq(configurationPresets.ownerUserId, user.id),
      isNull(configurationPresets.deletedAt),
    ),
  );
}

async function getPresetRow(id: string): Promise<PresetRow | null> {
  const rows = await db
    .select()
    .from(configurationPresets)
    .where(eq(configurationPresets.id, id))
    .limit(1);
  return rows[0] ?? null;
}

// Read authorization: official presets are readable by everyone (archived
// ones stay readable by id so an already-selected preset degrades
// gracefully); private presets only by their owner. Foreign or deleted
// presets surface as 404 to avoid leaking existence.
export async function getReadablePreset(
  id: string,
  user: User | null,
): Promise<PresetRow> {
  const preset = await getPresetRow(id);
  if (!preset || preset.deletedAt) {
    throw error(404, { message: "Preset not found" });
  }
  if (preset.scope === "user" && preset.ownerUserId !== user?.id) {
    throw error(404, { message: "Preset not found" });
  }
  return preset;
}

// Write authorization: owners mutate their private presets; only admins
// mutate official presets. Scope/owner always come from the stored row.
async function getMutablePreset(id: string, user: User): Promise<PresetRow> {
  const preset = await getPresetRow(id);
  if (!preset || preset.deletedAt) {
    throw error(404, { message: "Preset not found" });
  }
  if (preset.scope === "official") {
    if (!user.isAdmin) {
      throw error(403, {
        message: "Only administrators can modify official presets",
      });
    }
    return preset;
  }
  if (preset.ownerUserId !== user.id) {
    throw error(404, { message: "Preset not found" });
  }
  return preset;
}

export async function getPresetWithRevision(
  id: string,
  user: User | null,
  revisionId?: string | null,
): Promise<{ preset: PresetRow; revision: RevisionRow }> {
  const preset = await getReadablePreset(id, user);
  const targetRevisionId = revisionId ?? preset.currentRevisionId;
  if (!targetRevisionId) {
    throw error(404, { message: "Preset has no revision" });
  }
  const rows = await db
    .select()
    .from(configurationPresetRevisions)
    .where(
      and(
        eq(configurationPresetRevisions.id, targetRevisionId),
        eq(configurationPresetRevisions.presetId, preset.id),
      ),
    )
    .limit(1);
  if (!rows[0]) {
    throw error(404, { message: "Preset revision not found" });
  }
  return { preset, revision: rows[0] };
}

async function assertNameAvailable(
  user: User,
  configId: string,
  name: string,
  excludePresetId?: string,
): Promise<void> {
  const rows = await db
    .select({ id: configurationPresets.id })
    .from(configurationPresets)
    .where(
      and(
        eq(configurationPresets.scope, "user"),
        eq(configurationPresets.ownerUserId, user.id),
        eq(configurationPresets.configId, configId),
        isNull(configurationPresets.deletedAt),
        sql`lower(${configurationPresets.name}) = ${name.toLowerCase()}`,
      ),
    );
  const conflict = rows.find((row) => row.id !== excludePresetId);
  if (conflict) {
    throw error(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors: { name: "You already have a preset with this name" },
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

export interface CreatePresetInput {
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

export async function createPreset(
  user: User,
  input: CreatePresetInput,
): Promise<{ preset: PresetRow; revision: RevisionRow }> {
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
      .from(configurationPresets)
      .where(
        and(
          eq(configurationPresets.scope, "user"),
          eq(configurationPresets.ownerUserId, user.id),
          isNull(configurationPresets.deletedAt),
        ),
      );
    if ((countRows[0]?.count ?? 0) >= MAX_USER_PRESETS) {
      throw error(400, {
        message: `You have reached the maximum of ${MAX_USER_PRESETS} saved presets. Delete one to save another.`,
      });
    }
    await assertNameAvailable(user, input.configId, input.name);
  }

  const now = new Date();
  const presetId = generateId();
  const revisionId = generateId();

  db.transaction((tx) => {
    tx.insert(configurationPresets)
      .values({
        id: presetId,
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
    tx.insert(configurationPresetRevisions)
      .values({
        id: revisionId,
        presetId,
        revisionNumber: 1,
        configSchemaVersion: input.configSchemaVersion ?? CONFIG_SCHEMA_VERSION,
        settings: input.settings,
        changeSummary: input.changeSummary ?? null,
        createdBy: user.id,
        createdAt: now,
        publishedAt: scope === "official" ? now : null,
      })
      .run();
    tx.update(configurationPresets)
      .set({ currentRevisionId: revisionId })
      .where(eq(configurationPresets.id, presetId))
      .run();
    if (scope === "official" && input.isRecommended) {
      tx.update(configurationPresets)
        .set({ isRecommended: false })
        .where(
          and(
            eq(configurationPresets.scope, "official"),
            eq(configurationPresets.configId, input.configId),
            sql`${configurationPresets.id} <> ${presetId}`,
          ),
        )
        .run();
    }
  });

  const preset = await getPresetRow(presetId);
  const revision = await db
    .select()
    .from(configurationPresetRevisions)
    .where(eq(configurationPresetRevisions.id, revisionId))
    .limit(1);
  return { preset: preset!, revision: revision[0] };
}

export async function createRevision(
  user: User,
  presetId: string,
  input: {
    settings: NormalizedConfig;
    changeSummary?: string | null;
    baseRevisionId: string | null;
  },
): Promise<{ preset: PresetRow; revision: RevisionRow }> {
  const preset = await getMutablePreset(presetId, user);

  const now = new Date();
  const revisionId = generateId();

  db.transaction((tx) => {
    // Re-read inside the write transaction. SQLite serializes writers, so two
    // saves based on the same revision cannot both pass this check.
    const current = tx
      .select({
        currentRevisionId: configurationPresets.currentRevisionId,
      })
      .from(configurationPresets)
      .where(eq(configurationPresets.id, preset.id))
      .get();
    if (
      !current ||
      (input.baseRevisionId ?? null) !== (current.currentRevisionId ?? null)
    ) {
      throw error(409, {
        message:
          "This preset was updated elsewhere. Reload it before saving to avoid overwriting the newer version.",
      });
    }

    const maxRow = tx
      .select({
        max: sql<number>`coalesce(max(${configurationPresetRevisions.revisionNumber}), 0)`,
      })
      .from(configurationPresetRevisions)
      .where(eq(configurationPresetRevisions.presetId, preset.id))
      .get();
    const nextRevisionNumber = (maxRow?.max ?? 0) + 1;

    tx.insert(configurationPresetRevisions)
      .values({
        id: revisionId,
        presetId: preset.id,
        revisionNumber: nextRevisionNumber,
        configSchemaVersion: CONFIG_SCHEMA_VERSION,
        settings: input.settings,
        changeSummary: input.changeSummary ?? null,
        createdBy: user.id,
        createdAt: now,
        publishedAt: preset.scope === "official" ? now : null,
      })
      .run();
    tx.update(configurationPresets)
      .set({ currentRevisionId: revisionId, updatedAt: now })
      .where(eq(configurationPresets.id, preset.id))
      .run();
  });

  const updated = await getPresetRow(preset.id);
  const revisionRows = await db
    .select()
    .from(configurationPresetRevisions)
    .where(eq(configurationPresetRevisions.id, revisionId))
    .limit(1);
  return { preset: updated!, revision: revisionRows[0] };
}

export interface UpdatePresetMetaInput {
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

export async function updatePresetMeta(
  user: User,
  presetId: string,
  patch: UpdatePresetMetaInput,
): Promise<PresetRow> {
  const preset = await getMutablePreset(presetId, user);

  const curationKeys: Array<keyof UpdatePresetMetaInput> = [
    "archived",
    "slug",
    "gameTags",
    "difficultyTag",
    "isRecommended",
    "featured",
    "displayOrder",
  ];
  if (
    preset.scope === "user" &&
    curationKeys.some((key) => patch[key] !== undefined)
  ) {
    throw error(400, {
      message: "These fields can only be set on official presets",
    });
  }

  if (patch.name !== undefined && preset.scope === "user") {
    await assertNameAvailable(user, preset.configId, patch.name, preset.id);
  }

  const set: Partial<PresetRow> = { updatedAt: new Date() };
  if (patch.name !== undefined) set.name = patch.name;
  if (patch.description !== undefined) set.description = patch.description;
  if (preset.scope === "official") {
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
    tx.update(configurationPresets)
      .set(set)
      .where(eq(configurationPresets.id, preset.id))
      .run();
    // Keep the single-recommended invariant per config page.
    if (preset.scope === "official" && patch.isRecommended) {
      tx.update(configurationPresets)
        .set({ isRecommended: false })
        .where(
          and(
            eq(configurationPresets.scope, "official"),
            eq(configurationPresets.configId, preset.configId),
            sql`${configurationPresets.id} <> ${preset.id}`,
          ),
        )
        .run();
    }
  });

  return (await getPresetRow(preset.id))!;
}

export async function softDeletePreset(
  user: User,
  presetId: string,
): Promise<void> {
  const preset = await getMutablePreset(presetId, user);
  const now = new Date();
  db.transaction((tx) => {
    tx.update(configurationPresets)
      .set({ deletedAt: now, updatedAt: now })
      .where(eq(configurationPresets.id, preset.id))
      .run();
    tx.update(userPresetPreferences)
      .set({ defaultPresetId: null, updatedAt: now })
      .where(eq(userPresetPreferences.defaultPresetId, preset.id))
      .run();
    tx.update(userPresetPreferences)
      .set({ lastUsedPresetId: null, updatedAt: now })
      .where(eq(userPresetPreferences.lastUsedPresetId, preset.id))
      .run();
    tx.delete(userPresetFavorites)
      .where(eq(userPresetFavorites.presetId, preset.id))
      .run();
  });
}

export async function setPreferences(
  user: User,
  input: {
    defaultPresetId?: string | null;
    lastUsedPresetId?: string | null;
  },
): Promise<PresetPreferencesDto> {
  for (const id of [input.defaultPresetId, input.lastUsedPresetId]) {
    if (id) {
      // Throws 404 when the preset is not readable by this user.
      await getReadablePreset(id, user);
    }
  }

  const now = new Date();
  const values: typeof userPresetPreferences.$inferInsert = {
    userId: user.id,
    updatedAt: now,
  };
  const set: Partial<typeof userPresetPreferences.$inferInsert> = {
    updatedAt: now,
  };
  if (input.defaultPresetId !== undefined) {
    values.defaultPresetId = input.defaultPresetId;
    set.defaultPresetId = input.defaultPresetId;
  }
  if (input.lastUsedPresetId !== undefined) {
    values.lastUsedPresetId = input.lastUsedPresetId;
    set.lastUsedPresetId = input.lastUsedPresetId;
  }

  await db
    .insert(userPresetPreferences)
    .values(values)
    .onConflictDoUpdate({ target: userPresetPreferences.userId, set });

  return await getPreferences(user);
}

export async function setFavorite(
  user: User,
  presetId: string,
  favorited: boolean,
  displayOrder?: number,
): Promise<void> {
  if (favorited) {
    await getReadablePreset(presetId, user);
    await db
      .insert(userPresetFavorites)
      .values({
        userId: user.id,
        presetId,
        displayOrder: displayOrder ?? 0,
        createdAt: new Date(),
      })
      .onConflictDoUpdate({
        target: [userPresetFavorites.userId, userPresetFavorites.presetId],
        set: { displayOrder: displayOrder ?? 0 },
      });
  } else {
    await db
      .delete(userPresetFavorites)
      .where(
        and(
          eq(userPresetFavorites.userId, user.id),
          eq(userPresetFavorites.presetId, presetId),
        ),
      );
  }
}

// --- Share links ---

// Capability tokens: anyone holding the token may load the preset's current
// revision (read-only). Distinct from ids so links can be revoked.
const generateShareToken = customAlphabet(
  "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
  24,
);

export async function ensureShareToken(
  user: User,
  presetId: string,
): Promise<string> {
  const preset = await getMutablePreset(presetId, user);
  if (preset.shareToken) return preset.shareToken;
  const token = generateShareToken();
  await db
    .update(configurationPresets)
    .set({ shareToken: token, updatedAt: new Date() })
    .where(eq(configurationPresets.id, preset.id));
  return token;
}

export async function clearShareToken(
  user: User,
  presetId: string,
): Promise<void> {
  const preset = await getMutablePreset(presetId, user);
  await db
    .update(configurationPresets)
    .set({ shareToken: null, updatedAt: new Date() })
    .where(eq(configurationPresets.id, preset.id));
}

// Resolve a share token to the preset's current revision. Returns null for
// unknown, revoked, deleted or revision-less presets — never throws.
export async function getSharedPreset(
  token: string,
): Promise<{ preset: PresetRow; revision: RevisionRow } | null> {
  if (!token) return null;
  const rows = await db
    .select()
    .from(configurationPresets)
    .leftJoin(
      configurationPresetRevisions,
      eq(
        configurationPresets.currentRevisionId,
        configurationPresetRevisions.id,
      ),
    )
    .where(
      and(
        eq(configurationPresets.shareToken, token),
        isNull(configurationPresets.deletedAt),
      ),
    )
    .limit(1);
  const row = rows[0];
  if (!row || !row.configuration_preset_revision) return null;
  return {
    preset: row.configuration_preset,
    revision: row.configuration_preset_revision,
  };
}

// All official presets for the admin panel, including archived ones (but not
// soft-deleted).
export async function listOfficialPresetsForAdmin(): Promise<
  PresetSummaryDto[]
> {
  return await listWithCurrentRevision(
    and(
      eq(configurationPresets.scope, "official"),
      isNull(configurationPresets.deletedAt),
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
    .select({ id: configurationPresets.id })
    .from(configurationPresets)
    .where(eq(configurationPresets.slug, slug))
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
  source: PresetRow,
  revision: RevisionRow,
  input: PromoteInput,
  slug: string,
): Promise<{ preset: PresetRow; revision: RevisionRow }> {
  return createPreset(user, {
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

// Promote a readable preset into a new official preset: the current revision
// is copied into a brand-new official preset; the source is untouched.
export async function promotePresetToOfficial(
  user: User,
  presetId: string,
  input: PromoteInput,
): Promise<{ preset: PresetRow; revision: RevisionRow }> {
  if (!user.isAdmin) {
    throw error(403, {
      message: "Only administrators can create official presets",
    });
  }
  const { preset: source, revision } = await getPresetWithRevision(
    presetId,
    user,
  );
  const slug = await assertNewOfficialSlug(input.slug);
  return await promoteCopy(user, source, revision, input, slug);
}

// Promote through a share link: lets admins turn any user's shared preset
// into an official preset without ever gaining access to the preset itself.
export async function promoteSharedPresetToOfficial(
  user: User,
  token: string,
  input: PromoteInput,
): Promise<{ preset: PresetRow; revision: RevisionRow }> {
  if (!user.isAdmin) {
    throw error(403, {
      message: "Only administrators can create official presets",
    });
  }
  const shared = await getSharedPreset(token);
  if (!shared) {
    throw error(404, { message: "This share link is no longer valid" });
  }
  const slug = await assertNewOfficialSlug(input.slug);
  return await promoteCopy(user, shared.preset, shared.revision, input, slug);
}

// What the seed page may say about the preset a seed was generated from.
// `presetId` is only set when the viewer is allowed to open the preset on
// the config page (officials for everyone, private presets for their owner).
export interface SeedAttributionDto {
  presetId: string | null;
  slug: string | null;
  name: string;
  scope: "official" | "user";
  configId: string;
  revisionNumber: number | null;
  deleted: boolean;
}

// Resolve seed attribution for display. Never throws: seeds outlive presets
// (attribution columns have no FKs), so unknown presets resolve to null and
// deleted ones keep their name but lose the link. Private presets are only
// revealed to their owner — everyone else sees no attribution at all.
export async function getSeedAttribution(
  presetId: string,
  revisionId: string | null,
  user: User | null,
): Promise<SeedAttributionDto | null> {
  const preset = await getPresetRow(presetId);
  if (!preset) return null;
  if (preset.scope === "user" && preset.ownerUserId !== user?.id) {
    return null;
  }

  let revisionNumber: number | null = null;
  if (revisionId) {
    const rows = await db
      .select({
        revisionNumber: configurationPresetRevisions.revisionNumber,
      })
      .from(configurationPresetRevisions)
      .where(
        and(
          eq(configurationPresetRevisions.id, revisionId),
          eq(configurationPresetRevisions.presetId, preset.id),
        ),
      )
      .limit(1);
    revisionNumber = rows[0]?.revisionNumber ?? null;
  }

  const deleted = !!preset.deletedAt;
  return {
    presetId: deleted ? null : preset.id,
    slug: preset.scope === "official" ? preset.slug : null,
    name: preset.name,
    scope: preset.scope,
    configId: preset.configId,
    revisionNumber,
    deleted,
  };
}

// Copy a readable preset (typically an official preset) into an independent
// private preset. The settings snapshot is copied verbatim, including its
// schema version — no live inheritance.
export async function duplicatePreset(
  user: User,
  presetId: string,
  requestedName?: string,
): Promise<{ preset: PresetRow; revision: RevisionRow }> {
  const { preset: source, revision } = await getPresetWithRevision(
    presetId,
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

  return await createPreset(user, {
    configId: source.configId,
    name,
    description: source.description,
    settings: revision.settings as NormalizedConfig,
    configSchemaVersion: revision.configSchemaVersion,
    changeSummary: `Copied from ${source.name}`,
    scope: "user",
  });
}
