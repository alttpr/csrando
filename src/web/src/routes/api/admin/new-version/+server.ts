import { error, json, type RequestHandler } from "@sveltejs/kit";
import {
  AdminVersionError,
  createRandomizerVersion,
  getConfiguredAdminTokenHash,
  isTokenAuthorized,
} from "$lib/server/admin/version-service";

type CreateVersionPayload = {
  baseVersion?: unknown;
  randomizerId?: unknown;
  metadataId?: unknown;
  tag?: unknown;
  activate?: unknown;
  basePatchBase64?: unknown;
  ipsPatchBase64?: unknown;
  gitCommitHash?: unknown;
  buildDate?: unknown;
};

function extractAdminToken(request: Request): string | null {
  const authHeader = request.headers.get("authorization");
  if (authHeader && authHeader.startsWith("Bearer ")) {
    const token = authHeader.slice("Bearer ".length).trim();
    if (token.length > 0) {
      return token;
    }
  }
  const headerToken = request.headers.get("x-admin-version-token");
  if (headerToken && headerToken.trim().length > 0) {
    return headerToken.trim();
  }
  return null;
}

export const POST: RequestHandler = async ({ request }) => {
  if (!getConfiguredAdminTokenHash()) {
    throw error(500, {
      message: "Admin token is not configured on the server.",
    });
  }

  const providedToken = extractAdminToken(request);
  if (!isTokenAuthorized(providedToken)) {
    throw error(401, { message: "Invalid admin access token." });
  }

  let payload: CreateVersionPayload;
  try {
    const body = await request.json();
    if (!body || typeof body !== "object") {
      throw error(400, { message: "Request body must be a JSON object." });
    }
    payload = body as CreateVersionPayload;
  } catch (err) {
    if ((err as { status?: number }).status) {
      throw err;
    }
    throw error(400, { message: "Request body must be valid JSON." });
  }

  const baseVersion =
    typeof payload.baseVersion === "string" ? payload.baseVersion : "";
  const randomizerId =
    typeof payload.randomizerId === "string" ? payload.randomizerId : "";
  const metadataId =
    typeof payload.metadataId === "string" ? payload.metadataId : undefined;
  const tag = typeof payload.tag === "string" ? payload.tag : undefined;
  const activate = payload.activate === true;
  const gitCommitHash =
    typeof payload.gitCommitHash === "string"
      ? payload.gitCommitHash
      : undefined;
  const buildDate =
    typeof payload.buildDate === "string" ? payload.buildDate : undefined;
  const patchBase64Raw =
    typeof payload.basePatchBase64 === "string"
      ? payload.basePatchBase64
      : typeof payload.ipsPatchBase64 === "string"
        ? payload.ipsPatchBase64
        : "";
  const patchBase64 = patchBase64Raw.replace(/\s+/g, "");

  let patchBuffer: Buffer;
  try {
    patchBuffer = Buffer.from(patchBase64, "base64");
  } catch {
    throw error(400, {
      message: "basePatchBase64 must be a base64 encoded string.",
    });
  }
  if (patchBase64.length > 0 && patchBuffer.length === 0) {
    throw error(400, {
      message: "basePatchBase64 must be a base64 encoded string.",
    });
  }

  try {
    const result = await createRandomizerVersion({
      baseVersion,
      randomizerId,
      metadataId,
      tag,
      activate,
      basePatch: patchBuffer,
      gitCommitHash,
      buildDate,
    });
    return json(result);
  } catch (err) {
    if (err instanceof AdminVersionError) {
      throw error(err.status, {
        message: err.message,
        fieldErrors: err.fieldErrors,
      });
    }
    throw err;
  }
};
