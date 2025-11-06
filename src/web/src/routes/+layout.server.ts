import type { LayoutServerLoad } from "./$types";
import { env as publicEnv } from "$env/dynamic/public";
import { resolveSiteMode, siteModeFeatures } from "$lib/config/site";

export const load: LayoutServerLoad = async ({ locals, url }) => {
  const spritesBase = (publicEnv.PUBLIC_SPRITES_BASE_URL || "").replace(
    /\/$/,
    "",
  );

  const siteMode = resolveSiteMode(url.hostname);

  return {
    user: locals.user,
    spritesBaseUrl: spritesBase || null,
    siteMode,
    siteFeatures: siteModeFeatures(siteMode),
  };
};
