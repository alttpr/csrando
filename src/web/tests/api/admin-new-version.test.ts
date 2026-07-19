import {
  beforeAll,
  beforeEach,
  afterEach,
  describe,
  expect,
  it,
  vi,
} from "vitest";
import type { RequestHandler } from "@sveltejs/kit";
import { createHash } from "crypto";
import { randomizerVersions } from "$lib/server/db/schema";

type DbModule = typeof import("$lib/server/db");

const resolveCanonicalIdMock = vi.fn(async (id: string) => id);
const getByIdMock = vi.fn(
  async (id: string) =>
    ({
      id,
      name: id,
      postGenSettings: {},
    }) as unknown,
);

vi.mock("$env/dynamic/private", () => ({
  env: new Proxy(
    {},
    {
      get: (_target, key: string) => process.env[key] ?? undefined,
    },
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
    resolveCanonicalId: resolveCanonicalIdMock,
    getById: getByIdMock,
  },
}));

let POST: RequestHandler;
let db: DbModule["db"];

const TEST_TOKEN = "test-secret-token";
const adminLocals = {
  user: {
    id: "version-admin",
    username: "version-admin",
    githubId: null,
    isAdmin: true,
  },
  session: { id: "admin-session" },
};

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  process.env.PRIVATE_ADMIN_VERSION_TOKEN = TEST_TOKEN;

  ({ db } = await import("$lib/server/db"));
  ({ POST } = await import("../../src/routes/api/admin/new-version/+server"));
});

beforeEach(() => {
  vi.clearAllMocks();
  process.env.PRIVATE_ADMIN_VERSION_TOKEN = TEST_TOKEN;
});

afterEach(async () => {
  await db.delete(randomizerVersions);
});

describe("POST /api/admin/new-version", () => {
  const url = "http://localhost/api/admin/new-version";

  it("requires an authenticated browser session", async () => {
    const request = new Request(url, { method: "POST" });

    await expect(
      POST({
        request,
        locals: { user: null, session: null },
      } as never),
    ).rejects.toMatchObject({
      status: 401,
    });
  });

  it("does not accept the legacy token from a non-admin session", async () => {
    const request = new Request(url, {
      method: "POST",
      headers: {
        "content-type": "application/json",
        authorization: "Bearer wrong-token",
      },
      body: JSON.stringify({
        baseVersion: "v1",
        randomizerId: "test",
        basePatchBase64: Buffer.from("patch").toString("base64"),
      }),
    });

    await expect(
      POST({
        request,
        locals: {
          user: { ...adminLocals.user, isAdmin: false },
          session: adminLocals.session,
        },
      } as never),
    ).rejects.toMatchObject({ status: 403 });
  });

  it("validates required fields", async () => {
    const request = new Request(url, {
      method: "POST",
      headers: {
        "content-type": "application/json",
        authorization: `Bearer ${TEST_TOKEN}`,
      },
      body: JSON.stringify({
        baseVersion: "",
        randomizerId: "",
        basePatchBase64: "",
      }),
    });

    await expect(
      POST({ request, locals: adminLocals } as never),
    ).rejects.toMatchObject({
      status: 400,
      body: {
        message: "Fix the highlighted errors and try again.",
        fieldErrors: {
          baseVersion: expect.any(String),
          randomizerId: expect.any(String),
          basePatch: expect.any(String),
        },
      },
    });
  });

  it("rejects invalid base64 payloads", async () => {
    const request = new Request(url, {
      method: "POST",
      headers: {
        "content-type": "application/json",
        authorization: `Bearer ${TEST_TOKEN}`,
      },
      body: JSON.stringify({
        baseVersion: "v1",
        randomizerId: "test",
        basePatchBase64: "%",
      }),
    });

    await expect(
      POST({ request, locals: adminLocals } as never),
    ).rejects.toMatchObject({
      status: 400,
      body: {
        message: "basePatchBase64 must be a base64 encoded string.",
      },
    });
  });

  it("creates a new randomizer version when payload is valid", async () => {
    const patchBuffer = Buffer.from("patch-bytes");
    const patchBase64 = patchBuffer.toString("base64");

    resolveCanonicalIdMock.mockResolvedValueOnce("CanonicalMeta");
    getByIdMock.mockResolvedValueOnce({
      id: "CanonicalMeta",
      name: "CanonicalMeta",
      postGenSettings: { test: { options: [] } },
    } as unknown);

    const request = new Request(url, {
      method: "POST",
      headers: {
        "content-type": "application/json",
        authorization: `Bearer ${TEST_TOKEN}`,
      },
      body: JSON.stringify({
        baseVersion: "v1",
        randomizerId: "Test",
        metadataId: "TestMeta",
        tag: null,
        activate: true,
        basePatchBase64: patchBase64,
      }),
    });

    const response = await POST({ request, locals: adminLocals } as never);
    expect(response.status).toBe(200);

    const payload = await response.json();
    expect(payload).toMatchObject({
      versionTag: "v1-test",
      randomizerId: "test",
      metadataId: "TestMeta",
      canonicalMetadataId: "CanonicalMeta",
      activated: true,
    });
    expect(payload.patchSha256).toBe(
      createHash("sha256").update(patchBuffer).digest("hex"),
    );

    expect(resolveCanonicalIdMock).toHaveBeenCalledWith("TestMeta");
    expect(getByIdMock).toHaveBeenCalledWith("CanonicalMeta");

    const rows = await db.select().from(randomizerVersions);
    expect(rows).toHaveLength(1);
    expect(rows[0].versionTag).toBe("v1-test");
    expect(rows[0].randomizerId).toBe("test");
    expect(Boolean(rows[0].isActive)).toBe(true);
  });
});
