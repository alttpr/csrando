<script lang="ts">
	import Badge from "$lib/components/ui/Badge.svelte";

	interface Props {
		data: {
			account: {
				username: string;
				loginMethod: string;
				isAdmin: boolean;
			};
			stats: { seeds: number; profiles: number; apiKeys: number };
		};
	}

	let { data }: Props = $props();

	const cards = [
		{
			href: "/profile/seeds",
			label: "Generated seeds",
			value: data.stats.seeds,
			description: "Browse and reopen your generated seeds.",
		},
		{
			href: "/profile/profiles",
			label: "Saved profiles",
			value: data.stats.profiles,
			description: "Manage reusable randomizer configurations.",
		},
		{
			href: "/profile/api-keys",
			label: "Active API keys",
			value: data.stats.apiKeys,
			description: "Control access for bots and external tools.",
		},
	];
</script>

<section aria-labelledby="overview-heading">
	<div class="mb-5 flex flex-wrap items-center gap-2">
		<h2 id="overview-heading" class="text-xl font-semibold text-slate-900 dark:text-slate-100">
			Overview
		</h2>
		{#if data.account.isAdmin}<Badge variant="info">Administrator</Badge>{/if}
	</div>

	<div class="grid gap-3 sm:grid-cols-3">
		{#each cards as card (card.href)}
			<a
				href={card.href}
				class="rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition hover:border-indigo-300 hover:shadow-md dark:border-slate-700 dark:bg-slate-800 dark:hover:border-indigo-500"
			>
				<p class="text-xs font-medium text-slate-500 dark:text-slate-400">{card.label}</p>
				<p class="mt-1 text-3xl font-semibold text-slate-900 dark:text-slate-100">{card.value}</p>
				<p class="mt-2 text-sm text-slate-600 dark:text-slate-300">{card.description}</p>
			</a>
		{/each}
	</div>

	<div class="mt-5 rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-800">
		<h3 class="font-semibold text-slate-900 dark:text-slate-100">Account details</h3>
		<dl class="mt-3 grid gap-3 text-sm sm:grid-cols-2">
			<div>
				<dt class="text-slate-500 dark:text-slate-400">Username</dt>
				<dd class="font-medium text-slate-900 dark:text-slate-100">{data.account.username}</dd>
			</div>
			<div>
				<dt class="text-slate-500 dark:text-slate-400">Login method</dt>
				<dd class="font-medium text-slate-900 dark:text-slate-100">{data.account.loginMethod}</dd>
			</div>
		</dl>
	</div>
</section>
