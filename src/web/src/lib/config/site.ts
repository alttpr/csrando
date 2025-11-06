export const SITE_NAME = "Alttp & Quad Randomizer";

export type SiteMode = "all" | "alttpr" | "combo";

const HOSTNAME_SITE_MODE: Record<string, SiteMode> = {
  "alttpr.samus.link": "alttpr",
  "quad.samus.link": "combo",
};

export function resolveSiteMode(hostname?: string | null): SiteMode {
  if (!hostname) return "all";
  const normalized = hostname.toLowerCase();
  return HOSTNAME_SITE_MODE[normalized] ?? "all";
}

export type SiteFeatures = {
  showAlttpr: boolean;
  showCombo: boolean;
};

export function siteModeFeatures(mode: SiteMode): SiteFeatures {
  return {
    showAlttpr: mode === "all" || mode === "alttpr",
    showCombo: mode === "all" || mode === "combo",
  };
}

const SITE_NAME_BY_MODE: Record<SiteMode, string> = {
  all: SITE_NAME,
  alttpr: "ALttP Randomizer",
  combo: "Combo Randomizer",
};

export function siteNameForMode(mode: SiteMode): string {
  return SITE_NAME_BY_MODE[mode] ?? SITE_NAME;
}
