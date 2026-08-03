import { describe, expect, it } from "vitest";
import {
  COMBO_POSTGEN_GAME_ID,
  clampNumberSetting,
  numberToLittleEndianBytes,
} from "$lib/postgen";
import { GamePostGenConfigSchema } from "$lib/schemas/postgen";
import type { NumberPostGenSetting } from "$lib/types";

const volume = (
  overrides: Partial<NumberPostGenSetting> = {},
): Pick<NumberPostGenSetting, "default" | "min" | "max"> => ({
  default: 0x7f,
  min: 0,
  max: 255,
  ...overrides,
});

describe("clampNumberSetting", () => {
  it("keeps in-range numbers", () => {
    expect(clampNumberSetting(volume(), 0)).toBe(0);
    expect(clampNumberSetting(volume(), 200)).toBe(200);
    expect(clampNumberSetting(volume(), 255)).toBe(255);
  });

  it("clamps out-of-range numbers to the declared bounds", () => {
    expect(clampNumberSetting(volume(), -20)).toBe(0);
    expect(clampNumberSetting(volume(), 999)).toBe(255);
    expect(clampNumberSetting(volume({ min: 10, max: 20 }), 5)).toBe(10);
    expect(clampNumberSetting(volume({ min: 10, max: 20 }), 50)).toBe(20);
  });

  it("rounds fractional values", () => {
    expect(clampNumberSetting(volume(), 12.4)).toBe(12);
    expect(clampNumberSetting(volume(), 12.6)).toBe(13);
  });

  it("parses numeric strings, as persisted selections may hold them", () => {
    expect(clampNumberSetting(volume(), "42")).toBe(42);
    expect(clampNumberSetting(volume(), "300")).toBe(255);
  });

  it("falls back to the default for missing or unusable values", () => {
    expect(clampNumberSetting(volume(), undefined)).toBe(0x7f);
    expect(clampNumberSetting(volume(), "")).toBe(0x7f);
    expect(clampNumberSetting(volume(), "loud")).toBe(0x7f);
    expect(clampNumberSetting(volume(), true)).toBe(0x7f);
    expect(clampNumberSetting(volume(), Number.NaN)).toBe(0x7f);
  });
});

describe("numberToLittleEndianBytes", () => {
  it("writes a single byte by default", () => {
    expect(Array.from(numberToLittleEndianBytes(0x7f, 1))).toEqual([0x7f]);
    expect(Array.from(numberToLittleEndianBytes(255, 1))).toEqual([0xff]);
  });

  it("writes wider values low byte first", () => {
    expect(Array.from(numberToLittleEndianBytes(0x1234, 2))).toEqual([
      0x34, 0x12,
    ]);
    expect(Array.from(numberToLittleEndianBytes(1, 3))).toEqual([1, 0, 0]);
  });

  it("truncates to the requested width instead of overflowing", () => {
    expect(Array.from(numberToLittleEndianBytes(0x1ff, 1))).toEqual([0xff]);
  });

  it("never emits negative bytes", () => {
    expect(Array.from(numberToLittleEndianBytes(-5, 1))).toEqual([0x00]);
  });
});

describe("number post-gen settings schema", () => {
  it("parses a backend-shaped combo volume option", () => {
    const parsed = GamePostGenConfigSchema.parse({
      options: [
        {
          type: "number",
          id: "msu_volume",
          name: "MSU-1 Volume",
          description: "Maximum volume for MSU-1 PCM music.",
          default: 127,
          min: 0,
          max: 255,
          step: 1,
          patches: [{ targetAddress: 0x7fff06, length: 1 }],
        },
      ],
    });

    const option = parsed.options[0];
    expect(option.type).toBe("number");
    if (option.type !== "number") throw new Error("expected number option");
    expect(option.default).toBe(127);
    expect(option.patches).toEqual([{ targetAddress: 0x7fff06, length: 1 }]);
  });

  it("defaults the patch width to one byte", () => {
    const parsed = GamePostGenConfigSchema.parse({
      options: [
        {
          type: "number",
          id: "msu_volume",
          name: "MSU-1 Volume",
          patches: [{ targetAddress: "7FFF06" }],
        },
      ],
    });

    const option = parsed.options[0];
    if (option.type !== "number") throw new Error("expected number option");
    expect(option.patches[0]).toEqual({ targetAddress: 0x7fff06, length: 1 });
    expect(option.min).toBe(0);
    expect(option.max).toBe(255);
  });

  it("still accepts toggle and select options alongside numbers", () => {
    const parsed = GamePostGenConfigSchema.parse({
      options: [
        {
          type: "toggle",
          id: "quick-swap",
          name: "Quick Swap",
          default: true,
          on: { patches: [{ targetAddress: 0x18004b, data: [1] }] },
          off: { patches: [{ targetAddress: 0x18004b, data: [0] }] },
        },
        {
          type: "select",
          id: "heart-color",
          name: "Heart Color",
          default: "red",
          choices: [
            {
              value: "red",
              label: "Red",
              patches: [{ targetAddress: 0x187020, data: [0] }],
            },
          ],
        },
      ],
    });

    expect(parsed.options.map((option) => option.type)).toEqual([
      "toggle",
      "select",
    ]);
  });
});

describe("combo-wide post-gen group", () => {
  it("uses the hidden combo game id the backend keys these options under", () => {
    expect(COMBO_POSTGEN_GAME_ID).toBe("combo");
  });
});
