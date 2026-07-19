import { z } from "zod";
import {
  MAX_PRESET_DESCRIPTION_LENGTH,
  MAX_PRESET_NAME_LENGTH,
} from "$lib/config/constants";

// Shape of the canonical settings payload (see $lib/config/normalize). Values
// are validated against the real generator metadata server-side.
export const NormalizedConfigSchema = z.object({
  selectedGames: z.array(z.string().max(64)).max(64),
  global: z.record(z.unknown()),
  perGame: z.record(z.record(z.unknown())),
});

export const PresetNameSchema = z
  .string()
  .trim()
  .min(1, "Name is required")
  .max(MAX_PRESET_NAME_LENGTH, "Name is too long");

export const PresetDescriptionSchema = z
  .string()
  .trim()
  .max(MAX_PRESET_DESCRIPTION_LENGTH, "Description is too long");

const CurationFieldsSchema = z.object({
  slug: z
    .string()
    .trim()
    .regex(/^[a-z0-9-]+$/, "Slug must be lowercase letters, digits and dashes")
    .max(64)
    .optional(),
  gameTags: z.array(z.string().trim().max(32)).max(16).optional(),
  // Nullable so admins can clear it again.
  difficultyTag: z.string().trim().max(32).nullable().optional(),
  isRecommended: z.boolean().optional(),
  featured: z.boolean().optional(),
  displayOrder: z.number().int().optional(),
});

export const CreatePresetRequestSchema = z
  .object({
    configId: z.string().trim().min(1).max(64),
    name: PresetNameSchema,
    description: PresetDescriptionSchema.optional(),
    settings: NormalizedConfigSchema,
    changeSummary: z.string().trim().max(240).optional(),
    setAsDefault: z.boolean().optional(),
    favorite: z.boolean().optional(),
    // "official" requires administrator rights; owner is always derived
    // server-side and never accepted from the request.
    scope: z.enum(["official", "user"]).optional(),
  })
  .merge(CurationFieldsSchema);

export const UpdatePresetRequestSchema = z
  .object({
    name: PresetNameSchema.optional(),
    description: PresetDescriptionSchema.nullable().optional(),
    archived: z.boolean().optional(),
  })
  .merge(CurationFieldsSchema);

export const CreateRevisionRequestSchema = z.object({
  settings: NormalizedConfigSchema,
  changeSummary: z.string().trim().max(240).optional(),
  // Optimistic concurrency: must match the preset's current revision.
  baseRevisionId: z.string().nullable(),
});

export const DuplicatePresetRequestSchema = z.object({
  name: PresetNameSchema.optional(),
});

export const PresetPreferencesRequestSchema = z.object({
  defaultPresetId: z.string().nullable().optional(),
  lastUsedPresetId: z.string().nullable().optional(),
});

export const PresetFavoriteRequestSchema = z.object({
  presetId: z.string(),
  favorited: z.boolean(),
  displayOrder: z.number().int().optional(),
});

export type CreatePresetRequest = z.infer<typeof CreatePresetRequestSchema>;
export type UpdatePresetRequest = z.infer<typeof UpdatePresetRequestSchema>;
export type CreateRevisionRequest = z.infer<typeof CreateRevisionRequestSchema>;

// Response DTOs shared by the API endpoints and the client.
export interface PresetSummaryDto {
  id: string;
  scope: "official" | "user";
  slug: string | null;
  configId: string;
  name: string;
  description: string | null;
  currentRevisionId: string | null;
  revisionNumber: number | null;
  configSchemaVersion: number | null;
  selectedGames: string[];
  gameTags: string[] | null;
  difficultyTag: string | null;
  isRecommended: boolean;
  featured: boolean;
  archived: boolean;
  displayOrder: number;
  createdAt: string | null;
  updatedAt: string | null;
}

export interface PresetRevisionDto {
  id: string;
  presetId: string;
  revisionNumber: number;
  configSchemaVersion: number;
  settings: unknown;
  changeSummary: string | null;
  createdAt: string | null;
}

export interface PresetPreferencesDto {
  defaultPresetId: string | null;
  lastUsedPresetId: string | null;
  favorites: Array<{ presetId: string; displayOrder: number }>;
}

export interface PresetListResponseDto {
  officials: PresetSummaryDto[];
  mine: PresetSummaryDto[];
  preferences: PresetPreferencesDto | null;
  recommendedId: string | null;
  configSchemaVersion: number;
}

export interface PresetDetailResponseDto {
  preset: PresetSummaryDto;
  revision: PresetRevisionDto;
}
