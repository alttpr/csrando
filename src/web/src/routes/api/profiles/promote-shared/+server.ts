import { error, json, type RequestHandler } from "@sveltejs/kit";
import { z } from "zod";
import { requireSessionUser } from "$lib/server/auth-guards";
import {
  promoteSharedProfileToOfficial,
  toProfileSummary,
} from "$lib/server/profiles/service";

const PromoteSharedSchema = z.object({
  token: z.string().trim().min(1),
  slug: z.string().trim().min(2).max(64),
  name: z.string().trim().min(1).max(60).optional(),
  description: z.string().trim().max(240).nullable().optional(),
});

// Promote a profile received through a share link into a new official preset
// (admin only; the admin check lives in the service).
export const POST: RequestHandler = async ({ request, locals }) => {
  const user = requireSessionUser(locals);
  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw error(400, { message: "Request body must be valid JSON." });
  }
  const parsed = PromoteSharedSchema.safeParse(body);
  if (!parsed.success) {
    throw error(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors: {
        slug: "A share token and a slug of 2-64 characters are required",
      },
    });
  }

  const { token, ...input } = parsed.data;
  const { profile, revision } = await promoteSharedProfileToOfficial(
    user,
    token,
    input,
  );
  return json(
    { profile: toProfileSummary(profile, revision) },
    { status: 201 },
  );
};
