import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  clearShareToken,
  ensureShareToken,
} from "$lib/server/profiles/service";

// Create (or return the existing) share token for a profile the caller may
// modify. The share URL is assembled client-side from the token.
export const POST: RequestHandler = async ({ params, locals }) => {
  const user = requireUser(locals);
  const token = await ensureShareToken(user, params.id);
  return json({ token });
};

// Revoke the share link. Idempotent.
export const DELETE: RequestHandler = async ({ params, locals }) => {
  const user = requireUser(locals);
  await clearShareToken(user, params.id);
  return json({ ok: true });
};
