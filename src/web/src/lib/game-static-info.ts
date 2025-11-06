export type RomMapping = "lorom" | "hirom" | "exhirom" | "sa1rom";

export interface RdcAddressTarget {
  address: number;
  type: "pc" | "snes";
}

export interface RdcManifestAddress {
  mapping: RomMapping | "default";
  address: number;
}

export interface RdcManifestSegment {
  length: number;
  entries?: number;
  entryStride?: number;
  entryOffsets?: number[];
  addressType: "pc" | "snes";
  addresses: RdcManifestAddress[];
}

export interface SpriteManifestConfig {
  mapping: RomMapping;
  segments: readonly RdcManifestSegment[];
}

export interface SpriteTargetVariant {
  pointers?: Record<string, RdcAddressTarget>;
  manifest?: SpriteManifestConfig;
}

export interface GameStaticInfo {
  id: string;
  displayName: string;
  expectedHash?: string;
  forcedHeaderBytes?: Uint8Array;
  fileExtensions?: string;
  targetOffsets?: Record<string, number>;
  rdcTargets?: Record<string, Record<string, SpriteTargetVariant>>;
}

// --- NES Manifests (Zelda 1 & Metroid 1) ---
const zelda1ManifestSegments: RdcManifestSegment[] = [
  {
    length: 32,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x608e34 }],
  }, // LIFTING_ITEM
  {
    length: 32,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x608eb4 }],
  }, // WALK1_PROFILE_BIGSHIELD
  {
    length: 448,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x61007f }],
  }, // 7 poses block
  {
    length: 32,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6105bf }],
  }, // WALK2_PROFILE_BIGSHIELD
  {
    length: 64,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6105ff }],
  }, // WALK1_DOWN_SMALLSHIELD + WALK2_DOWN_SMALLSHIELD
  {
    length: 32,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x61067f }],
  }, // FACING_DOWN_BIGSHIELD
  {
    length: 3,
    addressType: "pc",
    // Base colors replicated to multiple target addresses (mirrors)
    addresses: [
      { mapping: "default", address: 0x631314 },
      { mapping: "default", address: 0x631410 },
      { mapping: "default", address: 0x63150c },
      { mapping: "default", address: 0x631608 },
      { mapping: "default", address: 0x631704 },
      { mapping: "default", address: 0x631800 },
      { mapping: "default", address: 0x6318fc },
      { mapping: "default", address: 0x6319f8 },
      { mapping: "default", address: 0x631af4 },
      { mapping: "default", address: 0x631bf0 },
      { mapping: "default", address: 0x631cec },
      { mapping: "default", address: 0x3d3804 },
    ],
  },
  {
    length: 3,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x631cf0 }],
  }, // LEVEL2_COLORS
  {
    length: 3,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x631cf4 }],
  }, // LEVEL3_COLORS
  {
    length: 3,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x612287 }],
  }, // TUNIC_COLORS
];

const zelda1ManifestConfig: SpriteManifestConfig = {
  mapping: "lorom",
  segments: zelda1ManifestSegments,
};

const metroidManifestSegments: RdcManifestSegment[] = [
  {
    length: 64,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0000 }],
  },
  {
    length: 80,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0050 }],
  },
  {
    length: 64,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b00b0 }],
  },
  {
    length: 16,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0170 }],
  },
  {
    length: 96,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0190 }],
  },
  {
    length: 64,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0200 }],
  },
  {
    length: 48,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0250 }],
  },
  {
    length: 96,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0290 }],
  },
  {
    length: 96,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0310 }],
  },
  {
    length: 16,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0390 }],
  },
  {
    length: 32,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b03b0 }],
  },
  {
    length: 96,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0400 }],
  },
  {
    length: 48,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0490 }],
  },
  {
    length: 112,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0500 }],
  },
  {
    length: 112,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0600 }],
  },
  {
    length: 16,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0690 }],
  },
  {
    length: 32,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0720 }],
  },
  {
    length: 64,
    addressType: "pc",
    addresses: [{ mapping: "default", address: 0x6b0770 }],
  },
  {
    length: 3,
    addressType: "pc",
    addresses: [
      { mapping: "default", address: 0x68a285 },
      { mapping: "default", address: 0x68a2e8 },
      { mapping: "default", address: 0x69218c },
      { mapping: "default", address: 0x6921ef },
      { mapping: "default", address: 0x69a72c },
      { mapping: "default", address: 0x69a7a5 },
      { mapping: "default", address: 0x6a2169 },
      { mapping: "default", address: 0x6a21a9 },
      { mapping: "default", address: 0x6aa0ff },
      { mapping: "default", address: 0x6aa153 },
    ],
  },
  {
    length: 2,
    addressType: "pc",
    addresses: [
      { mapping: "default", address: 0x68a298 },
      { mapping: "default", address: 0x69219f },
      { mapping: "default", address: 0x69a73f },
      { mapping: "default", address: 0x6a217c },
      { mapping: "default", address: 0x6aa112 },
    ],
  },
  {
    length: 2,
    addressType: "pc",
    addresses: [
      { mapping: "default", address: 0x68a29e },
      { mapping: "default", address: 0x6921a5 },
      { mapping: "default", address: 0x69a745 },
      { mapping: "default", address: 0x6a2182 },
      { mapping: "default", address: 0x6aa118 },
    ],
  },
  {
    length: 2,
    addressType: "pc",
    addresses: [
      { mapping: "default", address: 0x68a2a4 },
      { mapping: "default", address: 0x6921ab },
      { mapping: "default", address: 0x69a74b },
      { mapping: "default", address: 0x6a2188 },
      { mapping: "default", address: 0x6aa11e },
    ],
  },
  {
    length: 2,
    addressType: "pc",
    addresses: [
      { mapping: "default", address: 0x68a2aa },
      { mapping: "default", address: 0x6921b1 },
      { mapping: "default", address: 0x69a751 },
      { mapping: "default", address: 0x6a218e },
      { mapping: "default", address: 0x6aa124 },
    ],
  },
];

const metroidManifestConfig: SpriteManifestConfig = {
  mapping: "lorom",
  segments: metroidManifestSegments,
};

const samusManifestSegments: RdcManifestSegment[] = [
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x440000 },
      { mapping: "lorom", address: 0x9c8000 },
      { mapping: "sa1rom", address: 0x9c8000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x450000 },
      { mapping: "lorom", address: 0x9d8000 },
      { mapping: "sa1rom", address: 0x9d8000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x460000 },
      { mapping: "lorom", address: 0x9e8000 },
      { mapping: "sa1rom", address: 0x9e8000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x470000 },
      { mapping: "lorom", address: 0x9f8000 },
      { mapping: "sa1rom", address: 0x9f8000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x480000 },
      { mapping: "lorom", address: 0xf58000 },
      { mapping: "sa1rom", address: 0xd10000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x490000 },
      { mapping: "lorom", address: 0xf68000 },
      { mapping: "sa1rom", address: 0xd18000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x4a0000 },
      { mapping: "lorom", address: 0xf78000 },
      { mapping: "sa1rom", address: 0xd20000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x4b0000 },
      { mapping: "lorom", address: 0xf88000 },
      { mapping: "sa1rom", address: 0xd28000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x540000 },
      { mapping: "lorom", address: 0xf98000 },
      { mapping: "sa1rom", address: 0xd30000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x550000 },
      { mapping: "lorom", address: 0xfa8000 },
      { mapping: "sa1rom", address: 0xd38000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x560000 },
      { mapping: "lorom", address: 0xfb8000 },
      { mapping: "sa1rom", address: 0xd40000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x570000 },
      { mapping: "lorom", address: 0xfc8000 },
      { mapping: "sa1rom", address: 0xd48000 },
    ],
  },
  {
    length: 0x8000,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x580000 },
      { mapping: "lorom", address: 0xfd8000 },
      { mapping: "sa1rom", address: 0xd50000 },
    ],
  },
  {
    length: 0x7880,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x590000 },
      { mapping: "lorom", address: 0xfe8000 },
      { mapping: "sa1rom", address: 0xd58000 },
    ],
  },
  {
    length: 0x3f60,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x5a0000 },
      { mapping: "lorom", address: 0xff8000 },
      { mapping: "sa1rom", address: 0xd60000 },
    ],
  },
  {
    length: 0x3f60,
    addressType: "snes",
    addresses: [
      { mapping: "exhirom", address: 0x5a4000 },
      { mapping: "lorom", address: 0xffc000 },
      { mapping: "sa1rom", address: 0xd64000 },
    ],
  },
  {
    length: 0x3c0,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9a9a00 }],
  },
  {
    length: 0x600,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0xb6da00 }],
  },
  {
    length: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0xb6d900 }],
  },
  {
    length: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0xb6d980 }],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9402 }],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9522 }],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9802 }],
  },
  {
    length: 0x1e,
    entries: 9,
    entryOffsets: [0x0, 0x24, 0x4f, 0x73, 0x9e, 0xc2, 0xed, 0x111, 0x139],
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8ddb6d }],
  },
  {
    length: 0x1e,
    entries: 9,
    entryOffsets: [0x0, 0x24, 0x4f, 0x73, 0x9e, 0xc2, 0xed, 0x111, 0x139],
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8ddcd3 }],
  },
  {
    length: 0x1e,
    entries: 9,
    entryOffsets: [0x0, 0x24, 0x4f, 0x73, 0x9e, 0xc2, 0xed, 0x111, 0x139],
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8dde39 }],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x22,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8de468 }],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x22,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8de694 }],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x22,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8de8c0 }],
  },
  {
    length: 0x1e,
    entries: 8,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9822 }],
  },
  {
    length: 0x1e,
    entries: 8,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9922 }],
  },
  {
    length: 0x1e,
    entries: 8,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9a22 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9b22 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9d22 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9f22 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9ba2 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9da2 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9fa2 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9c22 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9e22 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9ba022 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9ca2 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b9ea2 }],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9ba0a2 }],
  },
  {
    length: 0x1e,
    entries: 6,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9b96c2 }],
  },
  {
    length: 0x1e,
    entries: 9,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9ba122 }],
  },
  {
    length: 0x1e,
    entries: 10,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9ba242 }],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9ba3a2 },
      { mapping: "default", address: 0x8ce56b },
    ],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x9ba382 }],
  },
  {
    length: 0x6,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9ba3c0 },
      { mapping: "default", address: 0x9ba3c6 },
    ],
  },
  {
    length: 0x2,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x82e52c }],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8ee5e2 }],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8ce68b }],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x24,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8dd6c2 }],
  },
  {
    length: 0x1c,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0xa2a5a0 }],
  },
  {
    length: 0x2,
    entries: 14,
    entryStride: 0x6,
    addressType: "snes",
    addresses: [{ mapping: "default", address: 0x8dca54 }],
  },
];

const samusDefaultManifest: SpriteManifestConfig = {
  mapping: "lorom",
  segments: samusManifestSegments,
};

const samusComboManifest: SpriteManifestConfig = {
  mapping: "sa1rom",
  segments: samusManifestSegments,
};

const linkDefaultPointers: Record<string, RdcAddressTarget> = {
  gfx: { address: 0x108000, type: "snes" },
  palette: { address: 0x1bd308, type: "snes" },
  gloves: { address: 0x1bedf5, type: "snes" },
};

const linkComboPointers: Record<string, RdcAddressTarget> = {
  gfx: { address: 0x480000, type: "pc" },
  palette: { address: 0x4dd308, type: "pc" },
  gloves: { address: 0x4dedf5, type: "pc" },
};
// Exported static info. Zelda1 & Metroid now fully manifest-driven (no pointer constants needed).
export const gameStaticInfo: Record<string, GameStaticInfo> = {
  supermetroid: {
    id: "supermetroid",
    displayName: "Super Metroid",
    expectedHash:
      "12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72",
    fileExtensions: ".sfc,.smc,.zip",
    targetOffsets: { alttpr: -1, combo: 0 },
    rdcTargets: {
      "rdc/samus": {
        default: { manifest: samusDefaultManifest },
        combo: { manifest: samusComboManifest },
      },
    },
  },
  alttp: {
    id: "alttp",
    displayName: "A Link to the Past",
    expectedHash:
      "794e040b02c7591b59ad8843b51e7c619b88f87cddc6083a8e7a4027b96a2271",
    fileExtensions: ".sfc,.smc,.zip",
    targetOffsets: { alttpr: 0, combo: 3_145_728 },
    rdcTargets: {
      "rdc/link": {
        default: { pointers: linkDefaultPointers },
        combo: { pointers: linkComboPointers },
      },
    },
  },
  zelda1: {
    id: "zelda1",
    displayName: "The Legend of Zelda (NES)",
    expectedHash:
      "8f72dc2e98572eb4ba7c3a902bca5f69c448fc4391837e5f8f0d4556280440ac",
    forcedHeaderBytes: new Uint8Array([
      0x4e, 0x45, 0x53, 0x1a, 0x08, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00,
      0x00, 0x00, 0x00, 0x00,
    ]),
    fileExtensions: ".nes,.zip",
    targetOffsets: { alttpr: -1, combo: 4_325_392 },
    rdcTargets: {
      "rdc/nes-z1": {
        default: { manifest: zelda1ManifestConfig },
        combo: { manifest: zelda1ManifestConfig },
      },
    },
  },
  metroid: {
    id: "metroid",
    displayName: "Metroid (NES)",
    expectedHash:
      "c5eea06e1e1128b576bd789f1a4f63bb154d6d31579c2f319382fb77a72d34a6",
    forcedHeaderBytes: new Uint8Array([
      0x4e, 0x45, 0x53, 0x1a, 0x08, 0x00, 0x11, 0x00, 0x00, 0x00, 0x4e, 0x49,
      0x20, 0x31, 0x2e, 0x33,
    ]),
    fileExtensions: ".nes,.zip",
    targetOffsets: { alttpr: -1, combo: 4_194_304 },
    rdcTargets: {
      "rdc/nes-m1": {
        default: { manifest: metroidManifestConfig },
        combo: { manifest: metroidManifestConfig },
      },
    },
  },
  combo: {
    id: "combo",
    displayName: "",
    fileExtensions: "",
  },
};
