<script lang="ts">
	interface Props {
		title?: string | undefined;
		subtitle?: string | undefined;
		className?: string;
		children?: import('svelte').Snippet;
	}

	let {
		title = undefined,
		subtitle = undefined,
		className = '',
		class: classAttr = '',
		children
	}: Props & { class?: string } = $props<Props & { class?: string }>();
</script>

<div class="bg-white dark:bg-slate-800 rounded-lg shadow-md p-6 mb-6 {className} {classAttr}">
	{#if title}
		<h2 class="text-2xl font-bold mb-2 text-slate-900 dark:text-slate-100">{title}</h2>
		{#if subtitle}
			<p class="mb-4 text-slate-600 dark:text-slate-400">{subtitle}</p>
		{/if}
	{/if}
	{@render children?.()}
</div>
