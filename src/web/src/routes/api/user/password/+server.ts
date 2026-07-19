import { error, json, type RequestHandler } from "@sveltejs/kit";
import { z } from "zod";
import { Argon2id } from "oslo/password";
import { eq } from "drizzle-orm";
import { db } from "$lib/server/db";
import { users } from "$lib/server/db/schema";
import { lucia } from "$lib/server/auth";
import { requireSessionUser } from "$lib/server/auth-guards";

const ChangePasswordSchema = z.object({
  currentPassword: z.string().optional(),
  newPassword: z.string().min(6).max(255),
});

// Change the logged-in user's password. The current password is required
// whenever one is set (GitHub-only accounts can set their first password
// without it). All other sessions are invalidated; the current one is
// re-issued so the user stays logged in.
export const POST: RequestHandler = async ({ request, locals, cookies }) => {
  const user = requireSessionUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw error(400, { message: "Request body must be valid JSON." });
  }
  const parsed = ChangePasswordSchema.safeParse(body);
  if (!parsed.success) {
    throw error(400, {
      message: "Password must be at least 6 characters long.",
    });
  }

  const rows = await db
    .select({ hashedPassword: users.hashedPassword })
    .from(users)
    .where(eq(users.id, user.id))
    .limit(1);
  if (!rows[0]) {
    throw error(404, { message: "User not found." });
  }

  if (rows[0].hashedPassword) {
    const current = parsed.data.currentPassword ?? "";
    const valid =
      current.length > 0 &&
      (await new Argon2id().verify(rows[0].hashedPassword, current));
    if (!valid) {
      throw error(400, { message: "The current password is incorrect." });
    }
  }

  const hashedPassword = await new Argon2id().hash(parsed.data.newPassword);
  await db.update(users).set({ hashedPassword }).where(eq(users.id, user.id));

  await lucia.invalidateUserSessions(user.id);
  const session = await lucia.createSession(user.id, {});
  const sessionCookie = lucia.createSessionCookie(session.id);
  cookies.set(sessionCookie.name, sessionCookie.value, {
    path: "/",
    ...sessionCookie.attributes,
  });

  return json({ ok: true });
};
