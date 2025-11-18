import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { db } from "$lib/server/db";
import { presets } from "$lib/server/db/schema";
import { eq } from "drizzle-orm";

export const DELETE: RequestHandler = async ({ params, locals }) => {
  if (!locals.user) {
    throw svelteError(401, "Unauthorized");
  }

  const { id } = params;

  // Check if preset exists and belongs to user or is system and user is admin
  const preset = await db.query.presets.findFirst({
    where: eq(presets.id, id),
  });

  if (!preset) {
    throw svelteError(404, "Preset not found");
  }

  if (preset.isSystem && locals.user.role !== "admin") {
    throw svelteError(403, "Only admins can delete system presets");
  }

  if (!preset.isSystem && preset.userId !== locals.user.id) {
    throw svelteError(403, "You can only delete your own presets");
  }

  await db.delete(presets).where(eq(presets.id, id));

  return json({ success: true });
};

export const PUT: RequestHandler = async ({ params, request, locals }) => {
  if (!locals.user) {
    throw svelteError(401, "Unauthorized");
  }

  const { id } = params;
  const body = await request.json();
  const { name, description, options, isSystem } = body;

  const preset = await db.query.presets.findFirst({
    where: eq(presets.id, id),
  });

  if (!preset) {
    throw svelteError(404, "Preset not found");
  }

  if (preset.isSystem && locals.user.role !== "admin") {
    throw svelteError(403, "Only admins can update system presets");
  }

  if (!preset.isSystem && preset.userId !== locals.user.id) {
    throw svelteError(403, "You can only update your own presets");
  }

  // If changing isSystem, check admin
  if (
    isSystem !== undefined &&
    isSystem !== preset.isSystem &&
    locals.user.role !== "admin"
  ) {
    throw svelteError(403, "Only admins can change system status");
  }

  await db
    .update(presets)
    .set({
      name: name ?? preset.name,
      description: description ?? preset.description,
      options: options ?? preset.options,
      isSystem: isSystem ?? preset.isSystem,
      updatedAt: new Date(),
    })
    .where(eq(presets.id, id));

  return json({ success: true });
};
