<script lang="ts">
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import SpoilerLog from "$lib/components/seed/SpoilerLog.svelte";
	import { profileConfigPath } from "$lib/config/profile-links";

	interface Props {
		data: {
			seed: {
				id: string;
				createdAt: string | null;
				raceMode: boolean;
				spoilerLog: unknown;
				differedFromRevision: boolean | null;
			};
			attribution: {
				profileId: string | null;
				slug: string | null;
				name: string;
				scope: "official" | "user";
				configId: string;
				revisionNumber: number | null;
				deleted: boolean;
			} | null;
		};
	}

	let { data }: Props = $props();
	const seed = data.seed;
</script>

<svelte:head>
	<title>Admin: seed {seed.id}</title>
</svelte:head>

<h1 class="mb-1 text-2xl font-bold text-primary-600 dark:text-primary-400">
	Seed
	<span class="font-mono">{seed.id}</span>
</h1>

<div class="mb-3 flex flex-wrap items-center gap-2 text-sm">
	{#if seed.raceMode}
		<Badge variant="warning">Race seed — spoiler hidden publicly</Badge>
	{/if}
	{#if seed.createdAt}
		<span class="text-slate-600 dark:text-slate-300">
			Created {new Date(seed.createdAt).toLocaleString()}
		</span>
	{/if}
</div>

{#if data.attribution}
	<p class="mb-3 text-sm text-slate-600 dark:text-slate-300">
		Generated from profile
		{#if data.attribution.profileId}
			<a
				href={profileConfigPath(data.attribution) ?? undefined}
				class="text-primary-600 hover:underline dark:text-primary-400"
			>
				{data.attribution.name}
			</a>
		{:else}
			<span>{data.attribution.name}</span>
		{/if}
		{#if data.attribution.revisionNumber !== null}
			(revision {data.attribution.revisionNumber})
		{/if}
		{#if data.attribution.deleted}
			<Badge variant="neutral">deleted</Badge>
		{/if}
		{#if seed.differedFromRevision}
			<Badge variant="info">modified</Badge>
		{/if}
	</p>
{/if}

<div class="mb-4 flex gap-2">
	<Button href={`/seed/${seed.id}`} variant="primary" size="xs">
		Open public permalink
	</Button>
</div>

{#if seed.spoilerLog}
	<div class="rounded-lg bg-white p-4 shadow-lg dark:bg-slate-800">
		<SpoilerLog spoilerLog={seed.spoilerLog} />
	</div>
{:else}
	<p class="text-sm text-slate-500 dark:text-slate-400">
		This seed has no stored spoiler log.
	</p>
{/if}
