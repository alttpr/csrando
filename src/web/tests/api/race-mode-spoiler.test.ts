import {
  afterEach,
  beforeAll,
  beforeEach,
  describe,
  expect,
  it,
  vi,
} from "vitest";
import { createHash } from "crypto";
import { seeds } from "$lib/server/db/schema";
import { ADMIN_VERSION_COOKIE_NAME } from "$lib/server/admin/version-service";

type DbModule = typeof import("$lib/server/db");

const { createRandomizerMock, getActiveRandomizerVersionForMock } = vi.hoisted(
  () => ({
    createRandomizerMock: vi.fn(),
    getActiveRandomizerVersionForMock: vi.fn(),
  }),
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
  randomizeApi: { create: createRandomizerMock },
}));

vi.mock("$lib/server/db/randomizer", () => ({
  getActiveRandomizerVersionFor: getActiveRandomizerVersionForMock,
}));

let publicSeedGet: typeof import("../../src/routes/api/seed/[id]/+server").GET;
let adminSpoilerGet: typeof import("../../src/routes/api/admin/seed/[id]/spoiler/+server").GET;
let randomizePost: typeof import("../../src/routes/api/randomize/+server").POST;
let db: DbModule["db"];

const TEST_TOKEN = "test-secret-token";
const seedId = "race-seed";
const apiSeedId = "api-hide-spoiler-seed";
const spoilerLog = { world: { location: "item" } };

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  process.env.PRIVATE_ADMIN_VERSION_TOKEN = TEST_TOKEN;

  ({ db } = await import("$lib/server/db"));
  ({ GET: publicSeedGet } = await import(
    "../../src/routes/api/seed/[id]/+server"
  ));
  ({ GET: adminSpoilerGet } = await import(
    "../../src/routes/api/admin/seed/[id]/spoiler/+server"
  ));
  ({ POST: randomizePost } = await import(
    "../../src/routes/api/randomize/+server"
  ));
});

beforeEach(async () => {
  vi.clearAllMocks();
  createRandomizerMock.mockResolvedValue({
    seed: 123456,
    worlds: { "0": { ipsPatch: "patch" } },
    spoilerLog,
  });
  getActiveRandomizerVersionForMock.mockResolvedValue(null);

  await db.insert(seeds).values([
    {
      id: seedId,
      options: {
        Seed: 0,
        IncludeSpoiler: false,
        Configs: [{}],
      },
      patchData: "patch",
      placementInfo: [],
      spoilerLog,
      createdAt: new Date(),
    },
    {
      id: apiSeedId,
      options: {
        Seed: 0,
        IncludeSpoiler: false,
        Configs: [{}],
      },
      patchData: "patch",
      placementInfo: [],
      spoilerLog,
      createdAt: new Date(),
    },
  ]);
});

afterEach(async () => {
  await db.delete(seeds);
});

describe("race mode spoilers", () => {
  it("omits the spoiler log from public seed responses when IncludeSpoiler is false", async () => {
    const response = await publicSeedGet({ params: { id: seedId } } as never);

    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toMatchObject({
      id: seedId,
      spoilerLog: null,
    });
  });

  it("omits a spoiler when an external caller set IncludeSpoiler to false", async () => {
    const response = await publicSeedGet({
      params: { id: apiSeedId },
    } as never);

    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toMatchObject({
      id: apiSeedId,
      spoilerLog: null,
    });
  });

  it("stores an external caller's hidden spoiler without returning it", async () => {
    const config = { Language: "en", Game: "Alttpr", Alttp: {} };
    const request = new Request("http://localhost/api/randomize", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({
        Seed: 0,
        IncludeSpoiler: false,
        Configs: [config],
      }),
    });

    const response = await randomizePost({
      request,
      locals: { user: null },
    } as never);
    const payload = await response.json();
    const storedSeed = (await db.select().from(seeds)).find(
      (seed) => seed.id === payload.id,
    );

    expect(response.status).toBe(200);
    expect(payload).not.toHaveProperty("spoilerLog");
    expect(createRandomizerMock).toHaveBeenCalledWith({
      Seed: 0,
      IncludeSpoiler: true,
      Configs: [config],
    });
    expect(storedSeed).toMatchObject({
      options: expect.objectContaining({
        IncludeSpoiler: false,
      }),
      spoilerLog,
    });
  });

  it("requires admin authentication before returning the spoiler log", async () => {
    await expect(
      adminSpoilerGet({
        params: { id: seedId },
        cookies: { get: () => undefined },
      } as never),
    ).rejects.toMatchObject({
      status: 401,
      body: { message: "Admin authentication is required." },
    });
  });

  it("returns the stored spoiler log to an authenticated admin", async () => {
    const tokenHash = createHash("sha256").update(TEST_TOKEN).digest("hex");
    const response = await adminSpoilerGet({
      params: { id: seedId },
      cookies: {
        get: (name: string) =>
          name === ADMIN_VERSION_COOKIE_NAME ? tokenHash : undefined,
      },
    } as never);

    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toEqual({ id: seedId, spoilerLog });
  });
});
