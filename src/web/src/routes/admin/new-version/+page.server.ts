import { dev } from "$app/environment";
import { fail } from "@sveltejs/kit";
import {
  ADMIN_VERSION_COOKIE_NAME,
  AdminVersionError,
  createRandomizerVersion,
  getConfiguredAdminTokenHash,
  hashAdminToken,
  isAdminAuthorized,
} from "$lib/server/admin/version-service";
import { db } from "$lib/server/db";
import { randomizerVersions } from "$lib/server/db/schema";
import { desc } from "drizzle-orm";
import type { Actions, PageServerLoad } from "./$types";

type RecentVersionSummary = {
  id: string;
  versionTag: string;
  randomizerId: string | null;
  isActive: boolean;
  createdAt: string;
  buildDate: string;
  gitCommitHash: string | null;
};

type CreateActionValues = {
  baseVersion?: string;
  randomizerId?: string;
  metadataId?: string;
  tag?: string;
  activate?: boolean;
  gitCommitHash?: string;
  buildDate?: string;
};

export type ActionData =
  | { type: "authenticate"; success: true }
  | { type: "authenticate"; success: false; message: string }
  | {
      type: "create";
      success: true;
      result: {
        versionId: string;
        versionTag: string;
        randomizerId: string | null;
        metadataId: string;
        canonicalMetadataId: string;
        activated: boolean;
        patchSha256: string;
        gitCommitHash: string | null;
        buildDate: string;
      };
    }
  | {
      type: "create";
      success: false;
      message: string;
      fieldErrors?: Record<string, string>;
      values?: CreateActionValues;
    }
  | { type: "logout"; success: true };

function serializeDate(value: unknown): string {
  if (value instanceof Date) {
    return value.toISOString();
  }
  if (typeof value === "string") {
    const parsed = new Date(value);
    if (!Number.isNaN(parsed.getTime())) {
      return parsed.toISOString();
    }
  }
  return new Date().toISOString();
}

export const load: PageServerLoad = async ({ cookies }) => {
  const configuredTokenHash = getConfiguredAdminTokenHash();
  const authorized =
    configuredTokenHash !== null &&
    cookies.get(ADMIN_VERSION_COOKIE_NAME) === configuredTokenHash;

  let recentVersions: RecentVersionSummary[] = [];
  if (authorized) {
    const rows = await db
      .select({
        id: randomizerVersions.id,
        versionTag: randomizerVersions.versionTag,
        randomizerId: randomizerVersions.randomizerId,
        isActive: randomizerVersions.isActive,
        createdAt: randomizerVersions.createdAt,
        buildDate: randomizerVersions.buildDate,
        gitCommitHash: randomizerVersions.gitCommitHash,
      })
      .from(randomizerVersions)
      .orderBy(desc(randomizerVersions.createdAt))
      .limit(15);

    recentVersions = rows.map((row) => ({
      id: row.id,
      versionTag: row.versionTag,
      randomizerId: row.randomizerId,
      isActive: Boolean(row.isActive),
      createdAt: serializeDate(row.createdAt),
      buildDate: serializeDate(row.buildDate ?? row.createdAt),
      gitCommitHash: row.gitCommitHash ?? null,
    }));
  }

  return {
    authorized,
    secretConfigured: configuredTokenHash !== null,
    recentVersions,
  };
};

export const actions: Actions = {
  authenticate: async ({ request, cookies }) => {
    const configured = getConfiguredAdminTokenHash();
    if (!configured) {
      return fail(500, {
        type: "authenticate",
        success: false,
        message: "Admin token is not configured on the server.",
      });
    }

    const formData = await request.formData();
    const token = formData.get("token");
    if (!token || typeof token !== "string" || token.trim().length === 0) {
      return fail(400, {
        type: "authenticate",
        success: false,
        message: "Access token is required.",
      });
    }

    if (hashAdminToken(token) !== configured) {
      return fail(401, {
        type: "authenticate",
        success: false,
        message: "Invalid access token.",
      });
    }

    cookies.set(ADMIN_VERSION_COOKIE_NAME, configured, {
      path: "/admin",
      httpOnly: true,
      sameSite: "lax",
      secure: !dev,
      maxAge: 60 * 60 * 12, // 12 hours
    });

    return { type: "authenticate", success: true } satisfies ActionData;
  },

  logout: async ({ cookies }) => {
    cookies.delete(ADMIN_VERSION_COOKIE_NAME, { path: "/admin" });
    return { type: "logout", success: true } satisfies ActionData;
  },

  create: async ({ request, cookies }) => {
    if (!isAdminAuthorized(cookies)) {
      return fail(401, {
        type: "create",
        success: false,
        message:
          "You must enter the admin access token before creating versions.",
      });
    }

    const formData = await request.formData();
    const baseVersionRaw = formData.get("baseVersion");
    const randomizerIdRaw = formData.get("randomizerId");
    const metadataIdRaw = formData.get("metadataId");
    const tagRaw = formData.get("tag");
    const activateRaw = formData.get("activate");
    const gitCommitHashRaw = formData.get("gitCommitHash");
    const buildDateRaw = formData.get("buildDate");
    const gitCommitHashValue =
      typeof gitCommitHashRaw === "string"
        ? gitCommitHashRaw.trim()
        : undefined;
    const buildDateValue =
      typeof buildDateRaw === "string" ? buildDateRaw.trim() : undefined;
    const basePatchFile = formData.get("basePatch") ?? formData.get("ipsPatch");

    const values: CreateActionValues = {
      baseVersion:
        typeof baseVersionRaw === "string" ? baseVersionRaw.trim() : undefined,
      randomizerId:
        typeof randomizerIdRaw === "string"
          ? randomizerIdRaw.trim()
          : undefined,
      metadataId:
        typeof metadataIdRaw === "string" ? metadataIdRaw.trim() : undefined,
      tag: typeof tagRaw === "string" ? tagRaw.trim() : undefined,
      activate: activateRaw === "on",
      gitCommitHash:
        gitCommitHashValue && gitCommitHashValue.length > 0
          ? gitCommitHashValue
          : undefined,
      buildDate:
        buildDateValue && buildDateValue.length > 0
          ? buildDateValue
          : undefined,
    };
    let patchBuffer = Buffer.alloc(0);
    if (basePatchFile instanceof File && basePatchFile.size > 0) {
      patchBuffer = Buffer.from(await basePatchFile.arrayBuffer());
    }

    try {
      const result = await createRandomizerVersion({
        baseVersion: values.baseVersion ?? "",
        randomizerId: values.randomizerId ?? "",
        metadataId: values.metadataId,
        tag: values.tag,
        activate: values.activate,
        basePatch: patchBuffer,
        gitCommitHash: values.gitCommitHash,
        buildDate: values.buildDate,
      });
      return {
        type: "create",
        success: true,
        result,
      } satisfies ActionData;
    } catch (err) {
      if (err instanceof AdminVersionError) {
        return fail(err.status, {
          type: "create",
          success: false,
          message: err.message,
          fieldErrors: err.fieldErrors,
          values,
        });
      }
      throw err;
    }
  },
};
