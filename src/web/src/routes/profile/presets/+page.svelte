<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import Badge from "$lib/components/ui/Badge.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Modal from "$lib/components/ui/Modal.svelte";
	import PresetSaveDialog, {
		type SaveDialogResult,
	} from "$lib/components/config/presets/PresetSaveDialog.svelte";
	import {
		ApiError,
		deletePreset,
		duplicatePreset,
		patchPreset,
		savePresetFavorite,
		savePresetPreferences,
	} from "$lib/services/data";
	import type {
		PresetPreferencesDto,
		PresetSummaryDto,
	} from "$lib/schemas/presets";
	import { presetConfigPath } from "$lib/config/preset-links";

	interface PageData {
		presets: PresetSummaryDto[];
		preferences: PresetPreferencesDto;
	}

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let presets = $state<PresetSummaryDto[]>([...data.presets]);
	let defaultPresetId = $state<string | null>(
		data.preferences.defaultPresetId,
	);
	let favorites = $state<string[]>(
		data.preferences.favorites.map((f) => f.presetId),
	);
	let search = $state("");
	let error = $state<string | null>(null);
	let busy = $state(false);

	let renameTarget = $state<PresetSummaryDto | null>(null);
	let renameOpen = $state(false);
	let renameFieldErrors = $state<Record<string, string> | null>(null);
	let deleteTarget = $state<PresetSummaryDto | null>(null);
	let deleteOpen = $state(false);

	const filtered = $derived(
		presets.filter((p) => {
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

	function openRename(preset: PresetSummaryDto) {
		renameTarget = preset;
		renameFieldErrors = null;
		renameOpen = true;
	}

	async function handleRename(result: SaveDialogResult) {
		if (!renameTarget) return;
		const target = renameTarget;
		renameFieldErrors = null;
		try {
			const { preset } = await patchPreset(target.id, {
				name: result.name,
				description: result.description || null,
			});
			presets = presets.map((p) => (p.id === target.id ? preset : p));
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

	async function handleDuplicate(preset: PresetSummaryDto) {
		await run(async () => {
			const detail = await duplicatePreset(preset.id);
			presets = [...presets, detail.preset];
		});
	}

	async function handleSetDefault(preset: PresetSummaryDto) {
		const next = defaultPresetId === preset.id ? null : preset.id;
		await run(async () => {
			await savePresetPreferences({ defaultPresetId: next });
			defaultPresetId = next;
		});
	}

	async function handleTogglePin(preset: PresetSummaryDto) {
		const pinned = favorites.includes(preset.id);
		await run(async () => {
			await savePresetFavorite(preset.id, !pinned);
			favorites = pinned
				? favorites.filter((id) => id !== preset.id)
				: [...favorites, preset.id];
		});
	}

	async function handleDelete() {
		if (!deleteTarget) return;
		const target = deleteTarget;
		await run(async () => {
			await deletePreset(target.id);
			presets = presets.filter((p) => p.id !== target.id);
			if (defaultPresetId === target.id) defaultPresetId = null;
			favorites = favorites.filter((id) => id !== target.id);
		});
		deleteOpen = false;
		deleteTarget = null;
	}
</script>

<section aria-labelledby="presets-heading">
	<div class="mb-5 flex flex-wrap items-center justify-between gap-3">
		<h2 id="presets-heading" class="text-xl font-semibold text-slate-900 dark:text-slate-100">
			{m.seed_preset_manage_title()}
		</h2>
		<a
			href="/content/presets"
			class="text-sm font-medium text-blue-600 underline hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300"
		>
			Preset guide
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

	{#if presets.length === 0}
		<p class="text-lg text-slate-700 dark:text-slate-300">
			{m.seed_preset_manage_empty()}
		</p>
	{:else}
		<div class="mb-4 max-w-sm">
			<label class="sr-only" for="preset-search">
				{m.seed_preset_manage_search_label()}
			</label>
			<input
				id="preset-search"
				type="search"
				bind:value={search}
				placeholder={m.seed_preset_manage_search_label()}
				class="w-full rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
			/>
		</div>

		{#if filtered.length === 0}
			<p class="text-slate-600 dark:text-slate-400">
				{m.seed_preset_manage_no_matches()}
			</p>
		{/if}

		<div class="space-y-3">
			{#each filtered as preset (preset.id)}
				<div
					class="bg-white dark:bg-slate-800 p-4 rounded-lg shadow hover:shadow-md transition-shadow"
				>
					<div class="flex flex-wrap items-center gap-2">
						<span
							class="text-lg font-semibold text-slate-900 dark:text-slate-100"
						>
							{preset.name}
						</span>
						<Badge variant="neutral">{preset.configId}</Badge>
						{#if defaultPresetId === preset.id}
							<Badge variant="success">
								{m.seed_preset_badge_default()}
							</Badge>
						{/if}
						{#if favorites.includes(preset.id)}
							<Badge variant="warning">
								{m.seed_preset_action_pin()}
							</Badge>
						{/if}
						{#if preset.revisionNumber !== null}
							<Badge variant="neutral">
								{m.seed_preset_badge_revision({
									revision: String(preset.revisionNumber),
								})}
							</Badge>
						{/if}
					</div>
					<p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
						{preset.description || m.seed_preset_description_none()}
					</p>
					<p class="mt-0.5 text-xs text-slate-500 dark:text-slate-500">
						{m.seed_preset_games_label({
							games: preset.selectedGames.join(", "),
						})}
						{#if preset.updatedAt}
							&middot;
							{m.seed_preset_manage_updated({
								date: new Date(preset.updatedAt).toLocaleString(),
							})}
						{/if}
					</p>
					<div class="mt-3 flex flex-wrap gap-2">
						<Button
							href={presetConfigPath({
								presetId: preset.id,
								slug: preset.slug,
								configId: preset.configId,
							}) ?? undefined}
							variant="primary"
							size="xs"
						>
							{m.seed_preset_manage_open()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => openRename(preset)}
						>
							{m.seed_preset_action_edit()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => void handleDuplicate(preset)}
						>
							{m.seed_preset_action_duplicate()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => void handleSetDefault(preset)}
						>
							{defaultPresetId === preset.id
								? m.seed_preset_action_unset_default()
								: m.seed_preset_action_set_default()}
						</Button>
						<Button
							type="button"
							variant="secondary"
							size="xs"
							disabled={busy}
							onclick={() => void handleTogglePin(preset)}
						>
							{favorites.includes(preset.id)
								? m.seed_preset_action_unpin()
								: m.seed_preset_action_pin()}
						</Button>
						<Button
							type="button"
							variant="danger"
							size="xs"
							disabled={busy}
							onclick={() => {
								deleteTarget = preset;
								deleteOpen = true;
							}}
						>
							{m.seed_preset_action_delete()}
						</Button>
					</div>
				</div>
			{/each}
		</div>
	{/if}
</section>

<PresetSaveDialog
	bind:open={renameOpen}
	mode="edit"
	initialName={renameTarget?.name ?? ""}
	initialDescription={renameTarget?.description ?? ""}
	saving={busy}
	fieldErrors={renameFieldErrors}
	onsubmit={(result) => void handleRename(result)}
	oncancel={() => (renameFieldErrors = null)}
/>

<Modal bind:open={deleteOpen} title={m.seed_preset_dialog_delete_title()}>
	<p>
		{m.seed_preset_dialog_delete_message({
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
			{m.seed_preset_dialog_cancel_button()}
		</Button>
		<Button
			type="button"
			variant="danger"
			size="sm"
			disabled={busy}
			onclick={() => void handleDelete()}
		>
			{m.seed_preset_dialog_delete_confirm()}
		</Button>
	{/snippet}
</Modal>
