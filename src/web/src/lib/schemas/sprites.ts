import { z } from "zod";

export const SpritePatchFileSchema = z.object({
  id: z.string().min(1),
  path: z.string().min(1),
});

export const SpritePatchEntrySchema = z.object({
  fileId: z.string().min(1),
  targetAddress: z.union([z.number().int(), z.string().min(1)]),
  dataLength: z.number().int().nonnegative(),
  dataOffsetInFile: z.number().int().nonnegative().optional(),
});

export const SpritePatchDetailsSchema = z.object({
  files: z.array(SpritePatchFileSchema),
  patches: z.array(SpritePatchEntrySchema),
});

export const SpriteInfoSchema = z.object({
  value: z.string().min(1),
  name: z.string().min(1),
  imagePath: z.string().min(1),
  rdcPath: z.string().min(1).optional(),
  kind: z
    .enum(["rdc/link", "rdc/samus", "rdc/nes-z1", "rdc/nes-m1"])
    .optional(),
  patchDetails: SpritePatchDetailsSchema.optional(),
});

export const GameSpriteConfigSchema = z.object({
  defaultSpriteValue: z.string().optional(),
  sprites: z.array(SpriteInfoSchema),
});
