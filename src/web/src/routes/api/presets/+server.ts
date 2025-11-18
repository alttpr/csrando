import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { db } from "$lib/server/db";
import { presets } from "$lib/server/db/schema";
import { generateId } from "$lib/utils/id";
import { eq, or } from "drizzle-orm";

export const GET: RequestHandler = async ({ locals }) => {
  // Get system presets and user's presets if logged in
  const conditions = [eq(presets.isSystem, true)];
  if (locals.user) {
    conditions.push(eq(presets.userId, locals.user.id));
  }

  const results = await db
    .select()
    .from(presets)
    .where(or(...conditions));
  return json(results);
};

export const POST: RequestHandler = async ({ request, locals }) => {
  if (!locals.user) {
    throw svelteError(401, "Unauthorized");
  }

  const body = await request.json();
  const { name, description, options, isSystem } = body;

  if (!name || !options) {
    throw svelteError(400, "Name and options are required");
  }

  if (isSystem && locals.user.role !== "admin") {
    throw svelteError(403, "Only admins can create system presets");
  }

  const id = generateId();
  await db.insert(presets).values({
    id,
    name,
    description,
    options,
    userId: locals.user.id,
    isSystem: !!isSystem,
  });

  return json({ id });
};
