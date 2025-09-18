// Minimal stub for SvelteKit's $env/dynamic/private during Vitest runs.
// Vite needs to resolve the module id even if tests mock it via vi.mock.
export const env: Record<string, string | undefined> = new Proxy(
  {},
  {
    get: (_t, key: string) =>
      typeof process !== "undefined" ? process.env[key] : undefined,
  },
);
