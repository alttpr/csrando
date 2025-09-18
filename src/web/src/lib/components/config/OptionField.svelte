<script lang="ts">
	import type { MetadataSetting } from '$lib/types';
	import ToggleField from './fields/ToggleField.svelte';
	import SelectField from './fields/SelectField.svelte';
	import SliderField from './fields/SliderField.svelte';
	import MultiChoiceField from './fields/MultiChoiceField.svelte';
	import TextInputField from './fields/TextInputField.svelte';

	import Self from './OptionField.svelte';

	interface Props {
		option: MetadataSetting;
		values?: Record<string, unknown>;
		visibleOptions?: MetadataSetting[];
		depth?: number;
	}

	let {
		option,
		values = $bindable<Record<string, unknown>>({}),
		visibleOptions = [],
		depth = 0
	}: Props = $props();

	const hasOptionsFor = (o: MetadataSetting): o is MetadataSetting & { optionsFor: string } =>
		typeof (o as { optionsFor?: unknown }).optionsFor === 'string';

	const children = $derived(
		(visibleOptions || []).filter(
			(o) => o.dependsOn?.key === option.key || (hasOptionsFor(o) && o.optionsFor === option.key)
		)
	);

	const labelClass = $derived(
		depth === 0
			? 'block text-xs font-medium mb-0.5 text-slate-600 dark:text-slate-300'
			: 'block text-[11px] font-medium mb-0.5 text-slate-600 dark:text-slate-300'
	);
	const descriptionClass = $derived(
		depth === 0
			? 'text-xs text-slate-500 dark:text-slate-400 mb-1'
			: 'text-[11px] text-slate-500 dark:text-slate-400 mb-1'
	);
</script>

<div class="form-group">
	<label for={option.key} class={labelClass}>{option.name}</label>
	{#if option.description}
		<p class={descriptionClass}>{option.description}</p>
	{/if}

	{#if option.type === 'Toggle'}
		<ToggleField
			id={option.key}
			description={option.description || ''}
			bind:value={values[option.key] as boolean | undefined}
		/>
	{:else if option.type === 'SingleChoice'}
		<SelectField
			id={option.key}
			bind:value={values[option.key] as string | undefined}
			className="mt-1 text-sm py-1.5"
			items={Object.entries(option.values).map((opt) => ({ value: String(opt[1]), name: opt[0] }))}
		/>
	{:else if option.type === 'Slider'}
		<SliderField
			id={option.key}
			min={option.range.from ?? 0}
			max={option.range.to ?? 100}
			step={1}
			bind:value={values[option.key] as number | undefined}
		/>
	{:else if option.type === 'MultipleChoice'}
		<MultiChoiceField
			idPrefix={option.key}
			items={option.values}
			bind:selected={values[option.key] as string[] | undefined}
			isOptionsFor={!!option.optionsFor}
		/>
	{:else}
		<TextInputField
			id={option.key}
			type={(option.type as 'text' | 'number') || 'text'}
			bind:value={values[option.key] as string | number | undefined}
		/>
	{/if}
</div>

{#each children as child (child.key)}
	<div class="mt-2 pl-4 border-l-2 border-slate-200 dark:border-slate-700">
		<Self option={child} bind:values {visibleOptions} depth={depth + 1} />
	</div>
{/each}
