<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Modal from "$lib/components/ui/Modal.svelte";
	import ProfileSaveDialog, {
		type SaveDialogResult,
	} from "$lib/components/config/profiles/ProfileSaveDialog.svelte";
	import {
		ApiError,
		deleteProfile,
		duplicateProfile,
		patchProfile,
		saveProfileFavorite,
		saveProfilePreferences,
	} from "$lib/services/data";
	import type {
		ProfilePreferencesDto,
		ProfileSummaryDto,
	} from "$lib/schemas/profiles";
	import { profileConfigPath } from "$lib/config/profile-links";

	interface PageData {
		profiles: ProfileSummaryDto[];
		preferences: ProfilePreferencesDto;
	}

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let profiles = $state<ProfileSummaryDto[]>([...data.profiles]);
	let defaultProfileId = $state<string | null>(
		data.preferences.defaultProfileId,
	);
	let favorites = $state<string[]>(
		data.preferences.favorites.map((f) => f.profileId),
	);
	let search = $state("");
	let error = $state<string | null>(null);
	let busy = $state(false);

	let renameTarget = $state<ProfileSummaryDto | null>(null);
	let renameOpen = $state(false);
	let renameFieldErrors = $state<Record<string, string> | null>(null);
	let deleteTarget = $state<ProfileSummaryDto | null>(null);
	let deleteOpen = $state(false);

	const filtered = $derived(
		profiles.filter((p) => {
			const q = search.trim().toLowerCase();
			if (!q) return true;
			return (
				p.name.toLowerCase().includes(q) ||
				(p.description ?? "").toLowerCase().includes(q) ||
				p.selectedGames.some((g) => g.toLowerCase().includes(q)) ||
				p.configId.toLowerCase().includes(q)
			);
		}),
	);

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

	function openRename(profile: ProfileSummaryDto) {
		renameTarget = profile;
		renameFieldErrors = null;
		renameOpen = true;
	}

	async function handleRename(result: SaveDialogResult) {
		if (!renameTarget) return;
		const target = renameTarget;
		renameFieldErrors = null;
		try {
			const { profile } = await patchProfile(target.id, {
				name: result.name,
				description: result.description || null,
			});
			profiles = profiles.map((p) => (p.id === target.id ? profile : p));
			renameOpen = false;
		} catch (e) {
			if (e instanceof ApiError && e.fieldErrors) {
				renameFieldErrors = e.fieldErrors;
			} else {
				renameOpen = false;
				error = e instanceof Error ? e.message : String(e);
			}
		}
	}

	async function handleDuplicate(profile: ProfileSummaryDto) {
		await run(async () => {
			const detail = await duplicateProfile(profile.id);
			profiles = [...profiles, detail.profile];
		});
	}

	async function handleSetDefault(profile: ProfileSummaryDto) {
		const next = defaultProfileId === profile.id ? null : profile.id;
		await run(async () => {
			await saveProfilePreferences({ defaultProfileId: next });
			defaultProfileId = next;
		});
	}

	async function handleTogglePin(profile: ProfileSummaryDto) {
		const pinned = favorites.includes(profile.id);
		await run(async () => {
			await saveProfileFavorite(profile.id, !pinned);
			favorites = pinned
				? favorites.filter((id) => id !== profile.id)
				: [...favorites, profile.id];
		});
	}

	async function handleDelete() {
		if (!deleteTarget) return;
		const target = deleteTarget;
		await run(async () => {
			await deleteProfile(target.id);
			profiles = profiles.filter((p) => p.id !== target.id);
			if (defaultProfileId === target.id) defaultProfileId = null;
			favorites = favorites.filter((id) => id !== target.id);
		});
		deleteOpen = false;
		deleteTarget = null;
	}
</script>

<section aria-labelledby="profiles-heading">
	<div class="mb-5 flex flex-wrap items-center justify-between gap-3">
		<h2 id="profiles-heading" class="text-xl font-semibold text-slate-900 dark:text-slate-100">
			{m.seed_profile_manage_title()}
		</h2>
		<a
			href="/content/profiles"
			class="text-sm font-medium text-blue-600 underline hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300"
		>
			Profile guide
		</a>
	</div>

	{#if error}
		<div
			class="mb-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded dark:bg-red-700 dark:border-red-600 dark:text-red-200"
			role="alert"
		>
			<strong class="font-bold">{m.error_label()}</strong>
			<span class="block sm:inline">{error}</span>
		</div>
	{/if}

	{#if profiles.length === 0}
		<p class="text-lg text-slate-700 dark:text-slate-300">
			{m.seed_profile_manage_empty()}
		</p>
	{:else}
		<div class="mb-4 max-w-sm">
			<label class="sr-only" for="profile-search">
				{m.seed_profile_manage_search_label()}
			</label>
			<input
				id="profile-search"
				type="search"
				bind:value={search}
				placeholder={m.seed_profile_manage_search_label()}
				class="w-full rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			/>
		</div>

		{#if filtered.length === 0}
			<p class="text-slate-600 dark:text-slate-400">
				{m.seed_profile_manage_no_matches()}
			</p>
		{/if}

		<div class="space-y-3">
			{#each filtered as profile (profile.id)}
				<div
					class="bg-white dark:bg-slate-800 p-4 rounded-lg shadow hover:shadow-md transition-shadow"
				>
					<div class="flex flex-wrap items-center gap-2">
						<span
							class="text-lg font-semibold text-slate-900 dark:text-slate-100"
						>
							{profile.name}
						</span>
						<Badge variant="neutral">{profile.configId}</Badge>
						{#if defaultProfileId === profile.id}
							<Badge variant="success">
								{m.seed_profile_badge_default()}
							</Badge>
						{/if}
						{#if favorites.includes(profile.id)}
							<Badge variant="warning">
								{m.seed_profile_action_pin()}
							</Badge>
						{/if}
						{#if profile.revisionNumber !== null}
							<Badge variant="neutral">
								{m.seed_profile_badge_revision({
									revision: String(profile.revisionNumber),
								})}
							</Badge>
						{/if}
					</div>
					<p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
						{profile.description || m.seed_profile_description_none()}
					</p>
					<p class="mt-0.5 text-xs text-slate-500 dark:text-slate-500">
						{m.seed_profile_games_label({
							games: profile.selectedGames.join(", "),
						})}
						{#if profile.updatedAt}
							&middot;
							{m.seed_profile_manage_updated({
								date: new Date(profile.updatedAt).toLocaleString(),
							})}
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
							{m.seed_profile_manage_open()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => openRename(profile)}
						>
							{m.seed_profile_action_edit()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => void handleDuplicate(profile)}
						>
							{m.seed_profile_action_duplicate()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => void handleSetDefault(profile)}
						>
							{defaultProfileId === profile.id
								? m.seed_profile_action_unset_default()
								: m.seed_profile_action_set_default()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => void handleTogglePin(profile)}
						>
							{favorites.includes(profile.id)
								? m.seed_profile_action_unpin()
								: m.seed_profile_action_pin()}
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
							{m.seed_profile_action_delete()}
						</Button>
					</div>
				</div>
			{/each}
		</div>
	{/if}
</section>

<ProfileSaveDialog
	bind:open={renameOpen}
	mode="edit"
	initialName={renameTarget?.name ?? ""}
	initialDescription={renameTarget?.description ?? ""}
	saving={busy}
	fieldErrors={renameFieldErrors}
	onsubmit={(result) => void handleRename(result)}
	oncancel={() => (renameFieldErrors = null)}
/>

<Modal bind:open={deleteOpen} title={m.seed_profile_dialog_delete_title()}>
	<p>
		{m.seed_profile_dialog_delete_message({
			name: deleteTarget?.name ?? "",
		})}
	</p>
	{#snippet footer()}
		<Button
			type="button"
			variant="secondary"
			size="sm"
			data-autofocus
			onclick={() => (deleteOpen = false)}
		>
			{m.seed_profile_dialog_cancel_button()}
		</Button>
		<Button
			type="button"
			variant="danger"
			size="sm"
			disabled={busy}
			onclick={() => void handleDelete()}
		>
			{m.seed_profile_dialog_delete_confirm()}
		</Button>
	{/snippet}
</Modal>
