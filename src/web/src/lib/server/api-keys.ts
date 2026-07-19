import { createHash } from "crypto";
import { and, eq, isNull } from "drizzle-orm";
import { customAlphabet } from "nanoid";
import type { User } from "lucia";
import { db } from "$lib/server/db";
import { apiKeys, users } from "$lib/server/db/schema";
import { generateId } from "$lib/utils/id";

// Personal API keys for external tools (bots). Secrets look like
// "qr_<40 chars>"; only their SHA-256 hash is persisted.

export const API_KEY_PREFIX = "qr_";
export const MAX_API_KEYS_PER_USER = 10;
export const MAX_API_KEY_NAME_LENGTH = 60;

const SECRET_ALPHABET =
  "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
const generateSecretBody = customAlphabet(SECRET_ALPHABET, 40);

export function hashApiKey(secret: string): string {
  return createHash("sha256").update(secret).digest("hex");
}

export interface ApiKeySummary {
  id: string;
  name: string;
  tokenPrefix: string;
  createdAt: Date | null;
  lastUsedAt: Date | null;
}

function toSummary(row: typeof apiKeys.$inferSelect): ApiKeySummary {
  return {
    id: row.id,
    name: row.name,
    tokenPrefix: row.tokenPrefix,
    createdAt: row.createdAt,
    lastUsedAt: row.lastUsedAt,
  };
}

export async function listApiKeys(userId: string): Promise<ApiKeySummary[]> {
  const rows = await db
    .select()
    .from(apiKeys)
    .where(and(eq(apiKeys.userId, userId), isNull(apiKeys.revokedAt)));
  return rows.map(toSummary);
}

// Creates a key and returns the plain secret exactly once.
export async function createApiKey(
  userId: string,
  name: string,
): Promise<{ key: ApiKeySummary; secret: string }> {
  const secret = `${API_KEY_PREFIX}${generateSecretBody()}`;
  const now = new Date();
  const row: typeof apiKeys.$inferInsert = {
    id: generateId(),
    userId,
    name,
    tokenHash: hashApiKey(secret),
    tokenPrefix: secret.slice(0, API_KEY_PREFIX.length + 6),
    createdAt: now,
  };
  await db.insert(apiKeys).values(row);
  return {
    key: toSummary({
      ...row,
      lastUsedAt: null,
      revokedAt: null,
      createdAt: now,
    }),
    secret,
  };
}

export async function revokeApiKey(
  userId: string,
  keyId: string,
): Promise<boolean> {
  const result = await db
    .update(apiKeys)
    .set({ revokedAt: new Date() })
    .where(
      and(
        eq(apiKeys.id, keyId),
        eq(apiKeys.userId, userId),
        isNull(apiKeys.revokedAt),
      ),
    );
  return result.changes > 0;
}

export async function countApiKeys(userId: string): Promise<number> {
  return (await listApiKeys(userId)).length;
}

// Resolve a Bearer secret to its owning user. Returns a Lucia-shaped user so
// downstream authorization works unchanged. isAdmin is deliberately forced to
// false: API keys never grant official-preset administration.
export async function authenticateApiKey(
  authorizationHeader: string | null,
): Promise<User | null> {
  if (!authorizationHeader?.startsWith("Bearer ")) return null;
  const secret = authorizationHeader.slice("Bearer ".length).trim();
  if (!secret.startsWith(API_KEY_PREFIX)) return null;

  const rows = await db
    .select({ key: apiKeys, user: users })
    .from(apiKeys)
    .innerJoin(users, eq(apiKeys.userId, users.id))
    .where(
      and(eq(apiKeys.tokenHash, hashApiKey(secret)), isNull(apiKeys.revokedAt)),
    )
    .limit(1);
  const row = rows[0];
  if (!row) return null;

  // Best-effort usage bookkeeping.
  try {
    await db
      .update(apiKeys)
      .set({ lastUsedAt: new Date() })
      .where(eq(apiKeys.id, row.key.id));
  } catch {
    // ignore
  }

  return {
    id: row.user.id,
    username: row.user.username,
    githubId: row.user.githubId,
    isAdmin: false,
  } as User;
}
