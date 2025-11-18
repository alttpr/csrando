<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import { goto } from "$app/navigation";
	import PresetManager from "$lib/components/config/PresetManager.svelte";
	import GameSelector from "$lib/components/config/GameSelector.svelte";
	import OptionForm from "$lib/components/config/OptionForm.svelte";
	import GameTabs from "$lib/components/config/GameTabs.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import Select from "$lib/components/ui/Select.svelte";
	import { createSeed } from "$lib/services/data";
	import type {
		ConfigOptions,
		Metadata,
		MetadataSetting,
		SingleChoiceSetting,
		MultipleChoiceSetting,
		SliderSetting,
		ToggleSetting,
		GenericSetting,
		InputSetting,
		PresetOptions,
	} from "$lib/types";

	interface PageData {
		metadata: Metadata | null;
		error: string | null;
	}

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	const metadata = $derived(data.metadata);
	const pageError = $derived(data.error);

	let formValues: ConfigOptions = $state({ global: {}, perGame: {} });

	let availableGames: Array<{
		id: string;
		name: string;
		description?: string;
	}> = $state([]);
	let selectedGames: string[] = $state([]);
	let activeGameTab: string | null = $state(null);

	function applyPreset(options: PresetOptions) {
		if (options.global) {
			formValues.global = { ...options.global };
		}
		if (options.perGame) {
			formValues.perGame = { ...options.perGame };
		}
		if (options.selectedGames) {
			selectedGames = [...options.selectedGames];
			// Also set active tab if needed
			if (
				selectedGames.length > 0 &&
				(!activeGameTab || !selectedGames.includes(activeGameTab))
			) {
				activeGameTab = selectedGames[0];
			}
		}
	}

	// Helper function to get default value for an option
	function getDefaultValue(option: MetadataSetting): unknown {
		switch (option.type) {
			case "SingleChoice": {
				const sc = option as SingleChoiceSetting;
				if (sc.default && sc.default in option.values)
					return sc.default;
				const firstKey = Object.keys(option.values)[0];
				if (firstKey) {
					return option.values[firstKey];
				}
				return "";
			}
			case "MultipleChoice":
				return (option as MultipleChoiceSetting).default || [];
			case "Slider":
				return (option as SliderSetting).default || 0;
			case "Toggle":
				return (option as ToggleSetting).default ?? false;
			case "Input":
				return (option as InputSetting).default || "";
			case "Generic":
				return (option as GenericSetting).default || null;
			default:
				return null;
		}
	}

	function initializeFormValues(metadata: Metadata) {
		const result = {
			availableGames: [] as Array<{
				id: string;
				name: string;
				description?: string;
			}>,
			formGlobal: {} as { [key: string]: unknown },
			formPerGame: {} as {
				[gameKey: string]: { [key: string]: unknown };
			},
			selectedGames: [] as string[],
			activeTab: null as string | null,
		};

		// Process global settings
		if (metadata.settings) {
			for (const option of metadata.settings) {
				result.formGlobal[option.key] = getDefaultValue(option);
			}
		}

		if (metadata.gameSettings) {
			for (const game in metadata.gameSettings) {
				const gameSettings = metadata.gameSettings[game];

				// Initialize an empty object for this game's options
				result.formPerGame[game] = {};

				const gameSpecificOptions =
					gameSettings && gameSettings.settings
						? gameSettings.settings
						: [];

				// Only add to availableGames if there are actual settings
				if (
					Array.isArray(gameSpecificOptions) &&
					gameSpecificOptions.length > 0
				) {
					// Add to our results array
					result.availableGames.push({
						id: game,
						name:
							(
								gameSettings as unknown as {
									game?: { name?: string };
								}
							).game?.name || game,
						description: (
							gameSettings as unknown as {
								game?: { description?: string };
							}
						).game?.description,
					});

					// Process each option
					for (const option of gameSpecificOptions) {
						if (option && typeof option.key === "string") {
							result.formPerGame[game][option.key] =
								getDefaultValue(option);
						}
					}
				}
			}
		}

		// Set initial selection if games are available
		if (result.availableGames.length > 0) {
			// Set all available games as selected by default
			result.selectedGames = result.availableGames.map((game) => game.id);
			// Set the first game as the active tab
			result.activeTab = result.availableGames[0].id;
		}

		return result;
	}

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

	$effect(() => {
		const meta = metadata ?? null;
		const signature = getMetadataSignature(meta);
		if (signature === lastMetadataSignature) return;
		lastMetadataSignature = signature;
		resetFormState(meta);
		generating = false;
		formSubmissionError = null;
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

	// Hotfix: when global target is "Combo", require Alttp to be selected and lock it
	let requiredGames: string[] = $state([]);

	$effect(() => {
		// Determine current global target (RandomizerTarget)
		const currentTarget =
			(formValues.global?.["Game"] as string) ||
			"" ||
			((
				metadata?.settings?.find((s) => s.key === "Game") as
					| { default?: string }
					| undefined
			)?.default ??
				"");
		const isCombo = (currentTarget || "").toLowerCase() === "combo";
		requiredGames = isCombo ? ["Alttp"] : [];

		// Ensure Alttp remains selected when Combo target is active
		if (isCombo && !selectedGames.includes("Alttp")) {
			selectedGames = ["Alttp", ...selectedGames];
		}
	});

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
			const isRandomSelection = (value: unknown) =>
				typeof value === "string" &&
				value.trim().toLowerCase() === "randompick";

			const filterNonNullValues = (obj: unknown) => {
				const o = obj as Record<string, unknown> | undefined;
				if (!o) {
					return {};
				}
				const entries: Array<[string, unknown]> = [];
				for (const [key, value] of Object.entries(o)) {
					if (value === null || isRandomSelection(value)) {
						continue;
					}

					if (Array.isArray(value)) {
						const sanitizedArray = value.filter(
							(item) => item != null && !isRandomSelection(item),
						);
						if (sanitizedArray.length === 0) {
							continue;
						}
						entries.push([key, sanitizedArray]);
						continue;
					}

					if (typeof value === "string") {
						const trimmed = value.trim();
						if (
							!trimmed ||
							trimmed.toLowerCase() === "randompick"
						) {
							continue;
						}
						entries.push([key, trimmed]);
						continue;
					}

					entries.push([key, value]);
				}
				return Object.fromEntries(entries);
			};

			const gameSettings: { [key: string]: { [key: string]: unknown } } =
				{};

			for (const gameId of selectedGames) {
				const currentGameOptions = formValues.perGame[gameId];

				if (currentGameOptions) {
					const validGameOptions =
						filterNonNullValues(currentGameOptions);

					if (Object.keys(validGameOptions).length > 0) {
						gameSettings[gameId] = validGameOptions;
					}
				}
			}

			// If there's a "default" game in metadata, always include it if it's not already in gameSettings
			if (metadata && metadata.settings) {
				const gameSetting = metadata.settings.find(
					(opt) => opt.key === "Game",
				);
				if (
					gameSetting &&
					gameSetting.default &&
					!gameSettings[gameSetting.default as string]
				) {
					gameSettings[gameSetting.default as string] =
						filterNonNullValues(
							formValues.perGame[gameSetting.default as string],
						);
				}
			}

			// Transform per-game settings for sliders that provide options (optionsFor)
			if (metadata?.gameSettings) {
				for (const [gameKey, gameMeta] of Object.entries(
					metadata.gameSettings,
				)) {
					for (const setting of gameMeta.settings) {
						if (setting.type === "Slider" && setting.optionsFor) {
							const currentVal = (gameSettings[gameKey] || {})[
								setting.key
							];
							if (currentVal === undefined) {
								// If user didn't pick, send full numeric range as array
								const from = setting.range.from ?? 0;
								const to = setting.range.to;
								const arr = Array.from(
									{ length: to - from + 1 },
									(_, i) => i + from,
								);
								if (!gameSettings[gameKey])
									gameSettings[gameKey] = {};
								gameSettings[gameKey][setting.key] = arr;
							} else if (typeof currentVal === "number") {
								gameSettings[gameKey][setting.key] = [
									currentVal,
								];
							}
						}
					}
				}
			}

			// Use the explicit global Game setting (RandomizerTarget enum) provided by metadata instead of deriving.
			let globalGameTarget = (formValues.global["Game"] as string) || "";
			if (!globalGameTarget && metadata?.settings) {
				const gameSetting = metadata.settings.find(
					(s) => s.key === "Game",
				);
				if (
					gameSetting &&
					"default" in gameSetting &&
					typeof (gameSetting as { default?: unknown }).default ===
						"string"
				) {
					globalGameTarget = (gameSetting as { default?: string })
						.default as string;
				}
			}
			if (!globalGameTarget) globalGameTarget = "Alttpr";

			const worldConfig: Record<string, unknown> = {
				Language: (formValues.global["Language"] as string) || "en",
			};
			if (!isRandomSelection(globalGameTarget)) {
				worldConfig.Game = globalGameTarget;
			}

			const worldGameKeys = new Set<string>();
			for (const gameKey of selectedGames) {
				worldGameKeys.add(gameKey);
			}
			if (metadata?.settings) {
				const defaultGame = metadata.settings.find(
					(opt) =>
						opt.key === "Game" &&
						typeof (opt as { default?: unknown }).default ===
							"string",
				) as
					| {
							default?: string;
					  }
					| undefined;
				if (
					defaultGame?.default &&
					metadata?.gameSettings?.[defaultGame.default]
				) {
					worldGameKeys.add(defaultGame.default);
				}
			}

			// Hotfix enforcement: if Combo target, ensure Alttp is always included
			if ((globalGameTarget || "").toLowerCase() === "combo") {
				worldGameKeys.add("Alttp");
			}

			for (const gameKey of worldGameKeys) {
				const perGame =
					gameSettings[gameKey] ??
					filterNonNullValues(formValues.perGame[gameKey]);
				worldConfig[gameKey] = perGame || {};
			}

			if (worldGameKeys.size === 0) {
				worldConfig["Alttp"] =
					filterNonNullValues(formValues.perGame["Alttp"]) || {};
			}

			const payload = {
				Seed: 0,
				IncludeSpoiler: true,
				Configs: [worldConfig],
			};

			const result = await createSeed(payload);

			if (!result.id) {
				throw new Error(m.config_randomize_no_seed_id());
			}

			// Navigate to the generated seed page. Use an absolute path to avoid base path issues.
			await goto(`/seed/${result.id}`);
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
		<!-- Top controls bar -->
		<div
			class="mb-4 flex flex-col sm:flex-row justify-between items-end gap-4"
		>
			<PresetManager
				onApply={applyPreset}
				currentOptions={formValues}
				{selectedGames}
			/>

			<div class="flex items-center gap-2">
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
		</div>

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
