import type { PageServerLoad } from "./$types";
import { requirePanelAccess } from "$lib/server/admin/guard";
import {
  ADMIN_SEEDS_PAGE_SIZE,
  listSeedsForAdmin,
} from "$lib/server/admin/admin-service";

export const load: PageServerLoad = async ({ locals, cookies, url }) => {
  requirePanelAccess(locals, cookies);
  const query = url.searchParams.get("q") ?? "";
  const page = Math.max(0, Number(url.searchParams.get("page") ?? "0") || 0);
  const result = await listSeedsForAdmin({ query, page });
  return {
    ...result,
    query,
    pageSize: ADMIN_SEEDS_PAGE_SIZE,
  };
};
