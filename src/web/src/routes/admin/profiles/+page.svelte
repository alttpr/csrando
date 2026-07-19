<script lang="ts">
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Modal from "$lib/components/ui/Modal.svelte";
	import ProfileSaveDialog, {
		type SaveDialogResult,
	} from "$lib/components/config/profiles/ProfileSaveDialog.svelte";
	import {
		ApiError,
		deleteProfile,
		patchProfile,
		promoteProfile,
		promoteSharedProfile,
	} from "$lib/services/data";
	import type { ProfileSummaryDto } from "$lib/schemas/profiles";
	import { profileConfigPath } from "$lib/config/profile-links";

	interface Props {
		data: {
			officials: ProfileSummaryDto[];
			myProfiles: ProfileSummaryDto[];
		};
	}

	let { data }: Props = $props();

	let officials = $state<ProfileSummaryDto[]>([...data.officials]);
	let error = $state<string | null>(null);
	let busy = $state(false);

	let editTarget = $state<ProfileSummaryDto | null>(null);
	let editOpen = $state(false);
	let editFieldErrors = $state<Record<string, string> | null>(null);
	let deleteTarget = $state<ProfileSummaryDto | null>(null);
	let deleteOpen = $state(false);

	// Promotion form state. A pasted share link wins over the own-profile
	// dropdown so admins can promote other users' shared profiles.
	let promoteSourceId = $state("");
	let promoteShareLink = $state("");
	let promoteSlug = $state("");
	let promoteName = $state("");
	let promoteError = $state<string | null>(null);
	let promoteBusy = $state(false);

	// Accepts a full share URL (/config/x?share=TOKEN) or a bare token.
	function extractShareToken(value: string): string {
		const trimmed = value.trim();
		try {
			const url = new URL(trimmed, window.location.origin);
			const token = url.searchParams.get("share");
			if (token) return token;
		} catch {
			// not a URL — fall through
		}
		return trimmed;
	}

	async function run(action: () => Promise<void>) {
		busy = true;
		error = null;
		try {
			await action();
		} catch (e) {
			error = e instanceof Error ? e.message : String(e);
		} finally {
			busy = false;
		}
	}

	function replaceProfile(next: ProfileSummaryDto) {
		officials = officials.map((p) => (p.id === next.id ? next : p));
	}

	async function handleArchiveToggle(profile: ProfileSummaryDto) {
		await run(async () => {
			const { profile: updated } = await patchProfile(profile.id, {
				archived: !profile.archived,
			});
			replaceProfile(updated);
		});
	}

	async function handleSetRecommended(profile: ProfileSummaryDto) {
		await run(async () => {
			const { profile: updated } = await patchProfile(profile.id, {
				isRecommended: true,
			});
			// The single-recommended invariant is enforced server-side per
			// config page; mirror it locally.
			officials = officials.map((p) =>
				p.id === updated.id
					? updated
					: p.configId === updated.configId
						? { ...p, isRecommended: false }
						: p,
			);
		});
	}

	async function handleEdit(result: SaveDialogResult) {
		if (!editTarget) return;
		const target = editTarget;
		editFieldErrors = null;
		try {
			const { profile: updated } = await patchProfile(target.id, {
				name: result.name,
				description: result.description || null,
				difficultyTag: result.difficultyTag || null,
				gameTags: result.gameTags.length > 0 ? result.gameTags : null,
				displayOrder: result.displayOrder,
			});
			replaceProfile(updated);
			editOpen = false;
		} catch (e) {
			if (e instanceof ApiError && e.fieldErrors) {
				editFieldErrors = e.fieldErrors;
			} else {
				editOpen = false;
				error = e instanceof Error ? e.message : String(e);
			}
		}
	}

	async function handleDelete() {
		if (!deleteTarget) return;
		const target = deleteTarget;
		await run(async () => {
			await deleteProfile(target.id);
			officials = officials.filter((p) => p.id !== target.id);
		});
		deleteOpen = false;
		deleteTarget = null;
	}

	async function handlePromote(event: SubmitEvent) {
		event.preventDefault();
		const shareToken = promoteShareLink.trim()
			? extractShareToken(promoteShareLink)
			: "";
		if ((!promoteSourceId && !shareToken) || !promoteSlug.trim()) {
			promoteError =
				"Pick a profile or paste a share link, and enter a slug.";
			return;
		}
		promoteBusy = true;
		promoteError = null;
		try {
			const input = {
				slug: promoteSlug.trim(),
				name: promoteName.trim() || undefined,
			};
			const { profile } = shareToken
				? await promoteSharedProfile({ token: shareToken, ...input })
				: await promoteProfile(promoteSourceId, input);
			officials = [...officials, profile];
			promoteSourceId = "";
			promoteShareLink = "";
			promoteSlug = "";
			promoteName = "";
		} catch (e) {
			if (e instanceof ApiError && e.fieldErrors?.slug) {
				promoteError = e.fieldErrors.slug;
			} else {
				promoteError = e instanceof Error ? e.message : String(e);
			}
		} finally {
			promoteBusy = false;
		}
	}
</script>

<svelte:head>
	<title>Admin: official profiles</title>
</svelte:head>

<h1 class="mb-4 text-2xl font-bold text-primary-600 dark:text-primary-400">
	Official profiles
</h1>

{#if error}
	<div
		class="mb-3 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-800 dark:border-red-700 dark:bg-red-900 dark:text-red-200"
		role="alert"
	>
		{error}
	</div>
{/if}

<div class="space-y-3">
	{#each officials as profile (profile.id)}
		<div
			class="rounded-lg bg-white p-4 shadow transition-shadow hover:shadow-md dark:bg-slate-800 {profile.archived
				? 'opacity-70'
				: ''}"
		>
			<div class="flex flex-wrap items-center gap-2">
				<span
					class="text-lg font-semibold text-slate-900 dark:text-slate-100"
				>
					{profile.name}
				</span>
				<Badge variant="neutral">{profile.configId}</Badge>
				{#if profile.isRecommended}
					<Badge variant="success">Recommended</Badge>
				{/if}
				{#if profile.difficultyTag}
					<Badge variant="info">{profile.difficultyTag}</Badge>
				{/if}
				{#each profile.gameTags ?? [] as tag (tag)}
					<Badge variant="neutral">{tag}</Badge>
				{/each}
				{#if profile.archived}
					<Badge variant="warning">Archived</Badge>
				{/if}
				{#if profile.revisionNumber !== null}
					<Badge variant="neutral">rev {profile.revisionNumber}</Badge>
				{/if}
			</div>
			{#if profile.slug}
				<p class="mt-0.5 text-xs text-slate-500 dark:text-slate-400">
					Slug: <span class="font-mono">{profile.slug}</span>
				</p>
			{/if}
			<p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
				{profile.description || "No description"}
			</p>
			<p class="mt-0.5 text-xs text-slate-500 dark:text-slate-500">
				{profile.selectedGames.join(", ")}
				{#if profile.updatedAt}
					&middot; updated {new Date(
						profile.updatedAt,
					).toLocaleString()}
				{/if}
			</p>
			<div class="mt-3 flex flex-wrap gap-2">
				<Button
					href={profileConfigPath({
						profileId: profile.id,
						slug: profile.slug,
						configId: profile.configId,
					}) ?? undefined}
					variant="primary"
					size="xs"
				>
					Open
				</Button>
				<Button
					type="button"
					variant="secondary"
					size="xs"
					disabled={busy}
					onclick={() => {
						editTarget = profile;
						editFieldErrors = null;
						editOpen = true;
					}}
				>
					Edit
				</Button>
				{#if !profile.isRecommended && !profile.archived}
					<Button
						type="button"
						variant="secondary"
						size="xs"
						disabled={busy}
						onclick={() => void handleSetRecommended(profile)}
					>
						Set recommended
					</Button>
				{/if}
				<Button
					type="button"
					variant="secondary"
					size="xs"
					disabled={busy}
					onclick={() => void handleArchiveToggle(profile)}
				>
					{profile.archived ? "Unarchive" : "Archive"}
				</Button>
				<Button
					type="button"
					variant="danger"
					size="xs"
					disabled={busy}
					onclick={() => {
						deleteTarget = profile;
						deleteOpen = true;
					}}
				>
					Delete
				</Button>
			</div>
		</div>
	{:else}
		<p class="text-slate-600 dark:text-slate-400">
			No official presets yet.
		</p>
	{/each}
</div>

<div
	class="mt-6 rounded-lg border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-800"
>
	<h2 class="text-lg font-semibold text-slate-900 dark:text-slate-100">
		Promote a profile
	</h2>
	<p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
		Copies the profile's current settings into a new official preset — the
		source profile stays untouched. Pick one of your own profiles, or paste
		a share link a user sent you (the share link takes precedence).
	</p>
	{#if promoteError}
		<div
			class="mt-2 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-800 dark:border-red-700 dark:bg-red-900 dark:text-red-200"
			role="alert"
		>
			{promoteError}
		</div>
	{/if}
	<form class="mt-3 flex flex-wrap items-end gap-3" onsubmit={handlePromote}>
		{#if data.myProfiles.length > 0}
			<label
				class="block text-xs font-medium text-slate-700 dark:text-slate-300"
			>
				My profile
				<select
					bind:value={promoteSourceId}
					class="mt-1 block w-56 rounded-md border border-slate-300 bg-white px-2 py-1.5 text-sm text-slate-900 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
				>
					<option value="">Select a profile…</option>
					{#each data.myProfiles as profile (profile.id)}
						<option value={profile.id}>
							{profile.name} ({profile.configId})
						</option>
					{/each}
				</select>
			</label>
		{/if}
		<label
			class="block text-xs font-medium text-slate-700 dark:text-slate-300"
		>
			Share link or token
			<input
				type="text"
				bind:value={promoteShareLink}
				placeholder="https://…?share=TOKEN"
				class="mt-1 block w-64 rounded-md border border-slate-300 bg-white px-2 py-1.5 font-mono text-sm text-slate-900 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			/>
		</label>
		<label
			class="block text-xs font-medium text-slate-700 dark:text-slate-300"
		>
			Slug
			<input
				type="text"
				bind:value={promoteSlug}
				placeholder="my-preset"
				class="mt-1 block w-44 rounded-md border border-slate-300 bg-white px-2 py-1.5 font-mono text-sm text-slate-900 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			/>
		</label>
		<label
			class="block text-xs font-medium text-slate-700 dark:text-slate-300"
		>
			Name (optional)
			<input
				type="text"
				bind:value={promoteName}
				placeholder="Defaults to the profile name"
				class="mt-1 block w-56 rounded-md border border-slate-300 bg-white px-2 py-1.5 text-sm text-slate-900 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			/>
		</label>
		<Button type="submit" variant="primary" size="sm" disabled={promoteBusy}>
			Promote to official
		</Button>
	</form>
</div>

<ProfileSaveDialog
	bind:open={editOpen}
	mode="edit"
	officialEdit={true}
	initialName={editTarget?.name ?? ""}
	initialDescription={editTarget?.description ?? ""}
	initialDifficultyTag={editTarget?.difficultyTag ?? ""}
	initialGameTags={(editTarget?.gameTags ?? []).join(", ")}
	initialDisplayOrder={editTarget?.displayOrder ?? 0}
	saving={busy}
	fieldErrors={editFieldErrors}
	onsubmit={(result) => void handleEdit(result)}
	oncancel={() => (editFieldErrors = null)}
/>

<Modal bind:open={deleteOpen} title="Delete official preset">
	<p>
		Delete "{deleteTarget?.name ?? ""}"? Seeds generated from it keep their
		settings, but the preset disappears for everyone.
	</p>
	{#snippet footer()}
		<Button
			type="button"
			variant="secondary"
			size="sm"
			data-autofocus
			onclick={() => (deleteOpen = false)}
		>
			Cancel
		</Button>
		<Button
			type="button"
			variant="danger"
			size="sm"
			disabled={busy}
			onclick={() => void handleDelete()}
		>
			Delete
		</Button>
	{/snippet}
</Modal>
