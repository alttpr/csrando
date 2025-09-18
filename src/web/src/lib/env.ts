import { env as publicEnv } from "$env/dynamic/public";
import { browser } from "$app/environment";

// Resolve the public sprites base URL safely in all environments.
export function getPublicSpritesBaseUrl(): string {
  // Resolution priority (first non-empty wins):
  // 1. Runtime global injected by layout (window.__PUBLIC_SPRITES_BASE_URL__)
  // 2. Static build-time env ($env/static/public)
  // 3. Dynamic runtime env (SSR) ($env/dynamic/public)
  // 4. Vite import.meta.env (client bundle embed)
  // 5. Default '/sprites'
  const fromWindow = browser ? window.__PUBLIC_SPRITES_BASE_URL__ : undefined;
  // Only dynamic public env (server runtime). We intentionally avoid $env/static/public so
  // builds succeed even if the variable is only provided at deploy/runtime (e.g. Azure App Setting).
  const fromDynamic = publicEnv.PUBLIC_SPRITES_BASE_URL;
  // Avoid dynamic access to import.meta.env in SSR (causes Vite module runner error).
  // We already cover build-time via static import and runtime via dynamic/public + window global.
  const candidate = fromWindow || fromDynamic || "/sprites";
  return candidate.replace(/\/$/, "");
}
