import { json, error as svelteError } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { randomizeApi } from "$lib/services/api";
import { db } from "$lib/server/db";
import { seeds, userSeeds } from "$lib/server/db/schema";
import { getActiveRandomizerVersionFor } from "$lib/server/db/randomizer";
import { generateId } from "$lib/utils/id";
import { shouldHideSpoiler } from "$lib/server/seed-visibility";
import {
  RandomizerResponseSchema,
  RandomizeRequestSchema,
  RandomizeByPresetRequestSchema,
} from "$lib/schemas/backend";
import { NormalizedConfigSchema } from "$lib/schemas/presets";
import {
  buildRandomizePayload,
  configsEqual,
  hydrateFormState,
  normalizeConfig,
  type NormalizedConfig,
} from "$lib/config/normalize";
import { migrateConfig, ConfigMigrationError } from "$lib/config/migrations";
import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
import {
  getPresetWithRevision,
  setPreferences,
} from "$lib/server/presets/service";
import { getValidationMetadata } from "$lib/server/presets/validate";
import type { User } from "lucia";

interface SeedPresetAttribution {
  presetId: string | null;
  presetRevisionId: string | null;
  differedFromRevision: boolean | null;
  configSchemaVersion: number | null;
  settingsSnapshot: unknown;
}

// Resolve preset attribution for a generated seed. Best-effort provenance
// metadata: an invalid or vanished preset reference degrades to a plain
// snapshot and must never fail generation. The "differed" flag is computed
// here from the stored revision — the client's opinion is not trusted.
async function resolvePresetAttribution(
  preset: NonNullable<
    ReturnType<typeof RandomizeRequestSchema.parse>["Preset"]
  >,
  user: User | null,
  generatorRequest: {
    Seed: number;
    IncludeSpoiler: boolean;
    Configs: Array<Record<string, unknown>>;
  },
): Promise<SeedPresetAttribution> {
  const snapshotParse = NormalizedConfigSchema.safeParse(
    preset.settingsSnapshot,
  );
  const settingsSnapshot = snapshotParse.success ? snapshotParse.data : null;

  const result: SeedPresetAttribution = {
    presetId: null,
    presetRevisionId: null,
    differedFromRevision: null,
    configSchemaVersion: null,
    settingsSnapshot: null,
  };

  try {
    if (!settingsSnapshot) return result;

    const game = generatorRequest.Configs[0]?.Game;
    const configId = typeof game === "string" ? game.toLowerCase() : null;
    if (!configId) return result;

    // Bind the claimed normalized snapshot to the generator payload actually
    // submitted. Otherwise a client could attribute arbitrary Configs to an
    // unrelated preset or make a modified seed appear unchanged.
    const metadata = await getValidationMetadata(configId);
    if (!metadata) return result;
    const { form } = hydrateFormState(settingsSnapshot, metadata);
    const normalized = normalizeConfig(form, metadata);
    const rebuilt = buildRandomizePayload(normalized, metadata, {
      includeSpoiler: generatorRequest.IncludeSpoiler,
      seed: generatorRequest.Seed,
    });
    if (!configsEqual(rebuilt.Configs, generatorRequest.Configs)) {
      console.warn(
        "Seed preset attribution dropped: settings do not match Configs",
      );
      return result;
    }

    result.settingsSnapshot = normalized;
    result.configSchemaVersion = CONFIG_SCHEMA_VERSION;

    if (preset.presetId && preset.presetRevisionId) {
      // Enforce preset visibility only after preserving the independently
      // verified snapshot; invalid attribution should not break "open these
      // settings" for the generated seed.
      const stored = await getPresetWithRevision(
        preset.presetId,
        user,
        preset.presetRevisionId,
      );
      if (stored.preset.configId !== configId) return result;
      result.presetId = stored.preset.id;
      result.presetRevisionId = stored.revision.id;
      result.differedFromRevision = !configsEqual(
        normalized,
        stored.revision.settings,
      );
    } else if (preset.presetId || preset.presetRevisionId) return result;
  } catch (err) {
    console.error("Failed to resolve seed preset attribution:", err);
  }
  return result;
}

// Expand a generate-by-preset request into a full randomize request. Used by
// external tools (bots) so they can generate from a saved preset id without
// reconstructing the generator payload themselves.
async function expandPresetRequest(
  raw: unknown,
  user: User | null,
): Promise<Record<string, unknown>> {
  const parsed = RandomizeByPresetRequestSchema.safeParse(raw);
  if (!parsed.success) {
    throw svelteError(400, { message: "Invalid preset generation request" });
  }
  const { PresetId, RevisionId, Seed, IncludeSpoiler } = parsed.data;

  // The reference may be an internal id or an official preset slug. Private
  // presets remain available only to their owner.
  const { preset, revision } = await getPresetWithRevision(
    PresetId,
    user,
    RevisionId ?? null,
  );

  let settings: NormalizedConfig;
  try {
    settings = migrateConfig(
      revision.settings as NormalizedConfig,
      revision.configSchemaVersion,
    ).settings;
  } catch (err) {
    if (err instanceof ConfigMigrationError) {
      throw svelteError(400, {
        message:
          "This preset was saved with an unsupported configuration version.",
      });
    }
    throw err;
  }

  const metadata = await getValidationMetadata(preset.configId);
  if (!metadata) {
    throw svelteError(503, {
      message:
        "Generator metadata is unavailable; cannot expand the preset into a configuration.",
    });
  }

  const { form } = hydrateFormState(settings, metadata);
  const normalized = normalizeConfig(form, metadata);
  const payload = buildRandomizePayload(normalized, metadata, {
    includeSpoiler: IncludeSpoiler ?? true,
    seed: Seed ?? 0,
  });

  return {
    ...payload,
    Preset: {
      presetId: preset.id,
      presetRevisionId: revision.id,
      settingsSnapshot: normalized,
      configSchemaVersion: CONFIG_SCHEMA_VERSION,
    },
  };
}

export const POST: RequestHandler = async ({ request, locals }) => {
  let optionsFromRequest: unknown;
  try {
    optionsFromRequest = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid JSON request body" });
  }

  // Basic validation
  if (!optionsFromRequest || typeof optionsFromRequest !== "object") {
    throw svelteError(400, { message: "Invalid configuration data" });
  }

  try {
    if ("PresetId" in (optionsFromRequest as Record<string, unknown>)) {
      optionsFromRequest = await expandPresetRequest(
        optionsFromRequest,
        locals.user,
      );
    }

    const parsedRequest = RandomizeRequestSchema.safeParse(optionsFromRequest);
    if (!parsedRequest.success) {
      console.error(
        "Invalid randomize request:",
        parsedRequest.error.flatten(),
      );
      throw svelteError(400, { message: "Invalid configuration data" });
    }

    const {
      IncludeSpoiler: includeSpoiler,
      Seed,
      Configs,
      Preset: presetBlock,
    } = parsedRequest.data;
    // Race mode controls public visibility only. Always retain spoilers for admin diagnostics.
    // The Preset attribution block is intentionally NOT forwarded to the generator.
    const generatorRequest = { Seed, IncludeSpoiler: true, Configs };
    const randomizeResponseRaw = await randomizeApi.create(generatorRequest);

    const parsedResponse =
      RandomizerResponseSchema.safeParse(randomizeResponseRaw);
    if (!parsedResponse.success) {
      console.error(
        "Invalid randomize API response:",
        parsedResponse.error.flatten(),
      );
      throw svelteError(500, {
        message: "Received invalid seed data from the backend service.",
      });
    }
    const randomizeResponse = parsedResponse.data;

    const worlds = Object.entries(randomizeResponse.worlds);
    if (worlds.length === 0) {
      console.error(".NET API response contains no worlds:", randomizeResponse);
      throw svelteError(500, {
        message: "Received invalid seed data from the backend service.",
      });
    }
    const firstWorld = worlds[0][1];
    const returnedPatchData = firstWorld.ipsPatch ?? firstWorld.bpsPatch!;

    // Generate a unique ID by generating a random UUID
    const uniqueId = generateId();

    // Game is omitted when the metadata's random-target option is selected.
    // In that case generation still works; there simply is no version id to
    // associate until the backend resolves the target.
    const requestedGame = Configs[0]?.Game;
    const inferredRandomizerId =
      typeof requestedGame === "string" && requestedGame.trim()
        ? requestedGame.trim().toLowerCase()
        : null;
    let activeVersion = null;

    if (inferredRandomizerId !== null) {
      try {
        activeVersion =
          await getActiveRandomizerVersionFor(inferredRandomizerId);
      } catch (err) {
        console.error(
          "Error fetching active version for game",
          inferredRandomizerId,
          err,
        );
      }
    }

    const attribution = presetBlock
      ? await resolvePresetAttribution(presetBlock, locals.user, {
          ...generatorRequest,
          IncludeSpoiler: includeSpoiler,
        })
      : null;

    // Save the seed to the database
    await db.insert(seeds).values({
      id: uniqueId,
      options: {
        ...generatorRequest,
        IncludeSpoiler: includeSpoiler,
      },
      patchData: returnedPatchData,
      placementInfo: [],
      spoilerLog: randomizeResponse.spoilerLog || null,
      randomizerVersionId: activeVersion?.id,
      presetId: attribution?.presetId ?? null,
      presetRevisionId: attribution?.presetRevisionId ?? null,
      differedFromRevision: attribution?.differedFromRevision ?? null,
      configSchemaVersion: attribution?.configSchemaVersion ?? null,
      settingsSnapshot: attribution?.settingsSnapshot ?? null,
      createdAt: new Date(),
    });

    // If a user is logged in, associate the seed with them
    if (locals.user) {
      await db.insert(userSeeds).values({
        userId: locals.user.id,
        seedId: uniqueId,
        createdAt: new Date(),
      });
      if (attribution?.presetId) {
        try {
          await setPreferences(locals.user, {
            lastUsedPresetId: attribution.presetId,
          });
        } catch (err) {
          // Best-effort bookkeeping; never fail generation over it.
          console.error("Failed to record last-used preset:", err);
        }
      }
    }
    // --- End of Database Interaction ---

    const response = {
      id: uniqueId,
      seed: randomizeResponse.seed,
      worlds: randomizeResponse.worlds,
      spoilerLog: shouldHideSpoiler({ IncludeSpoiler: includeSpoiler })
        ? undefined
        : randomizeResponse.spoilerLog,
    };
    return json(response);
  } catch (err: unknown) {
    console.error("Error in POST /api/randomize:", err);
    if (
      (err as { status?: number; body?: unknown }).status &&
      (err as { body?: unknown }).body
    ) {
      throw err;
    }
    throw svelteError(500, {
      message: "Failed to process randomization request.",
    });
  }
};
