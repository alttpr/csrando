import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  duplicatePreset,
  toPresetSummary,
  toRevisionDto,
} from "$lib/server/presets/service";
import { DuplicatePresetRequestSchema } from "$lib/schemas/presets";

export const POST: RequestHandler = async ({ params, request, locals }) => {
  const user = requireUser(locals);

  let body: unknown = {};
  try {
    const text = await request.text();
    if (text) body = JSON.parse(text);
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = DuplicatePresetRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw svelteError(400, { message: "Invalid duplicate request" });
  }

  const { preset, revision } = await duplicatePreset(
    user,
    params.id,
    parsed.data.name,
  );
  return json(
    {
      preset: toPresetSummary(preset, revision),
      revision: toRevisionDto(revision),
    },
    { status: 201 },
  );
};
