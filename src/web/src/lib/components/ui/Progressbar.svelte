<script lang="ts">
	interface Props {
		progress?: number;
		color?: 'primary' | 'yellow' | 'red';
		size?: string;
		labelInside?: boolean;
		className?: string;
	}

	let {
		progress = 0,
		color = 'primary',
		size = 'h-4',
		labelInside = false,
		className = '',
		class: classAttr = ''
	}: Props & { class?: string } = $props();

	let progressValue = $derived(Math.min(Math.max(0, progress), 100));

	let colorClasses = $derived(
		{
			primary: 'bg-indigo-500 dark:bg-indigo-600',
			yellow: 'bg-yellow-500 dark:bg-yellow-600',
			red: 'bg-red-500 dark:bg-red-600'
		}[color]
	);
</script>

<div class="w-full bg-slate-200 dark:bg-slate-700 rounded-full {size} {className} {classAttr}">
	<div
		class="{colorClasses} rounded-full transition-all duration-300 {size} relative"
		style="width: {progressValue}%"
	>
		{#if labelInside && progressValue > 10}
			<div class="absolute inset-0 flex items-center justify-center text-white text-xs font-medium">
				{progressValue}%
			</div>
		{/if}
	</div>
</div>
{#if !labelInside && progressValue !== undefined}
	<div class="text-xs text-slate-600 dark:text-slate-400 mt-1 text-right">
		{progressValue}%
	</div>
{/if}
