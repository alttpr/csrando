import { error } from "@sveltejs/kit";
import type { User } from "lucia";

// Throws the API-idiomatic errors when the request lacks the required
// authentication level. Use inside +server.ts handlers and server loads.
export function requireUser(locals: App.Locals): User {
  if (!locals.user) {
    throw error(401, { message: "Unauthorized" });
  }
  return locals.user;
}

export function requireAdmin(locals: App.Locals): User {
  const user = requireUser(locals);
  if (!user.isAdmin) {
    throw error(403, { message: "Administrator access required" });
  }
  return user;
}

// Requires a browser session (cookie login). API-key requests carry a user
// but no session; sensitive account operations (like managing API keys)
// must not be reachable with a key alone.
export function requireSessionUser(locals: App.Locals): User {
  const user = requireUser(locals);
  if (!locals.session) {
    throw error(403, {
      message: "This operation requires a browser login session",
    });
  }
  return user;
}
