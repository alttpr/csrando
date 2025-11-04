import { z } from "zod";
import { GamePostGenConfigSchema } from "./postgen";

// Base shape for any setting coming from metadata. Some upstream sources may omit
// description, so treat it as optional to avoid parse failures on otherwise valid data.
const SettingBaseSchema = z.object({
  key: z.string(),
  name: z.string(),
  description: z.string().optional(),
  visibility: z.string().optional(),
  category: z
    .union([
      z.string(),
      z.object({
        name: z.string(),
        display: z.enum(["Static", "Expanded", "Collapsed"]).optional(),
      }),
    ])
    .optional(),
  dependsOn: z
    .object({
      key: z.string(),
      values: z.array(z.any()).min(1),
    })
    .optional(),
  // Subcategory supports the same dual-shape as category.
  subcategory: z
    .union([
      z.string(),
      z.object({
        name: z.string(),
        display: z.enum(["Static", "Expanded", "Collapsed"]).optional(),
      }),
    ])
    .optional(),
});

// Accept nullable values from backend (e.g. { "Random": null }) and coerce them to their key or a special value
export const SingleChoiceSettingSchema = SettingBaseSchema.extend({
  type: z.literal("SingleChoice"),
  values: z.record(z.string().nullable()).transform((rec) =>
    Object.fromEntries(
      Object.entries(rec).map(([k, v]) => [k, (v === null && k === "Random") ? "RandomPick" : v ?? k])),
  ),
  default: z.union([z.string(), z.number()]).optional(),
});

export const MultipleChoiceSettingSchema = SettingBaseSchema.extend({
  type: z.literal("MultipleChoice"),
  values: z
    .record(z.string().nullable())
    .transform((rec) =>
      Object.fromEntries(Object.entries(rec).map(([k, v]) => [k, (v === null && k === "Random") ? "RandomPick" : v ?? k])),
    ),
  default: z.string().optional(),
  optionsFor: z.string().optional(),
});

// Sliders that act as auxiliary "choices" (provide a selectable range for another slider)
// are marked with `optionsFor` and do not need a default value themselves.
export const SliderSettingSchema = SettingBaseSchema.extend({
  type: z.literal("Slider"),
  range: z.object({ from: z.number().optional(), to: z.number() }),
  default: z.number().optional(), // optional to support auxiliary option providers
  optionsFor: z.string().optional(),
  combined: z.string().optional(),
});

export const ToggleSettingSchema = SettingBaseSchema.extend({
  type: z.literal("Toggle"),
  default: z.boolean().optional(),
});

export const InputSettingSchema = SettingBaseSchema.extend({
  type: z.literal("Input"),
  default: z.union([z.string(), z.number()]).optional(),
});

export const GenericSettingSchema = SettingBaseSchema.extend({
  type: z.literal("Generic"),
  default: z.union([z.string(), z.number(), z.boolean()]).optional(),
});

export const MetadataSettingSchema = z.discriminatedUnion("type", [
  SingleChoiceSettingSchema,
  MultipleChoiceSettingSchema,
  SliderSettingSchema,
  ToggleSettingSchema,
  InputSettingSchema,
  GenericSettingSchema,
] as const);

// Individual game settings entry. "game" metadata (name/description) is optional and
// settings array may be omitted (defaults to empty) in some backends.
export const GameSettingEntrySchema = z.object({
  game: z
    .object({
      name: z.string().optional(),
      description: z.string().optional(),
      code: z.string().optional(),
    })
    .optional(),
  settings: z.array(MetadataSettingSchema).optional().default([]),
});

export const GameSettingsSchema = z.record(GameSettingEntrySchema);

export const MetadataSchema = z.object({
  settings: z.array(MetadataSettingSchema).optional().default([]),
  gameSettings: GameSettingsSchema.optional().default({}),
  // Optional post-generation settings provided by backend, keyed by game id
  postGenSettings: z.record(GamePostGenConfigSchema).optional().default({}),
});

type UnknownRecord = Record<string, unknown>;

function normalizeRawMetadata(raw: unknown): unknown {
  if (!raw || typeof raw !== "object") return raw;
  const r = raw as UnknownRecord;

  if ("gameSettings" in r) return raw;

  if (r.targetSettings && typeof r.targetSettings === "object") {
    const ts = r.targetSettings as UnknownRecord;
    const gameSettings: Record<
      string,
      {
        game?: { name?: string; description?: string; code?: string };
        settings: unknown[];
      }
    > = {};
    for (const [gameKey, value] of Object.entries(ts)) {
      if (!value || typeof value !== "object") continue;
      const v = value as UnknownRecord;
      const targetInfo =
        v.target && typeof v.target === "object"
          ? (v.target as {
            name?: string;
            description?: string;
            game?: string;
          })
          : undefined;
      const settingsArr = Array.isArray(v.settings) ? v.settings : [];
      gameSettings[gameKey] = {
        game: targetInfo
          ? {
            name: targetInfo.name,
            description: targetInfo.description,
            code: targetInfo.game,
          }
          : undefined,
        settings: settingsArr,
      };
    }
    const { targetSettings: _targetSettings, ...rest } = r;
    void _targetSettings;
    return {
      ...rest,
      gameSettings,
    };
  }

  return raw;
}

/** Convenience function to parse raw metadata. */
export function parseMetadata(raw: unknown) {
  const normalized = normalizeRawMetadata(raw);
  return MetadataSchema.safeParse(normalized);
}
