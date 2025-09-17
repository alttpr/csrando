import { describe, it, expect } from "vitest";
import { GameSpriteConfigSchema } from "../sprites";

describe("GameSpriteConfigSchema", () => {
  it("accepts a valid config", () => {
    const valid = {
      defaultSpriteValue: "default",
      sprites: [
        {
          value: "default",
          name: "Default",
          imagePath: "default.png",
          kind: "rdc/link",
          rdcPath: "default.rdc",
        },
        {
          value: "legacy",
          name: "Legacy",
          imagePath: "legacy.png",
          patchDetails: {
            files: [{ id: "gfx", path: "link.gfx.bin" }],
            patches: [
              { fileId: "gfx", targetAddress: "0x508000", dataLength: 16 },
            ],
          },
        },
      ],
    };
    const parsed = GameSpriteConfigSchema.safeParse(valid);
    expect(parsed.success).toBe(true);
  });

  it("rejects invalid kind", () => {
    const invalid = {
      sprites: [{ value: "x", name: "X", imagePath: "x.png", kind: "unknown" }],
    };
    const parsed = GameSpriteConfigSchema.safeParse(invalid);
    expect(parsed.success).toBe(false);
  });

  it("rejects missing required sprite fields", () => {
    const invalid = {
      sprites: [{ name: "NoValue", imagePath: "x.png" }],
    } as unknown;
    const parsed = GameSpriteConfigSchema.safeParse(invalid);
    expect(parsed.success).toBe(false);
  });
});
