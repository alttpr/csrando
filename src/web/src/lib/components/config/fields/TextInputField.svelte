<script lang="ts">
	interface Props {
		id: string;
		type?: 'text' | 'number';
		value?: string | number;
	}

	let { id, type = 'text', value = $bindable<string | number>('') }: Props = $props();

	function onInput(e: Event) {
		const target = e.target as HTMLInputElement;
		if (type === 'number') {
			const num = parseFloat(target.value);
			if (!Number.isNaN(num)) value = num;
			else value = target.value;
		} else {
			value = target.value;
		}
	}
</script>

<input
	{id}
	{type}
	{value}
	oninput={onInput}
	class="block w-full border border-slate-300 dark:border-slate-600 rounded-md shadow-sm py-1 px-2 text-xs focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 bg-white dark:bg-slate-700 text-slate-700 dark:text-white"
/>
