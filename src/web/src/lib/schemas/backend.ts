import { z } from "zod";

export const BackendWorldPatchSchema = z
  .object({
    bpsPatch: z.string().optional(),
    ipsPatch: z.string().optional(),
  })
  .refine((world) => !!world.bpsPatch || !!world.ipsPatch, {
    message: "A world must include an IPS or BPS patch",
  });

export const RandomizerResponseSchema = z.object({
  seed: z.number().int(),
  worlds: z.record(BackendWorldPatchSchema),
  spoilerLog: z.record(z.record(z.string())).optional(),
});

// Optional seed-profile attribution attached by the config page. Stored with
// the generated seed for provenance; stripped before forwarding to the
// generator backend and never required for generation.
export const SeedProfileAttributionSchema = z.object({
  profileId: z.string().nullable().optional(),
  profileRevisionId: z.string().nullable().optional(),
  settingsSnapshot: z.record(z.unknown()).optional(),
  configSchemaVersion: z.number().int().min(1).optional(),
});

// Minimal request validation for /api/randomize
export const RandomizeRequestSchema = z.object({
  Seed: z.number().int().nonnegative(),
  IncludeSpoiler: z.boolean(),
  Configs: z.array(z.record(z.unknown())).min(1),
  Profile: SeedProfileAttributionSchema.optional(),
});

// Alternative /api/randomize body for external tools: generate directly from
// a saved profile without submitting the full configuration. The server
// expands this into a regular randomize request.
export const RandomizeByProfileRequestSchema = z.object({
  ProfileId: z.string().min(1),
  RevisionId: z.string().optional(),
  Seed: z.number().int().nonnegative().optional(),
  IncludeSpoiler: z.boolean().optional(),
});
