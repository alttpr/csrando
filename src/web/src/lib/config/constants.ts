// Shared limits for seed presets, enforced server-side and mirrored client-side
// for early feedback. Server enforcement is authoritative.
export const CONFIG_SCHEMA_VERSION = 1;
export const MAX_PRESET_NAME_LENGTH = 60;
export const MAX_PRESET_DESCRIPTION_LENGTH = 240;
export const MAX_USER_PRESETS = 100;
export const MAX_SETTINGS_JSON_BYTES = 65536;
export const MAX_RECENT_PRESETS = 5;
