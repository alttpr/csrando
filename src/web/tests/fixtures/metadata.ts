import { parseMetadata } from "$lib/schemas/metadata";
import type { Metadata } from "$lib/types";
import {
  initializeFormValues,
  type FormStateSnapshot,
} from "$lib/config/normalize";

// Shared generator-metadata fixture used by normalization and profiles tests.
export const rawMetadataFixture = {
  settings: [
    {
      key: "Game",
      name: "Game",
      type: "SingleChoice",
      values: { Combo: "Combo", Alttpr: "Alttpr", Random: null },
      default: "Combo",
    },
    {
      key: "Language",
      name: "Language",
      type: "SingleChoice",
      values: { en: "en", de: "de" },
      default: "en",
    },
  ],
  gameSettings: {
    Alttpr: {
      game: { name: "ALttP Randomizer" },
      settings: [
        {
          key: "Swords",
          name: "Swords",
          type: "SingleChoice",
          values: {
            Randomized: "Randomized",
            Assured: "Assured",
            Random: null,
          },
          default: "Randomized",
        },
        {
          key: "Glitches",
          name: "Glitches",
          type: "MultipleChoice",
          values: {
            None: "None",
            OverworldGlitches: "OverworldGlitches",
            Major: "Major",
          },
        },
        {
          key: "Crystals",
          name: "Crystals",
          type: "Slider",
          range: { from: 0, to: 7 },
          default: 7,
        },
        {
          key: "KeyShuffle",
          name: "Key Shuffle",
          type: "Toggle",
          default: false,
        },
        { key: "Notes", name: "Notes", type: "Input", default: "" },
        { key: "Extra", name: "Extra", type: "Generic" },
      ],
    },
    Sm: {
      game: { name: "Super Metroid" },
      settings: [
        {
          key: "BossTokenCount",
          name: "Boss Tokens",
          type: "Slider",
          range: { from: 1, to: 4 },
          optionsFor: "BossTokens",
        },
        {
          key: "Logic",
          name: "Logic",
          type: "SingleChoice",
          values: { Casual: "Casual", Tournament: "Tournament" },
          default: "Casual",
        },
      ],
    },
    Combo: {
      settings: [
        {
          key: "PortalMode",
          name: "Portal Mode",
          type: "SingleChoice",
          values: { Full: "Full", Basic: "Basic" },
          default: "Full",
        },
      ],
    },
    EmptyGame: { settings: [] },
  },
};

export function loadMetadataFixture(): Metadata {
  const parsed = parseMetadata(rawMetadataFixture);
  if (!parsed.success) throw new Error("fixture metadata failed to parse");
  return parsed.data;
}

export function defaultFormStateFixture(metadata: Metadata): FormStateSnapshot {
  const init = initializeFormValues(metadata);
  return {
    selectedGames: [...init.selectedGames],
    global: { ...init.formGlobal },
    perGame: Object.fromEntries(
      Object.entries(init.formPerGame).map(([k, v]) => [k, { ...v }]),
    ),
  };
}
