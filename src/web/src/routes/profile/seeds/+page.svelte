<script lang="ts">
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";

	interface SeedRow {
		id: string;
		options: unknown;
		createdAt: string | null;
		presetId: string | null;
		differedFromRevision: boolean | null;
	}

	interface Props {
		data: { seeds: SeedRow[]; page: number; pageSize: number; total: number };
	}

	let { data }: Props = $props();
	const pageCount = $derived(Math.max(1, Math.ceil(data.total / data.pageSize)));
</script>

<svelte:head><title>Account: seeds</title></svelte:head>

<section aria-labelledby="seeds-heading">
	<div class="mb-4 flex flex-wrap items-end justify-between gap-2">
		<div>
			<h2 id="seeds-heading" class="text-xl font-semibold text-slate-900 dark:text-slate-100">Generated seeds</h2>
			<p class="mt-1 text-sm text-slate-600 dark:text-slate-400">{data.total} seed{data.total === 1 ? "" : "s"} associated with your account.</p>
		</div>
		<Button href="/config/combo" variant="primary" size="sm">Generate a seed</Button>
	</div>

	{#if data.seeds.length === 0}
		<div class="rounded-lg border border-dashed border-slate-300 p-8 text-center dark:border-slate-700">
			<p class="text-slate-600 dark:text-slate-300">You have not generated any seeds yet.</p>
		</div>
	{:else}
		<div class="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800">
			<ul class="divide-y divide-slate-200 dark:divide-slate-700">
				{#each data.seeds as seed (seed.id)}
					<li class="flex flex-wrap items-center justify-between gap-3 px-4 py-3">
						<div class="min-w-0">
							<a href={`/seed/${seed.id}`} class="break-all font-mono text-sm font-semibold text-indigo-600 hover:underline dark:text-indigo-400">{seed.id}</a>
							<p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
								{seed.createdAt ? new Date(seed.createdAt).toLocaleString() : "Unknown date"}
							</p>
						</div>
						<div class="flex items-center gap-2">
							{#if seed.presetId}
								<Badge variant={seed.differedFromRevision ? "warning" : "neutral"}>
									{seed.differedFromRevision ? "Modified preset" : "Preset"}
								</Badge>
							{/if}
							<Button href={`/seed/${seed.id}`} variant="secondary" size="xs">Open</Button>
						</div>
					</li>
				{/each}
			</ul>
		</div>

		{#if pageCount > 1}
			<nav class="mt-4 flex items-center justify-between" aria-label="Seed pages">
				{#if data.page > 0}
					<Button href={`/profile/seeds?page=${data.page - 1}`} variant="secondary" size="sm">Previous</Button>
				{:else}
					<span class="px-3 py-1.5 text-sm text-slate-400">Previous</span>
				{/if}
				<span class="text-sm text-slate-600 dark:text-slate-400">Page {data.page + 1} of {pageCount}</span>
				{#if data.page + 1 < pageCount}
					<Button href={`/profile/seeds?page=${data.page + 1}`} variant="secondary" size="sm">Next</Button>
				{:else}
					<span class="px-3 py-1.5 text-sm text-slate-400">Next</span>
				{/if}
			</nav>
		{/if}
	{/if}
</section>
