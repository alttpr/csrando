import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  getPresetWithRevision,
  softDeletePreset,
  toPresetSummary,
  toRevisionDto,
  updatePresetMeta,
} from "$lib/server/presets/service";
import { UpdatePresetRequestSchema } from "$lib/schemas/presets";

export const GET: RequestHandler = async ({ params, url, locals }) => {
  const revisionId = url.searchParams.get("revision");
  const { preset, revision } = await getPresetWithRevision(
    params.id,
    locals.user,
    revisionId,
  );
  return json({
    preset: toPresetSummary(
      preset,
      revision.id === preset.currentRevisionId ? revision : null,
    ),
    revision: toRevisionDto(revision),
  });
};

export const PATCH: RequestHandler = async ({ params, request, locals }) => {
  const user = requireUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = UpdatePresetRequestSchema.safeParse(body);
  if (!parsed.success) {
    const fieldErrors: Record<string, string> = {};
    for (const issue of parsed.error.issues) {
      fieldErrors[issue.path.join(".")] = issue.message;
    }
    throw svelteError(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors,
    });
  }

  const preset = await updatePresetMeta(user, params.id, parsed.data);
  return json({ preset: toPresetSummary(preset) });
};

export const DELETE: RequestHandler = async ({ params, locals }) => {
  const user = requireUser(locals);
  await softDeletePreset(user, params.id);
  return json({ ok: true });
};
