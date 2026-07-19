import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  getProfileWithRevision,
  softDeleteProfile,
  toProfileSummary,
  toRevisionDto,
  updateProfileMeta,
} from "$lib/server/profiles/service";
import { UpdateProfileRequestSchema } from "$lib/schemas/profiles";

export const GET: RequestHandler = async ({ params, url, locals }) => {
  const revisionId = url.searchParams.get("revision");
  const { profile, revision } = await getProfileWithRevision(
    params.id,
    locals.user,
    revisionId,
  );
  return json({
    profile: toProfileSummary(
      profile,
      revision.id === profile.currentRevisionId ? revision : null,
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

  const parsed = UpdateProfileRequestSchema.safeParse(body);
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

  const profile = await updateProfileMeta(user, params.id, parsed.data);
  return json({ profile: toProfileSummary(profile) });
};

export const DELETE: RequestHandler = async ({ params, locals }) => {
  const user = requireUser(locals);
  await softDeleteProfile(user, params.id);
  return json({ ok: true });
};
