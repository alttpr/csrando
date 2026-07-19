import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import { setPreferences } from "$lib/server/presets/service";
import { PresetPreferencesRequestSchema } from "$lib/schemas/presets";

export const PUT: RequestHandler = async ({ request, locals }) => {
  const user = requireUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = PresetPreferencesRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw svelteError(400, { message: "Invalid preferences payload" });
  }

  const preferences = await setPreferences(user, parsed.data);
  return json({ preferences });
};
