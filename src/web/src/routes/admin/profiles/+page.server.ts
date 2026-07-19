import type { PageServerLoad } from "./$types";
import { requirePanelAccess } from "$lib/server/admin/guard";
import {
  listOfficialProfilesForAdmin,
  listUserProfiles,
} from "$lib/server/profiles/service";

export const load: PageServerLoad = async ({ locals, cookies }) => {
  requirePanelAccess(locals, cookies);
  return {
    officials: await listOfficialProfilesForAdmin(),
    // The promotion picker offers the admin's own private profiles (private
    // profiles of other users are not readable, by design).
    myProfiles: locals.user ? await listUserProfiles(locals.user) : [],
  };
};
