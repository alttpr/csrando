import { env as privateEnv } from "$env/dynamic/private";
import { error as svelteError } from "@sveltejs/kit";
import type { Metadata } from "$lib/types";
import { parseMetadata } from "$lib/schemas/metadata";
import { mockDataService } from "./mock-data";

type RequestOptions = {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  headers?: HeadersInit;
};

/**
 * Handles API requests to the .NET backend, with error handling and response processing
 */
export async function callBackendApi<T = unknown>(
  endpoint: string,
  options: RequestOptions = { method: "GET" },
): Promise<T> {
  const { method = "GET", body, headers = {} } = options;

  try {
    const base = (privateEnv.PRIVATE_DOTNET_API_BASE_URL || "").replace(
      /\/$/,
      "",
    );
    if (!base) {
      throw svelteError(500, "PRIVATE_DOTNET_API_BASE_URL is not configured");
    }
    const url = `${base}/${endpoint.startsWith("/") ? endpoint.slice(1) : endpoint}`;

    const requestOptions: RequestInit = {
      method,
      headers: {
        "Content-Type": "application/json",
        ...headers,
      },
    };

    if (body) {
      requestOptions.body = JSON.stringify(body);
    }

    const response = await fetch(url, requestOptions);

    if (!response.ok) {
      let bodyText: string | null = null;
      let errorJson: unknown = null;
      try {
        // Try text first (safe single read)
        bodyText = await response.text();
        try {
          errorJson = JSON.parse(bodyText);
        } catch {
          // not JSON
        }
      } catch (readErr) {
        console.error("Failed reading error body", readErr);
      }
      const message =
        (errorJson as { message?: string })?.message ||
        bodyText ||
        response.statusText ||
        `Error from backend API (${response.status})`;
      console.error(
        `.NET API error: ${response.status}`,
        errorJson || bodyText,
      );
      throw svelteError(response.status, message);
    }

    // For 204 No Content responses, return null
    if (response.status === 204) {
      return null as T;
    }

    return (await response.json()) as T;
  } catch (err: unknown) {
    console.error(`Error calling .NET API (${endpoint}):`, err);

    // If it's already a SvelteKit error, rethrow it
    if ((err as { status?: number }).status) {
      throw err;
    }

    // Otherwise, wrap it in a generic 500 error
    throw svelteError(500, {
      message: "Failed to communicate with the backend service.",
    });
  }
}

/**
 * API service for metadata operations
 */
export const metadataApi = {
  /**
   * Resolve a canonical backend randomizer id (case/alias insensitive) using the /meta index.
   * Falls back to the provided id if no match is found.
   */
  resolveCanonicalId: async (id: string): Promise<string> => {
    const want = (id || "").toLowerCase();
    if (!want) return id;

    // Quick alias map to bridge common shorthand -> backend ids
    const alias: Record<string, string> = {
      alttp: "Alttpr",
      z3: "Alttpr",
      zelda3: "Alttpr",
      z1: "Z1R",
      zelda1: "Z1R",
      goonies2: "G2R",
      g2: "G2R",
    };

    try {
      const index = (await callBackendApi<
        Array<{ name?: string; description?: string; randomizer: string }>
      >("meta")) as Array<{
        name?: string;
        description?: string;
        randomizer: string;
      }>;
      // Exact/randomizer match (case-insensitive) or name match
      let match = index.find(
        (e) =>
          e.randomizer?.toLowerCase() === want ||
          e.name?.toLowerCase() === want,
      );
      if (match) return match.randomizer;

      // Alias mapping to a candidate, then match again to be safe
      const candidate = alias[want];
      if (candidate) {
        const lc = candidate.toLowerCase();
        match = index.find((e) => e.randomizer?.toLowerCase() === lc);
        if (match) return match.randomizer;
        return candidate; // fall back to alias string even if index mismatch
      }
    } catch {
      // ignore
    }
    // No mapping found, return original id
    return id;
  },
  /**
   * Retrieves all metadata from the backend (or mock data in test mode)
   */
  getAll: async () => {
    // Check if we're in local test mode
    if (privateEnv.LOCAL_TEST_MODE === "true") {
      const mockData = mockDataService.getAllMetadata();
      return mockData.length > 0 ? mockData : [];
    }

    return await callBackendApi("meta");
  },

  /**
   * Retrieves metadata for a specific ID (or mock data in test mode)
   */
  getById: async (id: string) => {
    // Check if we're in local test mode
    if (privateEnv.LOCAL_TEST_MODE === "true") {
      const mockData = mockDataService.getMetadataById(id);
      if (mockData) {
        return mockData;
      }
      // If no mock data found for this ID, throw a 404-like error
      throw svelteError(
        404,
        `Mock metadata not found for ID: ${id}. Available IDs: ${mockDataService.getAvailableGameIds().join(", ")}`,
      );
    }

    const raw = await callBackendApi<unknown>(`meta/${id}`);
    const parsed = parseMetadata(raw);
    if (!parsed.success) {
      throw new Error(
        "Invalid metadata structure received: " + JSON.stringify(parsed.error),
      );
    }
    return parsed.data as Metadata;
  },
};

/**
 * API service for randomization operations
 */
export const randomizeApi = {
  /**
   * Creates a new randomization based on the provided options (or mock response in test mode)
   */
  create: async (options: unknown) => {
    // Check if we're in local test mode
    if (privateEnv.LOCAL_TEST_MODE === "true") {
      // Return a mock randomization response
      return {
        id: "mock-seed-" + Date.now(),
        patchData: {
          // Mock patch data structure
          patches: [],
          metadata: {
            games: ["alttp"],
            version: "1.0.0-mock",
            generated: new Date().toISOString(),
          },
        },
        placementInfo: {
          seed: "mock-seed",
          logic: "normal",
          mode: "local-test",
        },
        options: options,
      };
    }

    return await callBackendApi("randomize", {
      method: "POST",
      body: options,
    });
  },
};
