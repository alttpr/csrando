import { getPublicSpritesBaseUrl } from "$lib/env";

// Types align with atlas-builder output (subset used by client)
export interface SpriteAtlas {
  version: 1;
  game: string;
  hash: string;
  generatedAt: string;
  sheet: { image: string; width: number; height: number };
  cell: { w: number; h: number };
  sprites: Array<{
    value: string;
    name?: string;
    author?: string;
    x: number;
    y: number;
    w: number;
    h: number;
    originalImagePath: string;
  }>;
  byValue: Record<
    string,
    { x: number; y: number; w: number; h: number; i: number }
  >;
}

interface AtlasCacheEntry {
  atlas: SpriteAtlas | null; // null indicates failed fetch cached
  fetchedAt: number;
}

const atlasCache = new Map<string, AtlasCacheEntry>();
const inflight = new Map<string, Promise<SpriteAtlas | null>>();
const CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes (metadata small; adjust as needed)

function baseUrl(): string {
  return getPublicSpritesBaseUrl();
}

export async function loadAtlas(
  game: string,
  opts: { force?: boolean } = {},
): Promise<SpriteAtlas | null> {
  const key = game.toLowerCase();
  if (!opts.force) {
    const cached = atlasCache.get(key);
    if (cached && Date.now() - cached.fetchedAt < CACHE_TTL_MS) {
      return cached.atlas;
    }
  }
  if (inflight.has(key)) return inflight.get(key)!;

  const url = `${baseUrl()}/${key}/sheet-${key}-latest.json`;
  const p = (async () => {
    try {
      const res = await fetch(url, { cache: "no-store" });
      if (!res.ok) {
        atlasCache.set(key, { atlas: null, fetchedAt: Date.now() });
        return null;
      }
      const json = await res.json();
      if (
        json &&
        json.version === 1 &&
        json.game === key &&
        json.sheet &&
        json.byValue
      ) {
        atlasCache.set(key, {
          atlas: json as SpriteAtlas,
          fetchedAt: Date.now(),
        });
        return json as SpriteAtlas;
      }
    } catch {
      // swallow network errors; treat as no atlas
    } finally {
      inflight.delete(key);
    }
    atlasCache.set(key, { atlas: null, fetchedAt: Date.now() });
    return null;
  })();
  inflight.set(key, p);
  return p;
}

export function getCachedAtlas(game: string): SpriteAtlas | null {
  const entry = atlasCache.get(game.toLowerCase());
  return entry ? entry.atlas : null;
}

export interface SpriteRenderInfo {
  sheetUrl: string;
  sheetW: number;
  sheetH: number;
  x: number;
  y: number;
  w: number;
  h: number;
}

export function resolveSprite(
  game: string,
  value: string,
): SpriteRenderInfo | null {
  const atlas = getCachedAtlas(game);
  if (!atlas) return null;
  const entry = atlas.byValue[value];
  if (!entry) return null;
  return {
    sheetUrl: `${baseUrl()}/${game}/${atlas.sheet.image}`,
    sheetW: atlas.sheet.width,
    sheetH: atlas.sheet.height,
    x: entry.x,
    y: entry.y,
    w: entry.w,
    h: entry.h,
  };
}

// Helper to build inline style object for Svelte (optional)
export function spriteStyle(info: SpriteRenderInfo): Record<string, string> {
  // Use hyphenated CSS property names because we stringify this object
  // into a style attribute, not a DOMStyle object.
  return {
    width: info.w + "px",
    height: info.h + "px",
    "background-image": `url(${info.sheetUrl})`,
    "background-position": `-${info.x}px -${info.y}px`,
    "background-repeat": "no-repeat",
    "image-rendering": "pixelated",
    "background-size": `${info.sheetW}px ${info.sheetH}px`,
  };
}
