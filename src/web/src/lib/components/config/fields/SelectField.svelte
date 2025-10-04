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
		value = $bindable<string | undefined>(undefined),
		className = ''
	}: Props = $props();

	const baseClasses =
		'block w-full border border-slate-300 dark:border-slate-600 rounded-md shadow-sm py-1 px-2 text-xs bg-white dark:bg-slate-700 text-slate-700 dark:text-white focus:outline-none focus:ring-indigo-500 focus:border-indigo-500';
	const classes = $derived(`${baseClasses} ${className}`.trim());
</script>

<select
	{id}
	class={classes}
	bind:value
>
	{#each items as item (item.value)}
		<option value={item.value}>{item.name}</option>
	{/each}
</select>
