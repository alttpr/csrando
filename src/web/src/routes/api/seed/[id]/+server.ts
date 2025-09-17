import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { db } from "$lib/server/db";
import { seeds } from "$lib/server/db/schema";
import { eq } from "drizzle-orm";

export const GET: RequestHandler = async ({ params }) => {
  const seedId = params.id;

  if (!seedId) {
    throw svelteError(400, { message: "Seed ID is required" });
  }

  try {
    const seedResult = await db
      .select()
      .from(seeds)
      .where(eq(seeds.id, seedId))
      .limit(1);

    if (seedResult.length === 0) {
      throw svelteError(404, { message: "Seed not found" });
    }

    return json(seedResult[0]);
  } catch (dbError: unknown) {
    console.error("Database error while fetching seed:", dbError);
    if ((dbError as { status?: number }).status === 404) throw dbError; // Re-throw SvelteKit errors
    throw svelteError(500, { message: "Failed to fetch seed data." });
  }
};
