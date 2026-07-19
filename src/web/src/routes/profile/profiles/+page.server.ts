import { redirect } from "@sveltejs/kit";
import type { User } from "lucia";
import { getPreferences, listUserProfiles } from "$lib/server/profiles/service";
import type {
  ProfilePreferencesDto,
  ProfileSummaryDto,
} from "$lib/schemas/profiles";

interface LoadEvent {
  locals: { user: User | null };
}

interface LoadResult {
  profiles: ProfileSummaryDto[];
  preferences: ProfilePreferencesDto;
}

export const load = async ({ locals }: LoadEvent): Promise<LoadResult> => {
  if (!locals.user) {
    throw redirect(302, "/login");
  }
  return {
    profiles: await listUserProfiles(locals.user),
    preferences: await getPreferences(locals.user),
  };
};
