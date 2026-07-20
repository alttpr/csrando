<script lang="ts">
	interface Props {
		idPrefix?: string;
		items: Record<string, string>;
		selected?: string[];
		isOptionsFor?: boolean;
	}

	let {
		idPrefix = '',
		items,
		selected = $bindable<string[]>([]),
		isOptionsFor = false
	}: Props = $props();

	$effect(() => {
		// Auxiliary random-choice fields start with every option enabled, but a
		// non-empty selection may have come from a loaded preset and must not be
		// overwritten when this field mounts.
		if (isOptionsFor && selected.length === 0) {
			selected = Object.keys(items);
		}
	});

	function toggleKey(key: string, checked: boolean) {
		if (checked) {
			selected = [...selected, key];
		} else {
			selected = selected.filter((k) => k !== key);
		}
	}
</script>

<div class="flex flex-row flex-wrap gap-x-4 gap-y-1">
	{#each Object.entries(items) as [key, label] (key)}
		<label class="inline-flex items-center text-xs">
			<input
				type="checkbox"
				id={(idPrefix || 'multi') + '-' + key}
				checked={selected?.includes(key)}
				oninput={(e) => toggleKey(key, (e.target as HTMLInputElement).checked)}
				class="form-checkbox h-4 w-4 text-indigo-600 border-slate-300 rounded focus:ring-indigo-500"
			/>
			<span class="ml-2">{label}</span>
		</label>
	{/each}
</div>
