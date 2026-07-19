import type { PageServerLoad } from "./$types";
import { requirePanelAccess } from "$lib/server/admin/guard";
import {
  listOfficialPresetsForAdmin,
  listUserPresets,
} from "$lib/server/presets/service";

export const load: PageServerLoad = async ({ locals, cookies }) => {
  requirePanelAccess(locals, cookies);
  return {
    officials: await listOfficialPresetsForAdmin(),
    // The promotion picker offers the admin's own private presets (private
    // presets of other users are not readable, by design).
    myPresets: locals.user ? await listUserPresets(locals.user) : [],
  };
};
