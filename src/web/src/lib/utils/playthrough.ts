export interface PlaythroughItem {
  name: string;
  game: string;
  world: number;
  count: number;
  meta?: boolean;
  initial?: boolean;
  simple?: boolean;
}

export interface PlaythroughResource {
  name: string;
  amount: number;
}

export interface PlaythroughVertex {
  name: string;
  game: string;
  world: number;
  type: string;
}

export interface PlaythroughPathStep {
  from: PlaythroughVertex;
  to: PlaythroughVertex;
  requirement?: PlaythroughItem | null;
  requirements?: PlaythroughItem[];
  resourcesSpent?: PlaythroughResource[];
  game?: string;
  crossGame?: boolean;
  strategy?: string | null;
}

export interface PlaythroughPickup {
  location: PlaythroughVertex;
  item: PlaythroughItem;
  requiredItems: PlaythroughItem[];
  path: PlaythroughPathStep[];
  meta?: boolean;
  required?: boolean;
}

export interface PlaythroughSphere {
  number: number;
  pickups: PlaythroughPickup[];
}

export interface PlaythroughData {
  complete: boolean;
  victoryItems: string[];
  startingItems: PlaythroughItem[];
  spheres: PlaythroughSphere[];
  warnings: string[];
}

/** Shows only pickups used to win in simple mode, retaining victory flags. */
export const visiblePlaythroughPickups = (
  playthrough: PlaythroughData,
  pickups: PlaythroughPickup[],
  includeMeta = false,
): PlaythroughPickup[] =>
  includeMeta
    ? pickups
    : pickups.filter(
        (pickup) =>
          pickup.required !== false &&
          (pickup.meta !== true ||
            playthrough.victoryItems.includes(pickup.item.name)),
      );

/** Keeps only route edges where logic consumed an item or resource, plus game travel. */
export const condensedPlaythroughPath = (
  path: PlaythroughPathStep[],
  includeMeta = false,
): PlaythroughPathStep[] =>
  path.filter((step) => {
    const requirements = step.requirements?.length
      ? step.requirements
      : step.requirement
        ? [step.requirement]
        : [];
    const crossGame =
      step.crossGame === true || step.from.game !== step.to.game;
    const spendsResources = (step.resourcesSpent?.length ?? 0) > 0;
    const usesVisibleItem = includeMeta
      ? requirements.length > 0
      : requirements.some((item) => item.simple ?? item.initial !== true);
    return usesVisibleItem || crossGame || spendsResources;
  });

const object = (value: unknown): value is Record<string, unknown> =>
  !!value && typeof value === "object" && !Array.isArray(value);

const item = (value: unknown): value is PlaythroughItem =>
  object(value) &&
  typeof value.name === "string" &&
  typeof value.game === "string" &&
  typeof value.world === "number" &&
  typeof value.count === "number" &&
  (value.meta == null || typeof value.meta === "boolean") &&
  (value.initial == null || typeof value.initial === "boolean") &&
  (value.simple == null || typeof value.simple === "boolean");

const resource = (value: unknown): value is PlaythroughResource =>
  object(value) &&
  typeof value.name === "string" &&
  typeof value.amount === "number";

const vertex = (value: unknown): value is PlaythroughVertex =>
  object(value) &&
  typeof value.name === "string" &&
  typeof value.game === "string" &&
  typeof value.world === "number" &&
  typeof value.type === "string";

const pathStep = (value: unknown): value is PlaythroughPathStep =>
  object(value) &&
  vertex(value.from) &&
  vertex(value.to) &&
  (value.requirement == null || item(value.requirement)) &&
  (value.requirements == null ||
    (Array.isArray(value.requirements) && value.requirements.every(item))) &&
  (value.resourcesSpent == null ||
    (Array.isArray(value.resourcesSpent) &&
      value.resourcesSpent.every(resource))) &&
  (value.game == null || typeof value.game === "string") &&
  (value.crossGame == null || typeof value.crossGame === "boolean") &&
  (value.strategy == null || typeof value.strategy === "string");

const pickup = (value: unknown): value is PlaythroughPickup =>
  object(value) &&
  vertex(value.location) &&
  item(value.item) &&
  Array.isArray(value.requiredItems) &&
  value.requiredItems.every(item) &&
  Array.isArray(value.path) &&
  value.path.every(pathStep) &&
  (value.meta == null || typeof value.meta === "boolean") &&
  (value.required == null || typeof value.required === "boolean");

export function parsePlaythrough(
  value: string | undefined,
): PlaythroughData | null {
  if (!value) return null;
  try {
    const parsed: unknown = JSON.parse(value);
    if (
      !object(parsed) ||
      typeof parsed.complete !== "boolean" ||
      !Array.isArray(parsed.victoryItems) ||
      !parsed.victoryItems.every((goal) => typeof goal === "string") ||
      !Array.isArray(parsed.startingItems) ||
      !parsed.startingItems.every(item) ||
      !Array.isArray(parsed.spheres) ||
      !parsed.spheres.every(
        (sphere) =>
          object(sphere) &&
          typeof sphere.number === "number" &&
          Array.isArray(sphere.pickups) &&
          sphere.pickups.every(pickup),
      ) ||
      !Array.isArray(parsed.warnings) ||
      !parsed.warnings.every((warning) => typeof warning === "string")
    ) {
      return null;
    }
    return parsed as unknown as PlaythroughData;
  } catch {
    return null;
  }
}
