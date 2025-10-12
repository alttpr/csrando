import type { PageLoad } from "./$types";
import type { GraphData, LogicGameId } from "$lib/server/logic-viewer/types";

export const load: PageLoad = async ({ fetch, url }) => {
  const defaultGame: LogicGameId = "metroid";
  const requestedGame = (url.searchParams.get("game") ??
    defaultGame) as LogicGameId;
  const requestedLevel = url.searchParams.get("level");

  let initialData: GraphData | null = null;

  try {
    const query = requestedLevel
      ? `?level=${encodeURIComponent(requestedLevel)}`
      : "";
    const response = await fetch(`/api/logic-viewer/${requestedGame}${query}`);
    if (response.ok) {
      initialData = (await response.json()) as GraphData;
    }
  } catch (error) {
    console.error("Failed to load initial logic graph", error);
  }

  return {
    initialGame: requestedGame,
    initialLevel: requestedLevel,
    initialData,
  };
};
