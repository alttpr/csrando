import { and, desc, eq, gte, isNull, like, sql } from "drizzle-orm";
import { db } from "$lib/server/db";
import {
  apiKeys,
  configurationPresets,
  randomizerVersions,
  seeds,
  users,
} from "$lib/server/db/schema";
import { shouldHideSpoiler } from "$lib/server/seed-visibility";

export interface SiteStats {
  users: number;
  admins: number;
  seedsTotal: number;
  seedsLast24h: number;
  seedsLast7d: number;
  seedsLast30d: number;
  userPresets: number;
  officialPresets: number;
  archivedOfficialPresets: number;
  activeApiKeys: number;
  versions: number;
  activeVersionTags: string[];
}

export async function getSiteStats(): Promise<SiteStats> {
  const now = Date.now();
  const dayAgo = new Date(now - 24 * 60 * 60 * 1000);
  const weekAgo = new Date(now - 7 * 24 * 60 * 60 * 1000);
  const monthAgo = new Date(now - 30 * 24 * 60 * 60 * 1000);

  const count = sql<number>`count(*)`;

  const [
    userRows,
    adminRows,
    seedRows,
    seed24Rows,
    seed7Rows,
    seed30Rows,
    userPresetRows,
    officialRows,
    archivedOfficialRows,
    apiKeyRows,
    versionRows,
    activeVersions,
  ] = await Promise.all([
    db.select({ count }).from(users),
    db.select({ count }).from(users).where(eq(users.isAdmin, true)),
    db.select({ count }).from(seeds),
    db.select({ count }).from(seeds).where(gte(seeds.createdAt, dayAgo)),
    db.select({ count }).from(seeds).where(gte(seeds.createdAt, weekAgo)),
    db.select({ count }).from(seeds).where(gte(seeds.createdAt, monthAgo)),
    db
      .select({ count })
      .from(configurationPresets)
      .where(
        and(
          eq(configurationPresets.scope, "user"),
          isNull(configurationPresets.deletedAt),
        ),
      ),
    db
      .select({ count })
      .from(configurationPresets)
      .where(
        and(
          eq(configurationPresets.scope, "official"),
          isNull(configurationPresets.deletedAt),
          eq(configurationPresets.archived, false),
        ),
      ),
    db
      .select({ count })
      .from(configurationPresets)
      .where(
        and(
          eq(configurationPresets.scope, "official"),
          isNull(configurationPresets.deletedAt),
          eq(configurationPresets.archived, true),
        ),
      ),
    db.select({ count }).from(apiKeys).where(isNull(apiKeys.revokedAt)),
    db.select({ count }).from(randomizerVersions),
    db
      .select({ versionTag: randomizerVersions.versionTag })
      .from(randomizerVersions)
      .where(eq(randomizerVersions.isActive, true))
      .orderBy(randomizerVersions.versionTag),
  ]);

  return {
    users: userRows[0]?.count ?? 0,
    admins: adminRows[0]?.count ?? 0,
    seedsTotal: seedRows[0]?.count ?? 0,
    seedsLast24h: seed24Rows[0]?.count ?? 0,
    seedsLast7d: seed7Rows[0]?.count ?? 0,
    seedsLast30d: seed30Rows[0]?.count ?? 0,
    userPresets: userPresetRows[0]?.count ?? 0,
    officialPresets: officialRows[0]?.count ?? 0,
    archivedOfficialPresets: archivedOfficialRows[0]?.count ?? 0,
    activeApiKeys: apiKeyRows[0]?.count ?? 0,
    versions: versionRows[0]?.count ?? 0,
    activeVersionTags: activeVersions.map((v) => v.versionTag),
  };
}

export interface AdminUserRow {
  id: string;
  username: string;
  isAdmin: boolean;
  hasGithub: boolean;
}

export async function listUsersForAdmin(query?: string): Promise<{
  users: AdminUserRow[];
  total: number;
}> {
  const q = (query ?? "").trim().toLowerCase();
  const where = q ? sql`lower(${users.username}) LIKE ${`%${q}%`}` : undefined;
  const [rows, totalRows] = await Promise.all([
    db
      .select({
        id: users.id,
        username: users.username,
        isAdmin: users.isAdmin,
        githubId: users.githubId,
      })
      .from(users)
      .where(where)
      .orderBy(users.username)
      .limit(200),
    db
      .select({ count: sql<number>`count(*)` })
      .from(users)
      .where(where),
  ]);
  return {
    users: rows.map((row) => ({
      id: row.id,
      username: row.username,
      isAdmin: Boolean(row.isAdmin),
      hasGithub: row.githubId !== null,
    })),
    total: totalRows[0]?.count ?? 0,
  };
}

export interface AdminVersionRow {
  id: string;
  versionTag: string;
  randomizerId: string | null;
  isActive: boolean;
  createdAt: string | null;
  buildDate: string | null;
  gitCommitHash: string | null;
}

export async function listVersionsForAdmin(): Promise<AdminVersionRow[]> {
  const rows = await db
    .select({
      id: randomizerVersions.id,
      versionTag: randomizerVersions.versionTag,
      randomizerId: randomizerVersions.randomizerId,
      isActive: randomizerVersions.isActive,
      createdAt: randomizerVersions.createdAt,
      buildDate: randomizerVersions.buildDate,
      gitCommitHash: randomizerVersions.gitCommitHash,
    })
    .from(randomizerVersions)
    .orderBy(desc(randomizerVersions.createdAt));
  return rows.map((row) => ({
    id: row.id,
    versionTag: row.versionTag,
    randomizerId: row.randomizerId,
    isActive: Boolean(row.isActive),
    createdAt: toIso(row.createdAt),
    buildDate: toIso(row.buildDate),
    gitCommitHash: row.gitCommitHash ?? null,
  }));
}

// Make a version the active one for its randomizer id (only one active per
// randomizer; new seeds use the active version).
export async function setActiveVersion(versionId: string): Promise<boolean> {
  const rows = await db
    .select({
      id: randomizerVersions.id,
      randomizerId: randomizerVersions.randomizerId,
    })
    .from(randomizerVersions)
    .where(eq(randomizerVersions.id, versionId))
    .limit(1);
  const target = rows[0];
  if (!target) return false;

  if (target.randomizerId) {
    await db
      .update(randomizerVersions)
      .set({ isActive: false })
      .where(eq(randomizerVersions.randomizerId, target.randomizerId));
  } else {
    await db
      .update(randomizerVersions)
      .set({ isActive: false })
      .where(sql`randomizer_id IS NULL`);
  }
  await db
    .update(randomizerVersions)
    .set({ isActive: true })
    .where(eq(randomizerVersions.id, versionId));
  return true;
}

export async function deactivateVersion(versionId: string): Promise<boolean> {
  const rows = await db
    .select({ id: randomizerVersions.id })
    .from(randomizerVersions)
    .where(eq(randomizerVersions.id, versionId))
    .limit(1);
  if (!rows[0]) return false;
  await db
    .update(randomizerVersions)
    .set({ isActive: false })
    .where(eq(randomizerVersions.id, versionId));
  return true;
}

export interface AdminSeedRow {
  id: string;
  createdAt: string | null;
  versionTag: string | null;
  raceMode: boolean;
  presetId: string | null;
  differedFromRevision: boolean | null;
}

export const ADMIN_SEEDS_PAGE_SIZE = 25;

function toIso(value: unknown): string | null {
  if (value instanceof Date) {
    return Number.isNaN(value.getTime()) ? null : value.toISOString();
  }
  if (typeof value === "string" || typeof value === "number") {
    const d = new Date(value);
    return Number.isNaN(d.getTime()) ? null : d.toISOString();
  }
  return null;
}

// Paged, newest-first seed listing with substring search on the seed id —
// enough for "a user lost their permalink" lookups.
export async function listSeedsForAdmin(options: {
  query?: string;
  page?: number;
}): Promise<{ seeds: AdminSeedRow[]; total: number; page: number }> {
  const q = (options.query ?? "").trim();
  const page = Math.max(0, options.page ?? 0);
  const where = q ? like(seeds.id, `%${q}%`) : undefined;

  const [rows, totalRows] = await Promise.all([
    db
      .select({
        id: seeds.id,
        createdAt: seeds.createdAt,
        options: seeds.options,
        presetId: seeds.presetId,
        differedFromRevision: seeds.differedFromRevision,
        versionTag: randomizerVersions.versionTag,
      })
      .from(seeds)
      .leftJoin(
        randomizerVersions,
        eq(seeds.randomizerVersionId, randomizerVersions.id),
      )
      .where(where)
      .orderBy(desc(seeds.createdAt))
      .limit(ADMIN_SEEDS_PAGE_SIZE)
      .offset(page * ADMIN_SEEDS_PAGE_SIZE),
    db
      .select({ count: sql<number>`count(*)` })
      .from(seeds)
      .where(where),
  ]);

  return {
    seeds: rows.map((row) => ({
      id: row.id,
      createdAt: toIso(row.createdAt),
      versionTag: row.versionTag ?? null,
      raceMode: shouldHideSpoiler(row.options),
      presetId: row.presetId,
      differedFromRevision:
        row.differedFromRevision === null
          ? null
          : Boolean(row.differedFromRevision),
    })),
    total: totalRows[0]?.count ?? 0,
    page,
  };
}
