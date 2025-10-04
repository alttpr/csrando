import { deLocalizeUrl } from "$lib/paraglide/runtime";
import { initThemeService } from "$lib/services/theme";

if (typeof window !== "undefined") {
  initThemeService();
}

export const reroute = (request: { url: string | URL }) =>
  deLocalizeUrl(request.url).pathname;

export const transport = {};
