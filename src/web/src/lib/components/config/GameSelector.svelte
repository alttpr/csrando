<script lang="ts">
	import * as m from '$lib/paraglide/messages';
	interface Props {
		games?: Array<{ id: string; name: string; description?: string }>;
		selectedGames?: string[];
		loading?: boolean;
		requiredGames?: string[];
	}

	let {
		games = [],
		selectedGames = $bindable([]),
		loading = false,
		requiredGames = []
	}: Props = $props();

	function toggleGameSelection(gameId: string) {
		if (requiredGames.includes(gameId)) return;
		selectedGames = selectedGames.includes(gameId)
			? selectedGames.filter((id) => id !== gameId)
			: [...selectedGames, gameId];
	}
</script>

{#if games.length >= 2}
	<div class="bg-white dark:bg-slate-800 rounded-lg shadow-md p-3 mb-3">
		<h2 class="text-lg font-bold mb-1.5">{m.config_game_selection_title()}</h2>
		<p class="mb-2 text-xs text-slate-600 dark:text-slate-400">
			{m.config_game_selection_description()}
		</p>

		{#if loading}
			<div class="flex justify-center my-3">
				<div
					class="animate-spin rounded-full h-6 w-6 border-t-2 border-b-2 border-indigo-500"
				></div>
			</div>
		{:else}
			<div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-1.5 mb-2">
				{#each games as game (game.id)}
					{@const isSelected = selectedGames.includes(game.id)}
					<button
						type="button"
						class="border rounded-md p-1.5 cursor-pointer transition-all text-xs
						{isSelected
							? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20'
							: 'border-slate-200 dark:border-slate-700 hover:border-indigo-300 dark:hover:border-indigo-700'}"
						onclick={() => toggleGameSelection(game.id)}
						aria-pressed={isSelected}
					>
						<div class="flex items-start space-x-1.5">
							<input
								type="checkbox"
								class="mt-0.5 h-3 w-3 pointer-events-none"
								checked={isSelected}
								tabindex="-1"
								aria-hidden="true"
							/>
							<div>
								<span class="font-medium block text-xs">{game.name}</span>
								{#if game.description}
									<p class="text-[10px] text-slate-600 dark:text-slate-400 mt-0.5">
										{game.description}
									</p>
								{/if}
							</div>
						</div>
					</button>
				{/each}
			</div>
		{/if}
	</div>
{/if}
