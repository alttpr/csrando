// Ambient global typings for browser runtime injected values
// This file ensures the window property is recognized in runes mode Svelte components.
export {};

declare global {
  interface Window {
    __PUBLIC_SPRITES_BASE_URL__?: string;
  }
}
