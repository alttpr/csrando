<script lang="ts">
	import PasswordSection from "$lib/components/profile/PasswordSection.svelte";
</script>

<svelte:head><title>Account: security</title></svelte:head>

<section aria-labelledby="security-heading">
	<h2 id="security-heading" class="sr-only">Security</h2>
	<PasswordSection standalone={true} />
	<div class="mt-8 rounded-lg border border-red-200 bg-red-50 p-4 dark:border-red-900 dark:bg-red-950/30">
		<h3 class="font-semibold text-red-800 dark:text-red-300">Delete account</h3>
		<p class="mt-1 text-sm text-red-700 dark:text-red-300">Permanently remove your account, saved presets, API keys, and seed associations.</p>
		<a href="/profile/danger" class="mt-3 inline-block text-sm font-semibold text-red-700 underline dark:text-red-300">Open danger zone</a>
	</div>
</section>
