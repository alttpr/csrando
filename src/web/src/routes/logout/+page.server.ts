import { redirect } from "@sveltejs/kit";
import { lucia } from "$lib/server/auth";
import type { Actions, PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ locals }) => {
  if (!locals.user) {
    throw redirect(302, "/login");
  }
  return {};
};

export const actions: Actions = {
  default: async ({ locals, cookies }) => {
    if (!locals.session) {
      throw redirect(302, "/login");
    }
    await lucia.invalidateSession(locals.session.id);
    const sessionCookie = lucia.createBlankSessionCookie();
    cookies.set(sessionCookie.name, sessionCookie.value, {
      path: "/",
      ...sessionCookie.attributes,
    });
    throw redirect(302, "/login");
  },
};
