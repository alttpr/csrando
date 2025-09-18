import { writable, get } from "svelte/store";

export type Theme = "light" | "dark" | "system";

const THEME_KEY = "theme";
const DARK_MODE_QUERY = "(prefers-color-scheme: dark)";
const VALID_THEMES: Theme[] = ["light", "dark", "system"];
const isBrowser = typeof window !== "undefined";

const isTheme = (value: unknown): value is Theme =>
  typeof value === "string" && VALID_THEMES.includes(value as Theme);

const prefersDark = () =>
  isBrowser ? window.matchMedia(DARK_MODE_QUERY).matches : false;

const shouldUseDark = (value: Theme) =>
  value === "dark" || (value === "system" && prefersDark());

const toggleDarkClass = (enable: boolean) => {
  if (!isBrowser) return;
  document.documentElement.classList.toggle("dark", enable);
};

const readStoredTheme = (): Theme | null => {
  if (!isBrowser) return null;
  const stored = localStorage.getItem(THEME_KEY);
  return isTheme(stored) ? stored : null;
};

const getInitialTheme = (): Theme => {
  if (!isBrowser) return "light";
  return readStoredTheme() ?? "system";
};

export const theme = writable<Theme>(getInitialTheme());

export function applyTheme(value: Theme) {
  toggleDarkClass(shouldUseDark(value));
}

export function setupSystemThemeListener() {
  if (!isBrowser) return;
  const mediaQuery = window.matchMedia(DARK_MODE_QUERY);
  const applyIfSystem = (isDark: boolean) => {
    if (get(theme) === "system") {
      toggleDarkClass(isDark);
    }
  };

  mediaQuery.addEventListener("change", (event) => {
    applyIfSystem(event.matches);
  });
}

export function initThemeService() {
  if (!isBrowser) return;

  theme.subscribe((value) => {
    localStorage.setItem(THEME_KEY, value);
    applyTheme(value);
  });

  setupSystemThemeListener();
}

export function applyInitialTheme() {
  applyTheme(readStoredTheme() ?? "system");
}
