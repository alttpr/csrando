<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import { goto, beforeNavigate } from "$app/navigation";
	import GameSelector from "$lib/components/config/GameSelector.svelte";
	import OptionForm from "$lib/components/config/OptionForm.svelte";
	import GameTabs from "$lib/components/config/GameTabs.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Select from "$lib/components/ui/Select.svelte";
	import Toggle from "$lib/components/ui/Toggle.svelte";
	import PresetToolbar from "$lib/components/config/presets/PresetToolbar.svelte";
	import { createSeed } from "$lib/services/data";
	import {
		initializeFormValues,
		normalizeConfig,
		buildRandomizePayload,
		hydrateFormState,
		configsEqual,
		type NormalizedConfig,
	} from "$lib/config/normalize";
	import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
	import { PresetState, type LoadedForm } from "$lib/config/preset-state.svelte";
	import { resolveStartupSelection } from "$lib/config/preset-selection";
	import {
		clearDraft,
		loadDraft,
		loadLocalLastUsedPreset,
		saveDraft,
	} from "$lib/config/preset-storage";
	import type { PresetListResponseDto } from "$lib/schemas/presets";
	import type { Metadata, SingleChoiceSetting } from "$lib/types";

	interface PageData {
		metadata: Metadata | null;
		error: string | null;
		configId?: string;
		presetBootstrap?: PresetListResponseDto | null;
		queryPresetId?: string | null;
		sharedPreset?: {
			name: string;
			description: string | null;
			settings: unknown;
			configSchemaVersion: number;
		} | null;
		sharedInvalid?: boolean;
		seedSettings?: {
			seedId: string;
			settings: unknown;
			configSchemaVersion: number;
		} | null;
		seedSettingsInvalid?: boolean;
		// Provided by the root layout load.
		user?: { username: string; isAdmin?: boolean } | null;
	}

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	const metadata = $derived(data.metadata);
	const pageError = $derived(data.error);
	const configId = $derived(data.configId ?? "");
	const user = $derived(data.user ?? null);
	const isAdmin = $derived(!!user?.isAdmin);

	let formValues: {
		global: { [key: string]: unknown };
		perGame: { [gameKey: string]: { [key: string]: unknown } };
	} = $state({ global: {}, perGame: {} });

	let availableGames: Array<{
		id: string;
		name: string;
		description?: string;
	}> = $state([]);
	let selectedGames: string[] = $state([]);
	let activeGameTab: string | null = $state(null);

	function resetFormState(newMetadata: Metadata | null) {
		if (!newMetadata) {
			formValues = { global: {}, perGame: {} };
			availableGames = [];
			selectedGames = [];
			activeGameTab = null;
			return;
		}

		const initData = initializeFormValues(newMetadata);
		formValues = {
			global: { ...initData.formGlobal },
			perGame: { ...initData.formPerGame },
		};
		availableGames = [...initData.availableGames];
		selectedGames = [...initData.selectedGames];
		activeGameTab = initData.activeTab;
	}

	function getMetadataSignature(meta: Metadata | null): string {
		if (!meta) return "null";
		try {
			return JSON.stringify(meta);
		} catch (err) {
			console.warn(
				"Failed to serialize metadata for change detection",
				err,
			);
			return `${Date.now()}`;
		}
	}

	let lastMetadataSignature: string | null = null;

	let generating = $state(false);
	let formSubmissionError: string | null = $state(null);
	let includeSpoiler = $state(true);

	let presetState = $state<PresetState | null>(null);
	let toolbarRef = $state<ReturnType<typeof PresetToolbar> | null>(null);
	// Skip the unsaved-changes guard for navigations we initiate deliberately
	// (seed generation, confirmed leave).
	let bypassNavigationGuard = false;
	let presetInitToken = 0;

	function applyLoadedForm(loaded: LoadedForm) {
		formValues = {
			global: { ...loaded.form.global },
			perGame: Object.fromEntries(
				Object.entries(loaded.form.perGame).map(([game, values]) => [
					game,
					{ ...values },
				]),
			),
		};
		selectedGames = [...loaded.form.selectedGames];
		if (loaded.activeTab) activeGameTab = loaded.activeTab;
	}

	// Name of a preset loaded through a ?share= link, shown in the toolbar.
	let sharedPresetName = $state<string | null>(null);
	// Seed id whose settings were loaded through ?fromSeed=.
	let seedSourceId = $state<string | null>(null);

	// Startup selection: ?share= link > ?fromSeed= seed settings > explicit
	// preset path (or legacy query) > recoverable draft > default > last-used >
	// recommended preset > defaults.
	async function initializePresets(meta: Metadata) {
		const token = ++presetInitToken;
		const state = new PresetState(configId, !!user);
		if (data.presetBootstrap) {
			state.applyList(data.presetBootstrap);
		} else {
			await state.refreshList();
			if (token !== presetInitToken) return;
		}
		presetState = state;

		if (data.sharedPreset) {
			const loaded = state.applySharedSettings(
				data.sharedPreset.settings as NormalizedConfig,
				data.sharedPreset.configSchemaVersion,
				meta,
			);
			if (token === presetInitToken && loaded) {
				applyLoadedForm(loaded);
				sharedPresetName = data.sharedPreset.name;
			}
			return;
		}

		if (data.seedSettings) {
			const loaded = state.applySharedSettings(
				data.seedSettings.settings as NormalizedConfig,
				data.seedSettings.configSchemaVersion,
				meta,
			);
			if (token === presetInitToken && loaded) {
				applyLoadedForm(loaded);
				seedSourceId = data.seedSettings.seedId;
			}
			return;
		}

		const draft = loadDraft(configId);
		const selection = resolveStartupSelection(
			{
				queryPresetId: data.queryPresetId,
				defaultPresetId: state.defaultPresetId,
				lastUsedPresetId:
					data.presetBootstrap?.preferences?.lastUsedPresetId ?? null,
				localLastUsedPresetId: loadLocalLastUsedPreset(configId),
				recommendedId: state.recommendedId,
				knownPresetIds: state.allPresets().map((p) => p.id),
			},
			draft,
		);

		if (selection.kind === "draft") {
			const loaded = await state.applyDraft(selection.draft, meta);
			if (token === presetInitToken && loaded) applyLoadedForm(loaded);
		} else if (selection.kind === "preset") {
			const loaded = await state.select(selection.presetId, meta, {
				recordUse: false,
			});
			if (token === presetInitToken && loaded) applyLoadedForm(loaded);
		}
	}

	$effect(() => {
		const meta = metadata ?? null;
		const signature = getMetadataSignature(meta);
		if (signature === lastMetadataSignature) return;
		lastMetadataSignature = signature;
		resetFormState(meta);
		generating = false;
		formSubmissionError = null;
		presetState = null;
		if (meta && configId) {
			void initializePresets(meta);
		} else {
			presetInitToken++;
		}
	});

	const currentNormalized = $derived(
		metadata
			? normalizeConfig(
					{
						selectedGames,
						global: formValues.global,
						perGame: formValues.perGame,
					},
					metadata,
				)
			: null,
	);
	const defaultsNormalized = $derived(
		metadata
			? normalizeConfig(hydrateFormState({}, metadata).form, metadata)
			: null,
	);
	const presetStatus = $derived(
		presetState ? presetState.status(currentNormalized) : "custom",
	);

	// Persist unsaved work as a local draft (debounced). The draft is cleared
	// once the configuration matches the loaded preset or plain defaults.
	let draftTimer: ReturnType<typeof setTimeout> | null = null;
	$effect(() => {
		if (!presetState || !currentNormalized || !configId) return;
		const status = presetStatus;
		const snapshot = currentNormalized;
		const defaults = defaultsNormalized;
		const selectedId = presetState.selected?.id ?? null;
		const revisionId = presetState.selectedRevisionId;
		if (draftTimer) clearTimeout(draftTimer);
		draftTimer = setTimeout(() => {
			if (status === "modified") {
				saveDraft(configId, {
					settings: snapshot,
					presetId: selectedId,
					presetRevisionId: revisionId,
				});
			} else if (status === "custom") {
				if (defaults && !configsEqual(snapshot, defaults)) {
					saveDraft(configId, {
						settings: snapshot,
						presetId: null,
						presetRevisionId: null,
					});
				} else {
					clearDraft(configId);
				}
			} else {
				clearDraft(configId);
			}
		}, 1000);
		return () => {
			if (draftTimer) clearTimeout(draftTimer);
		};
	});

	// Full reset from the preset overflow menu: drop the local draft and the
	// preset selection and go back to plain metadata defaults.
	function resetConfiguration() {
		clearDraft(configId);
		sharedPresetName = null;
		seedSourceId = null;
		presetState?.clearSelection();
		if (metadata) resetFormState(metadata);
	}

	// Toolbar-driven form loads (preset switch, revert) leave the shared-link
	// context behind.
	function handleToolbarApply(loaded: LoadedForm) {
		sharedPresetName = null;
		seedSourceId = null;
		applyLoadedForm(loaded);
	}

	beforeNavigate((navigation) => {
		if (bypassNavigationGuard || presetStatus !== "modified") return;
		if (navigation.type === "leave") {
			// Closing the tab / hard navigation: let the browser prompt. The
			// local draft additionally preserves the work.
			navigation.cancel();
			return;
		}
		navigation.cancel();
		toolbarRef?.requestLeave(() => {
			bypassNavigationGuard = true;
			if (navigation.to) {
				void goto(navigation.to.url).finally(() => {
					bypassNavigationGuard = false;
				});
			}
		});
	});
	let selectedVisibility = $state(["Basic"]);
	let visibilitySelection = $state<"basic" | "advanced" | "expert" | "wip">(
		"basic",
	);

	const visibilityLevels = [
		{ id: "basic", name: "Basic", value: ["Basic"] as const },
		{
			id: "advanced",
			name: "Advanced",
			value: ["Basic", "Advanced"] as const,
		},
		{
			id: "expert",
			name: "Expert",
			value: ["Basic", "Advanced", "Expert"] as const,
		},
		{
			id: "wip",
			name: "Work in progress",
			value: ["Basic", "Advanced", "Expert", "Wip"] as const,
		},
	];

	$effect(() => {
		// keep selectedVisibility in sync with compact selection
		const map: Record<typeof visibilitySelection, string[]> = {
			basic: ["Basic"],
			advanced: ["Basic", "Advanced"],
			expert: ["Basic", "Advanced", "Expert"],
			wip: ["Basic", "Advanced", "Expert", "Wip"],
		};
		selectedVisibility = map[visibilitySelection];
	});

	const hasGlobalOptions = $derived(
		!!(metadata && metadata.settings && metadata.settings.length > 0),
	);

	// Show global options only if there is actually a choice to make for the
	// current visibility level. If all visible globals are selects with a single
	// item, hide the card entirely.
	const shouldShowGlobalOptions = () => {
		if (!(metadata && metadata.settings && metadata.settings.length > 0))
			return false;

		let showGlobalOptions = false;
		if (metadata && metadata.settings) {
			for (const option of metadata.settings) {
				if (selectedVisibility.includes(option.visibility || "Basic")) {
					if (
						option.type !== "SingleChoice" ||
						(option.type === "SingleChoice" &&
							Object.keys((option as SingleChoiceSetting).values)
								.length > 1)
					) {
						showGlobalOptions = true;
						break;
					}
				}
			}
		}
		return showGlobalOptions;
	};

	const hasSelectedGames = $derived(selectedGames.length > 0);

	const requiredGames: string[] = [];

	$effect(() => {
		// Check if there's only one game available and the activeGameTab is not set or the wrong game, and reset it
		if (
			selectedGames.length === 1 &&
			(!activeGameTab || !selectedGames.includes(activeGameTab))
		) {
			activeGameTab = selectedGames[0];
		}
	});

	async function handleSubmit() {
		generating = true;
		formSubmissionError = null; // Clear previous submission errors

		if (selectedGames.length === 0) {
			formSubmissionError = m.config_error_no_games_selected();
			generating = false;
			return;
		}

		try {
			if (!metadata) {
				throw new Error(m.config_no_metadata());
			}

			const snapshot = normalizeConfig(
				{
					selectedGames,
					global: formValues.global,
					perGame: formValues.perGame,
				},
				metadata,
			);
			const payload = buildRandomizePayload(snapshot, metadata, {
				includeSpoiler,
			});

			// Generation always uses the visible configuration; the preset
			// reference is provenance metadata only and never requires saving.
			const result = await createSeed(payload, {
				presetId: presetState?.selected?.id ?? null,
				presetRevisionId: presetState?.selectedRevisionId ?? null,
				settingsSnapshot: snapshot,
				configSchemaVersion: CONFIG_SCHEMA_VERSION,
			});

			if (!result.id) {
				throw new Error(m.config_randomize_no_seed_id());
			}

			// Navigate to the generated seed page. Use an absolute path to avoid base path issues.
			bypassNavigationGuard = true;
			try {
				await goto(`/seed/${result.id}`);
			} finally {
				bypassNavigationGuard = false;
			}
		} catch (e: unknown) {
			formSubmissionError =
				(e as { message?: string })?.message ||
				m.config_unknown_error();
			console.error("Failed to generate seed:", e);
		} finally {
			generating = false;
		}
	}
</script>

<div class="container mx-auto px-4 py-4">
	<h1 class="text-2xl font-bold mb-2 text-primary-500 dark:text-primary-400">
		{m.config_header()}
	</h1>
	<p class="mb-4 text-slate-600 dark:text-slate-400 text-sm">
		{m.config_description()}
	</p>

	{#if pageError}
		<div
			class="bg-red-100 border border-red-400 text-red-700 px-3 py-2 rounded relative dark:bg-red-700 dark:border-red-600 dark:text-red-200 text-xs"
			data-testid="error-message"
			role="alert"
		>
			<strong class="font-bold">{m.error_label()} </strong>
			<span class="block sm:inline">{pageError}</span>
		</div>
	{:else if metadata}
		<!-- Display form submission error if any -->
		{#if formSubmissionError}
			<div
				class="bg-red-100 border border-red-400 text-red-700 px-3 py-2 rounded relative dark:bg-red-700 dark:border-red-600 dark:text-red-200 text-xs mb-4"
				data-testid="form-submission-error-message"
				role="alert"
			>
				<strong class="font-bold">{m.error_label()} </strong>
				<span class="block sm:inline">{formSubmissionError}</span>
			</div>
		{/if}

		<!-- Compact visibility selector -->
		<div class="mb-2 flex justify-end items-center gap-2">
			<label
				for="visibility-select"
				class="text-xs text-slate-600 dark:text-slate-400"
			>
				Options detail
			</label>
			<div class="w-44">
				<Select
					id="visibility-select"
					bind:value={visibilitySelection}
					items={visibilityLevels.map((l) => ({
						value: l.id,
						name: l.name,
					}))}
					className="text-xs py-1.5"
				/>
			</div>
		</div>

		<!-- Seed preset toolbar -->
		{#if presetState}
			<PresetToolbar
				bind:this={toolbarRef}
				state={presetState}
				{metadata}
				{currentNormalized}
				isAuthenticated={!!user}
				{isAdmin}
				sharedName={sharedPresetName}
				sharedInvalid={data.sharedInvalid ?? false}
				{seedSourceId}
				seedSourceInvalid={data.seedSettingsInvalid ?? false}
				onapply={handleToolbarApply}
				onreset={resetConfiguration}
			/>
		{/if}

		<form
			onsubmit={(event) => {
				event.preventDefault();
				handleSubmit();
			}}
			class="space-y-4"
		>
			<!-- Game Selection Component -->
			<GameSelector
				games={availableGames}
				bind:selectedGames
				loading={false}
				{requiredGames}
			/>
			<!-- Global Options Component -->
			{#if hasGlobalOptions && shouldShowGlobalOptions()}
				<OptionForm
					options={metadata.settings}
					values={formValues.global}
					title={m.config_global_options_header()}
					visibility={selectedVisibility}
				/>
			{/if}
			<!-- Game-specific Options Component -->
			{#if hasSelectedGames}
				<GameTabs
					bind:selectedGames
					bind:activeGameTab
					perGameOptions={metadata.gameSettings}
					bind:formValues
					games={availableGames}
					visibility={selectedVisibility}
				/>
			{/if}

			<div class="bg-white dark:bg-slate-800 rounded-lg shadow-md p-3">
				<div class="flex items-center justify-between gap-3">
					<label
						for="include-spoiler"
						class="text-sm font-medium text-slate-900 dark:text-slate-100"
					>
						Include spoiler log
						<span
							class="ml-2 hidden text-xs font-normal text-slate-600 dark:text-slate-400 sm:inline"
						>
							Turn off for a race seed.
						</span>
					</label>
					<Toggle
						id="include-spoiler"
						bind:checked={includeSpoiler}
						size="sm"
					/>
				</div>
			</div>

			<!-- Generate Button -->
			<div class="mt-3 flex justify-center">
				<Button
					type="submit"
					variant="primary"
					size="sm"
					disabled={generating || !hasSelectedGames}
					fullWidth={true}
					className="relative"
				>
					{#if generating}
						<span
							class="absolute left-2 top-1/2 transform -translate-y-1/2"
						>
							<svg
								class="animate-spin h-3.5 w-3.5 text-white"
								xmlns="http://www.w3.org/2000/svg"
								fill="none"
								viewBox="0 0 24 24"
							>
								<circle
									class="opacity-25"
									cx="12"
									cy="12"
									r="10"
									stroke="currentColor"
									stroke-width="4"
								></circle>
								<path
									class="opacity-75"
									fill="currentColor"
									d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
								></path>
							</svg>
						</span>
					{/if}
					{generating
						? m.config_generating()
						: m.config_generate_seed_button()}
				</Button>
			</div>
		</form>
	{:else}
		<!-- Loading indicator (if no error and no metadata yet) -->
		<div class="flex justify-center my-6">
			<div
				class="animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-indigo-500"
			></div>
		</div>
	{/if}
</div>
