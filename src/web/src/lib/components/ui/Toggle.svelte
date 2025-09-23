<script lang="ts">
	import { createEventDispatcher } from 'svelte';

	interface Props {
		id?: string;
		label?: string;
		checked?: boolean;
		disabled?: boolean;
		size?: 'sm' | 'md';
	}

	let {
		id = crypto.randomUUID(),
		label = '',
		checked = $bindable(false),
		disabled = false,
		size = 'md',
		class: classAttr = ''
	}: Props & { class?: string } = $props<Props & { class?: string }>();

	const dims = size === 'sm'
		? { outer: 'w-9 h-5', circle: 'h-4 w-4', translate: 'translate-x-4' }
		: { outer: 'w-11 h-6', circle: 'h-5 w-5', translate: 'translate-x-5' };

	const dispatch = createEventDispatcher<{
		change: { checked: boolean };
		input: { checked: boolean };
	}>();

	const labelClasses = $derived(
		`inline-flex items-center gap-2 cursor-pointer select-none ${classAttr}`.trim()
	);

	export type $$Events = {
		change: CustomEvent<{ checked: boolean }>;
		input: CustomEvent<{ checked: boolean }>;
	};

	function handleInput(event: Event) {
		checked = (event.target as HTMLInputElement).checked;
		dispatch('input', { checked });
	}

	function handleChange(event: Event) {
		checked = (event.target as HTMLInputElement).checked;
		dispatch('change', { checked });
	}
</script>

<label class={labelClasses}>
	<input
		{id}
		type="checkbox"
		class="sr-only peer"
		role="switch"
		aria-checked={checked}
		bind:checked
		{disabled}
		oninput={handleInput}
		onchange={handleChange}
	/>
	<div
		class={`relative ${dims.outer} rounded-full transition-colors duration-200 bg-slate-300 dark:bg-slate-600 peer-checked:bg-indigo-600 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-indigo-500 peer-focus:ring-offset-2 peer-disabled:opacity-50 peer-disabled:cursor-not-allowed`}
		aria-hidden="true"
	>
		<span
			class={`pointer-events-none absolute top-1/2 -translate-y-1/2 transition-transform duration-200 ${dims.circle} rounded-full bg-white shadow ${checked ? dims.translate : 'translate-x-0.5'}`}
		></span>
	</div>
	{#if label}
		<span class="text-xs text-slate-700 dark:text-slate-200">{label}</span>
	{/if}
</label>

<style>
	/* no extra styles; all utility */
</style>
