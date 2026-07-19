<script lang="ts">
	interface SiteStats {
		users: number;
		admins: number;
		seedsTotal: number;
		seedsLast24h: number;
		seedsLast7d: number;
		seedsLast30d: number;
		userPresets: number;
		officialPresets: number;
		archivedOfficialPresets: number;
		activeApiKeys: number;
		versions: number;
		activeVersionTags: string[];
	}

	interface Props {
		data: { stats: SiteStats };
	}

	let { data }: Props = $props();
	const stats = data.stats;

	const tiles = [
		{ label: "Seeds generated", value: stats.seedsTotal },
		{ label: "Seeds (24 h)", value: stats.seedsLast24h },
		{ label: "Seeds (7 days)", value: stats.seedsLast7d },
		{ label: "Seeds (30 days)", value: stats.seedsLast30d },
		{ label: "Members", value: stats.users },
		{ label: "Administrators", value: stats.admins },
		{ label: "User presets", value: stats.userPresets },
		{
			label: "Official presets",
			value: stats.officialPresets,
			hint:
				stats.archivedOfficialPresets > 0
					? `+ ${stats.archivedOfficialPresets} archived`
					: null,
		},
		{ label: "Active API keys", value: stats.activeApiKeys },
		{ label: "Randomizer versions", value: stats.versions },
	];
</script>

<svelte:head>
	<title>Admin dashboard</title>
</svelte:head>

<h1 class="mb-4 text-2xl font-bold text-primary-600 dark:text-primary-400">
	Admin dashboard
</h1>

<div class="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5">
	{#each tiles as tile (tile.label)}
		<div
			class="rounded-lg border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-800"
		>
			<p class="text-xs text-slate-500 dark:text-slate-400">
				{tile.label}
			</p>
			<p
				class="mt-1 text-2xl font-semibold text-slate-900 dark:text-slate-100"
			>
				{tile.value}
			</p>
			{#if "hint" in tile && tile.hint}
				<p class="mt-0.5 text-xs text-slate-500 dark:text-slate-400">
					{tile.hint}
				</p>
			{/if}
		</div>
	{/each}
</div>

{#if stats.activeVersionTags.length > 0}
	<div
		class="mt-4 rounded-lg border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-800"
	>
		<p class="text-xs font-semibold text-slate-500 dark:text-slate-400">
			Active randomizer versions
		</p>
		<p class="mt-1 font-mono text-sm text-slate-900 dark:text-slate-100">
			{stats.activeVersionTags.join(", ")}
		</p>
	</div>
{/if}
