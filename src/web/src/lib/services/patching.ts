import { writable } from "svelte/store";
import { getPublicSpritesBaseUrl } from "$lib/env";
import * as m from "$lib/paraglide/messages";
import { fetchWithTimeout } from "$lib/services/http";
import type { GameSpriteConfig, GamePostGenConfig } from "$lib/types";

// Stores for reactive UI updates
export const isPatching = writable<boolean>(false);
export const patchingProgress = writable<number>(0);
export const patchingError = writable<string | null>(null);

let patcherWorkerInstance: Worker | null = null;

const SPRITE_PROGRESS_START = 5;
const SPRITE_PROGRESS_RANGE = 15; // 5 -> 20 in the legacy UI

const base64ToArrayBuffer = (base64: string): ArrayBuffer => {
  const atobFn =
    typeof globalThis.atob === "function" ? globalThis.atob : undefined;
  if (!atobFn) {
    throw new Error("Base64 decoding is not supported in this environment");
  }

  const binaryString = atobFn(base64);
  const bytes = new Uint8Array(binaryString.length);
  for (let i = 0; i < binaryString.length; i += 1) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  return bytes.buffer;
};

type SpriteRequest = {
  path: string;
  url: string;
};

const createWorker = () =>
  new Worker(new URL("../workers/patcher.ts", import.meta.url), {
    type: "module",
  });

const updateSpriteProgress = (completed: number, total: number) => {
  if (total === 0) return;
  const progress =
    SPRITE_PROGRESS_START +
    Math.round((completed / total) * SPRITE_PROGRESS_RANGE);
  patchingProgress.set(progress);
};

const collectSpriteRequests = (
  params: InitiatePatchingParams,
): { requests: SpriteRequest[]; gameIds: string[] } => {
  const base = getPublicSpritesBaseUrl();
  const gameIds = Array.from(
    new Set<string>([
      params.primaryGameId,
      ...Object.keys(params.additionalRoms),
    ]),
  );

  const requests: SpriteRequest[] = [];
  const seenPaths = new Set<string>();

  for (const gameId of gameIds) {
    const spriteValue = params.selectedSpritesByGameId.get(gameId);
    if (!spriteValue) continue;

    const spriteConfig = params.gameIdToSpriteInfoMap
      .get(gameId)
      ?.sprites?.find((sprite) => sprite.value === spriteValue);
    const spriteFiles = spriteConfig?.patchDetails?.files ?? [];

    for (const fileEntry of spriteFiles) {
      if (seenPaths.has(fileEntry.path)) continue;
      const url = /^(https?:)?\/\//.test(fileEntry.path)
        ? fileEntry.path
        : `${base}/${gameId}/${fileEntry.path}`;
      requests.push({ path: fileEntry.path, url });
      seenPaths.add(fileEntry.path);
    }
  }

  return { requests, gameIds };
};

const fetchSpritePatchFiles = async (requests: SpriteRequest[]) => {
  const contents = new Map<string, ArrayBuffer>();
  if (requests.length === 0) {
    return contents;
  }

  let completed = 0;
  for (const request of requests) {
    try {
      const response = await fetchWithTimeout(request.url, {
        timeoutMs: 20000,
      });
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }
      const arrayBuffer = await response.arrayBuffer();
      contents.set(request.path, arrayBuffer);
    } catch (fetchError) {
      console.error(
        "Failed to fetch sprite patch file",
        request.url,
        fetchError,
      );
      throw new Error(
        m.patching_error_fetching_sprite_file({
          filePath: request.path,
        }) + (fetchError instanceof Error ? ` (${fetchError.message})` : ""),
      );
    }
    completed += 1;
    updateSpriteProgress(completed, requests.length);
  }

  return contents;
};

const downloadPatchedRom = (
  payload: { fileName: string | undefined; patchedRom: ArrayBuffer },
  params: InitiatePatchingParams,
) => {
  const blob = new Blob([payload.patchedRom], {
    type: "application/octet-stream",
  });
  const url = URL.createObjectURL(blob);

  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download =
    payload.fileName ||
    `patched_rom_${params.outputFileNameDetails.seedId}${params.outputFileNameDetails.defaultExtension}`;

  document.body.appendChild(anchor);
  anchor.click();
  URL.revokeObjectURL(url);
  document.body.removeChild(anchor);
};

// Internal helper to reset patching state and terminate worker
function resetPatchingState(
  errorMsg: string | null = null,
  keepProgressAtEnd: boolean = false,
): void {
  if (patcherWorkerInstance) {
    patcherWorkerInstance.terminate();
    patcherWorkerInstance = null;
  }
  isPatching.set(false);
  if (!keepProgressAtEnd) {
    // Reset progress unless explicitly told to keep its final state
    patchingProgress.set(0);
  }
  patchingError.set(errorMsg);
}

export interface InitiatePatchingParams {
  baseRomBuffer: ArrayBuffer;
  patchDataSource: string | ArrayBuffer; // Can be base64 string or ArrayBuffer
  additionalRoms: Record<string, ArrayBuffer>;
  primaryGameId: string;
  randomizerId?: string; // Identifier to select base patch (e.g., 'alttpr')
  basePatchUrl?: string; // Optional explicit base patch URL (e.g., per-seed)
  outputFileNameDetails: {
    seedId: string;
    baseName?: string | null;
    defaultDisplayName: string;
    defaultExtension: string;
    randomizerVersion?: string | null;
    optionSummary?: {
      tokens: string[];
      total: number;
      derivedFromMetadata: boolean;
    };
  };
  selectedSpritesByGameId: Map<string, string>;
  gameIdToSpriteInfoMap: Map<string, GameSpriteConfig>;
  selectedPostGenByGameId?: Map<string, Record<string, string | boolean>>;
  gameIdToPostGenConfigMap?: Map<string, GamePostGenConfig>;
}

export async function initiatePatching(
  params: InitiatePatchingParams,
): Promise<void> {
  if (patcherWorkerInstance) {
    patcherWorkerInstance.terminate();
    patcherWorkerInstance = null;
  }

  isPatching.set(true);
  patchingProgress.set(0);
  patchingError.set(null);

  try {
    const { requests, gameIds } = collectSpriteRequests(params);

    patchingProgress.set(SPRITE_PROGRESS_START);
    const spritePatchDataContents = await fetchSpritePatchFiles(requests);
    patchingProgress.set(SPRITE_PROGRESS_START + SPRITE_PROGRESS_RANGE);

    const patchData =
      typeof params.patchDataSource === "string"
        ? base64ToArrayBuffer(params.patchDataSource)
        : params.patchDataSource;

    patcherWorkerInstance = createWorker();

    const payload = {
      baseRom: params.baseRomBuffer,
      patchData,
      additionalRoms: params.additionalRoms,
      primaryGameId: params.primaryGameId,
      randomizerId: params.randomizerId,
      basePatchUrl: params.basePatchUrl,
      outputFileNameDetails: params.outputFileNameDetails,
      selectedSpritesByGameId: params.selectedSpritesByGameId,
      gameIdToSpriteInfoMap: params.gameIdToSpriteInfoMap,
      spritePatchDataContents,
      allGameIds: gameIds,
      publicSpritesBaseUrl: getPublicSpritesBaseUrl(),
      selectedPostGenByGameId: params.selectedPostGenByGameId,
      gameIdToPostGenConfigMap: params.gameIdToPostGenConfigMap,
    };

    patcherWorkerInstance.postMessage(payload);

    patcherWorkerInstance.onmessage = (event) => {
      const data = event.data as
        | { type: "ready" }
        | { type: "progress"; progress: number }
        | { type: "complete"; patchedRom: ArrayBuffer; fileName?: string }
        | { type: "error"; message?: string };

      if (data.type === "ready") {
        // Worker startup acknowledgement; nothing to do yet.
        return;
      }

      if (data.type === "progress") {
        patchingProgress.set(data.progress);
        return;
      }

      if (data.type === "complete") {
        downloadPatchedRom(
          { patchedRom: data.patchedRom, fileName: data.fileName },
          params,
        );
        resetPatchingState(null, true);
        return;
      }

      if (data.type === "error") {
        resetPatchingState(data.message || m.patching_generic_error());
        return;
      }

      console.warn("Received unexpected message from patcher worker", data);
    };

    patcherWorkerInstance.onerror = (errorEvent) => {
      console.error("Patching worker error event:", errorEvent);
      resetPatchingState(
        m.patching_worker_error() +
          (errorEvent.message ? ` (${errorEvent.message})` : ""),
      );
    };
  } catch (error) {
    console.error("Error initiating patching process:", error);
    resetPatchingState(
      m.patching_initiation_error() +
        (error instanceof Error ? ` (${error.message})` : ""),
    );
  }
}

// Function to cancel an ongoing patching operation
export function cancelPatching(): void {
  resetPatchingState(null); // User cancelled, clear error message
}

// Function to clean up the patcher instance (e.g., on component destroy)
export function cleanupPatcher(): void {
  resetPatchingState(null); // General cleanup, clear any error, reset progress
}
