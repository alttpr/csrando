import { Buffer } from "buffer";
import { describe, it, expect } from "vitest";

import { extractZelda1SegmentsFromAsset } from "../zelda1-assets";
import { Zelda1SpriteDataBlock } from "../rdc-types";

describe("zelda1 asset utilities", () => {
  it("extracts quad-patcher segments into manifest order", () => {
    const block = new Zelda1SpriteDataBlock();
    const manifest = block.manifest as [number[], number, number[]][];
    const asset = {
      name: "Test Sprite",
      creator: "Tester",
      originalBy: "Original",
      category: "Demo",
      writes: [] as Array<{
        offset: string | string[];
        length: number;
        base64: string;
      }>,
    };

    manifest.forEach(([addresses, length], idx) => {
      const toHex = (addr: number) => `0x${addr.toString(16)}`;
      const buffer = Buffer.alloc(length, idx);
      const base64 = buffer.toString("base64");
      const offsetValue =
        addresses.length > 1
          ? addresses.map((addr) => toHex(addr))
          : toHex(addresses[0]);
      asset.writes.push({
        offset: offsetValue,
        length,
        base64,
      });
    });

    const extraction = extractZelda1SegmentsFromAsset(asset);

    expect(extraction.buffers).toHaveLength(manifest.length);
    extraction.buffers.forEach((buffer, idx) => {
      const [, length] = manifest[idx];
      expect(buffer.length).toBe(length);
      expect(buffer.equals(Buffer.alloc(length, idx))).toBe(true);
    });
    expect(extraction.unusedWrites).toHaveLength(0);
  });

  it("throws when a required segment is missing", () => {
    const block = new Zelda1SpriteDataBlock();
    const [firstSegment] = block.manifest as [number[], number, number[]][];
    const faultyAsset = {
      name: "Broken",
      writes: [
        {
          offset: `0x${firstSegment[0][0].toString(16)}`,
          length: firstSegment[1],
          base64: Buffer.alloc(firstSegment[1], 0x11).toString("base64"),
        },
      ],
    };

    expect(() => extractZelda1SegmentsFromAsset(faultyAsset)).toThrow(
      /Could not locate data segment/,
    );
  });
});
