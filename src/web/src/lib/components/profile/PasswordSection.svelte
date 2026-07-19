<script lang="ts">
	import Button from "$lib/components/ui/Button.svelte";

	interface Props { standalone?: boolean }
	let { standalone = false }: Props = $props();

	let currentPassword = $state("");
	let newPassword = $state("");
	let confirmPassword = $state("");
	let busy = $state(false);
	let error = $state<string | null>(null);
	let success = $state(false);

	async function submit(event: SubmitEvent) {
		event.preventDefault();
		error = null;
		success = false;
		if (newPassword.length < 6) {
			error = "Password must be at least 6 characters long.";
			return;
		}
		if (newPassword !== confirmPassword) {
			error = "The new passwords do not match.";
			return;
		}
		busy = true;
		try {
			const response = await fetch("/api/user/password", {
				method: "POST",
				headers: { "content-type": "application/json" },
				body: JSON.stringify({
					currentPassword: currentPassword || undefined,
					newPassword,
				}),
			});
			if (!response.ok) {
				const body = await response.json().catch(() => null);
				throw new Error(
					body?.message ?? `Request failed (${response.status})`,
				);
			}
			success = true;
			currentPassword = "";
			newPassword = "";
			confirmPassword = "";
		} catch (e) {
			error = e instanceof Error ? e.message : String(e);
		} finally {
			busy = false;
		}
	}
</script>

<section class={standalone ? "" : "mt-10 border-t border-slate-200 pt-6 dark:border-slate-700"}>
	<h2 class="text-xl font-semibold mb-2 text-slate-900 dark:text-slate-100">
		Change password
	</h2>
	<p class="text-sm text-slate-600 dark:text-slate-300 mb-3">
		Changing your password signs you out everywhere else. If an admin reset
		your password for you, enter the password you were given as the current
		one.
	</p>

	{#if error}
		<div
			class="mb-3 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-800 dark:border-red-700 dark:bg-red-900 dark:text-red-200"
			role="alert"
		>
			{error}
		</div>
	{/if}
	{#if success}
		<div
			class="mb-3 rounded border border-green-400 bg-green-100 px-3 py-2 text-xs text-green-800 dark:border-green-700 dark:bg-green-900 dark:text-green-200"
			role="status"
		>
			Your password has been changed.
		</div>
	{/if}

	<form class="max-w-xs space-y-3" onsubmit={submit}>
		<label class="block text-sm" for="current-password">
			Current password
			<input
				id="current-password"
				type="password"
				autocomplete="current-password"
				bind:value={currentPassword}
				class="mt-1 w-full rounded border border-slate-300 bg-white px-3 py-2 text-sm dark:border-slate-600 dark:bg-slate-900"
			/>
		</label>
		<label class="block text-sm" for="new-password">
			New password
			<input
				id="new-password"
				type="password"
				autocomplete="new-password"
				bind:value={newPassword}
				class="mt-1 w-full rounded border border-slate-300 bg-white px-3 py-2 text-sm dark:border-slate-600 dark:bg-slate-900"
			/>
		</label>
		<label class="block text-sm" for="confirm-password">
			Confirm new password
			<input
				id="confirm-password"
				type="password"
				autocomplete="new-password"
				bind:value={confirmPassword}
				class="mt-1 w-full rounded border border-slate-300 bg-white px-3 py-2 text-sm dark:border-slate-600 dark:bg-slate-900"
			/>
		</label>
		<Button type="submit" variant="primary" size="sm" disabled={busy}>
			{busy ? "Changing…" : "Change password"}
		</Button>
	</form>
</section>
