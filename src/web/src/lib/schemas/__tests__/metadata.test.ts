import { describe, it, expect } from "vitest";
import { MetadataSchema } from "../metadata";

describe("MetadataSchema", () => {
  it("accepts minimal valid metadata", () => {
    const data = {
      settings: [
        {
          type: "Toggle",
          key: "HardMode",
          name: "Hard Mode",
          // description intentionally omitted to verify optional
        },
      ],
      gameSettings: {
        alttp: {
          game: { name: "ALTTP" },
          settings: [
            {
              type: "SingleChoice",
              key: "Difficulty",
              name: "Difficulty",
              values: { Easy: "easy", Normal: "normal" },
              default: "normal",
            },
          ],
        },
      },
    };
    const parsed = MetadataSchema.safeParse(data);
    expect(parsed.success).toBe(true);
  });

  it("rejects invalid slider (missing to)", () => {
    const data = {
      settings: [
        {
          type: "Slider",
          key: "Volume",
          name: "Volume",
          description: "Master volume",
          range: { from: 0 },
          default: 10,
        },
      ],
      gameSettings: {},
    } as unknown;
    const parsed = MetadataSchema.safeParse(data);
    expect(parsed.success).toBe(false);
  });

  it("accepts metadata with only gameSettings (no global settings)", () => {
    const data = {
      gameSettings: {
        alttp: {
          settings: [
            {
              type: "Toggle",
              key: "SomeFlag",
              name: "Some Flag",
            },
          ],
        },
      },
    };
    const parsed = MetadataSchema.safeParse(data);
    expect(parsed.success).toBe(true);
    if (parsed.success) {
      expect(parsed.data.settings.length).toBe(0);
    }
  });

  it("accepts metadata with neither settings nor gameSettings (both default)", () => {
    const data = {};
    const parsed = MetadataSchema.safeParse(data);
    expect(parsed.success).toBe(true);
    if (parsed.success) {
      expect(parsed.data.settings.length).toBe(0);
      expect(Object.keys(parsed.data.gameSettings).length).toBe(0);
    }
  });
});
