<script lang="ts">
	import { onMount } from 'svelte';
	import * as m from '$lib/paraglide/messages';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import type { Seed } from '$lib/types';
	import { fetchUserSeeds } from '$lib/services/data';

	let seeds: Seed[] = $state([]);
	let loading = $state(true);
	let error: string | null = $state(null);
	let deleteConfirm = $state('');
	let deleting = $state(false);

	onMount(async () => {
		if (!page.data.user) {
			// This should ideally be caught by +layout.server.ts which redirects.
			// If somehow the user lands here without being logged in, redirect to login.
			goto('/login');
			return;
		}
		loading = true;
		try {
			seeds = await fetchUserSeeds();
		} catch (e: unknown) {
			const message = (e as { message?: string })?.message || m.error_generic_server();
			if (message.toLowerCase().includes('unauthorized')) {
				error = m.error_unauthorized();
				goto('/login');
				return;
			}
			error = message;
			console.error('Failed to load user seeds:', e);
		} finally {
			loading = false;
		}
	});
</script>

<div class="container mx-auto px-4 py-8">
	<h1 class="text-3xl font-bold mb-6 text-primary-500 dark:text-primary-400">
		{m.profile_title()}
	</h1>

	{#if !page.data.user}
		<!-- This block might not be reached if onMount redirect works quickly -->
		<p class="text-lg text-slate-700 dark:text-slate-300">{m.profile_redirecting_login()}</p>
	{:else if loading}
		<p class="text-lg text-slate-700 dark:text-slate-300">{m.profile_loading_seeds()}</p>
	{:else if error}
		<div
			class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative dark:bg-red-700 dark:border-red-600 dark:text-red-200"
			role="alert"
		>
			<strong class="font-bold">{m.error_label()}</strong>
			<span class="block sm:inline">{error}</span>
		</div>
	{:else if seeds.length === 0}
		<p class="text-lg text-slate-700 dark:text-slate-300">{m.profile_no_seeds()}</p>
	{:else}
		<div class="space-y-4">
			{#each seeds as seed (seed.id)}
				<div
					class="bg-white dark:bg-slate-800 p-4 rounded-lg shadow hover:shadow-md transition-shadow"
				>
					<a
						href={`/seed/${seed.id}`}
						class="text-xl font-semibold text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 dark:hover:text-indigo-300"
					>
						{m.seed_id()}: {seed.id}
					</a>
					<p class="text-sm text-slate-500 dark:text-slate-400 mt-1">
						{m.seed_created_at()}: {new Date(seed.createdAt).toLocaleString()}
					</p>
					<details class="mt-2 text-sm">
						<summary
							class="cursor-pointer text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-gray-100"
							>{m.seed_options_show()}</summary
						>
						<pre
							class="mt-1 bg-gray-100 dark:bg-gray-700 p-3 rounded-md overflow-x-auto text-xs text-gray-800 dark:text-gray-200">{JSON.stringify(
								seed.options,
								null,
								2
							)}</pre>
					</details>
				</div>
			{/each}
		</div>
	{/if}
	{#if page.data.user && !loading && !error}
		<!-- Danger zone: account deletion -->
		<section class="mt-10 border-t border-slate-200 dark:border-slate-700 pt-6">
			<h2 class="text-xl font-semibold mb-2 text-red-600 dark:text-red-400">Delete your account</h2>
			<p class="text-sm text-slate-600 dark:text-slate-300 mb-3">
				This permanently deletes your profile and removes associations to your seeds. This action
				cannot be undone.
			</p>
			<form
				method="POST"
				action="?/deleteAccount"
				onsubmit={(e) => {
					if (deleteConfirm !== 'DELETE') {
						e.preventDefault();
					} else {
						deleting = true;
					}
				}}
			>
				<label class="block text-sm mb-2" for="deleteConfirm">
					Type <span class="font-mono">DELETE</span> to confirm
				</label>
				<input
					id="deleteConfirm"
					type="text"
					bind:value={deleteConfirm}
					placeholder="DELETE"
					class="w-full sm:w-64 px-3 py-2 rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 text-sm mb-3"
				/>
				<input type="hidden" name="confirm" value={deleteConfirm} />
				<button
					type="submit"
					disabled={deleteConfirm !== 'DELETE' || deleting}
					class="inline-flex items-center gap-2 px-4 py-2 rounded bg-red-600 text-white disabled:opacity-50 disabled:cursor-not-allowed hover:bg-red-700"
				>
					{deleting ? 'Deleting…' : 'Delete account'}
				</button>
			</form>
		</section>
	{/if}
</div>
