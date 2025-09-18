<script lang="ts">
	import * as m from '$lib/paraglide/messages';
	import ThemeSwitcher from './ThemeSwitcher.svelte';
	import AuthButtons from './AuthButtons.svelte';
	import type { User } from '$lib/types';

	interface Props {
		user?: User | null;
	}

	let { user = null }: Props = $props();
</script>

<nav class="bg-white dark:bg-slate-800 shadow-md">
	<div class="container mx-auto px-6 py-3 flex justify-between items-center">
		<div class="flex items-center space-x-4">
			<a
				href="/"
				class="text-lg font-semibold hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
				>{m.nav_home()}</a
			>
			<a
				href="/config/alttpr"
				class="text-lg font-semibold hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
				>{m.nav_config()}</a
			>
		</div>

		<div class="flex items-center space-x-2">
			<ThemeSwitcher />
			<AuthButtons {user} />
		</div>
	</div>
</nav>
