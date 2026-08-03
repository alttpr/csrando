<script lang="ts">
	import type { MetadataSetting } from "$lib/types";
	import OptionField from "./OptionField.svelte";

	interface Props {
		options?: Array<MetadataSetting>;
		values?: { [key: string]: unknown };
		title: string;
		visibility?: Array<string>;
		selectedGames?: Array<string>;
	}

	let {
		options = [],
		values = $bindable({}),
		title,
		visibility = [],
		selectedGames = [],
	}: Props = $props();

	const hasOptionsFor = (
		o: MetadataSetting,
	): o is MetadataSetting & { optionsFor: string } => {
		return typeof (o as { optionsFor?: unknown }).optionsFor === "string";
	};

	const getVisibleOptions = (
		opts: MetadataSetting[],
		vals: { [key: string]: unknown },
		vis: Array<string>,
		selected: Array<string>,
	) => {
		return opts.filter((option) => {
			// 1. Respect the requested visibility level for this option.
			const isVisibilityLevelMet = (() => {
				if (!vis.length) return true;
				if (!option.visibility) return true;
				return vis.includes(option.visibility);
			})();
			if (!isVisibilityLevelMet) return false;

			// 1b. Game-selection gate: hide as soon as any selected game falls
			// outside the option's allow-list.
			if (
				option.onlyWithGames &&
				selected.some((game) => !option.onlyWithGames!.includes(game))
			) {
				return false;
			}

			// 2. Enforce dependsOn relationships against the normalized schema shape ({ key, values }).
			if (option.dependsOn?.key) {
				const allowedValues = option.dependsOn.values;
				const parentValue = vals[option.dependsOn.key];
				const valuesToCheck = Array.isArray(parentValue)
					? parentValue
					: [parentValue];
				const hasAllowedValue = valuesToCheck.some((value) =>
					allowedValues.some((allowed) => allowed === value),
				);
				if (!hasAllowedValue) return false;
			}

			// 3. Auxiliary option providers only show when parent is set to 'Random'.
			if (hasOptionsFor(option)) {
				return vals[option.optionsFor] === "RandomPick";
			}

			return true;
		});
	};

	const visibleOptions = $derived(
		getVisibleOptions(options, values, visibility, selectedGames),
	);

	const topLevelOptions = $derived(
		visibleOptions.filter(
			(opt) => !opt.dependsOn?.key && !hasOptionsFor(opt),
		),
	);

	function normalizeName(value: unknown): string {
		const s =
			typeof value === "string"
				? value
				: (value as { name?: string })?.name;
		const trimmed = (s ?? "General").trim();
		return trimmed.length > 0 ? trimmed : "General";
	}

	function getCategoryName(option: MetadataSetting): string {
		const c = (option as unknown as { category?: unknown }).category;
		return normalizeName(c);
	}

	function getSubcategoryName(option: MetadataSetting): string {
		const sc = (option as unknown as { subcategory?: unknown }).subcategory;
		return normalizeName(sc);
	}

	const groupedOptions = $derived(
		topLevelOptions.reduce(
			(acc, option) => {
				const category = getCategoryName(option);
				const subcategory = getSubcategoryName(option);

				acc[category] ??= {};
				acc[category][subcategory] ??= [];
				acc[category][subcategory].push(option);
				return acc;
			},
			{} as Record<string, Record<string, MetadataSetting[]>>,
		),
	);

	const orderedCategories = $derived<
		[string, Record<string, MetadataSetting[]>][]
	>(
		(() => {
			const entries = Object.entries(groupedOptions) as Array<
				[string, Record<string, MetadataSetting[]>]
			>;
			const general = entries.filter(([c]) => c === "General");
			const specified = entries
				.filter(([c]) => c !== "General")
				.sort((a, b) => a[0].localeCompare(b[0]));
			const withDisplay = specified.map(([name, subcategories]) => ({
				name,
				subcategories,
				display: resolveCategoryDisplay(name),
			}));
			const uncollapsed = withDisplay
				.filter(({ display }) => display !== "Collapsed")
				.map(
					({ name, subcategories }) =>
						[name, subcategories] as [
							string,
							Record<string, MetadataSetting[]>,
						],
				);
			const collapsed = withDisplay
				.filter(({ display }) => display === "Collapsed")
				.map(
					({ name, subcategories }) =>
						[name, subcategories] as [
							string,
							Record<string, MetadataSetting[]>,
						],
				);
			return [...uncollapsed, ...collapsed, ...general];
		})(),
	);

	// Collapsible display modes inferred from metadata on any option within the group.
	// Normalize various upstream casings/values to a single internal shape.
	type DisplayMode = "Expanded" | "Collapsed" | undefined;

	function normalizeDisplayMode(value: unknown): DisplayMode {
		if (typeof value !== "string") return undefined;
		switch (value.toLowerCase()) {
			case "expanded":
				return "Expanded";
			case "collapsed":
				return "Collapsed";
			case "static":
			default:
				return undefined; // treat unknown/static as non-collapsible
		}
	}

	function modeRank(mode: DisplayMode): number {
		// Precedence: Collapsed (2) > Expanded (1) > Static/undefined (0)
		switch (mode) {
			case "Collapsed":
				return 2;
			case "Expanded":
				return 1;
			default:
				return 0;
		}
	}

	function resolveCategoryDisplay(cat: string): DisplayMode {
		let best: DisplayMode = undefined;
		let bestRank = 0;
		for (const opt of options) {
			if (getCategoryName(opt) !== cat) continue;
			const c = (opt as unknown as { category?: unknown }).category;
			let cand: DisplayMode = undefined;
			if (
				c &&
				typeof c === "object" &&
				(c as { display?: unknown }).display
			) {
				const raw = (c as { display?: unknown }).display;
				cand = normalizeDisplayMode(
					typeof raw === "string" ? raw.trim() : raw,
				);
			}
			const rank = modeRank(cand);
			if (rank > bestRank) {
				best = cand;
				bestRank = rank;
				if (bestRank === modeRank("Collapsed")) break;
			}
		}
		// Default: collapse the auto-generated top-level "General" bucket if no mode found
		if (!best && cat === "General") return "Collapsed";
		return best;
	}

	function resolveSubcategoryDisplay(cat: string, sub: string): DisplayMode {
		let best: DisplayMode = undefined;
		let bestRank = 0;
		for (const opt of options) {
			if (getCategoryName(opt) !== cat) continue;
			if (getSubcategoryName(opt) !== sub) continue;
			const sc = (opt as unknown as { subcategory?: unknown })
				.subcategory;
			let cand: DisplayMode = undefined;
			if (
				sc &&
				typeof sc === "object" &&
				(sc as { display?: unknown }).display
			) {
				const raw = (sc as { display?: unknown }).display;
				cand = normalizeDisplayMode(
					typeof raw === "string" ? raw.trim() : raw,
				);
			}
			const rank = modeRank(cand);
			if (rank > bestRank) {
				best = cand;
				bestRank = rank;
				if (bestRank === modeRank("Collapsed")) break;
			}
		}
		return best;
	}

	interface GroupedSubcategory {
		name: string;
		options: MetadataSetting[];
		displayMode: DisplayMode;
		showHeading: boolean;
	}

	interface GroupedCategory {
		name: string;
		subcategories: GroupedSubcategory[];
		displayMode: DisplayMode;
		showHeading: boolean;
	}

	const categories = $derived<GroupedCategory[]>(
		(() =>
			orderedCategories.map(([categoryName, subcategories]) => {
				const entries = Object.entries(subcategories)
					.sort((a, b) => {
						if (a[0] === "General" && b[0] !== "General") return 1;
						if (b[0] === "General" && a[0] !== "General") return -1;
						return a[0].localeCompare(b[0]);
					})
					.map(([name, opts], _, all) => ({
						name,
						options: opts,
						displayMode: resolveSubcategoryDisplay(
							categoryName,
							name,
						),
						showHeading: !(all.length === 1 && name === "General"),
					}));

				return {
					name: categoryName,
					subcategories: entries,
					displayMode: resolveCategoryDisplay(categoryName),
					showHeading: !(
						orderedCategories.length === 1 &&
						categoryName === "General"
					),
				};
			}))(),
	);

	// Track collapsible open state so reactive updates do not snap sections shut.
	const categoryOpen = $state<Record<string, boolean>>({});
	const subcategoryOpen = $state<Record<string, boolean>>({});

	$effect(() => {
		for (const category of categories) {
			if (
				category.displayMode &&
				categoryOpen[category.name] === undefined
			) {
				categoryOpen[category.name] =
					category.displayMode === "Expanded";
			}

			for (const subcategory of category.subcategories) {
				if (!subcategory.displayMode) continue;
				const subKey = `${category.name}::${subcategory.name}`;
				if (subcategoryOpen[subKey] === undefined) {
					subcategoryOpen[subKey] =
						subcategory.displayMode === "Expanded";
				}
			}
		}
	});

	// child relationships handled by OptionField
</script>

<div class="bg-white dark:bg-slate-800 rounded-lg shadow-md p-3 mb-3">
	<h2 class="text-lg font-bold mb-2">{title}</h2>

	{#snippet OptionsGrid(options: MetadataSetting[])}
		<div
			class="rounded-md ring-1 ring-slate-200 dark:ring-slate-700 bg-slate-50/60 dark:bg-slate-900/40 p-3 grid grid-cols-1 md:grid-cols-2 gap-x-4 gap-y-4"
		>
			{#each options as option (option.key)}
				<div>
					<OptionField
						{option}
						bind:values
						{visibleOptions}
						depth={0}
					/>
				</div>
			{/each}
		</div>
	{/snippet}

	{#each categories as category (category.name)}
		<div
			class="mb-6 pt-4 border-t first:border-t-0 first:pt-0 border-slate-200 dark:border-slate-700"
		>
			{#if category.displayMode}
				<details
					class="mb-1 group"
					bind:open={categoryOpen[category.name]}
				>
					<summary
						class="cursor-pointer flex items-center gap-3 mb-2 select-none focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/60 rounded"
						title="Toggle section"
					>
						<div
							class="h-5 w-1 rounded bg-gradient-to-b from-indigo-500 to-violet-500"
						></div>
						<span
							class="inline-flex items-center justify-center h-6 w-6 rounded-md bg-slate-100 text-slate-600 ring-1 ring-slate-200/70 transition-colors hover:bg-slate-200 hover:ring-slate-300 dark:bg-slate-700/70 dark:text-slate-100 dark:ring-slate-500/50 dark:hover:bg-slate-600/70 dark:hover:ring-slate-400/60 group-open:bg-indigo-50 group-open:text-indigo-600 dark:group-open:bg-indigo-400/25 dark:group-open:text-indigo-200"
						>
							<svg
								class="h-4.5 w-4.5 text-current transition-transform group-open:rotate-90"
								viewBox="0 0 20 20"
								fill="currentColor"
								aria-hidden="true"
							>
								<path
									fill-rule="evenodd"
									d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z"
									clip-rule="evenodd"
								/>
							</svg>
						</span>
						<h3
							class="text-sm font-semibold tracking-wide uppercase text-slate-700 dark:text-slate-200"
						>
							{category.name}
						</h3>
					</summary>

					{#each category.subcategories as subcategory (subcategory.name)}
						<div class="mb-4">
							{#if subcategory.displayMode}
								<details
									class="mb-1 group"
									bind:open={
										subcategoryOpen[
											`${category.name}::${subcategory.name}`
										]
									}
								>
									<summary
										class="cursor-pointer mb-2 flex items-center gap-2 select-none focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/60 rounded"
										title="Toggle section"
									>
										<span
											class="inline-flex items-center justify-center h-5.5 w-5.5 rounded-md bg-slate-100 text-slate-600 ring-1 ring-slate-200/70 transition-colors hover:bg-slate-200 hover:ring-slate-300 dark:bg-slate-700/70 dark:text-slate-100 dark:ring-slate-500/50 dark:hover:bg-slate-600/70 dark:hover:ring-slate-400/60 group-open:bg-indigo-50 group-open:text-indigo-600 dark:group-open:bg-indigo-400/25 dark:group-open:text-indigo-200"
										>
											<svg
												class="h-4 w-4 text-current transition-transform group-open:rotate-90"
												viewBox="0 0 20 20"
												fill="currentColor"
												aria-hidden="true"
											>
												<path
													fill-rule="evenodd"
													d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z"
													clip-rule="evenodd"
												/>
											</svg>
										</span>
										{#if subcategory.showHeading}
											<h4
												class="text-[13px] font-medium text-slate-600 dark:text-slate-300"
											>
												{subcategory.name}
											</h4>
											<div
												class="flex-1 h-px bg-slate-200 dark:bg-slate-700"
											></div>
										{/if}
									</summary>
									{@render OptionsGrid(subcategory.options)}
								</details>
							{:else}
								{#if subcategory.showHeading}
									<div class="mb-2 flex items-center gap-2">
										<h4
											class="text-[13px] font-medium text-slate-600 dark:text-slate-300"
										>
											{subcategory.name}
										</h4>
										<div
											class="flex-1 h-px bg-slate-200 dark:bg-slate-700"
										></div>
									</div>
								{/if}
								{@render OptionsGrid(subcategory.options)}
							{/if}
						</div>
					{/each}
				</details>
			{:else}
				{#if category.showHeading}
					<div class="flex items-center gap-2 mb-3">
						<div
							class="h-5 w-1 rounded bg-gradient-to-b from-indigo-500 to-violet-500"
						></div>
						<h3
							class="text-sm font-semibold tracking-wide uppercase text-slate-700 dark:text-slate-200"
						>
							{category.name}
						</h3>
					</div>
				{/if}

				{#each category.subcategories as subcategory (subcategory.name)}
					<div class="mb-4">
						{#if subcategory.showHeading}
							<div class="mb-2 flex items-center gap-2">
								<h4
									class="text-[13px] font-medium text-slate-600 dark:text-slate-300"
								>
									{subcategory.name}
								</h4>
								<div
									class="flex-1 h-px bg-slate-200 dark:bg-slate-700"
								></div>
							</div>
						{/if}
						{@render OptionsGrid(subcategory.options)}
					</div>
				{/each}
			{/if}
		</div>
	{/each}
</div>
