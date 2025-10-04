import { writable, get } from "svelte/store";

type Theme = "light" | "dark" | "system";

const THEME_KEY = "theme";

function getInitialTheme(): Theme {
  if (typeof window !== "undefined") {
    const storedTheme = localStorage.getItem(THEME_KEY) as Theme | null;
    if (storedTheme && ["light", "dark", "system"].includes(storedTheme)) {
      return storedTheme;
    }
    return "system";
  }
  return "light";
}

export const theme = writable<Theme>(getInitialTheme());

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

  window
    .matchMedia("(prefers-color-scheme: dark)")
    .addEventListener("change", (e) => {
      const current = get(theme);
      if (current === "system") {
        document.documentElement.classList.toggle("dark", e.matches);
      }
    });
}
