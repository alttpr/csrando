import { Buffer } from "buffer";
import { pathToFileURL } from "url";
import { Zelda1SpriteDataBlock } from "./rdc-types";

export interface Zelda1AssetWrite {
  offset: string | number | Array<string | number>;
  length: number;
  base64: string;
}

export interface Zelda1SpriteAsset {
  name?: string;
  category?: string;
  creator?: string;
  originalBy?: string;
  writes: Zelda1AssetWrite[];
}

interface Zelda1SegmentLayout {
  addresses: number[];
  length: number;
}

interface NormalizedAssetWrite {
  offsets: number[];
  length: number;
  data: Buffer;
  index: number;
  raw: Zelda1AssetWrite;
  used: boolean;
}

export interface Zelda1AssetExtraction {
  buffers: Buffer[];
  unusedWrites: Zelda1AssetWrite[];
}

const HEX_PREFIX = "0x";

const DEFAULT_ZELDA1_SEGMENT_LAYOUT: Zelda1SegmentLayout[] = [
  { addresses: [0x608e34], length: 32 },
  { addresses: [0x608eb4], length: 32 },
  { addresses: [0x61007f], length: 448 },
  { addresses: [0x6105bf], length: 32 },
  { addresses: [0x6105ff], length: 64 },
  { addresses: [0x61067f], length: 32 },
  {
    addresses: [
      0x631314, 0x631410, 0x63150c, 0x631608, 0x631704, 0x631800, 0x6318fc,
      0x6319f8, 0x631af4, 0x631bf0, 0x631cec, 0x3d3804, 0x793804, 0x7cb804,
    ],
    length: 3,
  },
  { addresses: [0x631cf0], length: 3 },
  { addresses: [0x631cf4], length: 3 },
  { addresses: [0x612287, 0x794325, 0x7cc325], length: 3 },
];

function buildZeldaSegmentLayout(): Zelda1SegmentLayout[] {
  try {
    const block = new Zelda1SpriteDataBlock();
    const manifest = (block as unknown as { manifest?: unknown }).manifest;
    if (Array.isArray(manifest)) {
      return manifest.map((entry) => {
        const [addresses, length] = entry as [number[], number, number[]];
        return { addresses, length } satisfies Zelda1SegmentLayout;
      });
    }
  } catch {
    /* ignore and fall back */
  }
  return DEFAULT_ZELDA1_SEGMENT_LAYOUT;
}

const SEGMENT_LAYOUT: Zelda1SegmentLayout[] = buildZeldaSegmentLayout();

function normalizeOffset(value: string | number): number {
  if (typeof value === "number" && Number.isFinite(value)) {
    return Math.trunc(value);
  }
  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed) {
      throw new Error(
        "Encountered empty offset string while normalizing Zelda 1 asset data.",
      );
    }
    const radix = trimmed.toLowerCase().startsWith(HEX_PREFIX) ? 16 : 10;
    const parsed = Number.parseInt(trimmed.replace(/_/g, ""), radix);
    if (Number.isNaN(parsed)) {
      throw new Error(
        `Unable to parse offset value "${value}" from Zelda 1 asset data.`,
      );
    }
    return parsed;
  }
  throw new Error(`Unsupported offset value type: ${typeof value}`);
}

function normalizeOffsets(
  value: string | number | Array<string | number>,
): number[] {
  const arr = Array.isArray(value) ? value : [value];
  return arr.map((entry) => normalizeOffset(entry));
}

function canonicalize(addresses: number[]): string {
  return addresses
    .slice()
    .sort((a, b) => a - b)
    .map((addr) => addr.toString(16))
    .join(",");
}

function normalizeWrite(
  write: Zelda1AssetWrite,
  index: number,
): NormalizedAssetWrite {
  const offsets = normalizeOffsets(write.offset);
  const data = Buffer.from(write.base64, "base64");
  const expectedLength = write.length ?? data.length;
  if (data.length !== expectedLength) {
    throw new Error(
      `Base64 payload length mismatch for Zelda 1 asset write at index ${index}. Expected ${expectedLength} bytes, got ${data.length}.`,
    );
  }
  return {
    offsets,
    length: expectedLength,
    data,
    index,
    raw: write,
    used: false,
  };
}

function findMatchingWrite(
  writes: NormalizedAssetWrite[],
  segment: Zelda1SegmentLayout,
): NormalizedAssetWrite | undefined {
  const canonicalTarget = canonicalize(segment.addresses);

  // Exact match (all addresses align)
  const exact = writes.find(
    (w) =>
      !w.used &&
      w.length === segment.length &&
      canonicalize(w.offsets) === canonicalTarget,
  );
  if (exact) {
    return exact;
  }

  // Match on any shared address with expected length
  for (const addr of segment.addresses) {
    const partial = writes.find(
      (w) => !w.used && w.length === segment.length && w.offsets.includes(addr),
    );
    if (partial) {
      return partial;
    }
  }

  // Fallback: single candidate with matching length remaining
  const lengthOnlyMatches = writes.filter(
    (w) => !w.used && w.length === segment.length,
  );
  if (lengthOnlyMatches.length === 1) {
    return lengthOnlyMatches[0];
  }
  return undefined;
}

export async function loadZelda1AssetModule(
  filePath: string,
): Promise<Zelda1SpriteAsset[]> {
  const moduleUrl = pathToFileURL(filePath).href;
  let mod;
  try {
    mod = await import(moduleUrl);
  } catch (err) {
    throw new Error(
      `Failed to import Zelda 1 asset module at ${filePath}: ${(err as Error).message}`,
    );
  }
  const exported = (mod && (mod.default ?? mod.assets)) as unknown;
  if (!Array.isArray(exported)) {
    throw new Error(
      `Expected default export array from Zelda 1 asset module ${filePath}.`,
    );
  }
  return exported as Zelda1SpriteAsset[];
}

export function extractZelda1SegmentsFromAsset(
  asset: Zelda1SpriteAsset,
): Zelda1AssetExtraction {
  if (!asset || !Array.isArray(asset.writes)) {
    throw new Error("Malformed Zelda 1 asset: missing writes array.");
  }
  const normalizedWrites = asset.writes.map((write, idx) =>
    normalizeWrite(write, idx),
  );
  const buffers: Buffer[] = [];

  for (const segment of SEGMENT_LAYOUT) {
    const match = findMatchingWrite(normalizedWrites, segment);
    if (!match) {
      const addrSummary = segment.addresses
        .map((addr) => `0x${addr.toString(16)}`)
        .join(", ");
      throw new Error(
        `Could not locate data segment for offsets [${addrSummary}] (length ${segment.length}) in asset "${asset.name ?? "(unnamed)"}".`,
      );
    }
    buffers.push(match.data);
    match.used = true;
  }

  const unusedWrites = normalizedWrites
    .filter((write) => !write.used)
    .map((write) => write.raw);

  return { buffers, unusedWrites };
}
