// vitest.config.ts
import { defineConfig } from "vitest/config";
import { svelte } from "@sveltejs/vite-plugin-svelte";
import { svelteTesting } from "@testing-library/svelte/vite";
import { fileURLToPath } from "node:url";

export default defineConfig({
  // The mixed dependency graph between vitest's bundled vite and project vite types can
  // cause an incompatible Plugin type error. Runtime is fine; suppress type noise.
  // @ts-expect-error – vite/vitest duplicate type versions
  plugins: [svelte(), svelteTesting()],
  resolve: {
    alias: {
      $lib: fileURLToPath(new URL("./src/lib", import.meta.url)),
      // Provide SvelteKit virtual module shims so Vite can resolve them during tests.
      // Tests often vi.mock these ids, but Vite still needs a resolve target.
      "$env/dynamic/private": fileURLToPath(
        new URL("./tests/vite-stubs/env-dynamic-private.ts", import.meta.url),
      ),
      "$app/environment": fileURLToPath(
        new URL("./tests/vite-stubs/app-environment.ts", import.meta.url),
      ),
    },
  },
  test: {
    globals: true,
    environment: "jsdom", // use jsdom by default so Svelte components can mount
    include: [
      "tools/sprite-importer/**/*.test.ts",
      "src/lib/schemas/**/*.test.ts",
      "src/lib/patch/**/*.test.ts",
      "tests/**/*.test.ts",
    ],
    // environmentMatchGlobs no longer required since default is jsdom
  },
});
