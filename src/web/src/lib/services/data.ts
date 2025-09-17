import type { Seed } from "$lib/types";

type JsonFetchInit = RequestInit & { errorMessage?: string };

async function fetchJson<T>(
  input: RequestInfo | URL,
  init: JsonFetchInit = {},
): Promise<T> {
  const { errorMessage, ...fetchInit } = init;
  const response = await fetch(input, fetchInit);

  if (!response.ok) {
    let message: string | undefined;
    try {
      const data = (await response.json()) as {
        message?: string;
        error?: string;
      };
      message = data.message || data.error;
    } catch {
      try {
        message = await response.text();
      } catch {
        // ignore secondary failure
      }
    }
    throw new Error(
      message?.trim() ||
        errorMessage ||
        `Request failed with ${response.status}`,
    );
  }

  try {
    return (await response.json()) as T;
  } catch {
    throw new Error(
      `Failed to parse response from ${
        typeof input === "string" ? input : input.toString()
      }`,
    );
  }
}

export async function fetchSeed(id: string): Promise<Seed> {
  return await fetchJson<Seed>(`/api/seed/${id}`, {
    errorMessage: `Failed to fetch seed with ID ${id}`,
  });
}

export async function fetchUserSeeds(): Promise<Seed[]> {
  return await fetchJson<Seed[]>("/api/user/seeds", {
    errorMessage: "Failed to fetch user seeds",
  });
}

export async function createSeed(options: unknown): Promise<{ id: string }> {
  return await fetchJson<{ id: string }>("/api/randomize", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(options),
    errorMessage: "Failed to create seed",
  });
}
