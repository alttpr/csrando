export interface SpritePatchFile {
  id: string;
  path: string;
}

export interface SpritePatchEntry {
  fileId: string;
  targetAddress: string;
  dataLength: number;
  dataOffsetInFile?: number;
}

export interface SpritePatchDetails {
  files: SpritePatchFile[];
  patches: SpritePatchEntry[];
}

export interface SpriteInfoFrontend {
  value: string;
  name: string;
  imagePath: string;
  author?: string;
  rdcPath?: string;
  kind?: string;
  patchDetails?: SpritePatchDetails;
}

export interface GameSpriteConfig {
  defaultSpriteValue?: string;
  sprites: SpriteInfoFrontend[];
}

export interface SpriteInfoEntry {
  title: string;
  author: string;
  game: string;
  path: string;
  files: Record<string, string>;
}

export type SpritesJson = Record<string, SpriteInfoEntry>;
