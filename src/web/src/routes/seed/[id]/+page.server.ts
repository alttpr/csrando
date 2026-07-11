import { error as svelteError } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";
import { db } from "$lib/server/db";
import { seeds } from "$lib/server/db/schema";
import { eq } from "drizzle-orm";
import { metadataApi } from "$lib/services/api";
import { parseMetadata } from "$lib/schemas/metadata";
import type { Metadata } from "$lib/types";
import { getRandomizerVersionBySeedId } from "$lib/server/db/randomizer";
import { shouldHideSpoiler } from "$lib/server/seed-visibility";

// Define a type for the seed details, inferring from the Drizzle schema
type SeedDetails = typeof seeds.$inferSelect;

export const load: PageServerLoad = async ({ params }) => {
  const seedId = params.id;

  if (!seedId) {
    throw svelteError(400, "Seed ID is required");
  }

  try {
    const seedResultArray = await db
      .select()
      .from(seeds)
      .where(eq(seeds.id, seedId))
      .limit(1);

    if (seedResultArray.length === 0) {
      throw svelteError(404, "Seed not found");
    }

    const seedDetails: SeedDetails = seedResultArray[0];

    // Prefer metadata snapshot from the randomizer version used for this seed (back-compat view)
    let metadata: Metadata | null = null;
    let rawMetadata: unknown = null;
    let versionTag: string | null = null;
    let versionId: string | null = null;
    let randomizerIdForSeed: string | null = null;
    let gitCommitHash: string | null = null;
    let buildDateIso: string | null = null;
    try {
      const v = await getRandomizerVersionBySeedId(seedId);
      if (v?.optionsMetadata) {
        versionTag = v.versionTag ?? null;
        versionId = v.id ?? null;
        randomizerIdForSeed =
          (v as { randomizerId?: string })?.randomizerId || null;
        gitCommitHash =
          (v as { gitCommitHash?: string | null }).gitCommitHash ?? null;
        const rawBuildDate = (v as { buildDate?: unknown }).buildDate;
        if (rawBuildDate instanceof Date) {
          buildDateIso = Number.isNaN(rawBuildDate.getTime())
            ? null
            : rawBuildDate.toISOString();
        } else if (typeof rawBuildDate === "string") {
          const parsed = new Date(rawBuildDate);
          if (!Number.isNaN(parsed.getTime())) {
            buildDateIso = parsed.toISOString();
          }
        } else if (typeof rawBuildDate === "number") {
          const parsed = new Date(rawBuildDate);
          if (!Number.isNaN(parsed.getTime())) {
            buildDateIso = parsed.toISOString();
          }
        }
        // Ensure postGenSettings are present from snapshot if not embedded in the metadata
        const meta = v.optionsMetadata as Record<string, unknown>;
        if (
          v.postGenSettings &&
          (!meta.postGenSettings || typeof meta.postGenSettings !== "object")
        ) {
          rawMetadata = {
            ...meta,
            postGenSettings: v.postGenSettings,
          };
        } else {
          rawMetadata = meta;
        }
        // Normalize/validate snapshot metadata shape for the viewer
        const parsed = parseMetadata(rawMetadata);
        if (parsed.success) {
          metadata = parsed.data;
        } else {
          console.warn(
            "Snapshot metadata failed validation; falling back to backend metadata for display",
          );
          const idToFetch = randomizerIdForSeed || "alttpr";
          const canonical = await metadataApi.resolveCanonicalId(idToFetch);
          metadata = await metadataApi.getById(canonical);
        }
      } else {
        // No snapshot found: choose metadata by trying to infer the game id from seed options
        const optionsRoot = seedDetails.options as
          | { Configs?: Array<Record<string, unknown>> }
          | undefined;
        const configsArray = optionsRoot?.Configs ?? [];
        const rawConfig = configsArray[0] || {};
        // Prefer explicit Game field if present
        const gameField =
          (rawConfig as { Game?: unknown; game?: unknown }).Game ||
          (rawConfig as { game?: unknown }).game;
        let inferredId: string | null = null;
        if (typeof gameField === "string" && gameField.trim()) {
          inferredId = gameField.trim().toLowerCase();
        } else {
          const firstGameEntry = Object.entries(rawConfig).find(
            ([, v]) =>
              v !== null &&
              typeof v === "object" &&
              (v as object).constructor === Object,
          );
          inferredId = firstGameEntry?.[0]
            ? String(firstGameEntry[0]).toLowerCase()
            : null;
        }
        randomizerIdForSeed = inferredId;
        // Fallback to a sensible default if we still cannot infer
        const idToFetchRaw = inferredId || "alttpr";
        const canonical = await metadataApi.resolveCanonicalId(idToFetchRaw);
        metadata = await metadataApi.getById(canonical);
      }
    } catch (e) {
      // Non-fatal, the page can still render without metadata
      console.error("Failed to fetch metadata for seed page:", e);
    }

    return {
      seedDetails: shouldHideSpoiler(seedDetails.options)
        ? { ...seedDetails, spoilerLog: null }
        : seedDetails,
      metadata,
      randomizerVersion: versionTag
        ? {
            id: versionId,
            versionTag,
            gitCommitHash,
            buildDate: buildDateIso,
          }
        : null,
      randomizerId: randomizerIdForSeed,
    };
  } catch (err) {
    if ((err as { status?: number }).status === 404) {
      throw err; // preserve 404
    }
    console.error("Error loading seed details for page:", err);
    throw svelteError(
      500,
      "Failed to load seed details due to a server error.",
    );
  }
};
