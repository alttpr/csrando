import type { z } from "zod";
import {
  MetadataSchema,
  MetadataSettingSchema,
  SingleChoiceSettingSchema,
  MultipleChoiceSettingSchema,
  SliderSettingSchema,
  ToggleSettingSchema,
  InputSettingSchema,
  GenericSettingSchema,
  GameSettingsSchema,
} from "$lib/schemas/metadata";
import {
  GameSpriteConfigSchema,
  SpriteInfoSchema,
  SpritePatchDetailsSchema,
  SpritePatchEntrySchema,
  SpritePatchFileSchema,
} from "$lib/schemas/sprites";
import {
  RandomizerResponseSchema,
  RandomizeRequestSchema,
} from "$lib/schemas/backend";
import {
  GamePostGenConfigSchema,
  PostGenSettingSchema,
  SelectPostGenSettingSchema,
  TogglePostGenSettingSchema,
  PostGenPatchEntrySchema,
} from "$lib/schemas/postgen";

export interface User {
  id: string;
  username: string;
  githubId: number | null;
}

export interface Seed {
  id: string;
  options: {
    games: string[];
    settings: {
      global: Record<string, unknown>;
      perGame: Record<string, Record<string, unknown>>;
    };
  };
  patchData: unknown;
  placementInfo: unknown;
  createdAt: string;
}

export interface RomFileData {
  buffer: ArrayBuffer | null;
  fileName: string | null;
  hashStatus:
    | "no_rom"
    | "checking"
    | "verified"
    | "mismatch"
    | "error"
    | "uploaded_no_verify";
  calculatedHash?: string;
  expectedHash?: string;
  gameName: string;
}

export interface GameStaticInfo {
  id: string;
  displayName: string;
  expectedHash?: string;
  fileExtensions: string;
}

export interface OptionChoice {
  value: string | number;
  label: string;
}

export interface Option {
  name: string;
  label?: string;
  description?: string;
  type: "string" | "number" | "boolean" | "select" | "multiselect";
  defaultValue: string | number | boolean | string[];
  options?: OptionChoice[];
  min?: number;
  max?: number;
  step?: number;
}

export type SingleChoiceSetting = z.infer<typeof SingleChoiceSettingSchema>;
export type MultipleChoiceSetting = z.infer<typeof MultipleChoiceSettingSchema>;
export type SliderSetting = z.infer<typeof SliderSettingSchema>;
export type ToggleSetting = z.infer<typeof ToggleSettingSchema>;
export type InputSetting = z.infer<typeof InputSettingSchema>;
export type GenericSetting = z.infer<typeof GenericSettingSchema>;
export type MetadataSetting = z.infer<typeof MetadataSettingSchema>;
export type GameSettings = z.infer<typeof GameSettingsSchema>;
export type Metadata = z.infer<typeof MetadataSchema>;

export type SpritePatchFile = z.infer<typeof SpritePatchFileSchema>;
export type SpritePatchEntry = z.infer<typeof SpritePatchEntrySchema>;
export type SpritePatchDetails = z.infer<typeof SpritePatchDetailsSchema>;
export type SpriteInfo = z.infer<typeof SpriteInfoSchema>;
export type GameSpriteConfig = z.infer<typeof GameSpriteConfigSchema>;

export type GameSpriteConfigMap = Map<string, GameSpriteConfig>;

export type RandomizerResponse = z.infer<typeof RandomizerResponseSchema>;
export type RandomizeRequest = z.infer<typeof RandomizeRequestSchema>;

export type GamePostGenConfig = z.infer<typeof GamePostGenConfigSchema>;
export type PostGenSetting = z.infer<typeof PostGenSettingSchema>;
export type SelectPostGenSetting = z.infer<typeof SelectPostGenSettingSchema>;
export type TogglePostGenSetting = z.infer<typeof TogglePostGenSettingSchema>;
export type PostGenPatchEntry = z.infer<typeof PostGenPatchEntrySchema>;
