import { redirect } from "@sveltejs/kit";
import type { User } from "lucia";
import { getPreferences, listUserPresets } from "$lib/server/presets/service";
import type {
  PresetPreferencesDto,
  PresetSummaryDto,
} from "$lib/schemas/presets";

interface LoadEvent {
  locals: { user: User | null };
}

interface LoadResult {
  presets: PresetSummaryDto[];
  preferences: PresetPreferencesDto;
}

export const load = async ({ locals }: LoadEvent): Promise<LoadResult> => {
  if (!locals.user) {
    throw redirect(302, "/login");
  }
  return {
    presets: await listUserPresets(locals.user),
    preferences: await getPreferences(locals.user),
  };
};
