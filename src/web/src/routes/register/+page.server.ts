import { redirect, fail } from "@sveltejs/kit";
import { lucia } from "$lib/server/auth";
import { Argon2id } from "oslo/password";
import { db } from "$lib/server/db";
import { users } from "$lib/server/db/schema";
import { nanoid } from "nanoid";
import type { Actions } from "./$types";
import * as m from "$lib/paraglide/messages";

export const actions: Actions = {
  default: async ({ request, cookies }) => {
    const formData = await request.formData();
    const username = formData.get("username") as string;
    const password = formData.get("password") as string;
    const confirmPassword = formData.get("confirmPassword") as string;

    if (!username || typeof username !== "string" || username.length < 3) {
      return fail(400, {
        error: true,
        message: m.form_error_required({ field: m.register_username() }),
        username,
      });
    }
    if (!password || typeof password !== "string" || password.length < 6) {
      return fail(400, {
        error: true,
        message: "Password must be at least 6 characters long.",
        username,
      });
    }
    if (password !== confirmPassword) {
      return fail(400, {
        error: true,
        message: m.form_error_password_mismatch(),
        username,
      });
    }

    const hashedPassword = await new Argon2id().hash(password);
    const userId = nanoid(15);

    try {
      await db.insert(users).values({
        id: userId,
        username: username,
        hashedPassword: hashedPassword,
      });
    } catch (e: unknown) {
      if (
        e instanceof Error &&
        e.message?.includes("UNIQUE constraint failed: user.username")
      ) {
        return fail(400, {
          error: true,
          message: m.register_error_username_taken(),
          username,
        });
      }
      console.error("Registration error:", e);
      return fail(500, {
        error: true,
        message: m.register_error_generic(),
        username,
      });
    }

    const session = await lucia.createSession(userId, {});
    const sessionCookie = lucia.createSessionCookie(session.id);
    cookies.set(sessionCookie.name, sessionCookie.value, {
      path: "/",
      ...sessionCookie.attributes,
    });

    throw redirect(302, "/profile");
  },
};
