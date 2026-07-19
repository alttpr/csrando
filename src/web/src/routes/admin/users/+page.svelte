<script lang="ts">
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Modal from "$lib/components/ui/Modal.svelte";

	interface AdminUserRow {
		id: string;
		username: string;
		isAdmin: boolean;
		hasGithub: boolean;
	}

	interface Props {
		data: {
			users: AdminUserRow[];
			total: number;
			query: string;
			selfUserId: string | null;
		};
	}

	let { data }: Props = $props();

	// Writable derived: re-syncs on navigation/search, still locally
	// reassignable after promote/demote.
	let users = $derived<AdminUserRow[]>([...data.users]);
	let error = $state<string | null>(null);
	let busyId = $state<string | null>(null);

	// Password reset flow: confirm first, then show the generated password
	// exactly once.
	let resetTarget = $state<AdminUserRow | null>(null);
	let resetConfirmOpen = $state(false);
	let resetResult = $state<{ username: string; password: string } | null>(
		null,
	);
	let resetResultOpen = $state(false);
	let resetCopied = $state(false);

	async function handleResetPassword() {
		if (!resetTarget) return;
		const target = resetTarget;
		busyId = target.id;
		error = null;
		try {
			const response = await fetch(
				`/api/admin/users/${encodeURIComponent(target.id)}/reset-password`,
				{ method: "POST" },
			);
			if (!response.ok) {
				const body = await response.json().catch(() => null);
				throw new Error(
					body?.message ?? `Request failed (${response.status})`,
				);
			}
			resetResult = await response.json();
			resetConfirmOpen = false;
			resetCopied = false;
			resetResultOpen = true;
		} catch (e) {
			resetConfirmOpen = false;
			error = e instanceof Error ? e.message : String(e);
		} finally {
			busyId = null;
		}
	}

	async function copyResetPassword() {
		if (!resetResult) return;
		try {
			await navigator.clipboard.writeText(resetResult.password);
			resetCopied = true;
		} catch {
			// Clipboard unavailable; the password stays visible for manual copy.
		}
	}

	async function setAdmin(user: AdminUserRow, isAdmin: boolean) {
		busyId = user.id;
		error = null;
		try {
			const response = await fetch("/api/admin/promote", {
				method: "POST",
				headers: { "content-type": "application/json" },
				body: JSON.stringify({ username: user.username, isAdmin }),
			});
			if (!response.ok) {
				const body = await response.json().catch(() => null);
				throw new Error(
					body?.message ?? `Request failed (${response.status})`,
				);
			}
			users = users.map((u) => (u.id === user.id ? { ...u, isAdmin } : u));
		} catch (e) {
			error = e instanceof Error ? e.message : String(e);
		} finally {
			busyId = null;
		}
	}
</script>

<svelte:head>
	<title>Admin: users</title>
</svelte:head>

<h1 class="mb-4 text-2xl font-bold text-primary-600 dark:text-primary-400">
	Users
</h1>

<form method="GET" class="mb-3 flex items-center gap-2">
	<input
		type="search"
		name="q"
		value={data.query}
		placeholder="Search by username"
		class="w-full max-w-xs rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
	/>
	<Button type="submit" variant="secondary" size="xs">Search</Button>
</form>

{#if error}
	<div
		class="mb-3 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-800 dark:border-red-700 dark:bg-red-900 dark:text-red-200"
		role="alert"
	>
		{error}
	</div>
{/if}

<p class="mb-2 text-xs text-slate-500 dark:text-slate-400">
	{data.total} user{data.total === 1 ? "" : "s"}{users.length < data.total
		? ` (showing first ${users.length})`
		: ""}
</p>

<div
	class="overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800"
>
	<table class="w-full text-left text-sm">
		<thead
			class="border-b border-slate-200 text-xs uppercase tracking-wide text-slate-500 dark:border-slate-700 dark:text-slate-400"
		>
			<tr>
				<th class="px-3 py-2">Username</th>
				<th class="px-3 py-2">Login</th>
				<th class="px-3 py-2">Role</th>
				<th class="px-3 py-2 text-right">Actions</th>
			</tr>
		</thead>
		<tbody>
			{#each users as user (user.id)}
				<tr class="border-b border-slate-100 last:border-0 dark:border-slate-700/60">
					<td
						class="px-3 py-2 font-medium text-slate-900 dark:text-slate-100"
					>
						{user.username}
					</td>
					<td class="px-3 py-2 text-slate-600 dark:text-slate-300">
						{user.hasGithub ? "GitHub" : "Password"}
					</td>
					<td class="px-3 py-2">
						{#if user.isAdmin}
							<Badge variant="info">Admin</Badge>
						{:else}
							<Badge variant="neutral">Member</Badge>
						{/if}
					</td>
					<td class="px-3 py-2 text-right">
						<span class="inline-flex flex-wrap justify-end gap-1">
						<Button
							variant="secondary"
							size="xs"
							disabled={busyId === user.id}
							onclick={() => {
								resetTarget = user;
								resetConfirmOpen = true;
							}}
						>
							Reset password
						</Button>
						{#if user.isAdmin}
							<Button
								variant="secondary"
								size="xs"
								disabled={busyId === user.id ||
									user.id === data.selfUserId}
								onclick={() => setAdmin(user, false)}
							>
								Remove admin
							</Button>
						{:else}
							<Button
								variant="secondary"
								size="xs"
								disabled={busyId === user.id}
								onclick={() => setAdmin(user, true)}
							>
								Make admin
							</Button>
						{/if}
						</span>
					</td>
				</tr>
			{:else}
				<tr>
					<td
						class="px-3 py-4 text-center text-slate-500 dark:text-slate-400"
						colspan="4"
					>
						No users match this search.
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>

<Modal bind:open={resetConfirmOpen} title="Reset password">
	<p>
		Reset the password for
		<strong>{resetTarget?.username ?? ""}</strong>? They are signed out
		everywhere and get a new autogenerated password, shown to you once.
	</p>
	{#snippet footer()}
		<Button
			type="button"
			variant="secondary"
			size="sm"
			data-autofocus
			onclick={() => (resetConfirmOpen = false)}
		>
			Cancel
		</Button>
		<Button
			type="button"
			variant="danger"
			size="sm"
			disabled={busyId !== null}
			onclick={() => void handleResetPassword()}
		>
			Reset password
		</Button>
	{/snippet}
</Modal>

<Modal bind:open={resetResultOpen} title="New password">
	<p class="text-sm">
		Pass this password to
		<strong>{resetResult?.username ?? ""}</strong> through a channel you
		trust. It is shown only once; they can change it afterwards on their
		preset page.
	</p>
	<p
		class="mt-3 select-all rounded border border-slate-300 bg-slate-50 px-3 py-2 text-center font-mono text-lg dark:border-slate-600 dark:bg-slate-900"
	>
		{resetResult?.password ?? ""}
	</p>
	{#snippet footer()}
		<Button
			type="button"
			variant="secondary"
			size="sm"
			onclick={() => void copyResetPassword()}
		>
			{resetCopied ? "Copied!" : "Copy"}
		</Button>
		<Button
			type="button"
			variant="primary"
			size="sm"
			onclick={() => {
				resetResultOpen = false;
				resetResult = null;
			}}
		>
			Done
		</Button>
	{/snippet}
</Modal>
