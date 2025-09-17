import { redirect, fail } from "@sveltejs/kit";
import { lucia } from "$lib/server/auth";
import { Argon2id } from "oslo/password";
import { db } from "$lib/server/db";
import { users } from "$lib/server/db/schema";
import { eq } from "drizzle-orm";
import type { Actions, PageServerLoad } from "./$types";
import * as m from "$lib/paraglide/messages";

export const load: PageServerLoad = async ({ locals }) => {
  if (locals.user) {
    throw redirect(302, "/profile");
  }
  return {};
};

export const actions: Actions = {
  default: async ({ request, cookies }) => {
    const formData = await request.formData();
    const username = formData.get("username") as string;
    const password = formData.get("password") as string;

    if (
      !username ||
      typeof username !== "string" ||
      !password ||
      typeof password !== "string"
    ) {
      return fail(400, { error: true, message: m.login_error(), username });
    }

    const existingUser = await db
      .select()
      .from(users)
      .where(eq(users.username, username))
      .limit(1);
    if (existingUser.length === 0) {
      return fail(400, { error: true, message: m.login_error(), username });
    }

    const user = existingUser[0];
    if (!user.hashedPassword) {
      // Should not happen with new registration logic
      return fail(400, { error: true, message: m.login_error(), username });
    }

    const validPassword = await new Argon2id().verify(
      user.hashedPassword,
      password,
    );
    if (!validPassword) {
      return fail(400, { error: true, message: m.login_error(), username });
    }

    const session = await lucia.createSession(user.id, {});
    const sessionCookie = lucia.createSessionCookie(session.id);
    cookies.set(sessionCookie.name, sessionCookie.value, {
      path: "/",
      ...sessionCookie.attributes,
    });

    throw redirect(302, "/profile");
  },
};
