import type { PageServerLoad } from "./$types";
import { requirePanelAccess } from "$lib/server/admin/guard";
import { listUsersForAdmin } from "$lib/server/admin/admin-service";

export const load: PageServerLoad = async ({ locals, cookies, url }) => {
  requirePanelAccess(locals, cookies);
  const query = url.searchParams.get("q") ?? "";
  const { users, total } = await listUsersForAdmin(query);
  return {
    users,
    total,
    query,
    selfUserId: locals.user?.id ?? null,
  };
};
