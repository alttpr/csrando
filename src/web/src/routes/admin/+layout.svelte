<script lang="ts">
	import { page } from "$app/state";
	import type { Snippet } from "svelte";

	interface Props {
		data: { panelAuthorized: boolean };
		children?: Snippet;
	}

	let { data, children }: Props = $props();

	const links = [
		{ href: "/admin", label: "Dashboard", exact: true },
		{ href: "/admin/users", label: "Users", exact: false },
		{ href: "/admin/presets", label: "Official presets", exact: false },
		{ href: "/admin/seeds", label: "Seeds", exact: false },
		{ href: "/admin/versions", label: "Versions", exact: false },
		{ href: "/admin/new-version", label: "New version", exact: false },
	];

	const isActive = (link: { href: string; exact: boolean }) =>
		link.exact
			? page.url.pathname === link.href
			: page.url.pathname.startsWith(link.href);
</script>

<div class="container mx-auto px-4 py-4">
	{#if data.panelAuthorized}
		<nav
			class="mb-4 flex flex-wrap gap-1 rounded-lg border border-slate-200 bg-white p-1 text-sm shadow-sm dark:border-slate-700 dark:bg-slate-800"
			aria-label="Admin sections"
		>
			{#each links as link (link.href)}
				<a
					href={link.href}
					class="rounded-md px-3 py-1.5 font-medium transition-colors {isActive(
						link,
					)
						? 'bg-indigo-600 text-white'
						: 'text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-700'}"
					aria-current={isActive(link) ? "page" : undefined}
				>
					{link.label}
				</a>
			{/each}
		</nav>
	{/if}
	{@render children?.()}
</div>
