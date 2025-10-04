import { browser } from "$app/environment";
import type { RomFileData } from "$lib/types";
import localforage from "localforage";

type StorageAdapter = {
  setItem<T>(key: string, value: T): Promise<T>;
  getItem<T>(key: string): Promise<T | null>;
  removeItem(key: string): Promise<void>;
};

let storageAdapter: StorageAdapter | null = null;

const createMemoryAdapter = (): StorageAdapter => {
  const memory = new Map<string, unknown>();
  return {
    async setItem<T>(key: string, value: T) {
      memory.set(key, value);
      return value;
    },
    async getItem<T>(key: string) {
      return (memory.get(key) as T | undefined) ?? null;
    },
    async removeItem(key: string) {
      memory.delete(key);
    },
  };
};

const ensureStorage = (): StorageAdapter => {
  if (storageAdapter) return storageAdapter;

  if (!browser) {
    storageAdapter = createMemoryAdapter();
    return storageAdapter;
  }

  localforage.config({
    name: "RandoWebRoms",
    storeName: "rom_files",
    description: "Storage for uploaded ROM files",
  });
  storageAdapter = localforage;
  return storageAdapter;
};

const getRomKey = (gameId: string): string => `rom_global_${gameId}`;

export async function saveRomData(
  gameId: string,
  romData: RomFileData,
): Promise<void> {
  const store = ensureStorage();
  await store.setItem(getRomKey(gameId), romData);
}

export async function getRomData(gameId: string): Promise<RomFileData | null> {
  const store = ensureStorage();
  return await store.getItem<RomFileData>(getRomKey(gameId));
}

export async function removeRomData(gameId: string): Promise<void> {
  const store = ensureStorage();
  await store.removeItem(getRomKey(gameId));
}

export async function calculateSha256Hash(
  buffer: ArrayBuffer,
): Promise<string> {
  const hashBuffer = await crypto.subtle.digest("SHA-256", buffer);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  return hashArray.map((b) => b.toString(16).padStart(2, "0")).join("");
}
