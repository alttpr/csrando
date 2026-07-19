<script lang="ts">
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";

	interface AdminSeedRow {
		id: string;
		createdAt: string | null;
		versionTag: string | null;
		raceMode: boolean;
		presetId: string | null;
		differedFromRevision: boolean | null;
	}

	interface Props {
		data: {
			seeds: AdminSeedRow[];
			total: number;
			page: number;
			pageSize: number;
			query: string;
		};
	}

	let { data }: Props = $props();

	const lastPage = $derived(
		Math.max(0, Math.ceil(data.total / data.pageSize) - 1),
	);

	function pageHref(page: number): string {
		const params = new URLSearchParams();
		if (data.query) params.set("q", data.query);
		if (page > 0) params.set("page", String(page));
		const qs = params.toString();
		return qs ? `/admin/seeds?${qs}` : "/admin/seeds";
	}
</script>

<svelte:head>
	<title>Admin: seeds</title>
</svelte:head>

<h1 class="mb-4 text-2xl font-bold text-primary-600 dark:text-primary-400">
	Seeds
</h1>

<form method="GET" class="mb-3 flex items-center gap-2">
	<input
		type="search"
		name="q"
		value={data.query}
		placeholder="Search by seed id"
		class="w-full max-w-xs rounded-md border border-slate-300 bg-white px-3 py-1.5 font-mono text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
	/>
	<Button type="submit" variant="secondary" size="xs">Search</Button>
</form>

<p class="mb-2 text-xs text-slate-500 dark:text-slate-400">
	{data.total} seed{data.total === 1 ? "" : "s"}
	{#if data.total > data.pageSize}
		&middot; page {data.page + 1} of {lastPage + 1}
	{/if}
</p>

<div
	class="overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800"
>
	<table class="w-full text-left text-sm">
		<thead
			class="border-b border-slate-200 text-xs uppercase tracking-wide text-slate-500 dark:border-slate-700 dark:text-slate-400"
		>
			<tr>
				<th class="px-3 py-2">Seed</th>
				<th class="px-3 py-2">Created</th>
				<th class="px-3 py-2">Version</th>
				<th class="px-3 py-2">Flags</th>
				<th class="px-3 py-2 text-right">Actions</th>
			</tr>
		</thead>
		<tbody>
			{#each data.seeds as seed (seed.id)}
				<tr
					class="border-b border-slate-100 last:border-0 dark:border-slate-700/60"
				>
					<td class="px-3 py-2">
						<a
							href={`/seed/${seed.id}`}
							class="font-mono text-primary-600 hover:underline dark:text-primary-400"
						>
							{seed.id}
						</a>
					</td>
					<td class="px-3 py-2 text-slate-600 dark:text-slate-300">
						{seed.createdAt
							? new Date(seed.createdAt).toLocaleString()
							: "—"}
					</td>
					<td
						class="px-3 py-2 font-mono text-xs text-slate-600 dark:text-slate-300"
					>
						{seed.versionTag ?? "—"}
					</td>
					<td class="px-3 py-2">
						<span class="flex flex-wrap gap-1">
							{#if seed.raceMode}
								<Badge variant="warning">Race</Badge>
							{/if}
							{#if seed.presetId}
								<Badge variant="info">Preset</Badge>
							{/if}
							{#if seed.differedFromRevision}
								<Badge variant="neutral">Modified</Badge>
							{/if}
						</span>
					</td>
					<td class="px-3 py-2 text-right">
						<Button
							href={`/admin/seed/${seed.id}`}
							variant="secondary"
							size="xs"
						>
							Details{seed.raceMode ? " / spoiler" : ""}
						</Button>
					</td>
				</tr>
			{:else}
				<tr>
					<td
						class="px-3 py-4 text-center text-slate-500 dark:text-slate-400"
						colspan="5"
					>
						No seeds match this search.
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

{#if data.total > data.pageSize}
	<div class="mt-3 flex items-center gap-2">
		<Button
			href={pageHref(Math.max(0, data.page - 1))}
			variant="secondary"
			size="xs"
			disabled={data.page === 0}
		>
			Previous
		</Button>
		<Button
			href={pageHref(Math.min(lastPage, data.page + 1))}
			variant="secondary"
			size="xs"
			disabled={data.page >= lastPage}
		>
			Next
		</Button>
	</div>
{/if}
