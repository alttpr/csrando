import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireSessionUser } from "$lib/server/auth-guards";
import { revokeApiKey } from "$lib/server/api-keys";

export const DELETE: RequestHandler = async ({ params, locals }) => {
  const user = requireSessionUser(locals);
  const revoked = await revokeApiKey(user.id, params.id);
  if (!revoked) {
    throw svelteError(404, { message: "API key not found" });
  }
  return json({ ok: true });
};
