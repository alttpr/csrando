import { error, json, type RequestHandler } from "@sveltejs/kit";
import { requireAdmin, requireSessionUser } from "$lib/server/auth-guards";
import { db } from "$lib/server/db";
import { seeds } from "$lib/server/db/schema";
import { eq } from "drizzle-orm";

export const GET: RequestHandler = async ({ params, locals }) => {
  requireSessionUser(locals);
  requireAdmin(locals);

  const seedId = params.id;
  if (!seedId) {
    throw error(400, { message: "Seed ID is required" });
  }

  const [seed] = await db
    .select({ id: seeds.id, spoilerLog: seeds.spoilerLog })
    .from(seeds)
    .where(eq(seeds.id, seedId))
    .limit(1);

  if (!seed) {
    throw error(404, { message: "Seed not found" });
  }

  return json(seed);
};
