import type {
  GamePostGenConfig,
  PostGenPatchEntry,
  PostGenSelections,
  SelectPostGenSetting,
  TogglePostGenSetting,
} from "$lib/types";

const CONTROLLER_OPTION_PREFIX = "controller_";

const BUTTONS = [
  { value: "a", label: "A", mask: 0x0080 },
  { value: "b", label: "B", mask: 0x8000 },
  { value: "x", label: "X", mask: 0x0040 },
  { value: "y", label: "Y", mask: 0x4000 },
  { value: "select", label: "Select", mask: 0x2000 },
  { value: "l", label: "L", mask: 0x0020 },
  { value: "r", label: "R", mask: 0x0010 },
] as const;

interface ControllerAction {
  id: string;
  name: string;
  defaultButton: (typeof BUTTONS)[number]["value"];
  targetAddress: number;
}

// Combo ROM PC offsets for the initial SRAM images at $F9:2400 and $F9:9020.
const M1_INITIAL_CONTROLS_PC = 0x792400;
const SM_INITIAL_CONTROLS_PC = 0x799020;
const SM_INITIAL_MOONWALK_PC = 0x799052;

const CONTROLLER_ACTIONS: Record<string, ControllerAction[]> = {
  metroid: [
    {
      id: "shoot",
      name: "Shoot",
      defaultButton: "y",
      targetAddress: M1_INITIAL_CONTROLS_PC,
    },
    {
      id: "jump",
      name: "Jump",
      defaultButton: "b",
      targetAddress: M1_INITIAL_CONTROLS_PC + 0x02,
    },
    {
      id: "item_select",
      name: "Item Select",
      defaultButton: "select",
      targetAddress: M1_INITIAL_CONTROLS_PC + 0x04,
    },
    {
      id: "map",
      name: "Map",
      defaultButton: "x",
      targetAddress: M1_INITIAL_CONTROLS_PC + 0x06,
    },
  ],
  supermetroid: [
    {
      id: "shot",
      name: "Shot",
      defaultButton: "x",
      targetAddress: SM_INITIAL_CONTROLS_PC,
    },
    {
      id: "jump",
      name: "Jump",
      defaultButton: "a",
      targetAddress: SM_INITIAL_CONTROLS_PC + 0x02,
    },
    {
      id: "dash",
      name: "Dash",
      defaultButton: "b",
      targetAddress: SM_INITIAL_CONTROLS_PC + 0x04,
    },
    {
      id: "item_cancel",
      name: "Item Cancel",
      defaultButton: "y",
      targetAddress: SM_INITIAL_CONTROLS_PC + 0x06,
    },
    {
      id: "item_select",
      name: "Item Select",
      defaultButton: "select",
      targetAddress: SM_INITIAL_CONTROLS_PC + 0x08,
    },
    {
      id: "angle_down",
      name: "Angle Down",
      defaultButton: "l",
      targetAddress: SM_INITIAL_CONTROLS_PC + 0x0a,
    },
    {
      id: "angle_up",
      name: "Angle Up",
      defaultButton: "r",
      targetAddress: SM_INITIAL_CONTROLS_PC + 0x0c,
    },
  ],
};

function littleEndianWord(value: number): number[] {
  return [value & 0xff, (value >>> 8) & 0xff];
}

function controllerOption(action: ControllerAction): SelectPostGenSetting {
  return {
    id: `${CONTROLLER_OPTION_PREFIX}${action.id}`,
    name: action.name,
    type: "select",
    default: action.defaultButton,
    choices: BUTTONS.map((button) => {
      const patch: PostGenPatchEntry = {
        targetAddress: action.targetAddress,
        data: littleEndianWord(button.mask),
      };
      return {
        value: button.value,
        label: button.label,
        patches: [patch],
      };
    }),
  };
}

function moonwalkOption(): TogglePostGenSetting {
  return {
    id: "moonwalk",
    name: "Moonwalk",
    description: "Enable Super Metroid's moonwalk setting.",
    type: "toggle",
    default: false,
    on: {
      patches: [
        {
          targetAddress: SM_INITIAL_MOONWALK_PC,
          data: littleEndianWord(1),
        },
      ],
    },
    off: {
      patches: [
        {
          targetAddress: SM_INITIAL_MOONWALK_PC,
          data: littleEndianWord(0),
        },
      ],
    },
  };
}

export function withControllerMappings(
  gameId: string,
  config: GamePostGenConfig,
): GamePostGenConfig {
  const actions = CONTROLLER_ACTIONS[gameId];
  if (!actions) return config;

  const existingIds = new Set(config.options.map((option) => option.id));
  const controllerOptions = actions
    .map(controllerOption)
    .filter((option) => !existingIds.has(option.id));
  const extraOptions =
    gameId === "supermetroid" && !existingIds.has("moonwalk")
      ? [moonwalkOption()]
      : [];

  return {
    options: [...config.options, ...controllerOptions, ...extraOptions],
  };
}

export function isControllerMappingOption(
  option: SelectPostGenSetting,
): boolean {
  return option.id.startsWith(CONTROLLER_OPTION_PREFIX);
}

function controllerOptions(config: GamePostGenConfig): SelectPostGenSetting[] {
  return config.options.filter(
    (option): option is SelectPostGenSetting =>
      option.type === "select" && isControllerMappingOption(option),
  );
}

function selectedButton(
  option: SelectPostGenSetting,
  selections: PostGenSelections,
): string | undefined {
  const selected = selections[option.id];
  return typeof selected === "string" ? selected : option.default;
}

export function assignControllerButton(
  config: GamePostGenConfig,
  selections: PostGenSelections,
  optionId: string,
  button: string,
): PostGenSelections | null {
  const options = controllerOptions(config);
  const target = options.find((option) => option.id === optionId);
  if (!target) return null;
  if (!target.choices.some((choice) => choice.value === button)) return null;

  const previousButton = selectedButton(target, selections);
  const previousOwner = options.find(
    (option) =>
      option.id !== optionId && selectedButton(option, selections) === button,
  );
  const updated = { ...selections, [optionId]: button };

  if (previousOwner && previousButton !== undefined) {
    updated[previousOwner.id] = previousButton;
  }

  return updated;
}

export function restoreControllerDefaults(
  config: GamePostGenConfig,
  selections: PostGenSelections,
): PostGenSelections {
  const restored = { ...selections };
  for (const option of controllerOptions(config)) {
    if (option.default !== undefined) restored[option.id] = option.default;
  }
  return restored;
}

export function normalizeControllerMappings(
  config: GamePostGenConfig,
  selections: PostGenSelections,
): PostGenSelections {
  const options = controllerOptions(config);
  const usedButtons = new Set<string>();

  for (const option of options) {
    const button = selectedButton(option, selections);
    const isValid =
      button !== undefined &&
      option.choices.some((choice) => choice.value === button);
    if (!isValid || usedButtons.has(button)) {
      return restoreControllerDefaults(config, selections);
    }
    usedButtons.add(button);
  }

  return selections;
}
