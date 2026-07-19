import { redirect, fail } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { db } from "$lib/server/db";
import {
  apiKeys,
  configurationPresets,
  users,
  sessions,
  userSeeds,
} from "$lib/server/db/schema";
import { and, eq, isNull, sql } from "drizzle-orm";
import { lucia } from "$lib/server/auth";

export const load: PageServerLoad = async ({ locals }) => {
  if (!locals.user) {
    throw redirect(302, "/login");
  }
  const count = sql<number>`count(*)`;
  const [seedRows, presetRows, keyRows] = await Promise.all([
    db
      .select({ count })
      .from(userSeeds)
      .where(eq(userSeeds.userId, locals.user.id)),
    db
      .select({ count })
      .from(configurationPresets)
      .where(
        and(
          eq(configurationPresets.ownerUserId, locals.user.id),
          isNull(configurationPresets.deletedAt),
        ),
      ),
    db
      .select({ count })
      .from(apiKeys)
      .where(
        and(eq(apiKeys.userId, locals.user.id), isNull(apiKeys.revokedAt)),
      ),
  ]);
  return {
    account: {
      username: locals.user.username,
      loginMethod: locals.user.githubId === null ? "Password" : "GitHub",
      isAdmin: Boolean(locals.user.isAdmin),
    },
    stats: {
      seeds: seedRows[0]?.count ?? 0,
      presets: presetRows[0]?.count ?? 0,
      apiKeys: keyRows[0]?.count ?? 0,
    },
  };
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
