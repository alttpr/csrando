import { describe, it, expect } from "vitest";
import { ZsprParser } from "../zspr";
import { Buffer } from "buffer";

// Helper function to create a ZSPR buffer
function createMockZsprBuffer({
  title = "TestSprite",
  author = "TestAuthor",
  authorAscii = "ASCIIAuthor",
  gfxContent = Buffer.alloc(0x100, 1), // Default content filled with 1s
  paletteContent = Buffer.alloc(0x20, 2), // Default content filled with 2s
  glovesContent = Buffer.alloc(4, 3), // Default content filled with 3s
  overrideHeader,
  overrideVersion,
  overrideSpriteType,
  truncatedLength, // For testing truncated files
}: {
  title?: string;
  author?: string;
  authorAscii?: string;
  gfxContent?: Buffer;
  paletteContent?: Buffer;
  glovesContent?: Buffer;
  overrideHeader?: string;
  overrideVersion?: number;
  overrideSpriteType?: number;
  truncatedLength?: number;
}): Buffer {
  const headerBytes = Buffer.from(overrideHeader || "ZSPR");
  const versionByte = Buffer.from([
    overrideVersion === undefined ? 1 : overrideVersion,
  ]);
  const checksumBytes = Buffer.alloc(4, 0); // Dummy checksum

  const titleUtf16 = Buffer.from(title, "utf16le");
  const titleNull = Buffer.alloc(2, 0); // UTF-16 null terminator
  const authorUtf16 = Buffer.from(author, "utf16le");
  const authorNull = Buffer.alloc(2, 0);
  const authorAsciiBytes = Buffer.from(authorAscii, "ascii");
  const authorAsciiNull = Buffer.alloc(1, 0);

  const metadataBlock = Buffer.concat([
    titleUtf16,
    titleNull,
    authorUtf16,
    authorNull,
    authorAsciiBytes,
    authorAsciiNull,
  ]);

  // Define lengths for header fields
  const gfxLengthField = gfxContent.length;
  const paletteLengthField = paletteContent.length;
  // Gloves length is implicit (4 bytes) and not stored in ZSPR header fields, but part of the overall content.

  // Calculate offsets
  // Fixed header part: ZSPR(4) + Ver(1) + Checksum(4) + OffsetsBlock(4+2+4+2) + Type(2) + Reserved(6) = 29 bytes
  const fixedHeaderPartLength = 29;
  const spriteOffsetVal = fixedHeaderPartLength + metadataBlock.length;
  const paletteOffsetVal = spriteOffsetVal + gfxLengthField;
  // The ZsprParser reads gloves data from paletteOffsetVal + paletteLengthField
  // So, glovesData needs to be placed there in the mock buffer.

  const offsetsAndTypeBytes = Buffer.alloc(14); // spriteOff(4), spriteLen(2), palOff(4), palLen(2), type(2)
  let cur = 0;
  offsetsAndTypeBytes.writeUInt32LE(spriteOffsetVal, cur);
  cur += 4;
  offsetsAndTypeBytes.writeUInt16LE(gfxLengthField, cur);
  cur += 2;
  offsetsAndTypeBytes.writeUInt32LE(paletteOffsetVal, cur);
  cur += 4;
  offsetsAndTypeBytes.writeUInt16LE(paletteLengthField, cur);
  cur += 2;
  offsetsAndTypeBytes.writeUInt16LE(
    overrideSpriteType === undefined ? 1 : overrideSpriteType,
    cur,
  );
  cur += 2;

  const reservedBytes = Buffer.alloc(6, 0);

  const fullBuffer = Buffer.concat([
    headerBytes,
    versionByte,
    checksumBytes,
    offsetsAndTypeBytes, // This now only contains the offset/length fields + type
    reservedBytes, // Reserved bytes follow type
    metadataBlock,
    gfxContent,
    paletteContent,
    glovesContent, // Gloves data is placed after palette data for the parser to find
  ]);

  if (truncatedLength !== undefined) {
    if (truncatedLength < 0 || truncatedLength > fullBuffer.length) {
      throw new Error(
        `Invalid truncatedLength: ${truncatedLength}. Must be between 0 and ${fullBuffer.length}.`,
      );
    }
    return fullBuffer.subarray(0, truncatedLength);
  }

  return fullBuffer;
}

describe("ZsprParser", () => {
  it("should parse a valid ZSPR buffer", () => {
    const gfx = Buffer.alloc(0x70, 0xaa);
    const palette = Buffer.alloc(0x20, 0xbb);
    const gloves = Buffer.alloc(0x04, 0xcc);

    const mockBuffer = createMockZsprBuffer({
      title: "Link",
      author: "Nintendo",
      authorAscii: "NintendoA",
      gfxContent: gfx,
      paletteContent: palette,
      glovesContent: gloves,
    });

    const zsprData = ZsprParser.parse(mockBuffer);

    expect(zsprData.title).toBe("Link");
    expect(zsprData.author).toBe("Nintendo");
    expect(zsprData.authorAscii).toBe("NintendoA");

    const expectedContentLength = gfx.length + palette.length + gloves.length;
    expect(zsprData.content.length).toBe(expectedContentLength);

    // Verify that ZsprData.content is a concatenation of gfx, palette, and gloves
    const expectedConcatenated = Buffer.concat([gfx, palette, gloves]);
    expect(zsprData.content).toEqual(expectedConcatenated);
  });

  it("should throw on invalid header", () => {
    const mockBuffer = createMockZsprBuffer({ overrideHeader: "NOPE" });
    expect(() => ZsprParser.parse(mockBuffer)).toThrow(
      "Invalid ZSPR file format: Header mismatch.",
    );
  });

  it("should throw on invalid version", () => {
    const mockBuffer = createMockZsprBuffer({ overrideVersion: 2 });
    expect(() => ZsprParser.parse(mockBuffer)).toThrow(
      "Unsupported ZSPR version: 2. Expected: 1.",
    );
  });

  it("should throw on invalid sprite type", () => {
    const mockBuffer = createMockZsprBuffer({ overrideSpriteType: 2 });
    expect(() => ZsprParser.parse(mockBuffer)).toThrow(
      "Unsupported sprite type: 2. Expected Link Sprite (type 1).",
    );
  });

  it("should throw if buffer is truncated before header is complete", () => {
    const mockBuffer = createMockZsprBuffer({ truncatedLength: 3 }); // Only "ZSP"
    expect(() => ZsprParser.parse(mockBuffer)).toThrow(/buffer length/i); // Or a specific error from Buffer/DataView
  });

  it("should throw if buffer is truncated before metadata is complete", () => {
    // Fixed header is 29 bytes. Let's truncate just after that.
    const mockBuffer = createMockZsprBuffer({
      title: "VeryLongTitleThatWillBeCut",
      truncatedLength: 35,
    });
    expect(() => ZsprParser.parse(mockBuffer)).toThrow(); // Specific error depends on where it gets cut
  });

  it("should throw if buffer is truncated before GFX data is complete based on header offset/length", () => {
    // Construct a valid header pointing to GFX data, but then truncate the GFX data itself
    const gfx = Buffer.alloc(0x100, 0xaa);
    const mockBufferFull = createMockZsprBuffer({ gfxContent: gfx });
    // Truncate within the GFX data portion
    const fixedHeaderAndMetaLength =
      29 +
      ("TestSprite".length * 2 + 2) +
      ("TestAuthor".length * 2 + 2) +
      ("ASCIIAuthor".length + 1);
    const truncatedBuffer = mockBufferFull.subarray(
      0,
      fixedHeaderAndMetaLength + gfx.length / 2,
    );

    expect(() => ZsprParser.parse(truncatedBuffer)).toThrow(
      /Sprite data extends beyond buffer length/i,
    );
  });

  it("should throw if buffer is truncated before Palette data is complete", () => {
    const gfx = Buffer.alloc(0x50, 0xaa);
    const palette = Buffer.alloc(0x30, 0xbb);
    const mockBufferFull = createMockZsprBuffer({
      gfxContent: gfx,
      paletteContent: palette,
    });

    const fixedHeaderAndMetaLength =
      29 +
      ("TestSprite".length * 2 + 2) +
      ("TestAuthor".length * 2 + 2) +
      ("ASCIIAuthor".length + 1);
    const truncatedBuffer = mockBufferFull.subarray(
      0,
      fixedHeaderAndMetaLength + gfx.length + palette.length / 2,
    );

    expect(() => ZsprParser.parse(truncatedBuffer)).toThrow(
      /Palette data extends beyond buffer length/i,
    );
  });

  it("should throw if buffer is truncated before Gloves data is complete", () => {
    const gfx = Buffer.alloc(0x50, 0xaa);
    const palette = Buffer.alloc(0x30, 0xbb);
    const gloves = Buffer.alloc(0x04, 0xcc);
    const mockBufferFull = createMockZsprBuffer({
      gfxContent: gfx,
      paletteContent: palette,
      glovesContent: gloves,
    });

    const fixedHeaderAndMetaLength =
      29 +
      ("TestSprite".length * 2 + 2) +
      ("TestAuthor".length * 2 + 2) +
      ("ASCIIAuthor".length + 1);
    const truncatedBuffer = mockBufferFull.subarray(
      0,
      fixedHeaderAndMetaLength +
        gfx.length +
        palette.length +
        gloves.length / 2,
    );

    expect(() => ZsprParser.parse(truncatedBuffer)).toThrow(
      /Gloves data extends beyond buffer length/i,
    );
  });

  it("should handle empty title, author, authorAscii strings", () => {
    const gfx = Buffer.alloc(0x10, 0xaa);
    const palette = Buffer.alloc(0x10, 0xbb);
    const gloves = Buffer.alloc(0x04, 0xcc);
    const mockBuffer = createMockZsprBuffer({
      title: "",
      author: "",
      authorAscii: "",
      gfxContent: gfx,
      paletteContent: palette,
      glovesContent: gloves,
    });
    const zsprData = ZsprParser.parse(mockBuffer);
    expect(zsprData.title).toBe("");
    expect(zsprData.author).toBe("");
    expect(zsprData.authorAscii).toBe("");
    expect(zsprData.content.length).toBe(
      gfx.length + palette.length + gloves.length,
    );
  });
});
