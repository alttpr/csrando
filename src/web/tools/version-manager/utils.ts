import { readFileSync, existsSync } from "fs";
import { resolve, dirname, isAbsolute } from "path";
import { createHash, randomUUID } from "crypto";
import { db } from "../../src/lib/server/db/node";
import { randomizerVersions } from "../../src/lib/server/db/schema";
import { and, eq, ne, sql } from "drizzle-orm";

export type RandomizerId = string | null | undefined;

export interface VersionTagOptions {
  baseVersion: string;
  randomizerId?: RandomizerId;
  explicitTag?: string;
}

export interface BasePatchSourceOptions {
  explicitPath?: string;
  directory?: string;
  map?: Map<string, string>;
  randomizerId?: RandomizerId;
  fallbackDirectories?: string[];
}

export interface MetadataSourceOptions {
  explicitPath?: string;
  directory?: string;
  map?: Map<string, string>;
  metadataUrl?: string;
  backendBaseUrl?: string;
  randomizerId?: RandomizerId;
  fallbackDirectories?: string[];
}

export interface InsertVersionOptions {
  randomizerId?: RandomizerId;
  versionTag: string;
  optionsMetadata: Record<string, unknown>;
  postGenSettings: Record<string, unknown>;
  basePatchBase64: string;
  patchSha256: string;
  activate?: boolean;
  dryRun?: boolean;
  gitCommitHash?: string | null;
  buildDate?: string | Date | null;
}

export interface InsertResult {
  versionId: string;
  versionTag: string;
  randomizerId: string | null;
  patchSha256: string;
  inserted: boolean;
  gitCommitHash: string | null;
  buildDate: string;
}

export function parsePathMap(filePath: string): Map<string, string> {
  const resolved = resolve(filePath);
  let parsed: unknown;
  try {
    const raw = readFileSync(resolved, "utf8");
    parsed = JSON.parse(raw) as Record<string, unknown>;
  } catch (err) {
    throw new Error(`Failed to read map file at ${resolved}: ${err}`);
  }

  if (!parsed || typeof parsed !== "object") {
    throw new Error(
      `Map file ${resolved} must export an object of key/value pairs.`,
    );
  }

  const baseDir = dirname(resolved);
  const out = new Map<string, string>();
  for (const [key, value] of Object.entries(parsed)) {
    if (typeof value !== "string") continue;
    const normalizedKey = key.trim().toLowerCase();
    if (!normalizedKey) continue;
    const trimmedValue = value.trim();
    if (!trimmedValue) continue;
    const finalPath = isAbsolute(trimmedValue)
      ? trimmedValue
      : resolve(baseDir, trimmedValue);
    out.set(normalizedKey, finalPath);
  }
  return out;
}

export function normalizeRandomizerId(id: RandomizerId): string | null {
  if (!id) return null;
  const trimmed = String(id).trim();
  return trimmed.length > 0 ? trimmed.toLowerCase() : null;
}

export function sanitizeBaseUrl(baseUrl?: string | null): string | undefined {
  if (!baseUrl) return undefined;
  const trimmed = baseUrl.trim();
  if (!trimmed) return undefined;
  return trimmed.replace(/\/$/, "");
}

function normalizeGitCommitHash(value?: string | null): string | null {
  if (value == null) {
    return null;
  }
  const trimmed = value.trim();
  if (!trimmed) {
    return null;
  }
  const normalized = trimmed.toLowerCase();
  if (!/^[0-9a-f]{7,40}$/.test(normalized)) {
    throw new Error("Git commit hash must be 7-40 hexadecimal characters.");
  }
  return normalized;
}

function resolveBuildDate(value?: string | Date | null): Date {
  if (!value) {
    return new Date();
  }
  if (value instanceof Date) {
    if (Number.isNaN(value.getTime())) {
      throw new Error("Build date must be a valid date/time.");
    }
    return value;
  }
  const trimmed = value.trim();
  if (!trimmed) {
    return new Date();
  }
  if (trimmed.toLowerCase() === "now") {
    return new Date();
  }
  const parsed = new Date(trimmed);
  if (Number.isNaN(parsed.getTime())) {
    throw new Error("Build date must be a valid date/time.");
  }
  return parsed;
}

export function computeVersionTag({
  baseVersion,
  randomizerId,
  explicitTag,
}: VersionTagOptions): string {
  if (explicitTag && explicitTag.trim().length > 0) {
    return explicitTag.trim();
  }
  const base = baseVersion.trim();
  if (!base) {
    throw new Error("Version tag cannot be empty.");
  }
  const normalizedId = normalizeRandomizerId(randomizerId);
  if (!normalizedId) {
    return base;
  }
  const suffix = `-${normalizedId}`;
  if (base.toLowerCase().endsWith(suffix)) {
    return base;
  }
  return `${base}${suffix}`;
}

export function resolveBasePatchPath(options: BasePatchSourceOptions): string {
  const {
    explicitPath,
    directory,
    map,
    randomizerId,
    fallbackDirectories = ["static"],
  } = options;

  if (explicitPath) {
    const resolved = resolve(explicitPath);
    if (!existsSync(resolved)) {
      throw new Error(`Base patch file not found at ${resolved}`);
    }
    return resolved;
  }

  const normalizedId = normalizeRandomizerId(randomizerId);
  if (!normalizedId) {
    throw new Error(
      "Base patch path not provided and randomizer id is missing. Provide --patch/--ips or --randomizer.",
    );
  }

  const candidates: string[] = [];
  if (map) {
    const mapped = map.get(normalizedId);
    if (mapped) {
      candidates.push(resolve(mapped));
    }
  }
  const extensions = [".bps", ".ips"];
  if (directory) {
    for (const ext of extensions) {
      candidates.push(resolve(directory, `${normalizedId}${ext}`));
    }
  }
  for (const dir of fallbackDirectories) {
    for (const ext of extensions) {
      candidates.push(resolve(dir, `${normalizedId}${ext}`));
    }
  }

  for (const candidate of candidates) {
    if (existsSync(candidate)) {
      return candidate;
    }
  }

  const attempted = [explicitPath, ...candidates].filter(Boolean).join(", ");
  throw new Error(
    `Unable to locate base patch for ${normalizedId}. Checked paths: ${attempted || "<none>"}`,
  );
}

export function readPatchData(patchPath: string): {
  base64: string;
  sha256: string;
} {
  const data = readFileSync(patchPath);
  return {
    base64: data.toString("base64"),
    sha256: createHash("sha256").update(data).digest("hex"),
  };
}

export interface LoadedMetadata {
  data: Record<string, unknown>;
  source: string;
}

export async function loadMetadata(
  options: MetadataSourceOptions,
): Promise<LoadedMetadata> {
  const {
    explicitPath,
    directory,
    map,
    metadataUrl,
    backendBaseUrl,
    randomizerId,
    fallbackDirectories = ["metadata"],
  } = options;

  if (explicitPath) {
    const resolved = resolve(explicitPath);
    try {
      const text = readFileSync(resolved, "utf8");
      return {
        data: JSON.parse(text) as Record<string, unknown>,
        source: resolved,
      };
    } catch (err) {
      throw new Error(`Failed to read metadata file at ${resolved}: ${err}`);
    }
  }

  const normalizedId = normalizeRandomizerId(randomizerId);

  if (normalizedId && map) {
    const mapped = map.get(normalizedId);
    if (mapped) {
      const resolved = resolve(mapped);
      try {
        const text = readFileSync(resolved, "utf8");
        return {
          data: JSON.parse(text) as Record<string, unknown>,
          source: resolved,
        };
      } catch (err) {
        throw new Error(
          `Failed to read metadata for ${normalizedId} at ${resolved}: ${err}`,
        );
      }
    }
  }

  if (normalizedId && directory) {
    const candidate = resolve(directory, `${normalizedId}.json`);
    if (existsSync(candidate)) {
      try {
        const text = readFileSync(candidate, "utf8");
        return {
          data: JSON.parse(text) as Record<string, unknown>,
          source: candidate,
        };
      } catch (err) {
        throw new Error(
          `Failed to read metadata for ${normalizedId} at ${candidate}: ${err}`,
        );
      }
    }
  }

  if (normalizedId) {
    for (const dir of fallbackDirectories) {
      const candidate = resolve(dir, `${normalizedId}.json`);
      if (existsSync(candidate)) {
        try {
          const text = readFileSync(candidate, "utf8");
          return {
            data: JSON.parse(text) as Record<string, unknown>,
            source: candidate,
          };
        } catch (err) {
          throw new Error(
            `Failed to read metadata for ${normalizedId} at ${candidate}: ${err}`,
          );
        }
      }
    }
  }

  const sanitizedBase = sanitizeBaseUrl(backendBaseUrl);
  let finalUrl = metadataUrl?.trim();

  if (!finalUrl && sanitizedBase && normalizedId) {
    finalUrl = `${sanitizedBase}/meta/${normalizedId}`;
  }

  if (!finalUrl) {
    throw new Error(
      "Metadata source not provided. Supply --metadata, --metadataDir/--metadataMap, or --backend/--metadataUrl.",
    );
  }

  const res = await fetch(finalUrl);
  if (!res.ok) {
    throw new Error(
      `Failed to fetch metadata from ${finalUrl}: HTTP ${res.status}`,
    );
  }

  return {
    data: (await res.json()) as Record<string, unknown>,
    source: finalUrl,
  };
}

export async function ensureVersionTagAvailable(
  versionTag: string,
): Promise<void> {
  const existing = await db
    .select({ id: randomizerVersions.id })
    .from(randomizerVersions)
    .where(eq(randomizerVersions.versionTag, versionTag))
    .limit(1);

  if (existing.length > 0) {
    throw new Error(
      `Version tag ${versionTag} already exists in randomizer_version table. Use --tag to provide a unique value.`,
    );
  }
}

export async function insertVersion({
  randomizerId,
  versionTag,
  optionsMetadata,
  postGenSettings,
  basePatchBase64,
  patchSha256,
  activate,
  dryRun,
  gitCommitHash,
  buildDate,
}: InsertVersionOptions): Promise<InsertResult> {
  const normalizedId = normalizeRandomizerId(randomizerId);
  const versionId = randomUUID();
  const normalizedCommit = normalizeGitCommitHash(gitCommitHash ?? null);
  const resolvedBuildDate = resolveBuildDate(buildDate);

  if (dryRun) {
    return {
      versionId,
      versionTag,
      randomizerId: normalizedId,
      patchSha256,
      inserted: false,
      gitCommitHash: normalizedCommit,
      buildDate: resolvedBuildDate.toISOString(),
    };
  }

  await ensureVersionTagAvailable(versionTag);

  await db.insert(randomizerVersions).values({
    id: versionId,
    randomizerId: normalizedId,
    versionTag,
    optionsMetadata,
    postGenSettings,
    ipsBasePatchBase64: basePatchBase64,
    basePatchSha256: patchSha256,
    gitCommitHash: normalizedCommit,
    buildDate: resolvedBuildDate,
    isActive: Boolean(activate),
    createdAt: new Date(),
  });

  if (activate) {
    if (normalizedId) {
      await db
        .update(randomizerVersions)
        .set({ isActive: false })
        .where(
          and(
            eq(randomizerVersions.randomizerId, normalizedId),
            ne(randomizerVersions.id, versionId),
          ),
        );
    } else {
      await db
        .update(randomizerVersions)
        .set({ isActive: false })
        .where(
          and(
            sql`randomizer_id IS NULL` as unknown as any,
            ne(randomizerVersions.id, versionId),
          ),
        );
    }
    await db
      .update(randomizerVersions)
      .set({ isActive: true })
      .where(eq(randomizerVersions.id, versionId));
  }

  return {
    versionId,
    versionTag,
    randomizerId: normalizedId,
    patchSha256,
    inserted: true,
    gitCommitHash: normalizedCommit,
    buildDate: resolvedBuildDate.toISOString(),
  };
}

export function toMetadataPair(meta: Record<string, unknown>): {
  optionsMetadata: Record<string, unknown>;
  postGenSettings: Record<string, unknown>;
} {
  const postGenSettings =
    (meta as { postGenSettings?: Record<string, unknown> }).postGenSettings ??
    {};
  return {
    optionsMetadata: { ...meta },
    postGenSettings,
  };
}
