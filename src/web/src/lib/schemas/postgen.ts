import { z } from "zod";

const TargetAddressSchema = z
  .union([z.number().int(), z.string().min(1)])
  .transform((value, ctx) => {
    if (typeof value === "number") return value;
    const trimmed = value.trim();
    const parsed = Number.parseInt(trimmed, 16);
    if (Number.isNaN(parsed)) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: `Invalid hex address: ${value}`,
      });
      return z.NEVER;
    }
    return parsed;
  });

export const PostGenPatchEntrySchema = z.object({
  targetAddress: TargetAddressSchema,
  // Bytes to write at targetAddress. Either array of 0-255 or hex string ("FF00AA" or "FF 00 AA")
  data: z.union([
    z.array(z.number().int().min(0).max(0xff)),
    z.string().min(1),
  ]),
});

export const TogglePostGenSettingSchema = z.object({
  id: z.string().min(1),
  name: z.string().min(1),
  description: z.string().optional(),
  type: z.literal("toggle"),
  default: z.boolean().optional().default(false),
  on: z
    .object({ patches: z.array(PostGenPatchEntrySchema).default([]) })
    .default({ patches: [] }),
  off: z
    .object({ patches: z.array(PostGenPatchEntrySchema).default([]) })
    .optional(),
});

export const SelectPostGenSettingSchema = z.object({
  id: z.string().min(1),
  name: z.string().min(1),
  description: z.string().optional(),
  type: z.literal("select"),
  default: z.string().optional(),
  choices: z
    .array(
      z.object({
        value: z.string().min(1),
        label: z.string().min(1),
        patches: z.array(PostGenPatchEntrySchema).default([]),
      }),
    )
    .min(1),
});

export const PostGenSettingSchema = z.discriminatedUnion("type", [
  TogglePostGenSettingSchema,
  SelectPostGenSettingSchema,
]);

export const GamePostGenConfigSchema = z.object({
  options: z.array(PostGenSettingSchema).default([]),
});
