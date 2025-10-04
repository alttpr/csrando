<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import SpriteIcon from '$lib/components/ui/SpriteIcon.svelte';
	import { loadAtlas } from '$lib/sprites/atlas';

	type SpriteItem = {
		value: string;
		name: string;
		imagePath: string; // full URL or relative to PUBLIC_SPRITES_BASE_URL/<game>/<file>
	};

	interface Props {
		items?: SpriteItem[];
		value?: string;
		id?: string;
		className?: string;
		placeholder?: string;
		game?: string;
	}

	let {
		items = [],
		value = $bindable<string | undefined>(undefined),
		id = undefined,
		className = '',
		placeholder = 'Select a sprite',
		game = undefined,
		class: classAttr = ''
	}: Props & { class?: string } = $props<Props & { class?: string }>();

	let isOpen = $state(false);
	let rootEl: HTMLElement | null = null;

	const dispatch = createEventDispatcher<{ change: { value: string } }>();
	const selectedItem = $derived(items.find((item) => item.value === value));

	async function ensureAtlas() {
		if (!game) return;
		await loadAtlas(game).catch(() => null);
	}

	function selectItem(itemValue: string) {
		value = itemValue;
		isOpen = false;
		dispatch('change', { value: itemValue });
	}

	function toggleDropdown() {
		isOpen = !isOpen;
		if (isOpen) void ensureAtlas();
	}

	function handleClickOutside(event: MouseEvent) {
		if (!isOpen) return;
		const target = event.target as Node | null;
		if (rootEl && target && !rootEl.contains(target)) {
			isOpen = false;
		}
	}

	export type $$Events = {
		change: CustomEvent<{ value: string }>;
	};
</script>

<svelte:body onclick={handleClickOutside} />

<div class="relative {className} {classAttr}" class:is-open={isOpen} bind:this={rootEl}>
	<button
		type="button"
		class="w-full bg-white/90 dark:bg-slate-800/70 backdrop-blur-sm border border-gray-300 dark:border-slate-600 rounded-md shadow-sm hover:shadow focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 pl-3 pr-10 py-2 text-left cursor-default sm:text-sm transition-colors"
		aria-haspopup="listbox"
		aria-expanded={isOpen}
		onclick={toggleDropdown}
		{id}
	>
		<span class="flex items-center">
			{#if selectedItem}
				<SpriteIcon
					game={game || ''}
					value={selectedItem.value}
					name={selectedItem.name}
					imagePath={selectedItem.imagePath}
					className="flex-shrink-0 h-8 w-8 rounded-md inline-block mr-3 ring-1 ring-black/10 dark:ring-white/10"
					eager={true}
				/>
				<span class="block truncate text-slate-800 dark:text-slate-100">{selectedItem.name}</span>
			{:else if items && items.length > 0}
				<span class="block truncate text-slate-500 dark:text-slate-400">{placeholder}</span>
			{:else}
				<span class="block truncate text-slate-400">{placeholder} (No items)</span>
			{/if}
		</span>
		<span class="ml-3 absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
			<svg
				class="h-5 w-5 text-slate-400 dark:text-slate-500"
				xmlns="http://www.w3.org/2000/svg"
				viewBox="0 0 20 20"
				fill="currentColor"
				aria-hidden="true"
			>
				<path
					fill-rule="evenodd"
					d="M10 3a1 1 0 01.707.293l3 3a1 1 0 01-1.414 1.414L10 5.414 7.707 7.707a1 1 0 01-1.414-1.414l3-3A1 1 0 0110 3zm-3.707 9.293a1 1 0 011.414 0L10 14.586l2.293-2.293a1 1 0 011.414 1.414l-3 3a1 1 0 01-1.414 0l-3-3a1 1 0 010-1.414z"
					clip-rule="evenodd"
				/>
			</svg>
		</span>
	</button>

	{#if isOpen && items && items.length > 0}
		<ul
			class="absolute z-50 mt-1 w-full bg-white/95 dark:bg-slate-800/95 shadow-lg shadow-indigo-900/10 max-h-60 rounded-md py-1 text-base ring-1 ring-black/5 dark:ring-white/10 overflow-auto focus:outline-none sm:text-sm backdrop-blur border border-indigo-500/20"
			tabindex="-1"
			role="listbox"
			aria-labelledby={id || 'sprite-select-label'}
		>
			{#each items as item (item.value)}
				<li
					class="cursor-default select-none relative py-2 pl-3 pr-9 text-slate-800 dark:text-slate-100 hover:bg-indigo-600 hover:text-white transition-colors {value ===
					item.value
						? 'bg-indigo-50 dark:bg-indigo-600/30 text-indigo-700 dark:text-indigo-200'
						: ''}"
					role="option"
					aria-selected={value === item.value}
					onclick={() => selectItem(item.value)}
					onkeydown={(e) => {
						if (e.key === 'Enter' || e.key === ' ') selectItem(item.value);
					}}
				>
					<div class="flex items-center">
						<SpriteIcon
							game={game || ''}
							value={item.value}
							name={item.name}
							imagePath={item.imagePath}
							className="flex-shrink-0 h-8 w-8 rounded-md inline-block mr-3 ring-1 ring-black/10 dark:ring-white/10"
							eager={true}
						/>
						<span class="font-normal block truncate {value === item.value ? 'font-semibold' : ''}"
							>{item.name}</span
						>
					</div>

					{#if value === item.value}
						<span
							class="text-indigo-600 dark:text-indigo-300 absolute inset-y-0 right-0 flex items-center pr-4"
						>
							<svg
								class="h-5 w-5"
								xmlns="http://www.w3.org/2000/svg"
								viewBox="0 0 20 20"
								fill="currentColor"
								aria-hidden="true"
							>
								<path
									fill-rule="evenodd"
									d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
									clip-rule="evenodd"
								/>
							</svg>
						</span>
					{/if}
				</li>
			{/each}
		</ul>
	{/if}
</div>
