import { beforeAll, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import {
  rawMetadataFixture,
  loadMetadataFixture,
  defaultFormStateFixture,
} from "../fixtures/metadata";
import { normalizeConfig, type NormalizedConfig } from "$lib/config/normalize";
import { seeds, users } from "$lib/server/db/schema";

vi.mock("$env/dynamic/private", () => ({
  env: new Proxy(
    {},
    { get: (_target, key: string) => process.env[key] ?? undefined },
  ),
}));

vi.mock("$app/environment", () => ({
  browser: false,
  dev: true,
  building: false,
  version: "test",
}));

vi.mock("$lib/services/api", () => ({
  metadataApi: {
    resolveCanonicalId: vi.fn(async (id: string) => id),
    getById: vi.fn(async () => rawMetadataFixture),
  },
}));

type DbModule = typeof import("$lib/server/db");
type ServiceModule = typeof import("$lib/server/presets/service");
type AdminServiceModule = typeof import("$lib/server/admin/admin-service");
type PromotePresetRoute =
  typeof import("../../src/routes/api/presets/[id]/promote/+server");
type PromoteUserRoute =
  typeof import("../../src/routes/api/admin/promote/+server");
type SpoilerRoute =
  typeof import("../../src/routes/api/admin/seed/[id]/spoiler/+server");
type PromoteSharedRoute =
  typeof import("../../src/routes/api/presets/promote-shared/+server");
type ResetPasswordRoute =
  typeof import("../../src/routes/api/admin/users/[id]/reset-password/+server");
type ChangePasswordRoute =
  typeof import("../../src/routes/api/user/password/+server");

let db: DbModule["db"];
let service: ServiceModule;
let adminService: AdminServiceModule;
let promotePresetRoute: PromotePresetRoute;
let promoteUserRoute: PromoteUserRoute;
let spoilerRoute: SpoilerRoute;
let promoteSharedRoute: PromoteSharedRoute;
let resetPasswordRoute: ResetPasswordRoute;
let changePasswordRoute: ChangePasswordRoute;

const admin: User = {
  id: "panel-admin-0001",
  username: "panel-admin",
  githubId: null,
  isAdmin: true,
};
const member: User = {
  id: "panel-member-001",
  username: "panel-member",
  githubId: null,
  isAdmin: false,
};

const noCookies = { get: () => undefined } as never;

function sessionLocals(user: User | null) {
  return { user, session: user ? { id: "session" } : null };
}

function jsonRequest(url: string, body: unknown): Request {
  return new Request(`http://localhost${url}`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
}

function defaultSettings(): NormalizedConfig {
  const metadata = loadMetadataFixture();
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  delete process.env.PRIVATE_ADMIN_VERSION_TOKEN;
  ({ db } = await import("$lib/server/db"));
  service = await import("$lib/server/presets/service");
  adminService = await import("$lib/server/admin/admin-service");
  promotePresetRoute = await import(
    "../../src/routes/api/presets/[id]/promote/+server"
  );
  promoteUserRoute = await import("../../src/routes/api/admin/promote/+server");
  spoilerRoute = await import(
    "../../src/routes/api/admin/seed/[id]/spoiler/+server"
  );
  promoteSharedRoute = await import(
    "../../src/routes/api/presets/promote-shared/+server"
  );
  resetPasswordRoute = await import(
    "../../src/routes/api/admin/users/[id]/reset-password/+server"
  );
  changePasswordRoute = await import(
    "../../src/routes/api/user/password/+server"
  );

  for (const user of [admin, member]) {
    await db.insert(users).values({
      id: user.id,
      username: user.username,
      isAdmin: user.isAdmin,
    });
  }
});

describe("promotePresetToOfficial", () => {
  it("copies an admin's preset into a new official preset", async () => {
    const source = await service.createPreset(admin, {
      configId: "combo",
      name: "Panel Source",
      settings: defaultSettings(),
    });

    const response = await promotePresetRoute.POST({
      params: { id: source.preset.id },
      request: jsonRequest(`/api/presets/${source.preset.id}/promote`, {
        slug: "panel-promoted",
        name: "Promoted Preset",
      }),
      locals: sessionLocals(admin),
    } as never);
    expect(response.status).toBe(201);
    const body = await response.json();
    expect(body.preset).toMatchObject({
      scope: "official",
      slug: "panel-promoted",
      name: "Promoted Preset",
      configId: "combo",
    });

    // The source stays a private user preset.
    const untouched = await service.getReadablePreset(source.preset.id, admin);
    expect(untouched.scope).toBe("user");
  });

  it("rejects non-admins and duplicate slugs", async () => {
    const source = await service.createPreset(member, {
      configId: "combo",
      name: "Member Source",
      settings: defaultSettings(),
    });
    await expect(
      service.promotePresetToOfficial(member, source.preset.id, {
        slug: "member-slug",
      }),
    ).rejects.toMatchObject({ status: 403 });

    await expect(
      service.promotePresetToOfficial(admin, source.preset.id, {
        slug: "panel-promoted",
      }),
    ).rejects.toMatchObject({ status: 404 }); // member's private preset is invisible

    const own = await service.createPreset(admin, {
      configId: "combo",
      name: "Panel Slug Clash",
      settings: defaultSettings(),
    });
    await expect(
      service.promotePresetToOfficial(admin, own.preset.id, {
        slug: "panel-promoted",
      }),
    ).rejects.toMatchObject({ status: 400 });
  });
});

describe("listOfficialPresetsForAdmin", () => {
  it("includes archived presets", async () => {
    const { preset } = await service.createPreset(admin, {
      configId: "combo",
      name: "Panel Archived",
      settings: defaultSettings(),
      scope: "official",
      slug: "panel-archived",
    });
    await service.updatePresetMeta(admin, preset.id, { archived: true });

    const all = await service.listOfficialPresetsForAdmin();
    const archived = all.find((p) => p.id === preset.id);
    expect(archived?.archived).toBe(true);
  });
});

describe("POST /api/admin/promote with a session admin", () => {
  it("promotes and demotes users without the server token", async () => {
    const response = await promoteUserRoute.POST({
      request: jsonRequest("/api/admin/promote", {
        username: member.username,
        isAdmin: true,
      }),
      cookies: noCookies,
      locals: sessionLocals(admin),
    } as never);
    expect(response.status).toBe(200);
    expect((await response.json()).user.isAdmin).toBe(true);

    const demote = await promoteUserRoute.POST({
      request: jsonRequest("/api/admin/promote", {
        username: member.username,
        isAdmin: false,
      }),
      cookies: noCookies,
      locals: sessionLocals(admin),
    } as never);
    expect(demote.status).toBe(200);
  });

  it("blocks self-demotion and non-admin sessions", async () => {
    await expect(
      promoteUserRoute.POST({
        request: jsonRequest("/api/admin/promote", {
          username: admin.username,
          isAdmin: false,
        }),
        cookies: noCookies,
        locals: sessionLocals(admin),
      } as never),
    ).rejects.toMatchObject({ status: 400 });

    // Without a session admin and without a configured token, access fails.
    await expect(
      promoteUserRoute.POST({
        request: jsonRequest("/api/admin/promote", {
          username: member.username,
          isAdmin: true,
        }),
        cookies: noCookies,
        locals: sessionLocals(member),
      } as never),
    ).rejects.toMatchObject({ status: 500 });
  });
});

describe("GET /api/admin/seed/[id]/spoiler", () => {
  it("serves the spoiler to session admins and rejects everyone else", async () => {
    await db.insert(seeds).values({
      id: "panel-race-seed1",
      options: { IncludeSpoiler: false },
      patchData: {},
      placementInfo: {},
      spoilerLog: { alttp: { "Link's House": "Moon Pearl" } },
    });

    const ok = await spoilerRoute.GET({
      params: { id: "panel-race-seed1" },
      cookies: noCookies,
      locals: sessionLocals(admin),
    } as never);
    expect(ok.status).toBe(200);
    expect((await ok.json()).spoilerLog).toBeTruthy();

    await expect(
      spoilerRoute.GET({
        params: { id: "panel-race-seed1" },
        cookies: noCookies,
        locals: sessionLocals(member),
      } as never),
    ).rejects.toMatchObject({ status: 403 });
  });
});

describe("promote from share link", () => {
  it("lets an admin promote another user's shared preset", async () => {
    const source = await service.createPreset(member, {
      configId: "combo",
      name: "Member Shared",
      settings: defaultSettings(),
    });
    const token = await service.ensureShareToken(member, source.preset.id);

    const response = await promoteSharedRoute.POST({
      request: jsonRequest("/api/presets/promote-shared", {
        token,
        slug: "shared-promoted",
      }),
      locals: sessionLocals(admin),
    } as never);
    expect(response.status).toBe(201);
    const body = await response.json();
    expect(body.preset).toMatchObject({
      scope: "official",
      slug: "shared-promoted",
      name: "Member Shared",
    });

    // The member's preset stays private and untouched.
    const untouched = await service.getReadablePreset(source.preset.id, member);
    expect(untouched.scope).toBe("user");
  });

  it("rejects invalid tokens and non-admins", async () => {
    await expect(
      service.promoteSharedPresetToOfficial(admin, "bogus-token", {
        slug: "never-used",
      }),
    ).rejects.toMatchObject({ status: 404 });

    const source = await service.createPreset(member, {
      configId: "combo",
      name: "Member Shared Two",
      settings: defaultSettings(),
    });
    const token = await service.ensureShareToken(member, source.preset.id);
    await expect(
      service.promoteSharedPresetToOfficial(member, token, {
        slug: "member-cannot",
      }),
    ).rejects.toMatchObject({ status: 403 });
  });
});

describe("password management", () => {
  it("admin reset issues a working password and kills sessions", async () => {
    const { Argon2id } = await import("oslo/password");
    const { sessions } = await import("$lib/server/db/schema");
    const { eq } = await import("drizzle-orm");

    await db.insert(sessions).values({
      id: "panel-session-1",
      userId: member.id,
      expiresAt: Math.floor(Date.now() / 1000) + 3600,
    });

    const response = await resetPasswordRoute.POST({
      params: { id: member.id },
      locals: sessionLocals(admin),
      cookies: noCookies,
    } as never);
    expect(response.status).toBe(200);
    const body = await response.json();
    expect(body.username).toBe(member.username);
    expect(body.password).toMatch(/^[A-Za-z0-9]{14}$/);

    const row = (
      await db.select().from(users).where(eq(users.id, member.id)).limit(1)
    )[0];
    expect(
      await new Argon2id().verify(row.hashedPassword!, body.password),
    ).toBe(true);

    const remaining = await db
      .select()
      .from(sessions)
      .where(eq(sessions.userId, member.id));
    expect(remaining).toHaveLength(0);

    // Non-admins cannot reset anyone.
    await expect(
      resetPasswordRoute.POST({
        params: { id: admin.id },
        locals: sessionLocals(member),
        cookies: noCookies,
      } as never),
    ).rejects.toMatchObject({ status: 403 });
  });

  it("users change their password with the current one", async () => {
    const { Argon2id } = await import("oslo/password");
    const { eq } = await import("drizzle-orm");

    const initial = await new Argon2id().hash("old-password");
    await db
      .update(users)
      .set({ hashedPassword: initial })
      .where(eq(users.id, member.id));

    const cookieSet = vi.fn();
    const cookies = { set: cookieSet, get: () => undefined } as never;

    await expect(
      changePasswordRoute.POST({
        request: jsonRequest("/api/user/password", {
          currentPassword: "wrong",
          newPassword: "new-password",
        }),
        locals: sessionLocals(member),
        cookies,
      } as never),
    ).rejects.toMatchObject({ status: 400 });

    const ok = await changePasswordRoute.POST({
      request: jsonRequest("/api/user/password", {
        currentPassword: "old-password",
        newPassword: "new-password",
      }),
      locals: sessionLocals(member),
      cookies,
    } as never);
    expect(ok.status).toBe(200);
    expect(cookieSet).toHaveBeenCalled();

    const row = (
      await db.select().from(users).where(eq(users.id, member.id)).limit(1)
    )[0];
    expect(
      await new Argon2id().verify(row.hashedPassword!, "new-password"),
    ).toBe(true);
  });
});

describe("admin-service", () => {
  it("computes site stats", async () => {
    const stats = await adminService.getSiteStats();
    expect(stats.users).toBeGreaterThanOrEqual(2);
    expect(stats.seedsTotal).toBeGreaterThanOrEqual(1);
    expect(
      stats.officialPresets + stats.archivedOfficialPresets,
    ).toBeGreaterThanOrEqual(2);
  });

  it("lists and searches seeds with a race flag", async () => {
    await db.insert(seeds).values({
      id: "panel-open-seed1",
      options: { IncludeSpoiler: true },
      patchData: {},
      placementInfo: {},
    });

    const all = await adminService.listSeedsForAdmin({});
    const race = all.seeds.find((s) => s.id === "panel-race-seed1");
    const open = all.seeds.find((s) => s.id === "panel-open-seed1");
    expect(race?.raceMode).toBe(true);
    expect(open?.raceMode).toBe(false);

    const searched = await adminService.listSeedsForAdmin({
      query: "open-seed",
    });
    expect(searched.seeds.map((s) => s.id)).toEqual(["panel-open-seed1"]);
    expect(searched.total).toBe(1);
  });

  it("activates and deactivates versions exclusively per randomizer", async () => {
    const { randomizerVersions } = await import("$lib/server/db/schema");
    await db.insert(randomizerVersions).values([
      {
        id: "panel-v1",
        randomizerId: "combo",
        versionTag: "panel-1.0",
        optionsMetadata: {},
        postGenSettings: {},
        ipsBasePatchBase64: "",
        isActive: true,
      },
      {
        id: "panel-v2",
        randomizerId: "combo",
        versionTag: "panel-2.0",
        optionsMetadata: {},
        postGenSettings: {},
        ipsBasePatchBase64: "",
        isActive: false,
      },
    ]);

    expect(await adminService.setActiveVersion("panel-v2")).toBe(true);
    let versions = await adminService.listVersionsForAdmin();
    expect(versions.find((v) => v.id === "panel-v1")?.isActive).toBe(false);
    expect(versions.find((v) => v.id === "panel-v2")?.isActive).toBe(true);

    expect(await adminService.deactivateVersion("panel-v2")).toBe(true);
    versions = await adminService.listVersionsForAdmin();
    expect(versions.find((v) => v.id === "panel-v2")?.isActive).toBe(false);

    expect(await adminService.setActiveVersion("missing")).toBe(false);
    expect(await adminService.deactivateVersion("missing")).toBe(false);
  });
});
