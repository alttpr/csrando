<script lang="ts">
	import * as m from '$lib/paraglide/messages';
	import Card from '../ui/Card.svelte';
	import Button from '../ui/Button.svelte';

	interface Props {
		id: string;
		createdAt: string;
		options: {
			games?: string[];
			settings?: { global?: Record<string, unknown>; perGame?: Record<string, unknown> };
		};
	}

	let { id, createdAt, options }: Props = $props();

	let gamesList = $derived(options?.games || []);
	let globalSettings = $derived(options?.settings?.global || {});
	let perGameSettings = $derived(options?.settings?.perGame || {});

	function formatDate(dateString: string): string {
		try {
			const date = new Date(dateString);
			return new Intl.DateTimeFormat('en-US', {
				dateStyle: 'medium',
				timeStyle: 'short'
			}).format(date);
		} catch {
			return dateString;
		}
	}
</script>

<Card title={m.seed_info_title()} className="mb-6">
	<div class="space-y-4">
		<div>
			<h3 class="font-bold text-slate-800 dark:text-slate-200 mb-1">{m.seed_id_label()}</h3>
			<p class="text-slate-700 dark:text-slate-300 font-mono">{id}</p>
		</div>

		<div>
			<h3 class="font-bold text-slate-800 dark:text-slate-200 mb-1">{m.seed_created_label()}</h3>
			<p class="text-slate-700 dark:text-slate-300">{formatDate(createdAt)}</p>
		</div>

		<div>
			<h3 class="font-bold text-slate-800 dark:text-slate-200 mb-1">{m.seed_games_label()}</h3>
			{#if gamesList.length > 0}
				<ul class="list-disc list-inside text-slate-700 dark:text-slate-300">
					{#each gamesList as game (game)}
						<li>{game}</li>
					{/each}
				</ul>
			{:else}
				<p class="text-slate-500 dark:text-slate-400">{m.seed_no_games()}</p>
			{/if}
		</div>

		{#if Object.keys(globalSettings).length > 0}
			<div>
				<h3 class="font-bold text-slate-800 dark:text-slate-200 mb-1">
					{m.seed_global_settings_label()}
				</h3>
				<div class="bg-slate-100 dark:bg-slate-700 p-3 rounded-md">
					<pre class="text-xs overflow-x-auto">{JSON.stringify(globalSettings, null, 2)}</pre>
				</div>
			</div>
		{/if}

		{#if Object.keys(perGameSettings).length > 0}
			<div>
				<h3 class="font-bold text-slate-800 dark:text-slate-200 mb-1">
					{m.seed_game_settings_label()}
				</h3>
				<div class="bg-slate-100 dark:bg-slate-700 p-3 rounded-md max-h-60 overflow-y-auto">
					<pre class="text-xs overflow-x-auto">{JSON.stringify(perGameSettings, null, 2)}</pre>
				</div>
			</div>
		{/if}

		<Button href="/config" variant="secondary" className="w-full">
			{m.seed_create_new_seed_button()}
		</Button>
	</div>
</Card>
