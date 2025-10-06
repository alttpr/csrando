import { Buffer } from "buffer";
import { describe, it, expect } from "vitest";

import { Metroid1SpriteDataBlock } from "../rdc-types";
import { extractMetroidSegmentsFromAsset } from "../metroid-assets";

describe("metroid asset utilities", () => {
  it("extracts segments according to manifest order", () => {
    const block = new Metroid1SpriteDataBlock();
    const manifest = block.manifest as [number[], number, number[]][];
    const asset = {
      name: "Test Samus",
      writes: [] as Array<{
        offset: string | string[];
        length: number;
        base64: string;
      }>,
    };

    let addressCursor = 0x6b0000;
    manifest.forEach(([addresses, length], idx) => {
      const toHex = (addr: number) => `0x${addr.toString(16)}`;
      const buffer = Buffer.alloc(length, idx + 1);
      const base64 = buffer.toString("base64");
      const offsetValue =
        addresses.length > 1
          ? addresses.map(() => toHex(addressCursor))
          : toHex(addressCursor);
      asset.writes.push({
        offset: offsetValue,
        length,
        base64,
      });
      addressCursor += length;
    });

    const extraction = extractMetroidSegmentsFromAsset(asset);
    expect(extraction.buffers).toHaveLength(manifest.length);
    extraction.buffers.forEach((buffer, idx) => {
      const [, length] = manifest[idx];
      expect(buffer.length).toBe(length);
      expect(buffer.equals(Buffer.alloc(length, idx + 1))).toBe(true);
    });
    expect(extraction.unusedWrites).toHaveLength(0);
  });
});
