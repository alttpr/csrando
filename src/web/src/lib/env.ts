import { env as publicEnv } from "$env/dynamic/public";
import { browser } from "$app/environment";

/** Resolve the public sprite base URL from runtime hints or fall back to `/sprites`. */
export function getPublicSpritesBaseUrl(): string {
  const fromWindow = browser ? window.__PUBLIC_SPRITES_BASE_URL__ : undefined;
  const fromDynamic = publicEnv.PUBLIC_SPRITES_BASE_URL;
  const candidate = fromWindow || fromDynamic || "/sprites";
  return candidate.replace(/\/$/, "");
}
