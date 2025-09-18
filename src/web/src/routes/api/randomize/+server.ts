import { json, error as svelteError } from "@sveltejs/kit";
import { dev } from "$app/environment";
import type { RequestHandler } from "./$types";
import { randomizeApi } from "$lib/services/api";
import { db } from "$lib/server/db";
import { seeds, userSeeds } from "$lib/server/db/schema";
import { getActiveRandomizerVersionFor } from "$lib/server/db/randomizer";
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
    // The .NET API expects a root object with a `request` property. If the client
    // forgot to wrap it, wrap here defensively.

    // Forward body directly; backend expects root object with Seed, IncludeSpoiler, Configs
    if (dev)
      console.debug(
        "Randomize payload -> .NET:",
        JSON.stringify(optionsFromRequest),
      );
    const randomizeResponseRaw = await randomizeApi.create(optionsFromRequest);
    const parsedRequest = RandomizeRequestSchema.safeParse(optionsFromRequest);
    if (!parsedRequest.success) {
      console.error(
        "Invalid randomize request:",
        parsedRequest.error.flatten(),
      );
      throw svelteError(400, { message: "Invalid configuration data" });
    }

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
    const uniqueId = crypto.randomUUID();

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
    console.debug("Saving seed with id", uniqueId);
    await db.insert(seeds).values({
      id: uniqueId,
      options: optionsFromRequest,
      patchData: returnedPatchData,
      placementInfo: [],
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

    return json({ id: uniqueId, ...randomizeResponse });
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
