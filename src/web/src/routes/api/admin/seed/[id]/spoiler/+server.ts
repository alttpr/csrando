import { error, json, type RequestHandler } from "@sveltejs/kit";
import { isAdminAuthorized } from "$lib/server/admin/version-service";
import { db } from "$lib/server/db";
import { seeds } from "$lib/server/db/schema";
import { eq } from "drizzle-orm";

export const GET: RequestHandler = async ({ params, cookies }) => {
  if (!isAdminAuthorized(cookies)) {
    throw error(401, { message: "Admin authentication is required." });
  }

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
