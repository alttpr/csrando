import { Buffer } from "buffer";
import { pathToFileURL } from "url";
import { Metroid1SpriteDataBlock } from "./rdc-types";

export interface MetroidAssetWrite {
  offset: string | number | Array<string | number>;
  length: number;
  base64: string;
}

export interface MetroidSpriteAsset {
  name?: string;
  category?: string;
  creator?: string;
  originalBy?: string;
  writes: MetroidAssetWrite[];
}

interface MetroidSegmentLayout {
  addresses: number[];
  length: number;
}

interface NormalizedMetroidWrite {
  offsets: number[];
  length: number;
  data: Buffer;
  raw: MetroidAssetWrite;
  used: boolean;
  index: number;
}

export interface MetroidAssetExtraction {
  buffers: Buffer[];
  unusedWrites: MetroidAssetWrite[];
}

const DEFAULT_METROID_SEGMENT_LAYOUT: MetroidSegmentLayout[] = [
  { addresses: [0x6b0000], length: 64 },
  { addresses: [0x6b0050], length: 80 },
  { addresses: [0x6b00b0], length: 64 },
  { addresses: [0x6b0170], length: 16 },
  { addresses: [0x6b0190], length: 96 },
  { addresses: [0x6b0200], length: 64 },
  { addresses: [0x6b0250], length: 48 },
  { addresses: [0x6b0290], length: 96 },
  { addresses: [0x6b0310], length: 96 },
  { addresses: [0x6b0390], length: 16 },
  { addresses: [0x6b03b0], length: 32 },
  { addresses: [0x6b0400], length: 96 },
  { addresses: [0x6b0490], length: 48 },
  { addresses: [0x6b0500], length: 112 },
  { addresses: [0x6b0600], length: 112 },
  { addresses: [0x6b0690], length: 16 },
  { addresses: [0x6b0720], length: 32 },
  { addresses: [0x6b0770], length: 64 },
  { addresses: [0x68a285], length: 3 },
  { addresses: [0x68a298], length: 2 },
  { addresses: [0x68a29e], length: 2 },
  { addresses: [0x68a2a4], length: 2 },
  { addresses: [0x68a2aa], length: 2 },
];

function buildMetroidSegmentLayout(): MetroidSegmentLayout[] {
  try {
    const block = new Metroid1SpriteDataBlock();
    const manifest = (block as unknown as { manifest?: unknown }).manifest;
    if (Array.isArray(manifest)) {
      return manifest.map((entry, idx) => {
        const [addresses, length] = entry as [number[], number, number[]];
        if (!addresses || addresses.length === 0) {
          return {
            addresses: [
              DEFAULT_METROID_SEGMENT_LAYOUT[idx]?.addresses?.[0] ?? idx,
            ],
            length,
          } satisfies MetroidSegmentLayout;
        }
        return { addresses, length } satisfies MetroidSegmentLayout;
      });
    }
  } catch {
    /* ignore */
  }
  return DEFAULT_METROID_SEGMENT_LAYOUT;
}

const SEGMENT_LAYOUT: MetroidSegmentLayout[] = buildMetroidSegmentLayout();

function normalizeOffset(value: string | number): number {
  if (typeof value === "number" && Number.isFinite(value)) {
    return Math.trunc(value);
  }
  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed)
      throw new Error(
        "Encountered empty offset while normalizing Metroid asset data.",
      );
    const radix =
      trimmed.startsWith("0x") || trimmed.startsWith("0X") ? 16 : 10;
    const parsed = Number.parseInt(trimmed.replace(/_/g, ""), radix);
    if (Number.isNaN(parsed)) {
      throw new Error(
        `Unable to parse offset "${value}" from Metroid asset data.`,
      );
    }
    return parsed;
  }
  throw new Error(
    `Unsupported offset type for Metroid asset data: ${typeof value}`,
  );
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
  write: MetroidAssetWrite,
  index: number,
): NormalizedMetroidWrite {
  const offsets = normalizeOffsets(write.offset);
  const data = Buffer.from(write.base64, "base64");
  const expectedLength = write.length ?? data.length;
  if (data.length !== expectedLength) {
    throw new Error(
      `Base64 payload length mismatch for Metroid asset write at index ${index}. Expected ${expectedLength} bytes, got ${data.length}.`,
    );
  }
  return {
    offsets,
    length: expectedLength,
    data,
    raw: write,
    used: false,
    index,
  };
}

function findMatchingWrite(
  writes: NormalizedMetroidWrite[],
  segment: MetroidSegmentLayout,
): NormalizedMetroidWrite | undefined {
  const canonicalTarget = canonicalize(segment.addresses);

  const exact = writes.find(
    (w) =>
      !w.used &&
      w.length === segment.length &&
      canonicalize(w.offsets) === canonicalTarget,
  );
  if (exact) return exact;

  for (const addr of segment.addresses) {
    const partial = writes.find(
      (w) => !w.used && w.length === segment.length && w.offsets.includes(addr),
    );
    if (partial) return partial;
  }

  const lengthOnly = writes.filter(
    (w) => !w.used && w.length === segment.length,
  );
  if (lengthOnly.length === 1) return lengthOnly[0];
  if (lengthOnly.length > 1) return lengthOnly[0];
  return undefined;
}

export async function loadMetroidAssetModule(
  filePath: string,
): Promise<MetroidSpriteAsset[]> {
  const moduleUrl = pathToFileURL(filePath).href;
  let mod;
  try {
    mod = await import(moduleUrl);
  } catch (err) {
    throw new Error(
      `Failed to import Metroid asset module at ${filePath}: ${(err as Error).message}`,
    );
  }
  const exported = (mod && (mod.default ?? mod.assets)) as unknown;
  if (!Array.isArray(exported)) {
    throw new Error(
      `Expected default export array from Metroid asset module ${filePath}.`,
    );
  }
  return exported as MetroidSpriteAsset[];
}

export function extractMetroidSegmentsFromAsset(
  asset: MetroidSpriteAsset,
): MetroidAssetExtraction {
  if (!asset || !Array.isArray(asset.writes)) {
    throw new Error("Malformed Metroid asset: missing writes array.");
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
