import type { Metadata, MetadataSetting } from "$lib/types";

const GAME_ALIASES: Record<string, string[]> = {
  alttp: ["alttpr", "zelda3", "z3"],
  alttpr: ["alttp", "zelda3", "z3"],
  z1: ["z1r", "zelda1"],
  z1r: ["z1", "zelda1"],
  g2: ["g2r", "goonies2"],
  g2r: ["g2", "goonies2"],
};

export interface OptionSummaryParams {
  metadata: Metadata | null | undefined;
  gameId: string | null | undefined;
  options?: Record<string, unknown> | null;
  maxTokens?: number;
  maxTokenLength?: number;
}

export interface OptionSummaryResult {
  tokens: string[];
  total: number;
  derivedFromMetadata: boolean;
}

interface CanonicalValue {
  compare: string | null;
  label: string | null;
  bool?: boolean;
  hasValue: boolean;
}

function resolveGameSettingsKey(
  metadata: Metadata | null | undefined,
  gameId: string | null | undefined,
): string | undefined {
  if (!metadata || !gameId) return undefined;
  const entries = Object.keys(metadata.gameSettings ?? {});
  if (entries.length === 0) return undefined;

  const normalized = gameId.toLowerCase();
  const byLower = new Map(
    entries.map((key) => [key.toLowerCase(), key] as const),
  );
  const direct = byLower.get(normalized);
  if (direct) return direct;

  const aliases = GAME_ALIASES[normalized] ?? [];
  for (const alias of aliases) {
    const match = byLower.get(alias.toLowerCase());
    if (match) return match;
  }

  if (entries.length === 1) return entries[0];
  return undefined;
}

function getSettingMetadata(
  metadata: Metadata | null | undefined,
  gameId: string | null | undefined,
  key: string,
): MetadataSetting | undefined {
  if (!metadata) return undefined;

  const gameKey = resolveGameSettingsKey(metadata, gameId);
  if (gameKey) {
    const entry = metadata.gameSettings?.[gameKey];
    const fromGame = entry?.settings?.find((s) => s.key === key);
    if (fromGame) return fromGame;
  }

  return metadata.settings?.find((s) => s.key === key);
}

function toBoolean(raw: unknown): boolean | undefined {
  if (typeof raw === "boolean") return raw;
  if (typeof raw === "number") {
    if (raw === 1) return true;
    if (raw === 0) return false;
  }
  if (typeof raw === "string") {
    const normalized = raw.trim().toLowerCase();
    if (["true", "yes", "y", "on", "enabled"].includes(normalized)) return true;
    if (["false", "no", "n", "off", "disabled"].includes(normalized))
      return false;
    if (normalized === "1") return true;
    if (normalized === "0") return false;
  }
  return undefined;
}

function normalizeChoice(
  values: Record<string, unknown> | undefined,
  raw: unknown,
): { key?: string; label?: string } {
  if (raw === undefined || raw === null) return {};
  const rawStr = String(raw);
  if (values && rawStr in values) {
    const label = values[rawStr];
    return {
      key: rawStr,
      label:
        label === undefined || label === null
          ? rawStr
          : String(label).trim() || rawStr,
    };
  }
  if (values) {
    const lowered = rawStr.toLowerCase();
    for (const [key, label] of Object.entries(values)) {
      const labelStr =
        label === undefined || label === null ? "" : String(label);
      if (labelStr.toLowerCase() === lowered) {
        return {
          key,
          label: labelStr || key,
        };
      }
    }
  }
  return { label: rawStr };
}

function normalizeChoiceList(
  values: Record<string, unknown> | undefined,
  raw: unknown,
): { keys: string[]; labels: string[] } {
  if (raw === undefined || raw === null) return { keys: [], labels: [] };
  const arr = Array.isArray(raw)
    ? raw
    : typeof raw === "string"
      ? raw
          .split(",")
          .map((v) => v.trim())
          .filter((v) => v.length > 0)
      : [raw];

  const keys: string[] = [];
  const labels: string[] = [];
  for (const item of arr) {
    const normalized = normalizeChoice(values, item);
    const key =
      normalized.key ??
      (normalized.label ? normalized.label.toLowerCase() : undefined);
    const label =
      normalized.label ?? (normalized.key ? normalized.key : String(item));
    if (key) keys.push(key);
    labels.push(label);
  }
  return { keys, labels };
}

function canonicalizeSettingValue(
  meta: MetadataSetting,
  raw: unknown,
): CanonicalValue {
  switch (meta.type) {
    case "Toggle": {
      const bool = toBoolean(raw);
      if (bool === undefined) {
        return { compare: null, label: null, hasValue: false };
      }
      return {
        compare: bool ? "true" : "false",
        label: bool ? "On" : "Off",
        bool,
        hasValue: true,
      };
    }
    case "Slider": {
      if (raw === undefined || raw === null)
        return { compare: null, label: null, hasValue: false };
      const num = typeof raw === "number" ? raw : Number(raw);
      if (!Number.isFinite(num)) {
        return {
          compare:
            raw === undefined || raw === null
              ? null
              : String(raw).toLowerCase(),
          label: raw === undefined || raw === null ? null : String(raw),
          hasValue: raw !== undefined && raw !== null,
        };
      }
      return {
        compare: String(num),
        label: String(num),
        hasValue: true,
      };
    }
    case "SingleChoice": {
      const normalized = normalizeChoice(
        (meta as { values?: Record<string, unknown> }).values,
        raw,
      );
      if (!normalized.key && !normalized.label) {
        return { compare: null, label: null, hasValue: false };
      }
      const compareSource = normalized.key ?? normalized.label ?? null;
      return {
        compare: compareSource ? compareSource.toLowerCase() : null,
        label: normalized.label ?? normalized.key ?? null,
        hasValue: true,
      };
    }
    case "MultipleChoice": {
      const normalized = normalizeChoiceList(
        (meta as { values?: Record<string, unknown> }).values,
        raw,
      );
      if (normalized.keys.length === 0 && normalized.labels.length === 0) {
        return { compare: null, label: null, hasValue: false };
      }
      const compareKeys = normalized.keys.map((k) => k.toLowerCase()).sort();
      const compare = compareKeys.length
        ? compareKeys.join("|")
        : normalized.labels
            .map((l) => l.toLowerCase())
            .sort()
            .join("|");
      const label = normalized.labels.join("+");
      return {
        compare: compare || null,
        label: label || null,
        hasValue: true,
      };
    }
    case "Input":
    case "Generic": {
      if (raw === undefined || raw === null || raw === "") {
        return { compare: null, label: null, hasValue: false };
      }
      const str = String(raw);
      return {
        compare: str.toLowerCase(),
        label: str,
        hasValue: true,
      };
    }
    default: {
      if (raw === undefined || raw === null) {
        return { compare: null, label: null, hasValue: false };
      }
      const str = String(raw);
      return {
        compare: str.toLowerCase(),
        label: str,
        hasValue: true,
      };
    }
  }
}

function splitIdentifier(value: string): string[] {
  if (!value) return [];
  return value
    .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
    .replace(/[-_+]+/g, " ")
    .split(/\s+/)
    .filter((segment) => segment.length > 0);
}

function abbreviateKey(key: string, maxLength: number): string {
  const words = splitIdentifier(key);
  if (words.length === 0) return key.slice(0, maxLength);
  let result = "";
  for (const word of words) {
    const lower = word.toLowerCase();
    const chunkLength = Math.min(2, Math.max(1, lower.length));
    const chunk = lower.slice(0, chunkLength);
    if (!chunk) continue;
    if (result.length === 0) {
      result += chunk.charAt(0).toUpperCase() + chunk.slice(1);
    } else {
      result += chunk;
    }
    if (result.length >= maxLength) break;
  }
  return result.slice(0, maxLength) || key.slice(0, maxLength);
}

function abbreviateValue(value: unknown, maxLength: number): string {
  if (value === null || value === undefined) return "";
  if (typeof value === "number" || typeof value === "bigint") {
    return String(value);
  }
  if (typeof value === "boolean") {
    return value ? "On" : "Off";
  }
  if (Array.isArray(value)) {
    const perItem = Math.max(
      2,
      Math.floor(maxLength / Math.max(1, value.length)),
    );
    const parts = value
      .map((entry) => abbreviateValue(entry, perItem))
      .filter((part) => part.length > 0);
    return parts.join("+") || String(value.length);
  }
  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed) return "";
    if (trimmed.includes("+")) {
      const plusParts = trimmed
        .split("+")
        .map((p) => p.trim())
        .filter((p) => p.length > 0);
      const partMax = Math.max(
        2,
        Math.floor(maxLength / Math.max(1, plusParts.length)),
      );
      const pieces = plusParts
        .map((p) => abbreviateValue(p, partMax))
        .filter((p) => p.length > 0);
      if (pieces.length > 0) return pieces.join("+");
    }
    const words = splitIdentifier(trimmed);
    if (words.length === 0) {
      return trimmed.slice(0, maxLength);
    }
    let result = "";
    for (let i = 0; i < words.length && result.length < maxLength; i++) {
      const word = words[i];
      const lower = word.toLowerCase();
      const chunkLength = i === 0 ? Math.min(3, maxLength - result.length) : 1;
      const chunk = lower.slice(0, chunkLength);
      if (!chunk) continue;
      if (i === 0) {
        result += chunk.charAt(0).toUpperCase() + chunk.slice(1);
      } else {
        result += chunk.charAt(0);
      }
    }
    return result.slice(0, maxLength) || trimmed.slice(0, maxLength);
  }
  return "Custom";
}

export function slugifyForFilename(
  input: string,
  opts?: { maxLength?: number; preserveCase?: boolean },
): string {
  if (!input) return "";
  const normalized = input
    .normalize("NFKD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^\p{ASCII}]/gu, " ");
  const working = opts?.preserveCase ? normalized : normalized.toLowerCase();
  const invalidChars = opts?.preserveCase ? /[^a-zA-Z0-9]+/g : /[^a-z0-9]+/g;
  let slug = working
    .replace(invalidChars, "-")
    .replace(/^-+/, "")
    .replace(/-+$/, "")
    .replace(/-{2,}/g, "-");

  if (!slug) return "";
  const maxLength = opts?.maxLength ?? 0;
  if (maxLength > 0 && slug.length > maxLength) {
    slug = slug.slice(0, maxLength).replace(/-+$/, "");
    if (!slug) {
      const tail = working
        .replace(invalidChars, "-")
        .replace(/^-+/, "")
        .replace(/-+$/, "")
        .replace(/-{2,}/g, "-");
      slug = tail.slice(-maxLength).replace(/^-+/, "");
    }
  }

  return slug;
}

function getDefaultForSetting(meta: MetadataSetting): unknown {
  if (Object.prototype.hasOwnProperty.call(meta, "default")) {
    const candidate = (meta as { default?: unknown }).default;
    if (candidate !== undefined) return candidate;
  }

  switch (meta.type) {
    case "Toggle":
      return false;
    case "Slider": {
      const range = (meta as { range?: { from?: number; to?: number } }).range;
      if (range && typeof range.from === "number") return range.from;
      return 0;
    }
    case "SingleChoice": {
      const values = (meta as { values?: Record<string, unknown> }).values;
      if (values) {
        const first = Object.keys(values)[0];
        if (first !== undefined) return first;
      }
      return null;
    }
    case "MultipleChoice":
      return [];
    case "Input":
    case "Generic":
      return null;
    default:
      return null;
  }
}

function buildToken(
  meta: MetadataSetting,
  actual: CanonicalValue,
  defaults: CanonicalValue,
  maxTokenLength: number,
): string | null {
  if (!actual.hasValue) return null;
  if ((actual.compare ?? null) === (defaults.compare ?? null)) return null;

  const baseSlug = abbreviateKey(meta.name ?? meta.key, maxTokenLength);

  if (meta.type === "Toggle") {
    const actualBool = actual.bool ?? actual.compare === "true";
    const defaultBool = defaults.bool ?? defaults.compare === "true";
    if (actualBool === defaultBool) return null;
    if (actualBool === true && defaultBool === false) return baseSlug;
    if (actualBool === false && defaultBool === true) return `${baseSlug}~`;
    const label = actual.label ?? (actualBool ? "On" : "Off");
    const valueSlug = abbreviateValue(label, Math.min(maxTokenLength, 4));
    return valueSlug ? `${baseSlug}${valueSlug}` : baseSlug;
  }

  const label = actual.label ?? actual.compare ?? "custom";
  const valueSlug = abbreviateValue(
    label,
    Math.max(2, Math.min(6, maxTokenLength)),
  );
  if (!valueSlug) return baseSlug;
  return `${baseSlug}${valueSlug}`;
}

export function buildOptionSummaryTokens(
  params: OptionSummaryParams,
): OptionSummaryResult {
  const { metadata, gameId, options } = params;
  const maxTokens = params.maxTokens ?? 6;
  const maxTokenLength = params.maxTokenLength ?? 12;

  if (!options || Object.keys(options).length === 0) {
    return { tokens: [], total: 0, derivedFromMetadata: false };
  }

  const orderedTokens: string[] = [];
  const seen = new Set<string>();
  let derivedFromMetadata = false;

  for (const [key, value] of Object.entries(options)) {
    const meta = getSettingMetadata(metadata, gameId, key);
    if (!meta) continue;
    if ((meta as { optionsFor?: string }).optionsFor) continue;
    derivedFromMetadata = true;
    const actual = canonicalizeSettingValue(meta, value);
    const defaults = canonicalizeSettingValue(meta, getDefaultForSetting(meta));
    const token = buildToken(meta, actual, defaults, maxTokenLength);
    if (!token) continue;
    if (!seen.has(token)) {
      seen.add(token);
      orderedTokens.push(token);
    }
  }

  const total = orderedTokens.length;
  const limited = orderedTokens.slice(0, maxTokens);
  return { tokens: limited, total, derivedFromMetadata };
}
