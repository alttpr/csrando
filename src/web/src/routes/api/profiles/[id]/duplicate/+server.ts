import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  duplicateProfile,
  toProfileSummary,
  toRevisionDto,
} from "$lib/server/profiles/service";
import { DuplicateProfileRequestSchema } from "$lib/schemas/profiles";

export const POST: RequestHandler = async ({ params, request, locals }) => {
  const user = requireUser(locals);

  let body: unknown = {};
  try {
    const text = await request.text();
    if (text) body = JSON.parse(text);
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = DuplicateProfileRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw svelteError(400, { message: "Invalid duplicate request" });
  }

  const { profile, revision } = await duplicateProfile(
    user,
    params.id,
    parsed.data.name,
  );
  return json(
    {
      profile: toProfileSummary(profile, revision),
      revision: toRevisionDto(revision),
    },
    { status: 201 },
  );
};
