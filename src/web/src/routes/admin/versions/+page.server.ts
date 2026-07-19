import { fail } from "@sveltejs/kit";
import type { Actions, PageServerLoad } from "./$types";
import { isPanelAuthorized, requirePanelAccess } from "$lib/server/admin/guard";
import {
  deactivateVersion,
  listVersionsForAdmin,
  setActiveVersion,
} from "$lib/server/admin/admin-service";

export const load: PageServerLoad = async ({ locals, cookies }) => {
  requirePanelAccess(locals, cookies);
  return { versions: await listVersionsForAdmin() };
};

export const actions: Actions = {
  activate: async ({ request, locals, cookies }) => {
    if (!isPanelAuthorized(locals, cookies)) {
      return fail(403, { message: "Administrator access required" });
    }
    const versionId = (await request.formData()).get("versionId");
    if (typeof versionId !== "string" || !versionId) {
      return fail(400, { message: "versionId is required" });
    }
    if (!(await setActiveVersion(versionId))) {
      return fail(404, { message: "Version not found" });
    }
    return { success: true };
  },

  deactivate: async ({ request, locals, cookies }) => {
    if (!isPanelAuthorized(locals, cookies)) {
      return fail(403, { message: "Administrator access required" });
    }
    const versionId = (await request.formData()).get("versionId");
    if (typeof versionId !== "string" || !versionId) {
      return fail(400, { message: "versionId is required" });
    }
    if (!(await deactivateVersion(versionId))) {
      return fail(404, { message: "Version not found" });
    }
    return { success: true };
  },
};
