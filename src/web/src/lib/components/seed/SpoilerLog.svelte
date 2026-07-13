<script lang="ts">
  import DungeonMapViewer from "./DungeonMapViewer.svelte";
  import M1MapViewer from "./M1MapViewer.svelte";
  import PlaythroughViewer from "./PlaythroughViewer.svelte";
  import { parsePlaythrough } from "$lib/utils/playthrough";

  interface Props {
    spoilerLog:
      | Record<string, Record<string, string>>
      | null
      | undefined
      | unknown;
  }

  let { spoilerLog }: Props = $props();

  let isExpanded = $state(false);
  const panelId = "seed-spoiler-log-panel";
  let searchQuery = $state("");

  function toggleExpanded() {
    isExpanded = !isExpanded;
  }

  function handleSearchInput(event: Event) {
    const target = event.currentTarget as HTMLInputElement | null;
    if (!target) return;
    searchQuery = target.value;
  }

  // Type guard to check if spoilerLog is valid
  function isValidSpoilerLog(
    log: unknown,
  ): log is Record<string, Record<string, string>> {
    if (!log || typeof log !== "object") return false;
    return Object.values(log).every(
      (section) => section && typeof section === "object",
    );
  }

  const validSpoilerLog = $derived(
    isValidSpoilerLog(spoilerLog) ? spoilerLog : null,
  );

  // Extract dungeon map data if present
  const dungeonMaps = $derived(() => {
    if (!validSpoilerLog) return [];
    const mapSection = validSpoilerLog["z1DungeonMaps"];
    if (!mapSection?.data) return [];
    try {
      return JSON.parse(mapSection.data) as Array<{
        level: number;
        width: number;
        height: number;
        rooms: Array<{
          x: number;
          y: number;
          roles: string[];
          doors: Record<string, string>;
          enemy: string | null;
          enemyCount: number;
          segment: number;
          screen: string;
          connectedTo: number[][] | null;
        }>;
        itemPlacements: Record<string, string>;
      }>;
    } catch {
      return [];
    }
  });

  // Extract the M1 generated map if present
  const m1Map = $derived(() => {
    if (!validSpoilerLog) return null;
    const mapSection = validSpoilerLog["m1Map"];
    if (!mapSection?.data) return null;
    try {
      return JSON.parse(mapSection.data) as {
        cells: Array<{
          x: number;
          y: number;
          area: string;
          role?: string | null;
          screen: string;
          edges: Record<string, string>;
        }>;
        itemPlacements: Record<string, string>;
        landmarks: Record<string, string>;
      };
    } catch {
      return null;
    }
  });

  const playthrough = $derived(() =>
    parsePlaythrough(validSpoilerLog?.["playthrough"]?.data),
  );

  // Special section keys that are rendered separately (not as key-value pairs)
  const specialSections = new Set(["z1DungeonMaps", "m1Map", "playthrough"]);

  // Sort sections to put "meta" at the end and keep the rest alphabetically sorted
  const sortedSections = $derived(() => {
    if (!validSpoilerLog) return [];

    const entries = Object.entries(validSpoilerLog).filter(
      ([key]) => !specialSections.has(key),
    );
    const metaSection = entries.find(([key]) => key === "meta");
    const otherSections = entries
      .filter(([key]) => key !== "meta")
      .sort(([a], [b]) => a.localeCompare(b));

    return metaSection ? [...otherSections, metaSection] : otherSections;
  });

  const normalizedSearch = $derived(() => searchQuery.trim().toLowerCase());

  const filteredSections = $derived(() => {
    const sections = sortedSections();
    const query = normalizedSearch();

    if (!query) return sections;

    return sections.reduce(
      (acc, [sectionName, sectionData]) => {
        const filteredEntries = Object.entries(sectionData).filter(
          ([location, item]) =>
            [sectionName, location, item]
              .map((text) => text.toLowerCase())
              .some((text) => text.includes(query)),
        );

        if (filteredEntries.length > 0) {
          acc.push([sectionName, Object.fromEntries(filteredEntries)]);
        }

        return acc;
      },
      [] as Array<[string, Record<string, string>]>,
    );
  });

  // Helper to format keys into more readable names
  function formatKey(key: string): string {
    // Convert camelCase/PascalCase to Title Case with spaces
    return key
      .replace(/([A-Z])/g, " $1")
      .replace(/^./, (str) => str.toUpperCase())
      .trim();
  }

  // Helper to format location/item names
  function formatValue(value: string): string {
    // Add spaces before capital letters when followed by lowercase letters and convert to Title Case
    return value
      .replace(/([A-Z])(?=[a-z])/g, " $1")
      .replace(/^./, (str) => str.toUpperCase())
      .trim();
  }
</script>

{#if validSpoilerLog && Object.keys(validSpoilerLog).length > 0}
  <section class="mt-6">
    <div
      class="rounded-lg border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-800 sm:p-5"
    >
      <header
        class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between"
      >
        <div class="flex items-start gap-3">
          <span
            class="inline-flex h-9 w-9 items-center justify-center rounded-md bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-200"
          >
            <svg
              class="h-5 w-5"
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden="true"
            >
              <path
                fill-rule="evenodd"
                d="M8.257 3.099c.765-1.36 2.72-1.36 3.485 0l6.516 11.59c.75 1.334-.213 3.01-1.742 3.01H3.483c-1.53 0-2.493-1.676-1.743-3.01l6.517-11.59ZM11 13a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm-1-8a1 1 0 0 0-1 1v3a1 1 0 1 0 2 0V6a1 1 0 0 0-1-1Z"
                clip-rule="evenodd"
              />
            </svg>
          </span>
          <div class="space-y-1">
            <h3
              class="text-base font-semibold text-slate-900 dark:text-slate-100"
            >
              Spoiler log
            </h3>
            <p class="text-xs text-slate-500 dark:text-slate-400">
              This reveals item placements and other secrets for the seed.
            </p>
          </div>
        </div>
        <button
          type="button"
          class="inline-flex items-center justify-center gap-2 self-start rounded-md border border-slate-200 bg-white/80 px-3 py-1.5 text-xs font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/60 dark:border-slate-600 dark:bg-slate-900/60 dark:text-slate-300 dark:hover:bg-slate-800 dark:hover:text-slate-100"
          onclick={toggleExpanded}
          aria-expanded={isExpanded}
          aria-controls={panelId}
        >
          <span>{isExpanded ? "Hide details" : "Show spoiler log"}</span>
          <svg
            class="h-3.5 w-3.5 transition-transform"
            viewBox="0 0 20 20"
            fill="currentColor"
            aria-hidden="true"
            class:rotate-90={isExpanded}
          >
            <path
              fill-rule="evenodd"
              d="M7.293 14.707a1 1 0 0 1 0-1.414L10.586 10 7.293 6.707a1 1 0 0 1 1.414-1.414l4 4a1 1 0 0 1 0 1.414l-4 4a1 1 0 0 1-1.414 0Z"
              clip-rule="evenodd"
            />
          </svg>
        </button>
      </header>
      <div
        id={panelId}
        class="mt-5 space-y-6"
        class:hidden={!isExpanded}
        aria-hidden={!isExpanded}
      >
        {#if dungeonMaps().length > 0}
          <DungeonMapViewer dungeons={dungeonMaps()} />
        {/if}

        {#if m1Map()}
          <M1MapViewer map={m1Map()!} />
        {/if}

        {#if playthrough()}
          <PlaythroughViewer playthrough={playthrough()!} />
        {/if}

        <div
          class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"
        >
          <div class="relative w-full sm:max-w-xs">
            <label class="sr-only" for="spoiler-log-filter">
              Search spoiler log
            </label>
            <input
              id="spoiler-log-filter"
              type="search"
              class="w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-xs text-slate-700 shadow-sm outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/50 dark:border-slate-600 dark:bg-slate-900/60 dark:text-slate-200"
              placeholder="Filter locations or items"
              value={searchQuery}
              oninput={handleSearchInput}
            />
          </div>
        </div>

        {#if normalizedSearch() && filteredSections().length === 0}
          <p
            class="rounded-md bg-slate-50 px-3 py-2 text-xs text-slate-500 dark:bg-slate-800/60 dark:text-slate-300"
          >
            No results found for "{searchQuery.trim()}".
          </p>
        {/if}

        {#each filteredSections() as [sectionName, sectionData] (sectionName)}
          <section
            class="border-t border-slate-200 pt-5 first:border-t-0 first:pt-0 dark:border-slate-700"
          >
            <header class="mb-3 flex items-center gap-3">
              <span class="inline-flex h-5 w-1 rounded bg-indigo-500"></span>
              <h4
                class="text-[12px] font-semibold uppercase tracking-wide text-slate-600 dark:text-slate-300"
              >
                {formatKey(sectionName)}
              </h4>
            </header>
            <div class="grid grid-cols-1 gap-2 md:grid-cols-2">
              {#each Object.entries(sectionData) as [location, item] (location)}
                <div
                  class="flex items-baseline gap-2 rounded-md bg-slate-50 px-3 py-2 text-xs ring-1 ring-slate-200/70 transition hover:ring-slate-300 dark:bg-slate-800/60 dark:ring-slate-600/60 dark:hover:ring-slate-500"
                >
                  <span
                    class="mr-1.5 font-medium text-slate-600 dark:text-slate-300"
                  >
                    {formatKey(location)}:
                  </span>
                  <span
                    class="break-words text-slate-800 dark:text-slate-200 font-bold"
                  >
                    {formatValue(item)}
                  </span>
                </div>
              {/each}
            </div>
          </section>
        {/each}
      </div>
    </div>
  </section>
{/if}
