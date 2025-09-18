<script lang="ts">
	import '../app.css';
	import Navbar from '$lib/components/ui/Navbar.svelte';
	import Footer from '$lib/components/ui/Footer.svelte';
	import { page } from '$app/state';
	import * as m from '$lib/paraglide/messages';
	import { onMount } from 'svelte';

	// Bridge PUBLIC_SPRITES_BASE_URL to client runtime for env resolver
	onMount(() => {
		const base = page.data.spritesBaseUrl as string | null | undefined;
		if (base) {
			// Typed via src/global.d.ts augmentation
			window.__PUBLIC_SPRITES_BASE_URL__ = base;
		}
	});

	const SITE_NAME = 'Game Randomizer';

	function routeTitle(pathname: string): string | null {
		if (pathname === '/') return null; // homepage -> use just site name
		if (pathname.startsWith('/login')) return m.login_title();
		if (pathname.startsWith('/register')) return m.register_title();
		if (pathname.startsWith('/profile')) return m.profile_title();
		if (pathname.startsWith('/config')) return m.config_title();
		if (pathname.startsWith('/seed')) return m.permalink_title();
		return null;
	}

	$effect.pre(() => {
		// ensure reactive dependency on $page
		void page.url;
	});
</script>

<svelte:head>
	{#key page.url.pathname}
		<title>{routeTitle(page.url.pathname) ? `${routeTitle(page.url.pathname)} — ${SITE_NAME}` : SITE_NAME}</title>
	{/key}
</svelte:head>

<div
	class="min-h-screen flex flex-col bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100"
>
	<Navbar user={page.data.user} />
	<main class="flex-1">
		<slot />
	</main>

	<Footer />
</div>
