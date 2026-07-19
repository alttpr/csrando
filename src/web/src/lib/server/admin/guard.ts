import { error, redirect, type Cookies } from "@sveltejs/kit";

// Interactive administration always requires an authenticated admin session.
// The legacy server token is restricted to the one-time first-admin bootstrap
// endpoint and must never become a general-purpose panel credential.
export function isPanelAuthorized(
  locals: App.Locals,
  cookies?: Cookies,
): boolean {
  void cookies;
  return !!(locals.user?.isAdmin && locals.session);
}

export function requirePanelAccess(locals: App.Locals, cookies: Cookies): void {
  if (isPanelAuthorized(locals, cookies)) return;
  if (!locals.user) {
    throw redirect(302, "/login");
  }
  throw error(403, { message: "Administrator access required" });
}
