import { env as privateEnv } from "$env/dynamic/private";
import type { Cookies } from "@sveltejs/kit";
import { metadataApi } from "$lib/services/api";
import { db } from "$lib/server/db";
import { randomizerVersions } from "$lib/server/db/schema";
import { and, eq, ne, sql } from "drizzle-orm";
import { createHash, randomUUID } from "crypto";

export const ADMIN_VERSION_COOKIE_NAME = "admin_version_token";

export class AdminVersionError extends Error {
  status: number;
  fieldErrors?: Record<string, string>;

  constructor(
    status: number,
    message: string,
    fieldErrors?: Record<string, string>,
  ) {
    super(message);
    this.name = "AdminVersionError";
    this.status = status;
    if (fieldErrors) {
      this.fieldErrors = fieldErrors;
    }
  }
}

export function hashAdminToken(token: string): string {
  return createHash("sha256").update(token).digest("hex");
}

export function getConfiguredAdminTokenHash(): string | null {
  const secret = privateEnv.PRIVATE_ADMIN_VERSION_TOKEN;
  if (!secret || secret.trim().length === 0) {
    return null;
  }
  return hashAdminToken(secret);
}

export function isAdminAuthorized(cookies: Cookies): boolean {
  const configured = getConfiguredAdminTokenHash();
  if (!configured) {
    return false;
  }
  return cookies.get(ADMIN_VERSION_COOKIE_NAME) === configured;
}

export function isTokenAuthorized(token: string | null | undefined): boolean {
  const configured = getConfiguredAdminTokenHash();
  if (!configured || !token || token.trim().length === 0) {
    return false;
  }
  return hashAdminToken(token) === configured;
}

type CreateVersionInput = {
  baseVersion: string;
  randomizerId: string;
  metadataId?: string | null;
  tag?: string | null;
  activate?: boolean;
  basePatch: Buffer;
};

export type CreateVersionResult = {
  versionId: string;
  versionTag: string;
  randomizerId: string | null;
  metadataId: string;
  canonicalMetadataId: string;
  activated: boolean;
  patchSha256: string;
};

function normalizeRandomizerId(id: string | null | undefined): string | null {
  if (!id) {
    return null;
  }
  const trimmed = id.trim().toLowerCase();
  return trimmed.length > 0 ? trimmed : null;
}

function computeVersionTag(
  baseVersion: string,
  randomizerId: string | null,
  explicitTag?: string | null,
): string {
  const override = explicitTag?.trim();
  if (override) {
    return override;
  }
  const base = baseVersion.trim();
  if (!base) {
    throw new AdminVersionError(400, "Version tag cannot be empty.", {
      versionTag: "Version tag cannot be empty.",
    });
  }
  if (!randomizerId) {
    return base;
  }
  const suffix = `-${randomizerId}`;
  if (base.toLowerCase().endsWith(suffix)) {
    return base;
  }
  return `${base}${suffix}`;
}

type ErrorWithMessage = { message?: unknown };
type ErrorWithBody = { body?: unknown };

function extractErrorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === "object") {
    if (
      "message" in err &&
      typeof (err as ErrorWithMessage).message === "string"
    ) {
      return (err as { message: string }).message;
    }
    if ("body" in err) {
      const body = (err as ErrorWithBody).body;
      if (
        body &&
        typeof body === "object" &&
        "message" in body &&
        typeof (body as ErrorWithMessage).message === "string"
      ) {
        return String((body as { message: string }).message);
      }
    }
  }
  if (typeof err === "string" && err.trim().length > 0) {
    return err;
  }
  return fallback;
}

export async function createRandomizerVersion(
  input: CreateVersionInput,
): Promise<CreateVersionResult> {
  const baseVersion = input.baseVersion?.trim() ?? "";
  const randomizerIdRaw = input.randomizerId?.trim() ?? "";
  const metadataIdRaw = input.metadataId?.trim() ?? "";
  const tagRaw = input.tag?.trim() ?? "";
  const activate = Boolean(input.activate);
  const patchBuffer = input.basePatch;

  const fieldErrors: Record<string, string> = {};

  if (!baseVersion) {
    fieldErrors.baseVersion = "Base version label is required.";
  }
  if (!randomizerIdRaw) {
    fieldErrors.randomizerId = "Randomizer identifier is required.";
  }
  if (!patchBuffer || patchBuffer.length === 0) {
    fieldErrors.basePatch = "Provide an IPS or BPS patch file.";
  }

  if (Object.keys(fieldErrors).length > 0) {
    throw new AdminVersionError(
      400,
      "Fix the highlighted errors and try again.",
      fieldErrors,
    );
  }

  const randomizerId = normalizeRandomizerId(randomizerIdRaw);
  const metadataLookup = metadataIdRaw || randomizerIdRaw;

  let canonicalMetadataId = metadataLookup;
  try {
    canonicalMetadataId = await metadataApi.resolveCanonicalId(metadataLookup);
  } catch (err) {
    console.error("Failed to resolve canonical metadata id", err);
    throw new AdminVersionError(
      500,
      "Failed to resolve metadata identifier from the backend.",
    );
  }

  let metadata: Record<string, unknown>;
  try {
    const fetched = await metadataApi.getById(canonicalMetadataId);
    metadata = structuredClone(fetched) as Record<string, unknown>;
  } catch (err) {
    const status = (err as { status?: number }).status ?? 500;
    const message = extractErrorMessage(
      err,
      "Failed to fetch metadata from the backend.",
    );
    console.error("Failed to fetch metadata", err);
    throw new AdminVersionError(status, message);
  }

  const postGenSettings =
    (metadata.postGenSettings && typeof metadata.postGenSettings === "object"
      ? (metadata.postGenSettings as Record<string, unknown>)
      : {}) ?? {};

  let versionTag: string;
  try {
    versionTag = computeVersionTag(baseVersion, randomizerId, tagRaw);
  } catch (err) {
    if (err instanceof AdminVersionError) {
      throw err;
    }
    const message = extractErrorMessage(err, "Invalid version tag");
    throw new AdminVersionError(400, message, { versionTag: message });
  }

  const existing = await db
    .select({ id: randomizerVersions.id })
    .from(randomizerVersions)
    .where(eq(randomizerVersions.versionTag, versionTag))
    .limit(1);
  if (existing.length > 0) {
    throw new AdminVersionError(
      400,
      `Version tag ${versionTag} already exists. Choose a different tag.`,
      { versionTag: "Version tag already exists." },
    );
  }

  const patchSha256 = createHash("sha256").update(patchBuffer).digest("hex");
  const ipsBase64 = patchBuffer.toString("base64");
  const versionId = randomUUID();

  try {
    await db.insert(randomizerVersions).values({
      id: versionId,
      randomizerId,
      versionTag,
      optionsMetadata: metadata,
      postGenSettings,
      ipsBasePatchBase64: ipsBase64,
      basePatchSha256: patchSha256,
      isActive: activate,
      createdAt: new Date(),
    });
  } catch (err) {
    console.error("Failed to insert randomizer version", err);
    throw new AdminVersionError(
      500,
      "Failed to store the randomizer version in the database.",
    );
  }

  if (activate) {
    try {
      if (randomizerId) {
        await db
          .update(randomizerVersions)
          .set({ isActive: false })
          .where(
            and(
              eq(randomizerVersions.randomizerId, randomizerId),
              ne(randomizerVersions.id, versionId),
            ),
          );
      } else {
        await db
          .update(randomizerVersions)
          .set({ isActive: false })
          .where(
            and(
              sql`randomizer_id IS NULL`,
              ne(randomizerVersions.id, versionId),
            ),
          );
      }
      await db
        .update(randomizerVersions)
        .set({ isActive: true })
        .where(eq(randomizerVersions.id, versionId));
    } catch (err) {
      console.error("Failed updating active randomizer version flag", err);
      throw new AdminVersionError(
        500,
        "Version saved, but failed to update active status. Please review versions manually.",
      );
    }
  }

  return {
    versionId,
    versionTag,
    randomizerId,
    metadataId: metadataLookup,
    canonicalMetadataId,
    activated: activate,
    patchSha256,
  };
}
