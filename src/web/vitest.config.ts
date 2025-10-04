// vitest.config.ts
import { defineConfig } from "vitest/config";
import { svelte } from "@sveltejs/vite-plugin-svelte";
import { fileURLToPath } from "node:url";

export default defineConfig({
  // The mixed dependency graph between vitest's bundled vite and project vite types can
  // cause an incompatible Plugin type error. Runtime is fine; suppress type noise.
  // @ts-expect-error – vite/vitest duplicate type versions
  plugins: [
    // Force DOM compile so component tests don't import server runtime
    svelte({
      // @ts-expect-error this option still compiles the client build for component tests
      compilerOptions: { generate: "dom" },
    }),
  ],
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
      // Force client runtime instead of server version when running in jsdom/node tests
      "svelte/src/index-server.js": "svelte/src/index.js",
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
