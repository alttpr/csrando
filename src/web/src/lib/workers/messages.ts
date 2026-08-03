// Types used to communicate with the ROM patching Web Worker
import type {
  GameSpriteConfig,
  GamePostGenConfig,
  PostGenSelections,
} from "$lib/types";

export type WorkerRequest = {
  baseRom: ArrayBuffer;
  patchData: ArrayBuffer;
  additionalRoms: Record<string, ArrayBuffer>;
  primaryGameId: string;
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
  spritePatchDataContents: Map<string, ArrayBuffer>;
  allGameIds: string[];
  publicSpritesBaseUrl: string;
  // Post-generation settings support
  selectedPostGenByGameId?: Map<string, PostGenSelections>;
  gameIdToPostGenConfigMap?: Map<string, GamePostGenConfig>;
};

export type WorkerResponse =
  | { type: "ready" }
  | { type: "progress"; progress: number }
  | { type: "complete"; patchedRom: ArrayBuffer; fileName: string }
  | { type: "error"; message: string };
