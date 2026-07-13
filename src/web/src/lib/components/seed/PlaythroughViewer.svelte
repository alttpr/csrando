<script lang="ts">
  import type {
    PlaythroughData,
    PlaythroughItem,
    PlaythroughPathStep,
  } from "$lib/utils/playthrough";
  import {
    condensedPlaythroughPath,
    visiblePlaythroughPickups,
  } from "$lib/utils/playthrough";

  interface Props {
    playthrough: PlaythroughData;
  }
  let { playthrough }: Props = $props();
  let showAdvanced = $state(false);

  const itemName = (item: PlaythroughItem) =>
    `${item.name}${item.count > 1 ? ` ×${item.count}` : ""}`;
  const gameName = (game: string) =>
    ({ alttp: "ALttP", sm: "SM", m1: "Metroid", z1: "Zelda 1" })[game] ?? game;
  const edgeGame = (step: PlaythroughPathStep) => step.game ?? step.from.game;
  const isCrossGame = (step: PlaythroughPathStep) =>
    step.crossGame ?? step.from.game !== step.to.game;
  const stepRequirements = (step: PlaythroughPathStep) =>
    step.requirements?.length
      ? step.requirements
      : step.requirement
        ? [step.requirement]
        : [];
  const visibleStartingItems = (items: PlaythroughItem[]) =>
    showAdvanced ? items : items.filter((item) => item.meta !== true);
  const visibleRequirements = (items: PlaythroughItem[]) =>
    showAdvanced
      ? items
      : items.filter((item) => item.simple ?? item.initial !== true);
  const visiblePickups = (
    pickups: PlaythroughData["spheres"][number]["pickups"],
  ) => visiblePlaythroughPickups(playthrough, pickups, showAdvanced);
  const visibleSpheres = () =>
    playthrough.spheres
      .map((sphere) => ({ ...sphere, pickups: visiblePickups(sphere.pickups) }))
      .filter((sphere) => sphere.pickups.length > 0);
  const keyPath = (path: PlaythroughPathStep[]) =>
    condensedPlaythroughPath(path, showAdvanced);
  const resourceSpend = (step: PlaythroughPathStep) =>
    (step.resourcesSpent ?? [])
      .map((resource) => `${resource.amount} ${resource.name}`)
      .join(", ");
</script>

<details
  class="group/playthrough overflow-hidden rounded-lg border border-slate-200 dark:border-slate-700"
>
  <summary
    class="flex cursor-pointer list-none flex-wrap items-center justify-between gap-2 bg-slate-50 px-3 py-3 marker:hidden dark:bg-slate-900/40"
  >
    <div>
      <h4
        id="playthrough-title"
        class="text-sm font-semibold text-slate-800 dark:text-slate-100"
      >
        Logical playthrough
      </h4>
      <p class="text-xs text-slate-500 dark:text-slate-400">
        {visibleSpheres().length} progression spheres · goal: {playthrough.victoryItems.join(
          ", ",
        )}
      </p>
    </div>
    <div class="flex items-center gap-2">
      {#if !playthrough.complete}
        <span
          class="rounded-full bg-rose-100 px-2.5 py-1 text-xs font-medium text-rose-700 dark:bg-rose-500/20 dark:text-rose-200"
        >
          Incomplete
        </span>
      {/if}
      <span
        class="text-indigo-600 transition-transform group-open/playthrough:rotate-90 dark:text-indigo-300"
        >›</span
      >
    </div>
  </summary>

  <div class="space-y-3 border-t border-slate-200 p-3 dark:border-slate-700">
    {#if visibleStartingItems(playthrough.startingItems).length > 0}
      <div class="rounded-md bg-slate-50 p-3 text-xs dark:bg-slate-900/40">
        <span class="font-semibold text-slate-600 dark:text-slate-300"
          >Starting inventory:</span
        >
        <span class="ml-1 text-slate-700 dark:text-slate-200">
          {visibleStartingItems(playthrough.startingItems)
            .map(itemName)
            .join(", ")}
        </span>
      </div>
    {/if}

    {#each playthrough.warnings as warning (warning)}
      <p
        class="rounded-md bg-amber-50 px-3 py-2 text-xs text-amber-800 dark:bg-amber-500/10 dark:text-amber-200"
      >
        {warning}
      </p>
    {/each}

    <label
      class="flex cursor-pointer items-center gap-2 rounded-md bg-slate-50 px-3 py-2 text-xs text-slate-600 dark:bg-slate-900/40 dark:text-slate-300"
    >
      <input
        type="checkbox"
        bind:checked={showAdvanced}
        class="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800"
      />
      Show logic events, flags, and full routes
    </label>

    <div class="space-y-3">
      {#each visibleSpheres() as sphere, visibleIndex (sphere.number)}
        <section
          class="overflow-hidden rounded-lg border border-slate-200 dark:border-slate-700"
        >
          <header
            class="bg-indigo-50 px-3 py-2 text-xs font-semibold text-indigo-800 dark:bg-indigo-500/10 dark:text-indigo-200"
          >
            Sphere {showAdvanced ? sphere.number + 1 : visibleIndex + 1}
          </header>
          <div class="divide-y divide-slate-200 dark:divide-slate-700">
            {#each sphere.pickups as pickup (pickup)}
              <details class="group/pickup px-3 py-2.5">
                <summary class="cursor-pointer list-none text-xs marker:hidden">
                  <span class="font-bold text-slate-800 dark:text-slate-100"
                    >{itemName(pickup.item)}</span
                  >
                  <span
                    class="ml-1 rounded bg-slate-200 px-1 py-0.5 text-[0.65rem] font-semibold uppercase text-slate-600 dark:bg-slate-700 dark:text-slate-200"
                    >{gameName(pickup.item.game)}</span
                  >
                  <span class="mx-1 text-slate-500 dark:text-slate-400">at</span
                  >
                  <span class="text-slate-500 dark:text-slate-400"
                    >{pickup.location.name}</span
                  >
                  <span
                    class="ml-1 rounded bg-slate-200 px-1 py-0.5 text-[0.65rem] font-semibold uppercase text-slate-600 dark:bg-slate-700 dark:text-slate-200"
                    >{gameName(pickup.location.game)}</span
                  >
                  <span
                    class="float-right text-indigo-600 group-open/pickup:rotate-90 dark:text-indigo-300"
                    >›</span
                  >
                </summary>
                <div
                  class="mt-3 space-y-3 border-l-2 border-indigo-200 pl-3 text-xs dark:border-indigo-700"
                >
                  {#if showAdvanced}
                    <div>
                      <span
                        class="font-semibold text-slate-600 dark:text-slate-300"
                        >Required from start:</span
                      >
                      <span class="ml-1 text-slate-700 dark:text-slate-200">
                        {visibleRequirements(pickup.requiredItems).length
                          ? visibleRequirements(pickup.requiredItems)
                              .map(itemName)
                              .join(", ")
                          : "Nothing"}
                      </span>
                    </div>
                  {/if}
                  <div>
                    <ol class="space-y-1.5 text-slate-600 dark:text-slate-300">
                      <li>
                        <span class="font-medium">Start:</span>
                        <span
                          class="mx-1 rounded bg-slate-200 px-1 py-0.5 text-[0.65rem] font-semibold uppercase text-slate-600 dark:bg-slate-700 dark:text-slate-200"
                          >{gameName(
                            pickup.path[0]?.from.game ?? pickup.location.game,
                          )}</span
                        >
                        {pickup.path[0]?.from.name ?? pickup.location.name}
                      </li>
                      {#each keyPath(pickup.path) as step, index (index)}
                        <li
                          class={`grid grid-cols-[1.25rem_1fr] gap-1 rounded px-1 py-0.5 ${
                            isCrossGame(step)
                              ? "bg-cyan-50 text-cyan-900 ring-1 ring-cyan-200 dark:bg-cyan-500/10 dark:text-cyan-100 dark:ring-cyan-700"
                              : ""
                          }`}
                        >
                          <span class="text-right text-slate-400"
                            >{index + 1}.</span
                          >
                          <span>
                            <span
                              class="mr-1 rounded bg-slate-200 px-1 py-0.5 text-[0.65rem] font-semibold uppercase text-slate-600 dark:bg-slate-700 dark:text-slate-200"
                              >{gameName(edgeGame(step))}</span
                            >
                            {#if isCrossGame(step)}<span class="font-semibold"
                                >Travel from {step.from.name} to {gameName(
                                  step.to.game,
                                )}</span
                              >{/if}
                            {#if visibleRequirements(stepRequirements(step)).length}<span
                                class="font-medium text-amber-700 dark:text-amber-300"
                                >{isCrossGame(step) ? " · " : ""}Use {visibleRequirements(
                                  stepRequirements(step),
                                )
                                  .map(itemName)
                                  .join(", ")}</span
                              >{/if}
                            {#if visibleRequirements(stepRequirements(step)).length && step.strategy && showAdvanced}
                              ·
                            {/if}
                            {#if step.resourcesSpent?.length}<span
                                class="font-medium text-rose-700 dark:text-rose-300"
                              >
                                · Spend {resourceSpend(step)}</span
                              >{/if}
                            {#if step.strategy && showAdvanced}<span
                                class="font-medium text-violet-700 dark:text-violet-300"
                              >
                                · {step.strategy}</span
                              >{/if}
                            <span> → {step.to.name}</span>
                          </span>
                        </li>
                      {/each}
                      <li>
                        <span class="font-medium">Finish:</span>
                        <span
                          class="mx-1 rounded bg-slate-200 px-1 py-0.5 text-[0.65rem] font-semibold uppercase text-slate-600 dark:bg-slate-700 dark:text-slate-200"
                          >{gameName(pickup.location.game)}</span
                        >
                        {pickup.location.name}
                      </li>
                    </ol>
                  </div>
                  {#if showAdvanced}
                    <details>
                      <summary
                        class="cursor-pointer font-semibold text-indigo-700 dark:text-indigo-300"
                      >
                        Full route ({pickup.path.length} edges)
                      </summary>
                      <ol
                        class="mt-2 space-y-1.5 text-slate-600 dark:text-slate-300"
                      >
                        {#each pickup.path as step, index (index)}
                          <li
                            class={`grid grid-cols-[1.25rem_1fr] gap-1 rounded px-1 py-0.5 ${
                              isCrossGame(step)
                                ? "bg-cyan-50 text-cyan-900 ring-1 ring-cyan-200 dark:bg-cyan-500/10 dark:text-cyan-100 dark:ring-cyan-700"
                                : ""
                            }`}
                          >
                            <span class="text-right text-slate-400"
                              >{index + 1}.</span
                            >
                            <span>
                              <span
                                class="mr-1 rounded bg-slate-200 px-1 py-0.5 text-[0.65rem] font-semibold uppercase text-slate-600 dark:bg-slate-700 dark:text-slate-200"
                                >{gameName(edgeGame(step))}</span
                              >
                              {#if isCrossGame(step)}<span class="font-semibold"
                                  >Travel {gameName(step.from.game)} → {gameName(
                                    step.to.game,
                                  )} ·</span
                                >{/if}
                              {step.from.name} → {step.to.name}
                              {#if stepRequirements(step).length}<span
                                  class="text-amber-700 dark:text-amber-300"
                                >
                                  · {stepRequirements(step)
                                    .map(itemName)
                                    .join(", ")}</span
                                >{/if}
                              {#if step.strategy}<span
                                  class="text-violet-700 dark:text-violet-300"
                                >
                                  · {step.strategy}</span
                                >{/if}
                              {#if step.resourcesSpent?.length}<span
                                  class="text-rose-700 dark:text-rose-300"
                                >
                                  · Spend {resourceSpend(step)}</span
                                >{/if}
                            </span>
                          </li>
                        {/each}
                      </ol>
                    </details>
                  {/if}
                </div>
              </details>
            {/each}
          </div>
        </section>
      {/each}
    </div>
  </div>
</details>
