import { Buffer } from "buffer";
import { BlockType } from "./rdc-types";

const RDC_HEADER = "RETRODATACONTAINER";
const RDC_VERSION = 1;
const RDC_HEADER_LENGTH = RDC_HEADER.length; // 18 bytes

/**
 * Helper function to read a null-terminated ASCII string from a buffer.
 * @param buffer The buffer to read from.
 * @param offset The starting offset in the buffer.
 * @returns An object containing the decoded string and the total length read (including null terminator).
 */
function readNullTerminatedAscii(
  buffer: Buffer,
  offset: number,
): { text: string; length: number } {
  let end = offset;
  let foundTerminator = false;
  while (end < buffer.length) {
    if (buffer[end] === 0) {
      foundTerminator = true;
      break;
    }
    end++;
  }
  if (!foundTerminator) {
    throw new Error(
      "Buffer too short to read author string (missing null terminator or data).",
    );
  }
  const text = buffer.toString("ascii", offset, end);
  // Include the null terminator
  return { text, length: end - offset + 1 };
}

export class Rdc {
  public readonly header: string = RDC_HEADER;
  public readonly version: number = RDC_VERSION;
  public author: string;
  public offsets: Map<number, number>; // blockType -> fileOffset
  public parsedBlocks: Map<number, BlockType>; // blockType -> parsed BlockType instance

  constructor(author: string, offsets: Map<number, number>) {
    this.author = author;
    this.offsets = offsets;
    this.parsedBlocks = new Map<number, BlockType>();
  }

  /**
   * Parses an RDC file from a Buffer.
   * @param fileBuffer The buffer containing the RDC file data.
   * @returns An instance of the Rdc class.
   */
  public static parse(fileBuffer: Buffer): Rdc {
    const dataView = new DataView(
      fileBuffer.buffer,
      fileBuffer.byteOffset,
      fileBuffer.byteLength,
    );
    let cursor = 0;

    // 1. Header Validation
    if (cursor + RDC_HEADER_LENGTH > fileBuffer.length) {
      throw new Error("Buffer too short to contain RDC header.");
    }
    const headerStr = fileBuffer.toString(
      "ascii",
      cursor,
      cursor + RDC_HEADER_LENGTH,
    );
    cursor += RDC_HEADER_LENGTH;
    if (headerStr !== RDC_HEADER) {
      throw new Error(
        `Invalid RDC header. Expected "${RDC_HEADER}", got "${headerStr}".`,
      );
    }

    // 2. Version Check
    if (cursor >= fileBuffer.length) {
      throw new Error("Buffer too short to contain RDC version.");
    }
    const versionNum = dataView.getUint8(cursor);
    cursor += 1;
    if (versionNum !== RDC_VERSION) {
      throw new Error(
        `Unsupported RDC version. Expected ${RDC_VERSION}, got ${versionNum}.`,
      );
    }

    // 3. Block Offsets Parsing
    if (cursor + 4 > fileBuffer.length) {
      // 4 bytes for numBlocks
      throw new Error("Buffer too short to read number of blocks.");
    }
    const numBlocks = dataView.getUint32(cursor, true); // true for little-endian
    cursor += 4;

    const offsets = new Map<number, number>();
    for (let i = 0; i < numBlocks; i++) {
      if (cursor + 8 > fileBuffer.length) {
        // 4 bytes for blockType, 4 for blockOffset
        throw new Error(`Buffer too short to read block entry ${i + 1}.`);
      }
      const blockType = dataView.getUint32(cursor, true);
      cursor += 4;
      const blockOffset = dataView.getUint32(cursor, true);
      cursor += 4;
      offsets.set(blockType, blockOffset);
    }

    // 4. Author Parsing
    if (cursor >= fileBuffer.length) {
      throw new Error(
        "Buffer too short to read author string (missing null terminator or data).",
      );
    }
    const authorResult = readNullTerminatedAscii(fileBuffer, cursor);
    const author = authorResult.text;
    cursor += authorResult.length; // Advance cursor by the length of author string + null terminator

    // 5. Constructor Call
    return new Rdc(author, offsets);
  }

  /**
   * Checks if the RDC file contains a block of the specified type.
   * @param blockTypeNumber The numerical type of the block.
   * @returns True if the block type exists in the RDC file's offsets, false otherwise.
   */
  public contains(blockTypeNumber: number): boolean {
    return this.offsets.has(blockTypeNumber);
  }

  /**
   * Tries to parse a specific block from the RDC file.
   * If the block has been parsed before, it returns the cached version.
   * @param fileBuffer The buffer containing the RDC file data.
   * @param blockConstructor The constructor of the block type to parse (e.g., MetaDataBlock).
   * @returns An instance of the parsed block, or undefined if the block type is not found.
   */
  public tryParseBlock<T extends BlockType>(
    fileBuffer: Buffer,
    blockConstructor: new () => T,
  ): T | undefined {
    // Create a temporary instance to get its type property
    // This assumes the type property is available on the instance without parsing.
    const tempInstance = new blockConstructor();
    const blockType = tempInstance.type;

    if (!this.offsets.has(blockType)) {
      return undefined; // Block type not listed in RDC offsets
    }

    if (this.parsedBlocks.has(blockType)) {
      return this.parsedBlocks.get(blockType) as T; // Return cached block
    }

    const blockOffset = this.offsets.get(blockType)!; // Existence checked by offsets.has()

    // Offsets in RDC point directly to the start of the block's data.
    // Each block defines its own structure; MetaData starts with a uint32 length; DataBlocks are raw segments.
    if (blockOffset < 0 || blockOffset >= fileBuffer.length) {
      throw new Error(
        `Block offset ${blockOffset} out of bounds for block type ${blockType}.`,
      );
    }

    const block = new blockConstructor();
    try {
      // Determine an end boundary for dynamic blocks (like SamusSprite) by looking at next block offset.
      let endExclusive = fileBuffer.length;
      const sortedOffsets = Array.from(this.offsets.values()).sort(
        (a, b) => a - b,
      );
      const idx = sortedOffsets.indexOf(blockOffset);
      if (idx >= 0) {
        for (let i = idx + 1; i < sortedOffsets.length; i++) {
          const candidate = sortedOffsets[i];
          if (candidate > blockOffset) {
            endExclusive = candidate;
            break;
          }
        }
      }
      // Pass a slice representing only this block's region to parse for dynamic sizing.
      const slice = fileBuffer.subarray(blockOffset, endExclusive);
      block.parse(slice, 0);
    } catch (e: unknown) {
      if (typeof e === "object" && e !== null && "message" in e) {
        console.error(
          `Error parsing block type ${blockType} at offset ${blockOffset}: ${(e as { message?: string }).message}`,
        );
      } else {
        console.error(
          `Error parsing block type ${blockType} at offset ${blockOffset}:`,
          e,
        );
      }
      throw e;
    }

    this.parsedBlocks.set(block.type, block);
    return block;
  }

  public static write(author: string, blocks: BlockType[]): Buffer {
    // Calculate sizes
    const authorBytes = Buffer.from(author, "ascii");
    const authorLengthWithNull = authorBytes.length + 1;

    const offsetTableSize = blocks.length * 8; // type (4) + offset (4)
    const totalBlocksDataSize = blocks.reduce((sum, b) => sum + b.length, 0);

    const totalSize =
      RDC_HEADER_LENGTH +
      1 +
      4 +
      offsetTableSize +
      authorLengthWithNull +
      totalBlocksDataSize;

    const rdcBuffer = Buffer.alloc(totalSize);
    const dataView = new DataView(
      rdcBuffer.buffer,
      rdcBuffer.byteOffset,
      rdcBuffer.byteLength,
    );
    let cursor = 0;

    // Header + version
    rdcBuffer.write(RDC_HEADER, cursor, "ascii");
    cursor += RDC_HEADER_LENGTH;
    dataView.setUint8(cursor, RDC_VERSION);
    cursor += 1;

    // Precompute offsets (block data starts after table + author)
    const firstBlockOffset =
      RDC_HEADER_LENGTH + 1 + 4 + offsetTableSize + authorLengthWithNull;
    const blockFileOffsets: number[] = [];
    {
      let running = firstBlockOffset;
      for (const b of blocks) {
        blockFileOffsets.push(running);
        running += b.length;
      }
    }

    // Num blocks + offset table
    dataView.setUint32(cursor, blocks.length, true);
    cursor += 4;
    for (let i = 0; i < blocks.length; i++) {
      dataView.setUint32(cursor, blocks[i].type, true);
      cursor += 4;
      dataView.setUint32(cursor, blockFileOffsets[i], true);
      cursor += 4;
    }

    // Author (null-terminated)
    authorBytes.copy(rdcBuffer, cursor);
    cursor += authorBytes.length;
    rdcBuffer.writeUInt8(0, cursor);
    cursor += 1;

    // Block data (no per-block envelope)
    for (let i = 0; i < blocks.length; i++) {
      const block = blocks[i];
      const writeOffset = blockFileOffsets[i];
      block.write(rdcBuffer, writeOffset);
    }

    return rdcBuffer;
  }
}
