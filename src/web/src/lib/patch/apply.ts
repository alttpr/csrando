import { parse as bpsParse, apply as bpsApply } from "bps";

export type PatchFormat = "ips" | "bps";

function asUint8Array(buffer: ArrayBuffer | Uint8Array): Uint8Array {
  return buffer instanceof Uint8Array ? buffer : new Uint8Array(buffer);
}

export function detectPatchFormat(patchData: ArrayBuffer): PatchFormat {
  const header = new Uint8Array(
    patchData,
    0,
    Math.min(5, patchData.byteLength),
  );
  if (
    header.length >= 5 &&
    header[0] === 0x50 &&
    header[1] === 0x41 &&
    header[2] === 0x54 &&
    header[3] === 0x43 &&
    header[4] === 0x48
  ) {
    return "ips";
  }
  if (
    header.length >= 4 &&
    header[0] === 0x42 &&
    header[1] === 0x50 &&
    header[2] === 0x53 &&
    header[3] === 0x31
  ) {
    return "bps";
  }
  throw new Error("Unsupported patch format. Expected IPS or BPS file.");
}

export function applyIpsPatch(
  baseRom: ArrayBuffer,
  patchData: ArrayBuffer,
): ArrayBuffer {
  const patchView = new DataView(patchData);
  let patchedRom = new Uint8Array(baseRom);
  let maxWriteEnd = baseRom.byteLength;

  function ensureCapacity(requiredEndOffsetExclusive: number) {
    if (requiredEndOffsetExclusive <= patchedRom.length) return;
    const bank = 0x8000;
    const newSize = Math.ceil(requiredEndOffsetExclusive / bank) * bank;
    const newRom = new Uint8Array(newSize);
    newRom.set(patchedRom);
    patchedRom = newRom;
  }

  let patchOffset = 0;

  if (
    patchView.getUint8(patchOffset++) !== 0x50 ||
    patchView.getUint8(patchOffset++) !== 0x41 ||
    patchView.getUint8(patchOffset++) !== 0x54 ||
    patchView.getUint8(patchOffset++) !== 0x43 ||
    patchView.getUint8(patchOffset++) !== 0x48
  ) {
    throw new Error('Invalid IPS patch: Missing "PATCH" header.');
  }

  while (patchOffset < patchData.byteLength) {
    if (
      patchOffset + 3 <= patchData.byteLength &&
      patchView.getUint8(patchOffset) === 0x45 &&
      patchView.getUint8(patchOffset + 1) === 0x4f &&
      patchView.getUint8(patchOffset + 2) === 0x46
    ) {
      return patchedRom.buffer;
    }

    if (patchOffset + 5 > patchData.byteLength) {
      throw new Error("Invalid IPS patch: Truncated record or missing EOF.");
    }

    const offset =
      (patchView.getUint8(patchOffset++) << 16) |
      (patchView.getUint8(patchOffset++) << 8) |
      patchView.getUint8(patchOffset++);
    const size =
      (patchView.getUint8(patchOffset++) << 8) |
      patchView.getUint8(patchOffset++);

    if (size === 0) {
      if (patchOffset + 3 > patchData.byteLength) {
        throw new Error("Invalid IPS patch: Truncated RLE record.");
      }
      const rleSize =
        (patchView.getUint8(patchOffset++) << 8) |
        patchView.getUint8(patchOffset++);
      const rleValue = patchView.getUint8(patchOffset++);

      ensureCapacity(offset + rleSize);
      for (let i = 0; i < rleSize; i++) {
        patchedRom[offset + i] = rleValue;
      }
      if (offset + rleSize > maxWriteEnd) maxWriteEnd = offset + rleSize;
    } else {
      if (patchOffset + size > patchData.byteLength) {
        throw new Error(
          "Invalid IPS patch: Truncated data payload for a record.",
        );
      }
      ensureCapacity(offset + size);
      for (let i = 0; i < size; i++) {
        patchedRom[offset + i] = patchView.getUint8(patchOffset++);
      }
      if (offset + size > maxWriteEnd) maxWriteEnd = offset + size;
    }
  }

  throw new Error(
    'Invalid IPS patch: Missing "EOF" marker at the end of the patch.',
  );
}

export function applyBpsPatch(
  baseRom: ArrayBuffer,
  patchData: ArrayBuffer,
): ArrayBuffer {
  // Parse BPS into an instruction set and apply using the library
  const { instructions } = bpsParse(new Uint8Array(patchData));

  // The library validates source checksum; we still need to ensure the baseRom view
  // matches the expected source size (handling 512-byte copier headers gracefully)
  const sourceBytes = asUint8Array(baseRom);
  let sourceView: Uint8Array = sourceBytes;
  const srcSize = instructions.sourceSize >>> 0;
  if (sourceView.byteLength < srcSize) {
    throw new Error(
      `Invalid base ROM: BPS patch expects ${srcSize} bytes but received ${sourceView.byteLength}.`,
    );
  }
  if (sourceView.byteLength > srcSize) {
    if (sourceView.byteLength - srcSize === 512) {
      sourceView = sourceView.subarray(512, 512 + srcSize);
    } else {
      sourceView = sourceView.subarray(0, srcSize);
    }
  }

  const result = bpsApply(instructions, sourceView);
  // Ensure ArrayBuffer return type
  return result.buffer as ArrayBuffer;
}

// Helper to read BPS source/target sizes using the library's parser
export function getBpsPatchInfo(
  patchData: ArrayBuffer,
): { sourceSize: number; targetSize: number } | null {
  try {
    const { instructions } = bpsParse(new Uint8Array(patchData));
    const src = Number(instructions.sourceSize ?? 0);
    const tgt = Number(instructions.targetSize ?? 0);
    if (!Number.isFinite(src) || !Number.isFinite(tgt) || src <= 0 || tgt <= 0)
      return null;
    return { sourceSize: src, targetSize: tgt };
  } catch {
    return null;
  }
}

export function applyPatch(
  baseRom: ArrayBuffer,
  patchData: ArrayBuffer,
  forcedFormat?: PatchFormat,
): ArrayBuffer {
  const format = forcedFormat ?? detectPatchFormat(patchData);
  if (format === "ips") {
    return applyIpsPatch(baseRom, patchData);
  }
  if (format === "bps") {
    return applyBpsPatch(baseRom, patchData);
  }
  throw new Error(`Unsupported patch format: ${format}`);
}
