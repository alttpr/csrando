import { db } from "$lib/server/db";
import { and, desc, eq } from "drizzle-orm";
import { randomizerVersions, seeds } from "./schema";

export async function getActiveRandomizerVersion() {
  const rows = await db
    .select()
    .from(randomizerVersions)
    .where(eq(randomizerVersions.isActive, true))
    .orderBy(desc(randomizerVersions.createdAt))
    .limit(1);
  return rows[0] || null;
}

export async function getActiveRandomizerVersionFor(randomizerId: string) {
  const rid = (randomizerId || "").toLowerCase();
  try {
    const rows = await db
      .select()
      .from(randomizerVersions)
      .where(
        and(
          eq(randomizerVersions.isActive, true),
          eq(randomizerVersions.randomizerId, rid),
        ),
      )
      .orderBy(desc(randomizerVersions.createdAt))
      .limit(1);
    if (rows[0]) return rows[0];
  } catch (e) {
    const msg = String(e || "");
    if (!msg.includes("no such column")) throw e;
    // If column doesn't exist (pre-migration), fall through
  }
  return await getActiveRandomizerVersion();
}

export async function getRandomizerVersionBySeedId(seedId: string) {
  const rows = await db
    .select({
      versionId: seeds.randomizerVersionId,
    })
    .from(seeds)
    .where(eq(seeds.id, seedId))
    .limit(1);
  const versionId = rows[0]?.versionId;
  if (!versionId) return null;
  const v = await db
    .select()
    .from(randomizerVersions)
    .where(eq(randomizerVersions.id, versionId))
    .limit(1);
  return v[0] || null;
}
