<script lang="ts">
	import { page } from "$app/state";
	import type { Snippet } from "svelte";

	interface Props {
		data: { user: { username: string } };
		children?: Snippet;
	}

	let { data, children }: Props = $props();

	const links = [
		{ href: "/profile", label: "Overview", exact: true },
		{ href: "/profile/seeds", label: "Seeds", exact: false },
		{ href: "/profile/profiles", label: "Profiles", exact: false },
		{ href: "/profile/api-keys", label: "API keys", exact: false },
		{ href: "/profile/security", label: "Security", exact: false },
	];

	const isActive = (link: { href: string; exact: boolean }) =>
		link.exact
			? page.url.pathname === link.href
			: page.url.pathname.startsWith(link.href);
</script>

<svelte:head>
	<title>Account: {data.user.username}</title>
</svelte:head>

<div class="container mx-auto px-4 py-6">
	<div class="mb-5 flex flex-wrap items-end justify-between gap-3">
		<div>
			<p class="text-xs font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
				Account
			</p>
			<h1 class="text-2xl font-bold text-primary-600 dark:text-primary-400">
				{data.user.username}
			</h1>
		</div>
		<a
			href="/logout"
			class="text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-300 dark:hover:text-white"
		>
			Sign out
		</a>
	</div>

	<nav
		class="mb-6 flex gap-1 overflow-x-auto rounded-lg border border-slate-200 bg-white p-1 text-sm shadow-sm dark:border-slate-700 dark:bg-slate-800"
		aria-label="Account sections"
	>
		{#each links as link (link.href)}
			<a
				href={link.href}
				class="whitespace-nowrap rounded-md px-3 py-1.5 font-medium transition-colors {isActive(link)
					? 'bg-indigo-600 text-white'
					: 'text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-700'}"
				aria-current={isActive(link) ? "page" : undefined}
			>
				{link.label}
			</a>
		{/each}
	</nav>

	{@render children?.()}
</div>
