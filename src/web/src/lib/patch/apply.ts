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

function readUnsignedNumber(
  data: Uint8Array,
  state: { offset: number },
): number {
  let result = 0;
  let shift = 1;

  while (true) {
    if (state.offset >= data.length) {
      throw new Error(
        "Invalid BPS patch: Unexpected end of file while reading varint.",
      );
    }
    const value = data[state.offset++];
    result += (value & 0x7f) * shift;
    if (value & 0x80) break;
    shift <<= 7;
    result += shift;
  }

  return result;
}

function readSignedNumber(data: Uint8Array, state: { offset: number }): number {
  const unsigned = readUnsignedNumber(data, state);
  const negative = (unsigned & 1) === 1;
  const magnitude = unsigned >> 1;
  return negative ? -(magnitude + 1) : magnitude;
}

function ensureRange(
  label: string,
  start: number,
  length: number,
  max: number,
) {
  if (start < 0 || length < 0 || start + length > max) {
    throw new Error(
      `${label} out of bounds while applying BPS patch (offset=${start}, length=${length}, size=${max}).`,
    );
  }
}

export function applyBpsPatch(
  baseRom: ArrayBuffer,
  patchData: ArrayBuffer,
): ArrayBuffer {
  const patchBytes = asUint8Array(patchData);
  if (patchBytes.length < 19) {
    throw new Error("Invalid BPS patch: File too small.");
  }
  if (
    patchBytes[0] !== 0x42 ||
    patchBytes[1] !== 0x50 ||
    patchBytes[2] !== 0x53 ||
    patchBytes[3] !== 0x31
  ) {
    throw new Error('Invalid BPS patch: Missing "BPS1" header.');
  }

  // CRC32 helper (IEEE 802.3 polynomial 0xEDB88320)
  const CRC_TABLE = (() => {
    const table = new Uint32Array(256);
    for (let i = 0; i < 256; i++) {
      let c = i;
      for (let j = 0; j < 8; j++) {
        c = c & 1 ? (c >>> 1) ^ 0xedb88320 : c >>> 1;
      }
      table[i] = c >>> 0;
    }
    return table;
  })();
  function crc32(data: Uint8Array, start = 0, end = data.length): number {
    let crc = 0xffffffff;
    for (let i = start; i < end; i++) {
      crc = CRC_TABLE[(crc ^ data[i]) & 0xff] ^ (crc >>> 8);
    }
    return (crc ^ 0xffffffff) >>> 0;
  }

  const state = { offset: 4 };

  const sourceSize = readUnsignedNumber(patchBytes, state);
  const targetSize = readUnsignedNumber(patchBytes, state);
  const metadataSize = readUnsignedNumber(patchBytes, state);

  ensureRange("Metadata", state.offset, metadataSize, patchBytes.length);
  state.offset += metadataSize;

  const sourceBytes = asUint8Array(baseRom);
  let sourceView: Uint8Array = sourceBytes;
  if (sourceView.byteLength < sourceSize) {
    throw new Error(
      `Invalid base ROM: BPS patch expects ${sourceSize} bytes but received ${sourceView.byteLength}.`,
    );
  }
  if (sourceView.byteLength > sourceSize) {
    if (sourceView.byteLength - sourceSize === 512) {
      sourceView = sourceView.subarray(512, 512 + sourceSize);
    } else {
      sourceView = sourceView.subarray(0, sourceSize);
    }
  }

  const output = new Uint8Array(targetSize);
  const dataEnd = patchBytes.length - 12; // last 12 bytes: [sourceCRC, targetCRC, patchCRC]
  if (dataEnd < state.offset) {
    throw new Error("Invalid BPS patch: Missing data records.");
  }

  // Validate patch CRCs early to catch corrupted patches
  const dv = new DataView(
    patchBytes.buffer,
    patchBytes.byteOffset,
    patchBytes.byteLength,
  );
  const sourceCrcInPatch = dv.getUint32(patchBytes.byteLength - 12, true);
  const targetCrcInPatch = dv.getUint32(patchBytes.byteLength - 8, true);
  const patchCrcInPatch = dv.getUint32(patchBytes.byteLength - 4, true);

  // Verify patch CRC covers everything except the final 4 bytes (patch CRC itself)
  const computedPatchCrc = crc32(patchBytes, 0, patchBytes.length - 4);
  if (computedPatchCrc !== patchCrcInPatch) {
    throw new Error(
      `Invalid BPS patch: Patch CRC mismatch (expected ${patchCrcInPatch >>> 0}, computed ${computedPatchCrc >>> 0}).`,
    );
  }

  let sourceRelativeOffset = 0;
  let targetRelativeOffset = 0;
  let sourceReadOffset = 0;
  let targetOffset = 0;

  while (state.offset < dataEnd) {
    const encoded = readUnsignedNumber(patchBytes, state);
    const action = encoded & 3;
    const length = (encoded >> 2) + 1;

    switch (action) {
      case 0: {
        ensureRange("Source read", sourceReadOffset, length, sourceView.length);
        ensureRange("Target write", targetOffset, length, output.length);
        output.set(
          sourceView.subarray(sourceReadOffset, sourceReadOffset + length),
          targetOffset,
        );
        sourceReadOffset += length;
        targetOffset += length;
        break;
      }
      case 1: {
        ensureRange("Target literal", state.offset, length, dataEnd);
        ensureRange("Target write", targetOffset, length, output.length);
        output.set(
          patchBytes.subarray(state.offset, state.offset + length),
          targetOffset,
        );
        state.offset += length;
        targetOffset += length;
        break;
      }
      case 2: {
        sourceRelativeOffset += readSignedNumber(patchBytes, state);
        ensureRange(
          "Source copy",
          sourceRelativeOffset,
          length,
          sourceView.length,
        );
        ensureRange("Target write", targetOffset, length, output.length);
        output.set(
          sourceView.subarray(
            sourceRelativeOffset,
            sourceRelativeOffset + length,
          ),
          targetOffset,
        );
        sourceRelativeOffset += length;
        targetOffset += length;
        break;
      }
      case 3: {
        targetRelativeOffset += readSignedNumber(patchBytes, state);
        ensureRange("Target copy", targetRelativeOffset, length, output.length);
        ensureRange("Target write", targetOffset, length, output.length);
        output.copyWithin(
          targetOffset,
          targetRelativeOffset,
          targetRelativeOffset + length,
        );
        targetRelativeOffset += length;
        targetOffset += length;
        break;
      }
      default: {
        throw new Error(`Invalid BPS patch: Unknown action ${action}.`);
      }
    }
  }

  if (targetOffset !== targetSize) {
    throw new Error(
      `Invalid BPS patch: Output size mismatch (expected ${targetSize}, wrote ${targetOffset}).`,
    );
  }

  // Verify CRCs for source and target contents
  const computedSourceCrc = crc32(sourceView);
  if (computedSourceCrc !== sourceCrcInPatch) {
    throw new Error(
      `Invalid base ROM: CRC mismatch (expected ${sourceCrcInPatch >>> 0}, computed ${computedSourceCrc >>> 0}).`,
    );
  }
  const computedTargetCrc = crc32(output);
  if (computedTargetCrc !== targetCrcInPatch) {
    throw new Error(
      `Invalid BPS patch: Target CRC mismatch (expected ${targetCrcInPatch >>> 0}, computed ${computedTargetCrc >>> 0}).`,
    );
  }

  return output.buffer;
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
