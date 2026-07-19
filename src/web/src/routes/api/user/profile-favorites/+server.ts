import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import { setFavorite } from "$lib/server/profiles/service";
import { ProfileFavoriteRequestSchema } from "$lib/schemas/profiles";

export const PUT: RequestHandler = async ({ request, locals }) => {
  const user = requireUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = ProfileFavoriteRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw svelteError(400, { message: "Invalid favorite payload" });
  }

  await setFavorite(
    user,
    parsed.data.profileId,
    parsed.data.favorited,
    parsed.data.displayOrder,
  );
  return json({ ok: true });
};
