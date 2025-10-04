import { Buffer } from "buffer";
import { PNG } from "pngjs";
import { ZsprData } from "./zspr";
import {
  SamusSprite,
  Zelda1SpriteDataBlock,
  Metroid1SpriteDataBlock,
} from "./rdc-types"; // Added NES block types

export interface Color {
  r: number; // 0-255
  g: number; // 0-255
  b: number; // 0-255
  a: number; // 0-255 (alpha)
}

export const TransparentBlack: Color = { r: 0, g: 0, b: 0, a: 0 };

/**
 * Converts a 2-byte SNES color value to an RGBA Color object.
 * Correct SNES color format (BGR15): 0BBBBBGGGGGRRRRR (bit 0-4 = Red, 5-9 = Green, 10-14 = Blue)
 * Documentation: https://problemkaputt.de/fullsnes.htm#snesppumemory ($213B / CGRAM format)
 * @param snesColorData A Buffer containing 2 bytes (UInt16LE) or a number array [lo, hi].
 * @returns A Color object.
 */
export function convertSnesColor(snesColorData: any): Color {
  let value: number;
  if (
    snesColorData &&
    typeof snesColorData === "object" &&
    "length" in snesColorData &&
    (snesColorData as any).length >= 2
  ) {
    // If it has readUInt16LE use it, else treat as array-like
    if (typeof (snesColorData as any).readUInt16LE === "function") {
      value = (snesColorData as any).readUInt16LE(0);
    } else {
      value = (snesColorData[0] & 0xff) | ((snesColorData[1] & 0xff) << 8);
    }
  } else if (Array.isArray(snesColorData) && snesColorData.length >= 2) {
    value = snesColorData[0] | (snesColorData[1] << 8);
  } else {
    throw new Error(
      "Invalid snesColorData format. Must be a 2-byte Buffer, Uint8Array, or number array.",
    );
  }

  // Extract 5-bit channels (BGR15 -> store as RGB)
  const r5 = (value >> 0) & 0x1f; // Red   bits 0-4
  const g5 = (value >> 5) & 0x1f; // Green bits 5-9
  const b5 = (value >> 10) & 0x1f; // Blue  bits 10-14

  // Expand 5-bit to 8-bit like the reference C# (<< 3)
  const to8 = (v: number) => (v << 3) & 0xff;

  return { r: to8(r5), g: to8(g5), b: to8(b5), a: 255 };
}

/**
 * Converts a Buffer of SNES palette data (multiple 2-byte colors) to an array of Color objects.
 * @param paletteData Buffer containing SNES palette data.
 * @param includeTransparent If true, prepends TransparentBlack to the palette.
 * @returns An array of Color objects.
 */
export function convertPalette(
  paletteData: Buffer,
  includeTransparent: boolean = true,
): Color[] {
  if (paletteData.length % 2 !== 0) {
    throw new Error("Palette data length must be a multiple of 2.");
  }

  const colors: Color[] = [];
  if (includeTransparent) {
    colors.push(TransparentBlack);
  }

  for (let i = 0; i < paletteData.length; i += 2) {
    // Pass as a buffer slice to convertSnesColor to reuse its logic
    colors.push(convertSnesColor(paletteData.subarray(i, i + 2)));
  }
  return colors;
}

/**
 * Converts SNES 4bpp tile data (32 bytes for an 8x8 tile) to an array of palette indices (0-15).
 * @param tileData Buffer containing the 32 bytes of tile data.
 * @returns An array of 64 numbers, each representing a palette index for a pixel.
 */
export function convertSnesTileToPixelIndices(tileData: Buffer): number[] {
  if (tileData.length < 32) {
    throw new Error(
      "Tile data must be at least 32 bytes for an 8x8 4bpp tile.",
    );
  }

  const pixelIndices: number[] = new Array(64);

  for (let rowNum = 0; rowNum < 8; rowNum++) {
    // Get the 4 bytes for the current row from the bitplanes
    const bytePlane0 = tileData[rowNum * 2];
    const bytePlane1 = tileData[rowNum * 2 + 1];
    const bytePlane2 = tileData[rowNum * 2 + 16];
    const bytePlane3 = tileData[rowNum * 2 + 17];

    for (let colNum = 0; colNum < 8; colNum++) {
      const mask = 0x80 >> colNum;

      const bit0 = bytePlane0 & mask ? 1 : 0; // Plane 0
      const bit1 = bytePlane1 & mask ? 1 : 0; // Plane 1
      const bit2 = bytePlane2 & mask ? 1 : 0; // Plane 2
      const bit3 = bytePlane3 & mask ? 1 : 0; // Plane 3

      // Combine bits to form the palette index
      // C# order: ((row[3] & m) ? 8:0) | ((row[2] & m)?4:0) | ((row[1] & m)?2:0) | ((row[0] & m)?1:0)
      // row[0] is plane0, row[1] is plane1, row[2] is plane2, row[3] is plane3
      // So, palette_idx = (bit_from_plane3 << 3) | (bit_from_plane2 << 2) | (bit_from_plane1 << 1) | bit_from_plane0;
      const paletteIndex = (bit3 << 3) | (bit2 << 2) | (bit1 << 1) | bit0;

      pixelIndices[rowNum * 8 + colNum] = paletteIndex;
    }
  }
  return pixelIndices;
}

/**
 * Renders an array of pixel palette indices into a PNG object using a given palette.
 * @param pixelIndices Array of palette indices (e.g., from convertSnesTileToPixelIndices).
 * @param palette Array of Color objects.
 * @param width Width of the image.
 * @param height Height of the image.
 * @returns A PNG object.
 */
export function renderTile(
  pixelIndices: number[],
  palette: Color[],
  width: number = 8,
  height: number = 8,
): PNG {
  if (pixelIndices.length !== width * height) {
    throw new Error(
      `Pixel indices array length (${pixelIndices.length}) does not match width*height (${width * height}).`,
    );
  }

  const png = new PNG({ width, height });

  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const pixelArrayIdx = y * width + x;
      const paletteIdx = pixelIndices[pixelArrayIdx];

      const pngDataIdx = (png.width * y + x) << 2; // Each pixel is 4 bytes (RGBA)

      // Special handling for palette index 0, which is always transparent for SNES sprites.
      if (paletteIdx === 0) {
        png.data[pngDataIdx] = 0;
        png.data[pngDataIdx + 1] = 0;
        png.data[pngDataIdx + 2] = 0;
        png.data[pngDataIdx + 3] = 0;
        continue;
      }

      const color = palette[paletteIdx] || TransparentBlack; // Default to transparent if palette index is out of bounds

      png.data[pngDataIdx] = color.r;
      png.data[pngDataIdx + 1] = color.g;
      png.data[pngDataIdx + 2] = color.b;
      png.data[pngDataIdx + 3] = color.a;
    }
  }
  return png;
}

// Helper to slice a buffer, ensuring a copy is made.
function sliceBuffer(buffer: Buffer, index: number, size: number): Buffer {
  if (index < 0 || size < 0 || index + size > buffer.length) {
    throw new Error(
      `sliceBuffer: invalid slice parameters. Index: ${index}, Size: ${size}, Buffer Length: ${buffer.length}`,
    );
  }
  const newBuffer = Buffer.alloc(size);
  buffer.copy(newBuffer, 0, index, index + size);
  return newBuffer;
}

export function renderZ3AvatarImage(
  spriteDataProvider:
    | {
        fetch8x8: (tileIndex: number) => Buffer;
        fetchPalette: (index: number) => Buffer;
      }
    | ZsprData,
  outputWidth: number = 16, // Default Link avatar is 16x24
  outputHeight: number = 24,
): PNG {
  let palette: Color[];
  let fetchTileFunc: (tileIndex: number) => Buffer;

  if ("content" in spriteDataProvider && "title" in spriteDataProvider) {
    // Heuristic for ZsprData (has content and title)
    const zspr = spriteDataProvider as ZsprData;
    // For ZSPR:
    // Palette is at offset 0x7000 in zspr.content, length 30 bytes (for the first palette)
    // Sprite GFX is at offset 0 in zspr.content
    const mainPaletteData = sliceBuffer(zspr.content, 0x7000, 30); // First 30 bytes for one palette
    palette = convertPalette(mainPaletteData); // convertPalette adds transparent at index 0 by default

    fetchTileFunc = (tileIndex: number) => {
      const tileOffset = tileIndex * 0x20; // Each tile is 0x20 bytes
      // Ensure we don't read past the GFX data section (0x7000 bytes for ZSPR Link sprite)
      if (tileOffset < 0 || tileOffset + 0x20 > 0x7000) {
        throw new Error(
          `ZSPR tile index ${tileIndex} is out of bounds for GFX data (0x0000-0x6FFF). Offset: ${tileOffset}`,
        );
      }
      return sliceBuffer(zspr.content, tileOffset, 0x20);
    };
  } else {
    // Assumed to be LinkSprite-like
    const rdcSprite = spriteDataProvider as {
      fetch8x8: (tileIndex: number) => Buffer;
      fetchPalette: (index: number) => Buffer;
    };
    const mainPaletteData = rdcSprite.fetchPalette(0); // Fetch first palette
    palette = convertPalette(mainPaletteData); // convertPalette adds transparent at index 0
    // Bind method to preserve 'this' context
    fetchTileFunc = (tileIndex: number) => rdcSprite.fetch8x8(tileIndex);
  }

  const png = new PNG({ width: outputWidth, height: outputHeight });
  // Fill with transparent background (RGBA all zeros)
  for (let i = 0; i < png.data.length; i++) {
    png.data[i] = 0;
  }

  // Helper function to draw a 16x16 meta-tile composed of four 8x8 tiles
  const pasteMetaTile = (
    tileStartIndex: number,
    targetX: number,
    targetY: number,
  ) => {
    for (let yTile = 0; yTile < 2; yTile++) {
      // 2 tiles high
      for (let xTile = 0; xTile < 2; xTile++) {
        // 2 tiles wide
        // Tiles are typically laid out in 16-tile wide rows in GFX data (0x10 tiles per row)
        const tileIndex = tileStartIndex + xTile + yTile * 0x10;

        let tilePixelData: Buffer;
        try {
          tilePixelData = fetchTileFunc(tileIndex);
        } catch (e: any) {
          console.warn(
            `Could not fetch tile ${tileIndex} for Z3 Avatar: ${e.message}. Skipping tile.`,
          );
          // Create a dummy transparent tile to avoid crashing renderTile/convertSnesTileToPixelIndices
          tilePixelData = Buffer.alloc(32, 0); // All zeros = all palette index 0
        }

        const pixelIndices = convertSnesTileToPixelIndices(tilePixelData);

        // Manually blit pixels, skipping transparent ones, to avoid "punching out" underlying pixels.
        const tileOriginX = targetX + xTile * 8;
        const tileOriginY = targetY + yTile * 8;

        for (let y = 0; y < 8; y++) {
          // y within the 8x8 tile
          for (let x = 0; x < 8; x++) {
            // x within the 8x8 tile
            const pixelArrayIdx = y * 8 + x;
            const paletteIdx = pixelIndices[pixelArrayIdx];

            // If the pixel is transparent (palette index 0), do nothing.
            if (paletteIdx === 0) {
              continue;
            }

            const color = palette[paletteIdx] || TransparentBlack;

            // Calculate index in the *main* png data
            const mainPngX = tileOriginX + x;
            const mainPngY = tileOriginY + y;
            const pngDataIdx = (png.width * mainPngY + mainPngX) << 2;

            png.data[pngDataIdx] = color.r;
            png.data[pngDataIdx + 1] = color.g;
            png.data[pngDataIdx + 2] = color.b;
            png.data[pngDataIdx + 3] = color.a;
          }
        }
      }
    }
  };

  // Render Z3 Link sprite (default pose)
  // Body: MetaTile starting at GFX index 0x26, drawn at (0, 8) on the 16x24 avatar
  pasteMetaTile(0x26, 0, 8);
  // Head: MetaTile starting at GFX index 0x02, drawn at (0, 0) on the 16x24 avatar
  pasteMetaTile(0x02, 0, 0);

  return png;
}

export function renderSMAvatarImage(
  samusSprite: SamusSprite, // SM sprites are expected to be in RDC format via SamusSprite
  outputWidth: number = 32, // Default Samus avatar is 32x48
  outputHeight: number = 48,
): PNG {
  // For Samus, the default pose uses the Power Standard palette, which is a single 30-byte palette.
  const powerStandard = samusSprite.fetchPowerStandardPalette();
  if (powerStandard.length < 30) {
    throw new Error(
      `Unexpected Samus Power Standard palette length: ${powerStandard.length}`,
    );
  }
  const palette = convertPalette(powerStandard.subarray(0, 30));

  const fetchTileFunc = (tileIndex: number): Buffer => {
    try {
      return samusSprite.fetchDma8x8(tileIndex);
    } catch {
      // If a tile is missing (e.g., out of bounds), return a buffer for a transparent 8x8 tile.
      // This assumes convertSnesTileToPixelIndices can handle such a buffer or this case is handled before rendering.
      // For simplicity, we'll ensure convertSnesTileToPixelIndices returns all zeros for a zeroed buffer.
      return Buffer.alloc(0x20, 0); // All zeros, will map to palette index 0 (transparent)
    }
  };

  const png = new PNG({ width: outputWidth, height: outputHeight });
  // Fill with transparent background
  for (let i = 0; i < png.data.length; i++) {
    png.data[i] = 0;
  }

  // Helper function to draw a row of 8x8 tiles
  const pasteTileRow = (
    tileStartIndex: number,
    count: number,
    targetX: number,
    targetY: number,
  ) => {
    for (let i = 0; i < count; i++) {
      const tileIndex = tileStartIndex + i;
      const tilePixelData = fetchTileFunc(tileIndex); // fetchTileFunc handles potential errors
      const pixelIndices = convertSnesTileToPixelIndices(tilePixelData);
      const tileImage = renderTile(pixelIndices, palette); // renderTile produces an 8x8 PNG

      // Blit tileImage onto the main png
      PNG.bitblt(tileImage, png, 0, 0, 8, 8, targetX + i * 8, targetY);
    }
  };

  // Render SM Samus sprite (default pose - standing, facing right)
  // The C# code uses `poseOffset = 6 * 1024;` which is `0x1800` as a tile index.
  // This seems to be a specific offset into the *combined* DMA GFX banks if they were one giant buffer.
  // `SamusSprite.fetchDma8x8` already handles bank selection. So, tile indices are global.
  const poseTileOffset = 0x1800; // Or 6 * 1024 if that's a byte offset / 0x20 bytes_per_tile

  // From C# `CompileSMMontage` for one Samus sprite:
  // pasteRow(poseOffset + 0, 4, new Point { X = offset * 32, Y = 0 });
  // pasteRow(poseOffset + 4, 4, new Point { X = offset * 32, Y = 16 });
  // pasteRow(poseOffset + 8, 4, new Point { X = offset * 32, Y = 32 });
  // pasteRow(poseOffset + 12, 4, new Point { X = offset * 32, Y = 8 });
  // pasteRow(poseOffset + 16, 4, new Point { X = offset * 32, Y = 24 });
  // pasteRow(poseOffset + 20, 4, new Point { X = offset * 32, Y = 40 });
  // Target X is 0 for a single avatar.

  pasteTileRow(poseTileOffset + 0, 4, 0, 0);
  pasteTileRow(poseTileOffset + 4, 4, 0, 16);
  pasteTileRow(poseTileOffset + 8, 4, 0, 32);
  pasteTileRow(poseTileOffset + 12, 4, 0, 8);
  pasteTileRow(poseTileOffset + 16, 4, 0, 24);
  pasteTileRow(poseTileOffset + 20, 4, 0, 40);

  return png;
}

// NES helpers (2bpp CHR, 16 bytes per 8x8 tile)
export function convertNesTile2bppToPixelIndices(tileData: Buffer): number[] {
  if (tileData.length < 16) {
    throw new Error("NES tile data must be 16 bytes for an 8x8 2bpp tile.");
  }
  const pixels: number[] = new Array(64);
  for (let row = 0; row < 8; row++) {
    const plane0 = tileData[row];
    const plane1 = tileData[row + 8];
    for (let col = 0; col < 8; col++) {
      const mask = 0x80 >> col;
      const bit0 = plane0 & mask ? 1 : 0;
      const bit1 = plane1 & mask ? 1 : 0;
      // NES 2bpp index 0..3
      pixels[row * 8 + col] = (bit1 << 1) | bit0;
    }
  }
  return pixels;
}

export function renderNESAvatarImage(
  sprite: Zelda1SpriteDataBlock | Metroid1SpriteDataBlock,
  outputWidth: number = 16,
  outputHeight: number = 16,
): PNG {
  // For our NES format, palette content[1] is 0x20 bytes, we use first 8 bytes (4 colors)
  const palBuf = sprite.content[1] ?? Buffer.alloc(0);
  const palette = convertPalette(
    palBuf.subarray(0, Math.min(8, palBuf.length)),
    true,
  );

  const fetchNesTile = (tileIndex: number): Buffer => {
    const gfx = sprite.content[0] ?? Buffer.alloc(0);
    const offs = tileIndex * 16;
    if (offs < 0 || offs + 16 > gfx.length) return Buffer.alloc(16, 0);
    return gfx.subarray(offs, offs + 16);
  };

  const png = new PNG({ width: outputWidth, height: outputHeight });
  // Clear transparent
  for (let i = 0; i < png.data.length; i++) png.data[i] = 0;

  // Draw a simple 2x2 meta-tile using tile indices 0,1,16,17 (like a typical CHR page layout)
  const tiles = [0, 1, 16, 17];
  let idx = 0;
  for (let y = 0; y < 2; y++) {
    for (let x = 0; x < 2; x++) {
      const tile = fetchNesTile(tiles[idx++] || 0);
      const indices = convertNesTile2bppToPixelIndices(tile);
      const tilePng = renderTile(indices, palette, 8, 8);
      PNG.bitblt(tilePng, png, 0, 0, 8, 8, x * 8, y * 8);
    }
  }
  return png;
}
