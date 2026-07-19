<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import { goto } from "$app/navigation";
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Modal from "$lib/components/ui/Modal.svelte";
	import PresetCombobox from "./PresetCombobox.svelte";
	import PresetOverflowMenu, {
		type MenuItem,
	} from "./PresetOverflowMenu.svelte";
	import PresetSaveDialog, {
		type SaveDialogResult,
	} from "./PresetSaveDialog.svelte";
	import UnsavedChangesDialog from "./UnsavedChangesDialog.svelte";
	import type {
		PresetState,
		LoadedForm,
	} from "$lib/config/preset-state.svelte";
	import type { NormalizedConfig } from "$lib/config/normalize";
	import type { Metadata } from "$lib/types";
	import { presetConfigPath } from "$lib/config/preset-links";
	import {
		ApiError,
		ensurePresetShare,
		revokePresetShare,
	} from "$lib/services/data";

	interface Props {
		state: PresetState;
		metadata: Metadata;
		currentNormalized: NormalizedConfig | null;
		isAuthenticated: boolean;
		isAdmin: boolean;
		// Name of a preset loaded through a ?share= link, if any.
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
		state: presets,
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
			window.prompt(m.seed_preset_action_share(), text);
		}
	}

	async function handleShare(): Promise<void> {
		if (!selected) return;
		const base = `${window.location.origin}/config/${presets.configId}`;
		if (selected.scope === "official") {
			// Official presets use their stable, human-readable public slug.
			const path = presetConfigPath({
				presetId: selected.id,
				slug: selected.slug,
				configId: presets.configId,
			});
			if (!path) return;
			await copyToClipboard(`${window.location.origin}${path}`);
			showNotice(m.seed_preset_share_copied());
			return;
		}
		try {
			const { token } = await ensurePresetShare(selected.id);
			await copyToClipboard(`${base}?share=${token}`);
			showNotice(m.seed_preset_share_copied());
		} catch (err) {
			presets.error = err instanceof Error ? err.message : String(err);
		}
	}

	async function handleUnshare(): Promise<void> {
		if (!selected) return;
		try {
			await revokePresetShare(selected.id);
			showNotice(m.seed_preset_share_revoked());
		} catch (err) {
			presets.error = err instanceof Error ? err.message : String(err);
		}
	}

	const status = $derived(presets.status(currentNormalized));
	const selected = $derived(presets.selected);
	const canSaveChanges = $derived(
		!!selected && (selected.scope === "user" || isAdmin),
	);
	const isDefault = $derived(
		!!selected && presets.defaultPresetId === selected.id,
	);

	// Page navigation guard entry point (called from beforeNavigate).
	export function requestLeave(proceed: () => void): void {
		pendingLeaveProceed = proceed;
		unsavedMode = "leave";
		unsavedDialogOpen = true;
	}

	async function doSwitch(presetId: string): Promise<void> {
		pendingSwitchId = null;
		const loaded = await presets.select(presetId, metadata);
		if (loaded) onapply(loaded);
	}

	function requestSwitch(presetId: string): void {
		if (presetId === selected?.id && status !== "modified") return;
		if (status === "modified") {
			pendingSwitchId = presetId;
			unsavedMode = "switch";
			unsavedDialogOpen = true;
		} else {
			void doSwitch(presetId);
		}
	}

	async function handleSaveChanges(): Promise<void> {
		if (!currentNormalized) return;
		const ok = await presets.saveChanges(currentNormalized, metadata);
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
			const ok = await presets.updateMeta(selected.id, patch);
			if (ok) {
				saveDialogOpen = false;
			} else {
				dialogError = presets.error;
				presets.error = null;
			}
			return;
		}
		if (!currentNormalized) return;
		try {
			await presets.saveAsNew(
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
			presets.error = null;
			if (err instanceof ApiError) {
				dialogFieldErrors = err.fieldErrors ?? null;
				dialogError = err.fieldErrors ? null : err.message;
			} else {
				dialogError = err instanceof Error ? err.message : String(err);
			}
		}
	}

	function handleRevert(): void {
		const loaded = presets.revert(metadata);
		if (loaded) onapply(loaded);
	}

	function menuItems(): MenuItem[] {
		const items: MenuItem[] = [];
		if (selected && isAuthenticated) {
			if (selected.scope === "user" || isAdmin) {
				items.push({
					id: "edit",
					label: m.seed_preset_action_edit(),
					onselect: openEdit,
				});
			}
			items.push({
				id: "duplicate",
				label: m.seed_preset_action_duplicate(),
				onselect: () => void presets.duplicate(selected.id, metadata),
			});
			items.push({
				id: "default",
				label: isDefault
					? m.seed_preset_action_unset_default()
					: m.seed_preset_action_set_default(),
				onselect: () =>
					void presets.setDefault(isDefault ? null : selected.id),
			});
			items.push({
				id: "pin",
				label: presets.favorites.includes(selected.id)
					? m.seed_preset_action_unpin()
					: m.seed_preset_action_pin(),
				onselect: () => void presets.togglePin(selected.id),
			});
			if (selected.scope === "user" || isAdmin) {
				items.push({
					id: "delete",
					label: m.seed_preset_action_delete(),
					danger: true,
					onselect: () => (deleteDialogOpen = true),
				});
			}
			if (selected.scope === "official" && isAdmin) {
				items.push({
					id: "archive",
					label: m.seed_preset_action_archive(),
					danger: true,
					onselect: () => void presets.archive(selected.id),
				});
				if (!selected.isRecommended) {
					items.push({
						id: "recommend",
						label: m.seed_preset_action_set_recommended(),
						onselect: () =>
							void presets.setRecommended(selected.id),
					});
				}
			}
		}
		// Sharing: official presets are public links; private presets use a
		// revocable capability token (owner only).
		if (selected && (isAuthenticated || selected.scope === "official")) {
			items.push({
				id: "share",
				label: m.seed_preset_action_share(),
				onselect: () => void handleShare(),
			});
			if (isAuthenticated && selected.scope === "user") {
				items.push({
					id: "unshare",
					label: m.seed_preset_action_unshare(),
					onselect: () => void handleUnshare(),
				});
			}
		}
		if (presets.recommendedId) {
			items.push({
				id: "reset-recommended",
				label: m.seed_preset_reset_to_recommended(),
				onselect: () => requestSwitch(presets.recommendedId!),
			});
		}
		items.push({
			id: "reset",
			label: m.seed_preset_action_reset(),
			onselect: onreset,
		});
		if (isAuthenticated) {
			items.push({
				id: "manage",
				label: m.seed_preset_action_manage(),
				onselect: () => void goto("/profile/presets"),
			});
		}
		return items;
	}

	const overflowItems = $derived(menuItems());
</script>

<div
	class="mb-3 rounded-lg border border-slate-200 bg-white px-3 py-2 shadow-md dark:border-slate-700 dark:bg-slate-800"
	data-testid="preset-toolbar"
>
	{#if sharedName && !sharedNoticeDismissed}
		<div
			class="mb-2 flex flex-wrap items-center gap-2 rounded border border-blue-300 bg-blue-100 px-3 py-2 text-xs text-blue-800 dark:border-blue-700 dark:bg-blue-900 dark:text-blue-200"
			role="status"
			data-testid="preset-shared-notice"
		>
			<span class="flex-1">
				{m.seed_preset_shared_notice({ name: sharedName })}
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
			data-testid="preset-shared-invalid"
		>
			{m.seed_preset_shared_invalid()}
		</div>
	{/if}

	{#if seedSourceId && !seedNoticeDismissed}
		<div
			class="mb-2 flex flex-wrap items-center gap-2 rounded border border-blue-300 bg-blue-100 px-3 py-2 text-xs text-blue-800 dark:border-blue-700 dark:bg-blue-900 dark:text-blue-200"
			role="status"
			data-testid="preset-seed-notice"
		>
			<span class="flex-1">
				{m.seed_preset_from_seed_notice({ id: seedSourceId })}
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
			data-testid="preset-seed-invalid"
		>
			{m.seed_preset_from_seed_invalid()}
		</div>
	{/if}

	{#if notice}
		<div
			class="mb-2 rounded border border-green-400 bg-green-100 px-3 py-2 text-xs text-green-800 dark:border-green-700 dark:bg-green-900 dark:text-green-200"
			role="status"
			data-testid="preset-notice"
		>
			{notice}
		</div>
	{/if}

	{#if presets.error}
		<div
			class="mb-2 rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-700 dark:border-red-600 dark:bg-red-700 dark:text-red-200"
			role="alert"
			data-testid="preset-error"
		>
			{presets.error}
		</div>
	{/if}

	{#if presets.conflict}
		<div
			class="mb-2 flex flex-wrap items-center gap-2 rounded border border-yellow-400 bg-yellow-100 px-3 py-2 text-xs text-yellow-800 dark:border-yellow-700 dark:bg-yellow-900 dark:text-yellow-200"
			role="alert"
			data-testid="preset-conflict"
		>
			<span class="flex-1">{m.seed_preset_conflict_message()}</span>
			{#if selected}
				<Button
					type="button"
					variant="secondary"
					size="xs"
					onclick={() => void doSwitch(selected.id)}
				>
					{m.seed_preset_conflict_reload()}
				</Button>
			{/if}
		</div>
	{/if}

	{#if presets.loadReport}
		<div
			class="mb-2 rounded border border-yellow-400 bg-yellow-100 px-3 py-2 text-xs text-yellow-800 dark:border-yellow-700 dark:bg-yellow-900 dark:text-yellow-200"
			role="status"
			data-testid="preset-load-report"
		>
			<p class="font-medium">{m.seed_preset_load_report_title()}</p>
			{#if presets.loadReport.migrationSteps.length > 0}
				<p>
					{m.seed_preset_load_report_migrated({
						steps: presets.loadReport.migrationSteps.join("; "),
					})}
				</p>
			{/if}
			{#if presets.loadReport.removedKeys.length > 0}
				<p>
					{m.seed_preset_load_report_removed({
						keys: presets.loadReport.removedKeys.join(", "),
					})}
				</p>
			{/if}
			{#if presets.loadReport.resetKeys.length > 0}
				<p>
					{m.seed_preset_load_report_reset({
						keys: presets.loadReport.resetKeys.join(", "),
					})}
				</p>
			{/if}
		</div>
	{/if}

	<div class="flex flex-wrap items-center gap-2">
		<PresetCombobox
			officials={presets.officials}
			mine={presets.mine}
			favorites={presets.favorites}
			selectedId={selected?.id ?? null}
			selectedName={selected?.name ?? null}
			modified={status === "modified"}
			title={selected?.description ?? null}
			disabled={presets.loading || presets.saving}
			canPin={isAuthenticated}
			onselect={requestSwitch}
			onpin={(id) => void presets.togglePin(id)}
		/>

		<span
			class="flex flex-wrap items-center gap-1"
			data-testid="preset-badges"
		>
			{#if presets.loading}
				<Badge variant="info" dot
					>{m.seed_preset_badge_loading()}</Badge
				>
			{:else if presets.saving}
				<Badge variant="info" dot>{m.seed_preset_badge_saving()}</Badge
				>
			{:else if presets.error}
				<Badge variant="danger" dot
					>{m.seed_preset_badge_error()}</Badge
				>
			{:else if !selected}
				<Badge variant="neutral">{m.seed_preset_badge_custom()}</Badge>
			{:else}
				{#if selected.scope === "official"}
					<Badge variant="info"
						>{m.seed_preset_badge_official()}</Badge
					>
				{:else}
					<Badge variant="neutral"
						>{m.seed_preset_badge_mine()}</Badge
					>
				{/if}
				{#if selected.archived}
					<Badge variant="warning"
						>{m.seed_preset_badge_archived()}</Badge
					>
				{/if}
				{#if isDefault}
					<Badge variant="success"
						>{m.seed_preset_badge_default()}</Badge
					>
				{/if}
				{#if status === "modified"}
					<Badge variant="warning" dot>
						{m.seed_preset_badge_modified()}
					</Badge>
				{/if}
			{/if}
		</span>

		{#if selected}
			<span
				class="flex flex-wrap items-center gap-1"
				data-testid="preset-tags"
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
						{m.seed_preset_login_to_save()}
					</Button>
				{/if}
			{:else if status === "modified"}
				{#if canSaveChanges}
					<Button
						type="button"
						variant="primary"
						size="xs"
						disabled={presets.saving}
						onclick={() => void handleSaveChanges()}
						data-testid="preset-save-changes"
					>
						{m.seed_preset_save_changes()}
					</Button>
					<Button
						type="button"
						variant="secondary"
						size="xs"
						disabled={presets.saving}
						onclick={openSaveAsNew}
						data-testid="preset-save-as-new"
					>
						{m.seed_preset_save_as_new()}
					</Button>
				{:else}
					<Button
						type="button"
						variant="primary"
						size="xs"
						disabled={presets.saving}
						onclick={openSaveAsNew}
						data-testid="preset-save-as-mine"
					>
						{m.seed_preset_save_as_mine()}
					</Button>
				{/if}
				<Button
					type="button"
					variant="ghost"
					size="xs"
					disabled={presets.saving}
					onclick={handleRevert}
					data-testid="preset-revert"
				>
					{m.seed_preset_revert()}
				</Button>
			{:else if !selected}
				<Button
					type="button"
					variant="primary"
					size="xs"
					disabled={presets.saving}
					onclick={openSaveAsNew}
					data-testid="preset-save-as-new"
				>
					{m.seed_preset_save_as_new()}
				</Button>
			{/if}
			<PresetOverflowMenu
				items={overflowItems}
				disabled={presets.loading}
			/>
		</span>
	</div>
</div>

<PresetSaveDialog
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
	saving={presets.saving}
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
	saving={presets.saving}
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
	title={m.seed_preset_dialog_delete_title()}
>
	<p>
		{m.seed_preset_dialog_delete_message({ name: selected?.name ?? "" })}
	</p>
	{#snippet footer()}
		<Button
			type="button"
			variant="secondary"
			size="sm"
			data-autofocus
			onclick={() => (deleteDialogOpen = false)}
		>
			{m.seed_preset_dialog_cancel_button()}
		</Button>
		<Button
			type="button"
			variant="danger"
			size="sm"
			disabled={presets.saving}
			onclick={async () => {
				if (selected) await presets.remove(selected.id);
				deleteDialogOpen = false;
			}}
		>
			{m.seed_preset_dialog_delete_confirm()}
		</Button>
	{/snippet}
</Modal>
