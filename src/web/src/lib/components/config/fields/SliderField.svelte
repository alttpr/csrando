<script lang="ts">
	interface Props {
		id: string;
		min?: number;
		max?: number;
		step?: number;
		value?: number;
	}

	let { id, min = 0, max = 100, step = 1, value = $bindable(0) }: Props = $props();

	function onInput(e: Event) {
		const num = parseFloat((e.target as HTMLInputElement).value);
		if (!Number.isNaN(num)) value = num;
	}
</script>

<div class="flex items-center space-x-3">
	<input
		type="range"
		{id}
		{min}
		{max}
		{step}
		{value}
		oninput={onInput}
		class="w-full h-1 bg-slate-200 dark:bg-slate-600 rounded-lg appearance-none cursor-pointer"
	/>
	<span class="text-xs font-medium">{value}</span>
</div>
