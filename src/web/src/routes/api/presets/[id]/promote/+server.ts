import { error, json, type RequestHandler } from "@sveltejs/kit";
import { z } from "zod";
import { requireSessionUser } from "$lib/server/auth-guards";
import {
  promotePresetToOfficial,
  toPresetSummary,
} from "$lib/server/presets/service";

const PromotePresetSchema = z.object({
  slug: z.string().trim().min(2).max(64),
  name: z.string().trim().min(1).max(60).optional(),
  description: z.string().trim().max(240).nullable().optional(),
});

// Copy a preset's current revision into a new official preset (admin only;
// the admin check lives in the service).
export const POST: RequestHandler = async ({ params, request, locals }) => {
  const user = requireSessionUser(locals);
  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw error(400, { message: "Request body must be valid JSON." });
  }
  const parsed = PromotePresetSchema.safeParse(body);
  if (!parsed.success) {
    throw error(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors: { slug: "A slug of 2-64 characters is required" },
    });
  }

  const { preset, revision } = await promotePresetToOfficial(
    user,
    params.id!,
    parsed.data,
  );
  return json({ preset: toPresetSummary(preset, revision) }, { status: 201 });
};
