import { describe, it, expect, vi } from "vitest";
import { Rdc } from "../rdc";
import {
  MetaDataBlock,
  BlockType,
  LinkSprite,
  Zelda1SpriteDataBlock,
  Metroid1SpriteDataBlock,
} from "../rdc-types";
import { Buffer } from "buffer";

const RDC_HEADER_STRING = "RETRODATACONTAINER";
const RDC_VERSION_NUM = 1;

interface MockBlockDefinition {
  typeId: number;
  content: Record<string, any> | Buffer; // JSON object for MetaData, Buffer for others
}

// Helper to create Mock RDC Buffer
function createMockRdcBuffer({
  author = "TestAuthor",
  blocks = [],
  overrideHeader,
  overrideVersion,
  customNumBlocks, // To test discrepancies
  customBlockDescriptors, // To test malformed descriptors
  customAuthorSuffix = Buffer.from([0]), // Null terminator
  noAuthorNullTerminator = false,
  truncateAuthor,
  truncateAfterHeader,
  truncateAfterVersion,
  truncateAfterNumBlocks,
  truncateAfterDescriptors,
  truncateAfterAuthor,
}: {
  author?: string;
  blocks?: MockBlockDefinition[];
  overrideHeader?: string;
  overrideVersion?: number;
  customNumBlocks?: number;
  customBlockDescriptors?: Buffer;
  customAuthorSuffix?: Buffer;
  noAuthorNullTerminator?: boolean;
  truncateAuthor?: number;
  truncateAfterHeader?: boolean;
  truncateAfterVersion?: boolean;
  truncateAfterNumBlocks?: boolean;
  truncateAfterDescriptors?: boolean;
  truncateAfterAuthor?: boolean;
}): Buffer {
  const header = Buffer.from(overrideHeader || RDC_HEADER_STRING, "ascii");
  const version = Buffer.from([
    overrideVersion === undefined ? RDC_VERSION_NUM : overrideVersion,
  ]);

  const numBlocks =
    customNumBlocks === undefined ? blocks.length : customNumBlocks;
  const numBlocksBuffer = Buffer.alloc(4);
  numBlocksBuffer.writeUInt32LE(numBlocks, 0);

  const authorBuffer = Buffer.from(author, "ascii");
  const authorSegment = noAuthorNullTerminator
    ? authorBuffer
    : Buffer.concat([authorBuffer, customAuthorSuffix]);

  let currentOffset =
    header.length +
    version.length +
    numBlocksBuffer.length +
    numBlocks * 8 /*descriptors*/ +
    authorSegment.length;

  const blockDescriptors: Buffer[] = [];
  const blockDataSegments: Buffer[] = [];

  for (const blockDef of blocks) {
    let blockSpecificData: Buffer;
    if (
      blockDef.typeId === MetaDataBlock.RDC_TYPE_ID &&
      typeof blockDef.content !== "function" &&
      !Buffer.isBuffer(blockDef.content)
    ) {
      const jsonString = JSON.stringify(blockDef.content);
      const jsonBytes = Buffer.from(jsonString, "utf8");
      const lenPrefix = Buffer.alloc(4);
      lenPrefix.writeUInt32LE(jsonBytes.length, 0);
      blockSpecificData = Buffer.concat([lenPrefix, jsonBytes]);
    } else if (Buffer.isBuffer(blockDef.content)) {
      blockSpecificData = blockDef.content;
    } else {
      throw new Error(
        `Block content for type ${blockDef.typeId} must be a Buffer or JSON object for MetaDataBlock.`,
      );
    }

    // In RDC, the offset table points directly to the start of each block's data (no per-block envelope)
    const descriptor = Buffer.alloc(8);
    descriptor.writeUInt32LE(blockDef.typeId, 0); // Block Type ID
    descriptor.writeUInt32LE(currentOffset, 4); // Offset to this block's data
    blockDescriptors.push(descriptor);

    blockDataSegments.push(blockSpecificData);
    currentOffset += blockSpecificData.length;
  }

  const finalBlockDescriptors =
    customBlockDescriptors || Buffer.concat(blockDescriptors);
  const finalBlockData = Buffer.concat(blockDataSegments);

  let components = [
    header,
    version,
    numBlocksBuffer,
    finalBlockDescriptors,
    authorSegment,
    finalBlockData,
  ];

  if (truncateAfterHeader) components = [header];
  else if (truncateAfterVersion) components = [header, version];
  else if (truncateAfterNumBlocks)
    components = [header, version, numBlocksBuffer];
  else if (truncateAfterDescriptors)
    components = [header, version, numBlocksBuffer, finalBlockDescriptors];
  else if (truncateAfterAuthor) {
    if (truncateAuthor !== undefined) {
      const truncatedAuth = authorSegment.subarray(
        0,
        Math.min(truncateAuthor, authorSegment.length),
      );
      components = [
        header,
        version,
        numBlocksBuffer,
        finalBlockDescriptors,
        truncatedAuth,
      ];
    } else {
      components = [
        header,
        version,
        numBlocksBuffer,
        finalBlockDescriptors,
        authorSegment,
      ];
    }
  }

  return Buffer.concat(components);
}

describe("Rdc Parser and Types", () => {
  describe("MetaDataBlock standalone parsing", () => {
    it("should parse valid MetaDataBlock data", () => {
      const jsonData = { message: "Hello", count: 42 };
      const jsonString = JSON.stringify(jsonData);
      const jsonBuffer = Buffer.from(jsonString, "utf8");
      const lengthPrefix = Buffer.alloc(4);
      lengthPrefix.writeUInt32LE(jsonBuffer.length, 0);

      // This buffer is what MetaDataBlock.parse expects: [JSON_length_prefix | JSON_string]
      const blockSpecificDataBuffer = Buffer.concat([lengthPrefix, jsonBuffer]);

      const metaBlock = new MetaDataBlock();
      // Offset 0 because blockSpecificDataBuffer *is* the data segment for MetaDataBlock
      metaBlock.parse(blockSpecificDataBuffer, 0);
      expect(metaBlock.content).toEqual(jsonData);
      expect(metaBlock.length).toBe(4 + jsonBuffer.length);
    });

    it("should parse empty JSON object {}", () => {
      const jsonData = {};
      const jsonString = JSON.stringify(jsonData);
      const jsonBuffer = Buffer.from(jsonString, "utf8");
      const lengthPrefix = Buffer.alloc(4);
      lengthPrefix.writeUInt32LE(jsonBuffer.length, 0);
      const blockSpecificDataBuffer = Buffer.concat([lengthPrefix, jsonBuffer]);

      const metaBlock = new MetaDataBlock();
      metaBlock.parse(blockSpecificDataBuffer, 0);
      expect(metaBlock.content).toEqual(jsonData);
    });

    it("should throw on invalid JSON string", () => {
      const invalidJsonString = "{ message: 'Missing quotes' ";
      const jsonBuffer = Buffer.from(invalidJsonString, "utf8");
      const lengthPrefix = Buffer.alloc(4);
      lengthPrefix.writeUInt32LE(jsonBuffer.length, 0);
      const blockSpecificDataBuffer = Buffer.concat([lengthPrefix, jsonBuffer]);

      const metaBlock = new MetaDataBlock();
      expect(() => metaBlock.parse(blockSpecificDataBuffer, 0)).toThrow(
        /Failed to parse JSON content/i,
      );
    });

    it("should throw if buffer is too short for JSON length prefix", () => {
      const shortBuffer = Buffer.alloc(2); // Less than 4 bytes for length
      const metaBlock = new MetaDataBlock();
      expect(() => metaBlock.parse(shortBuffer, 0)).toThrow(); // Error from DataView read
    });

    it("should throw if buffer is too short for JSON content based on length prefix", () => {
      const jsonString = JSON.stringify({ message: "Too long for buffer" });
      const jsonBuffer = Buffer.from(jsonString, "utf8");
      const lengthPrefix = Buffer.alloc(4);
      lengthPrefix.writeUInt32LE(jsonBuffer.length + 10, 0); // Claim longer length
      const blockSpecificDataBuffer = Buffer.concat([lengthPrefix, jsonBuffer]); // Buffer is shorter than claimed

      const metaBlock = new MetaDataBlock();
      expect(() => metaBlock.parse(blockSpecificDataBuffer, 0)).toThrow(
        /Metadata JSON content extends beyond buffer length/i,
      );
    });
  });

  describe("Rdc class parsing", () => {
    it("should parse a valid RDC buffer with one MetaDataBlock", () => {
      const author = "RDC Test Author";
      const metaBlockContent = { title: "My Test RDC", version: "1.0" };

      const _mockRdcBuffer = createMockRdcBuffer({
        author,
        blocks: [
          { typeId: MetaDataBlock.RDC_TYPE_ID, content: metaBlockContent },
        ],
      });

      const rdc = Rdc.parse(_mockRdcBuffer);
      expect(rdc.author).toBe(author);
      expect(rdc.offsets.size).toBe(1);
      expect(rdc.offsets.has(MetaDataBlock.RDC_TYPE_ID)).toBe(true);

      // Assuming Rdc.tryParseBlock correctly handles the offset to pass to block.parse
      // (i.e., it passes offset_to_block_envelope + 8)
      const parsedMeta = rdc.tryParseBlock(_mockRdcBuffer, MetaDataBlock);
      expect(parsedMeta).toBeInstanceOf(MetaDataBlock);
      expect(parsedMeta?.content).toEqual(metaBlockContent);
    });

    it("should parse a valid RDC buffer with multiple blocks", () => {
      const author = "MultiBlock Man";
      const metaContent = { data: "meta" };
      // Create some dummy data for a non-MetaDataBlock
      const linkSpriteRawData = Buffer.from([1, 2, 3, 4, 5, 6, 7, 8]); // Min 8 bytes for DataBlock.parse
      const _mockRdcBuffer = createMockRdcBuffer({
        author,
        blocks: [
          { typeId: MetaDataBlock.RDC_TYPE_ID, content: metaContent },
          { typeId: LinkSprite.RDC_TYPE_ID, content: linkSpriteRawData },
        ],
      });
      const rdc = Rdc.parse(_mockRdcBuffer);
      expect(rdc.author).toBe(author);
      expect(rdc.offsets.size).toBe(2);
      expect(rdc.contains(MetaDataBlock.RDC_TYPE_ID)).toBe(true);
      expect(rdc.contains(LinkSprite.RDC_TYPE_ID)).toBe(true);

      const parsedMeta = rdc.tryParseBlock(_mockRdcBuffer, MetaDataBlock);
      expect(parsedMeta?.content).toEqual(metaContent);

      // Mock LinkSprite's parse if it's too complex or just check instance
      // For this test, we'll assume LinkSprite.parse can handle raw data or we mock it
      const linkSpriteInstance = new LinkSprite(); // manifest expects multiple segments
      const linkSpriteDataSegments = linkSpriteInstance.manifest.map((entry) =>
        Buffer.alloc(entry[1] * entry[2].length),
      );
      const mockLinkSpriteRdcBuffer = createMockRdcBuffer({
        author,
        blocks: [
          {
            typeId: LinkSprite.RDC_TYPE_ID,
            content: Buffer.concat(linkSpriteDataSegments),
          },
        ],
      });
      const rdcForLink = Rdc.parse(mockLinkSpriteRdcBuffer);
      const parsedLink = rdcForLink.tryParseBlock(
        mockLinkSpriteRdcBuffer,
        LinkSprite,
      );
      expect(parsedLink).toBeInstanceOf(LinkSprite);
      // Check content if LinkSprite.parse is robust enough for this simple data
      expect(parsedLink?.content.length).toBe(
        linkSpriteInstance.manifest.length,
      );
    });

    it("should throw on invalid RDC header", () => {
      const mockRdcBuffer = createMockRdcBuffer({
        overrideHeader: "NOT_RDC_CONTAINER",
      });
      expect(() => Rdc.parse(mockRdcBuffer)).toThrow(/Invalid RDC header/i);
    });

    it("should throw on invalid RDC version", () => {
      const mockRdcBuffer = createMockRdcBuffer({ overrideVersion: 99 });
      expect(() => Rdc.parse(mockRdcBuffer)).toThrow(
        /Unsupported RDC version/i,
      );
    });

    // Truncation Tests
    it("should throw if buffer too short for RDC header", () => {
      const mockRdcBuffer = createMockRdcBuffer({ truncateAfterHeader: true });
      // Remove some bytes from header
      const truncated = mockRdcBuffer.subarray(0, RDC_HEADER_STRING.length - 5);
      expect(() => Rdc.parse(truncated)).toThrow(
        /Buffer too short to contain RDC header/i,
      );
    });

    it("should throw if buffer too short for RDC version", () => {
      const mockRdcBuffer = createMockRdcBuffer({ truncateAfterVersion: true });
      // With only header+version present, the next read fails on numBlocks
      expect(() => Rdc.parse(mockRdcBuffer)).toThrow(/number of blocks/i);
    });

    it("should throw if buffer too short for numBlocks", () => {
      const _mockRdcBuffer = createMockRdcBuffer({
        truncateAfterNumBlocks: true,
      });
      // Depending on truncation layout, parse may fail reading the author string next
      expect(() => Rdc.parse(_mockRdcBuffer)).toThrow(/author string/i);
    });

    it("should throw if buffer too short for block descriptors", () => {
      // Manually construct a buffer that ends mid-descriptor
      const header = Buffer.from(RDC_HEADER_STRING, "ascii");
      const version = Buffer.from([RDC_VERSION_NUM]);
      const numBlocksBuffer = Buffer.alloc(4);
      numBlocksBuffer.writeUInt32LE(1, 0); // Say 1 block
      const partialDescriptors = Buffer.alloc(4); // Only 4 bytes of needed 8

      const truncated = Buffer.concat([
        header,
        version,
        numBlocksBuffer,
        partialDescriptors,
      ]);
      expect(() => Rdc.parse(truncated)).toThrow(
        /Buffer too short to read block entry/i,
      );
    });

    it("should throw if buffer too short for author string (missing null terminator)", () => {
      const mockRdcBuffer = createMockRdcBuffer({
        author: "ShortAuth",
        noAuthorNullTerminator: true,
        truncateAfterAuthor: true, // Ensures only author string, no data
      });
      expect(() => Rdc.parse(mockRdcBuffer)).toThrow(
        /Buffer too short to read author string/i,
      );
    });

    it("should throw if block offset points beyond buffer length", () => {
      const author = "BadOffsetAuthor";
      const _blockContent = { test: "data" };
      const headerLen = RDC_HEADER_STRING.length;
      const versionLen = 1;
      const numBlocksLen = 4;
      const descriptorLen = 8;
      const authorBytes = Buffer.from(author, "ascii");
      const authorSegmentLen = authorBytes.length + 1; // + null terminator

      // Offset that is clearly out of bounds
      const badOffset =
        headerLen +
        versionLen +
        numBlocksLen +
        descriptorLen +
        authorSegmentLen +
        1000;

      const badDescriptor = Buffer.alloc(8);
      badDescriptor.writeUInt32LE(MetaDataBlock.RDC_TYPE_ID, 0);
      badDescriptor.writeUInt32LE(badOffset, 4);

      const mockRdcBuffer = createMockRdcBuffer({
        author,
        blocks: [], // No actual block data, just a bad descriptor
        customNumBlocks: 1,
        customBlockDescriptors: badDescriptor,
      });

      const rdc = Rdc.parse(mockRdcBuffer);
      // This error should come from tryParseBlock when it tries to read the block header
      expect(() => rdc.tryParseBlock(mockRdcBuffer, MetaDataBlock)).toThrow(
        /out of bounds/i,
      );
    });

    // rdc.contains tests
    it("rdc.contains should return true for existing block types", () => {
      const rdc = Rdc.parse(
        createMockRdcBuffer({
          blocks: [{ typeId: MetaDataBlock.RDC_TYPE_ID, content: {} }],
        }),
      );
      expect(rdc.contains(MetaDataBlock.RDC_TYPE_ID)).toBe(true);
    });

    it("rdc.contains should return false for non-existing block types", () => {
      const rdc = Rdc.parse(createMockRdcBuffer({ blocks: [] }));
      expect(rdc.contains(MetaDataBlock.RDC_TYPE_ID)).toBe(false);
      expect(rdc.contains(999)).toBe(false);
    });

    // rdc.tryParseBlock tests
    it("rdc.tryParseBlock should return undefined for non-existent block type", () => {
      const rdc = Rdc.parse(createMockRdcBuffer({ blocks: [] }));
      const metaBlock = rdc.tryParseBlock(Buffer.alloc(0), MetaDataBlock); // Pass dummy buffer, not used if type not found
      expect(metaBlock).toBeUndefined();
    });

    it("rdc.tryParseBlock should cache parsed blocks", () => {
      const metaContent = { data: "cache_test" };
      const mockBuffer = createMockRdcBuffer({
        blocks: [{ typeId: MetaDataBlock.RDC_TYPE_ID, content: metaContent }],
      });
      const rdc = Rdc.parse(mockBuffer);

      // Spy on MetaDataBlock.prototype.parse
      // For this to work, MetaDataBlock.parse should not be an arrow function if defined on prototype
      // If MetaDataBlock.parse is `public parse = (buffer, offset) => {...}`, spying on prototype won't work.
      // Let's assume it's a standard method: `public parse(buffer, offset) {...}`
      const parseSpy = vi.spyOn(MetaDataBlock.prototype, "parse");

      const block1 = rdc.tryParseBlock(mockBuffer, MetaDataBlock);
      expect(parseSpy).toHaveBeenCalledTimes(1);
      expect(block1?.content).toEqual(metaContent);

      const block2 = rdc.tryParseBlock(mockBuffer, MetaDataBlock);
      expect(parseSpy).toHaveBeenCalledTimes(1); // Should not be called again
      expect(block2).toBe(block1); // Should return the same instance
      expect(block2?.content).toEqual(metaContent);

      parseSpy.mockRestore();
    });
  });
});

describe("LinkSprite", () => {
  it("should have correct type and RDC_TYPE_ID", () => {
    const linkSprite = new LinkSprite();
    expect(linkSprite.type).toBe(1);
    expect(LinkSprite.RDC_TYPE_ID).toBe(1);
  });

  it("should calculate correct total length from manifest", () => {
    const linkSprite = new LinkSprite();
    // Manifest: sprite (0x7000), palette (120), gloves (4)
    const expectedLength = 0x7000 + 120 + 4;
    expect(linkSprite.length).toBe(expectedLength);
  });

  it("should parse a dummy buffer into correct content segments", () => {
    const linkSprite = new LinkSprite();
    const totalLen = linkSprite.length;
    const dummyRdcDataSegment = Buffer.alloc(totalLen, 0xaa);

    // The DataBlock.parse method expects an offset pointing to the start of the RDC block envelope (TypeID).
    // However, for this standalone test of parsing the block's *internal* data structure from a
    // pre-segmented buffer, we are effectively giving it a buffer that *is* the data segment.
    // The +8 skip is done by DataBlock.parse itself.
    // So, if dummyRdcDataSegment is the *actual content* after the RDC header,
    // we need to simulate the RDC block envelope around it if we were passing it to a raw DataBlock.parse.
    // But the prompt implies we are testing the logic given the *content* part.
    // DataBlock.parse as of subtask 7 expects offset to be start of its content.
    // So, if dummyRdcDataSegment is the block's data segment, offset 0 is correct.
    linkSprite.parse(dummyRdcDataSegment, 0);

    expect(linkSprite.content).toHaveLength(linkSprite.manifest.length); // Access public getter
    expect(linkSprite.content[0].length).toBe(0x7000); // Sprite GFX
    if (linkSprite.content[0].length > 0)
      expect(linkSprite.content[0][0]).toBe(0xaa); // Check content
    expect(linkSprite.content[1].length).toBe(120); // Palette
    if (linkSprite.content[1].length > 0)
      expect(linkSprite.content[1][0]).toBe(0xaa);
    expect(linkSprite.content[2].length).toBe(4); // Gloves
    if (linkSprite.content[2].length > 0)
      expect(linkSprite.content[2][0]).toBe(0xaa);
  });
});

// describe('SamusSprite', () => { ... }); // Keep SamusSprite tests if they exist, or add later

// New tests for WRITE methods:

const RDC_HEADER_CONST = "RETRODATACONTAINER";
const RDC_VERSION_CONST = 1;
const RDC_HEADER_LENGTH_CONST = RDC_HEADER_CONST.length;

describe("Block Write Methods", () => {
  describe("MetaDataBlock.write()", () => {
    it("should write JSON content and its length correctly", () => {
      const content = { title: "Test Sprite", author: "Tester" };
      const metaBlock = new MetaDataBlock(content);
      const jsonString = JSON.stringify(content);
      const jsonBytes = Buffer.from(jsonString, "utf8");

      // metaBlock.length should be correctly calculated by the getter
      expect(metaBlock.length).toBe(4 + jsonBytes.length);

      const buffer = Buffer.alloc(metaBlock.length);
      metaBlock.write(buffer, 0);

      // Verify length prefix
      expect(buffer.readUInt32LE(0)).toBe(jsonBytes.length);
      // Verify JSON content
      expect(buffer.toString("utf8", 4)).toBe(jsonString);
    });

    it("should write empty JSON content {} correctly", () => {
      const content = {};
      const metaBlock = new MetaDataBlock(content);
      const jsonString = JSON.stringify(content);
      const jsonBytes = Buffer.from(jsonString, "utf8");

      expect(metaBlock.length).toBe(4 + jsonBytes.length);
      const buffer = Buffer.alloc(metaBlock.length);
      metaBlock.write(buffer, 0);

      expect(buffer.readUInt32LE(0)).toBe(jsonBytes.length);
      expect(buffer.toString("utf8", 4)).toBe(jsonString);
    });

    it("should handle content with special UTF-8 characters", () => {
      const content = { name: "Spécial Nàme — Test", value: "€" };
      const metaBlock = new MetaDataBlock(content);
      const jsonString = JSON.stringify(content);
      const jsonBytes = Buffer.from(jsonString, "utf8");

      expect(metaBlock.length).toBe(4 + jsonBytes.length);
      const buffer = Buffer.alloc(metaBlock.length);
      metaBlock.write(buffer, 0);

      expect(buffer.readUInt32LE(0)).toBe(jsonBytes.length);
      expect(buffer.toString("utf8", 4)).toBe(jsonString);
    });

    it("should throw if buffer is too small", () => {
      const metaBlock = new MetaDataBlock({ data: "some data" });
      const smallBuffer = Buffer.alloc(metaBlock.length - 1);
      expect(() => metaBlock.write(smallBuffer, 0)).toThrow(
        /Not enough space in stream to write MetaDataBlock data/i,
      );
    });
  });

  describe("DataBlock.write() (using LinkSprite)", () => {
    it("should write content buffers sequentially", () => {
      const linkSprite = new LinkSprite(); // Uses its default manifest
      // Use explicit c1/c2/c3 buffers defined below

      // Manually set content for testing. This matches LinkSprite's manifest segment count.
      // The actual LinkSprite content would be much larger.
      // We need to ensure the total length of these buffers matches what linkSprite.length would be
      // if these were derived from its manifest. For this test, we are directly testing DataBlock.write,
      // so we set _content and then recalculate length based on this _content.

      // Mock the manifest for length calculation or simplify content to match existing manifest structure.
      // For LinkSprite, manifest expects 3 segments.
      // Let's make sure our test data matches the expected structure or mock `length`.

      // Simplest: create a generic DataBlock or ensure LinkSprite's content matches its manifest structure
      // For LinkSprite, manifest is:
      // [DataBlock.addr(0x508000), 0x7000, DataBlock.single()],
      // [DataBlock.addr(0x5BD308), 4 * 30, DataBlock.single()],
      // [DataBlock.addr(0x5BEDF5), 4, DataBlock.single()],
      // So, 3 segments.

      const c1 = Buffer.alloc(10).fill(0xaa); // Represents GFX
      const c2 = Buffer.alloc(5).fill(0xbb); // Represents Palette
      const c3 = Buffer.alloc(2).fill(0xcc); // Represents Gloves
      (linkSprite as any)._content = [c1, c2, c3]; // Use 'any' to bypass private access for test

      // The `length` getter in DataBlock iterates over its manifest to calculate length.
      // If we directly set _content, the `length` getter might not reflect _content's length
      // if _content structure doesn't match manifest.
      // For a robust test of DataBlock.write, we want `linkSprite.length` to be sum of c1,c2,c3.
      // Let's mock the `length` getter for this specific test of DataBlock.write.

      const totalLength = c1.length + c2.length + c3.length;
      vi.spyOn(linkSprite, "length", "get").mockReturnValue(totalLength);

      const buffer = Buffer.alloc(totalLength);
      linkSprite.write(buffer, 0);

      expect(buffer.subarray(0, c1.length)).toEqual(c1);
      expect(buffer.subarray(c1.length, c1.length + c2.length)).toEqual(c2);
      expect(buffer.subarray(c1.length + c2.length, totalLength)).toEqual(c3);

      vi.restoreAllMocks(); // Clean up spy
    });

    it("should throw if buffer is too small for DataBlock content", () => {
      const linkSprite = new LinkSprite();
      const c1 = Buffer.alloc(10);
      (linkSprite as any)._content = [c1];
      vi.spyOn(linkSprite, "length", "get").mockReturnValue(c1.length);

      const smallBuffer = Buffer.alloc(c1.length - 1);
      expect(() => linkSprite.write(smallBuffer, 0)).toThrow(
        /Not enough space in stream to write DataBlock content/i,
      );

      vi.restoreAllMocks();
    });

    it("setContent should accept correct number of buffers", () => {
      const linkSprite = new LinkSprite();
      const gfxBuffer = Buffer.alloc(0x7000);
      const paletteBuffer = Buffer.alloc(120);
      const glovesBuffer = Buffer.alloc(4);
      expect(() =>
        linkSprite.setContent([gfxBuffer, paletteBuffer, glovesBuffer]),
      ).not.toThrow();
      expect(linkSprite.content).toEqual([
        gfxBuffer,
        paletteBuffer,
        glovesBuffer,
      ]);
    });

    it("setContent should throw for incorrect number of buffers", () => {
      const linkSprite = new LinkSprite();
      const gfxBuffer = Buffer.alloc(0x7000);
      expect(() => linkSprite.setContent([gfxBuffer])).toThrow(
        /Invalid number of buffers provided/i,
      );
    });
  });
});

describe("Rdc.write()", () => {
  it("should write an RDC file with MetaDataBlock only and be parseable", () => {
    const author = "RDC Author";
    const metaContent = { project: "TestRDC", version: "1.0" };
    const metaBlock = new MetaDataBlock(metaContent);

    const rdcBuffer = Rdc.write(author, [metaBlock]);

    // Parse it back
    const parsedRdc = Rdc.parse(rdcBuffer);
    expect(parsedRdc.author).toBe(author);
    expect(parsedRdc.version).toBe(RDC_VERSION_CONST);
    expect(parsedRdc.offsets.size).toBe(1);
    expect(parsedRdc.offsets.has(MetaDataBlock.RDC_TYPE_ID)).toBe(true);

    const parsedMetaBlock = parsedRdc.tryParseBlock(rdcBuffer, MetaDataBlock);
    expect(parsedMetaBlock).toBeInstanceOf(MetaDataBlock);
    expect(parsedMetaBlock?.content).toEqual(metaContent);
  });

  it("should write an RDC file with LinkSprite and MetaDataBlock and be parseable", () => {
    const author = "Test Author Sprite";
    const metaContent = { info: "Link sprite RDC" };
    const metaBlock = new MetaDataBlock(metaContent);

    const linkSprite = new LinkSprite(); // Uses default manifest
    // Populate LinkSprite's content based on its manifest structure
    const gfxData = Buffer.alloc(
      linkSprite.manifest[0][1] * linkSprite.manifest[0][2].length,
    ).fill(0xaa);
    const paletteData = Buffer.alloc(
      linkSprite.manifest[1][1] * linkSprite.manifest[1][2].length,
    ).fill(0xbb);
    const glovesData = Buffer.alloc(
      linkSprite.manifest[2][1] * linkSprite.manifest[2][2].length,
    ).fill(0xcc);
    (linkSprite as any)._content = [gfxData, paletteData, glovesData];

    const rdcBuffer = Rdc.write(author, [metaBlock, linkSprite]);

    // Parse back
    const parsedRdc = Rdc.parse(rdcBuffer);
    expect(parsedRdc.author).toBe(author);
    expect(parsedRdc.version).toBe(RDC_VERSION_CONST);
    expect(parsedRdc.offsets.size).toBe(2);
    expect(parsedRdc.contains(MetaDataBlock.RDC_TYPE_ID)).toBe(true);
    expect(parsedRdc.contains(LinkSprite.RDC_TYPE_ID)).toBe(true);

    // Check MetaDataBlock
    const parsedMeta = parsedRdc.tryParseBlock(rdcBuffer, MetaDataBlock);
    expect(parsedMeta?.content).toEqual(metaContent);

    // Check LinkSprite
    const parsedLink = parsedRdc.tryParseBlock(rdcBuffer, LinkSprite);
    expect(parsedLink).toBeInstanceOf(LinkSprite);
    expect(parsedLink?.content.length).toBe(3); // 3 segments in LinkSprite
    expect(parsedLink?.content[0]).toEqual(gfxData);
    expect(parsedLink?.content[1]).toEqual(paletteData);
    expect(parsedLink?.content[2]).toEqual(glovesData);

    // Verify offsets point to the start of each block's data (no envelope)
    const metaOffset = parsedRdc.offsets.get(MetaDataBlock.RDC_TYPE_ID)!;
    // MetaData starts with a 4-byte JSON length
    const jsonLen = rdcBuffer.readUInt32LE(metaOffset);
    expect(jsonLen).toBe(metaBlock.length - 4);

    const linkOffset = parsedRdc.offsets.get(LinkSprite.RDC_TYPE_ID)!;
    const expectedLinkConcat = Buffer.concat(
      (parsedLink as LinkSprite).content,
    );
    const actualLinkSlice = rdcBuffer.subarray(
      linkOffset,
      linkOffset + expectedLinkConcat.length,
    );
    expect(actualLinkSlice).toEqual(expectedLinkConcat);
  });

  it("should write an RDC file with no blocks", () => {
    const author = "Empty RDC";
    const rdcBuffer = Rdc.write(author, []);

    const parsedRdc = Rdc.parse(rdcBuffer);
    expect(parsedRdc.author).toBe(author);
    expect(parsedRdc.version).toBe(RDC_VERSION_CONST);
    expect(parsedRdc.offsets.size).toBe(0);
  });

  it("should correctly write author string with null terminator", () => {
    const author = "Test Author";
    const rdcBuffer = Rdc.write(author, []);

    const expectedAuthorOffset =
      RDC_HEADER_LENGTH_CONST + 1 /*version*/ + 4; /*numBlocks*/
    const authorBytes = Buffer.from(author, "ascii");
    const writtenAuthorBytes = rdcBuffer.subarray(
      expectedAuthorOffset,
      expectedAuthorOffset + authorBytes.length,
    );
    expect(writtenAuthorBytes.toString("ascii")).toBe(author);
    expect(rdcBuffer.readUInt8(expectedAuthorOffset + authorBytes.length)).toBe(
      0,
    ); // Null terminator
  });

  // Verify the calculated file size matches buffer length.
  it("should have calculated total size matching buffer length", () => {
    const author = "Size Test";
    const metaBlock = new MetaDataBlock({ data: "some data" });
    const linkSprite = new LinkSprite();
    const gfx = Buffer.alloc(10).fill(1);
    const pal = Buffer.alloc(5).fill(2);
    const glv = Buffer.alloc(2).fill(3);
    (linkSprite as any)._content = [gfx, pal, glv];
    // Mock LinkSprite's length to be sum of its _content for this test
    vi.spyOn(linkSprite, "length", "get").mockReturnValue(
      gfx.length + pal.length + glv.length,
    );

    const blocks: BlockType[] = [metaBlock, linkSprite];

    const authorBytes = Buffer.from(author, "ascii");
    const authorLengthWithNull = authorBytes.length + 1;
    const totalBlockDataSize = blocks.reduce((sum, b) => sum + b.length, 0);
    const offsetTableSize = blocks.length * 8;
    const expectedTotalSize =
      RDC_HEADER_LENGTH_CONST +
      1 +
      4 +
      offsetTableSize +
      authorLengthWithNull +
      totalBlockDataSize;

    const rdcBuffer = Rdc.write(author, blocks);
    expect(rdcBuffer.length).toBe(expectedTotalSize);

    vi.restoreAllMocks();
  });
});

describe("Zelda1SpriteDataBlock", () => {
  it("should have correct type and RDC_TYPE_ID", () => {
    const block = new Zelda1SpriteDataBlock();
    expect(block.type).toBe(2);
    expect(Zelda1SpriteDataBlock.RDC_TYPE_ID).toBe(2);
  });

  it("should calculate correct total length from manifest (sum of segments)", () => {
    const block = new Zelda1SpriteDataBlock();
    const expectedSegmentLengths = [32, 32, 448, 32, 64, 32, 3, 3, 3, 3];
    const expectedLength = expectedSegmentLengths.reduce((a, b) => a + b, 0);
    expect(block.length).toBe(expectedLength);
  });

  it("should parse a dummy buffer into correct content segments (manifest order)", () => {
    const block = new Zelda1SpriteDataBlock();
    const expectedSegmentLengths = [32, 32, 448, 32, 64, 32, 3, 3, 3, 3];
    const dummyRdcDataSegment = Buffer.alloc(block.length, 0xbb);
    block.parse(dummyRdcDataSegment, 0);
    expect(block.content).toHaveLength(block.manifest.length);
    expectedSegmentLengths.forEach((len, idx) => expect(block.content[idx].length).toBe(len));
  });
});

describe("Metroid1SpriteDataBlock", () => {
  it("should have correct type and RDC_TYPE_ID", () => {
    const block = new Metroid1SpriteDataBlock();
    expect(block.type).toBe(3);
    expect(Metroid1SpriteDataBlock.RDC_TYPE_ID).toBe(3);
  });

  it("should calculate correct total length from manifest (sum of segments)", () => {
    const block = new Metroid1SpriteDataBlock();
    const expectedSegmentLengths = [
      64, 80, 64, 16, 96, 64, 48, 96, 96, 16, 32, 96, 48, 112, 112, 16, 32, 64,
      3, 2, 2, 2, 2,
    ];
    const expectedLength = expectedSegmentLengths.reduce((a, b) => a + b, 0);
    expect(block.length).toBe(expectedLength);
  });

  it("should parse a dummy buffer into correct content segments (manifest order)", () => {
    const block = new Metroid1SpriteDataBlock();
    const expectedSegmentLengths = [
      64, 80, 64, 16, 96, 64, 48, 96, 96, 16, 32, 96, 48, 112, 112, 16, 32, 64,
      3, 2, 2, 2, 2,
    ];
    const dummyRdcDataSegment = Buffer.alloc(block.length, 0xcc);
    block.parse(dummyRdcDataSegment, 0);
    expect(block.content).toHaveLength(block.manifest.length);
    expectedSegmentLengths.forEach((len, idx) =>
      expect(block.content[idx].length).toBe(len),
    );
  });
});
