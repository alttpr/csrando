import { describe, expect, it } from "vitest";
import {
  applyBpsPatch,
  applyIpsPatch,
  applyPatch,
  detectPatchFormat,
} from "./apply";

function createSimpleIpsPatch(): ArrayBuffer {
  const bytes = new Uint8Array([
    0x50,
    0x41,
    0x54,
    0x43,
    0x48, // "PATCH"
    0x00,
    0x00,
    0x00, // offset 0
    0x00,
    0x03, // size 3
    0x01,
    0x02,
    0x03, // payload
    0x45,
    0x4f,
    0x46, // "EOF"
  ]);
  return bytes.buffer;
}

const CRC_TABLE = (() => {
  const table = new Uint32Array(256);
  for (let index = 0; index < 256; index++) {
    let crc = index;
    for (let bit = 0; bit < 8; bit++) {
      if (crc & 1) {
        crc = (crc >>> 1) ^ 0xedb88320;
      } else {
        crc >>>= 1;
      }
    }
    table[index] = crc >>> 0;
  }
  return table;
})();

function crc32(data: Uint8Array): number {
  let crc = 0xffffffff;
  for (const byte of data) {
    crc = CRC_TABLE[(crc ^ byte) & 0xff] ^ (crc >>> 8);
  }
  return (crc ^ 0xffffffff) >>> 0;
}

function encodeUnsigned(value: number): number[] {
  const out: number[] = [];
  let current = value;
  while (true) {
    const byte = current & 0x7f;
    current >>= 7;
    if (current === 0) {
      out.push(byte | 0x80);
      break;
    }
    out.push(byte);
    current -= 1;
  }
  return out;
}

function encodeSigned(value: number): number[] {
  const mapped = value < 0 ? (-value << 1) - 1 : value << 1;
  return encodeUnsigned(mapped);
}

function writeUint32LE(target: number[], value: number) {
  target.push(value & 0xff);
  target.push((value >>> 8) & 0xff);
  target.push((value >>> 16) & 0xff);
  target.push((value >>> 24) & 0xff);
}

function createBpsPatch(base: Uint8Array, target: Uint8Array): ArrayBuffer {
  const bytes: number[] = [];
  bytes.push(0x42, 0x50, 0x53, 0x31); // "BPS1"
  bytes.push(...encodeUnsigned(base.length));
  bytes.push(...encodeUnsigned(target.length));
  bytes.push(...encodeUnsigned(0)); // metadata size

  // Operations:
  // 1. SourceRead 2 bytes
  bytes.push(...encodeUnsigned(((2 - 1) << 2) | 0));
  // 2. TargetRead 2 bytes (99, 100)
  bytes.push(...encodeUnsigned(((2 - 1) << 2) | 1));
  bytes.push(99, 100);
  // 3. SourceCopy 2 bytes from offset +4 (values 50, 60)
  bytes.push(...encodeUnsigned(((2 - 1) << 2) | 2));
  bytes.push(...encodeSigned(4));
  // 4. TargetCopy 2 bytes from earlier literal data (offset +2)
  bytes.push(...encodeUnsigned(((2 - 1) << 2) | 3));
  bytes.push(...encodeSigned(2));

  const baseCrc = crc32(base);
  const targetCrc = crc32(target);

  const fullPatch: number[] = [...bytes];
  writeUint32LE(fullPatch, baseCrc);
  writeUint32LE(fullPatch, targetCrc);
  const patchCrcInput = new Uint8Array(fullPatch);
  const patchCrc = crc32(patchCrcInput);
  writeUint32LE(fullPatch, patchCrc);

  return new Uint8Array(fullPatch).buffer;
}

describe("patch detection", () => {
  it("identifies IPS patches", () => {
    const patch = createSimpleIpsPatch();
    expect(detectPatchFormat(patch)).toBe("ips");
  });

  it("identifies BPS patches", () => {
    const base = new Uint8Array([10, 20, 30, 40, 50, 60]);
    const target = new Uint8Array([10, 20, 99, 100, 50, 60, 99, 100]);
    const patch = createBpsPatch(base, target);
    expect(detectPatchFormat(patch)).toBe("bps");
  });
});

describe("apply patches", () => {
  it("applies a simple IPS patch", () => {
    const base = new Uint8Array([0, 0, 0, 0]);
    const result = new Uint8Array(
      applyIpsPatch(base.buffer, createSimpleIpsPatch()),
    );
    expect(Array.from(result)).toEqual([1, 2, 3, 0]);
  });

  it("applies a BPS patch with mixed operations", () => {
    const base = new Uint8Array([10, 20, 30, 40, 50, 60]);
    const target = new Uint8Array([10, 20, 99, 100, 50, 60, 99, 100]);
    const patch = createBpsPatch(base, target);
    const result = new Uint8Array(applyBpsPatch(base.buffer, patch));
    expect(Array.from(result)).toEqual(Array.from(target));
  });

  it("auto-detects format when applying", () => {
    const base = new Uint8Array([0, 0, 0, 0]);
    const ipsResult = new Uint8Array(
      applyPatch(base.buffer, createSimpleIpsPatch()),
    );
    expect(Array.from(ipsResult)).toEqual([1, 2, 3, 0]);

    const source = new Uint8Array([10, 20, 30, 40, 50, 60]);
    const target = new Uint8Array([10, 20, 99, 100, 50, 60, 99, 100]);
    const bpsPatch = createBpsPatch(source, target);
    const bpsResult = new Uint8Array(applyPatch(source.buffer, bpsPatch));
    expect(Array.from(bpsResult)).toEqual(Array.from(target));
  });
});
