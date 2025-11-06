<script lang="ts">
	import { page } from "$app/stores";
	import * as m from "$lib/paraglide/messages";
	import type { PageData } from "./$types";
	import { onMount, onDestroy } from "svelte";
	import localforage from "localforage";
	import {
		patchingError as patchingServiceError,
		initiatePatching,
		type InitiatePatchingParams,
		patchingProgress as patchingServiceProgress,
		isPatching as patchingServiceIsPatching,
		cleanupPatcher,
	} from "$lib/services/patching";
	import SpriteSelect from "$lib/components/ui/SpriteSelect.svelte";
	import Progressbar from "$lib/components/ui/Progressbar.svelte";
	import Button from "$lib/components/ui/Button.svelte";
	import SeedOptionsViewer from "$lib/components/seed/SeedOptionsViewer.svelte";
	import SpoilerLog from "$lib/components/seed/SpoilerLog.svelte";
	import RomUploader from "$lib/components/seed/RomUploader.svelte";
	import { getPublicSpritesBaseUrl } from "$lib/env";
	import Toggle from "$lib/components/ui/Toggle.svelte";
	import { buildOptionSummaryTokens } from "$lib/utils/options-summary";
	import {
		normalizeRomExtensions,
		resolveUploadedRomBuffer,
	} from "$lib/utils/rom";
	import { sha256Hex } from "$lib/utils/hash";

	import { gameStaticInfo } from "$lib/game-static-info";

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	interface GameStaticInfo {
		id: string;
		displayName: string;
		expectedHash?: string;
		fileExtensions: string;
		forcedHeaderBytes?: Uint8Array;
	}

	const normalizeGameId = (value: string) => value.toLowerCase();
	const isBlank = (value: string | undefined) =>
		!value || value.trim() === "";

	const staticInfoFromFile = (() => {
		const entries: Array<[string, GameStaticInfo]> = Object.entries(
			gameStaticInfo,
		).map(([rawId, info]) => {
			const normalizedId = normalizeGameId(rawId);
			return [
				normalizedId,
				{
					id: normalizedId,
					displayName: info.displayName,
					expectedHash: info.expectedHash,
					fileExtensions: info.fileExtensions ?? "",
					forcedHeaderBytes: info.forcedHeaderBytes,
				},
			];
		});
		return new Map<string, GameStaticInfo>(entries);
	})();

	const hiddenMetadataGameIds = new Set(
		Array.from(staticInfoFromFile.entries())
			.filter(
				([, info]) =>
					isBlank(info.displayName) || isBlank(info.fileExtensions),
			)
			.map(([gameId]) => gameId),
	);

	const isMetadataGameHidden = (gameId: string | undefined | null) => {
		if (!gameId) return false;
		return hiddenMetadataGameIds.has(normalizeGameId(gameId));
	};
	let gameIdToStaticInfo = $state(new Map<string, GameStaticInfo>());

	const formatIsoTimestamp = (value: string | null | undefined) => {
		if (!value) return null;
		const parsed = new Date(value);
		if (Number.isNaN(parsed.getTime())) {
			return value;
		}
		return parsed.toLocaleString();
	};

	// Sprite Configuration Interfaces
	import type { GameSpriteConfig, Metadata } from "$lib/types";
	import { GameSpriteConfigSchema } from "$lib/schemas/sprites";
	let gameIdToSpriteInfoMap = $state(new Map<string, GameSpriteConfig>());
	// Map retained for patcher; plain object used for reactive UI rendering
	let selectedSpritesByGameId = $state(new Map<string, string>());
	let spriteSelections = $state<Record<string, string>>({});

	// Post-Generation Settings Interfaces
	import type { GamePostGenConfig } from "$lib/types";
	let gameIdToPostGenConfigMap = $state(new Map<string, GamePostGenConfig>());
	let selectedPostGenByGameId = $state(
		new Map<string, Record<string, string | boolean>>(),
	);
	let postGenSelections = $state<
		Record<string, Record<string, string | boolean>>
	>({});

	// Persistence helpers for sprite selections
	function spriteSelectionKey(gameId: string) {
		return `sprite_selection_${gameId}`;
	}

	async function persistSpriteSelection(gameId: string, value: string) {
		const key = spriteSelectionKey(gameId);
		// Write to localStorage first for immediate availability on next reload.
		try {
			localStorage.setItem(key, value);
		} catch {
			/* ignore */
		}
		// Also attempt to persist via localforage (async, best-effort)
		try {
			await localforage.setItem(key, value);
		} catch {
			/* ignore */
		}
		if (import.meta.env.DEV) {
			console.debug("[sprites] persisted selection", { gameId, value });
		}
	}

	// Persistence helpers for post-gen selections (per game -> object of optionId -> value)
	function postGenSelectionKey(gameId: string) {
		return `postgen_selection_${gameId}`;
	}

	async function persistPostGenSelection(
		gameId: string,
		values: Record<string, string | boolean>,
	) {
		const key = postGenSelectionKey(gameId);
		try {
			localStorage.setItem(key, JSON.stringify(values));
		} catch {
			/* ignore */
		}
		try {
			await localforage.setItem(key, values);
		} catch {
			/* ignore */
		}
		if (import.meta.env.DEV) {
			console.debug("[postgen] persisted selection", { gameId, values });
		}
	}

	async function loadPostGenSelection(
		gameId: string,
	): Promise<Record<string, string | boolean> | null> {
		const key = postGenSelectionKey(gameId);
		try {
			await (
				localforage as unknown as { ready?: () => Promise<void> }
			).ready?.();
			const v =
				await localforage.getItem<Record<string, string | boolean>>(
					key,
				);
			if (v) return v;
		} catch {
			/* ignore */
		}
		try {
			const raw = localStorage.getItem(key);
			return raw
				? (JSON.parse(raw) as Record<string, string | boolean>)
				: null;
		} catch {
			return null;
		}
	}

	async function loadSpriteSelection(gameId: string): Promise<string | null> {
		const key = spriteSelectionKey(gameId);
		try {
			await (
				localforage as unknown as { ready?: () => Promise<void> }
			).ready?.();
			const v = await localforage.getItem<string>(key);
			if (v) return v;
		} catch {
			/* ignore */
		}
		try {
			return localStorage.getItem(key);
		} catch {
			return null;
		}
	}

	// Early hydration will run inside onMount after gameOptions derived from seed data.

	// Seed options are stored using the randomizer request payload shape.
	const optionsRoot = data.seedDetails?.options as
		| { Configs?: Array<Record<string, unknown>> }
		| undefined;
	const configsArray = optionsRoot?.Configs ?? [];
	const rawConfig = configsArray[0] || {};
	const gameOptions = Object.entries(rawConfig)
		.filter(
			([, v]) =>
				v !== null &&
				typeof v === "object" &&
				(v as object).constructor === Object,
		)
		.map(([id, options]) => ({
			id: id.toLowerCase(),
			options: options as Record<string, unknown>,
		}));
	const globalOptions = Object.fromEntries(
		Object.entries(rawConfig).filter(
			([, v]) =>
				!(
					v !== null &&
					typeof v === "object" &&
					(v as object).constructor === Object
				),
		),
	);

	const visibleGameOptions = gameOptions.filter(
		({ id }) => !isMetadataGameHidden(id),
	);

	const resolvedRandomizerIdRaw =
		typeof data.randomizerId === "string"
			? data.randomizerId
			: typeof globalOptions.Game === "string"
				? (globalOptions.Game as string)
				: null;

	const resolvedRandomizerIdNormalized = resolvedRandomizerIdRaw
		? resolvedRandomizerIdRaw.toLowerCase()
		: null;

	const requiredRomGameIds = resolvedRandomizerIdNormalized
		? Object.entries(gameStaticInfo)
				.filter(([gid, info]) => {
					if (gid === "combo") return false;
					const offset =
						info.targetOffsets?.[resolvedRandomizerIdNormalized];
					return typeof offset === "number" && offset >= 0;
				})
				.map(([gid]) => normalizeGameId(gid))
		: [];

	const romUploadGameIds = (() => {
		const ids = visibleGameOptions.map((g) => g.id);
		for (const gid of requiredRomGameIds) {
			if (!ids.includes(gid)) {
				ids.push(gid);
			}
		}
		return ids;
	})();
	// Primary game id (first entry) for patcher logic
	let primaryGameId = $derived(visibleGameOptions[0]?.id?.toLowerCase());

	interface RomFileData {
		buffer: ArrayBuffer | null;
		fileName: string | null;
		hashStatus:
			| "no_rom"
			| "checking"
			| "verified"
			| "mismatch"
			| "error"
			| "uploaded_no_verify";
		calculatedHash?: string;
		expectedHash?: string;
		gameName: string;
	}

	let romsData = $state(new Map<string, RomFileData>());

	if (romUploadGameIds.length > 0) {
		const initialStaticInfo = new Map<string, GameStaticInfo>();
		const initialRomData = new Map<string, RomFileData>();
		for (const gameId of romUploadGameIds) {
			const normalizedId = normalizeGameId(gameId);
			let staticInfoEntry = staticInfoFromFile.get(normalizedId);
			if (!staticInfoEntry) {
				staticInfoEntry = {
					id: normalizedId,
					displayName: `Game: ${normalizedId}`,
					fileExtensions: ".rom,.sfc,.smc",
					expectedHash: undefined,
				};
			}
			initialStaticInfo.set(normalizedId, staticInfoEntry);
			initialRomData.set(normalizedId, {
				buffer: null,
				fileName: null,
				hashStatus: "no_rom",
				gameName: staticInfoEntry.displayName,
				expectedHash: staticInfoEntry.expectedHash,
			});
		}
		gameIdToStaticInfo = initialStaticInfo;
		romsData = initialRomData;
	}

	function getLocalForageKey(gameId: string): string {
		return `rom_global_${gameId}`;
	}

	onMount(() => {
		localforage.config({
			name: "RandoWebRoms",
			storeName: "rom_files",
			description: "Storage for uploaded ROM files",
		});

		(async () => {
			// Early hydration of sprite selections (localStorage only) before async fetch of sprite configs
			try {
				let changed = false;
				for (const g of visibleGameOptions) {
					const gid = g.id.toLowerCase();
					const persisted = localStorage.getItem(
						spriteSelectionKey(gid),
					);
					// Allow empty string as a valid persisted "Default sprite" selection
					if (
						persisted !== null &&
						!selectedSpritesByGameId.has(gid)
					) {
						selectedSpritesByGameId.set(gid, persisted);
						spriteSelections[gid] = persisted;
						changed = true;
					}
				}
				if (changed) {
					selectedSpritesByGameId = selectedSpritesByGameId;
					spriteSelections = { ...spriteSelections };
				}
			} catch {
				/* ignore */
			}

			// Early hydration of post-gen selections (localStorage only) before async fetch of configs
			try {
				let changed = false;
				for (const g of visibleGameOptions) {
					const gid = g.id.toLowerCase();
					const persisted = localStorage.getItem(
						postGenSelectionKey(gid),
					);
					if (persisted && !selectedPostGenByGameId.has(gid)) {
						try {
							const parsed = JSON.parse(persisted) as Record<
								string,
								string | boolean
							>;
							selectedPostGenByGameId.set(gid, parsed);
							postGenSelections[gid] = parsed;
							changed = true;
						} catch {
							/* ignore */
						}
					}
				}
				if (changed) {
					selectedPostGenByGameId = selectedPostGenByGameId;
					postGenSelections = { ...postGenSelections };
				}
			} catch {
				/* ignore */
			}

			// Placeholder for new sprite info map while fetching
			const newSpriteInfoMapProvisional = new Map<
				string,
				GameSpriteConfig
			>();
			const newPostGenInfoMapProvisional = new Map<
				string,
				GamePostGenConfig
			>();

			const newRomsData = new Map<string, RomFileData>();
			const newGameIdToStaticInfo = new Map<string, GameStaticInfo>();

			if (romUploadGameIds.length > 0) {
				// First, populate static info and ROM data for every game that needs a ROM upload
				for (const gameIdRaw of romUploadGameIds) {
					const gameId = normalizeGameId(gameIdRaw);
					let staticInfoEntry = staticInfoFromFile.get(gameId);

					if (!staticInfoEntry) {
						console.warn(
							`Static info for game ID ${gameId} not found in JSON. Creating fallback.`,
						);
						staticInfoEntry = {
							id: gameId,
							displayName: `Game: ${gameId}`,
							fileExtensions: ".rom,.sfc,.smc",
							expectedHash: undefined,
						};
					}
					newGameIdToStaticInfo.set(gameId, staticInfoEntry);

					let romFileDataToSet: RomFileData | null = null;
					try {
						const persistedRomData =
							await localforage.getItem<RomFileData>(
								getLocalForageKey(gameId),
							);
						if (persistedRomData) {
							romFileDataToSet = {
								...persistedRomData,
								gameName: staticInfoEntry.displayName,
								expectedHash: staticInfoEntry.expectedHash,
								buffer: persistedRomData.buffer || null,
								fileName: persistedRomData.fileName || null,
								hashStatus:
									persistedRomData.hashStatus || "no_rom",
							};
						}
					} catch (error) {
						console.error(
							`Error loading ROM data for ${gameId} from localforage:`,
							error,
						);
					}

					if (!romFileDataToSet) {
						romFileDataToSet = {
							buffer: null,
							fileName: null,
							hashStatus: "no_rom",
							gameName: staticInfoEntry.displayName,
							expectedHash: staticInfoEntry.expectedHash,
						};
					}
					newRomsData.set(gameId, romFileDataToSet);
				}

				// After ROM data is processed, fetch sprite configs only for games with settings
				for (const game of visibleGameOptions) {
					const gameId = normalizeGameId(game.id); // Ensure consistent casing
					// Base URL where sprite folders live. For GitHub Pages hosting, set PUBLIC_SPRITES_BASE_URL
					// to the absolute URL (e.g. https://<user>.github.io/<repo>/sprites) so we don't hit the local dev origin.
					const spritesBase = getPublicSpritesBaseUrl();
					// Simple cache busting so updated sprite lists (add/remove) reflect quickly.
					// Images remain cacheable; only metadata JSON is busted.
					// Using a per-load timestamp; if you prefer some caching, replace Date.now() with
					// something like Math.floor(Date.now() / (5 * 60 * 1000)) for 5‑minute buckets.
					const spriteJsonPath = `${spritesBase}/${gameId}/sprites.json?cb=${Date.now()}`;
					try {
						const response = await fetch(spriteJsonPath);
						if (response.ok) {
							const json = await response.json();
							const parsed =
								GameSpriteConfigSchema.safeParse(json);
							if (parsed.success) {
								// Normalize image and patch file paths so that relative or root-relative entries
								// in remote metadata still resolve correctly when served from a different origin.
								const normalizePath = (
									p: string | undefined,
								): string | undefined => {
									if (!p) return p;
									// Already absolute URL (http/https) -> leave untouched
									if (/^https?:\/\//i.test(p)) return p;
									// If starts with '/' treat it as relative to the spritesBase origin (strip leading slash first)
									if (p.startsWith("/")) {
										return `${spritesBase}${p}`; // spritesBase already trimmed
									}
									// Bare relative filename -> assume spritesBase/<gameId>/<filename>
									return `${spritesBase}/${gameId}/${p}`;
								};

								const normalized = {
									defaultSpriteValue:
										parsed.data.defaultSpriteValue,
									sprites: parsed.data.sprites.map((s) => ({
										...s,
										imagePath: normalizePath(s.imagePath)!,
										patchDetails: s.patchDetails
											? {
													files: s.patchDetails.files.map(
														(f) => ({
															...f,
															path: normalizePath(
																f.path,
															)!,
														}),
													),
													patches:
														s.patchDetails.patches,
												}
											: undefined,
									})),
								};
								newSpriteInfoMapProvisional.set(
									gameId,
									normalized,
								);
							} else {
								console.warn(
									`Invalid sprite config for ${gameId}:`,
									parsed.error.flatten(),
								);
								newSpriteInfoMapProvisional.set(gameId, {
									defaultSpriteValue: undefined,
									sprites: [],
								});
							}
						} else {
							console.warn(
								`Sprite configuration file not found for game ${gameId} at ${spriteJsonPath} (status: ${response.status}). This game will have no custom sprites.`,
							);
							newSpriteInfoMapProvisional.set(gameId, {
								defaultSpriteValue: undefined,
								sprites: [],
							});
						}
					} catch (error) {
						console.error(
							`Error fetching or parsing sprite configuration for game ${gameId} from ${spriteJsonPath}:`,
							error,
						);
						newSpriteInfoMapProvisional.set(gameId, {
							defaultSpriteValue: undefined,
							sprites: [],
						});
					}
				}
				gameIdToSpriteInfoMap = newSpriteInfoMapProvisional; // Assign to $state variable

				// Build post-generation settings from backend metadata
				const metaPostGen =
					(data.metadata &&
						(
							data.metadata as unknown as {
								postGenSettings?: Record<
									string,
									GamePostGenConfig
								>;
							}
						).postGenSettings) ||
					{};
				for (const game of visibleGameOptions) {
					const gameId = normalizeGameId(game.id);
					let cfg =
						(metaPostGen as Record<string, unknown>)[gameId] ||
						(metaPostGen as Record<string, unknown>)[game.id];
					let baseCfg: GamePostGenConfig =
						cfg && typeof cfg === "object"
							? (cfg as GamePostGenConfig)
							: { options: [] };

					// Always include ALTTP palette randomizer option, regardless of backend
					if (gameId === "alttp") {
						const hasPalette = (baseCfg.options || []).some(
							(o) => o.id === "palette_randomize",
						);
						if (!hasPalette) {
							baseCfg = {
								options: [
									...(baseCfg.options || []),
									{
										id: "palette_randomize",
										name: "Randomize Palette",
										type: "toggle",
										default: false,
										on: { patches: [] },
									},
								],
							};
						}
					}

					newPostGenInfoMapProvisional.set(gameId, baseCfg);
				}
				gameIdToPostGenConfigMap = newPostGenInfoMapProvisional; // Assign to $state variable

				// Load persisted sprite selections (if any) and apply only if still valid.
				try {
					let changed = false;
					for (const game of visibleGameOptions) {
						const gid = game.id.toLowerCase();
						if (selectedSpritesByGameId.has(gid)) continue; // already set
						const persistedVal = await loadSpriteSelection(gid);
						if (import.meta.env.DEV)
							console.debug("[sprites] load attempt", {
								gid,
								persistedVal,
							});
						if (persistedVal !== null) {
							const cfg = gameIdToSpriteInfoMap.get(gid);
							// Accept empty string (Default sprite) or a valid sprite value
							if (
								persistedVal === "" ||
								(cfg &&
									cfg.sprites.some(
										(s) => s.value === persistedVal,
									))
							) {
								selectedSpritesByGameId.set(gid, persistedVal);
								spriteSelections[gid] = persistedVal;
								changed = true;
							}
						}
					}
					if (changed) {
						selectedSpritesByGameId = selectedSpritesByGameId; // trigger reactivity
						spriteSelections = { ...spriteSelections };
					}
				} catch (e) {
					console.warn(
						"Failed to load persisted sprite selections",
						e,
					);
				}

				// Load persisted post-generation selections (if any) and apply only if still valid keys
				try {
					let changed = false;
					for (const game of visibleGameOptions) {
						const gid = game.id.toLowerCase();
						if (selectedPostGenByGameId.has(gid)) continue;
						const persisted = await loadPostGenSelection(gid);
						if (persisted) {
							const cfg = gameIdToPostGenConfigMap.get(gid);
							if (cfg) {
								// Only keep entries with matching option ids
								const valid: Record<string, string | boolean> =
									{};
								const optionIds = new Set(
									cfg.options.map((o) => o.id),
								);
								// Allow special nested keys for ALTTP palette randomizer
								const extraAllowed = new Set<string>();
								if (
									gid === "alttp" &&
									optionIds.has("palette_randomize")
								) {
									extraAllowed.add("palette_randomize_mode");
									extraAllowed.add(
										"palette_randomize_overworld",
									);
									extraAllowed.add(
										"palette_randomize_dungeon",
									);
									extraAllowed.add(
										"palette_randomize_link_sprite",
									);
									extraAllowed.add("palette_randomize_sword");
									extraAllowed.add(
										"palette_randomize_shield",
									);
									extraAllowed.add("palette_randomize_hud");
								}

								for (const [k, v] of Object.entries(
									persisted,
								)) {
									if (
										optionIds.has(k) ||
										extraAllowed.has(k)
									) {
										valid[k] = v as string | boolean;
									}
								}
								selectedPostGenByGameId.set(gid, valid);
								postGenSelections[gid] = valid;
								changed = true;
							}
						}
					}
					if (changed) {
						selectedPostGenByGameId = selectedPostGenByGameId;
						postGenSelections = { ...postGenSelections };
					}
				} catch (e) {
					console.warn(
						"Failed to load persisted post-generation selections",
						e,
					);
				}
			}

			gameIdToStaticInfo = newGameIdToStaticInfo;
			romsData = newRomsData;
		})();
	});

	$effect(() => {
		if (visibleGameOptions.length > 0 && gameIdToSpriteInfoMap.size > 0) {
			let changed = false;
			for (const game of visibleGameOptions) {
				const gameId = game.id.toLowerCase();
				// Do not override explicit empty string (Default sprite). Only set when missing entirely.
				if (!selectedSpritesByGameId.has(gameId)) {
					const gameSpriteConfig = gameIdToSpriteInfoMap.get(gameId);
					let initialSpriteValue = "";
					if (
						gameSpriteConfig &&
						gameSpriteConfig.defaultSpriteValue !== undefined
					) {
						const candidate = gameSpriteConfig.defaultSpriteValue;
						if (
							candidate === "" ||
							gameSpriteConfig.sprites.some(
								(sprite) => sprite.value === candidate,
							)
						) {
							initialSpriteValue = candidate;
						}
					}
					selectedSpritesByGameId.set(gameId, initialSpriteValue);
					spriteSelections[gameId] = initialSpriteValue;
					changed = true;
				}
			}
			if (changed) {
				selectedSpritesByGameId = selectedSpritesByGameId;
				spriteSelections = { ...spriteSelections };
			}
		} else if (visibleGameOptions.length === 0) {
			selectedSpritesByGameId = new Map<string, string>();
			spriteSelections = {};
		}
	});

	// Initialize cosmetics defaults if missing selections
	$effect(() => {
		if (
			visibleGameOptions.length > 0 &&
			gameIdToPostGenConfigMap.size > 0
		) {
			let changed = false;
			for (const game of visibleGameOptions) {
				const gid = game.id.toLowerCase();
				if (!selectedPostGenByGameId.has(gid)) {
					const cfg = gameIdToPostGenConfigMap.get(gid);
					const initial: Record<string, string | boolean> = {};
					if (cfg) {
						for (const opt of cfg.options) {
							if (opt.type === "toggle")
								initial[opt.id] = opt.default ?? false;
							else if (opt.type === "select")
								initial[opt.id] =
									opt.default ?? opt.choices[0]?.value;
						}
						// Special defaults for ALTTP palette randomizer nested options
						if (
							gid === "alttp" &&
							cfg.options.some(
								(o) => o.id === "palette_randomize",
							)
						) {
							if (initial["palette_randomize"] === undefined)
								initial["palette_randomize"] = false;
							initial["palette_randomize_mode"] = "maseya";
							initial["palette_randomize_overworld"] = true;
							initial["palette_randomize_dungeon"] = true;
							initial["palette_randomize_link_sprite"] = true;
							initial["palette_randomize_sword"] = true;
							initial["palette_randomize_shield"] = true;
							initial["palette_randomize_hud"] = true;
						}
					}
					selectedPostGenByGameId.set(gid, initial);
					postGenSelections[gid] = initial;
					changed = true;
				}
			}
			if (changed) {
				selectedPostGenByGameId = selectedPostGenByGameId;
				postGenSelections = { ...postGenSelections };
			}
		} else if (visibleGameOptions.length === 0) {
			selectedPostGenByGameId = new Map();
			postGenSelections = {};
		}
	});

	onDestroy(() => {
		cleanupPatcher();
	});

	async function verifyRomHash(
		gameId: string,
		buffer: ArrayBuffer,
		expectedHash?: string,
	) {
		const gameData = romsData.get(gameId);
		if (!gameData) return;

		let updatedGameData: RomFileData = {
			...gameData,
			hashStatus: "checking",
		};
		{
			const next = new Map(romsData);
			next.set(gameId, updatedGameData);
			romsData = next;
		}

		try {
			const calculatedHash = await sha256Hex(buffer);
			let newStatus: RomFileData["hashStatus"] = "error";
			if (expectedHash) {
				if (
					calculatedHash.toLowerCase() === expectedHash.toLowerCase()
				) {
					newStatus = "verified";
				} else {
					newStatus = "mismatch";
				}
			} else {
				newStatus = "uploaded_no_verify";
			}
			updatedGameData = {
				...updatedGameData,
				calculatedHash,
				hashStatus: newStatus,
				buffer,
			};
		} catch (e) {
			console.error(
				"Error calculating SHA256 for game " + gameId + ":",
				e,
			);
			updatedGameData = {
				...updatedGameData,
				hashStatus: "error",
				buffer,
			};
		}
		{
			const next = new Map(romsData);
			next.set(gameId, updatedGameData);
			romsData = next;
		}

		try {
			await localforage.setItem(
				getLocalForageKey(gameId),
				updatedGameData,
			);
		} catch (error) {
			console.error(
				`Error saving ROM data for ${gameId} to localforage:`,

				error,
			);
		}
	}

	interface FileSelectedEventDetail {
		gameId: string;
		file: File;
	}

	async function handleFileUpload(
		eventData: FileSelectedEventDetail,
		gameId: string,
	) {
		const file = eventData.file;
		const staticInfo = gameIdToStaticInfo.get(gameId);

		if (!staticInfo) {
			alert(
				m.seed_page_rom_error_missing_game_config({ gameId }) ||
					`Error: Game with ID '${gameId}' is not configured.`,
			);
			return;
		}

		let currentRomData = romsData.get(gameId);
		if (!currentRomData) {
			currentRomData = {
				buffer: null,
				fileName: null,
				hashStatus: "no_rom",
				gameName: staticInfo.displayName,
				expectedHash: staticInfo.expectedHash,
			};
		} else {
			currentRomData = {
				...currentRomData,
				gameName: staticInfo.displayName,
				expectedHash: staticInfo.expectedHash,
			};
		}

		let updatedRomData: RomFileData = { ...currentRomData };
		const allowedExtensions = normalizeRomExtensions(
			staticInfo.fileExtensions,
		);

		if (file) {
			updatedRomData.fileName = file.name;
			updatedRomData.buffer = null;
			updatedRomData.calculatedHash = undefined;
			updatedRomData.hashStatus = updatedRomData.expectedHash
				? "checking"
				: "uploaded_no_verify";

			{
				const next = new Map(romsData);
				next.set(gameId, updatedRomData);
				romsData = next;
			}

			const reader = new FileReader();
			reader.onload = async (e) => {
				const resultBuffer = e.target?.result as ArrayBuffer | null;
				if (resultBuffer) {
					try {
						const resolved = await resolveUploadedRomBuffer(
							file.name,
							resultBuffer,
							allowedExtensions,
							staticInfo.forcedHeaderBytes,
						);
						if (
							resolved.fileName &&
							resolved.fileName !== updatedRomData.fileName
						) {
							updatedRomData = {
								...updatedRomData,
								fileName: resolved.fileName,
							};
							const next = new Map(romsData);
							next.set(gameId, updatedRomData);
							romsData = next;
						}
						await verifyRomHash(
							gameId,
							resolved.buffer,
							updatedRomData.expectedHash,
						);
					} catch (error) {
						console.error(
							`Error processing ROM upload for ${gameId}:`,
							error,
						);
						const errorData: RomFileData = {
							...updatedRomData,
							hashStatus: "error",
							buffer: null,
						};
						{
							const next = new Map(romsData);
							next.set(gameId, errorData);
							romsData = next;
						}
						try {
							await localforage.removeItem(
								getLocalForageKey(gameId),
							);
						} catch (storageError) {
							console.error(
								`Error removing ROM data for ${gameId} from localforage:`,
								storageError,
							);
						}
						const message =
							error instanceof Error
								? error.message
								: "Failed to process uploaded ROM file.";
						alert(message);
					}
				} else {
					const errorData: RomFileData = {
						...updatedRomData,
						hashStatus: "error",
						buffer: null,
					};
					{
						const next = new Map(romsData);
						next.set(gameId, errorData);
						romsData = next;
					}
					try {
						await localforage.removeItem(getLocalForageKey(gameId));
					} catch (storageError) {
						console.error(
							`Error removing ROM data for ${gameId} from localforage:`,
							storageError,
						);
					}
				}
			};
			reader.onerror = async () => {
				const errorData: RomFileData = {
					...updatedRomData,
					hashStatus: "error",
					buffer: null,
				};
				{
					const next = new Map(romsData);
					next.set(gameId, errorData);
					romsData = next;
				}
				try {
					await localforage.removeItem(getLocalForageKey(gameId));
				} catch (error) {
					console.error(
						`Error removing ROM data for ${gameId} from localforage:`,
						error,
					);
				}
			};
			reader.readAsArrayBuffer(file);
		} else {
			const resetRomData: RomFileData = {
				...updatedRomData,
				buffer: null,
				fileName: null,
				hashStatus: "no_rom",
				calculatedHash: undefined,
			};
			{
				const next = new Map(romsData);
				next.set(gameId, resetRomData);
				romsData = next;
			}
			try {
				await localforage.removeItem(getLocalForageKey(gameId));
			} catch (error) {
				console.error(
					`Error removing ROM data for ${gameId} from localforage:`,
					error,
				);
			}
		}
	}

	async function applyPatchAndDownload() {
		const resolvedPrimaryId = primaryGameId; // Use the derived state

		if (!resolvedPrimaryId) {
			alert(
				m.seed_page_rom_patch_error_missing_seed_details() ||
					"Primary game for patching not identified.",
			);
			return;
		}

		const currentSeedOpts = data.seedDetails?.options;
		if (romUploadGameIds.length === 0 || !currentSeedOpts) {
			alert(
				m.seed_page_rom_patch_error_missing_seed_details() ||
					"Game list not available.",
			);
			return;
		}

		for (const gameId of romUploadGameIds) {
			const romData = romsData.get(gameId);
			const gameInfo = gameIdToStaticInfo.get(gameId);

			if (!gameInfo) {
				alert(
					m.seed_page_rom_error_missing_game_config({ gameId }) ||
						`Error: Game with ID '${gameId}' is not configured.`,
				);
				return;
			}

			if (!romData || !romData.buffer) {
				alert(
					m.seed_page_rom_patch_error_missing_file_for_game({
						gameName: gameInfo.displayName || gameId,
					}) ||
						`ROM for ${gameInfo.displayName || gameId} is missing.`,
				);
				return;
			}

			if (gameInfo.expectedHash && romData.hashStatus !== "verified") {
				alert(
					m.seed_page_rom_patch_error_unverified_for_game({
						gameName: gameInfo.displayName || gameId,
					}) ||
						`ROM for ${gameInfo.displayName || gameId} is not verified.`,
				);
				return;
			}
		}

		if (!data.seedDetails?.patchData) {
			alert(m.seed_page_rom_patch_error_missing_patch_data());
			return;
		}

		const primaryRomData = romsData.get(resolvedPrimaryId); // resolvedPrimaryId is string here
		const primaryGameInfo = gameIdToStaticInfo.get(resolvedPrimaryId); // resolvedPrimaryId is string here

		if (!primaryRomData || !primaryRomData.buffer || !primaryGameInfo) {
			alert(m.seed_page_rom_patch_error_generic());
			return;
		}

		const additionalRoms: { [gameId: string]: ArrayBuffer } = {};
		for (const gameId of romUploadGameIds) {
			if (gameId !== resolvedPrimaryId) {
				// resolvedPrimaryId is string here
				const romData = romsData.get(gameId);
				if (romData?.buffer) {
					additionalRoms[gameId] = romData.buffer;
				}
			}
		}

		const currentPatchData = data.seedDetails.patchData;

		if (
			!(
				typeof currentPatchData === "string" ||
				currentPatchData instanceof ArrayBuffer
			)
		) {
			console.error(
				"Patch data is in an unrecognized format:",
				currentPatchData,
			);
			alert(m.seed_page_rom_patch_error_generic());
			patchingServiceError.set(m.seed_page_rom_patch_error_generic());
			return;
		}

		// Derive randomizerId:
		const randomizerId = resolvedRandomizerIdRaw ?? undefined;

		const primaryGameOptionsEntry = visibleGameOptions.find(
			(game) => game.id === resolvedPrimaryId,
		);
		const optionSummary = buildOptionSummaryTokens({
			metadata: data.metadata,
			gameId: resolvedPrimaryId,
			options: primaryGameOptionsEntry?.options,
			maxTokens: 6,
			maxTokenLength: 12,
		});
		let optionSummaryTokens = optionSummary.tokens;
		if (optionSummary.total > optionSummary.tokens.length) {
			const extraCount =
				optionSummary.total - optionSummary.tokens.length;
			if (extraCount > 0) {
				optionSummaryTokens = [
					...optionSummaryTokens,
					`x${extraCount}`,
				];
			}
		}

		const patchParams: InitiatePatchingParams = {
			baseRomBuffer: primaryRomData.buffer,
			patchDataSource: currentPatchData,
			additionalRoms: additionalRoms,
			primaryGameId: resolvedPrimaryId, // resolvedPrimaryId is string here
			randomizerId,
			basePatchUrl: `/api/seed/${data.seedDetails.id}/base-patch`,
			outputFileNameDetails: {
				seedId: data.seedDetails.id,
				baseName: primaryRomData.fileName,
				defaultDisplayName: primaryGameInfo?.displayName || "rom",
				defaultExtension:
					primaryGameInfo?.fileExtensions.split(",")[0] || ".sfc",
				randomizerVersion: data.randomizerVersion?.versionTag ?? null,
				optionSummary: {
					tokens: optionSummaryTokens,
					total: optionSummary.total,
					derivedFromMetadata: optionSummary.derivedFromMetadata,
				},
			},
			selectedSpritesByGameId,
			gameIdToSpriteInfoMap,
			// Post-generation settings
			selectedPostGenByGameId,
			gameIdToPostGenConfigMap,
		};

		try {
			await initiatePatching(patchParams);
		} catch (error) {
			console.error("Error during initiatePatching call:", error);
		}
	}

	let allRomsReadyForPatching = $derived(
		(() => {
			const resolvedPrimaryId = primaryGameId; // Use the derived state
			if (!resolvedPrimaryId) return false;

			if (romUploadGameIds.length === 0) return false;

			for (const gameId of romUploadGameIds) {
				const romData = romsData.get(gameId);
				const gameInfo = gameIdToStaticInfo.get(gameId);

				if (!gameInfo) return false;
				if (!romData || !romData.buffer) return false;

				if (gameInfo.expectedHash) {
					if (romData.hashStatus !== "verified") return false;
				} else {
					if (
						romData.hashStatus !== "uploaded_no_verify" &&
						romData.hashStatus !== "verified"
					)
						return false;
				}
			}
			return true;
		})(),
	);
</script>

<div class="container mx-auto px-4 py-4">
	<h1 class="text-2xl font-bold mb-4 text-primary-600 dark:text-primary-400">
		{m.permalink_title()}
	</h1>

	{#if $page.error}
		<div
			class="bg-red-100 border-l-4 border-red-500 text-red-700 p-3 dark:bg-red-700 dark:text-red-200 dark:border-red-600 text-sm"
			role="alert"
		>
			<p class="font-bold">{m.error_label()}</p>
			<p>{$page.error.message}</p>
		</div>
	{:else if data.seedDetails}
		{@const seedDetails = data.seedDetails}
		{@const seedOptions =
			visibleGameOptions.length > 0 ? visibleGameOptions[0].options : {}}
		<div
			class="space-y-4 bg-white dark:bg-slate-800 p-4 rounded-lg shadow-lg"
		>
			<div>
				<h2
					class="text-xl font-semibold mb-1 text-slate-900 dark:text-slate-100 flex items-center"
				>
					<svg
						xmlns="http://www.w3.org/2000/svg"
						class="h-5 w-5 mr-2 text-primary-600 dark:text-primary-400"
						fill="none"
						viewBox="0 0 24 24"
						stroke="currentColor"
						stroke-width="2"
						><path
							stroke-linecap="round"
							stroke-linejoin="round"
							d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4"
						/></svg
					>
					{m.seed_id()}:
					<span
						class="font-mono text-primary-600 dark:text-primary-400 ml-2"
						>{seedDetails.id}</span
					>
				</h2>
				<p class="text-xs text-slate-600 dark:text-slate-300">
					<strong class="text-slate-700 dark:text-slate-200"
						>{m.seed_created_at()}:</strong
					>
					{new Date(seedDetails.createdAt).toLocaleString()}
				</p>
				{#if data.randomizerVersion}
					{#if data.randomizerVersion.versionTag}
						<p
							class="text-xs text-slate-600 dark:text-slate-300 mt-1"
						>
							<strong class="text-slate-700 dark:text-slate-200"
								>Randomizer version:</strong
							>
							<span class="font-mono"
								>{data.randomizerVersion.versionTag}</span
							>
						</p>
					{/if}
					{@const formattedBuildDate = formatIsoTimestamp(
						data.randomizerVersion.buildDate,
					)}
					{#if formattedBuildDate || data.randomizerVersion.gitCommitHash}
						<p
							class="text-xs text-slate-600 dark:text-slate-300 mt-1"
						>
							<strong class="text-slate-700 dark:text-slate-200"
								>Build info:</strong
							>
							{#if formattedBuildDate}
								{formattedBuildDate}
							{/if}
							{#if data.randomizerVersion.gitCommitHash}
								{#if formattedBuildDate}
									<span aria-hidden="true"> · </span>
								{/if}
								<span>
									commit
									<span class="font-mono ml-1">
										{data.randomizerVersion.gitCommitHash}
									</span>
								</span>
							{/if}
						</p>
					{/if}
				{/if}
			</div>
			<div>
				<h3
					class="text-lg font-semibold mb-1 text-slate-900 dark:text-slate-100"
				>
					{m.seed_options()}:
				</h3>
				<SeedOptionsViewer
					options={visibleGameOptions}
					{globalOptions}
					metadata={data.metadata as Metadata | null}
					visibility={["Basic", "Advanced", "Expert"]}
				/>
			</div>

			<!-- Post-generation settings -->
			<div
				class="mt-4 pt-4 border-t border-slate-300 dark:border-slate-600"
			>
				<h3
					class="text-lg font-semibold text-slate-900 dark:text-slate-100 mb-1"
				>
					{m.post_generation_settings_title()}
				</h3>
				<p class="text-xs text-slate-600 dark:text-slate-300 mb-2">
					{m.post_generation_settings_description()}
				</p>
				<div class="space-y-4">
					{#each visibleGameOptions as game (game.id)}
						{@const gameId = game.id.toLowerCase()}
						{@const staticInfo = gameIdToStaticInfo.get(gameId)}
						{@const gameDisplayName =
							staticInfo?.displayName || `Game: ${gameId}`}
						{@const gameSpriteConfig =
							gameIdToSpriteInfoMap.get(gameId)}
						{@const spriteOptionsForGame =
							gameSpriteConfig?.sprites || []}
						{@const spriteOptionsWithDefault = [
							{
								value: "",
								name: "Default sprite",
								imagePath: "",
							},
							...spriteOptionsForGame,
						]}
						<!-- Use direct reactive lookup instead of {@const} to allow updates -->

						<!-- Post-generation settings for this game -->
						{@const postGenConfig =
							gameIdToPostGenConfigMap.get(gameId)}
						{@const postGenOptions = postGenConfig?.options ?? []}
						{@const hasSpriteOptions =
							spriteOptionsForGame.length > 0}
						{@const hasPostGenOptions = postGenOptions.length > 0}
						{#if hasSpriteOptions || hasPostGenOptions}
							<!-- Per‑game label for clarity in multi‑rando -->
							<div class="flex items-center gap-2 mt-3 mb-2">
								<div
									class="h-4 w-1 rounded bg-indigo-500"
								></div>
								<h4
									class="text-[12px] font-semibold uppercase tracking-wide text-slate-600 dark:text-slate-300"
								>
									{gameDisplayName}
								</h4>
							</div>
							<div
								class="w-full rounded-md ring-1 ring-slate-200 dark:ring-slate-700 bg-slate-50/60 dark:bg-slate-900/40 p-3 grid grid-cols-1 md:grid-cols-2 gap-x-4 gap-y-4"
							>
								{#if hasSpriteOptions}
									<div
										class="space-y-1 md:col-span-2 md:w-1/2"
									>
										<label
											for={`sprite-select-${gameId}`}
											class="block text-xs font-medium text-slate-900 dark:text-slate-100"
										>
											{`Sprite for ${gameDisplayName}`}
										</label>
										<SpriteSelect
											id={`sprite-select-${gameId}`}
											game={gameId}
											items={spriteOptionsWithDefault}
											value={spriteSelections[gameId] ||
												""}
											on:change={async (
												e: CustomEvent<{
													value: string;
												}>,
											) => {
												const newVal = e.detail.value;
												selectedSpritesByGameId.set(
													gameId,
													newVal,
												);
												spriteSelections[gameId] =
													newVal;
												selectedSpritesByGameId =
													selectedSpritesByGameId;
												spriteSelections = {
													...spriteSelections,
												};
												await persistSpriteSelection(
													gameId,
													newVal,
												);
											}}
											placeholder={m.sprite_select_placeholder() ||
												"Select a sprite"}
											className="text-xs"
										/>
									</div>
								{/if}

								{#if hasPostGenOptions}
									{#each postGenOptions as opt (opt.id)}
										{#if opt.type === "toggle"}
											<div class="space-y-1">
												<label
													class="block text-xs font-medium text-slate-900 dark:text-slate-100"
													for={`postgen-${gameId}-${opt.id}`}
													>{opt.name}</label
												>
												{#if opt.description}
													<p
														class="text-xs text-slate-500 dark:text-slate-400"
													>
														{opt.description}
													</p>
												{/if}
												<Toggle
													id={`postgen-${gameId}-${opt.id}`}
													size="sm"
													checked={(postGenSelections[
														gameId
													]?.[opt.id] as boolean) ??
														opt.default ??
														false}
													on:change={(
														e: CustomEvent<{
															checked: boolean;
														}>,
													) => {
														const on =
															e.detail.checked;
														const cur =
															selectedPostGenByGameId.get(
																gameId,
															) || {};
														cur[opt.id] = on;
														selectedPostGenByGameId.set(
															gameId,
															cur,
														);
														postGenSelections[
															gameId
														] = {
															...(postGenSelections[
																gameId
															] || {}),
															[opt.id]: on,
														};
														selectedPostGenByGameId =
															selectedPostGenByGameId;
														postGenSelections = {
															...postGenSelections,
														};
														persistPostGenSelection(
															gameId,
															postGenSelections[
																gameId
															],
														);
													}}
												/>
												{#if opt.id === "palette_randomize" && ((postGenSelections[gameId]?.[opt.id] as boolean) ?? opt.default ?? false)}
													<!-- Nested z3pr settings when palette randomizer is enabled -->
													<div
														class="mt-2 space-y-3 p-3 rounded-md bg-white/60 dark:bg-slate-800/50 ring-1 ring-slate-200 dark:ring-slate-700"
													>
														<!-- Mode select -->
														<label
															class="block mb-1 text-xs font-medium text-slate-900 dark:text-slate-100"
															for={`postgen-${gameId}-palette-mode`}
														>
															Palette Mode
														</label>
														<select
															id={`postgen-${gameId}-palette-mode`}
															class="w-full text-xs border border-slate-300 dark:border-slate-600 rounded p-1 bg-white dark:bg-slate-800"
															value={(postGenSelections[
																gameId
															]?.[
																"palette_randomize_mode"
															] as string) ??
																"maseya"}
															onchange={(e) => {
																const v = (
																	e.currentTarget as HTMLSelectElement
																).value;
																const cur =
																	selectedPostGenByGameId.get(
																		gameId,
																	) || {};
																cur[
																	"palette_randomize_mode"
																] = v;
																selectedPostGenByGameId.set(
																	gameId,
																	cur,
																);
																postGenSelections[
																	gameId
																] = {
																	...(postGenSelections[
																		gameId
																	] || {}),
																	palette_randomize_mode:
																		v,
																};
																selectedPostGenByGameId =
																	selectedPostGenByGameId;
																postGenSelections =
																	{
																		...postGenSelections,
																	};
																persistPostGenSelection(
																	gameId,
																	postGenSelections[
																		gameId
																	],
																);
															}}
														>
															<option
																value="maseya"
																>Maseya</option
															>
															<option
																value="grayscale"
																>Grayscale</option
															>
															<option
																value="negative"
																>Negative</option
															>
															<option
																value="blackout"
																>Blackout</option
															>
															<option
																value="classic"
																>Classic</option
															>
															<option
																value="dizzy"
																>Dizzy (Hue)</option
															>
															<option value="sick"
																>Sick (Luma)</option
															>
															<option value="puke"
																>Puke (Random)</option
															>
														</select>

														<!-- Scope toggles -->
														<div
															class="grid grid-cols-2 gap-3 mt-1"
														>
															{#each [{ key: "palette_randomize_overworld", label: "Overworld" }, { key: "palette_randomize_dungeon", label: "Dungeon/Underworld" }, { key: "palette_randomize_link_sprite", label: "Link Sprite" }, { key: "palette_randomize_sword", label: "Sword" }, { key: "palette_randomize_shield", label: "Shield" }, { key: "palette_randomize_hud", label: "HUD" }] as flag (flag.key)}
																<div
																	class="flex items-center justify-between gap-2"
																>
																	<span
																		class="text-[11px] text-slate-900 dark:text-slate-100"
																		>{flag.label}</span
																	>
																	<Toggle
																		id={`postgen-${gameId}-${flag.key}`}
																		size="sm"
																		checked={(postGenSelections[
																			gameId
																		]?.[
																			flag
																				.key
																		] as boolean) ??
																			true}
																		on:change={(
																			e: CustomEvent<{
																				checked: boolean;
																			}>,
																		) => {
																			const on =
																				e
																					.detail
																					.checked;
																			const cur =
																				selectedPostGenByGameId.get(
																					gameId,
																				) ||
																				{};
																			cur[
																				flag.key
																			] =
																				on;
																			selectedPostGenByGameId.set(
																				gameId,
																				cur,
																			);
																			postGenSelections[
																				gameId
																			] =
																				{
																					...(postGenSelections[
																						gameId
																					] ||
																						{}),
																					[flag.key]:
																						on,
																				};
																			selectedPostGenByGameId =
																				selectedPostGenByGameId;
																			postGenSelections =
																				{
																					...postGenSelections,
																				};
																			persistPostGenSelection(
																				gameId,
																				postGenSelections[
																					gameId
																				],
																			);
																		}}
																	/>
																</div>
															{/each}
														</div>
													</div>
												{/if}
											</div>
										{/if}
										{#if opt.type === "select"}
											<div class="space-y-1">
												<label
													class="block mb-1 text-xs font-medium text-slate-900 dark:text-slate-100"
													for={`postgen-${gameId}-${opt.id}`}
												>
													{opt.name}
												</label>
												{#if opt.description}
													<p
														class="text-xs text-slate-500 dark:text-slate-400 mb-1"
													>
														{opt.description}
													</p>
												{/if}
												<select
													id={`postgen-${gameId}-${opt.id}`}
													class="w-full text-xs border border-slate-300 dark:border-slate-600 rounded p-1 bg-white dark:bg-slate-800"
													value={(postGenSelections[
														gameId
													]?.[opt.id] as string) ??
														opt.default ??
														opt.choices[0]?.value}
													onchange={(e) => {
														const v = (
															e.currentTarget as HTMLSelectElement
														).value;
														const cur =
															selectedPostGenByGameId.get(
																gameId,
															) || {};
														cur[opt.id] = v;
														selectedPostGenByGameId.set(
															gameId,
															cur,
														);
														postGenSelections[
															gameId
														] = {
															...(postGenSelections[
																gameId
															] || {}),
															[opt.id]: v,
														};
														selectedPostGenByGameId =
															selectedPostGenByGameId;
														postGenSelections = {
															...postGenSelections,
														};
														persistPostGenSelection(
															gameId,
															postGenSelections[
																gameId
															],
														);
													}}
												>
													{#each opt.choices as c (c.value)}
														<option value={c.value}
															>{c.label}</option
														>
													{/each}
												</select>
											</div>
										{/if}
									{/each}
								{:else}
									<p
										class="md:col-span-2 text-[11px] text-slate-500 dark:text-slate-400 mt-1"
									>
										No post-generation settings for {gameDisplayName}.
									</p>
								{/if}
							</div>
						{/if}
					{/each}
				</div>
			</div>
			<!-- ROM Upload and Patching Section -->
			<div
				class="p-3 border border-slate-200 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800/70 shadow mt-4 pt-4 border-t border-slate-300 dark:border-slate-600"
			>
				<h3
					class="text-lg font-semibold text-slate-900 dark:text-slate-100 mb-1"
				>
					{m.seed_page_patch_download_title()}
				</h3>
				{#if allRomsReadyForPatching}
					<p
						class="mb-2 inline-flex items-center gap-2 text-xs font-medium text-emerald-700 dark:text-emerald-300"
					>
						<svg
							class="h-3.5 w-3.5"
							viewBox="0 0 20 20"
							fill="currentColor"
							aria-hidden="true"
						>
							<path
								fill-rule="evenodd"
								d="M16.704 5.29a1 1 0 0 1 .006 1.414l-7.07 7.072a1 1 0 0 1-1.42.007L3.29 8.852a1 1 0 0 1 1.418-1.41l4.093 4.116 6.363-6.364a1 1 0 0 1 1.54.096"
								clip-rule="evenodd"
							/>
						</svg>
						{m.seed_ready_to_patch()}
					</p>
				{:else}
					<p class="text-xs text-slate-600 dark:text-slate-300 mb-2">
						{m.seed_page_patch_download_instructions()}
					</p>
				{/if}
				<!-- Display patching error from the service store -->
				{#if $patchingServiceError}
					<div
						class="my-2 bg-red-100 border-l-4 border-red-500 text-red-700 p-2 dark:bg-red-700 dark:text-red-200 dark:border-red-600 text-xs"
						role="alert"
					>
						<p class="font-bold">{m.error_label()}</p>
						<p>{$patchingServiceError}</p>
					</div>
				{/if}
				{#if seedOptions && romUploadGameIds.length > 0}
					{#if !allRomsReadyForPatching}
						{#each romUploadGameIds as gameId (gameId)}
							{@const staticInfo = gameIdToStaticInfo.get(gameId)}
							{#if staticInfo}
								<RomUploader
									{gameId}
									gameName={staticInfo.displayName}
									expectedFileExtensions={staticInfo.fileExtensions}
									hashStatus={romsData.get(gameId)
										?.hashStatus}
									calculatedHash={romsData.get(gameId)
										?.calculatedHash}
									expectedHash={romsData.get(gameId)
										?.expectedHash}
									fileName={romsData.get(gameId)?.fileName}
									useCard={false}
									onFileSelected={(d) =>
										handleFileUpload(
											d as FileSelectedEventDetail,
											gameId,
										)}
								/>
							{/if}
						{/each}
					{/if}
					<div class="mt-4 space-y-3">
						{#if $patchingServiceIsPatching}
							<div>
								<Progressbar
									progress={$patchingServiceProgress}
									size="h-4"
									labelInside
									className="mb-1 dark:bg-slate-600 text-xs"
								/>
								<p
									class="text-xs text-center text-slate-700 dark:text-slate-300"
								>
									{m.seed_page_patching_in_progress_label()}...
								</p>
							</div>
						{/if}
						<Button
							onclick={applyPatchAndDownload}
							disabled={$patchingServiceIsPatching ||
								!seedDetails.patchData ||
								!allRomsReadyForPatching}
							size="sm"
							fullWidth={true}
							variant="primary"
							className="flex justify-center items-center"
						>
							{#if $patchingServiceIsPatching}
								{m.seed_page_patching_button()}
							{:else if !allRomsReadyForPatching}
								{m.seed_upload_required_roms()}
							{:else}
								{m.seed_page_apply_patch_and_download_button()}
							{/if}
						</Button>
					</div>
				{:else}
					<p class="text-sm text-slate-500 dark:text-slate-400">
						{m.seed_page_no_games_for_rom_upload()}
					</p>
				{/if}
			</div>

			<!-- Spoiler Log Section -->
			{#if seedDetails.spoilerLog}
				<SpoilerLog spoilerLog={seedDetails.spoilerLog} />
			{/if}
		</div>
	{:else}
		<p
			class="text-base text-center text-slate-600 dark:text-slate-300 bg-white dark:bg-slate-800 p-4 rounded-lg shadow-md"
		>
			{m.loading_seed_details()}
		</p>
	{/if}
</div>
