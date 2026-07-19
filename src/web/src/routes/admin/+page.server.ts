import type { PageServerLoad } from "./$types";
import { requirePanelAccess } from "$lib/server/admin/guard";
import { getSiteStats } from "$lib/server/admin/admin-service";

export const load: PageServerLoad = async ({ locals, cookies }) => {
  requirePanelAccess(locals, cookies);
  return { stats: await getSiteStats() };
};
