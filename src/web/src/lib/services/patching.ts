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

// Utility function to convert Base64 to ArrayBuffer
function base64ToArrayBuffer(base64: string): ArrayBuffer {
  const binaryString = window.atob(base64);
  const len = binaryString.length;
  const bytes = new Uint8Array(len);
  for (let i = 0; i < len; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  return bytes.buffer;
}

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
    baseName?: string | null; // From original ROM filename
    defaultDisplayName: string; // Game's display name
    defaultExtension: string; // e.g., ".sfc"
    randomizerVersion?: string | null;
    optionSummary?: {
      tokens: string[];
      total: number;
      derivedFromMetadata: boolean;
    };
  };
  // New params for sprite patching
  selectedSpritesByGameId: Map<string, string>;
  gameIdToSpriteInfoMap: Map<string, GameSpriteConfig>;
  // New params for post-generation settings
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
    // Fetch sprite binary data
    const spritePatchDataContents = new Map<string, ArrayBuffer>();
    const gameIdsInSeed = new Set<string>([
      params.primaryGameId,
      ...Object.keys(params.additionalRoms),
    ]);

    patchingProgress.set(5); // Initial progress for fetching sprite data

    let currentFile = 0;
    const totalFilesToFetch = Array.from(gameIdsInSeed).reduce(
      (count, gameId) => {
        const spriteValue = params.selectedSpritesByGameId.get(gameId);
        if (spriteValue) {
          const spriteConfig = params.gameIdToSpriteInfoMap
            .get(gameId)
            ?.sprites?.find((s) => s.value === spriteValue);
          if (spriteConfig?.patchDetails?.files) {
            return count + spriteConfig.patchDetails.files.length;
          }
        }
        return count;
      },
      0,
    );

    for (const gameId of gameIdsInSeed) {
      const spriteValue = params.selectedSpritesByGameId.get(gameId);
      if (spriteValue) {
        const spriteConfig = params.gameIdToSpriteInfoMap
          .get(gameId)
          ?.sprites?.find((s) => s.value === spriteValue);
        if (spriteConfig?.patchDetails?.files) {
          for (const fileEntry of spriteConfig.patchDetails.files) {
            if (!spritePatchDataContents.has(fileEntry.path)) {
              try {
                const base = getPublicSpritesBaseUrl();
                const resolvedUrl = /^(https?:)?\//.test(fileEntry.path)
                  ? fileEntry.path
                  : `${base}/${gameId}/${fileEntry.path}`;
                const response = await fetchWithTimeout(resolvedUrl, {
                  timeoutMs: 20000,
                });
                if (!response.ok) {
                  throw new Error(
                    `Failed to fetch sprite patch file: ${resolvedUrl} (status: ${response.status})`,
                  );
                }
                const arrayBuffer = await response.arrayBuffer();
                spritePatchDataContents.set(fileEntry.path, arrayBuffer);
              } catch (fetchError) {
                console.error(fetchError);
                throw new Error(
                  m.patching_error_fetching_sprite_file({
                    filePath: fileEntry.path,
                  }) +
                    (fetchError instanceof Error
                      ? ` (${fetchError.message})`
                      : ""),
                );
              }
            }
            currentFile++;
            if (totalFilesToFetch > 0) {
              patchingProgress.set(
                5 + Math.round((currentFile / totalFilesToFetch) * 15),
              ); // Sprite fetching up to 20%
            }
          }
        }
      }
    }

    patchingProgress.set(20); // Sprite data fetching complete

    // Convert main patchData to correct format if it's a base64 string
    const patchData =
      typeof params.patchDataSource === "string"
        ? base64ToArrayBuffer(params.patchDataSource)
        : params.patchDataSource;

    // Create a Web Worker for processing
    patcherWorkerInstance = new Worker(
      new URL("../workers/patcher.ts", import.meta.url),
      {
        type: "module",
      },
    );

    // Pass data to worker
    patcherWorkerInstance.postMessage({
      baseRom: params.baseRomBuffer,
      patchData: patchData,
      additionalRoms: params.additionalRoms,
      primaryGameId: params.primaryGameId,
      randomizerId: params.randomizerId,
      basePatchUrl: params.basePatchUrl,
      outputFileNameDetails: params.outputFileNameDetails,
      selectedSpritesByGameId: params.selectedSpritesByGameId,
      gameIdToSpriteInfoMap: params.gameIdToSpriteInfoMap,
      spritePatchDataContents: spritePatchDataContents,
      allGameIds: Array.from(gameIdsInSeed),
      publicSpritesBaseUrl: getPublicSpritesBaseUrl(),
      selectedPostGenByGameId: params.selectedPostGenByGameId,
      gameIdToPostGenConfigMap: params.gameIdToPostGenConfigMap,
    });

    // Handle worker messages
    patcherWorkerInstance.onmessage = (e) => {
      const data = e.data as
        | { type: "progress"; progress: number }
        | { type: "complete"; patchedRom: ArrayBuffer; fileName: string }
        | { type: "error"; message?: string };

      if (data.type === "progress") {
        patchingProgress.set(data.progress);
      } else if (data.type === "complete") {
        // Create a download for the patched ROM
        const blob = new Blob([data.patchedRom], {
          type: "application/octet-stream",
        });
        const url = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = url;
        a.download =
          data.fileName ||
          `patched_rom_${params.outputFileNameDetails.seedId}${params.outputFileNameDetails.defaultExtension}`;
        document.body.appendChild(a);
        a.click();

        // Clean up
        URL.revokeObjectURL(url);
        document.body.removeChild(a);

        resetPatchingState(null, true); // Patching complete, keep progress (e.g. 100%), clear error
      } else if (data.type === "error") {
        // Error reported by the worker's message
        resetPatchingState(data.message || m.patching_generic_error());
      }
    };

    // Handle worker errors directly
    patcherWorkerInstance.onerror = (errorEvent) => {
      console.error("Patching worker error event:", errorEvent);
      resetPatchingState(
        m.patching_worker_error() +
          (errorEvent.message ? ` (${errorEvent.message})` : ""),
      );
    };
  } catch (error) {
    console.error("Error initiating patching process:", error);
    // Error during the setup of the patching process
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
