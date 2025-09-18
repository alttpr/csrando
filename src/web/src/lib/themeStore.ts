import { writable, get } from "svelte/store";

type Theme = "light" | "dark" | "system";

const THEME_KEY = "theme";

// Determine initial theme from localStorage or system preference
function getInitialTheme(): Theme {
  if (typeof window !== "undefined") {
    const storedTheme = localStorage.getItem(THEME_KEY) as Theme | null;
    // 1. If a valid theme is stored, use it.
    if (storedTheme && ["light", "dark", "system"].includes(storedTheme)) {
      return storedTheme;
    }
    // 2. If no valid theme is stored, default to 'system'
    return "system";
  }
  // Default to 'light' during SSR
  return "light";
}

// Create a writable store for the theme
export const theme = writable<Theme>(getInitialTheme());

// Persist and apply theme changes (including system follow mode)
if (typeof window !== "undefined") {
  theme.subscribe((value) => {
    localStorage.setItem(THEME_KEY, value);
    const root = document.documentElement;
    if (value === "system") {
      const systemPrefersDark = window.matchMedia(
        "(prefers-color-scheme: dark)",
      ).matches;
      if (systemPrefersDark) {
        root.classList.add("dark");
      } else {
        root.classList.remove("dark");
      }
    } else if (value === "dark") {
      root.classList.add("dark");
    } else {
      root.classList.remove("dark");
    }
  });

  // Listen for system theme changes if 'system' is selected
  window
    .matchMedia("(prefers-color-scheme: dark)")
    .addEventListener("change", (e) => {
      const current = get(theme);
      if (current === "system") {
        document.documentElement.classList.toggle("dark", e.matches);
      }
    });
}
