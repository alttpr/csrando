import { json, error } from "@sveltejs/kit";

import { loadLogicGraph } from "$lib/server/logic-viewer";
import type { LogicGameId } from "$lib/server/logic-viewer/types";

export const GET = async ({ params, url }) => {
  const game = params.game?.toLowerCase();

  if (game !== "metroid" && game !== "zelda1") {
    throw error(400, "Unsupported game identifier");
  }

  try {
    const levelId = url.searchParams.get("level");
    const data = await loadLogicGraph(game as LogicGameId, { levelId });
    return json(data);
  } catch (err) {
    console.error("Failed to load logic graph", err);
    throw error(500, "Failed to load logic graph");
  }
};
