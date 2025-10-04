import { describe, it, expect } from "vitest";
import { SamusSprite } from "../rdc-types";

function makeSegment(len: number, fill: number): Buffer {
  return Buffer.alloc(len, fill & 0xff);
}

describe("SamusSprite parse and palette indexing", () => {
  it("parses banks and positions Power Standard palette at index 20 (30 bytes)", () => {
    // Segment lengths per SamusSprite.parse implementation
    const lengths = [
      // 13 full DMA banks
      ...Array.from({ length: 13 }, () => 0x8000),
      // partial bank
      0x7880,
      // death L/R, gun port, file select assets
      0x3f60,
      0x3f60,
      0x03c0,
      0x0600,
      0x0020,
      0x0020,
      // Power Standard palette
      0x001e,
    ];

    const segments = lengths.map((len, i) => makeSegment(len, 0x10 + i));
    const tail = makeSegment(100, 0xee); // leftover palette data etc.
    const buffer = Buffer.concat([...segments, tail]);

    const samus = new SamusSprite();
    samus.parse(buffer, 0);

    // Expect 22 total segments including the leftover
    expect(samus.content.length).toBe(22);

    // Check a few banks
    for (let i = 0; i < 13; i++) {
      expect(samus.content[i].length).toBe(0x8000);
    }
    expect(samus.content[13].length).toBe(0x7880);

    // Palette segment at index 20 should be 30 bytes
    expect(samus.content[20].length).toBe(0x001e);
    // fetchPowerStandardPalette returns the buffer at [20]
    const powerStd = samus.fetchPowerStandardPalette();
    expect(powerStd).toBe(samus.content[20]);
    expect(powerStd.length).toBe(30);

    // fetchDma8x8 should work in bank 0
    const tile0 = samus.fetchDma8x8(0);
    expect(tile0.length).toBe(0x20);

    // Compute a tile index that falls into the partial bank (bankIndex = 13)
    const tilesPerFullBank = 0x8000 / 0x20; // 1024
    const baseIndexBank13 = 13 * tilesPerFullBank; // start of partial bank
    const tilePartial = samus.fetchDma8x8(baseIndexBank13 + 10);
    expect(tilePartial.length).toBe(0x20);

    // Out-of-bounds bank (bankIndex >= 14) should throw
    const invalidIndex = 14 * tilesPerFullBank; // would imply bankIndex 14
    expect(() => samus.fetchDma8x8(invalidIndex)).toThrow();
  });
});
