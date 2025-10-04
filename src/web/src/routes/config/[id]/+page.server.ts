import type { Metadata } from "$lib/types";
import * as m from "$lib/paraglide/messages";
import { metadataApi } from "$lib/services/api";
import { parseMetadata } from "$lib/schemas/metadata";

// Define our own types since we can't access ./$types
interface Params {
  id: string;
}

interface LoadEvent {
  params: Params;
}

interface LoadResult {
  metadata: Metadata | null;
  error: string | null;
}

export const load = async ({ params }: LoadEvent): Promise<LoadResult> => {
  const id = (params.id || "").toLowerCase();
  if (!id) {
    return {
      metadata: null,
      error: "Invalid ID provided.",
    };
  }

  try {
    const canonical = await metadataApi.resolveCanonicalId(id);
    const raw = await metadataApi.getById(canonical);

    const parsed = parseMetadata(raw);
    if (!parsed.success) {
      console.error("Invalid metadata payload:", parsed.error.flatten());
      return { metadata: null, error: m.config_metadata_fetch_error() };
    }
    const metadata = parsed.data;

    if (!metadata) {
      return {
        metadata: null,
        error: m.config_no_metadata(),
      };
    }

    return {
      metadata,
      error: null,
    };
  } catch (e: unknown) {
    console.error(`Exception while fetching metadata for ID ${id}:`, e);

    // The error handling from our API service will provide proper error messages
    let errorMsg = m.config_metadata_fetch_error();
    if ((e as { status?: number }).status === 404) {
      errorMsg = `Metadata not found for ID: ${id}.`;
    } else if ((e as { body?: { message?: string } }).body?.message) {
      errorMsg = (e as { body?: { message?: string } }).body?.message as string;
    } else if ((e as { message?: string }).message) {
      errorMsg = (e as { message?: string }).message as string;
    }

    return {
      metadata: null,
      error: errorMsg,
    };
  }
};
