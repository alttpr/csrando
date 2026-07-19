import type { PageServerLoad } from "./$types";
import { desc, eq, sql } from "drizzle-orm";
import { db } from "$lib/server/db";
import { seeds, userSeeds } from "$lib/server/db/schema";

const PAGE_SIZE = 20;

function toIso(value: unknown): string | null {
  if (value instanceof Date) return value.toISOString();
  if (typeof value === "string" || typeof value === "number") {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date.toISOString();
  }
  return null;
}

export const load: PageServerLoad = async ({ locals, url }) => {
  const page = Math.max(0, Number(url.searchParams.get("page") ?? "0") || 0);
  const userId = locals.user!.id;
  const [rows, countRows] = await Promise.all([
    db
      .select({
        id: seeds.id,
        options: seeds.options,
        createdAt: seeds.createdAt,
        presetId: seeds.presetId,
        differedFromRevision: seeds.differedFromRevision,
      })
      .from(userSeeds)
      .innerJoin(seeds, eq(userSeeds.seedId, seeds.id))
      .where(eq(userSeeds.userId, userId))
      .orderBy(desc(userSeeds.createdAt))
      .limit(PAGE_SIZE)
      .offset(page * PAGE_SIZE),
    db
      .select({ count: sql<number>`count(*)` })
      .from(userSeeds)
      .where(eq(userSeeds.userId, userId)),
  ]);

  return {
    seeds: rows.map((seed) => ({
      ...seed,
      createdAt: toIso(seed.createdAt),
      differedFromRevision:
        seed.differedFromRevision === null
          ? null
          : Boolean(seed.differedFromRevision),
    })),
    page,
    pageSize: PAGE_SIZE,
    total: countRows[0]?.count ?? 0,
  };
};
