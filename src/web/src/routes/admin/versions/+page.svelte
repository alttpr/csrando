<script lang="ts">
	import { enhance } from "$app/forms";
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";

	interface AdminVersionRow {
		id: string;
		versionTag: string;
		randomizerId: string | null;
		isActive: boolean;
		createdAt: string | null;
		buildDate: string | null;
		gitCommitHash: string | null;
	}

	interface Props {
		data: { versions: AdminVersionRow[] };
		form?: { message?: string } | null;
	}

	let { data, form = null }: Props = $props();
</script>

<svelte:head>
	<title>Admin: versions</title>
</svelte:head>

<div class="mb-4 flex flex-wrap items-center justify-between gap-2">
	<h1 class="text-2xl font-bold text-primary-600 dark:text-primary-400">
		Randomizer versions
	</h1>
	<Button href="/admin/new-version" variant="primary" size="sm">
		Upload new version
	</Button>
</div>

{#if form?.message}
	<div
		class="mb-3 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-800 dark:border-red-700 dark:bg-red-900 dark:text-red-200"
		role="alert"
	>
		{form.message}
	</div>
{/if}

<p class="mb-2 text-xs text-slate-500 dark:text-slate-400">
	New seeds are generated with the active version of each randomizer;
	existing permalinks keep the version they were generated with.
</p>

<div
	class="overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800"
>
	<table class="w-full text-left text-sm">
		<thead
			class="border-b border-slate-200 text-xs uppercase tracking-wide text-slate-500 dark:border-slate-700 dark:text-slate-400"
		>
			<tr>
				<th class="px-3 py-2">Tag</th>
				<th class="px-3 py-2">Randomizer</th>
				<th class="px-3 py-2">Build</th>
				<th class="px-3 py-2">Status</th>
				<th class="px-3 py-2 text-right">Actions</th>
			</tr>
		</thead>
		<tbody>
			{#each data.versions as version (version.id)}
				<tr
					class="border-b border-slate-100 last:border-0 dark:border-slate-700/60"
				>
					<td
						class="px-3 py-2 font-mono text-slate-900 dark:text-slate-100"
					>
						{version.versionTag}
					</td>
					<td class="px-3 py-2 text-slate-600 dark:text-slate-300">
						{version.randomizerId ?? "—"}
					</td>
					<td class="px-3 py-2 text-xs text-slate-600 dark:text-slate-300">
						{version.buildDate
							? new Date(version.buildDate).toLocaleString()
							: "—"}
						{#if version.gitCommitHash}
							<span class="ml-1 font-mono">
								{version.gitCommitHash.slice(0, 10)}
							</span>
						{/if}
					</td>
					<td class="px-3 py-2">
						{#if version.isActive}
							<Badge variant="success" dot>Active</Badge>
						{:else}
							<Badge variant="neutral">Inactive</Badge>
						{/if}
					</td>
					<td class="px-3 py-2 text-right">
						{#if version.isActive}
							<form
								method="POST"
								action="?/deactivate"
								class="inline"
								use:enhance
							>
								<input
									type="hidden"
									name="versionId"
									value={version.id}
								/>
								<Button type="submit" variant="secondary" size="xs">
									Deactivate
								</Button>
							</form>
						{:else}
							<form
								method="POST"
								action="?/activate"
								class="inline"
								use:enhance
							>
								<input
									type="hidden"
									name="versionId"
									value={version.id}
								/>
								<Button type="submit" variant="secondary" size="xs">
									Make active
								</Button>
							</form>
						{/if}
					</td>
				</tr>
			{:else}
				<tr>
					<td
						class="px-3 py-4 text-center text-slate-500 dark:text-slate-400"
						colspan="5"
					>
						No versions uploaded yet.
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
