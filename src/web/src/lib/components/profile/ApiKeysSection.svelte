<script lang="ts">
	import { onMount } from "svelte";
	import * as m from "$lib/paraglide/messages";
	import Button from "$lib/components/ui/Button.svelte";
	import Modal from "$lib/components/ui/Modal.svelte";
	import {
		createApiKeyRequest,
		fetchApiKeys,
		revokeApiKeyRequest,
		type ApiKeyDto,
	} from "$lib/services/data";

	interface Props { standalone?: boolean }
	let { standalone = false }: Props = $props();

	let keys = $state<ApiKeyDto[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let newKeyName = $state("");
	let creating = $state(false);
	// The plain secret of a freshly created key; shown exactly once.
	let createdSecret = $state<string | null>(null);
	let createdCopied = $state(false);
	let revokeTarget = $state<ApiKeyDto | null>(null);
	let revokeOpen = $state(false);
	let revoking = $state(false);

	onMount(async () => {
		try {
			({ keys } = await fetchApiKeys());
		} catch (e) {
			error = e instanceof Error ? e.message : String(e);
		} finally {
			loading = false;
		}
	});

	async function handleCreate(event: SubmitEvent) {
		event.preventDefault();
		const name = newKeyName.trim();
		if (!name) return;
		creating = true;
		error = null;
		createdSecret = null;
		try {
			const { key, secret } = await createApiKeyRequest(name);
			keys = [...keys, key];
			createdSecret = secret;
			createdCopied = false;
			newKeyName = "";
		} catch (e) {
			error = e instanceof Error ? e.message : String(e);
		} finally {
			creating = false;
		}
	}

	async function copyCreatedSecret() {
		if (!createdSecret) return;
		try {
			await navigator.clipboard.writeText(createdSecret);
			createdCopied = true;
		} catch {
			// The value remains selectable for manual copying.
		}
	}

	async function handleRevoke() {
		if (!revokeTarget) return;
		const key = revokeTarget;
		error = null;
		revoking = true;
		try {
			await revokeApiKeyRequest(key.id);
			keys = keys.filter((k) => k.id !== key.id);
			revokeOpen = false;
			revokeTarget = null;
		} catch (e) {
			error = e instanceof Error ? e.message : String(e);
		} finally {
			revoking = false;
		}
	}
</script>

<section class={standalone ? "" : "mt-10 border-t border-slate-200 pt-6 dark:border-slate-700"}>
	<h2 class="text-xl font-semibold mb-2 text-slate-900 dark:text-slate-100">
		{m.api_keys_title()}
	</h2>
	<p class="text-sm text-slate-600 dark:text-slate-300 mb-3">
		{m.api_keys_description()}
	</p>

	{#if error}
		<div
			class="mb-3 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-700 dark:border-red-600 dark:bg-red-700 dark:text-red-200"
			role="alert"
		>
			{error}
		</div>
	{/if}

	{#if createdSecret}
		<div
			class="mb-3 rounded border border-green-400 bg-green-100 px-3 py-2 text-xs text-green-800 dark:border-green-700 dark:bg-green-900 dark:text-green-200"
			role="status"
			data-testid="api-key-secret"
		>
			<p class="mb-1 font-medium">{m.api_keys_secret_notice()}</p>
			<code
				class="block select-all break-all rounded bg-white/70 px-2 py-1 font-mono text-slate-900 dark:bg-slate-900/60 dark:text-slate-100"
			>
				{createdSecret}
			</code>
			<div class="mt-2 flex gap-2">
				<Button type="button" variant="secondary" size="xs" onclick={() => void copyCreatedSecret()}>
					{createdCopied ? "Copied!" : "Copy key"}
				</Button>
				<Button type="button" variant="secondary" size="xs" onclick={() => (createdSecret = null)}>
					I have saved it
				</Button>
			</div>
		</div>
	{/if}

	{#if loading}
		<p class="text-sm text-slate-500 dark:text-slate-400">...</p>
	{:else if keys.length === 0}
		<p class="text-sm text-slate-500 dark:text-slate-400">
			{m.api_keys_empty()}
		</p>
	{:else}
		<ul class="space-y-2">
			{#each keys as key (key.id)}
				<li
					class="flex flex-wrap items-center gap-2 rounded-lg bg-white p-3 shadow dark:bg-slate-800"
				>
					<span class="min-w-0 flex-1">
						<span
							class="block truncate text-sm font-medium text-slate-900 dark:text-slate-100"
						>
							{key.name}
							<code
								class="ml-2 font-mono text-xs text-slate-500 dark:text-slate-400"
							>
								{key.tokenPrefix}…
							</code>
						</span>
						<span class="block text-xs text-slate-500 dark:text-slate-400">
							{#if key.createdAt}
								{m.api_keys_created_at({
									date: new Date(key.createdAt).toLocaleString(),
								})}
								&middot;
							{/if}
							{key.lastUsedAt
								? m.api_keys_last_used({
										date: new Date(key.lastUsedAt).toLocaleString(),
									})
								: m.api_keys_never_used()}
						</span>
					</span>
					<Button
						type="button"
						variant="danger"
						size="xs"
						onclick={() => {
							revokeTarget = key;
							revokeOpen = true;
						}}
					>
						{m.api_keys_revoke_button()}
					</Button>
				</li>
			{/each}
		</ul>
	{/if}

	<form onsubmit={handleCreate} class="mt-3 flex flex-wrap items-end gap-2">
		<div>
			<label
				for="api-key-name"
				class="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300"
			>
				{m.api_keys_name_label()}
			</label>
			<input
				id="api-key-name"
				type="text"
				bind:value={newKeyName}
				maxlength="60"
				placeholder={m.api_keys_name_placeholder()}
				class="w-64 rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			/>
		</div>
		<Button
			type="submit"
			variant="secondary"
			size="sm"
			disabled={creating || !newKeyName.trim()}
		>
			{m.api_keys_create_button()}
		</Button>
	</form>
</section>

<Modal bind:open={revokeOpen} title="Revoke API key" onclose={() => (revokeTarget = null)}>
	<p>
		Revoke <strong>{revokeTarget?.name ?? ""}</strong>? Any tool using this
		key will immediately lose access.
	</p>
	{#snippet footer()}
		<Button type="button" variant="secondary" size="sm" data-autofocus onclick={() => {
			revokeOpen = false;
			revokeTarget = null;
		}}>Cancel</Button>
		<Button type="button" variant="danger" size="sm" disabled={revoking} onclick={() => void handleRevoke()}>
			{revoking ? "Revoking…" : "Revoke key"}
		</Button>
	{/snippet}
</Modal>
