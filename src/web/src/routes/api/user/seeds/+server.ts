import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { db } from "$lib/server/db";
import { seeds, userSeeds } from "$lib/server/db/schema";
import { eq, desc } from "drizzle-orm";

export const GET: RequestHandler = async ({ locals }) => {
  if (!locals.user) {
    throw svelteError(401, { message: "Unauthorized" });
  }

  try {
    const userSeedEntries = await db
      .select({
        id: seeds.id,
        options: seeds.options,
        createdAt: seeds.createdAt,
      })
      .from(userSeeds)
      .innerJoin(seeds, eq(userSeeds.seedId, seeds.id))
      .where(eq(userSeeds.userId, locals.user.id))
      .orderBy(desc(userSeeds.createdAt)); // Or seeds.createdAt

    return json(userSeedEntries);
  } catch (dbError: unknown) {
    console.error("Database error while fetching user seeds:", dbError);
    throw svelteError(500, {
      message: "Failed to fetch your generated seeds.",
    });
  }
};
