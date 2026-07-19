<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import { goto } from "$app/navigation";
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Modal from "$lib/components/ui/Modal.svelte";
	import ProfileCombobox from "./ProfileCombobox.svelte";
	import ProfileOverflowMenu, {
		type MenuItem,
	} from "./ProfileOverflowMenu.svelte";
	import ProfileSaveDialog, {
		type SaveDialogResult,
	} from "./ProfileSaveDialog.svelte";
	import UnsavedChangesDialog from "./UnsavedChangesDialog.svelte";
	import type {
		ProfileState,
		LoadedForm,
	} from "$lib/config/profile-state.svelte";
	import type { NormalizedConfig } from "$lib/config/normalize";
	import type { Metadata } from "$lib/types";
	import { profileConfigPath } from "$lib/config/profile-links";
	import {
		ApiError,
		ensureProfileShare,
		revokeProfileShare,
	} from "$lib/services/data";

	interface Props {
		state: ProfileState;
		metadata: Metadata;
		currentNormalized: NormalizedConfig | null;
		isAuthenticated: boolean;
		isAdmin: boolean;
		// Name of a profile loaded through a ?share= link, if any.
		sharedName?: string | null;
		// A ?share= token was present but no longer resolves.
		sharedInvalid?: boolean;
		// Seed id whose settings were loaded through ?fromSeed=, if any.
		seedSourceId?: string | null;
		// A ?fromSeed= id was present but had no stored settings.
		seedSourceInvalid?: boolean;
		onapply: (loaded: LoadedForm) => void;
		// Full reset: discard drafts and selection, back to plain defaults.
		onreset: () => void;
	}

	let {
		state: profiles,
		metadata,
		currentNormalized,
		isAuthenticated,
		isAdmin,
		sharedName = null,
		sharedInvalid = false,
		seedSourceId = null,
		seedSourceInvalid = false,
		onapply,
		onreset,
	}: Props = $props();

	let saveDialogOpen = $state(false);
	let saveDialogMode = $state<"create" | "edit">("create");
	let dialogFieldErrors = $state<Record<string, string> | null>(null);
	let dialogError = $state<string | null>(null);
	let unsavedDialogOpen = $state(false);
	let unsavedMode = $state<"switch" | "leave">("switch");
	let deleteDialogOpen = $state(false);
	let pendingSwitchId = $state<string | null>(null);
	let pendingLeaveProceed: (() => void) | null = null;
	// Transient confirmation ("Share link copied", ...), auto-dismissed.
	let notice = $state<string | null>(null);
	let noticeTimer: ReturnType<typeof setTimeout> | null = null;
	let sharedNoticeDismissed = $state(false);
	let seedNoticeDismissed = $state(false);

	function showNotice(text: string) {
		notice = text;
		if (noticeTimer) clearTimeout(noticeTimer);
		noticeTimer = setTimeout(() => (notice = null), 4000);
	}

	async function copyToClipboard(text: string): Promise<void> {
		try {
			await navigator.clipboard.writeText(text);
		} catch {
			// Clipboard API unavailable (e.g. non-secure context): show the
			// link so the user can copy it manually.
			window.prompt(m.seed_profile_action_share(), text);
		}
	}

	async function handleShare(): Promise<void> {
		if (!selected) return;
		const base = `${window.location.origin}/config/${profiles.configId}`;
		if (selected.scope === "official") {
			// Official presets use their stable, human-readable public slug.
			const path = profileConfigPath({
				profileId: selected.id,
				slug: selected.slug,
				configId: profiles.configId,
			});
			if (!path) return;
			await copyToClipboard(`${window.location.origin}${path}`);
			showNotice(m.seed_profile_share_copied());
			return;
		}
		try {
			const { token } = await ensureProfileShare(selected.id);
			await copyToClipboard(`${base}?share=${token}`);
			showNotice(m.seed_profile_share_copied());
		} catch (err) {
			profiles.error = err instanceof Error ? err.message : String(err);
		}
	}

	async function handleUnshare(): Promise<void> {
		if (!selected) return;
		try {
			await revokeProfileShare(selected.id);
			showNotice(m.seed_profile_share_revoked());
		} catch (err) {
			profiles.error = err instanceof Error ? err.message : String(err);
		}
	}

	const status = $derived(profiles.status(currentNormalized));
	const selected = $derived(profiles.selected);
	const canSaveChanges = $derived(
		!!selected && (selected.scope === "user" || isAdmin),
	);
	const isDefault = $derived(
		!!selected && profiles.defaultProfileId === selected.id,
	);

	// Page navigation guard entry point (called from beforeNavigate).
	export function requestLeave(proceed: () => void): void {
		pendingLeaveProceed = proceed;
		unsavedMode = "leave";
		unsavedDialogOpen = true;
	}

	async function doSwitch(profileId: string): Promise<void> {
		pendingSwitchId = null;
		const loaded = await profiles.select(profileId, metadata);
		if (loaded) onapply(loaded);
	}

	function requestSwitch(profileId: string): void {
		if (profileId === selected?.id && status !== "modified") return;
		if (status === "modified") {
			pendingSwitchId = profileId;
			unsavedMode = "switch";
			unsavedDialogOpen = true;
		} else {
			void doSwitch(profileId);
		}
	}

	async function handleSaveChanges(): Promise<void> {
		if (!currentNormalized) return;
		const ok = await profiles.saveChanges(currentNormalized, metadata);
		if (ok && pendingSwitchId) await doSwitch(pendingSwitchId);
	}

	function openSaveAsNew(): void {
		saveDialogMode = "create";
		dialogFieldErrors = null;
		dialogError = null;
		saveDialogOpen = true;
	}

	function openEdit(): void {
		saveDialogMode = "edit";
		dialogFieldErrors = null;
		dialogError = null;
		saveDialogOpen = true;
	}

	// Admin editing an official preset: expose the curation fields.
	const officialEditActive = $derived(
		saveDialogMode === "edit" && selected?.scope === "official" && isAdmin,
	);

	async function handleSaveDialogSubmit(
		result: SaveDialogResult,
	): Promise<void> {
		dialogFieldErrors = null;
		dialogError = null;
		if (saveDialogMode === "edit") {
			if (!selected) return;
			const patch: Record<string, unknown> = {
				name: result.name,
				description: result.description || null,
			};
			if (officialEditActive) {
				patch.difficultyTag = result.difficultyTag || null;
				patch.gameTags = result.gameTags;
				patch.displayOrder = result.displayOrder;
			}
			const ok = await profiles.updateMeta(selected.id, patch);
			if (ok) {
				saveDialogOpen = false;
			} else {
				dialogError = profiles.error;
				profiles.error = null;
			}
			return;
		}
		if (!currentNormalized) return;
		try {
			await profiles.saveAsNew(
				{
					name: result.name,
					description: result.description || undefined,
					setAsDefault: result.setAsDefault,
					favorite: result.pin,
					// Only admins get the official option in the dialog; the
					// server independently enforces the admin requirement.
					scope: result.official && isAdmin ? "official" : undefined,
					slug: result.official ? result.slug : undefined,
					isRecommended: result.official
						? result.isRecommended
						: undefined,
				},
				currentNormalized,
				metadata,
			);
			saveDialogOpen = false;
			if (pendingSwitchId) await doSwitch(pendingSwitchId);
		} catch (err) {
			// Keep the dialog open with inline errors; the configuration stays
			// untouched and Modified.
			profiles.error = null;
			if (err instanceof ApiError) {
				dialogFieldErrors = err.fieldErrors ?? null;
				dialogError = err.fieldErrors ? null : err.message;
			} else {
				dialogError = err instanceof Error ? err.message : String(err);
			}
		}
	}

	function handleRevert(): void {
		const loaded = profiles.revert(metadata);
		if (loaded) onapply(loaded);
	}

	function menuItems(): MenuItem[] {
		const items: MenuItem[] = [];
		if (selected && isAuthenticated) {
			if (selected.scope === "user" || isAdmin) {
				items.push({
					id: "edit",
					label: m.seed_profile_action_edit(),
					onselect: openEdit,
				});
			}
			items.push({
				id: "duplicate",
				label: m.seed_profile_action_duplicate(),
				onselect: () => void profiles.duplicate(selected.id, metadata),
			});
			items.push({
				id: "default",
				label: isDefault
					? m.seed_profile_action_unset_default()
					: m.seed_profile_action_set_default(),
				onselect: () =>
					void profiles.setDefault(isDefault ? null : selected.id),
			});
			items.push({
				id: "pin",
				label: profiles.favorites.includes(selected.id)
					? m.seed_profile_action_unpin()
					: m.seed_profile_action_pin(),
				onselect: () => void profiles.togglePin(selected.id),
			});
			if (selected.scope === "user" || isAdmin) {
				items.push({
					id: "delete",
					label: m.seed_profile_action_delete(),
					danger: true,
					onselect: () => (deleteDialogOpen = true),
				});
			}
			if (selected.scope === "official" && isAdmin) {
				items.push({
					id: "archive",
					label: m.seed_profile_action_archive(),
					danger: true,
					onselect: () => void profiles.archive(selected.id),
				});
				if (!selected.isRecommended) {
					items.push({
						id: "recommend",
						label: m.seed_profile_action_set_recommended(),
						onselect: () =>
							void profiles.setRecommended(selected.id),
					});
				}
			}
		}
		// Sharing: official presets are public links; private profiles use a
		// revocable capability token (owner only).
		if (selected && (isAuthenticated || selected.scope === "official")) {
			items.push({
				id: "share",
				label: m.seed_profile_action_share(),
				onselect: () => void handleShare(),
			});
			if (isAuthenticated && selected.scope === "user") {
				items.push({
					id: "unshare",
					label: m.seed_profile_action_unshare(),
					onselect: () => void handleUnshare(),
				});
			}
		}
		if (profiles.recommendedId) {
			items.push({
				id: "reset-recommended",
				label: m.seed_profile_reset_to_recommended(),
				onselect: () => requestSwitch(profiles.recommendedId!),
			});
		}
		items.push({
			id: "reset",
			label: m.seed_profile_action_reset(),
			onselect: onreset,
		});
		if (isAuthenticated) {
			items.push({
				id: "manage",
				label: m.seed_profile_action_manage(),
				onselect: () => void goto("/profile/profiles"),
			});
		}
		return items;
	}

	const overflowItems = $derived(menuItems());
</script>

<div
	class="mb-3 rounded-lg border border-slate-200 bg-white px-3 py-2 shadow-md dark:border-slate-700 dark:bg-slate-800"
	data-testid="profile-toolbar"
>
	{#if sharedName && !sharedNoticeDismissed}
		<div
			class="mb-2 flex flex-wrap items-center gap-2 rounded border border-blue-300 bg-blue-100 px-3 py-2 text-xs text-blue-800 dark:border-blue-700 dark:bg-blue-900 dark:text-blue-200"
			role="status"
			data-testid="profile-shared-notice"
		>
			<span class="flex-1">
				{m.seed_profile_shared_notice({ name: sharedName })}
			</span>
			<button
				type="button"
				class="rounded p-0.5 text-blue-700 hover:text-blue-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 dark:text-blue-300 dark:hover:text-blue-100"
				aria-label="Dismiss"
				onclick={() => (sharedNoticeDismissed = true)}
			>
				<svg
					class="h-3.5 w-3.5"
					xmlns="http://www.w3.org/2000/svg"
					viewBox="0 0 20 20"
					fill="currentColor"
					aria-hidden="true"
				>
					<path
						d="M6.28 5.22a.75.75 0 0 0-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 1 0 1.06 1.06L10 11.06l3.72 3.72a.75.75 0 1 0 1.06-1.06L11.06 10l3.72-3.72a.75.75 0 0 0-1.06-1.06L10 8.94 6.28 5.22Z"
					/>
				</svg>
			</button>
		</div>
	{/if}

	{#if sharedInvalid}
		<div
			class="mb-2 rounded border border-yellow-400 bg-yellow-100 px-3 py-2 text-xs text-yellow-800 dark:border-yellow-700 dark:bg-yellow-900 dark:text-yellow-200"
			role="alert"
			data-testid="profile-shared-invalid"
		>
			{m.seed_profile_shared_invalid()}
		</div>
	{/if}

	{#if seedSourceId && !seedNoticeDismissed}
		<div
			class="mb-2 flex flex-wrap items-center gap-2 rounded border border-blue-300 bg-blue-100 px-3 py-2 text-xs text-blue-800 dark:border-blue-700 dark:bg-blue-900 dark:text-blue-200"
			role="status"
			data-testid="profile-seed-notice"
		>
			<span class="flex-1">
				{m.seed_profile_from_seed_notice({ id: seedSourceId })}
			</span>
			<button
				type="button"
				class="rounded p-0.5 text-blue-700 hover:text-blue-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 dark:text-blue-300 dark:hover:text-blue-100"
				aria-label="Dismiss"
				onclick={() => (seedNoticeDismissed = true)}
			>
				<svg
					class="h-3.5 w-3.5"
					viewBox="0 0 20 20"
					fill="currentColor"
					aria-hidden="true"
				>
					<path
						d="M6.28 5.22a.75.75 0 0 0-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 1 0 1.06 1.06L10 11.06l3.72 3.72a.75.75 0 1 0 1.06-1.06L11.06 10l3.72-3.72a.75.75 0 0 0-1.06-1.06L10 8.94 6.28 5.22Z"
					/>
				</svg>
			</button>
		</div>
	{/if}

	{#if seedSourceInvalid}
		<div
			class="mb-2 rounded border border-yellow-400 bg-yellow-100 px-3 py-2 text-xs text-yellow-800 dark:border-yellow-700 dark:bg-yellow-900 dark:text-yellow-200"
			role="alert"
			data-testid="profile-seed-invalid"
		>
			{m.seed_profile_from_seed_invalid()}
		</div>
	{/if}

	{#if notice}
		<div
			class="mb-2 rounded border border-green-400 bg-green-100 px-3 py-2 text-xs text-green-800 dark:border-green-700 dark:bg-green-900 dark:text-green-200"
			role="status"
			data-testid="profile-notice"
		>
			{notice}
		</div>
	{/if}

	{#if profiles.error}
		<div
			class="mb-2 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-700 dark:border-red-600 dark:bg-red-700 dark:text-red-200"
			role="alert"
			data-testid="profile-error"
		>
			{profiles.error}
		</div>
	{/if}

	{#if profiles.conflict}
		<div
			class="mb-2 flex flex-wrap items-center gap-2 rounded border border-yellow-400 bg-yellow-100 px-3 py-2 text-xs text-yellow-800 dark:border-yellow-700 dark:bg-yellow-900 dark:text-yellow-200"
			role="alert"
			data-testid="profile-conflict"
		>
			<span class="flex-1">{m.seed_profile_conflict_message()}</span>
			{#if selected}
				<Button
					type="button"
					variant="secondary"
					size="xs"
					onclick={() => void doSwitch(selected.id)}
				>
					{m.seed_profile_conflict_reload()}
				</Button>
			{/if}
		</div>
	{/if}

	{#if profiles.loadReport}
		<div
			class="mb-2 rounded border border-yellow-400 bg-yellow-100 px-3 py-2 text-xs text-yellow-800 dark:border-yellow-700 dark:bg-yellow-900 dark:text-yellow-200"
			role="status"
			data-testid="profile-load-report"
		>
			<p class="font-medium">{m.seed_profile_load_report_title()}</p>
			{#if profiles.loadReport.migrationSteps.length > 0}
				<p>
					{m.seed_profile_load_report_migrated({
						steps: profiles.loadReport.migrationSteps.join("; "),
					})}
				</p>
			{/if}
			{#if profiles.loadReport.removedKeys.length > 0}
				<p>
					{m.seed_profile_load_report_removed({
						keys: profiles.loadReport.removedKeys.join(", "),
					})}
				</p>
			{/if}
			{#if profiles.loadReport.resetKeys.length > 0}
				<p>
					{m.seed_profile_load_report_reset({
						keys: profiles.loadReport.resetKeys.join(", "),
					})}
				</p>
			{/if}
		</div>
	{/if}

	<div class="flex flex-wrap items-center gap-2">
		<ProfileCombobox
			officials={profiles.officials}
			mine={profiles.mine}
			favorites={profiles.favorites}
			selectedId={selected?.id ?? null}
			selectedName={selected?.name ?? null}
			modified={status === "modified"}
			title={selected?.description ?? null}
			disabled={profiles.loading || profiles.saving}
			canPin={isAuthenticated}
			onselect={requestSwitch}
			onpin={(id) => void profiles.togglePin(id)}
		/>

		<span
			class="flex flex-wrap items-center gap-1"
			data-testid="profile-badges"
		>
			{#if profiles.loading}
				<Badge variant="info" dot
					>{m.seed_profile_badge_loading()}</Badge
				>
			{:else if profiles.saving}
				<Badge variant="info" dot>{m.seed_profile_badge_saving()}</Badge
				>
			{:else if profiles.error}
				<Badge variant="danger" dot
					>{m.seed_profile_badge_error()}</Badge
				>
			{:else if !selected}
				<Badge variant="neutral">{m.seed_profile_badge_custom()}</Badge>
			{:else}
				{#if selected.scope === "official"}
					<Badge variant="info"
						>{m.seed_profile_badge_official()}</Badge
					>
				{:else}
					<Badge variant="neutral"
						>{m.seed_profile_badge_mine()}</Badge
					>
				{/if}
				{#if selected.archived}
					<Badge variant="warning"
						>{m.seed_profile_badge_archived()}</Badge
					>
				{/if}
				{#if isDefault}
					<Badge variant="success"
						>{m.seed_profile_badge_default()}</Badge
					>
				{/if}
				{#if status === "modified"}
					<Badge variant="warning" dot>
						{m.seed_profile_badge_modified()}
					</Badge>
				{/if}
			{/if}
		</span>

		{#if selected}
			<span
				class="flex flex-wrap items-center gap-1"
				data-testid="profile-tags"
			>
				{#if selected.difficultyTag}
					<Badge variant="neutral">{selected.difficultyTag}</Badge>
				{/if}
				{#each selected.gameTags ?? [] as tag (tag)}
					<Badge variant="neutral">{tag}</Badge>
				{/each}
			</span>
		{/if}

		{#if selected?.description}
			<span
				class="hidden min-w-0 text-xs text-slate-600 dark:text-slate-400 sm:inline"
				title={selected.description}
			>
				{selected.description}
			</span>
		{/if}

		<span class="ml-auto flex flex-wrap items-center gap-2">
			{#if !isAuthenticated}
				{#if status === "modified" || !selected}
					<Button href="/login" variant="secondary" size="xs">
						{m.seed_profile_login_to_save()}
					</Button>
				{/if}
			{:else if status === "modified"}
				{#if canSaveChanges}
					<Button
						type="button"
						variant="primary"
						size="xs"
						disabled={profiles.saving}
						onclick={() => void handleSaveChanges()}
						data-testid="profile-save-changes"
					>
						{m.seed_profile_save_changes()}
					</Button>
					<Button
						type="button"
						variant="secondary"
						size="xs"
						disabled={profiles.saving}
						onclick={openSaveAsNew}
						data-testid="profile-save-as-new"
					>
						{m.seed_profile_save_as_new()}
					</Button>
				{:else}
					<Button
						type="button"
						variant="primary"
						size="xs"
						disabled={profiles.saving}
						onclick={openSaveAsNew}
						data-testid="profile-save-as-mine"
					>
						{m.seed_profile_save_as_mine()}
					</Button>
				{/if}
				<Button
					type="button"
					variant="ghost"
					size="xs"
					disabled={profiles.saving}
					onclick={handleRevert}
					data-testid="profile-revert"
				>
					{m.seed_profile_revert()}
				</Button>
			{:else if !selected}
				<Button
					type="button"
					variant="primary"
					size="xs"
					disabled={profiles.saving}
					onclick={openSaveAsNew}
					data-testid="profile-save-as-new"
				>
					{m.seed_profile_save_as_new()}
				</Button>
			{/if}
			<ProfileOverflowMenu
				items={overflowItems}
				disabled={profiles.loading}
			/>
		</span>
	</div>
</div>

<ProfileSaveDialog
	bind:open={saveDialogOpen}
	mode={saveDialogMode}
	initialName={saveDialogMode === "edit" ? (selected?.name ?? "") : ""}
	initialDescription={saveDialogMode === "edit"
		? (selected?.description ?? "")
		: ""}
	allowOfficial={isAdmin && saveDialogMode === "create"}
	officialEdit={officialEditActive}
	initialDifficultyTag={selected?.difficultyTag ?? ""}
	initialGameTags={(selected?.gameTags ?? []).join(", ")}
	initialDisplayOrder={selected?.displayOrder ?? 0}
	saving={profiles.saving}
	fieldErrors={dialogFieldErrors}
	errorMessage={dialogError}
	onsubmit={(result) => void handleSaveDialogSubmit(result)}
	oncancel={() => {
		dialogFieldErrors = null;
		dialogError = null;
		pendingSwitchId = null;
	}}
/>

<UnsavedChangesDialog
	bind:open={unsavedDialogOpen}
	mode={unsavedMode}
	{canSaveChanges}
	canSaveAsNew={isAuthenticated}
	saving={profiles.saving}
	onsave={() => void handleSaveChanges().then(() => pendingLeaveProceed?.())}
	onsaveasnew={openSaveAsNew}
	ondiscard={() => {
		if (unsavedMode === "leave") {
			pendingLeaveProceed?.();
			pendingLeaveProceed = null;
		} else if (pendingSwitchId) {
			void doSwitch(pendingSwitchId);
		}
	}}
	oncancel={() => {
		pendingSwitchId = null;
		pendingLeaveProceed = null;
	}}
/>

<Modal
	bind:open={deleteDialogOpen}
	title={m.seed_profile_dialog_delete_title()}
>
	<p>
		{m.seed_profile_dialog_delete_message({ name: selected?.name ?? "" })}
	</p>
	{#snippet footer()}
		<Button
			type="button"
			variant="secondary"
			size="sm"
			data-autofocus
			onclick={() => (deleteDialogOpen = false)}
		>
			{m.seed_profile_dialog_cancel_button()}
		</Button>
		<Button
			type="button"
			variant="danger"
			size="sm"
			disabled={profiles.saving}
			onclick={async () => {
				if (selected) await profiles.remove(selected.id);
				deleteDialogOpen = false;
			}}
		>
			{m.seed_profile_dialog_delete_confirm()}
		</Button>
	{/snippet}
</Modal>
