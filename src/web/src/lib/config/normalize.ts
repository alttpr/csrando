import type {
  Metadata,
  MetadataSetting,
  SingleChoiceSetting,
  MultipleChoiceSetting,
  SliderSetting,
  ToggleSetting,
  InputSetting,
  GenericSetting,
} from "$lib/types";

// The canonical seed-configuration shape stored in preset revisions, drafts and
// seed snapshots. It mirrors the config form state (not the generator payload,
// which is lossy and cannot be hydrated back into the form).
export interface NormalizedConfig {
  selectedGames: string[];
  global: Record<string, unknown>;
  perGame: Record<string, Record<string, unknown>>;
}

// The live form state on the config page; same shape as NormalizedConfig but
// values may be un-canonicalized (untrimmed strings, unsorted arrays, ...).
export type FormStateSnapshot = NormalizedConfig;

export interface HydrationReport {
  // Settings present in the stored config that no longer exist in metadata.
  removedKeys: string[];
  // Settings whose stored value was invalid and got reset to the default.
  resetKeys: string[];
}

export interface AvailableGame {
  id: string;
  name: string;
  description?: string;
}

export function getDefaultValue(option: MetadataSetting): unknown {
  switch (option.type) {
    case "SingleChoice": {
      const sc = option as SingleChoiceSetting;
      if (sc.default && sc.default in option.values) return sc.default;
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

export interface InitializedFormValues {
  availableGames: AvailableGame[];
  formGlobal: Record<string, unknown>;
  formPerGame: Record<string, Record<string, unknown>>;
  selectedGames: string[];
  activeTab: string | null;
}

export function initializeFormValues(
  metadata: Metadata,
): InitializedFormValues {
  const result: InitializedFormValues = {
    availableGames: [],
    formGlobal: {},
    formPerGame: {},
    selectedGames: [],
    activeTab: null,
  };

  if (metadata.settings) {
    for (const option of metadata.settings) {
      result.formGlobal[option.key] = getDefaultValue(option);
    }
  }

  if (metadata.gameSettings) {
    for (const game in metadata.gameSettings) {
      const gameSettings = metadata.gameSettings[game];

      result.formPerGame[game] = {};

      const gameSpecificOptions =
        gameSettings && gameSettings.settings ? gameSettings.settings : [];

      // Only add to availableGames if there are actual settings
      if (
        Array.isArray(gameSpecificOptions) &&
        gameSpecificOptions.length > 0
      ) {
        result.availableGames.push({
          id: game,
          name:
            (gameSettings as unknown as { game?: { name?: string } }).game
              ?.name || game,
          description: (
            gameSettings as unknown as { game?: { description?: string } }
          ).game?.description,
        });

        for (const option of gameSpecificOptions) {
          if (option && typeof option.key === "string") {
            result.formPerGame[game][option.key] = getDefaultValue(option);
          }
        }
      }
    }
  }

  if (result.availableGames.length > 0) {
    result.selectedGames = result.availableGames.map((game) => game.id);
    result.activeTab = result.availableGames[0].id;
  }

  return result;
}

const RANDOM_PICK = "randompick";

function isRandomSelection(value: unknown): boolean {
  return (
    typeof value === "string" && value.trim().toLowerCase() === RANDOM_PICK
  );
}

interface CanonicalResult {
  value: unknown;
  reset: boolean;
}

function allowedChoiceValues(
  values: Record<string, string>,
): Set<string | number> {
  const allowed = new Set<string | number>();
  for (const [key, value] of Object.entries(values)) {
    allowed.add(key);
    allowed.add(value);
  }
  return allowed;
}

function canonicalizeValue(
  option: MetadataSetting,
  raw: unknown,
): CanonicalResult {
  switch (option.type) {
    case "SingleChoice": {
      const sc = option as SingleChoiceSetting;
      if (typeof raw === "number") return { value: raw, reset: false };
      if (typeof raw === "string") {
        const trimmed = raw.trim();
        if (
          isRandomSelection(trimmed) ||
          allowedChoiceValues(sc.values).has(trimmed) ||
          trimmed === ""
        ) {
          return { value: trimmed, reset: false };
        }
      }
      return { value: getDefaultValue(option), reset: true };
    }
    case "MultipleChoice": {
      const mc = option as MultipleChoiceSetting;
      if (!Array.isArray(raw)) {
        return { value: getDefaultValue(option), reset: raw !== undefined };
      }
      const allowed = allowedChoiceValues(mc.values);
      const kept: string[] = [];
      let dropped = false;
      for (const item of raw) {
        if (
          typeof item === "string" &&
          (allowed.has(item) || isRandomSelection(item))
        ) {
          if (!kept.includes(item)) kept.push(item);
        } else {
          dropped = true;
        }
      }
      // Selection order carries no meaning; sort for deterministic output.
      kept.sort();
      return { value: kept, reset: dropped };
    }
    case "Slider": {
      const slider = option as SliderSetting;
      const num =
        typeof raw === "number"
          ? raw
          : typeof raw === "string" && raw.trim() !== ""
            ? Number(raw)
            : NaN;
      if (Number.isFinite(num)) {
        const from = slider.range.from ?? 0;
        const to = slider.range.to;
        if (num >= from && num <= to) return { value: num, reset: false };
      }
      return { value: getDefaultValue(option), reset: true };
    }
    case "Toggle": {
      if (typeof raw === "boolean") return { value: raw, reset: false };
      return { value: getDefaultValue(option), reset: true };
    }
    case "Input": {
      if (typeof raw === "string") return { value: raw.trim(), reset: false };
      if (typeof raw === "number") return { value: raw, reset: false };
      return { value: getDefaultValue(option), reset: true };
    }
    case "Generic": {
      if (
        raw === null ||
        typeof raw === "string" ||
        typeof raw === "number" ||
        typeof raw === "boolean"
      ) {
        return { value: raw, reset: false };
      }
      return { value: getDefaultValue(option), reset: true };
    }
    default:
      return { value: raw, reset: false };
  }
}

function canonicalizeSection(
  options: MetadataSetting[],
  source: Record<string, unknown> | undefined,
  keyPrefix: string,
  report: HydrationReport | null,
): Record<string, unknown> {
  const result: Record<string, unknown> = {};
  const known = new Set<string>();
  for (const option of options) {
    known.add(option.key);
    const hasValue = !!source && option.key in source;
    if (!hasValue) {
      result[option.key] = getDefaultValue(option);
      continue;
    }
    const raw = source![option.key];
    const { value, reset } = canonicalizeValue(option, raw);
    result[option.key] = value;
    // Only report a reset when the stored value actually changed; e.g. an
    // auxiliary slider whose default sits outside its own range is not an error.
    if (reset && report && stableStringify(value) !== stableStringify(raw)) {
      report.resetKeys.push(`${keyPrefix}${option.key}`);
    }
  }
  if (report && source) {
    for (const key of Object.keys(source)) {
      if (!known.has(key)) report.removedKeys.push(`${keyPrefix}${key}`);
    }
  }
  return result;
}

function canonicalize(
  form: Partial<FormStateSnapshot>,
  metadata: Metadata,
  report: HydrationReport | null,
): NormalizedConfig {
  const global = canonicalizeSection(
    metadata.settings ?? [],
    form.global,
    "global.",
    report,
  );

  const perGame: Record<string, Record<string, unknown>> = {};
  const availableGameIds = new Set<string>();
  for (const [game, entry] of Object.entries(metadata.gameSettings ?? {})) {
    const options = entry?.settings ?? [];
    perGame[game] = canonicalizeSection(
      options,
      form.perGame?.[game],
      `${game}.`,
      report,
    );
    if (options.length > 0) availableGameIds.add(game);
  }
  if (report && form.perGame) {
    for (const game of Object.keys(form.perGame)) {
      if (!(game in perGame)) report.removedKeys.push(`${game}.*`);
    }
  }

  const selectedGames: string[] = [];
  for (const game of form.selectedGames ?? []) {
    if (availableGameIds.has(game) && !selectedGames.includes(game)) {
      selectedGames.push(game);
    } else if (report && !availableGameIds.has(game)) {
      report.removedKeys.push(`selectedGames.${game}`);
    }
  }
  selectedGames.sort();

  return { selectedGames, global, perGame };
}

// Canonicalize live form state against metadata. Deterministic: same inputs
// always produce structurally identical output (compare with configsEqual).
export function normalizeConfig(
  form: FormStateSnapshot,
  metadata: Metadata,
): NormalizedConfig {
  return canonicalize(form, metadata, null);
}

// Inverse of normalizeConfig: stored settings -> full form state on top of
// metadata defaults, reporting anything removed or reset instead of silently
// replacing it.
export function hydrateFormState(
  config: Partial<NormalizedConfig>,
  metadata: Metadata,
): {
  form: FormStateSnapshot;
  availableGames: AvailableGame[];
  activeTab: string | null;
  report: HydrationReport;
} {
  const report: HydrationReport = { removedKeys: [], resetKeys: [] };
  const normalized = canonicalize(config, metadata, report);
  const init = initializeFormValues(metadata);

  let selectedGames = normalized.selectedGames;
  if (selectedGames.length === 0) {
    // A config that selects no available game is unusable; fall back to all.
    selectedGames = init.selectedGames;
  }
  const activeTab =
    init.availableGames.find((g) => selectedGames.includes(g.id))?.id ?? null;

  return {
    form: {
      selectedGames,
      global: normalized.global,
      perGame: normalized.perGame,
    },
    availableGames: init.availableGames,
    activeTab,
    report,
  };
}

function sortDeep(value: unknown): unknown {
  if (Array.isArray(value)) return value.map(sortDeep);
  if (value && typeof value === "object") {
    const entries = Object.entries(value as Record<string, unknown>)
      .filter(([, v]) => v !== undefined)
      .sort(([a], [b]) => (a < b ? -1 : a > b ? 1 : 0));
    const result: Record<string, unknown> = {};
    for (const [k, v] of entries) result[k] = sortDeep(v);
    return result;
  }
  return value;
}

// JSON serialization with recursively sorted object keys, so structurally
// equal configs always serialize identically.
export function stableStringify(value: unknown): string {
  return JSON.stringify(sortDeep(value));
}

export function configsEqual(a: unknown, b: unknown): boolean {
  return stableStringify(a) === stableStringify(b);
}

export interface RandomizePayload {
  Seed: number;
  IncludeSpoiler: boolean;
  Configs: Array<Record<string, unknown>>;
}

// Build the generator request from a normalized config. This is the single
// serialization path for seed generation (extracted from the config page's
// former inline submit logic).
export function buildRandomizePayload(
  config: NormalizedConfig,
  metadata: Metadata,
  opts: { includeSpoiler: boolean; seed?: number },
): RandomizePayload {
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
        if (!trimmed || trimmed.toLowerCase() === RANDOM_PICK) {
          continue;
        }
        entries.push([key, trimmed]);
        continue;
      }

      entries.push([key, value]);
    }
    return Object.fromEntries(entries);
  };

  // A setting gated to an explicit game list (onlyWithGames) is omitted as soon
  // as the selection includes any game outside that list; the stored form value
  // survives, so narrowing the selection back restores it.
  const violatesGameGate = (setting: MetadataSetting): boolean =>
    !!setting.onlyWithGames &&
    config.selectedGames.some((game) => !setting.onlyWithGames!.includes(game));

  const gameSettings: Record<string, Record<string, unknown>> = {};

  for (const gameId of config.selectedGames) {
    const currentGameOptions = config.perGame[gameId];

    if (currentGameOptions) {
      const validGameOptions = filterNonNullValues(currentGameOptions);

      for (const setting of metadata?.gameSettings?.[gameId]?.settings ?? []) {
        if (setting.key in validGameOptions && violatesGameGate(setting)) {
          delete validGameOptions[setting.key];
        }
      }

      if (Object.keys(validGameOptions).length > 0) {
        gameSettings[gameId] = validGameOptions;
      }
    }
  }

  // Transform per-game settings for sliders that provide options (optionsFor)
  if (metadata?.gameSettings) {
    for (const [gameKey, gameMeta] of Object.entries(metadata.gameSettings)) {
      for (const setting of gameMeta.settings) {
        if (setting.type === "Slider" && setting.optionsFor) {
          const currentVal = (gameSettings[gameKey] || {})[setting.key];
          if (currentVal === undefined) {
            // If user didn't pick, send full numeric range as array
            const from = setting.range.from ?? 0;
            const to = setting.range.to;
            const arr = Array.from(
              { length: to - from + 1 },
              (_, i) => i + from,
            );
            if (!gameSettings[gameKey]) gameSettings[gameKey] = {};
            gameSettings[gameKey][setting.key] = arr;
          } else if (typeof currentVal === "number") {
            gameSettings[gameKey][setting.key] = [currentVal];
          }
        }
      }
    }
  }

  // Use the explicit global Game setting (RandomizerTarget enum) provided by
  // metadata instead of deriving.
  let globalGameTarget = (config.global["Game"] as string) || "";
  if (!globalGameTarget && metadata?.settings) {
    const gameSetting = metadata.settings.find((s) => s.key === "Game");
    if (
      gameSetting &&
      "default" in gameSetting &&
      typeof (gameSetting as { default?: unknown }).default === "string"
    ) {
      globalGameTarget = (gameSetting as { default?: string })
        .default as string;
    }
  }
  if (!globalGameTarget) globalGameTarget = "Alttpr";

  const worldConfig: Record<string, unknown> = {
    Language: (config.global["Language"] as string) || "en",
  };
  if (!isRandomSelection(globalGameTarget)) {
    worldConfig.Game = globalGameTarget;
  }

  const worldGameKeys = new Set<string>();
  for (const gameKey of config.selectedGames) {
    worldGameKeys.add(gameKey);
  }

  for (const gameKey of worldGameKeys) {
    const perGame =
      gameSettings[gameKey] ?? filterNonNullValues(config.perGame[gameKey]);
    worldConfig[gameKey] = perGame || {};
  }

  if ((globalGameTarget || "").toLowerCase() === "combo") {
    worldConfig["Combo"] = filterNonNullValues(config.perGame["Combo"]) || {};
  }

  return {
    Seed: opts.seed ?? 0,
    IncludeSpoiler: opts.includeSpoiler,
    Configs: [worldConfig],
  };
}
