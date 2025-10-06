import { Buffer } from "buffer";

/**
 * Interface for a data block within an RDC file.
 */
export interface BlockType {
  readonly type: number;
  readonly length: number; // Total length of the block's data segment in the RDC file.
  parse(stream: Buffer, offset: number): void; // Parses data from the stream into the block
  write(stream: Buffer, offset: number): void; // Writes the block's data to the stream
}

/**
 * Represents the metadata block (type 0) in an RDC file.
 * This block stores JSON content.
 */
export class MetaDataBlock implements BlockType {
  static readonly RDC_TYPE_ID = 0;
  public readonly type: number = MetaDataBlock.RDC_TYPE_ID;
  private _content: Record<string, any> = {};
  private _jsonByteLength: number = 0; // Length of the JSON string in bytes

  public get content(): Record<string, any> {
    return this._content;
  }

  public set content(value: Record<string, any>) {
    this._content = value;
    const jsonString = JSON.stringify(this._content);
    this._jsonByteLength = Buffer.from(jsonString, "utf8").length;
  }

  /**
   * Gets the length of the data segment for this block.
   * For MetaDataBlock, this is 4 bytes for the JSON length prefix + JSON string bytes.
   * This is the value that would be stored in the RDC block's "TotalBlockLength" field,
   * MINUS 8 (for the RDC BlockTypeID and TotalBlockLength fields themselves).
   * Or, more simply, it's the length of the data that this block's Parse/Write methods handle.
   */
  public get length(): number {
    return 4 + this._jsonByteLength;
  }

  constructor(content?: Record<string, any>) {
    if (content) {
      this.content = content;
    } else {
      this._content = {};
      this._jsonByteLength = Buffer.from(
        JSON.stringify(this._content),
        "utf8",
      ).length;
    }
  }

  /**
   * Parses the metadata block from the given stream (RDC file buffer) at the specified offset.
   * The `offset` parameter points to the beginning of this block's specific data in the RDC file.
   * For MetaDataBlock, this means `offset` points to where the JSON length (UInt32) is stored.
   * (This assumes the caller, Rdc.tryParseBlock, has already skipped the generic RDC block TypeID and TotalBlockLength fields).
   *
   * Correction from previous interpretation: If Rdc.tryParseBlock passes the offset pointing to BlockTypeID,
   * then this parse method needs to skip 8 bytes to get to its actual data.
   * Based on the last subtask, `Rdc.tryParseBlock` passes `blockOffset` (points to BlockTypeID).
   * So, this method MUST skip 8 bytes.
   */
  public parse(stream: Buffer, offset: number): void {
    // offset is the exact starting point of MetaDataBlock's specific data (i.e., where JSON length is stored).
    const dataView = new DataView(
      stream.buffer,
      stream.byteOffset,
      stream.byteLength,
    );

    const jsonLength = dataView.getUint32(offset, true); // true for little-endian
    this._jsonByteLength = jsonLength;

    const jsonStringOffset = offset + 4;
    if (jsonStringOffset + jsonLength > stream.length) {
      throw new Error("Metadata JSON content extends beyond buffer length.");
    }
    const jsonBytes = stream.subarray(
      jsonStringOffset,
      jsonStringOffset + jsonLength,
    );
    const jsonString = jsonBytes.toString("utf8");

    try {
      this._content = JSON.parse(jsonString);
    } catch (e) {
      throw new Error(`Failed to parse JSON content: ${e}`);
    }
  }

  /**
   * Writes the metadata block to the given stream at the specified offset.
   * The `offset` parameter points to where this block's specific data (JSON length prefix) should begin.
   */
  public write(stream: Buffer, offset: number): void {
    // This method writes *only* the MetaDataBlock's specific data (JSON length + JSON string).
    // The RDC writer is responsible for writing the BlockTypeID and TotalBlockLength fields.
    const jsonString = JSON.stringify(this._content);
    const jsonBytes = Buffer.from(jsonString, "utf8");
    this._jsonByteLength = jsonBytes.length;

    if (offset + 4 + jsonBytes.length > stream.length) {
      throw new Error(
        "Not enough space in stream to write MetaDataBlock data.",
      );
    }

    const dataView = new DataView(
      stream.buffer,
      stream.byteOffset,
      stream.byteLength,
    );
    dataView.setUint32(offset, this._jsonByteLength, true); // Write JSON length
    jsonBytes.copy(stream, offset + 4); // Write JSON bytes
  }
}

// Manifest entry types for DataBlock
type ManifestEntrySimple = [number[], number, number[]];
type ManifestEntry = ManifestEntrySimple;

/**
 * Abstract base class for data blocks (e.g., sprite data, palette data).
 */
export abstract class DataBlock implements BlockType {
  abstract get type(): number;
  public abstract get manifest(): Array<ManifestEntry>; // Exposed publicly for inspection/testing

  protected _content: Buffer[] = [];

  /**
   * Gets the raw content buffers. Each buffer corresponds to a segment defined in the manifest.
   */
  public get content(): Buffer[] {
    return this._content;
  }

  /**
   * Calculates the total length of all data segments defined in the manifest.
   * This is the length of the data this block is responsible for in the RDC file.
   */
  public get length(): number {
    let totalLength = 0;
    for (const entry of this.manifest) {
      const [, dataSegmentLength, offsetsArray] = entry;
      totalLength += dataSegmentLength * offsetsArray.length;
    }
    return totalLength;
  }

  /**
   * Parses the data block from the RDC buffer.
   * `offset` points to the start of this block's RDC entry (i.e., its BlockTypeID).
   * This method must skip the BlockTypeID (4 bytes) and TotalBlockLength (4 bytes)
   * before reading its specific content.
   */
  public parse(rdcBuffer: Buffer, offset: number): void {
    this._content = []; // Clear any previous content
    // offset is the exact starting point of this DataBlock's content in the RDC file.
    let currentOffsetInBlockData = offset;

    for (const entry of this.manifest) {
      const [, dataSegmentLength, offsetsArray] = entry;
      const currentSegmentTotalLength = dataSegmentLength * offsetsArray.length;

      if (
        currentOffsetInBlockData + currentSegmentTotalLength >
        rdcBuffer.length
      ) {
        throw new Error(
          `Data segment for type ${this.type} extends beyond RDC buffer length. Offset: ${currentOffsetInBlockData}, SegmentLength: ${currentSegmentTotalLength}, BufferLength: ${rdcBuffer.length}`,
        );
      }

      const segmentData = Buffer.alloc(currentSegmentTotalLength);
      rdcBuffer.copy(
        segmentData,
        0,
        currentOffsetInBlockData,
        currentOffsetInBlockData + currentSegmentTotalLength,
      );
      this._content.push(segmentData);

      currentOffsetInBlockData += currentSegmentTotalLength;
    }
  }

  /**
   * Writes the block's data to the stream. (Deferred)
   * `offset` points to where this block's specific data should begin.
   */
  public write(stream: Buffer, offset: number): void {
    // The RDC writer handles BlockTypeID and TotalBlockLength.
    // This method writes the concatenated content buffers.
    let currentWriteOffset = offset;
    for (const buffer of this._content) {
      if (currentWriteOffset + buffer.length > stream.length) {
        throw new Error(
          "Not enough space in stream to write DataBlock content.",
        );
      }
      buffer.copy(stream, currentWriteOffset);
      currentWriteOffset += buffer.length;
    }
  }

  /**
   * Applies the data to a ROM buffer. (Deferred)
   */
  public apply(_rom: Buffer, _mapping?: string): void {
    throw new Error("Apply method not implemented.");
  }

  /**
   * Sets the content buffers for the DataBlock.
   * This is primarily used for constructing the block from external data (e.g., ZSPR).
   * The provided buffers must match the structure expected by the manifest.
   * @param buffers An array of Buffers, matching the order and number of segments in the manifest.
   */
  public setContent(buffers: Buffer[]): void {
    if (buffers.length !== this.manifest.length) {
      throw new Error(
        `Invalid number of buffers provided. Expected ${this.manifest.length}, got ${buffers.length}.`,
      );
    }
    // Optional: Add length checks for each buffer against manifest lengths if needed.
    // For now, assume the caller provides correctly structured and sized buffers.
    this._content = buffers;
  }

  protected static single(): number[] {
    return [0]; // Represents a single entry, offset 0 relative to its segment start
  }

  protected static addr(...addrs: number[]): number[] {
    return addrs; // For now, just return all addresses; SNES mapping logic is separate
  }

  protected static offsets(n: number, offsetValue: number): number[] {
    return Array.from({ length: n }, (_, i) => i * offsetValue);
  }
}

/**
 * Link Sprite data block (type 1).
 */
export class LinkSprite extends DataBlock {
  static readonly RDC_TYPE_ID = 1;
  get type(): number {
    return LinkSprite.RDC_TYPE_ID;
  }

  public get manifest(): Array<ManifestEntry> {
    return [
      // NOTE: These are the canonical LoROM addresses (banks $10/$1B) for Link's sprite assets.
      // Previous values using $50/$5B were mirror banks; keeping a single canonical form avoids
      // accidental mismatches with snesToPc() which expects the lower mirror.
      // sprite: Addr(0x108000), 0x7000, Single
      [DataBlock.addr(0x108000), 0x7000, DataBlock.single()],
      // palette: Addr(0x1BD308), 4 * 30, Single
      [DataBlock.addr(0x1bd308), 4 * 30, DataBlock.single()],
      // gloves: Addr(0x1BEDF5), 4, Single
      [DataBlock.addr(0x1bedf5), 4, DataBlock.single()],
    ];
  }

  /**
   * Extracts a 0x20 byte tile from the sprite data.
   * @param tileIndex The index of the tile.
   * @returns A Buffer containing the tile data.
   */
  fetch8x8(tileIndex: number): Buffer {
    if (!this.content[0]) {
      throw new Error("Sprite data (content[0]) not loaded.");
    }
    const spriteData = this.content[0];
    const tileOffset = tileIndex * 0x20;
    if (tileOffset < 0 || tileOffset + 0x20 > spriteData.length) {
      throw new Error(`Tile index ${tileIndex} is out of bounds.`);
    }
    return spriteData.subarray(tileOffset, tileOffset + 0x20);
  }

  /**
   * Extracts a 30-byte palette.
   * @param index The index of the palette (0-based).
   * @returns A Buffer containing the palette data.
   */
  fetchPalette(index: number): Buffer {
    if (!this.content[1]) {
      throw new Error("Palette data (content[1]) not loaded.");
    }
    const paletteData = this.content[1];
    const paletteOffset = index * 30;
    // Palettes are 30 bytes (15 colors * 2 bytes/color)
    if (paletteOffset < 0 || paletteOffset + 30 > paletteData.length) {
      throw new Error(`Palette index ${index} is out of bounds.`);
    }
    return paletteData.subarray(paletteOffset, paletteOffset + 30);
  }

  /**
   * Sets the content buffers for the LinkSprite.
   * This is primarily used for constructing the block from external data (e.g., ZSPR).
   * The provided buffers must match the structure expected by the manifest (3 segments).
   * @param buffers An array of Buffers: [gfxData, paletteData, glovesData].
   */
  public setContent(buffers: Buffer[]): void {
    if (buffers.length !== this.manifest.length) {
      throw new Error(
        `Invalid number of buffers provided. Expected ${this.manifest.length}, got ${buffers.length}.`,
      );
    }
    // Optional: Add length checks for each buffer against manifest lengths if needed.
    // For now, assume the caller provides correctly structured and sized buffers.
    this._content = buffers;
  }
}

/**
 * Zelda 1 Sprite data block (type 2).
 */
export class Zelda1SpriteDataBlock extends DataBlock {
  static readonly RDC_TYPE_ID = 2;
  get type(): number {
    return Zelda1SpriteDataBlock.RDC_TYPE_ID;
  }
  public get manifest(): Array<ManifestEntry> {
    return [
      [DataBlock.addr(0x608e34), 32, DataBlock.single()], // $LIFTING_ITEM
      [DataBlock.addr(0x608eb4), 32, DataBlock.single()], // $WALK1_PROFILE_BIGSHIELD
      [DataBlock.addr(0x61007f), 448, DataBlock.single()], // $WALK1_PROFILE, $WALK2_PROFILE, $FACING_DOWN_NOSHIELD, $FACING_UP, $ATTACKING_PROFILE, $ATTACKING_DOWN, $ATTACKING_UP
      [DataBlock.addr(0x6105bf), 32, DataBlock.single()], // $WALK2_PROFILE_BIGSHIELD
      [DataBlock.addr(0x6105ff), 64, DataBlock.single()], // $WALK1_DOWN_SMALLSHIELD, $WALK2_DOWN_SMALLSHIELD
      [DataBlock.addr(0x61067f), 32, DataBlock.single()], // $FACING_DOWN_BIGSHIELD
      [
        DataBlock.addr(
          0x631314,
          0x631410,
          0x63150c,
          0x631608,
          0x631704,
          0x631800,
          0x6318fc,
          0x6319f8,
          0x631af4,
          0x631bf0,
          0x631cec,
          0x3d3804,
        ),
        3,
        DataBlock.single(),
      ], // $BASE_COLORS
      [DataBlock.addr(0x631cf0), 3, DataBlock.single()], // $LEVEL2_COLORS
      [DataBlock.addr(0x631cf4), 3, DataBlock.single()], // $LEVEL3_COLORS
      [DataBlock.addr(0x612287), 3, DataBlock.single()], // $TUNIC_COLORS
    ];
  }
}

/**
 * Metroid 1 Sprite data block (type 3).
 * Detailed segment layout matching the manifest used in game-static-info.ts.
 * Addresses here are placeholders (0x0000 etc.) because for RDC extraction we only care about
 * segment ordering and lengths; actual ROM target addresses live in web runtime manifests.
 */
export class Metroid1SpriteDataBlock extends DataBlock {
  static readonly RDC_TYPE_ID = 3;
  get type(): number {
    return Metroid1SpriteDataBlock.RDC_TYPE_ID;
  }
  public get manifest(): Array<ManifestEntry> {
    // We emulate the structure of the runtime manifest but use sequential pseudo-address groups
    // to delineate segments. Each segment is a single chunk; color segments are replicated at apply time
    // via addresses array in the runtime manifest (not needed here). Therefore we only list one addr per segment.
    return [
      [DataBlock.addr(0x0000), 64, DataBlock.single()],
      [DataBlock.addr(0x0001), 80, DataBlock.single()],
      [DataBlock.addr(0x0002), 64, DataBlock.single()],
      [DataBlock.addr(0x0003), 16, DataBlock.single()],
      [DataBlock.addr(0x0004), 96, DataBlock.single()],
      [DataBlock.addr(0x0005), 64, DataBlock.single()],
      [DataBlock.addr(0x0006), 48, DataBlock.single()],
      [DataBlock.addr(0x0007), 96, DataBlock.single()],
      [DataBlock.addr(0x0008), 96, DataBlock.single()],
      [DataBlock.addr(0x0009), 16, DataBlock.single()],
      [DataBlock.addr(0x000a), 32, DataBlock.single()],
      [DataBlock.addr(0x000b), 96, DataBlock.single()],
      [DataBlock.addr(0x000c), 48, DataBlock.single()],
      [DataBlock.addr(0x000d), 112, DataBlock.single()],
      [DataBlock.addr(0x000e), 112, DataBlock.single()],
      [DataBlock.addr(0x000f), 16, DataBlock.single()],
      [DataBlock.addr(0x0010), 32, DataBlock.single()],
      [DataBlock.addr(0x0011), 64, DataBlock.single()],
      [DataBlock.addr(0x0012), 3, DataBlock.single()],
      [DataBlock.addr(0x0013), 2, DataBlock.single()],
      [DataBlock.addr(0x0014), 2, DataBlock.single()],
      [DataBlock.addr(0x0015), 2, DataBlock.single()],
      [DataBlock.addr(0x0016), 2, DataBlock.single()],
    ];
  }
}

// Loader offsets for Samus sprite, from C# SamusSprite.loaderOffsets
const _SAMUS_LOADER_OFFSETS: number[] = [
  0x0, 0x24, 0x48, 0x6c, 0x90, 0xb4, 0xd8, 0xfc, 0x120, 0x144, 0x168, 0x18c,
  0x1b0, 0x1d4, 0x1f8, 0x21c, 0x240, 0x264, 0x288, 0x2ac, 0x2d0, 0x2f4, 0x318,
  0x33c,
];

/**
 * Samus Sprite data block (type 4).
 */
export class SamusSprite extends DataBlock {
  static readonly RDC_TYPE_ID = 4;
  get type(): number {
    return SamusSprite.RDC_TYPE_ID;
  }

  public get manifest(): Array<ManifestEntry> {
    // The static manifest is not used for parsing in this implementation.
    // We keep a minimal placeholder to satisfy abstract requirements.
    return [];
  }

  /**
   * Extracts a 0x20 byte tile from the sprite sheet data.
   * Sprite sheets are concatenated in content[0] through content[19].
   * Each sheet is 0x8000 bytes.
   * @param tileIndex The global index of the tile.
   * @returns A Buffer containing the tile data.
   */
  fetchDma8x8(tileIndex: number): Buffer {
    const tileAddr = tileIndex * 0x20; // Each tile is 0x20 bytes
    const bankSize = 0x8000; // Each sprite sheet is 0x8000 bytes

    const bankIndex = Math.floor(tileAddr / bankSize);
    const offsetInBank = tileAddr % bankSize;

    if (bankIndex < 0 || bankIndex >= 14) {
      // Samus has 14 DMA banks in RDC (13 full + 1 partial)
      throw new Error(
        `Tile index ${tileIndex} results in invalid bank index ${bankIndex}.`,
      );
    }
    if (!this.content[bankIndex]) {
      throw new Error(
        `Sprite data for bank ${bankIndex} (content[${bankIndex}]) not loaded.`,
      );
    }

    const bankData = this.content[bankIndex];
    if (offsetInBank + 0x20 > bankData.length) {
      throw new Error(
        `Tile index ${tileIndex} (offset ${offsetInBank}) is out of bounds for bank ${bankIndex} of length ${bankData.length}.`,
      );
    }
    return bankData.subarray(offsetInBank, offsetInBank + 0x20);
  }

  /**
   * Fetches the "Power Standard" palette group.
   * This corresponds to the 21st entry in the manifest (index 20).
   * This entry uses `loaderOffsets`, so it's a group of 24 palettes, each 30 bytes.
   * The method returns the entire Buffer for this group (24 * 30 = 720 bytes).
   * @returns A Buffer containing the "Power Standard" palette group.
   */
  fetchPowerStandardPalette(): Buffer {
    // According to the C# reference, the "Power Standard" palette is the 21st segment (index 20)
    // within the SamusSprite data block.
    if (this._content[20]) {
      return this._content[20];
    }
    // Fallback: if parsing did not segment as expected, return the last segment
    const fallbackIndex = this._content.length - 1;
    if (fallbackIndex < 0)
      throw new Error("Power Standard palette data not loaded.");
    return this._content[fallbackIndex];
  }

  /**
   * Sets the content buffers for the SamusSprite.
   * This is primarily used for constructing the block from external data.
   * The provided buffers must match the structure expected by the manifest.
   * @param buffers An array of Buffers, matching the order and number of segments in the manifest.
   */
  public setContent(buffers: Buffer[]): void {
    if (buffers.length !== this.manifest.length) {
      throw new Error(
        `Invalid number of buffers provided. Expected ${this.manifest.length}, got ${buffers.length}.`,
      );
    }
    // Optional: Add length checks for each buffer against manifest segment lengths if needed.
    this._content = buffers;
  }
  /**
   * Parse the Samus RDC block using the C# reference layout for the initial segments
   * so that DMA banks and the "Power Standard" palette land at the correct indices.
   * We read the following segments in order:
   *  - 13 DMA banks of 0x8000 bytes
   *  - 1 DMA bank of 0x7880 bytes (bank 14)
   *  - Death left  (0x3F60)
   *  - Death right (0x3F60)
   *  - Gun port    (0x03C0)
   *  - File select sprites (0x0600)
   *  - File select missile (0x0020)
   *  - File select missile head (0x0020)
   *  - Power Standard palette (0x001E)
   * Any remaining data is appended as a single trailing segment for downstream uses.
   */
  public parse(rdcBuffer: Buffer, offset: number): void {
    const lengths: number[] = [
      // 13 full DMA banks
      0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
      0x8000, 0x8000, 0x8000, 0x8000,
      // 14th DMA bank (partial)
      0x7880,
      // death left/right, gun port, file select assets
      0x3f60, 0x3f60, 0x03c0, 0x0600, 0x0020, 0x0020,
      // Power Standard palette (30 bytes)
      0x001e,
    ];

    const segments: Buffer[] = [];
    let cursor = offset;
    for (const len of lengths) {
      if (cursor + len > rdcBuffer.length) {
        throw new Error(
          `SamusSprite.parse: segment overruns buffer (need ${len} at ${cursor}, buffer ${rdcBuffer.length}).`,
        );
      }
      segments.push(rdcBuffer.subarray(cursor, cursor + len));
      cursor += len;
    }

    // Append any remaining data as a single trailing segment (optional palettes, etc.)
    if (cursor < rdcBuffer.length) {
      segments.push(rdcBuffer.subarray(cursor));
    }

    this._content = segments;
  }
}
