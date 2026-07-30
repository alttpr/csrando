<script lang="ts">
	import type { SelectPostGenSetting } from "$lib/types";

	interface Props {
		gameId: string;
		gameName: string;
		options: SelectPostGenSetting[];
		selections: Record<string, string | boolean>;
		onchange: (optionId: string, value: string) => void;
		onrestore: () => void;
	}

	let {
		gameId,
		gameName,
		options,
		selections,
		onchange,
		onrestore,
	}: Props = $props();

	function selectedButton(option: SelectPostGenSetting) {
		const selected = selections[option.id];
		return typeof selected === "string" ? selected : option.default;
	}

	function assignedToAnother(optionId: string, button: string) {
		return options.find(
			(option) =>
				option.id !== optionId && selectedButton(option) === button,
		);
	}
</script>

<details
	class="group/controllers overflow-hidden rounded-md border border-slate-200 bg-white/60 md:col-span-2 dark:border-slate-700 dark:bg-slate-800/50"
>
	<summary
		class="flex cursor-pointer list-none items-center justify-between gap-3 px-3 py-2.5 marker:hidden"
	>
		<div>
			<h5 class="text-xs font-semibold text-slate-900 dark:text-slate-100">
				Controller mapping
			</h5>
			<p class="text-[11px] text-slate-500 dark:text-slate-400">
				Standard {gameName} controls are selected by default.
			</p>
		</div>
		<span
			class="text-indigo-600 transition-transform group-open/controllers:rotate-90 dark:text-indigo-300"
			aria-hidden="true">›</span
		>
	</summary>

	<div class="space-y-3 border-t border-slate-200 p-3 dark:border-slate-700">
		<div class="flex justify-end">
			<button
				type="button"
				class="text-[11px] font-medium text-indigo-600 hover:text-indigo-700 hover:underline dark:text-indigo-300 dark:hover:text-indigo-200"
				onclick={onrestore}
			>
				Restore defaults
			</button>
		</div>

			{#each options as option (option.id)}
				<div
					class="grid grid-cols-[7rem_minmax(0,1fr)] items-center gap-3"
				>
					<span
						class="text-right text-xs font-medium text-slate-900 dark:text-slate-100"
					>
						{option.name}
					</span>
					<div class="flex flex-wrap justify-start gap-1" role="group">
					{#each option.choices as choice (choice.value)}
						{@const selected = selectedButton(option) === choice.value}
						{@const assignedTo =
							!selected &&
							assignedToAnother(option.id, choice.value)}
						<button
							id={`controller-${gameId}-${option.id}-${choice.value}`}
							type="button"
							class={`min-w-8 rounded border px-2 py-1 text-[11px] font-semibold transition-colors ${
								selected
									? "border-indigo-600 bg-indigo-600 text-white dark:border-indigo-400 dark:bg-indigo-500"
									: "border-slate-300 bg-white text-slate-700 hover:border-indigo-400 hover:text-indigo-700 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200 dark:hover:border-indigo-400 dark:hover:text-indigo-200"
							}`}
							aria-pressed={selected}
							aria-label={`Map ${option.name} to ${choice.label}`}
							title={assignedTo
								? `Swap with ${assignedTo.name}`
								: undefined}
							onclick={() => onchange(option.id, choice.value)}
						>
							{choice.label}
						</button>
					{/each}
				</div>
			</div>
		{/each}
	</div>
</details>
