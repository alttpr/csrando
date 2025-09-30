import { describe, it, expect } from "vitest";
import {
  convertSnesColor,
  convertPalette,
  convertSnesTileToPixelIndices,
  convertNesTile2bppToPixelIndices,
  renderNESAvatarImage,
} from "../image-renderer";
import { Zelda1SpriteDataBlock, Metroid1SpriteDataBlock } from "../rdc-types";

function u16le(n: number): Buffer {
  const b = Buffer.alloc(2);
  b.writeUInt16LE(n, 0);
  return b;
}

function fillNesTile(buffer: Buffer, tileIndex: number, paletteValue: number) {
  const base = tileIndex * 16;
  for (let row = 0; row < 8; row++) {
    if (paletteValue & 0b01) buffer[base + row] = 0xff;
    if (paletteValue & 0b10) buffer[base + 8 + row] = 0xff;
  }
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

  it("convertNesTile2bppToPixelIndices mirrors bits like quad-patcher renderer", () => {
    const tile = Buffer.alloc(16, 0);
    // Row 0, set MSB (bit7) in plane0 so the leftmost pixel is colored
    tile[0] = 0x80;
    const indices = convertNesTile2bppToPixelIndices(tile);
    expect(indices[0]).toBe(1);
    expect(indices.slice(1, 8)).toEqual([0, 0, 0, 0, 0, 0, 0]);
  });

  it("renderNESAvatarImage arranges Zelda tiles using quad-patcher order", () => {
    const sprite = new Zelda1SpriteDataBlock();
    const manifest = sprite.manifest as [number[], number, number[]][];
    const buffers = manifest.map(([, length]) => Buffer.alloc(length, 0));

    const chr = buffers[2];
    fillNesTile(chr, 0, 0b01); // color index 1 at top-left (order slot 0)
    fillNesTile(chr, 2, 0b10); // color index 2 (order slot 2 -> bottom-left)
    fillNesTile(chr, 1, 0b11); // color index 3 (order slot 1 -> top-right)

    const palette = Buffer.from([0x01, 0x11, 0x21]);
    buffers[6] = palette;

    sprite.setContent(buffers);

    const png = renderNESAvatarImage(sprite);
    expect(png.width).toBe(16);
    expect(png.height).toBe(16);

    const sample = (x: number, y: number) => {
      const idx = (png.width * y + x) << 2;
      return png.data.slice(idx, idx + 4);
    };

    const topLeft = Array.from(sample(0, 0));
    const topRight = Array.from(sample(8, 0));
    const bottomLeft = Array.from(sample(0, 8));

    // Ensure colored pixels were drawn (alpha 255) and distinguishable
    expect(topLeft[3]).toBe(255);
    expect(topRight[3]).toBe(255);
    expect(bottomLeft[3]).toBe(255);
    expect(topLeft.slice(0, 3)).not.toEqual(topRight.slice(0, 3));
    expect(topLeft.slice(0, 3)).not.toEqual(bottomLeft.slice(0, 3));
    expect(topRight.slice(0, 3)).not.toEqual(bottomLeft.slice(0, 3));
  });

  it("renderNESAvatarImage arranges Metroid tiles like quad-patcher run pose", () => {
    const sprite = new Metroid1SpriteDataBlock();
    const manifest = sprite.manifest as [number[], number, number[]][];
    const buffers = manifest.map(([, length]) => Buffer.alloc(length, 0));

    fillNesTile(buffers[0], 2, 0b01);
    fillNesTile(buffers[0], 3, 0b10);
    fillNesTile(buffers[11], 2, 0b11);
    fillNesTile(buffers[11], 3, 0b01);
    fillNesTile(buffers[5], 2, 0b10);
    fillNesTile(buffers[5], 3, 0b11);
    fillNesTile(buffers[8], 1, 0b01);
    fillNesTile(buffers[8], 2, 0b10);
    fillNesTile(buffers[8], 3, 0b11);

    buffers[18] = Buffer.from([0x01, 0x11, 0x21]);
    sprite.setContent(buffers);

    const png = renderNESAvatarImage(sprite);
    expect(png.width).toBe(3 * 8);
    expect(png.height).toBe(4 * 8);

    const sample = (x: number, y: number) => {
      const idx = (png.width * y + x) << 2;
      return Array.from(png.data.slice(idx, idx + 3));
    };

    const color1 = [0, 34, 99];
    const color2 = [18, 81, 168];
    const color3 = [98, 161, 250];
    const expected = [
      color1,
      color2,
      color1,
      color3,
      color2,
      color3,
      color1,
      color2,
      color3,
    ];

    const positions: Array<[number, number]> = [
      [0, 0],
      [8, 0],
      [0, 8],
      [8, 8],
      [0, 16],
      [8, 16],
      [0, 24],
      [8, 24],
      [16, 24],
    ];

    positions.forEach(([x, y], idx) => {
      const color = sample(x, y);
      expect(color).toEqual(expected[idx]);
    });
  });
});
