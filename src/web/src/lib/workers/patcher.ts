import { applyPatch } from "$lib/patch/apply";
import gameStaticInfo from "$lib/game-static-info.json";

// ROM Patcher Web Worker

function applyBinarySlice(
  romBuffer: ArrayBuffer,
  patchData: ArrayBuffer,
  targetRomOffset: number,
  sourcePatchOffset: number = 0,
  bytesToCopy?: number,
): void {
  const romView = new Uint8Array(romBuffer);
  const patchView = new Uint8Array(patchData);

  const length = bytesToCopy ?? patchData.byteLength - sourcePatchOffset;

  if (targetRomOffset < 0 || targetRomOffset + length > romView.byteLength) {
    console.error(
      `Sprite patch error: Target offset ${targetRomOffset} or length ${length} is out of bounds for ROM size ${romView.byteLength}.`,
    );
    // Decide if to throw or just log and skip. For now, log and skip.
    return;
  }
  if (
    sourcePatchOffset < 0 ||
    sourcePatchOffset + length > patchView.byteLength
  ) {
    console.error(
      `Sprite patch error: Source offset ${sourcePatchOffset} or length ${length} is out of bounds for patch data size ${patchView.byteLength}.`,
    );
    return; // Log and skip
  }

  for (let i = 0; i < length; i++) {
    romView[targetRomOffset + i] = patchView[sourcePatchOffset + i];
  }
}

function readNullTermAscii(
  view: DataView,
  start: number,
  max: number,
): { text: string; length: number } {
  let offs = start;
  const bytes: number[] = [];
  while (offs < max) {
    const b = view.getUint8(offs);
    offs++;
    if (b === 0) break;
    bytes.push(b);
  }
  const text = String.fromCharCode(...bytes);
  return { text, length: offs - start };
}

function parseRdcOffsets(buffer: ArrayBuffer): {
  author: string;
  offsets: Map<number, number>;
} {
  const bytes = new Uint8Array(buffer);
  const header = "RETRODATACONTAINER";
  for (let i = 0; i < header.length; i++) {
    if (i >= bytes.length || bytes[i] !== header.charCodeAt(i)) {
      throw new Error("Invalid RDC header");
    }
  }
  const view = new DataView(buffer);
  let cursor = header.length;
  const version = view.getUint8(cursor);
  cursor += 1;
  if (version !== 1) throw new Error(`Unsupported RDC version ${version}`);
  const numBlocks = view.getUint32(cursor, true);
  cursor += 4;
  const offsets = new Map<number, number>();
  for (let i = 0; i < numBlocks; i++) {
    const type = view.getUint32(cursor, true);
    cursor += 4;
    const off = view.getUint32(cursor, true);
    cursor += 4;
    offsets.set(type, off);
  }
  const { text: author } = readNullTermAscii(view, cursor, bytes.length);
  return { author, offsets };
}

function copyBytes(
  dst: Uint8Array,
  src: Uint8Array,
  dstOffset: number,
  srcOffset: number,
  length: number,
) {
  // Assumes caller already ensured capacity; this is a low-level copy.
  dst.set(src.subarray(srcOffset, srcOffset + length), dstOffset);
}

function parseNumeric(value: unknown): number | null {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }
  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed) {
      return null;
    }
    const lower = trimmed.toLowerCase();
    if (lower.startsWith("snes:") || lower.startsWith("pc:")) {
      const colon = trimmed.indexOf(":");
      return parseNumeric(trimmed.slice(colon + 1));
    }
    const isHex = lower.startsWith("0x");
    const withoutPrefix = isHex ? lower.slice(2) : lower;
    const parsed = Number.parseInt(withoutPrefix, isHex ? 16 : 10);
    return Number.isNaN(parsed) ? null : parsed;
  }
  return null;
}

type RdcAddressRef = {
  address: number;
  addressType: "pc" | "snes";
  applyBaseOffset?: boolean;
};

type RdcResolvedSegment = {
  pcAddress: number;
  applyBaseOffset: boolean;
};

function normalizeSegmentConfig(value: unknown): RdcAddressRef | null {
  if (value === null || value === undefined) return null;

  if (typeof value === "number") {
    return { address: value, addressType: "pc" };
  }

  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed) return null;
    const lower = trimmed.toLowerCase();
    if (lower.startsWith("snes:")) {
      const addr = parseNumeric(trimmed.slice(trimmed.indexOf(":") + 1));
      if (addr === null) return null;
      return { address: addr, addressType: "snes" };
    }
    if (lower.startsWith("pc:")) {
      const addr = parseNumeric(trimmed.slice(trimmed.indexOf(":") + 1));
      if (addr === null) return null;
      return { address: addr, addressType: "pc" };
    }
    const addr = parseNumeric(trimmed);
    if (addr === null) return null;
    return { address: addr, addressType: "pc" };
  }

  if (typeof value === "object") {
    const obj = value as Record<string, unknown>;
    const hasSnes = Object.prototype.hasOwnProperty.call(obj, "snes");
    const hasPc = Object.prototype.hasOwnProperty.call(obj, "pc");
    const hasAddress = Object.prototype.hasOwnProperty.call(obj, "address");
    let address: number | null = null;
    let addressType: "pc" | "snes" = "pc";
    if (hasSnes) {
      address = parseNumeric(obj.snes);
      addressType = "snes";
    } else if (hasPc) {
      address = parseNumeric(obj.pc);
      addressType = "pc";
    } else if (hasAddress) {
      address = parseNumeric(obj.address);
      const typeRaw = typeof obj.addressType === "string" ? obj.addressType.toLowerCase() : undefined;
      addressType = typeRaw === "snes" ? "snes" : "pc";
    }
    if (address === null) return null;
    const applyBaseOffsetRaw = obj.applyBaseOffset;
    const absoluteRaw = obj.absolute;
    let applyBaseOffset: boolean | undefined;
    if (typeof applyBaseOffsetRaw === "boolean") {
      applyBaseOffset = applyBaseOffsetRaw;
    } else if (typeof absoluteRaw === "boolean") {
      applyBaseOffset = !absoluteRaw;
    }
    return { address, addressType, applyBaseOffset };
  }

  return null;
}

function isPlainRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function getRdcTargetConfig(
  gameId: string,
  spriteKind: string,
  randomizerId?: string,
): Record<string, unknown> | undefined {
  const info = (gameStaticInfo as Record<string, any>)[gameId];
  const perKind = (info?.rdcTargets as Record<string, any> | undefined)?.[
    spriteKind
  ];
  if (!perKind) return undefined;

  const defaultConfig = isPlainRecord(perKind.default)
    ? (perKind.default as Record<string, unknown>)
    : undefined;

  const randomizerKey = randomizerId?.toLowerCase();
  if (!randomizerKey) return defaultConfig;

  const specificRaw = perKind[randomizerKey];
  const specificConfig = isPlainRecord(specificRaw)
    ? (specificRaw as Record<string, unknown>)
    : undefined;

  if (defaultConfig) {
    return specificConfig ? { ...defaultConfig, ...specificConfig } : defaultConfig;
  }

  return specificConfig;
}

function resolveRdcSegmentTargets(
  gameId: string,
  spriteKind: string,
  randomizerId: string | undefined,
  requiredSegments: readonly string[],
): Map<string, RdcResolvedSegment> {
  const config = getRdcTargetConfig(gameId, spriteKind, randomizerId);
  if (!config) {
    throw new Error(
      `Missing RDC target configuration for '${spriteKind}' on game '${gameId}'.`,
    );
  }

  const configRecord = config as Record<string, unknown>;
  const resolved = new Map<string, RdcResolvedSegment>();

  for (const segment of requiredSegments) {
    const ref = normalizeSegmentConfig(configRecord[segment]);
    if (!ref) {
      throw new Error(
        `Missing RDC segment '${segment}' for '${spriteKind}' on game '${gameId}'.`,
      );
    }
    const pcAddress =
      ref.addressType === "snes" ? snesToPc(ref.address) : ref.address;
    const applyBaseOffset = ref.applyBaseOffset !== false;
    resolved.set(segment, { pcAddress, applyBaseOffset });
  }

  return resolved;
}

function computeSegmentTarget(
  resolved: Map<string, RdcResolvedSegment>,
  segment: string,
  baseOffset: number,
): number {
  const entry = resolved.get(segment);
  if (!entry) {
    throw new Error(`Missing resolved RDC segment '${segment}'.`);
  }
  return entry.pcAddress + (entry.applyBaseOffset ? baseOffset : 0);
}

type RdcApplyOptions = {
  gameId: string;
  randomizerId?: string;
  baseOffset?: number;
  spriteKind?: string;
};

type RdcManifestSegment = {
  addresses: number[];
  addressType: "snes" | "pc";
  length: number;
  entries: number;
  entryStride: number;
  entryOffsets?: number[];
  applyBaseOffset: boolean;
};

function snesToPcHiRom(snesAddr: number): number {
  return snesAddr & 0x3fffff;
}

function snesToPcByMapping(snesAddr: number, mapping: string): number {
  const mode = mapping?.toLowerCase?.() ?? "lorom";
  if (mode === "hirom" || mode === "exhirom") {
    return snesToPcHiRom(snesAddr);
  }
  return snesToPc(snesAddr);
}

function resolveAddressesForMapping(
  raw: unknown,
  mapping: string,
): { addresses: number[]; addressType: "snes" | "pc" } | null {
  if (raw === null || raw === undefined) return null;

  let source: unknown = raw;
  if (isPlainRecord(raw)) {
    const record = raw as Record<string, unknown>;
    const entries = Object.entries(record);
    const lowerMapping = mapping.toLowerCase();
    let selected = entries.find(([key]) => key.toLowerCase() === lowerMapping)?.[1];
    if (selected === undefined) {
      selected = entries.find(([key]) => key.toLowerCase() === "lorom")?.[1];
    }
    if (selected === undefined) {
      selected = entries.find(([key]) => key.toLowerCase() === "default")?.[1];
    }
    if (selected === undefined && entries.length > 0) {
      selected = entries[0][1];
    }
    source = selected;
  }

  if (source === undefined) return null;

  const list = Array.isArray(source) ? source : [source];
  const addresses: number[] = [];
  let addressType: "snes" | "pc" = "snes";

  for (const item of list) {
    if (typeof item === "number") {
      addresses.push(Math.trunc(item));
      continue;
    }
    if (typeof item === "string") {
      let text = item.trim();
      if (!text) continue;
      let explicitType: "snes" | "pc" | undefined;
      const lower = text.toLowerCase();
      if (lower.startsWith("pc:")) {
        explicitType = "pc";
        text = text.slice(3);
      } else if (lower.startsWith("snes:")) {
        explicitType = "snes";
        text = text.slice(5);
      }
      const parsed = parseNumeric(text);
      if (parsed === null) continue;
      addresses.push(Math.trunc(parsed));
      if (explicitType) addressType = explicitType;
    }
  }

  if (addresses.length === 0) return null;
  return { addresses, addressType };
}

function resolveManifestSegments(
  segmentsRaw: unknown,
  mapping: string,
  defaultApplyBaseOffset: boolean,
): RdcManifestSegment[] {
  if (!Array.isArray(segmentsRaw)) return [];
  const resolved: RdcManifestSegment[] = [];

  for (const entry of segmentsRaw) {
    if (!isPlainRecord(entry)) continue;
    const record = entry as Record<string, unknown>;
    const lengthVal = parseNumeric(record.length) ?? null;
    if (lengthVal === null) continue;
    const length = Math.max(0, Math.trunc(lengthVal));
    if (length === 0) continue;

    const entriesVal = parseNumeric(record.entries ?? 1) ?? 1;
    const entries = Math.max(1, Math.trunc(entriesVal));

    const entryStrideVal = parseNumeric(
      record.entryStride ?? record.entryStep ?? record.offset ?? 0,
    ) ?? 0;
    const entryStride = Math.trunc(entryStrideVal);

    let entryOffsets: number[] | undefined;
    const entryOffsetsRaw = record.entryOffsets ?? record.offsets;
    if (Array.isArray(entryOffsetsRaw)) {
      entryOffsets = entryOffsetsRaw
        .map((v) => parseNumeric(v) ?? 0)
        .map((v) => Math.trunc(v));
    }

    const addressInfo = resolveAddressesForMapping(record.addresses, mapping);
    if (!addressInfo) continue;

    const explicitType =
      typeof record.addressType === "string"
        ? record.addressType.toLowerCase() === "pc"
          ? "pc"
          : "snes"
        : undefined;

    const applyBaseOffset =
      typeof record.applyBaseOffset === "boolean"
        ? record.applyBaseOffset
        : defaultApplyBaseOffset;

    resolved.push({
      addresses: addressInfo.addresses,
      addressType: explicitType ?? addressInfo.addressType,
      length,
      entries,
      entryStride,
      entryOffsets,
      applyBaseOffset,
    });
  }

  return resolved;
}

function applySamusManifestSegments(
  romU8: Uint8Array,
  rdcU8: Uint8Array,
  samusOffset: number,
  segments: RdcManifestSegment[],
  mapping: string,
  baseOffset: number,
  gameId: string,
) {
  let cursor = samusOffset;

  for (const segment of segments) {
    const totalLength = segment.length * segment.entries;
    if (cursor + totalLength > rdcU8.length) {
      throw new Error(
        `[SamusRDC] Segment data truncated for ${gameId}; expected ${totalLength} bytes at ${cursor}, have ${rdcU8.length - cursor}.`,
      );
    }
    const segmentData = rdcU8.subarray(cursor, cursor + totalLength);
    cursor += totalLength;

    for (const baseAddress of segment.addresses) {
      for (let entryIndex = 0; entryIndex < segment.entries; entryIndex++) {
        const srcOffset = entryIndex * segment.length;
        const offsetValue =
          segment.entryOffsets && segment.entryOffsets.length > 0
            ? segment.entryOffsets[
            Math.min(entryIndex, segment.entryOffsets.length - 1)
            ] ?? 0
            : segment.entryStride * entryIndex;

        let destPc: number;
        if (segment.addressType === "pc") {
          destPc = baseAddress + offsetValue;
        } else {
          destPc = snesToPcByMapping(baseAddress + offsetValue, mapping);
        }
        if (segment.applyBaseOffset) destPc += baseOffset;

        if (destPc < 0 || destPc + segment.length > romU8.length) {
          console.error(
            `[SamusRDC] target out of bounds`,
            {
              gameId,
              destination: destPc,
              segmentLength: segment.length,
            },
          );
          continue;
        }

        copyBytes(romU8, segmentData, destPc, srcOffset, segment.length);
      }
    }
  }
}

function snesToPc(snesAddr: number): number {
  if (snesAddr < 0x8000) {
    throw new Error(`Invalid SNES address: ${snesAddr.toString(16)}`);
  }
  return (snesAddr & 0x7fff) | ((snesAddr & 0x7f0000) >> 1);
}

async function applyLinkRdc(
  rom: ArrayBuffer,
  rdcBuf: ArrayBuffer,
  options: RdcApplyOptions = { gameId: "alttp" },
): Promise<ArrayBuffer> {
  const {
    gameId = "alttp",
    randomizerId,
    baseOffset = 0,
    spriteKind,
  } = options || {};
  const { offsets } = parseRdcOffsets(rdcBuf);
  const linkDataOffset = offsets.get(1 /* LinkSprite */);
  if (linkDataOffset === undefined)
    throw new Error("RDC does not contain LinkSprite data");
  const workingRom = rom;
  const romU8 = new Uint8Array(workingRom);
  const rdcU8 = new Uint8Array(rdcBuf);

  // Centralized layout constants (must match tools/sprite-importer LinkSprite manifest)
  const LINK_GFX_LEN = 0x7000;
  const LINK_PALETTE_LEN = 4 * 30; // 4 palettes * 30 bytes
  const LINK_GLOVES_LEN = 0x04;
  const TOTAL_MIN_LEN = LINK_GFX_LEN + LINK_PALETTE_LEN + LINK_GLOVES_LEN;

  // Validate RDC slice length before copying to avoid writing garbage / crashing ROM
  if (linkDataOffset + TOTAL_MIN_LEN > rdcU8.length) {
    throw new Error(
      `Link RDC block truncated: need at least ${TOTAL_MIN_LEN} bytes starting at ${linkDataOffset}, have ${rdcU8.length - linkDataOffset}`,
    );
  }

  const gfxSrc = linkDataOffset;
  const palSrc = gfxSrc + LINK_GFX_LEN;
  const glvSrc = palSrc + LINK_PALETTE_LEN;
  const resolved = resolveRdcSegmentTargets(
    gameId,
    (spriteKind ?? "rdc/link").toLowerCase(),
    randomizerId,
    ["gfx", "palette", "gloves"],
  );
  const gfxDst = computeSegmentTarget(resolved, "gfx", baseOffset);
  const palDst = computeSegmentTarget(resolved, "palette", baseOffset);
  const glvDst = computeSegmentTarget(resolved, "gloves", baseOffset);

  // Optional debug (can be toggled later with an env flag)
  // Allow an optional debug flag on the worker global without using 'any'
  const dbgSelf = self as unknown as { DEBUG_LINK_RDC?: boolean };
  if (typeof console !== "undefined" && dbgSelf.DEBUG_LINK_RDC) {
    console.debug("[LinkRDC] copy plan", {
      gfx: { src: gfxSrc, dst: gfxDst, len: LINK_GFX_LEN },
      palette: { src: palSrc, dst: palDst, len: LINK_PALETTE_LEN },
      gloves: { src: glvSrc, dst: glvDst, len: LINK_GLOVES_LEN },
    });
  }

  copyBytes(romU8, rdcU8, gfxDst, gfxSrc, LINK_GFX_LEN);
  copyBytes(romU8, rdcU8, palDst, palSrc, LINK_PALETTE_LEN);
  copyBytes(romU8, rdcU8, glvDst, glvSrc, LINK_GLOVES_LEN);
  return workingRom;
}

async function applyNesRdc(
  rom: ArrayBuffer,
  rdcBuf: ArrayBuffer,
  gameId: "zelda1" | "metroid1",
  options: RdcApplyOptions = { gameId },
): Promise<ArrayBuffer> {
  const { randomizerId, baseOffset = 0, spriteKind } = options || {};
  const { offsets } = parseRdcOffsets(rdcBuf);
  const typeId =
    gameId === "zelda1"
      ? 2 /* Zelda1SpriteDataBlock */
      : 3; /* Metroid1SpriteDataBlock */
  const blockOffset = offsets.get(typeId);
  if (blockOffset === undefined)
    throw new Error("RDC does not contain NES sprite data");

  const workingRom = rom;
  const romU8 = new Uint8Array(workingRom);
  const rdcU8 = new Uint8Array(rdcBuf);

  const gfxLen = gameId === "zelda1" ? 0x1000 : 0x2000;
  const palLen = 0x20;
  const gfxSrc = blockOffset;
  const palSrc = gfxSrc + gfxLen;
  const resolved = resolveRdcSegmentTargets(
    gameId,
    (spriteKind ?? (gameId === "zelda1" ? "rdc/nes-z1" : "rdc/nes-m1")).toLowerCase(),
    randomizerId,
    ["gfx", "palette"],
  );

  const gfxDst = computeSegmentTarget(resolved, "gfx", baseOffset);
  const palDst = computeSegmentTarget(resolved, "palette", baseOffset);

  // Bounds-safe copies (clamped)
  copyBytes(
    romU8,
    rdcU8,
    Math.min(gfxDst, Math.max(0, romU8.length - gfxLen)),
    gfxSrc,
    Math.min(gfxLen, romU8.length),
  );
  copyBytes(
    romU8,
    rdcU8,
    Math.min(palDst, Math.max(0, romU8.length - palLen)),
    palSrc,
    Math.min(palLen, romU8.length),
  );
  return workingRom;
}

async function applySamusRdc(
  rom: ArrayBuffer,
  rdcBuf: ArrayBuffer,
  options: RdcApplyOptions = { gameId: "supermetroid" },
): Promise<ArrayBuffer> {
  const {
    gameId = "supermetroid",
    randomizerId,
    baseOffset = 0,
    spriteKind,
  } = options || {};
  const { offsets } = parseRdcOffsets(rdcBuf);
  const samusOffset = offsets.get(4 /* SamusSprite */);
  if (samusOffset === undefined)
    throw new Error("RDC does not contain SamusSprite data");

  const workingRom = rom;
  const romU8 = new Uint8Array(workingRom);
  const rdcU8 = new Uint8Array(rdcBuf);

  const spriteKindKey = (spriteKind ?? "rdc/samus").toLowerCase();
  const config = getRdcTargetConfig(gameId, spriteKindKey, randomizerId);
  const configRecord = config as Record<string, unknown> | undefined;
  const mappingValue =
    typeof configRecord?.mapping === "string"
      ? configRecord.mapping
      : "lorom";
  const manifestApplyBaseOffset =
    typeof configRecord?.applyBaseOffset === "boolean"
      ? configRecord.applyBaseOffset
      : true;
  const manifestSegmentsRaw = configRecord?.segments;

  if (!Array.isArray(manifestSegmentsRaw)) {
    throw new Error(
      `Missing 'segments' manifest for '${spriteKindKey}' on game '${gameId}'.`,
    );
  }

  const manifestSegments = resolveManifestSegments(
    manifestSegmentsRaw,
    mappingValue,
    manifestApplyBaseOffset,
  );

  if (manifestSegments.length === 0) {
    throw new Error(
      `Empty 'segments' manifest for '${spriteKindKey}' on game '${gameId}'.`,
    );
  }

  applySamusManifestSegments(
    romU8,
    rdcU8,
    samusOffset,
    manifestSegments,
    mappingValue,
    baseOffset,
    gameId,
  );
  return workingRom;
}

import type {
  GameSpriteConfig,
  GamePostGenConfig,
  PostGenSetting,
  PostGenPatchEntry,
} from "$lib/types";
import { slugifyForFilename } from "$lib/utils/options-summary";
import { randomize as z3prRandomize } from "@maseya/z3pr";

type MaybeMapOrRecord<T> = Map<string, T> | Record<string, T> | undefined;

function getFromMaybe<T>(src: MaybeMapOrRecord<T>, key: string): T | undefined {
  if (!src) return undefined;
  if (src instanceof Map) return src.get(key);
  return (src as Record<string, T>)[key];
}

function hexStringToBytes(s: string): Uint8Array {
  // Accept formats like "FF00AA", "FF 00 AA", "0xFF,0x00,0xAA"
  const cleaned = s
    .replace(/0x/gi, "")
    .replace(/[^0-9a-fA-F]/g, "")
    .toLowerCase();
  if (cleaned.length % 2 !== 0) {
    throw new Error(`Invalid hex byte string length: ${s}`);
  }
  const out = new Uint8Array(cleaned.length / 2);
  for (let i = 0; i < cleaned.length; i += 2) {
    out[i / 2] = parseInt(cleaned.slice(i, i + 2), 16);
  }
  return out;
}

function writeBytes(rom: ArrayBuffer, address: number, data: Uint8Array) {
  const view = new Uint8Array(rom);
  if (address < 0 || address + data.length > view.length) {
    console.error(
      `Post-gen patch write out of bounds: addr=${address} len=${data.length} rom=${view.length}`,
    );
    return;
  }
  view.set(data, address);
}

// --- Seed hashing helper for z3pr ------------------------------------------
function hashSeedToU32(s: string): number {
  // FNV-1a 32-bit
  let h = 0x811c9dc5;
  for (let i = 0; i < s.length; i++) {
    h ^= s.charCodeAt(i);
    h = Math.imul(h, 0x01000193);
  }
  return h >>> 0; // unsigned
}

function resolveFileExtension(
  baseName: string | null | undefined,
  fallback: string,
): string {
  if (!baseName) return fallback;
  const dotIndex = baseName.lastIndexOf(".");
  if (dotIndex <= 0 || dotIndex >= baseName.length - 1) return fallback;
  return baseName.slice(dotIndex);
}

function compactSeedIdentifier(seedSlug: string): string {
  if (!seedSlug) return "";
  if (seedSlug.length <= 16) return seedSlug;
  return `${seedSlug.slice(0, 8)}-${seedSlug.slice(-4)}`;
}

self.onmessage = async (event) => {
  const {
    patchData,
    baseRom,
    additionalRoms,
    primaryGameId,
    outputFileNameDetails,
    randomizerId,
    basePatchUrl,
    basePatchBytes,
    selectedSpritesByGameId,
    gameIdToSpriteInfoMap,
    spritePatchDataContents,
    allGameIds,
    publicSpritesBaseUrl,
    selectedPostGenByGameId,
    gameIdToPostGenConfigMap,
  } = event.data;

  const SPRITES_BASE = (publicSpritesBaseUrl || "/sprites").replace(/\/$/, "");

  try {
    // Progress: Base setup
    self.postMessage({ type: "progress", progress: 5 });

    // Validate inputs
    if (!patchData || !(patchData instanceof ArrayBuffer)) {
      throw new Error("Patch data is missing or not an ArrayBuffer.");
    }
    if (!baseRom || !(baseRom instanceof ArrayBuffer)) {
      throw new Error("Base ROM is missing or not an ArrayBuffer.");
    }

    self.postMessage({ type: "progress", progress: 10 });

    // Prepare base offsets per game (0 by default). If multiple ROMs are supplied and
    // static game data provides targetOffsets for this randomizer, assemble a combined ROM first.
    const randomizerIdLc = String(randomizerId || "").toLowerCase();
    const baseOffsetByGame = new Map<string, number>();
    const sourceLengthByGame = new Map<string, number>();

    let workingBaseRom: ArrayBuffer = baseRom;
    let isCombined = false;

    // Collect provided ROMs (primary + any additional)
    const included = new Map<string, ArrayBuffer>();
    included.set(primaryGameId, baseRom);
    if (additionalRoms) {
      for (const [gid, buf] of Object.entries(additionalRoms)) {
        if (buf instanceof ArrayBuffer) {
          included.set(gid, buf);
        }
      }
    }

    // Attempt to combine if more than one ROM is supplied and targetOffsets exist for this randomizer
    if (included.size > 1) {
      let totalSize = 0;
      let validWithOffsets = 0;
      for (const [gid, buf] of included) {
        const info: any = (gameStaticInfo as Record<string, any>)[gid];
        const offsRaw = info?.targetOffsets?.[randomizerIdLc];
        const offs: number | undefined =
          typeof offsRaw === "number" ? offsRaw : undefined;
        if (offs === undefined || offs < 0) {
          console.warn(
            `Game ${gid} has invalid targetOffsets.${randomizerIdLc} (${String(
              offsRaw,
            )}); skipping from combined ROM`,
          );
          continue;
        }
        validWithOffsets++;
        baseOffsetByGame.set(gid, offs);
        sourceLengthByGame.set(gid, buf.byteLength);
        totalSize = Math.max(totalSize, offs + buf.byteLength);
      }

      if (validWithOffsets >= 2 && totalSize > 0) {
        // Warn if some expected games for this randomizer weren't provided
        try {
          const expectedGames = Object.keys(
            gameStaticInfo as Record<string, any>,
          ).filter((g) => {
            const t = (gameStaticInfo as Record<string, any>)[g]?.targetOffsets?.[
              randomizerIdLc
            ];
            return typeof t === "number" && t >= 0 && g !== "combo";
          });
          const missing = expectedGames.filter((g) => !included.has(g));
          if (missing.length > 0) {
            console.warn(
              `Combined base will be incomplete for randomizer '${randomizerIdLc}'. Missing ROMs: ${missing.join(", ")}`,
            );
          }
        } catch { }

        const combined = new ArrayBuffer(totalSize);
        const combinedU8 = new Uint8Array(combined);
        for (const [gid, buf] of included) {
          const offs = baseOffsetByGame.get(gid);
          if (offs === undefined || offs < 0) continue;
          combinedU8.set(new Uint8Array(buf), offs);
        }
        // Log a brief map of offsets used for debugging
        try {
          const entries: Array<[string, number]> = [];
          for (const [gid, off] of baseOffsetByGame) entries.push([gid, off]);
          entries.sort((a, b) => a[1] - b[1]);
          console.debug(
            `[Patcher] Combined base assembled for '${randomizerIdLc}'. Total ${totalSize} bytes. Offsets: `,
            entries,
          );
        } catch { }
        workingBaseRom = combined;
        isCombined = true;
      }
    }

    // If not combined, default offsets to 0 for provided ROMs
    if (!isCombined) {
      for (const [gid, buf] of included) {
        baseOffsetByGame.set(gid, 0);
        sourceLengthByGame.set(gid, buf.byteLength);
      }
    }

    // Note: BPS patches are always applied to the base (or combined) ROM as a whole.

    // Always attempt to apply a base patch. Prefer explicit bytes/url if provided; otherwise fallback by randomizerId.
    // Mapping can be extended as new randomizers are supported.
    const basePatchMap: Record<string, string> = {
      alttpr: "/alttpr.ips",
      // e.g. 'smz3': '/smz3_base.ips', 'metroid1rando': '/m1_base.ips'
    };

    let basePatchPath = randomizerId
      ? basePatchMap[String(randomizerId).toLowerCase()]
      : undefined;

    if (basePatchUrl) {
      basePatchPath = basePatchUrl; // override mapping with explicit per-seed url
    }

    if (basePatchBytes && basePatchBytes instanceof ArrayBuffer) {
      try {
        self.postMessage({
          type: "progress",
          progress: 12,
          note: "Applying base patch (inline)",
        });
        workingBaseRom = applyPatch(workingBaseRom, basePatchBytes);
        self.postMessage({
          type: "progress",
          progress: 18,
          note: "Base patch applied",
        });
      } catch (e) {
        console.error("Failed applying provided base patch bytes:", e);
      }
    } else if (basePatchPath) {
      try {
        self.postMessage({
          type: "progress",
          progress: 12,
          note: `Fetching base patch (${randomizerId})`,
        });
        const resp = await fetch(basePatchPath);
        if (!resp.ok)
          throw new Error(
            `Failed to fetch base patch ${basePatchPath} (status ${resp.status})`,
          );
        const baseIps = await resp.arrayBuffer();
        self.postMessage({
          type: "progress",
          progress: 15,
          note: "Applying base patch",
        });
        workingBaseRom = applyPatch(workingBaseRom, baseIps);
        self.postMessage({
          type: "progress",
          progress: 18,
          note: "Base patch applied",
        });
      } catch (e) {
        console.error(`Failed applying base patch from ${basePatchPath}:`, e);
        // Fallback to mapping if we were using an explicit URL override
        if (basePatchUrl && randomizerId) {
          const fallback = basePatchMap[String(randomizerId).toLowerCase()];
          if (fallback && fallback !== basePatchPath) {
            try {
              const resp2 = await fetch(fallback);
              if (resp2.ok) {
                const baseIps2 = await resp2.arrayBuffer();
                self.postMessage({
                  type: "progress",
                  progress: 15,
                  note: "Applying base patch",
                });
                workingBaseRom = applyPatch(workingBaseRom, baseIps2);
                self.postMessage({
                  type: "progress",
                  progress: 18,
                  note: "Base patch applied",
                });
              } else {
                console.warn(
                  `Fallback fetch failed for base patch ${fallback} (status ${resp2.status})`,
                );
                // Continue even if base fails; main patch may still partially work.
              }
              // eslint-disable-next-line no-empty
            } catch { }
            // else: nothing to fallback to
          }
        }
      }
    } else {
      console.warn(
        `No base patch mapping found for randomizerId='${randomizerId}'. Proceeding without base patch.`,
      );
    }

    // Progress: Main patch (20% -> 50%)
    self.postMessage({ type: "progress", progress: 20 });

    // Apply the main patch to the (possibly base-patched) ROM
    let patchedRomBuffer: ArrayBuffer;
    try {
      patchedRomBuffer = applyPatch(workingBaseRom, patchData);
    } catch (err) {
      console.error("Main patch application failed:", err);
      // Re-throw to trigger error handling downstream
      throw err;
    }

    if (!patchedRomBuffer || patchedRomBuffer.byteLength === 0) {
      throw new Error("Main patching resulted in an empty or invalid ROM.");
    }
    self.postMessage({ type: "progress", progress: 50 }); // Main IPS patching done

    // Create ROM Buffers Map
    const romBuffers = new Map<string, ArrayBuffer>();
    if (isCombined) {
      // In combo mode, all gameIds share the same combined buffer
      for (const gid of allGameIds as string[]) {
        romBuffers.set(gid, patchedRomBuffer);
      }
    } else {
      romBuffers.set(primaryGameId, patchedRomBuffer); // This is the buffer modified by IPS patch
      if (additionalRoms) {
        for (const [gameId, buffer] of Object.entries(additionalRoms)) {
          // Ensure buffer is ArrayBuffer. Assuming it is from postMessage.
          romBuffers.set(gameId, buffer as ArrayBuffer);
        }
      }
    }
    self.postMessage({ type: "progress", progress: 55 }); // ROM buffers map created

    // Sprite Patching Loop (55% -> 90%)
    let currentSpritePatchGame = 0;
    const totalSpritePatchGames = allGameIds.length;

    for (const gameId of allGameIds) {
      const selectedSpriteValue = selectedSpritesByGameId.get(gameId);
      let romToPatch = romBuffers.get(gameId);
      const baseOffset = baseOffsetByGame.get(gameId) ?? 0;

      if (isCombined && !baseOffsetByGame.has(gameId)) {
        // Not part of the combined image (no offset defined); skip
        currentSpritePatchGame++;
        continue;
      }

      if (!selectedSpriteValue || !romToPatch) {
        currentSpritePatchGame++;
        continue;
      }

      const gameSpriteInfoContainer = (
        gameIdToSpriteInfoMap as Map<string, GameSpriteConfig>
      ).get(gameId);
      if (!gameSpriteInfoContainer || !gameSpriteInfoContainer.sprites) {
        console.warn(
          `Sprite info container not found for game ${gameId}. Skipping sprite patch.`,
        );
        currentSpritePatchGame++;
        continue;
      }

      const spriteConfig = gameSpriteInfoContainer.sprites.find(
        (s: unknown) => (s as { value?: string }).value === selectedSpriteValue,
      );

      if (spriteConfig?.rdcPath) {
        try {
          const url = /^(https?:)?\//.test(spriteConfig.rdcPath)
            ? spriteConfig.rdcPath
            : `${SPRITES_BASE}/${gameId}/${spriteConfig.rdcPath}`;
          const resp = await fetch(url);
          if (!resp.ok)
            throw new Error(`Failed to fetch RDC: ${url} (${resp.status})`);
          const rdcBuf = await resp.arrayBuffer();
          const kind = (spriteConfig.kind || "").toLowerCase();
          const randomizerKey = randomizerIdLc ? randomizerIdLc : undefined;
          let updated: ArrayBuffer | undefined;
          if (kind === "rdc/link") {
            updated = await applyLinkRdc(romToPatch, rdcBuf, {
              gameId,
              baseOffset,
              randomizerId: randomizerKey,
              spriteKind: kind,
            });
          } else if (kind === "rdc/nes-z1") {
            updated = await applyNesRdc(romToPatch, rdcBuf, "zelda1", {
              gameId: "zelda1",
              baseOffset,
              randomizerId: randomizerKey,
              spriteKind: kind,
            });
          } else if (kind === "rdc/nes-m1") {
            updated = await applyNesRdc(romToPatch, rdcBuf, "metroid1", {
              gameId: "metroid1",
              baseOffset,
              randomizerId: randomizerKey,
              spriteKind: kind,
            });
          } else if (kind === "rdc/samus") {
            updated = await applySamusRdc(romToPatch, rdcBuf, {
              gameId,
              baseOffset,
              randomizerId: randomizerKey,
              spriteKind: kind,
            });
          } else {
            // Default to Link-style handling when sprite kind is unspecified
            updated = await applyLinkRdc(romToPatch, rdcBuf, {
              gameId,
              baseOffset,
              randomizerId: randomizerKey,
              spriteKind: kind || "rdc/link",
            });
          }
          if (updated && updated !== romToPatch) {
            romToPatch = updated;
            romBuffers.set(gameId, updated);
          }
        } catch (e) {
          console.error("RDC apply failed:", e);
        }
      } else if (
        spriteConfig &&
        spriteConfig.patchDetails &&
        spriteConfig.patchDetails.patches
      ) {
        for (const patchEntry of spriteConfig.patchDetails.patches) {
          if (!spriteConfig.patchDetails.files) {
            console.error(
              `Patching error: Files configuration is missing for sprite ${selectedSpriteValue} on game ${gameId}.`,
            );
            continue;
          }
          const sourceFileConfig = spriteConfig.patchDetails.files.find(
            (file: unknown) =>
              (file as { id?: string }).id === patchEntry.fileId,
          );
          if (!sourceFileConfig) {
            console.error(
              `Patching error: Invalid fileId ${patchEntry.fileId} for sprite ${selectedSpriteValue} on game ${gameId}.`,
            );
            continue;
          }
          const patchBinary = spritePatchDataContents.get(
            sourceFileConfig.path,
          );

          if (patchBinary) {
            const targetAddress = parseInt(patchEntry.targetAddress, 16);
            if (isNaN(targetAddress)) {
              console.error(
                `Patching error: Invalid targetAddress ${patchEntry.targetAddress} for sprite ${selectedSpriteValue} on game ${gameId}.`,
              );
              continue;
            }
            applyBinarySlice(
              romToPatch,
              patchBinary,
              targetAddress,
              patchEntry.dataOffsetInFile || 0, // Assuming dataOffsetInFile from patchDetails
              patchEntry.dataLength,
            );
          } else {
            console.warn(
              `Sprite patch data not found for ${sourceFileConfig.path} (sprite: ${selectedSpriteValue}, game: ${gameId}). Skipping this patch entry.`,
            );
          }
        }
      }
      currentSpritePatchGame++;
      if (totalSpritePatchGames > 0) {
        self.postMessage({
          type: "progress",
          progress:
            55 +
            Math.round((currentSpritePatchGame / totalSpritePatchGames) * 35), // Sprite patching: 55% to 90%
        });
      }
    }
    self.postMessage({ type: "progress", progress: 90 }); // All sprite patching done

    // Apply post-generation settings (90% -> 96%)
    try {
      let currentPostGenGame = 0;
      const totalPostGenGames = allGameIds.length;
      for (const gameId of allGameIds) {
        const romToPatch = romBuffers.get(gameId);
        const baseOffset = baseOffsetByGame.get(gameId) ?? 0;
        if (isCombined && !baseOffsetByGame.has(gameId)) {
          currentPostGenGame++;
          continue;
        }
        if (!romToPatch) {
          currentPostGenGame++;
          continue;
        }
        const cosmeticSelections = getFromMaybe<
          Record<string, string | boolean>
        >(selectedPostGenByGameId, gameId);
        const postGenConfig: GamePostGenConfig | undefined =
          getFromMaybe<GamePostGenConfig>(gameIdToPostGenConfigMap, gameId);

        if (
          !postGenConfig ||
          !postGenConfig.options ||
          postGenConfig.options.length === 0
        ) {
          currentPostGenGame++;
          continue;
        }

        for (const opt of postGenConfig.options as PostGenSetting[]) {
          let selectedVal: string | boolean | undefined;
          if (
            cosmeticSelections &&
            Object.prototype.hasOwnProperty.call(cosmeticSelections, opt.id)
          ) {
            selectedVal = cosmeticSelections[opt.id];
          } else {
            selectedVal = opt.default;
          }

          const applyPatchEntries = (
            entries: PostGenPatchEntry[] | undefined,
          ) => {
            if (!entries) return;
            for (const e of entries) {
              const addr = parseInt(String(e.targetAddress), 16);
              if (isNaN(addr)) continue;
              let bytes: Uint8Array | null = null;
              if (Array.isArray(e.data)) {
                bytes = new Uint8Array(
                  e.data.map((n: number) => Math.max(0, Math.min(255, n | 0))),
                );
              } else if (typeof e.data === "string") {
                try {
                  bytes = hexStringToBytes(e.data);
                } catch (err) {
                  console.error("Invalid cosmetic hex data", e.data, err);
                }
              }
              if (bytes) writeBytes(romToPatch, addr, bytes);
            }
          };

          if (opt.type === "toggle") {
            const onSel = Boolean(selectedVal);
            // ALTTP palette randomizer via @maseya/z3pr
            if (onSel && gameId === "alttp" && opt.id === "palette_randomize") {
              try {
                const u8 = new Uint8Array(romToPatch);
                const seedNum = hashSeedToU32(
                  String(outputFileNameDetails?.seedId || "seed"),
                );
                const mode = String(
                  cosmeticSelections?.["palette_randomize_mode"] ?? "maseya",
                ) as
                  | "maseya"
                  | "grayscale"
                  | "negative"
                  | "blackout"
                  | "classic"
                  | "dizzy"
                  | "sick"
                  | "puke";
                const z3opts = {
                  mode,
                  randomize_overworld: Boolean(
                    cosmeticSelections?.["palette_randomize_overworld"] ?? true,
                  ),
                  randomize_dungeon: Boolean(
                    cosmeticSelections?.["palette_randomize_dungeon"] ?? true,
                  ),
                  randomize_link_sprite: Boolean(
                    cosmeticSelections?.["palette_randomize_link_sprite"] ?? true,
                  ),
                  randomize_shield: Boolean(
                    cosmeticSelections?.["palette_randomize_shield"] ?? true,
                  ),
                  randomize_hud: Boolean(
                    cosmeticSelections?.["palette_randomize_hud"] ?? true,
                  ),
                  seed: seedNum,
                } as const;
                z3prRandomize(u8, z3opts as unknown as Record<string, unknown>);
              } catch (e) {
                console.error("ALTTP palette randomization failed:", e);
              }
            }
            if (onSel) applyPatchEntries(opt.on?.patches);
            else applyPatchEntries(opt.off?.patches);
          } else if (opt.type === "select") {
            const val =
              typeof selectedVal === "string" ? selectedVal : opt.default;
            const choice = (opt.choices || []).find((c) => c.value === val);
            if (choice) applyPatchEntries(choice.patches);
          } else {
            // Unknown type; skip
          }
        }

        currentPostGenGame++;
        if (totalPostGenGames > 0) {
          self.postMessage({
            type: "progress",
            progress:
              90 + Math.round((currentPostGenGame / totalPostGenGames) * 6),
          });
        }
      }
    } catch (e) {
      console.error("Post-generation settings patching failed:", e);
    }

    // Construct a descriptive filename using outputFileNameDetails
    const {
      seedId,
      baseName,
      defaultDisplayName,
      defaultExtension,
      randomizerVersion,
      optionSummary,
    } = outputFileNameDetails;

    const romFileExt = resolveFileExtension(baseName, defaultExtension);

    const randomizerSlug = slugifyForFilename(String(randomizerId ?? ""), {
      maxLength: 20,
    });
    const versionSlug = randomizerVersion
      ? slugifyForFilename(randomizerVersion, { maxLength: 20 })
      : "";

    const seedSlugFull = slugifyForFilename(seedId, { maxLength: 32 });
    const seedSegment = compactSeedIdentifier(seedSlugFull);

    const summaryTokens = Array.isArray(optionSummary?.tokens)
      ? optionSummary.tokens
        .map((token: string) =>
          slugifyForFilename(token, { maxLength: 20, preserveCase: true }),
        )
        .filter((token: string) => token.length > 0)
      : [];

    let summarySegment: string | null = null;
    if (summaryTokens.length > 0) {
      summarySegment = summaryTokens.join("-");
    } else if (optionSummary?.derivedFromMetadata) {
      summarySegment = optionSummary.total === 0 ? "default" : "custom";
    } else {
      summarySegment = optionSummary ? "custom" : "custom";
    }

    const nameParts: string[] = [];
    if (randomizerSlug) nameParts.push(randomizerSlug);
    if (versionSlug) nameParts.push(versionSlug);
    if (seedSegment) nameParts.push(`seed-${seedSegment}`);
    if (summarySegment) nameParts.push(summarySegment);

    if (nameParts.length === 0) {
      const fallbackBase =
        slugifyForFilename(defaultDisplayName, { maxLength: 24 }) ||
        "patched-rom";
      nameParts.push(fallbackBase);
      if (seedSegment) nameParts.push(`seed-${seedSegment}`);
      nameParts.push("custom");
    }

    const romFileNameBase = nameParts.join("_");
    const fileName = `${romFileNameBase}${romFileExt}`;

    // Send 'complete' message type, ensuring patchedRom is the one from romBuffers for the primary game
    const finalPatchedPrimaryRom = romBuffers.get(primaryGameId);
    if (!finalPatchedPrimaryRom) {
      // This should ideally not happen if primaryGameId was in romBuffers
      throw new Error("Primary game ROM buffer not found after patching.");
    }

    self.postMessage({
      type: "complete",
      patchedRom: finalPatchedPrimaryRom,
      fileName: fileName,
    });
    self.postMessage({ type: "progress", progress: 100 }); // Ensure progress hits 100
  } catch (error) {
    console.error("Error in patcher worker:", error);
    // Send 'error' with a 'message' property as expected by patching.ts
    self.postMessage({
      type: "error",
      message:
        error instanceof Error
          ? error.message
          : "An unknown error occurred in the patcher worker.",
    });
  }
};

// Signal that the worker is ready to receive messages
self.postMessage({ type: "ready" });
