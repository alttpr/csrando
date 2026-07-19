import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import { setFavorite } from "$lib/server/presets/service";
import { PresetFavoriteRequestSchema } from "$lib/schemas/presets";

export const PUT: RequestHandler = async ({ request, locals }) => {
  const user = requireUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = PresetFavoriteRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw svelteError(400, { message: "Invalid favorite payload" });
  }

  await setFavorite(
    user,
    parsed.data.presetId,
    parsed.data.favorited,
    parsed.data.displayOrder,
  );
  return json({ ok: true });
};
