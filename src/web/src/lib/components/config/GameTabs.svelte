<script lang="ts">
	import type { GameSettings } from '$lib/types';

	import * as m from '$lib/paraglide/messages';
	import OptionForm from './OptionForm.svelte';

	interface Props {
		selectedGames?: string[];
		perGameOptions?: GameSettings;
		formValues: {
			perGame: { [gameKey: string]: { [key: string]: unknown } };
		};
		activeGameTab?: string | null;
		games?: Array<{ id: string; name: string }>;
		visibility?: Array<string>;
	}
	let {
		selectedGames = $bindable([]),
		perGameOptions = {},
		formValues = $bindable(),
		activeGameTab = $bindable(null),
		games = [],
		visibility = []
	}: Props = $props();

	$effect(() => {
		if (selectedGames.length === 0) {
			if (activeGameTab) {
				activeGameTab = null;
			}
			return;
		}

		if (!activeGameTab || !selectedGames.includes(activeGameTab)) {
			activeGameTab = selectedGames[0];
		}

		const missingGameEntries = selectedGames.filter(
			(gameId) => formValues.perGame[gameId] === undefined
		);

		if (missingGameEntries.length > 0) {
			const updatedPerGame = { ...formValues.perGame };
			for (const gameId of missingGameEntries) {
				updatedPerGame[gameId] = {};
			}
			formValues.perGame = updatedPerGame;
		}
	});

	function getGameName(gameId: string): string {
		const game = games.find((g) => g.id === gameId);
		return game ? game.name : gameId;
	}
</script>

{#if selectedGames.length > 0}
	<div class="bg-white dark:bg-slate-800 rounded-lg shadow-md p-3 mb-3">
		<h2 class="text-lg font-bold mb-2">{m.config_game_options_header()}</h2>

		<!-- Game tabs -->
		<div class="border-b border-slate-200 dark:border-slate-700 mb-2">
			<nav class="flex space-x-4 overflow-x-auto" aria-label="Game options">
				{#if selectedGames.length > 1}
					{#each selectedGames as gameId (gameId)}
						<button
							type="button"
							class="py-1.5 px-1 border-b-2 font-medium text-xs whitespace-nowrap
                  {activeGameTab === gameId
								? 'border-indigo-500 text-indigo-600 dark:text-indigo-400'
								: 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300 dark:text-slate-400 dark:hover:text-slate-300'}"
							aria-current={activeGameTab === gameId ? 'page' : undefined}
							onclick={() => (activeGameTab = gameId)}
						>
							{getGameName(gameId)}
						</button>
					{/each}
				{/if}
			</nav>
		</div>
		<!-- Active game options form -->
		{#if activeGameTab && perGameOptions[activeGameTab] && perGameOptions[activeGameTab].settings.length > 0}
			{#key activeGameTab}
				<OptionForm
					options={perGameOptions[activeGameTab].settings}
					bind:values={formValues.perGame[activeGameTab]}
					title={`${getGameName(activeGameTab)} ${m.config_options()}`}
					{visibility}
				/>
			{/key}
		{:else if activeGameTab}
			<div class="text-center p-2 text-slate-500 dark:text-slate-400 text-xs">
				{m.config_no_options_for_game({ gameName: getGameName(activeGameTab) })}
			</div>
		{/if}
	</div>
{/if}
