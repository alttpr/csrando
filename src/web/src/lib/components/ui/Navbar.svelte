<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import ThemeSwitcher from "./ThemeSwitcher.svelte";
	import AuthButtons from "./AuthButtons.svelte";
	import type { User } from "$lib/types";
	import type { SiteFeatures } from "$lib/config/site";

	interface Props {
		user?: User | null;
		siteFeatures: SiteFeatures;
	}

	let { user = null, siteFeatures }: Props = $props();
	let helpMenuOpen = $state(false);
</script>

<nav class="bg-white dark:bg-slate-800 shadow-md">
	<div class="container mx-auto px-6 py-3 flex justify-between items-center">
		<div class="flex items-center space-x-4">
			<a
				href="/"
				class="text-lg font-semibold hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
				>{m.nav_home()}</a
			>
			{#if siteFeatures.showAlttpr}
				<a
					href="/config/alttpr"
					class="text-lg font-semibold hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
					>{m.nav_config_alttpr()}</a
				>
			{/if}
			{#if siteFeatures.showCombo}
				<a
					href="/config/combo"
					class="text-lg font-semibold hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
					>{m.nav_config_combo()}</a
				>
			{/if}
		</div>

		<div class="flex items-center space-x-2">
			<details class="relative" bind:open={helpMenuOpen}>
				<summary
					class="flex cursor-pointer items-center gap-1 text-lg font-semibold transition-colors hover:text-blue-600 dark:hover:text-blue-400"
				>
					Help
					<svg
						xmlns="http://www.w3.org/2000/svg"
						viewBox="0 0 20 20"
						fill="currentColor"
						aria-hidden="true"
						class="h-4 w-4"
					>
						<path
							fill-rule="evenodd"
							d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z"
							clip-rule="evenodd"
						/>
					</svg>
				</summary>
				<div
					class="absolute right-0 z-20 mt-2 w-48 rounded-md border border-slate-200 bg-white py-2 shadow-lg ring-1 ring-black/5 dark:border-slate-700 dark:bg-slate-700"
				>
					<a
						href="/content/information"
						class="block px-4 py-2 text-sm text-slate-700 transition hover:bg-slate-100 dark:text-slate-100 dark:hover:bg-slate-600"
						onclick={() => (helpMenuOpen = false)}
					>
						{m.nav_information()}
					</a>
					<a
						href="/content/settings"
						class="block px-4 py-2 text-sm text-slate-700 transition hover:bg-slate-100 dark:text-slate-100 dark:hover:bg-slate-600"
						onclick={() => (helpMenuOpen = false)}
					>
						{m.nav_settings()}
					</a>
					<a
						href="/content/changelog"
						class="block px-4 py-2 text-sm text-slate-700 transition hover:bg-slate-100 dark:text-slate-100 dark:hover:bg-slate-600"
						onclick={() => (helpMenuOpen = false)}
					>
						{m.nav_changelog()}
					</a>
					<a
						href="/content/resources"
						class="block px-4 py-2 text-sm text-slate-700 transition hover:bg-slate-100 dark:text-slate-100 dark:hover:bg-slate-600"
						onclick={() => (helpMenuOpen = false)}
					>
						{m.nav_resources()}
					</a>
				</div>
			</details>
			<ThemeSwitcher />
			<AuthButtons {user} />
		</div>
	</div>
</nav>

<style>
	summary {
		list-style: none;
	}

	summary::-webkit-details-marker {
		display: none;
	}
</style>
