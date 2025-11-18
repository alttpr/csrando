<script lang="ts">
	interface SelectProps {
		onchange?: (value: string) => void;
		items: { value: string; name: string }[];
		value: string;
		id?: string;
		label?: string;
		className?: string;
		required?: boolean;
		disabled?: boolean;
		placeholder?: string;
	}

	let {
		onchange = undefined,
		items = [],
		value = $bindable(''),
		id = undefined,
		label = undefined,
		className = '',
		required = false,
		disabled = false,
		class: classAttr = '',
		placeholder = undefined,
		...otherProps
	}: SelectProps & { class?: string } = $props<SelectProps & { class?: string }>();

	function handleChange(event: Event) {
		const target = event.target as HTMLSelectElement;
		value = target.value;
		onchange?.(value);
	}
</script>

{#if label}
	<label for={id} class="block mb-2 text-sm font-medium text-slate-900 dark:text-slate-100">
		{label}
	</label>
{/if}

<div class="relative">
	<select
		{id}
		{required}
		{disabled}
		bind:value
		onchange={handleChange}
		class="custom-select bg-slate-50 border border-slate-300 text-slate-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 pr-10 dark:bg-slate-700 dark:border-slate-600 dark:placeholder-slate-400 dark:text-slate-100 dark:focus:ring-primary-500 dark:focus:border-primary-500 {className} {classAttr}"
		{...otherProps}
	>
		{#if placeholder}
			<option value="">{placeholder}</option>
		{/if}
		{#each items as item (item.value)}
			<option value={item.value}>{item.name}</option>
		{/each}
	</select>
	<div
		class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-slate-700 dark:text-slate-200"
	>
		<svg
			class="h-4 w-4"
			xmlns="http://www.w3.org/2000/svg"
			viewBox="0 0 20 20"
			fill="currentColor"
			aria-hidden="true"
		>
			<path
				fill-rule="evenodd"
				d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
				clip-rule="evenodd"
			/>
		</svg>
	</div>
</div>

<style>
	/* Remove default select arrows in various browsers */
	select::-ms-expand {
		display: none;
	}

	select.custom-select {
		-webkit-appearance: none !important;
		-moz-appearance: none !important;
		appearance: none !important;
		background-image: none !important;
	}
</style>
