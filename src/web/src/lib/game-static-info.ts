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
  fileExtensions?: string;
  targetOffsets?: Record<string, number>;
  rdcTargets?: Record<string, Record<string, SpriteTargetVariant>>;
}

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
    addresses: [
      { mapping: "default", address: 0x9a9a00 },
    ],
  },
  {
    length: 0x600,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0xb6da00 },
    ],
  },
  {
    length: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0xb6d900 },
    ],
  },
  {
    length: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0xb6d980 },
    ],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9402 },
    ],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9522 },
    ],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9802 },
    ],
  },
  {
    length: 0x1e,
    entries: 9,
    entryOffsets: [
      0x0,
      0x24,
      0x4f,
      0x73,
      0x9e,
      0xc2,
      0xed,
      0x111,
      0x139,
    ],
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8ddb6d },
    ],
  },
  {
    length: 0x1e,
    entries: 9,
    entryOffsets: [
      0x0,
      0x24,
      0x4f,
      0x73,
      0x9e,
      0xc2,
      0xed,
      0x111,
      0x139,
    ],
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8ddcd3 },
    ],
  },
  {
    length: 0x1e,
    entries: 9,
    entryOffsets: [
      0x0,
      0x24,
      0x4f,
      0x73,
      0x9e,
      0xc2,
      0xed,
      0x111,
      0x139,
    ],
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8dde39 },
    ],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x22,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8de468 },
    ],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x22,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8de694 },
    ],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x22,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8de8c0 },
    ],
  },
  {
    length: 0x1e,
    entries: 8,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9822 },
    ],
  },
  {
    length: 0x1e,
    entries: 8,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9922 },
    ],
  },
  {
    length: 0x1e,
    entries: 8,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9a22 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9b22 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9d22 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9f22 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9ba2 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9da2 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9fa2 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9c22 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9e22 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9ba022 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9ca2 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b9ea2 },
    ],
  },
  {
    length: 0x1e,
    entries: 4,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9ba0a2 },
    ],
  },
  {
    length: 0x1e,
    entries: 6,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9b96c2 },
    ],
  },
  {
    length: 0x1e,
    entries: 9,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9ba122 },
    ],
  },
  {
    length: 0x1e,
    entries: 10,
    entryStride: 0x20,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x9ba242 },
    ],
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
    addresses: [
      { mapping: "default", address: 0x9ba382 },
    ],
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
    addresses: [
      { mapping: "default", address: 0x82e52c },
    ],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8ee5e2 },
    ],
  },
  {
    length: 0x1e,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8ce68b },
    ],
  },
  {
    length: 0x1e,
    entries: 16,
    entryStride: 0x24,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8dd6c2 },
    ],
  },
  {
    length: 0x1c,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0xa2a5a0 },
    ],
  },
  {
    length: 0x2,
    entries: 14,
    entryStride: 0x6,
    addressType: "snes",
    addresses: [
      { mapping: "default", address: 0x8dca54 },
    ],
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

const zelda1DefaultPointers: Record<string, RdcAddressTarget> = {
  gfx: { address: 0x001000, type: "pc" },
  palette: { address: 0x002000, type: "pc" },
};

const zelda1ComboPointers: Record<string, RdcAddressTarget> = {
  gfx: { address: 0x421000, type: "pc" },
  palette: { address: 0x422000, type: "pc" },
};

const metroidDefaultPointers: Record<string, RdcAddressTarget> = {
  gfx: { address: 0x003000, type: "pc" },
  palette: { address: 0x004000, type: "pc" },
};

const metroidComboPointers: Record<string, RdcAddressTarget> = {
  gfx: { address: 0x403000, type: "pc" },
  palette: { address: 0x404000, type: "pc" },
};

export const gameStaticInfo: Record<string, GameStaticInfo> = {
  supermetroid: {
    id: "supermetroid",
    displayName: "Super Metroid",
    expectedHash:
      "12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72",
    fileExtensions: ".sfc,.smc,.zip",
    targetOffsets: {
      alttpr: -1,
      combo: 0,
    },
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
    targetOffsets: {
      alttpr: 0,
      combo: 3_145_728,
    },
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
    fileExtensions: ".nes,.zip",
    targetOffsets: {
      alttpr: -1,
      combo: 4_325_392,
    },
    rdcTargets: {
      "rdc/nes-z1": {
        default: { pointers: zelda1DefaultPointers },
        combo: { pointers: zelda1ComboPointers },
      },
    },
  },
  metroid: {
    id: "metroid",
    displayName: "Metroid (NES)",
    expectedHash:
      "c5eea06e1e1128b576bd789f1a4f63bb154d6d31579c2f319382fb77a72d34a6",
    fileExtensions: ".nes,.zip",
    targetOffsets: {
      alttpr: -1,
      combo: 4_194_304,
    },
    rdcTargets: {
      "rdc/nes-m1": {
        default: { pointers: metroidDefaultPointers },
        combo: { pointers: metroidComboPointers },
      },
    },
  },
  combo: {
    id: "combo",
    displayName: "",
    fileExtensions: "",
  },
};
