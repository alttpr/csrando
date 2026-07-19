import type { LayoutServerLoad } from "./$types";
import { isPanelAuthorized } from "$lib/server/admin/guard";

// Individual pages enforce the admin-session requirement; this feeds the
// shared sub-navigation without exposing any legacy-token authorization.
export const load: LayoutServerLoad = async ({ locals, cookies }) => {
  return {
    panelAuthorized: isPanelAuthorized(locals, cookies),
  };
};
