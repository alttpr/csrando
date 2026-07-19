import { beforeAll, beforeEach, describe, expect, it, vi } from "vitest";
import { eq } from "drizzle-orm";
import { users } from "$lib/server/db/schema";

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

type DbModule = typeof import("$lib/server/db");
type PromoteRoute = typeof import("../../src/routes/api/admin/promote/+server");

let db: DbModule["db"];
let promote: PromoteRoute;

const TEST_TOKEN = "promote-test-token";
const url = "http://localhost/api/admin/promote";

function makeRequest(body: unknown, token?: string): Request {
  const headers: Record<string, string> = {
    "content-type": "application/json",
  };
  if (token) headers.authorization = `Bearer ${token}`;
  return new Request(url, {
    method: "POST",
    headers,
    body: JSON.stringify(body),
  });
}

function noCookies() {
  return { get: () => undefined } as never;
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  process.env.PRIVATE_ADMIN_VERSION_TOKEN = TEST_TOKEN;
  ({ db } = await import("$lib/server/db"));
  promote = await import("../../src/routes/api/admin/promote/+server");

  await db.insert(users).values({
    id: "promote-user-0001",
    username: "PromoteMe",
    isAdmin: false,
  });
});

beforeEach(async () => {
  process.env.PRIVATE_ADMIN_VERSION_TOKEN = TEST_TOKEN;
  await db.update(users).set({ isAdmin: false });
});

describe("POST /api/admin/promote", () => {
  it("rejects requests without a valid admin token", async () => {
    await expect(
      promote.POST({
        request: makeRequest({ username: "PromoteMe" }, "wrong-token"),
        cookies: noCookies(),
      } as never),
    ).rejects.toMatchObject({ status: 401 });

    await expect(
      promote.POST({
        request: makeRequest({ username: "PromoteMe" }),
        cookies: noCookies(),
      } as never),
    ).rejects.toMatchObject({ status: 401 });
  });

  it("does not accept the former admin cookie", async () => {
    await expect(
      promote.POST({
        request: makeRequest({ username: "PromoteMe" }),
        cookies: { get: () => "legacy-cookie-value" } as never,
      } as never),
    ).rejects.toMatchObject({ status: 401 });
  });

  it("bootstraps the first admin case-insensitively and cannot be reused", async () => {
    const response = await promote.POST({
      request: makeRequest({ username: "promoteme" }, TEST_TOKEN),
      cookies: noCookies(),
    } as never);
    const body = await response.json();
    expect(body.user.isAdmin).toBe(true);

    const rows = await db
      .select()
      .from(users)
      .where(eq(users.id, "promote-user-0001"));
    expect(rows[0].isAdmin).toBe(true);

    await expect(
      promote.POST({
        request: makeRequest({ username: "PromoteMe" }, TEST_TOKEN),
        cookies: noCookies(),
      } as never),
    ).rejects.toMatchObject({ status: 409 });

    await expect(
      promote.POST({
        request: makeRequest(
          { username: "PromoteMe", isAdmin: false },
          TEST_TOKEN,
        ),
        cookies: noCookies(),
      } as never),
    ).rejects.toMatchObject({ status: 403 });
  });

  it("returns 404 for unknown users", async () => {
    await expect(
      promote.POST({
        request: makeRequest({ username: "nobody" }, TEST_TOKEN),
        cookies: noCookies(),
      } as never),
    ).rejects.toMatchObject({ status: 404 });
  });

  it("returns 500 when no admin token is configured", async () => {
    delete process.env.PRIVATE_ADMIN_VERSION_TOKEN;
    await expect(
      promote.POST({
        request: makeRequest({ username: "PromoteMe" }, TEST_TOKEN),
        cookies: noCookies(),
      } as never),
    ).rejects.toMatchObject({ status: 500 });
  });
});
