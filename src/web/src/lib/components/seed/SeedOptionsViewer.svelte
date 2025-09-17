<script lang="ts">
	import type { Metadata, MetadataSetting } from '$lib/types';

	interface Props {
		options: Array<{ id: string; options: Record<string, unknown> }>;
		globalOptions?: Record<string, unknown>;
		metadata?: Metadata | null;
		visibility?: Array<string>;
	}

	const { options, globalOptions = {}, metadata = null, visibility = [] }: Props = $props();

	import Card from '../ui/Card.svelte';

	// Feature flag: hide global settings in seed view for now
	const SHOW_GLOBAL_SETTINGS = false;
	import gameStaticDataFromFile from '$lib/game-static-info.json';

	interface GameStaticData {
		[gameId: string]: {
			id: string;
			displayName: string;
			// Add other fields from game-static-info.json if needed for display
			expectedHash?: string;
			fileExtensions?: string;
		};
	}

	const gameStaticData = gameStaticDataFromFile as GameStaticData;

	function getGameDisplayName(gameId: string): string {
		return gameStaticData[gameId]?.displayName || gameId; // Fallback to gameId if not found
	}

    function getSettingMetadata(gameId: string | null, key: string): MetadataSetting | undefined {
        if (!metadata) return undefined;
        if (gameId) {
            const gameKey = gameId.toLowerCase();
            const gs = metadata.gameSettings || {};
            // Resolve the best matching game key in metadata using case-insensitive and alias matching
            const aliasMap: Record<string, string[]> = {
                alttp: ["alttp", "alttpr", "zelda3", "z3"],
                alttpr: ["alttpr", "alttp", "zelda3", "z3"],
                z1: ["z1", "z1r", "zelda1"],
                z1r: ["z1r", "z1", "zelda1"],
                g2: ["g2", "g2r", "goonies2"],
                g2r: ["g2r", "g2", "goonies2"],
            };
            const candidates = [gameKey, ...(aliasMap[gameKey] || [])];
            let matchedKey: string | undefined = undefined;
            for (const cand of candidates) {
                const found = Object.keys(gs).find((k) => k.toLowerCase() === cand.toLowerCase());
                if (found) {
                    matchedKey = found;
                    break;
                }
            }
            if (!matchedKey) {
                // Final fallback: pick the single key if there is only one game in metadata
                const keys = Object.keys(gs);
                matchedKey = keys.length === 1 ? keys[0] : undefined;
            }
            if (matchedKey) {
                const settingsArr = gs[matchedKey]?.settings;
                if (settingsArr) return settingsArr.find((s) => s.key === key);
            }
        }
        return metadata.settings?.find((s) => s.key === key);
    }

	function isVisible(setting: MetadataSetting | undefined): boolean {
		if (!setting) return true;
		if (!setting.visibility) return true;
		if (visibility.length === 0) return true;
		return visibility.includes(setting.visibility);
	}

	function getOptionLabel(gameId: string | null, key: string): string {
		const setting = getSettingMetadata(gameId, key);
		return setting?.name || formatOptionKey(key);
	}

	// Grouping helpers -------------------------------------------------------
	interface DisplayEntry {
		key: string;
		value: unknown;
		meta?: MetadataSetting;
	}

	function groupEntries(entries: DisplayEntry[]) {
		const grouped: Record<string, Record<string, DisplayEntry[]>> = {};
		for (const e of entries) {
			const category = (() => {
				const c = (e.meta as unknown as { category?: unknown } | undefined)?.category;
				if (!c) return 'General';
				return typeof c === 'string' ? c : ((c as { name?: string }).name ?? 'General');
			})();
			const subcategory = (() => {
				const sc = (e.meta as unknown as { subcategory?: unknown } | undefined)?.subcategory;
				if (!sc) return 'General';
				return typeof sc === 'string' ? sc : ((sc as { name?: string }).name ?? 'General');
			})();
			if (!grouped[category]) grouped[category] = {};
			if (!grouped[category][subcategory]) grouped[category][subcategory] = [];
			grouped[category][subcategory].push(e);
		}
		return grouped;
	}

	function orderGrouped(grouped: Record<string, Record<string, DisplayEntry[]>>) {
		const categories = Object.entries(grouped);
		const specified = categories
			.filter(([c]) => c !== 'General')
			.sort((a, b) => a[0].localeCompare(b[0]));
		const general = categories.filter(([c]) => c === 'General');
		const ordered = [...specified, ...general];
		return ordered.map(([cat, subs]) => {
			const subEntries = Object.entries(subs);
			const subSpecified = subEntries
				.filter(([s]) => s !== 'General')
				.sort((a, b) => a[0].localeCompare(b[0]));
			const subGeneral = subEntries.filter(([s]) => s === 'General');
			return [cat, [...subSpecified, ...subGeneral]] as [string, Array<[string, DisplayEntry[]]>];
		});
	}

	// Collapsible display helpers with precedence and normalization
	type DisplayMode = 'expanded' | 'collapsed' | undefined;

	function normalizeDisplayMode(v: unknown): DisplayMode {
		if (typeof v !== 'string') return undefined;
		switch (v.trim().toLowerCase()) {
			case 'expanded':
				return 'expanded';
			case 'collapsed':
				return 'collapsed';
			case 'static':
			default:
				return undefined; // treat static/unknown as non-collapsible
		}
	}

	function modeRank(mode: DisplayMode): number {
		// Collapsed > Expanded > Static(undefined)
		return mode === 'collapsed' ? 2 : mode === 'expanded' ? 1 : 0;
	}

	function getCategoryDisplayMode(cat: string, subs: Array<[string, DisplayEntry[]]>): DisplayMode {
		let best: DisplayMode = undefined;
		let bestRank = 0;
		for (const [, entries] of subs) {
			for (const e of entries) {
				const c = (e.meta as unknown as { category?: unknown } | undefined)?.category;
				const displayValue =
					c && typeof c === 'object' ? (c as { display?: unknown }).display : undefined;
				const cand = normalizeDisplayMode(
					typeof displayValue === 'string' ? displayValue.trim() : displayValue
				);
				const rank = modeRank(cand);
				if (rank > bestRank) {
					best = cand;
					bestRank = rank;
					if (bestRank === 2) break; // collapsed wins
				}
			}
			if (bestRank === 2) break;
		}
		if (!best && cat === 'General') return 'collapsed';
		return best;
	}

	function getSubcategoryDisplayMode(entries: DisplayEntry[]): DisplayMode {
		let best: DisplayMode = undefined;
		let bestRank = 0;
		for (const e of entries) {
			const sc = (e.meta as unknown as { subcategory?: unknown } | undefined)?.subcategory;
			const displayValue =
				sc && typeof sc === 'object' ? (sc as { display?: unknown }).display : undefined;
			const cand = normalizeDisplayMode(
				typeof displayValue === 'string' ? displayValue.trim() : displayValue
			);
			const rank = modeRank(cand);
			if (rank > bestRank) {
				best = cand;
				bestRank = rank;
				if (bestRank === 2) break;
			}
		}
		return best;
	}

	function buildEntries(map: Record<string, unknown>, gameId: string | null): DisplayEntry[] {
		// We need to respect dependsOn + optionsFor just like the config form does.
		// Strategy:
		// 1. Gather raw entries with metadata.
		// 2. Build a lookup of current option values (coerce to string or array of strings for comparison).
		// 3. Filter out entries whose dependsOn criteria are not met.
		// 4. For options that are auxiliary providers (optionsFor) only show if parent value === 'Random'.
		const raw = Object.entries(map).map(([k, v]) => ({
			key: k,
			value: v,
			meta: getSettingMetadata(gameId, k)
		}));

		// If we have metadata, restrict display strictly to known options for the version snapshot.
		const known = metadata
			? raw.filter((e) => e.meta !== undefined)
			: raw;

		// Quick map for value lookups (already provided in map arg) but normalize for comparison.
		const valueStrings = new Map<string, string[]>();
		for (const [k, v] of Object.entries(map)) {
			if (Array.isArray(v)) {
				valueStrings.set(
					k,
					v.map((x) => String(x))
				);
			} else if (v !== null && typeof v === 'object') {
				// Objects are rare for primitive options; stringify keys & values in case some upstream shape exists.
				valueStrings.set(k, [String((v as { value?: unknown }).value ?? '')]);
			} else if (v !== undefined) {
				valueStrings.set(k, [String(v)]);
			}
		}

		const passesDependsOn = (entry: DisplayEntry): boolean => {
			const meta = entry.meta;
			if (!meta?.dependsOn?.key) return true;
			const parentKey = meta.dependsOn.key;
			const allowed = meta.dependsOn.values.map((v: unknown) => String(v));
			const parentValues = valueStrings.get(parentKey) || [];
			if (parentValues.some((pv) => allowed.includes(pv))) return true;
			// Fallback for Single/MultipleChoice where stored value is the mapped value and allowed are keys.
			const parentMeta = raw.find((r) => r.key === parentKey)?.meta;
			if (
				parentMeta &&
				(parentMeta.type === 'SingleChoice' || parentMeta.type === 'MultipleChoice') &&
				'values' in parentMeta &&
				parentMeta.values
			) {
				// Build reverse map value -> keys
				const reverse = new Map<string, string[]>();
				Object.entries(parentMeta.values as Record<string, unknown>).forEach(([k, v]) => {
					const sv = String(v);
					if (!reverse.has(sv)) reverse.set(sv, []);
					reverse.get(sv)!.push(k);
				});
				for (const pv of parentValues) {
					const keys = reverse.get(pv) || [];
					if (keys.some((k) => allowed.includes(k))) return true;
				}
			}
			return false;
		};

		const passesOptionsFor = (entry: DisplayEntry): boolean => {
			const meta = entry.meta as (MetadataSetting & { optionsFor?: string }) | undefined;
			if (!meta?.optionsFor) return true;
			const parentVals = valueStrings.get(meta.optionsFor) || [];
			// Only show auxiliary provider when parent is explicitly 'Random'
			return parentVals.includes('Random');
		};

		return known.filter((e) => isVisible(e.meta) && passesDependsOn(e) && passesOptionsFor(e));
	}

	// Format option keys to be more human-readable
	function formatOptionKey(key: string): string {
		return key
			.replace(/([a-z0-9])([A-Z])/g, '$1 $2') // Add spaces before capital letters only if preceded by lowercase or digit
			.replace(/^./, (str) => str.toUpperCase()) // Capitalize first letter
			.replace(/_/g, ' '); // Replace underscores with spaces
	}

	function formatOptionValue(value: unknown): string {
		if (typeof value === 'boolean') {
			return value ? 'Yes' : 'No';
		}
		if (Array.isArray(value)) {
			return value.join(', ');
		}
		if (value === null || value === undefined) {
			return 'Not specified';
		}
		if (typeof value === 'object' && value !== null) {
			const stringified = JSON.stringify(value);
			return stringified.length > 60 ? '[Complex Value]' : stringified;
		}
		return String(value);
	}
</script>

{#if options && options.length > 0}
	<div class="space-y-3">
		<!-- Games Included Section -->
		{#if options.length > 1}
			<div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm p-3">
				<h4 class="text-sm font-semibold text-slate-800 dark:text-slate-200 mb-1.5">
					Games Included
				</h4>
				<div class="grid grid-cols-1 md:grid-cols-2 gap-1 pl-1">
					{#each options as game (game)}
						{@const gameId = game.id}
						<div class="text-slate-700 dark:text-slate-300 flex items-center gap-1.5 text-sm">
							<span class="inline-block w-1.5 h-1.5 bg-slate-400 dark:bg-slate-500 rounded-full"
							></span>
							<span>{getGameDisplayName(gameId)}</span>
						</div>
					{/each}
				</div>
			</div>
		{/if}

		<!-- Global Settings Section (Grouped) -->
		{#if SHOW_GLOBAL_SETTINGS && globalOptions && Object.keys(globalOptions).length > 0}
			{@const globalGrouped = orderGrouped(groupEntries(buildEntries(globalOptions, null)))}
			{@const showGlobalCategoryHeadings = globalGrouped.some(([c]) => c !== 'General')}
			<div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm p-3">
				<h4 class="text-sm font-semibold text-slate-800 dark:text-slate-200 mb-1.5">
					Global Settings
				</h4>
				{#each globalGrouped as [cat, subs] (cat)}
					<div
						class="mb-4 pt-3 border-t first:border-t-0 first:pt-0 border-slate-200 dark:border-slate-700"
					>
						{#if getCategoryDisplayMode(cat, subs)}
							<details class="mb-1 group" open={getCategoryDisplayMode(cat, subs) === 'expanded'}>
								<summary
									class="cursor-pointer flex items-center gap-3 mb-2 select-none focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/60 rounded"
									title="Toggle section"
								>
									<div class="h-4 w-1 rounded bg-indigo-500"></div>
									<span
										class="inline-flex items-center justify-center h-5 w-5 rounded-md bg-slate-100 text-slate-600 ring-1 ring-slate-200/70 transition-colors hover:bg-slate-200 hover:ring-slate-300 dark:bg-slate-700/70 dark:text-slate-100 dark:ring-slate-500/50 dark:hover:bg-slate-600/70 dark:hover:ring-slate-400/60 group-open:bg-indigo-50 group-open:text-indigo-600 dark:group-open:bg-indigo-400/25 dark:group-open:text-indigo-200"
									>
										<svg
											class="h-3.5 w-3.5 text-current transition-transform group-open:rotate-90"
											viewBox="0 0 20 20"
											fill="currentColor"
											aria-hidden="true"
										>
											<path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
										</svg>
									</span>
									<h5
										class="text-[11px] font-semibold uppercase tracking-wide text-slate-600 dark:text-slate-300"
									>
										{cat}
									</h5>
								</summary>
								{#each subs as [subcat, entries] (subcat)}
									<div class="mb-3">
										{#if getSubcategoryDisplayMode(entries)}
											<details
												class="mb-1 group"
												open={getSubcategoryDisplayMode(entries) === 'expanded'}
											>
												<summary
													class="cursor-pointer text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1 select-none flex items-center gap-2 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/60 rounded"
													title="Toggle section"
												>
													<span
														class="inline-flex items-center justify-center h-4.5 w-4.5 rounded-md bg-slate-100 text-slate-600 ring-1 ring-slate-200/70 transition-colors hover:bg-slate-200 hover:ring-slate-300 dark:bg-slate-700/70 dark:text-slate-100 dark:ring-slate-500/50 dark:hover:bg-slate-600/70 dark:hover:ring-slate-400/60 group-open:bg-indigo-50 group-open:text-indigo-600 dark:group-open:bg-indigo-400/25 dark:group-open:text-indigo-200"
													>
														<svg
															class="h-3.5 w-3.5 text-current transition-transform group-open:rotate-90"
															viewBox="0 0 20 20"
															fill="currentColor"
															aria-hidden="true"
														>
															<path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
														</svg>
													</span>
													{subcat}
												</summary>

												<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
													{#each entries as e (e.key)}
														<div
															class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
														>
															<span class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
																>{getOptionLabel(null, e.key)}:</span
															>
															<span class="text-slate-800 dark:text-slate-200 break-words font-bold"
																>{formatOptionValue(e.value)}</span
															>
														</div>
													{/each}
												</div>
											</details>
										{:else}
											{#if subs.some(([s]) => s !== 'General') || subcat !== 'General'}
												<h6 class="text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1">
													{subcat}
												</h6>
											{/if}
											<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
												{#each entries as e (e.key)}
													<div
														class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
													>
														<span class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
															>{getOptionLabel(null, e.key)}:</span
														>
														<span class="text-slate-800 dark:text-slate-200 break-words font-bold"
															>{formatOptionValue(e.value)}</span
														>
													</div>
												{/each}
											</div>
										{/if}
									</div>
								{/each}
							</details>
						{:else}
							{#if showGlobalCategoryHeadings || cat !== 'General'}
								<div class="flex items-center gap-2 mb-2">
									<div class="h-4 w-1 rounded bg-indigo-500"></div>
									<h5
										class="text-[11px] font-semibold uppercase tracking-wide text-slate-600 dark:text-slate-300"
									>
										{cat}
									</h5>
								</div>
							{/if}
							{#each subs as [subcat, entries] (subcat)}
								<div class="mb-3">
									{#if getSubcategoryDisplayMode(entries)}
										<details class="mb-1" open={getSubcategoryDisplayMode(entries) === 'expanded'}>
											<summary
												class="cursor-pointer text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1 select-none"
												>{subcat}</summary
											>

											<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
												{#each entries as e (e.key)}
													<div
														class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
													>
														<span class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
															>{getOptionLabel(null, e.key)}:</span
														>
														<span class="text-slate-800 dark:text-slate-200 break-words font-bold"
															>{formatOptionValue(e.value)}</span
														>
													</div>
												{/each}
											</div>
										</details>
									{:else}
										{#if subs.some(([s]) => s !== 'General') || subcat !== 'General'}
											<h6 class="text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1">
												{subcat}
											</h6>
										{/if}
										<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
											{#each entries as e (e.key)}
												<div
													class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
												>
													<span class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
														>{getOptionLabel(null, e.key)}:</span
													>
													<span class="text-slate-800 dark:text-slate-200 break-words font-bold"
														>{formatOptionValue(e.value)}</span
													>
												</div>
											{/each}
										</div>
									{/if}
								</div>
							{/each}
						{/if}
					</div>
				{/each}
			</div>
		{/if}

		<!-- Per-Game Settings Section (Grouped) -->
		{#if options && options.length > 0}
			<div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm p-3">
				<h4 class="text-sm font-semibold text-slate-800 dark:text-slate-200 mb-1.5">
					Game-Specific Settings
				</h4>
				{#each options as game (game)}
					{@const gameId = game.id}
					{@const gameSettings = game.options}
					{#if gameSettings && Object.keys(gameSettings).length > 0}
						{@const grouped = orderGrouped(groupEntries(buildEntries(gameSettings, gameId)))}
						<div class="mb-5 last:mb-2">
							<h5
								class="text-xs font-semibold text-slate-700 dark:text-slate-300 mb-2 border-l-3 border-slate-400 pl-1.5"
							>
								{getGameDisplayName(gameId)}
							</h5>
							{#each grouped as [cat, subs] (cat)}
								<div
									class="mb-4 pt-3 border-t first:border-t-0 first:pt-0 border-slate-200 dark:border-slate-700"
								>
									{#if getCategoryDisplayMode(cat, subs)}
								<details class="mb-1 group" open={getCategoryDisplayMode(cat, subs) === 'expanded'}>
											<summary
												class="cursor-pointer flex items-center gap-3 mb-2 select-none focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/60 rounded"
												title="Toggle section"
											>
												<div class="h-4 w-1 rounded bg-indigo-500"></div>
												<span
													class="inline-flex items-center justify-center h-5 w-5 rounded-md bg-slate-100 text-slate-600 ring-1 ring-slate-200/70 transition-colors hover:bg-slate-200 hover:ring-slate-300 dark:bg-slate-700/70 dark:text-slate-100 dark:ring-slate-500/50 dark:hover:bg-slate-600/70 dark:hover:ring-slate-400/60 group-open:bg-indigo-50 group-open:text-indigo-600 dark:group-open:bg-indigo-400/25 dark:group-open:text-indigo-200"
												>
													<svg
														class="h-3.5 w-3.5 text-current transition-transform group-open:rotate-90"
														viewBox="0 0 20 20"
														fill="currentColor"
														aria-hidden="true"
													>
														<path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
													</svg>
												</span>
												<h6
													class="text-[11px] font-semibold uppercase tracking-wide text-slate-600 dark:text-slate-300"
												>
													{cat}
												</h6>
											</summary>
											{#each subs as [subcat, entries] (subcat)}
												<div class="mb-3">
													{#if getSubcategoryDisplayMode(entries)}
														<details
															class="mb-1"
															open={getSubcategoryDisplayMode(entries) === 'expanded'}
														>
															<summary
																class="cursor-pointer text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1 select-none"
																>{subcat}</summary
															>
															<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
																{#each entries as e (e.key)}
																	<div
																		class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
																	>
																		<span
																			class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
																			>{getOptionLabel(gameId, e.key)}:</span
																		>
																		<span
																			class="text-slate-800 dark:text-slate-200 break-words font-bold"
																			>{formatOptionValue(e.value)}</span
																		>
																	</div>
																{/each}
															</div>
														</details>
													{:else}
														{#if subs.some(([s]) => s !== 'General') || subcat !== 'General'}
															<h6
																class="text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1"
															>
																{subcat}
															</h6>
														{/if}
														<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
															{#each entries as e (e.key)}
																<div
																	class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
																>
																	<span
																		class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
																		>{getOptionLabel(gameId, e.key)}:</span
																	>
																	<span
																		class="text-slate-800 dark:text-slate-200 break-words font-bold"
																		>{formatOptionValue(e.value)}</span
																	>
																</div>
															{/each}
														</div>
													{/if}
												</div>
											{/each}
										</details>
									{:else}
										{#if grouped.some(([c]) => c !== 'General') || cat !== 'General'}
											<div class="flex items-center gap-2 mb-2">
												<div class="h-4 w-1 rounded bg-indigo-500"></div>
												<h6
													class="text-[11px] font-semibold uppercase tracking-wide text-slate-600 dark:text-slate-300"
												>
													{cat}
												</h6>
											</div>
										{/if}
										{#each subs as [subcat, entries] (subcat)}
											<div class="mb-3">
												{#if getSubcategoryDisplayMode(entries)}
													<details
														class="mb-1"
														open={getSubcategoryDisplayMode(entries) === 'expanded'}
													>
														<summary
															class="cursor-pointer text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1 select-none"
															>{subcat}</summary
														>
														<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
															{#each entries as e (e.key)}
																<div
																	class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
																>
																	<span
																		class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
																		>{getOptionLabel(gameId, e.key)}:</span
																	>
																	<span
																		class="text-slate-800 dark:text-slate-200 break-words font-bold"
																		>{formatOptionValue(e.value)}</span
																	>
																</div>
															{/each}
														</div>
													</details>
												{:else}
													{#if subs.some(([s]) => s !== 'General') || subcat !== 'General'}
														<h6
															class="text-[11px] font-medium text-slate-500 dark:text-slate-400 mb-1"
														>
															{subcat}
														</h6>
													{/if}
													<div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
														{#each entries as e (e.key)}
															<div
																class="flex items-baseline bg-slate-50 dark:bg-slate-700/70 rounded px-2 py-1"
															>
																<span class="font-medium text-slate-600 dark:text-slate-300 mr-1.5"
																	>{getOptionLabel(gameId, e.key)}:</span
																>
																<span
																	class="text-slate-800 dark:text-slate-200 break-words font-bold"
																	>{formatOptionValue(e.value)}</span
																>
															</div>
														{/each}
													</div>
												{/if}
											</div>
										{/each}
									{/if}
								</div>
							{/each}
						</div>
					{/if}
				{/each}
			</div>
		{/if}
	</div>
{:else}
	<Card>
		<p class="text-sm text-slate-500 dark:text-slate-400">
			Seed options are not available or are in an unexpected format.
		</p>
	</Card>
{/if}
