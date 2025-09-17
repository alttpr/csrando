import { describe, it, expect } from "vitest";
import {
  convertSnesColor,
  convertPalette,
  convertSnesTileToPixelIndices,
} from "../image-renderer";

function u16le(n: number): Buffer {
  const b = Buffer.alloc(2);
  b.writeUInt16LE(n, 0);
  return b;
}

describe("image-renderer color and tile conversion", () => {
  it("convertSnesColor uses 5-bit << 3 expansion (C#-aligned)", () => {
    const red = convertSnesColor(u16le(0x001f));
    expect(red).toEqual({ r: 248, g: 0, b: 0, a: 255 });

    const green = convertSnesColor(u16le(0x03e0));
    expect(green).toEqual({ r: 0, g: 248, b: 0, a: 255 });

    const blue = convertSnesColor(u16le(0x7c00));
    expect(blue).toEqual({ r: 0, g: 0, b: 248, a: 255 });
  });

  it("convertPalette prepends transparent and parses multiple colors", () => {
    const pal = Buffer.concat([
      u16le(0x001f), // red
      u16le(0x03e0), // green
    ]);
    const colors = convertPalette(pal, true);
    expect(colors.length).toBe(3);
    expect(colors[0].a).toBe(0); // transparent
    expect(colors[1]).toEqual({ r: 248, g: 0, b: 0, a: 255 });
    expect(colors[2]).toEqual({ r: 0, g: 248, b: 0, a: 255 });
  });

  it("convertSnesTileToPixelIndices decodes bitplanes to indices", () => {
    const tile = Buffer.alloc(32, 0);
    // Row 0: set plane0 bit7 => pixel (0,0) = 0001b = 1
    tile[0] = 0x80;
    // Row 0: set plane1 bit6 => pixel (1,0) = 0010b = 2
    tile[1] = 0x40;

    const indices = convertSnesTileToPixelIndices(tile);
    expect(indices.length).toBe(64);
    expect(indices.slice(0, 8)).toEqual([1, 2, 0, 0, 0, 0, 0, 0]);
  });
});
