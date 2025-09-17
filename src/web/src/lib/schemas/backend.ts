import { z } from "zod";

export const BackendWorldPatchSchema = z.object({
  bpsPatch: z.string().optional(),
  ipsPatch: z.string().optional(),
});

export const RandomizerResponseSchema = z.object({
  seed: z.number().int(),
  worlds: z.record(BackendWorldPatchSchema),
});

// Minimal request validation for /api/randomize
export const RandomizeRequestSchema = z.object({
  Seed: z.number().int().nonnegative(),
  IncludeSpoiler: z.boolean(),
  Configs: z.array(z.record(z.unknown())).min(1),
});
