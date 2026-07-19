import { error, json, type RequestHandler } from "@sveltejs/kit";
import { eq, sql } from "drizzle-orm";
import { z } from "zod";
import { db } from "$lib/server/db";
import { users } from "$lib/server/db/schema";
import {
  getConfiguredAdminTokenHash,
  isTokenAuthorized,
} from "$lib/server/admin/version-service";

// Grant (or revoke) the per-user administrator flag. Accepts a logged-in
// admin session (the admin panel), or — for the "first admin" bootstrap —
// the server-level admin token:
//
//   curl -X POST /api/admin/promote \
//     -H "Authorization: Bearer $PRIVATE_ADMIN_VERSION_TOKEN" \
//     -H "Content-Type: application/json" \
//     -d '{"username": "somebody"}'

const PromoteRequestSchema = z.object({
  username: z.string().trim().min(1),
  isAdmin: z.boolean().optional().default(true),
});

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

export const POST: RequestHandler = async ({ request, locals }) => {
  // A session admin may manage roles. The raw server token has exactly one
  // purpose: atomically create the first administrator when none exists.
  const sessionAdmin = !!(locals?.user?.isAdmin && locals?.session);
  let tokenBootstrap = false;
  if (!sessionAdmin) {
    if (!getConfiguredAdminTokenHash()) {
      throw error(500, {
        message: "Admin token is not configured on the server.",
      });
    }
    if (!isTokenAuthorized(extractAdminToken(request))) {
      throw error(401, { message: "Invalid admin access token." });
    }
    tokenBootstrap = true;
  }

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw error(400, { message: "Request body must be valid JSON." });
  }

  const parsed = PromoteRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw error(400, { message: "username is required." });
  }

  const rows = await db
    .select({ id: users.id, username: users.username, isAdmin: users.isAdmin })
    .from(users)
    .where(
      sql`lower(${users.username}) = ${parsed.data.username.toLowerCase()}`,
    )
    .limit(1);
  const user = rows[0];
  if (!user) {
    throw error(404, { message: "User not found." });
  }

  if (tokenBootstrap) {
    if (!parsed.data.isAdmin) {
      throw error(403, {
        message: "The bootstrap token can only create the first administrator.",
      });
    }

    let promoted = false;
    db.transaction((tx) => {
      const row = tx
        .select({ count: sql<number>`count(*)` })
        .from(users)
        .where(eq(users.isAdmin, true))
        .get();
      if ((row?.count ?? 0) > 0) return;
      tx.update(users)
        .set({ isAdmin: true })
        .where(eq(users.id, user.id))
        .run();
      promoted = true;
    });
    if (!promoted) {
      throw error(409, {
        message:
          "Administrator bootstrap is already complete. Sign in as an administrator to manage roles.",
      });
    }
  }

  // A session admin cannot demote themselves — prevents locking everyone out
  // by accident.
  if (sessionAdmin && user.id === locals!.user!.id && !parsed.data.isAdmin) {
    throw error(400, {
      message: "You cannot remove your own administrator access.",
    });
  }

  if (!tokenBootstrap) {
    await db
      .update(users)
      .set({ isAdmin: parsed.data.isAdmin })
      .where(eq(users.id, user.id));
  }

  return json({
    ok: true,
    user: {
      id: user.id,
      username: user.username,
      isAdmin: parsed.data.isAdmin,
    },
  });
};
