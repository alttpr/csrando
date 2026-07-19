import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  createRevision,
  getReadablePreset,
  toPresetSummary,
  toRevisionDto,
} from "$lib/server/presets/service";
import {
  getValidationMetadata,
  validateSettings,
} from "$lib/server/presets/validate";
import { CreateRevisionRequestSchema } from "$lib/schemas/presets";

export const POST: RequestHandler = async ({ params, request, locals }) => {
  const user = requireUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = CreateRevisionRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw svelteError(400, { message: "Invalid revision payload" });
  }

  // Resolve the preset first (404s for foreign/deleted presets) so we know
  // which config page's metadata to validate against.
  const preset = await getReadablePreset(params.id, user);

  const metadata = await getValidationMetadata(preset.configId);
  const validation = validateSettings(parsed.data.settings, metadata);
  if (!validation.ok || !validation.settings) {
    throw svelteError(400, {
      message: validation.message ?? "Invalid settings payload",
      fieldErrors: validation.fieldErrors,
    });
  }

  const { preset: updated, revision } = await createRevision(user, preset.id, {
    settings: validation.settings,
    changeSummary: parsed.data.changeSummary ?? null,
    baseRevisionId: parsed.data.baseRevisionId,
  });

  return json(
    {
      preset: toPresetSummary(updated, revision),
      revision: toRevisionDto(revision),
    },
    { status: 201 },
  );
};
