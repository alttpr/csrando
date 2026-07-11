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
} from "$lib/schemas/backend";

export const POST: RequestHandler = async ({ request, locals }) => {
  const optionsFromRequest = await request.json();

  // Basic validation
  if (!optionsFromRequest || typeof optionsFromRequest !== "object") {
    throw svelteError(400, { message: "Invalid configuration data" });
  }

  try {
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
    } = parsedRequest.data;
    // Race mode controls public visibility only. Always retain spoilers for admin diagnostics.
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
    const returnedPatchData = worlds[0][1].ipsPatch;

    if (worlds.length === 0) {
      console.error(".NET API response contains no worlds:", randomizeResponse);
      throw svelteError(500, {
        message: "Received invalid seed data from the backend service.",
      });
    }

    // Generate a unique ID by generating a random UUID
    const uniqueId = generateId();

    const inferredRandomizerId: string | null =
      optionsFromRequest.Configs?.[0].Game.toLowerCase() || null;
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
      createdAt: new Date(),
    });

    // If a user is logged in, associate the seed with them
    if (locals.user) {
      await db.insert(userSeeds).values({
        userId: locals.user.id,
        seedId: uniqueId,
        createdAt: new Date(),
      });
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
