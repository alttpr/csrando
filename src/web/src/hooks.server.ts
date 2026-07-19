import { lucia } from "$lib/server/auth";
import { sequence } from "@sveltejs/kit/hooks";
import { building } from "$app/environment";
import { ensureOfficialPresetsSeeded } from "$lib/server/presets/seed-official";
import { authenticateApiKey } from "$lib/server/api-keys";

// Seed missing official presets at boot (idempotent; retried lazily from the
// presets API when the generator metadata is not available yet).
if (!building) {
  ensureOfficialPresetsSeeded().catch((err) => {
    console.error("[presets] official preset seeding failed:", err);
  });
}

export const handle = sequence(async ({ event, resolve }) => {
  const sessionId = event.cookies.get(lucia.sessionCookieName);
  if (!sessionId) {
    // External tools can authenticate API routes with a personal API key
    // (Authorization: Bearer qr_...). Key-authenticated requests have
    // locals.session === null, which endpoints use to restrict
    // session-only operations (e.g. managing API keys).
    event.locals.user = event.url.pathname.startsWith("/api/")
      ? await authenticateApiKey(event.request.headers.get("authorization"))
      : null;
    event.locals.session = null;
    return resolve(event);
  }

  const { session, user } = await lucia.validateSession(sessionId);
  if (session && session.fresh) {
    const sessionCookie = lucia.createSessionCookie(session.id);
    event.cookies.set(sessionCookie.name, sessionCookie.value, {
      path: "/",
      ...sessionCookie.attributes,
    });
  }
  if (!session) {
    const sessionCookie = lucia.createBlankSessionCookie();
    event.cookies.set(sessionCookie.name, sessionCookie.value, {
      path: "/",
      ...sessionCookie.attributes,
    });
  }
  event.locals.user = user;
  event.locals.session = session;

  return resolve(event);
});
