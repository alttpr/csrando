import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { requireUser } from "$lib/server/auth-guards";
import {
  createPreset,
  listPresetsFor,
  setFavorite,
  setPreferences,
  toPresetSummary,
  toRevisionDto,
} from "$lib/server/presets/service";
import {
  getValidationMetadata,
  validateSettings,
} from "$lib/server/presets/validate";
import { CreatePresetRequestSchema } from "$lib/schemas/presets";

export const GET: RequestHandler = async ({ url, locals }) => {
  const configId = (url.searchParams.get("configId") || "").toLowerCase();
  if (!configId) {
    throw svelteError(400, { message: "configId is required" });
  }
  try {
    return json(await listPresetsFor(configId, locals.user));
  } catch (err) {
    if ((err as { status?: number }).status) throw err;
    console.error("Failed to list presets:", err);
    throw svelteError(500, { message: "Failed to load presets." });
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

  const parsed = CreatePresetRequestSchema.safeParse(body);
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
    const { preset, revision } = await createPreset(user, {
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
      await setPreferences(user, { defaultPresetId: preset.id });
    }
    if (input.favorite) {
      await setFavorite(user, preset.id, true);
    }

    return json(
      {
        preset: toPresetSummary(preset, revision),
        revision: toRevisionDto(revision),
      },
      { status: 201 },
    );
  } catch (err) {
    if ((err as { status?: number }).status) throw err;
    console.error("Failed to create preset:", err);
    throw svelteError(500, { message: "Failed to save the preset." });
  }
};
