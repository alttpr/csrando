<script lang="ts">
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

    let rootEl: HTMLElement | null = null;

	const dims =
		size === 'sm'
			? { outer: 'w-9 h-5', circle: 'h-4 w-4' }
			: { outer: 'w-11 h-6', circle: 'h-5 w-5' };

  export type $$Events = {
    change: CustomEvent<{ checked: boolean }>;
    input: CustomEvent<{ checked: boolean }>;
    click: MouseEvent;
  };
  import { createEventDispatcher } from 'svelte';

  const dispatch = createEventDispatcher<{ change: { checked: boolean }; input: { checked: boolean } }>();
</script>

<label class="inline-flex items-center gap-2 cursor-pointer select-none {classAttr}" bind:this={rootEl}>
	<input
		{id}
		type="checkbox"
		class="sr-only peer"
		role="switch"
		aria-checked={checked}
		{disabled}
		{checked}
        onchange={(e) => {
            e.stopPropagation();
            checked = (e.target as HTMLInputElement).checked;
            dispatch('change', { checked });
        }}
        oninput={(e) => {
            e.stopPropagation();
            checked = (e.target as HTMLInputElement).checked;
            dispatch('input', { checked });
        }}
    />
	<div
		class={`relative ${dims.outer} rounded-full transition-colors duration-200 bg-slate-300 dark:bg-slate-600 peer-checked:bg-indigo-600 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-indigo-500 peer-focus:ring-offset-2 peer-disabled:opacity-50 peer-disabled:cursor-not-allowed`}
		aria-hidden="true"
	>
		<span
			class={`pointer-events-none absolute top-1/2 -translate-y-1/2 transition-transform duration-200 ${dims.circle} rounded-full bg-white shadow ${checked ? (size === 'sm' ? 'translate-x-4' : 'translate-x-5') : 'translate-x-0.5'}`}
		></span>
	</div>
	{#if label}
		<span class="text-xs text-slate-700 dark:text-slate-200">{label}</span>
	{/if}
</label>

<style>
	/* no extra styles; all utility */
</style>
