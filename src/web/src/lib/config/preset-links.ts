interface LinkedPreset {
  presetId: string | null;
  slug: string | null;
  configId: string;
}

export function presetConfigPath(preset: LinkedPreset): string | null {
  if (!preset.presetId) return null;
  const reference = preset.slug ?? preset.presetId;
  return `/config/${encodeURIComponent(preset.configId)}/${encodeURIComponent(reference)}`;
}

export function seedSettingsPath(input: {
  seedId: string;
  settingsConfigId: string | null;
  preset: LinkedPreset | null;
  differedFromRevision: boolean | null;
}): string | null {
  if (!input.settingsConfigId) return null;
  if (!input.differedFromRevision && input.preset?.presetId) {
    return presetConfigPath(input.preset);
  }
  return `/config/${input.settingsConfigId}?fromSeed=${encodeURIComponent(input.seedId)}`;
}
