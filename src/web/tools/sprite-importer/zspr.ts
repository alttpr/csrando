import * as fs from "fs";
import { Buffer } from "buffer"; // Technically, Buffer is global in Node.js, but good for clarity

const ZSPR_HEADER = "ZSPR";
const ZSPR_VERSION = 1;
const SPRITE_TYPE_LINK = 1; // Type 1 is Link sprite

// Lengths of the data fields, from C# `fields` and `ParseContent`
// Default maximums for Link ZSPR; actual sizes are read from header
const DEFAULT_SPRITE_DATA_LENGTH = 0x7000;
const DEFAULT_PALETTE_DATA_LENGTH = 4 * 30; // 120 bytes
const GLOVES_DATA_LENGTH = 4; // Gloves length is fixed in format

export interface ZsprData {
  title: string;
  author: string;
  authorAscii: string;
  content: Buffer; // Concatenated sprite, palette, and gloves data
}

export class ZsprParser {
  public static parse(filePathOrBuffer: string | Buffer): ZsprData {
    let buffer: Buffer;
    if (typeof filePathOrBuffer === "string") {
      buffer = fs.readFileSync(filePathOrBuffer);
    } else {
      buffer = filePathOrBuffer;
    }

    let offset = 0;

    // Read and validate header
    if (buffer.length - offset < 4) {
      throw new Error("Buffer length too short for ZSPR header.");
    }
    const header = buffer.toString("ascii", offset, offset + 4);
    offset += 4;
    if (header !== ZSPR_HEADER) {
      throw new Error("Invalid ZSPR file format: Header mismatch.");
    }

    // Read and validate version
    const version = buffer.readUInt8(offset);
    offset += 1;
    if (version !== ZSPR_VERSION) {
      throw new Error(
        `Unsupported ZSPR version: ${version}. Expected: ${ZSPR_VERSION}.`,
      );
    }

    // Skip checksum (4 bytes) - C# stream.Position += 4;
    offset += 4;

    // Read offsets for sprite and palette
    const spriteOffset = buffer.readUInt32LE(offset);
    offset += 4;
    // Sprite length (2 bytes)
    const spriteLength = buffer.readUInt16LE(offset);
    offset += 2;
    const paletteOffset = buffer.readUInt32LE(offset);
    offset += 4;
    // Palette length (2 bytes)
    const paletteLength = buffer.readUInt16LE(offset);
    offset += 2;

    // Read and validate sprite type
    const type = buffer.readUInt16LE(offset);
    offset += 2;
    if (type !== SPRITE_TYPE_LINK) {
      throw new Error(
        `Unsupported sprite type: ${type}. Expected Link Sprite (type ${SPRITE_TYPE_LINK}).`,
      );
    }

    // Skip reserved bytes (6 bytes) - C# stream.Position += 6;
    offset += 6;

    // --- Metadata Reading ---
    // The current `offset` is where metadata (title, author, authorAscii) begins.

    // Helper to read null-terminated UTF16LE strings
    const readNullTerminatedUtf16String = (
      currentBuffer: Buffer,
      startOffset: number,
    ): { str: string; bytesRead: number } => {
      let end = startOffset;
      // Find the null terminator (two zero bytes for UTF-16)
      while (end < currentBuffer.length - 1) {
        if (currentBuffer[end] === 0 && currentBuffer[end + 1] === 0) {
          break;
        }
        end += 2; // UTF-16 characters are 2 bytes
      }
      const str = currentBuffer.toString("utf16le", startOffset, end);
      const bytesRead = end - startOffset + 2; // Include the null terminator bytes
      return { str, bytesRead };
    };

    const titleResult = readNullTerminatedUtf16String(buffer, offset);
    const title = titleResult.str;
    offset += titleResult.bytesRead;

    const authorResult = readNullTerminatedUtf16String(buffer, offset);
    const author = authorResult.str;
    offset += authorResult.bytesRead;

    // Helper to read null-terminated ASCII strings
    const readNullTerminatedAsciiString = (
      currentBuffer: Buffer,
      startOffset: number,
    ): { str: string; bytesRead: number } => {
      let end = startOffset;
      while (end < currentBuffer.length) {
        if (currentBuffer[end] === 0) {
          break;
        }
        end += 1;
      }
      const str = currentBuffer.toString("ascii", startOffset, end);
      const bytesRead = end - startOffset + 1; // Include the null terminator byte
      return { str, bytesRead };
    };

    // authorAscii is read after title and author, from the current stream position
    const authorAsciiResult = readNullTerminatedAsciiString(buffer, offset);
    const authorAscii = authorAsciiResult.str;
    offset += authorAsciiResult.bytesRead; // Advance offset past authorAscii

    // --- Content Parsing ---
    // As per C# ParseContent: sprite, palette, gloves
    // Offsets are absolute from the start of the file.

    // 1. Sprite data (use header-provided length; fallback to default for robustness)
    const spriteDataLength = spriteLength || DEFAULT_SPRITE_DATA_LENGTH;
    if (spriteOffset + spriteDataLength > buffer.length) {
      throw new Error("Sprite data extends beyond buffer length.");
    }
    const spriteData = Buffer.alloc(spriteDataLength);
    buffer.copy(spriteData, 0, spriteOffset, spriteOffset + spriteDataLength);

    // 2. Palette data
    const paletteDataLength = paletteLength || DEFAULT_PALETTE_DATA_LENGTH;
    if (paletteOffset + paletteDataLength > buffer.length) {
      throw new Error("Palette data extends beyond buffer length.");
    }
    const paletteData = Buffer.alloc(paletteDataLength);
    buffer.copy(
      paletteData,
      0,
      paletteOffset,
      paletteOffset + paletteDataLength,
    );

    // 3. Gloves data
    // The offset for gloves data is immediately after the palette data in the ZSPR structure,
    // absolute at `paletteOffset + paletteLength`.
    const glovesDataOffset = paletteOffset + paletteDataLength;
    if (glovesDataOffset + GLOVES_DATA_LENGTH > buffer.length) {
      throw new Error("Gloves data extends beyond buffer length.");
    }
    const glovesData = Buffer.alloc(GLOVES_DATA_LENGTH);
    buffer.copy(
      glovesData,
      0,
      glovesDataOffset,
      glovesDataOffset + GLOVES_DATA_LENGTH,
    );

    const content = Buffer.concat([spriteData, paletteData, glovesData]);

    return {
      title,
      author,
      authorAscii,
      content,
    };
  }
}
