import type { LayoutServerLoad } from "./$types";
import { env as publicEnv } from "$env/dynamic/public";

export const load: LayoutServerLoad = async ({ locals }) => {
  const spritesBase = (publicEnv.PUBLIC_SPRITES_BASE_URL || "").replace(
    /\/$/,
    "",
  );
  return {
    user: locals.user,
    spritesBaseUrl: spritesBase || null,
  };
};
