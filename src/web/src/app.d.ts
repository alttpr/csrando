// See https://kit.svelte.dev/docs/types#app
// for information about these interfaces
declare global {
  namespace App {
    interface Error {
      message?: string;
      fieldErrors?: Record<string, string>;
    }
    interface Locals {
      user: import("lucia").User | null;
      session: import("lucia").Session | null;
    }
    // interface PageData {}
    // interface Platform {}
  }
}

export {};
