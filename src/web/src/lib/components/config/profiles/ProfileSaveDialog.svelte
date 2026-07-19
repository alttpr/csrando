<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import Modal from "$lib/components/ui/Modal.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import {
		MAX_PROFILE_DESCRIPTION_LENGTH,
		MAX_PROFILE_NAME_LENGTH,
	} from "$lib/config/constants";

	export interface SaveDialogResult {
		name: string;
		description: string;
		setAsDefault: boolean;
		pin: boolean;
		// Admin only: create the profile as a globally visible official preset.
		official: boolean;
		slug: string;
		isRecommended: boolean;
		// Admin only, when editing an official preset.
		difficultyTag: string;
		gameTags: string[];
		displayOrder: number;
	}

	interface Props {
		open?: boolean;
		mode?: "create" | "edit";
		initialName?: string;
		initialDescription?: string;
		// Offer the admin-only "official preset" fields on create.
		allowOfficial?: boolean;
		// Editing an official preset (admin): expose curation fields.
		officialEdit?: boolean;
		initialDifficultyTag?: string;
		initialGameTags?: string;
		initialDisplayOrder?: number;
		saving?: boolean;
		fieldErrors?: Record<string, string> | null;
		errorMessage?: string | null;
		onsubmit: (result: SaveDialogResult) => void;
		oncancel?: () => void;
	}

	let {
		open = $bindable(false),
		mode = "create",
		initialName = "",
		initialDescription = "",
		allowOfficial = false,
		officialEdit = false,
		initialDifficultyTag = "",
		initialGameTags = "",
		initialDisplayOrder = 0,
		saving = false,
		fieldErrors = null,
		errorMessage = null,
		onsubmit,
		oncancel = undefined,
	}: Props = $props();

	let name = $state("");
	let description = $state("");
	let setAsDefault = $state(false);
	let pin = $state(false);
	let official = $state(false);
	let slug = $state("");
	let isRecommended = $state(false);
	let difficultyTag = $state("");
	let gameTagsText = $state("");
	let displayOrder = $state(0);
	let localError = $state<string | null>(null);
	let localSlugError = $state<string | null>(null);

	$effect(() => {
		if (open) {
			name = initialName;
			description = initialDescription;
			setAsDefault = false;
			pin = false;
			official = false;
			slug = "";
			isRecommended = false;
			difficultyTag = initialDifficultyTag;
			gameTagsText = initialGameTags;
			displayOrder = initialDisplayOrder;
			localError = null;
			localSlugError = null;
		}
	});

	function submit(event: SubmitEvent) {
		event.preventDefault();
		const trimmed = name.trim();
		if (!trimmed) {
			localError = m.form_error_required();
			return;
		}
		const trimmedSlug = slug.trim().toLowerCase();
		if (official) {
			if (!trimmedSlug) {
				localSlugError = m.form_error_required();
				return;
			}
			if (!/^[a-z0-9-]+$/.test(trimmedSlug)) {
				localSlugError = m.seed_profile_dialog_slug_invalid();
				return;
			}
		}
		localError = null;
		localSlugError = null;
		onsubmit({
			name: trimmed,
			description: description.trim(),
			setAsDefault: official ? false : setAsDefault,
			pin: official ? false : pin,
			official,
			slug: trimmedSlug,
			isRecommended: official && isRecommended,
			difficultyTag: difficultyTag.trim(),
			gameTags: gameTagsText
				.split(",")
				.map((tag) => tag.trim())
				.filter(Boolean),
			displayOrder: Number.isFinite(displayOrder) ? displayOrder : 0,
		});
	}

	const nameError = $derived(localError ?? fieldErrors?.name ?? null);
	const slugError = $derived(localSlugError ?? fieldErrors?.slug ?? null);
</script>

<Modal
	bind:open
	title={mode === "edit"
		? m.seed_profile_dialog_edit_title()
		: m.seed_profile_dialog_save_title()}
	onclose={() => oncancel?.()}
>
	<form onsubmit={submit} class="space-y-3">
		{#if errorMessage}
			<div
				class="rounded border border-red-400 bg-red-100 px-3 py-2 text-xs text-red-700 dark:border-red-600 dark:bg-red-700 dark:text-red-200"
				role="alert"
			>
				{errorMessage}
			</div>
		{/if}
		<div>
			<label
				for="profile-name-input"
				class="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300"
			>
				{m.seed_profile_dialog_name_label()}
			</label>
			<input
				id="profile-name-input"
				type="text"
				bind:value={name}
				maxlength={MAX_PROFILE_NAME_LENGTH}
				required
				data-autofocus
				aria-invalid={!!nameError}
				aria-describedby={nameError ? "profile-name-error" : undefined}
				class="w-full rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			/>
			{#if nameError}
				<p
					id="profile-name-error"
					class="mt-1 text-xs text-red-600 dark:text-red-400"
				>
					{nameError}
				</p>
			{/if}
		</div>
		<div>
			<label
				for="profile-description-input"
				class="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300"
			>
				{m.seed_profile_dialog_description_label()}
			</label>
			<textarea
				id="profile-description-input"
				bind:value={description}
				maxlength={MAX_PROFILE_DESCRIPTION_LENGTH}
				rows="2"
				class="w-full rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			></textarea>
		</div>
		{#if mode === "edit" && officialEdit}
			<div>
				<label
					for="profile-difficulty-input"
					class="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300"
				>
					{m.seed_profile_dialog_difficulty_label()}
				</label>
				<input
					id="profile-difficulty-input"
					type="text"
					bind:value={difficultyTag}
					maxlength="32"
					class="w-full rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
				/>
			</div>
			<div>
				<label
					for="profile-tags-input"
					class="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300"
				>
					{m.seed_profile_dialog_tags_label()}
				</label>
				<input
					id="profile-tags-input"
					type="text"
					bind:value={gameTagsText}
					placeholder="race, beginner"
					class="w-full rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
				/>
			</div>
			<div>
				<label
					for="profile-order-input"
					class="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300"
				>
					{m.seed_profile_dialog_order_label()}
				</label>
				<input
					id="profile-order-input"
					type="number"
					bind:value={displayOrder}
					class="w-32 rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
				/>
			</div>
		{/if}
		{#if mode === "create"}
			{#if !official}
				<div class="space-y-1.5">
					<label
						class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300"
					>
						<input
							type="checkbox"
							bind:checked={setAsDefault}
							class="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800"
						/>
						{m.seed_profile_dialog_set_default_label()}
					</label>
					<label
						class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300"
					>
						<input
							type="checkbox"
							bind:checked={pin}
							class="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800"
						/>
						{m.seed_profile_dialog_pin_label()}
					</label>
				</div>
			{/if}
			{#if allowOfficial}
				<div
					class="space-y-1.5 rounded border border-blue-300 bg-blue-50 p-2 dark:border-blue-800 dark:bg-blue-950"
				>
					<label
						class="flex items-center gap-2 text-sm font-medium text-blue-900 dark:text-blue-200"
					>
						<input
							type="checkbox"
							bind:checked={official}
							class="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800"
						/>
						{m.seed_profile_dialog_official_label()}
					</label>
					{#if official}
						<div>
							<label
								for="profile-slug-input"
								class="mb-1 block text-xs font-medium text-slate-700 dark:text-slate-300"
							>
								{m.seed_profile_dialog_slug_label()}
							</label>
							<input
								id="profile-slug-input"
								type="text"
								bind:value={slug}
								maxlength="64"
								placeholder="e.g. weekly-race"
								aria-invalid={!!slugError}
								aria-describedby={slugError
									? "profile-slug-error"
									: undefined}
								class="w-full rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
							/>
							{#if slugError}
								<p
									id="profile-slug-error"
									class="mt-1 text-xs text-red-600 dark:text-red-400"
								>
									{slugError}
								</p>
							{/if}
						</div>
						<label
							class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300"
						>
							<input
								type="checkbox"
								bind:checked={isRecommended}
								class="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800"
							/>
							{m.seed_profile_dialog_recommended_label()}
						</label>
					{/if}
				</div>
			{/if}
		{/if}
		<div class="flex justify-end gap-2 pt-1">
			<Button
				type="button"
				variant="secondary"
				size="sm"
				onclick={() => {
					open = false;
					oncancel?.();
				}}
			>
				{m.seed_profile_dialog_cancel_button()}
			</Button>
			<Button type="submit" variant="primary" size="sm" disabled={saving}>
				{saving
					? m.seed_profile_badge_saving()
					: m.seed_profile_dialog_save_button()}
			</Button>
		</div>
	</form>
</Modal>
