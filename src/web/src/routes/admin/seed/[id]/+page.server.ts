import { error as svelteError } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { eq } from "drizzle-orm";
import { db } from "$lib/server/db";
import { seeds } from "$lib/server/db/schema";
import { requirePanelAccess } from "$lib/server/admin/guard";
import { shouldHideSpoiler } from "$lib/server/seed-visibility";
import { getSeedAttribution } from "$lib/server/presets/service";

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

// Admin seed inspection: unlike the public permalink, the spoiler log is
// returned even for race seeds — that is the point of this page.
export const load: PageServerLoad = async ({ params, locals, cookies }) => {
  requirePanelAccess(locals, cookies);

  const rows = await db
    .select({
      id: seeds.id,
      createdAt: seeds.createdAt,
      options: seeds.options,
      spoilerLog: seeds.spoilerLog,
      presetId: seeds.presetId,
      presetRevisionId: seeds.presetRevisionId,
      differedFromRevision: seeds.differedFromRevision,
    })
    .from(seeds)
    .where(eq(seeds.id, params.id))
    .limit(1);
  const seed = rows[0];
  if (!seed) {
    throw svelteError(404, { message: "Seed not found" });
  }

  let attribution = null;
  if (seed.presetId) {
    try {
      attribution = await getSeedAttribution(
        seed.presetId,
        seed.presetRevisionId,
        locals.user,
      );
    } catch (e) {
      console.error("Failed to resolve seed attribution:", e);
    }
  }

  return {
    seed: {
      id: seed.id,
      createdAt: toIso(seed.createdAt),
      raceMode: shouldHideSpoiler(seed.options),
      spoilerLog: seed.spoilerLog,
      differedFromRevision:
        seed.differedFromRevision === null
          ? null
          : Boolean(seed.differedFromRevision),
    },
    attribution,
  };
};
