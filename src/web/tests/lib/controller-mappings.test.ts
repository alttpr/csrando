import { describe, expect, it } from "vitest";
import type { GamePostGenConfig, SelectPostGenSetting } from "$lib/types";
import {
  assignControllerButton,
  isControllerMappingOption,
  normalizeControllerMappings,
  restoreControllerDefaults,
  withControllerMappings,
} from "$lib/controller-mappings";

const emptyConfig = (): GamePostGenConfig => ({ options: [] });

function controllerOptions(gameId: string): SelectPostGenSetting[] {
  return withControllerMappings(gameId, emptyConfig()).options.filter(
    (option): option is SelectPostGenSetting => option.type === "select",
  );
}

function selectedPatch(
  option: SelectPostGenSetting,
  value: string,
): { targetAddress: number; data: number[] } {
  const choice = option.choices.find((entry) => entry.value === value);
  expect(choice).toBeDefined();
  const patch = choice!.patches[0];
  expect(Array.isArray(patch.data)).toBe(true);
  return {
    targetAddress: patch.targetAddress,
    data: patch.data as number[],
  };
}

describe("controller mapping post-generation settings", () => {
  it("defines standard M1 controls at the initial SRAM record", () => {
    const options = controllerOptions("metroid");

    expect(options.map((option) => [option.id, option.default])).toEqual([
      ["controller_shoot", "b"],
      ["controller_jump", "y"],
      ["controller_item_select", "select"],
      ["controller_map", "x"],
    ]);
    expect(selectedPatch(options[0], "b")).toEqual({
      targetAddress: 0x792400,
      data: [0x00, 0x80],
    });
    expect(selectedPatch(options[1], "y")).toEqual({
      targetAddress: 0x792402,
      data: [0x00, 0x40],
    });
    expect(selectedPatch(options[3], "x")).toEqual({
      targetAddress: 0x792406,
      data: [0x40, 0x00],
    });
  });

  it("defines standard SM controls in native SRAM order", () => {
    const options = controllerOptions("supermetroid");

    expect(options.map((option) => [option.id, option.default])).toEqual([
      ["controller_shot", "x"],
      ["controller_jump", "a"],
      ["controller_dash", "b"],
      ["controller_item_cancel", "y"],
      ["controller_item_select", "select"],
      ["controller_angle_down", "l"],
      ["controller_angle_up", "r"],
    ]);
    expect(selectedPatch(options[0], "x")).toEqual({
      targetAddress: 0x799020,
      data: [0x40, 0x00],
    });
    expect(selectedPatch(options[5], "l")).toEqual({
      targetAddress: 0x79902a,
      data: [0x20, 0x00],
    });
    expect(selectedPatch(options[6], "r")).toEqual({
      targetAddress: 0x79902c,
      data: [0x10, 0x00],
    });
  });

  it("defines an off-by-default SM moonwalk flag in initial SRAM", () => {
    const config = withControllerMappings("supermetroid", emptyConfig());
    const moonwalk = config.options.find((option) => option.id === "moonwalk");

    expect(moonwalk).toMatchObject({
      name: "Moonwalk",
      type: "toggle",
      default: false,
      on: {
        patches: [{ targetAddress: 0x799052, data: [0x01, 0x00] }],
      },
      off: {
        patches: [{ targetAddress: 0x799052, data: [0x00, 0x00] }],
      },
    });
  });

  it("leaves unsupported games unchanged and does not duplicate options", () => {
    const base = emptyConfig();
    expect(withControllerMappings("alttp", base)).toBe(base);

    const once = withControllerMappings("metroid", base);
    const twice = withControllerMappings("metroid", once);
    expect(twice.options).toHaveLength(4);
    expect(
      twice.options.every(
        (option) =>
          option.type === "select" && isControllerMappingOption(option),
      ),
    ).toBe(true);

    const smOnce = withControllerMappings("supermetroid", base);
    const smTwice = withControllerMappings("supermetroid", smOnce);
    expect(
      smTwice.options.filter((option) => option.id === "moonwalk"),
    ).toHaveLength(1);
  });

  it("swaps occupied buttons without creating duplicates", () => {
    const config = withControllerMappings("metroid", emptyConfig());
    const custom = {
      unrelated_setting: true,
      controller_shoot: "a",
      controller_jump: "b",
      controller_item_select: "select",
      controller_map: "x",
    };

    expect(
      assignControllerButton(config, custom, "controller_jump", "a"),
    ).toEqual({
      ...custom,
      controller_shoot: "b",
      controller_jump: "a",
    });
    expect(
      assignControllerButton(config, custom, "controller_jump", "y"),
    ).toEqual({
      ...custom,
      controller_jump: "y",
    });
  });

  it("can atomically restore defaults", () => {
    const config = withControllerMappings("metroid", emptyConfig());
    const custom = {
      unrelated_setting: true,
      controller_shoot: "a",
      controller_jump: "b",
      controller_item_select: "select",
      controller_map: "x",
    };

    expect(restoreControllerDefaults(config, custom)).toEqual({
      unrelated_setting: true,
      controller_shoot: "b",
      controller_jump: "y",
      controller_item_select: "select",
      controller_map: "x",
    });
  });

  it("repairs persisted duplicate mappings by restoring standard controls", () => {
    const config = withControllerMappings("supermetroid", emptyConfig());
    const invalid = {
      controller_shot: "a",
      controller_jump: "a",
      unrelated_setting: "kept",
    };

    expect(normalizeControllerMappings(config, invalid)).toMatchObject({
      controller_shot: "x",
      controller_jump: "a",
      controller_dash: "b",
      controller_item_cancel: "y",
      controller_item_select: "select",
      controller_angle_down: "l",
      controller_angle_up: "r",
      unrelated_setting: "kept",
    });
  });
});
