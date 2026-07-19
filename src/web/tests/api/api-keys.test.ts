import { beforeAll, beforeEach, describe, expect, it, vi } from "vitest";
import type { User } from "lucia";
import { apiKeys, users } from "$lib/server/db/schema";

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
type KeysRoute = typeof import("../../src/routes/api/user/api-keys/+server");
type KeyRoute =
  typeof import("../../src/routes/api/user/api-keys/[id]/+server");
type ApiKeysModule = typeof import("$lib/server/api-keys");

let db: DbModule["db"];
let keysRoute: KeysRoute;
let keyRoute: KeyRoute;
let apiKeysModule: ApiKeysModule;

const alice: User = {
  id: "key-alice-0000001",
  username: "key-alice",
  githubId: null,
  isAdmin: false,
};
const adminUser: User = {
  id: "key-admin-0000001",
  username: "key-admin",
  githubId: null,
  isAdmin: true,
};

function sessionLocals(user: User | null) {
  return { user, session: user ? { id: "session-1" } : null };
}

function apiKeyLocals(user: User) {
  return { user, session: null };
}

function jsonRequest(body: unknown): Request {
  return new Request("http://localhost/api/user/api-keys", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
  });
}

async function createKey(name: string, locals = sessionLocals(alice)) {
  const response = await keysRoute.POST({
    request: jsonRequest({ name }),
    locals,
  } as never);
  return await response.json();
}

beforeAll(async () => {
  process.env.DATABASE_URL = ":memory:";
  ({ db } = await import("$lib/server/db"));
  keysRoute = await import("../../src/routes/api/user/api-keys/+server");
  keyRoute = await import("../../src/routes/api/user/api-keys/[id]/+server");
  apiKeysModule = await import("$lib/server/api-keys");

  for (const user of [alice, adminUser]) {
    await db.insert(users).values({
      id: user.id,
      username: user.username,
      isAdmin: user.isAdmin,
    });
  }
});

beforeEach(async () => {
  await db.delete(apiKeys);
});

describe("API key management", () => {
  it("requires a browser session, not just any authentication", async () => {
    await expect(
      keysRoute.POST({
        request: jsonRequest({ name: "bot" }),
        locals: sessionLocals(null),
      } as never),
    ).rejects.toMatchObject({ status: 401 });

    // A user authenticated via an API key must not manage keys.
    await expect(
      keysRoute.POST({
        request: jsonRequest({ name: "bot" }),
        locals: apiKeyLocals(alice),
      } as never),
    ).rejects.toMatchObject({ status: 403 });
  });

  it("creates a key, returns the secret exactly once, and lists it", async () => {
    const created = await createKey("My bot");
    expect(created.secret).toMatch(/^qr_[0-9A-Za-z]{40}$/);
    expect(created.key.name).toBe("My bot");
    expect(created.secret.startsWith(created.key.tokenPrefix)).toBe(true);

    const list = await (
      await keysRoute.GET({ locals: sessionLocals(alice) } as never)
    ).json();
    expect(list.keys).toHaveLength(1);
    expect(list.keys[0].name).toBe("My bot");
    // The secret is never in the listing.
    expect(JSON.stringify(list)).not.toContain(created.secret);
  });

  it("enforces the per-user key limit", async () => {
    for (let i = 0; i < apiKeysModule.MAX_API_KEYS_PER_USER; i++) {
      await createKey(`key ${i}`);
    }
    await expect(createKey("one too many")).rejects.toMatchObject({
      status: 400,
    });
  });

  it("revokes a key and rejects revoking foreign or unknown keys", async () => {
    const created = await createKey("Doomed");

    await expect(
      keyRoute.DELETE({
        params: { id: created.key.id },
        locals: sessionLocals(adminUser),
      } as never),
    ).rejects.toMatchObject({ status: 404 });

    const response = await keyRoute.DELETE({
      params: { id: created.key.id },
      locals: sessionLocals(alice),
    } as never);
    expect((await response.json()).ok).toBe(true);

    await expect(
      keyRoute.DELETE({
        params: { id: created.key.id },
        locals: sessionLocals(alice),
      } as never),
    ).rejects.toMatchObject({ status: 404 });

    const list = await (
      await keysRoute.GET({ locals: sessionLocals(alice) } as never)
    ).json();
    expect(list.keys).toHaveLength(0);
  });
});

describe("authenticateApiKey", () => {
  it("resolves a valid Bearer secret to its user", async () => {
    const created = await createKey("Auth me");
    const user = await apiKeysModule.authenticateApiKey(
      `Bearer ${created.secret}`,
    );
    expect(user?.id).toBe(alice.id);
    expect(user?.username).toBe(alice.username);

    const list = await (
      await keysRoute.GET({ locals: sessionLocals(alice) } as never)
    ).json();
    expect(list.keys[0].lastUsedAt).not.toBeNull();
  });

  it("never grants admin through an API key", async () => {
    const created = await createKey("Admin bot", sessionLocals(adminUser));
    const user = await apiKeysModule.authenticateApiKey(
      `Bearer ${created.secret}`,
    );
    expect(user?.id).toBe(adminUser.id);
    expect(user?.isAdmin).toBe(false);
  });

  it("rejects invalid, malformed and revoked keys", async () => {
    expect(await apiKeysModule.authenticateApiKey(null)).toBeNull();
    expect(await apiKeysModule.authenticateApiKey("Bearer nope")).toBeNull();
    expect(
      await apiKeysModule.authenticateApiKey(`Bearer qr_${"x".repeat(40)}`),
    ).toBeNull();

    const created = await createKey("Revoked");
    await keyRoute.DELETE({
      params: { id: created.key.id },
      locals: sessionLocals(alice),
    } as never);
    expect(
      await apiKeysModule.authenticateApiKey(`Bearer ${created.secret}`),
    ).toBeNull();
  });
});
