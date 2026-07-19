<script lang="ts">
	import Button from "$lib/components/ui/Button.svelte";
	let confirmation = $state("");
	let deleting = $state(false);
</script>

<svelte:head><title>Account: delete account</title></svelte:head>

<section class="max-w-xl" aria-labelledby="danger-heading">
	<h2 id="danger-heading" class="text-xl font-semibold text-red-700 dark:text-red-300">Delete account</h2>
	<div class="mt-3 rounded-lg border border-red-300 bg-red-50 p-5 dark:border-red-900 dark:bg-red-950/30">
		<p class="text-sm text-red-800 dark:text-red-200">This permanently deletes your account, saved profiles and API keys, and removes the association between you and your generated seeds. This cannot be undone.</p>
		<form method="POST" action="/profile?/deleteAccount" class="mt-5" onsubmit={() => (deleting = true)}>
			<label for="delete-confirmation" class="block text-sm font-medium text-red-900 dark:text-red-100">Type <span class="font-mono">DELETE</span> to confirm</label>
			<input id="delete-confirmation" name="confirm" bind:value={confirmation} autocomplete="off" class="mt-2 w-full rounded-md border border-red-300 bg-white px-3 py-2 text-sm dark:border-red-800 dark:bg-slate-900" />
			<div class="mt-4 flex gap-2">
				<Button href="/profile/security" variant="secondary" size="sm">Cancel</Button>
				<Button type="submit" variant="danger" size="sm" disabled={confirmation !== "DELETE" || deleting}>{deleting ? "Deleting…" : "Delete my account"}</Button>
			</div>
		</form>
	</div>
</section>
