interface LinkedProfile {
  profileId: string | null;
  slug: string | null;
  configId: string;
}

export function profileConfigPath(profile: LinkedProfile): string | null {
  if (!profile.profileId) return null;
  const reference = profile.slug ?? profile.profileId;
  return `/config/${encodeURIComponent(profile.configId)}/${encodeURIComponent(reference)}`;
}

export function seedSettingsPath(input: {
  seedId: string;
  settingsConfigId: string | null;
  profile: LinkedProfile | null;
  differedFromRevision: boolean | null;
}): string | null {
  if (!input.settingsConfigId) return null;
  if (!input.differedFromRevision && input.profile?.profileId) {
    return profileConfigPath(input.profile);
  }
  return `/config/${input.settingsConfigId}?fromSeed=${encodeURIComponent(input.seedId)}`;
}
