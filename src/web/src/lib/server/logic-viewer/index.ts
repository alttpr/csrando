import type { GraphData, LogicGameId } from "./types";
import { loadMetroidGraph } from "./metroid";
import { loadZeldaGraph } from "./zelda1";

export interface LoadLogicGraphOptions {
  levelId?: string | null;
}

export async function loadLogicGraph(
  game: LogicGameId,
  options: LoadLogicGraphOptions = {},
): Promise<GraphData> {
  switch (game) {
    case "metroid":
      return loadMetroidGraph();
    case "zelda1":
      return loadZeldaGraph({ levelId: options.levelId });
    default:
      throw new Error(`Unsupported game: ${game satisfies never}`);
  }
}
