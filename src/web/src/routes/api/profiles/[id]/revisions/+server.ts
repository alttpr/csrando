import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  createRevision,
  getReadableProfile,
  toProfileSummary,
  toRevisionDto,
} from "$lib/server/profiles/service";
import {
  getValidationMetadata,
  validateSettings,
} from "$lib/server/profiles/validate";
import { CreateRevisionRequestSchema } from "$lib/schemas/profiles";

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

  // Resolve the profile first (404s for foreign/deleted profiles) so we know
  // which config page's metadata to validate against.
  const profile = await getReadableProfile(params.id, user);

  const metadata = await getValidationMetadata(profile.configId);
  const validation = validateSettings(parsed.data.settings, metadata);
  if (!validation.ok || !validation.settings) {
    throw svelteError(400, {
      message: validation.message ?? "Invalid settings payload",
      fieldErrors: validation.fieldErrors,
    });
  }

  const { profile: updated, revision } = await createRevision(
    user,
    profile.id,
    {
      settings: validation.settings,
      changeSummary: parsed.data.changeSummary ?? null,
      baseRevisionId: parsed.data.baseRevisionId,
    },
  );

  return json(
    {
      profile: toProfileSummary(updated, revision),
      revision: toRevisionDto(revision),
    },
    { status: 201 },
  );
};
