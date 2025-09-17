<script lang="ts">
	interface Item {
		value: string;
		name: string;
	}
	interface Props {
		id: string;
		items: Item[];
		value?: string;
		className?: string;
	}

	let {
		id,
		items,
		value = $bindable(undefined as string | undefined),
		className = ''
	}: Props = $props();
</script>

<select
	{id}
	class={className +
		' block w-full border border-slate-300 dark:border-slate-600 rounded-md shadow-sm py-1 px-2 text-xs bg-white dark:bg-slate-700 text-slate-700 dark:text-white focus:outline-none focus:ring-indigo-500 focus:border-indigo-500'}
	bind:value
	onchange={(e) => (value = (e.target as HTMLSelectElement).value)}
>
	{#each items as item (item.value)}
		<option value={item.value}>{item.name}</option>
	{/each}
</select>
