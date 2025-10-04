import { redirect, fail } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { db } from "$lib/server/db";
import { users, sessions, userSeeds } from "$lib/server/db/schema";
import { eq } from "drizzle-orm";
import { lucia } from "$lib/server/auth";

export const load: PageServerLoad = async ({ locals }) => {
  if (!locals.user) {
    throw redirect(302, "/login");
  }
  return {};
};

export const actions: Actions = {
  deleteAccount: async ({ locals, request, cookies }) => {
    if (!locals.user) {
      throw redirect(302, "/login");
    }

    const form = await request.formData();
    const confirm = (form.get("confirm") || "").toString().trim();
    if (confirm !== "DELETE") {
      return fail(400, { error: true, message: "Type DELETE to confirm." });
    }

    // Defensive cleanup of references to avoid FK violations.
    // 1) Ensure sessions are removed (in addition to lucia invalidation)
    await lucia.invalidateUserSessions(locals.user.id);
    await db.delete(sessions).where(eq(sessions.userId, locals.user.id));
    // 2) Remove user_seed links (should cascade, but explicit for SQLite robustness)
    await db.delete(userSeeds).where(eq(userSeeds.userId, locals.user.id));
    // 3) Delete the user
    await db.delete(users).where(eq(users.id, locals.user.id));

    // Clear the session cookie
    const blank = lucia.createBlankSessionCookie();
    cookies.set(blank.name, blank.value, { path: "/", ...blank.attributes });

    throw redirect(303, "/");
  },
};
