import type { RequestHandler } from "./$types";
import { error as svelteError } from "@sveltejs/kit";
import { getRandomizerVersionBySeedId } from "$lib/server/db/randomizer";

export const GET: RequestHandler = async ({ params }) => {
  const seedId = params.id;
  if (!seedId) throw svelteError(400, "Seed ID is required");

  const version = await getRandomizerVersionBySeedId(seedId);
  if (!version || !version.ipsBasePatchBase64) {
    throw svelteError(404, "Base patch not found for this seed");
  }

  try {
    const bin = Buffer.from(version.ipsBasePatchBase64, "base64");
    return new Response(bin, {
      status: 200,
      headers: {
        "Content-Type": "application/octet-stream",
        "Cache-Control": "public, max-age=31536000, immutable",
      },
    });
  } catch (e) {
    console.error("Failed to decode base patch for seed", seedId, e);
    throw svelteError(500, "Failed to load base patch");
  }
};
