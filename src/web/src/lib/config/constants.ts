// Shared limits for seed profiles, enforced server-side and mirrored client-side
// for early feedback. Server enforcement is authoritative.
export const CONFIG_SCHEMA_VERSION = 1;
export const MAX_PROFILE_NAME_LENGTH = 60;
export const MAX_PROFILE_DESCRIPTION_LENGTH = 240;
export const MAX_USER_PROFILES = 100;
export const MAX_SETTINGS_JSON_BYTES = 65536;
export const MAX_RECENT_PROFILES = 5;
