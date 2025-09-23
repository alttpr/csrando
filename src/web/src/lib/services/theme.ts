import { browser } from "$app/environment";
import { writable, get } from "svelte/store";

export type Theme = "light" | "dark" | "system";

const THEME_KEY = "theme";
const DARK_MODE_QUERY = "(prefers-color-scheme: dark)";
const VALID_THEMES: Theme[] = ["light", "dark", "system"];

const isTheme = (value: unknown): value is Theme =>
  typeof value === "string" && VALID_THEMES.includes(value as Theme);

const prefersDark = () =>
  browser ? window.matchMedia(DARK_MODE_QUERY).matches : false;

const shouldUseDark = (value: Theme) =>
  value === "dark" || (value === "system" && prefersDark());

const toggleDarkClass = (enable: boolean) => {
  if (!browser) return;
  document.documentElement.classList.toggle("dark", enable);
};

const readStoredTheme = (): Theme | null => {
  if (!browser) return null;
  const stored = localStorage.getItem(THEME_KEY);
  return isTheme(stored) ? stored : null;
};

const getInitialTheme = (): Theme => {
  if (!browser) return "light";
  return readStoredTheme() ?? "system";
};

export const theme = writable<Theme>(getInitialTheme());

export function applyTheme(value: Theme) {
  toggleDarkClass(shouldUseDark(value));
}

let teardown: (() => void) | null = null;

export function initThemeService() {
  if (!browser || teardown) return;

  const mediaQuery = window.matchMedia(DARK_MODE_QUERY);
  const handleMediaChange = (event: MediaQueryListEvent) => {
    if (get(theme) === "system") {
      toggleDarkClass(event.matches);
    }
  };

  mediaQuery.addEventListener("change", handleMediaChange);

  const unsubscribe = theme.subscribe((value) => {
    localStorage.setItem(THEME_KEY, value);
    applyTheme(value);
  });

  teardown = () => {
    mediaQuery.removeEventListener("change", handleMediaChange);
    unsubscribe();
  };
}

export function destroyThemeService() {
  if (!teardown) return;
  teardown();
  teardown = null;
}

export function applyInitialTheme() {
  applyTheme(readStoredTheme() ?? "system");
}
