<script lang="ts">
	import ApiKeysSection from "$lib/components/profile/ApiKeysSection.svelte";
</script>

<svelte:head><title>Account: API keys</title></svelte:head>

<section aria-labelledby="api-keys-heading">
	<h2 id="api-keys-heading" class="sr-only">API keys</h2>
	<ApiKeysSection standalone={true} />
</section>
