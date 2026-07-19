import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  createProfile,
  listProfilesFor,
  setFavorite,
  setPreferences,
  toProfileSummary,
  toRevisionDto,
} from "$lib/server/profiles/service";
import {
  getValidationMetadata,
  validateSettings,
} from "$lib/server/profiles/validate";
import { CreateProfileRequestSchema } from "$lib/schemas/profiles";

export const GET: RequestHandler = async ({ url, locals }) => {
  const configId = (url.searchParams.get("configId") || "").toLowerCase();
  if (!configId) {
    throw svelteError(400, { message: "configId is required" });
  }
  try {
    return json(await listProfilesFor(configId, locals.user));
  } catch (err) {
    if ((err as { status?: number }).status) throw err;
    console.error("Failed to list profiles:", err);
    throw svelteError(500, { message: "Failed to load profiles." });
  }
};

export const POST: RequestHandler = async ({ request, locals }) => {
  const user = requireUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = CreateProfileRequestSchema.safeParse(body);
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
  const input = parsed.data;
  const configId = input.configId.toLowerCase();

  const metadata = await getValidationMetadata(configId);
  const validation = validateSettings(input.settings, metadata);
  if (!validation.ok || !validation.settings) {
    throw svelteError(400, {
      message: validation.message ?? "Invalid settings payload",
      fieldErrors: validation.fieldErrors,
    });
  }

  try {
    const { profile, revision } = await createProfile(user, {
      configId,
      name: input.name,
      description: input.description ?? null,
      settings: validation.settings,
      changeSummary: input.changeSummary ?? null,
      scope: input.scope,
      slug: input.slug,
      gameTags: input.gameTags,
      difficultyTag: input.difficultyTag,
      isRecommended: input.isRecommended,
      featured: input.featured,
      displayOrder: input.displayOrder,
    });

    if (input.setAsDefault) {
      await setPreferences(user, { defaultProfileId: profile.id });
    }
    if (input.favorite) {
      await setFavorite(user, profile.id, true);
    }

    return json(
      {
        profile: toProfileSummary(profile, revision),
        revision: toRevisionDto(revision),
      },
      { status: 201 },
    );
  } catch (err) {
    if ((err as { status?: number }).status) throw err;
    console.error("Failed to create profile:", err);
    throw svelteError(500, { message: "Failed to save the profile." });
  }
};
