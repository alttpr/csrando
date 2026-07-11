import { describe, it, expect } from "vitest";
import { RandomizerResponseSchema } from "../backend";

describe("RandomizerResponseSchema", () => {
  it("accepts a valid payload", () => {
    const data = {
      seed: 1234,
      worlds: {
        w1: { ipsPatch: "abcd==" },
        w2: { bpsPatch: "efgh==" },
      },
    };
    const parsed = RandomizerResponseSchema.safeParse(data);
    expect(parsed.success).toBe(true);
  });

  it("rejects invalid worlds shape", () => {
    const data = {
      seed: 1234,
      worlds: [{ ipsPatch: "abcd==" }],
    } as unknown;
    const parsed = RandomizerResponseSchema.safeParse(data);
    expect(parsed.success).toBe(false);
  });
});

describe("RandomizeRequestSchema", () => {
  it("requires callers to specify whether to include the spoiler", async () => {
    const { RandomizeRequestSchema } = await import("../backend");
    const parsed = RandomizeRequestSchema.safeParse({
      Seed: 0,
      Configs: [{}],
    });

    expect(parsed.success).toBe(false);
  });
});
