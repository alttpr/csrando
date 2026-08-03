import type { NumberPostGenSetting, PostGenSelectionValue } from "$lib/types";

/**
 * Pseudo game id used for post-generation settings that apply to the combo ROM as a whole
 * rather than to one of the bundled games (for example the shared MSU-1 volume). The backend
 * emits these under the same key; `combo` is hidden from the per-game ROM/sprite UI, so the
 * seed page renders it as an extra group and the patcher applies it to the combined ROM.
 */
export const COMBO_POSTGEN_GAME_ID = "combo";

/** Label for the combo-wide post-generation group on the seed page. */
export const COMBO_POSTGEN_GROUP_NAME = "All Games";

/**
 * Coerce a persisted or user-entered value into an integer inside the option's range.
 * Falls back to the option default (then its minimum) when the value is not a number.
 */
export function clampNumberSetting(
  option: Pick<NumberPostGenSetting, "default" | "min" | "max">,
  value: PostGenSelectionValue | undefined,
): number {
  const min = Number.isFinite(option.min) ? option.min : 0;
  const max = Number.isFinite(option.max) ? option.max : 0xff;

  let candidate: number;
  if (typeof value === "number" && Number.isFinite(value)) {
    candidate = value;
  } else if (typeof value === "string" && value.trim() !== "") {
    const parsed = Number(value);
    candidate = Number.isFinite(parsed) ? parsed : option.default;
  } else {
    candidate = option.default;
  }

  if (!Number.isFinite(candidate)) candidate = min;
  return Math.min(max, Math.max(min, Math.round(candidate)));
}

/** Split a value into `length` little-endian bytes, as the ROM configuration block stores it. */
export function numberToLittleEndianBytes(
  value: number,
  length: number,
): Uint8Array {
  const size = Number.isFinite(length) && length > 0 ? Math.trunc(length) : 1;
  const bytes = new Uint8Array(size);
  let remaining = Math.max(0, Math.trunc(value));
  for (let i = 0; i < size; i += 1) {
    bytes[i] = remaining & 0xff;
    remaining = Math.floor(remaining / 256);
  }
  return bytes;
}
