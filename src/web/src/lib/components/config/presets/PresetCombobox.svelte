<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import Badge from "$lib/components/ui/Badge.svelte";
	import type { PresetSummaryDto } from "$lib/schemas/presets";

	interface Group {
		id: string;
		label: string;
		presets: PresetSummaryDto[];
	}

	interface Props {
		officials: PresetSummaryDto[];
		mine: PresetSummaryDto[];
		favorites: string[];
		selectedId: string | null;
		selectedName: string | null;
		// The current form state has diverged from the selected preset's
		// revision; the closed combobox flags it so the plain preset name is
		// never mistaken for the unmodified preset.
		modified?: boolean;
		// Tooltip for the closed state (e.g. the preset's description).
		title?: string | null;
		disabled?: boolean;
		canPin: boolean;
		onselect: (presetId: string) => void;
		onpin: (presetId: string) => void;
	}

	let {
		officials,
		mine,
		favorites,
		selectedId,
		selectedName,
		modified = false,
		title = null,
		disabled = false,
		canPin,
		onselect,
		onpin,
	}: Props = $props();

	let isOpen = $state(false);
	let query = $state("");
	let activeId = $state<string | null>(null);
	let rootEl: HTMLElement | null = null;
	let inputEl: HTMLInputElement | null = $state(null);

	const listboxId = "seed-preset-listbox";

	function matches(preset: PresetSummaryDto, q: string): boolean {
		if (!q) return true;
		const needle = q.trim().toLowerCase();
		return (
			preset.name.toLowerCase().includes(needle) ||
			(preset.description ?? "").toLowerCase().includes(needle) ||
			preset.selectedGames.some((g) => g.toLowerCase().includes(needle))
		);
	}

	// Pinned presets get their own group at the top and leave their scope
	// group, so every preset appears exactly once.
	const groups = $derived.by((): Group[] => {
		const isPinned = (p: PresetSummaryDto) => favorites.includes(p.id);
		const result: Group[] = [];
		const pinned = [...mine, ...officials].filter(
			(p) => isPinned(p) && matches(p, query),
		);
		if (pinned.length > 0) {
			result.push({
				id: "pinned",
				label: m.seed_preset_group_pinned(),
				presets: pinned,
			});
		}
		const mineFiltered = mine.filter(
			(p) => !isPinned(p) && matches(p, query),
		);
		if (mineFiltered.length > 0) {
			result.push({
				id: "mine",
				label: m.seed_preset_group_mine(),
				presets: mineFiltered,
			});
		}
		const officialFiltered = officials.filter(
			(p) => !isPinned(p) && matches(p, query),
		);
		if (officialFiltered.length > 0) {
			result.push({
				id: "official",
				label: m.seed_preset_group_official(),
				presets: officialFiltered,
			});
		}
		return result;
	});

	// Flattened visible options with group-scoped DOM ids.
	const flatOptions = $derived(
		groups.flatMap((group) =>
			group.presets.map((preset) => ({
				domId: `seed-preset-option-${group.id}-${preset.id}`,
				group,
				preset,
			})),
		),
	);

	function open() {
		if (disabled) return;
		isOpen = true;
		query = "";
		activeId = flatOptions.find((o) => o.preset.id === selectedId)?.domId ??
			flatOptions[0]?.domId ??
			null;
	}

	function close() {
		isOpen = false;
		query = "";
		activeId = null;
	}

	function selectOption(presetId: string) {
		close();
		inputEl?.blur();
		onselect(presetId);
	}

	function moveActive(delta: number) {
		if (flatOptions.length === 0) return;
		const index = flatOptions.findIndex((o) => o.domId === activeId);
		const next =
			index < 0
				? delta > 0
					? 0
					: flatOptions.length - 1
				: Math.min(Math.max(index + delta, 0), flatOptions.length - 1);
		activeId = flatOptions[next].domId;
		document
			.getElementById(flatOptions[next].domId)
			?.scrollIntoView({ block: "nearest" });
	}

	function handleKeydown(event: KeyboardEvent) {
		if (!isOpen) {
			if (["ArrowDown", "ArrowUp", "Enter"].includes(event.key)) {
				event.preventDefault();
				open();
			}
			return;
		}
		switch (event.key) {
			case "ArrowDown":
				event.preventDefault();
				moveActive(1);
				break;
			case "ArrowUp":
				event.preventDefault();
				moveActive(-1);
				break;
			case "Home":
				event.preventDefault();
				if (flatOptions[0]) activeId = flatOptions[0].domId;
				break;
			case "End":
				event.preventDefault();
				if (flatOptions.length > 0)
					activeId = flatOptions[flatOptions.length - 1].domId;
				break;
			case "Enter": {
				event.preventDefault();
				const active = flatOptions.find((o) => o.domId === activeId);
				if (active) selectOption(active.preset.id);
				break;
			}
			case "Escape":
				event.preventDefault();
				close();
				break;
		}
	}

	function handleClickOutside(event: MouseEvent) {
		if (!isOpen) return;
		const target = event.target as Node | null;
		if (rootEl && target && !rootEl.contains(target)) {
			close();
		}
	}

	const displayValue = $derived(
		isOpen
			? query
			: selectedName
				? modified
					? m.seed_preset_display_modified({ name: selectedName })
					: selectedName
				: m.seed_preset_custom_configuration(),
	);
</script>

<svelte:body onclick={handleClickOutside} />

<div class="relative w-full sm:max-w-xs" bind:this={rootEl}>
	<label class="sr-only" for="seed-preset-combobox">
		{m.seed_preset_toolbar_label()}
	</label>
	<input
		bind:this={inputEl}
		id="seed-preset-combobox"
		type="text"
		role="combobox"
		aria-expanded={isOpen}
		aria-controls={listboxId}
		aria-activedescendant={isOpen ? (activeId ?? undefined) : undefined}
		aria-autocomplete="list"
		autocomplete="off"
		title={isOpen ? undefined : (title ?? undefined)}
		{disabled}
		placeholder={isOpen
			? m.seed_preset_combobox_placeholder()
			: undefined}
		class="w-full rounded-md border border-slate-300 bg-white py-1.5 pl-3 pr-8 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 disabled:opacity-60 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100 {selectedName || isOpen
			? ''
			: 'italic text-slate-600 dark:text-slate-400'}"
		value={displayValue}
		onfocus={open}
		onclick={open}
		oninput={(event) => {
			if (!isOpen) isOpen = true;
			query = (event.target as HTMLInputElement).value;
			activeId = null;
		}}
		onkeydown={handleKeydown}
		data-testid="preset-combobox-input"
	/>
	<span
		class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2"
	>
		<svg
			class="h-4 w-4 text-slate-400 dark:text-slate-500"
			xmlns="http://www.w3.org/2000/svg"
			viewBox="0 0 20 20"
			fill="currentColor"
			aria-hidden="true"
		>
			<path
				fill-rule="evenodd"
				d="M5.22 8.22a.75.75 0 0 1 1.06 0L10 11.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 9.28a.75.75 0 0 1 0-1.06Z"
				clip-rule="evenodd"
			/>
		</svg>
	</span>

	{#if isOpen}
		<ul
			id={listboxId}
			role="listbox"
			aria-label={m.seed_preset_toolbar_label()}
			class="absolute z-50 mt-1 max-h-72 w-full min-w-64 overflow-auto rounded-md border border-slate-200 bg-white py-1 text-sm shadow-lg dark:border-slate-700 dark:bg-slate-800"
			data-testid="preset-combobox-listbox"
		>
			{#if flatOptions.length === 0}
				<li
					class="px-3 py-2 text-slate-500 dark:text-slate-400"
					role="presentation"
				>
					{m.seed_preset_no_results()}
				</li>
			{/if}
			{#each groups as group (group.id)}
				<li role="presentation">
					<div
						id="seed-preset-group-{group.id}"
						class="px-3 pb-1 pt-2 text-xs font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400"
						role="presentation"
					>
						{group.label}
					</div>
					<ul role="group" aria-labelledby="seed-preset-group-{group.id}">
						{#each group.presets as preset (preset.id)}
							{@const domId = `seed-preset-option-${group.id}-${preset.id}`}
							<li
								id={domId}
								role="option"
								title={preset.description ?? undefined}
								aria-selected={preset.id === selectedId}
								class="flex cursor-pointer items-center gap-2 px-3 py-1.5 {activeId ===
								domId
									? 'bg-indigo-600 text-white'
									: preset.id === selectedId
										? 'bg-indigo-50 text-indigo-800 dark:bg-indigo-600/30 dark:text-indigo-200'
										: 'text-slate-800 dark:text-slate-100'}"
								onclick={() => selectOption(preset.id)}
								onmousemove={() => (activeId = domId)}
								onkeydown={(event) => {
									if (event.key === "Enter" || event.key === " ")
										selectOption(preset.id);
								}}
							>
								<span class="min-w-0 flex-1">
									<span class="block truncate font-medium">
										{preset.name}
									</span>
									{#if preset.selectedGames.length > 0}
										<span
											class="block truncate text-xs {activeId === domId
												? 'text-indigo-100'
												: 'text-slate-500 dark:text-slate-400'}"
										>
											{preset.selectedGames.join(", ")}
										</span>
									{/if}
								</span>
								{#if preset.scope === "official"}
									<Badge variant="info">
										{m.seed_preset_badge_official()}
									</Badge>
								{:else}
									<Badge variant="neutral">
										{m.seed_preset_badge_mine()}
									</Badge>
								{/if}
								{#if canPin}
									<button
										type="button"
										class="rounded p-0.5 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-400 {favorites.includes(
											preset.id,
										)
											? 'text-yellow-500'
											: activeId === domId
												? 'text-indigo-200 hover:text-white'
												: 'text-slate-400 hover:text-slate-600 dark:hover:text-slate-200'}"
										aria-pressed={favorites.includes(preset.id)}
										aria-label={favorites.includes(preset.id)
											? m.seed_preset_unpin_aria({ name: preset.name })
											: m.seed_preset_pin_aria({ name: preset.name })}
										onclick={(event) => {
											event.stopPropagation();
											onpin(preset.id);
										}}
									>
										<svg
											class="h-4 w-4"
											xmlns="http://www.w3.org/2000/svg"
											viewBox="0 0 20 20"
											fill="currentColor"
											aria-hidden="true"
										>
											<path
												fill-rule="evenodd"
												d="M10.868 2.884c-.321-.772-1.415-.772-1.736 0l-1.83 4.401-4.753.381c-.833.067-1.171 1.107-.536 1.651l3.62 3.102-1.106 4.637c-.194.813.691 1.456 1.405 1.02L10 15.591l4.069 2.485c.713.436 1.598-.207 1.404-1.02l-1.106-4.637 3.62-3.102c.635-.544.297-1.584-.536-1.65l-4.752-.382-1.831-4.401Z"
												clip-rule="evenodd"
											/>
										</svg>
									</button>
								{/if}
							</li>
						{/each}
					</ul>
				</li>
			{/each}
		</ul>
	{/if}
</div>
