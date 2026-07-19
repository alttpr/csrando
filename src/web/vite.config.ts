import { paraglideVitePlugin } from "@inlang/paraglide-js";
import tailwindcss from "@tailwindcss/vite";
import { sveltekit } from "@sveltejs/kit/vite";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [
    tailwindcss(),
    sveltekit(),
    paraglideVitePlugin({
      project: "./project.inlang",
      outdir: "./src/lib/paraglide",
    }),
  ],
  optimizeDeps: {
    // Pre-bundle deps that Vite otherwise discovers on the first page load.
    // Late discovery forces a mid-session re-optimization + reload, which can
    // leave the browser with two copies of Svelte's runtime (manifesting as
    // "lifecycle_outside_component" errors in dev).
    include: ["clsx", "devalue"],
  },
});
