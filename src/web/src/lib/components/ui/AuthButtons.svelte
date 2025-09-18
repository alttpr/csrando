<script lang="ts">
	import * as m from '$lib/paraglide/messages';
	import Button from './Button.svelte';
	import type { User } from '$lib/types';

	interface Props {
		user?: User | null;
	}

	let { user = null }: Props = $props();
</script>

{#if user}
	<a
		href="/profile"
		class="text-sm hover:text-blue-600 dark:hover:text-blue-400 transition-colors ml-2"
		>{m.nav_profile()}</a
	>
	<form action="/logout" method="POST">
		<Button type="submit" variant="danger" size="sm">{m.nav_logout()}</Button>
	</form>
{:else}
	<a
		href="/login"
		class="text-sm hover:text-blue-600 dark:hover:text-blue-400 transition-colors ml-2"
		>{m.nav_login()}</a
	>
	<Button href="/register" variant="primary" size="sm">{m.nav_register()}</Button>
{/if}
