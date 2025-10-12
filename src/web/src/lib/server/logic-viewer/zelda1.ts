import { join, resolve } from "node:path";
import { readdir, readFile, stat } from "node:fs/promises";
import { parse } from "yaml";

import type {
  Coordinate,
  GraphData,
  GraphEdge,
  GraphLevelOption,
  GraphMetadata,
  GraphNode,
  LogicGameId,
  RoomInstance,
  ScreenInstance,
} from "./types";

type ZeldaArea = "Overworld" | "Underworld";
type Direction = "Up" | "Down" | "Left" | "Right";

interface ZeldaLevelYaml {
  name: string;
  level: number;
  area: ZeldaArea;
  rooms: number[];
}

interface ZeldaMapBaseYaml {
  name: string;
  area: ZeldaArea;
  map: number;
  screen: number;
}

type ZeldaOverworldMapYaml = ZeldaMapBaseYaml & {
  cave: number;
};

type ZeldaUnderworldMapYaml = ZeldaMapBaseYaml & {
  doors: number[];
  passage: boolean;
  passage_left: number;
  passage_right: number;
  room_item: number;
  item_pos: number;
  behaviour: number;
  level_nine_check?: boolean;
};

interface ZeldaScreenYaml {
  name: string;
  area: ZeldaArea;
  screen: number;
  nodes?: {
    exits?: ZeldaExitYaml[];
    caves?: ZeldaCaveYaml[];
    regions?: ZeldaRegionYaml[];
    meta?: ZeldaMetaYaml[];
  };
  edges?: {
    undirected?: Record<string, string[][]>;
    directed?: Record<string, string[][]>;
  };
}

interface ZeldaExitYaml {
  name: string;
  type: string;
  direction: Direction;
}

interface ZeldaCaveYaml {
  name: string;
  type: string;
}

interface ZeldaRegionYaml {
  name: string;
  type: string;
  from: [number, number];
  to: [number, number];
}

interface ZeldaMetaYaml {
  name: string;
  type: string;
  position?: string;
  item?: string;
}

interface ZeldaCaveYaml {
  name: string;
  cave: number;
  items: number[];
  flags: number[];
  prices: number[];
  text: number;
}

interface ZeldaSpecialYaml {
  overworld_item_room: number;
  overworld_item_x: number;
  overworld_item_id: number;
  armos_item_room: number;
  armos_item_x: number;
  armos_item_id: number;
  armos_stairs: number[];
  armos_x_pos: number[];
  step_ladder: number[];
  recorder_stairs: number[];
  recorder_dests: number[];
  recorder_y_pos: number[];
  any_road_x_pos: number[];
  start: number;
}

type ZeldaItemsYaml = Record<string, { byte: number }>;

interface NodeRegistryEntry extends GraphNode {
  metadata: Record<string, unknown>;
}

interface BuildContext {
  rooms: RoomInstance[];
  screens: ScreenInstance[];
  edges: GraphEdge[];
  nodeRegistry: Map<string, NodeRegistryEntry>;
  edgeIds: Set<string>;
}

interface PassageConnector {
  nodeId: string;
  label: string;
  connectorType: "stairs" | "region" | "exit";
  connectorName: string;
}

interface LevelResolution {
  id: string;
  level: ZeldaLevelYaml | null;
  area: ZeldaArea;
}

const DATA_ROOT = resolve(process.cwd(), "../Randomizer/Games/Zelda1/data");
const GRID_WIDTH = 16;
const REGION_GRID_WIDTH = 12;
const DEBUG_LOG = process.env.LOGIC_VIEWER_DEBUG === "1";

const DIRECTION_OFFSETS: Record<Direction, number> = {
  Up: -GRID_WIDTH,
  Down: GRID_WIDTH,
  Left: -1,
  Right: 1,
};

const OPPOSITE_DIRECTION: Record<Direction, Direction> = {
  Up: "Down",
  Down: "Up",
  Left: "Right",
  Right: "Left",
};

const DOOR_INDEX: Record<Direction, number> = {
  Up: 0,
  Down: 1,
  Left: 2,
  Right: 3,
};

enum DoorType {
  Open = 0,
  Wall = 1,
  PassThrough = 2,
  PassThroughNoSound = 3,
  Bombable = 4,
  Locked = 5,
  Locked2 = 6,
  Shutter = 7,
}

enum RoomBehaviour {
  None = 0,
  KillForItemShutter = 1,
  Leader = 2,
  GetTriforceShutter = 3,
  PushBlockShutter = 4,
  PushBlockStairs = 5,
  LeaderShutters = 6,
  KillForItemShutterBoss = 7,
}

export interface ZeldaGraphOptions {
  levelId?: string | null;
}

export async function loadZeldaGraph(
  options: ZeldaGraphOptions = {},
): Promise<GraphData> {
  const [
    levels,
    overworldMaps,
    underworldMaps,
    overworldScreens,
    underworldScreens,
    caves,
    special,
    items,
  ] = await Promise.all([
    loadYamlFiles<ZeldaLevelYaml>(join(DATA_ROOT, "Levels")),
    loadYamlFiles<ZeldaOverworldMapYaml>(join(DATA_ROOT, "Maps/Overworld")),
    loadYamlFiles<ZeldaUnderworldMapYaml>(join(DATA_ROOT, "Maps/Underworld")),
    loadYamlFiles<ZeldaScreenYaml>(join(DATA_ROOT, "Screens/Overworld")),
    loadYamlFiles<ZeldaScreenYaml>(join(DATA_ROOT, "Screens/Underworld")),
    loadYamlFile<ZeldaCaveYaml[]>(join(DATA_ROOT, "Caves.yml")),
    loadYamlFile<ZeldaSpecialYaml>(join(DATA_ROOT, "Special.yml")),
    loadYamlFile<ZeldaItemsYaml>(join(DATA_ROOT, "Items.yml")),
  ]);

  const levelOptions = buildLevelOptions(levels);
  const selectedLevelId = resolveLevelId(levelOptions, options.levelId);
  const selectedLevel = resolveLevel(levels, selectedLevelId);

  const screensByArea = new Map<ZeldaArea, Map<number, ZeldaScreenYaml>>([
    ["Overworld", indexScreens(overworldScreens)],
    ["Underworld", indexScreens(underworldScreens)],
  ]);

  const cavesById = new Map<number, ZeldaCaveYaml>();
  for (const cave of caves ?? []) {
    cavesById.set(cave.cave, cave);
  }

  const itemNamesByCode = new Map<number, string>();
  if (items) {
    for (const [name, payload] of Object.entries(items)) {
      itemNamesByCode.set(payload.byte, name);
    }
  }

  const underworldMapIndex = indexMaps(underworldMaps);

  const context: BuildContext = {
    rooms: [],
    screens: [],
    edges: [],
    nodeRegistry: new Map(),
    edgeIds: new Set(),
  };

  if (selectedLevel.area === "Overworld") {
    buildOverworldGraph(
      context,
      overworldMaps,
      screensByArea.get("Overworld")!,
      cavesById,
      special ?? null,
      itemNamesByCode,
    );
  } else {
    buildUnderworldGraph(
      context,
      underworldMaps,
      screensByArea.get("Underworld")!,
      selectedLevel,
      underworldMapIndex,
    );
  }

  const metadata: GraphMetadata = {
    levels: levelOptions,
    selectedLevelId,
  };

  return {
    game: "zelda1" satisfies LogicGameId,
    rooms: context.rooms,
    screens: context.screens,
    nodes: Array.from(context.nodeRegistry.values()),
    edges: context.edges,
    metadata,
  };
}

function buildOverworldGraph(
  context: BuildContext,
  maps: ZeldaOverworldMapYaml[],
  screens: Map<number, ZeldaScreenYaml>,
  cavesById: Map<number, ZeldaCaveYaml>,
  special: ZeldaSpecialYaml | null,
  itemNamesByCode: Map<number, string>,
) {
  const screenKeyByMap = new Map<number, string>();

  for (const map of maps) {
    const screen =
      screens.get(map.screen) ??
      (map.passage ? createSyntheticPassageScreen(map) : null);
    if (!screen) {
      if (DEBUG_LOG) {
        console.warn(
          `Zelda1 underworld screen 0x${map.screen.toString(16).toUpperCase()} missing for map 0x${map.map.toString(16).toUpperCase()}`,
        );
      }
      continue;
    }

    const mapIdHex = map.map.toString(16).toUpperCase().padStart(2, "0");
    const screenKey = `overworld::${mapIdHex}`;
    const roomId = `overworld::room::${mapIdHex}`;
    screenKeyByMap.set(map.map, screenKey);

    context.rooms.push({
      id: roomId,
      name: map.name,
      area: map.area,
      screenKeys: [screenKey],
    });

    context.screens.push({
      key: screenKey,
      name: `${map.area} · ${map.name}`,
      area: map.area,
      roomId,
      roomName: map.name,
      screenId: map.screen,
      screenLabel: `0x${map.screen.toString(16).toUpperCase()}`,
      coordinates: mapToCoordinate(map.map),
    });

    registerScreenNodes(context, screenKey, roomId, map.area, screen);
    registerScreenEdges(context, screenKey, roomId, map.area, screen);

    augmentOverworldSpecials(
      context,
      map,
      screen,
      screenKey,
      roomId,
      special,
      itemNamesByCode,
    );

    if (screen.nodes?.caves?.length) {
      const caveInfo = cavesById.get(map.cave);
      const caveItemNames =
        caveInfo?.items
          ?.map((code) => itemNamesByCode.get(code) ?? formatHex(code))
          .filter(Boolean) ?? [];

      for (const cave of screen.nodes.caves) {
        const caveNodeId = nodeId(screenKey, cave.name);
        const metadata: Record<string, unknown> = {
          caveType: cave.type,
        };

        if (map.cave > 0) {
          metadata.caveId = formatHex(map.cave);
        }

        if (map.cave > 0 && map.cave < 10) {
          metadata.linkedLevel = map.cave;
        }

        if (caveInfo) {
          metadata.caveName = caveInfo.name;
          if (caveItemNames.length) {
            metadata.caveItems = caveItemNames;
          }
          metadata.caveFlags = caveInfo.flags.map((value) => formatHex(value));
          const prices = caveInfo.prices.filter((value) => value > 0);
          if (prices.length) {
            metadata.cavePrices = prices;
          }
        }

        ensureNode(context, caveNodeId, {
          metadata,
        });
      }
    }
  }

  for (const map of maps) {
    const screenKey = screenKeyByMap.get(map.map);
    if (!screenKey) {
      continue;
    }

    const screen = screens.get(map.screen);
    if (!screen?.nodes?.exits?.length) {
      continue;
    }

    for (const exit of screen.nodes.exits) {
      const offset = DIRECTION_OFFSETS[exit.direction];
      if (offset === undefined) {
        continue;
      }

      const targetMap = maps.find(
        (candidate) => candidate.map === map.map + offset,
      );
      if (!targetMap) {
        continue;
      }

      const targetScreenKey = screenKeyByMap.get(targetMap.map);
      if (!targetScreenKey) {
        continue;
      }

      const targetScreen = screens.get(targetMap.screen);
      if (!targetScreen?.nodes?.exits) {
        continue;
      }

      const opposite = OPPOSITE_DIRECTION[exit.direction];
      const targets = targetScreen.nodes.exits.filter(
        (candidate) => candidate.direction === opposite,
      );

      for (const targetExit of targets) {
        const fromId = nodeId(screenKey, exit.name);
        const toId = nodeId(targetScreenKey, targetExit.name);

        ensureNode(context, fromId, {
          label: exit.name,
          nodeType: "exit",
          screenKey,
          area: map.area,
          roomId: `overworld::room::${map.map.toString(16).toUpperCase().padStart(2, "0")}`,
          metadata: {
            direction: exit.direction,
            exitType: exit.type,
          },
        });

        ensureNode(context, toId, {
          label: targetExit.name,
          nodeType: "exit",
          screenKey: targetScreenKey,
          area: targetMap.area,
          roomId: `overworld::room::${targetMap.map.toString(16).toUpperCase().padStart(2, "0")}`,
          metadata: {
            direction: targetExit.direction,
            exitType: targetExit.type,
          },
        });

        pushEdge(context, fromId, toId, "fixed", false);
      }
    }
  }
}

function augmentOverworldSpecials(
  context: BuildContext,
  map: ZeldaOverworldMapYaml,
  screen: ZeldaScreenYaml,
  screenKey: string,
  roomId: string,
  special: ZeldaSpecialYaml | null,
  itemNamesByCode: Map<number, string>,
) {
  if (!special) {
    return;
  }

  if (special.start === map.map) {
    const startNodeId = nodeId(screenKey, "Start Location");
    ensureNode(context, startNodeId, {
      label: "Start Location",
      nodeType: "meta",
      screenKey,
      area: map.area,
      roomId,
      metadata: {
        startPoint: true,
        overworldStart: true,
      },
    });
    pushMetadataListEntry(context, startNodeId, "specialCategories", "start");
  }

  if (special.overworld_item_room === map.map) {
    const itemMeta = screen.nodes?.meta?.find((entry) => entry.type === "Item");
    if (itemMeta) {
      const itemNodeId = nodeId(screenKey, itemMeta.name);
      const itemName =
        itemNamesByCode.get(special.overworld_item_id) ??
        formatHex(special.overworld_item_id);
      mergeNodeMetadata(context, itemNodeId, {
        item: itemName,
        overworldItem: true,
      });
      pushMetadataListEntry(
        context,
        itemNodeId,
        "specialCategories",
        "overworld-item",
      );
    }
  }

  if (special.armos_item_room === map.map) {
    const armosMeta = screen.nodes?.meta?.find(
      (entry) => entry.type === "Armos",
    );
    if (armosMeta) {
      const armosNodeId = nodeId(screenKey, armosMeta.name);
      const itemName =
        itemNamesByCode.get(special.armos_item_id) ??
        formatHex(special.armos_item_id);
      mergeNodeMetadata(context, armosNodeId, {
        armosItem: itemName,
      });
      pushMetadataListEntry(
        context,
        armosNodeId,
        "specialCategories",
        "armos-item",
      );

      const itemNodeId = nodeId(screenKey, `${armosMeta.name} - Item`);
      ensureNode(context, itemNodeId, {
        label: `${armosMeta.name} Item`,
        nodeType: "item",
        screenKey,
        area: map.area,
        roomId,
        metadata: {
          item: itemName,
          specialCategory: "armos-item",
        },
      });
      pushEdge(context, armosNodeId, itemNodeId, "fixed", true);
    }
  }

  if (special.armos_stairs?.includes(map.map)) {
    const armosMeta = screen.nodes?.meta?.find(
      (entry) => entry.type === "Armos",
    );
    if (!armosMeta) {
      return;
    }

    const armosNodeId = nodeId(screenKey, armosMeta.name);
    const destinationCode = map.cave;
    const isLevel = destinationCode > 0 && destinationCode <= 9;
    const destinationLabel = isLevel
      ? `Level ${destinationCode}`
      : `Cave ${formatHex(destinationCode)}`;

    mergeNodeMetadata(context, armosNodeId, {
      armosDestination: destinationLabel,
      armosDestinationType: isLevel ? "level" : "cave",
    });
    pushMetadataListEntry(
      context,
      armosNodeId,
      "specialCategories",
      "armos-stairs",
    );

    const caveEntry = screen.nodes?.caves?.[0];
    let targetNodeId: string;
    if (caveEntry) {
      targetNodeId = nodeId(screenKey, caveEntry.name);
      pushMetadataListEntry(
        context,
        targetNodeId,
        "specialCategories",
        "armos-destination",
      );
      mergeNodeMetadata(context, targetNodeId, {
        destinationLabel,
        armosEntry: true,
      });
    } else {
      targetNodeId = nodeId(screenKey, `${armosMeta.name} - Destination`);
      ensureNode(context, targetNodeId, {
        label: destinationLabel,
        nodeType: "meta",
        screenKey,
        area: map.area,
        roomId,
        metadata: {
          specialCategory: "armos-destination",
          destinationLabel,
          destinationType: isLevel ? "level" : "cave",
        },
      });
    }

    pushEdge(context, armosNodeId, targetNodeId, "fixed", false);
  }
}

function buildUnderworldGraph(
  context: BuildContext,
  maps: ZeldaUnderworldMapYaml[],
  screens: Map<number, ZeldaScreenYaml>,
  level: LevelResolution,
  mapIndex: Map<number, ZeldaUnderworldMapYaml>,
) {
  const levelRooms = new Set(level.level?.rooms ?? []);
  const selectedMaps = maps.filter((map) => levelRooms.has(map.map));
  if (DEBUG_LOG) {
    console.log(
      "Zelda1 selected maps",
      level.id,
      Array.from(levelRooms.values()),
      selectedMaps.map((map) => map.map),
    );
  }
  const screenKeyByMap = new Map<number, string>();
  const baseId = level.id;

  for (const map of selectedMaps) {
    const screen =
      screens.get(map.screen) ??
      (map.passage ? createSyntheticPassageScreen(map) : null);
    if (!screen) {
      if (DEBUG_LOG) {
        console.warn(
          `Zelda1 underworld screen 0x${map.screen.toString(16).toUpperCase()} missing for map 0x${map.map.toString(16).toUpperCase()}`,
        );
      }
      continue;
    }

    const mapIdHex = map.map.toString(16).toUpperCase().padStart(2, "0");
    const screenKey = `underworld::${baseId}::${mapIdHex}`;
    const roomId = `underworld::${baseId}::room::${mapIdHex}`;
    screenKeyByMap.set(map.map, screenKey);

    context.rooms.push({
      id: roomId,
      name: map.name,
      area: level.area,
      screenKeys: [screenKey],
    });

    context.screens.push({
      key: screenKey,
      name: `${level.level?.name ?? "Level"} · ${map.name}`,
      area: level.area,
      roomId,
      roomName: map.name,
      screenId: map.screen,
      screenLabel: `0x${map.screen.toString(16).toUpperCase()}`,
      coordinates: mapToCoordinate(map.map),
    });

    registerScreenNodes(context, screenKey, roomId, level.area, screen);
    registerScreenEdges(context, screenKey, roomId, level.area, screen);

    if (level.level && map.map === level.level.start_room_id) {
      markLevelStart(context, screen, screenKey, level.area, roomId);
    }
  }

  for (const map of selectedMaps) {
    if (!map.doors?.length) {
      continue;
    }

    const screenKey = screenKeyByMap.get(map.map);
    if (!screenKey) {
      continue;
    }

    const screen = screens.get(map.screen);
    if (!screen?.nodes?.exits) {
      continue;
    }

    for (const direction of Object.keys(DIRECTION_OFFSETS) as Direction[]) {
      const doorIndex = DOOR_INDEX[direction];
      const doorType = map.doors[doorIndex];
      if (doorType === undefined) {
        continue;
      }

      if (Number(doorType) === DoorType.Wall) {
        continue;
      }

      const offset = DIRECTION_OFFSETS[direction];
      const targetMap = selectedMaps.find(
        (candidate) => candidate.map === map.map + offset,
      );
      if (!targetMap) {
        continue;
      }

      const shouldHandle =
        map.map <= targetMap.map ||
        (map.map === targetMap.map &&
          doorIndex <= DOOR_INDEX[OPPOSITE_DIRECTION[direction]]);
      if (!shouldHandle) {
        continue;
      }

      const targetScreenKey = screenKeyByMap.get(targetMap.map);
      const targetScreen = targetScreenKey
        ? screens.get(targetMap.screen)
        : undefined;

      if (!targetScreenKey || !targetScreen?.nodes?.exits) {
        continue;
      }

      const sourceExit = screen.nodes.exits.find(
        (exit) => exit.direction === direction,
      );
      const targetExit = targetScreen.nodes.exits.find(
        (exit) => exit.direction === OPPOSITE_DIRECTION[direction],
      );

      if (!sourceExit || !targetExit) {
        continue;
      }

      const fromId = nodeId(screenKey, sourceExit.name);
      const toId = nodeId(targetScreenKey, targetExit.name);

      ensureNode(context, fromId, {
        label: sourceExit.name,
        nodeType: "exit",
        screenKey,
        area: level.area,
        roomId: `underworld::${baseId}::room::${map.map.toString(16).toUpperCase().padStart(2, "0")}`,
        metadata: {
          direction,
          doorType: doorTypeName(Number(doorType)),
        },
      });

      const oppositeDoorType =
        targetMap.doors[DOOR_INDEX[OPPOSITE_DIRECTION[direction]]] ??
        DoorType.Open;

      ensureNode(context, toId, {
        label: targetExit.name,
        nodeType: "exit",
        screenKey: targetScreenKey,
        area: level.area,
        roomId: `underworld::${baseId}::room::${targetMap.map.toString(16).toUpperCase().padStart(2, "0")}`,
        metadata: {
          direction: OPPOSITE_DIRECTION[direction],
          doorType: doorTypeName(Number(oppositeDoorType)),
        },
      });

      const forwardRequirement = doorRequirement(Number(doorType), map);
      const backwardRequirement = doorRequirement(
        Number(oppositeDoorType),
        targetMap,
      );

      pushEdge(context, fromId, toId, forwardRequirement, true);
      pushEdge(context, toId, fromId, backwardRequirement, true);
    }
  }

  linkUnderworldPassagesAndCellars(
    context,
    level,
    selectedMaps,
    mapIndex,
    screenKeyByMap,
    screens,
  );
}

function linkUnderworldPassagesAndCellars(
  context: BuildContext,
  level: LevelResolution,
  selectedMaps: ZeldaUnderworldMapYaml[],
  mapIndex: Map<number, ZeldaUnderworldMapYaml>,
  screenKeyByMap: Map<number, string>,
  screens: Map<number, ZeldaScreenYaml>,
) {
  const levelNumber = level.level?.level ?? 0;
  const area = level.area;
  const levelName = level.level?.name ?? "Level";
  const levelId = level.id;
  const offset = levelNumber >= 7 ? 0x80 : 0;

  for (const map of selectedMaps) {
    if (!map.passage) {
      continue;
    }

    const screenKey = screenKeyByMap.get(map.map);
    if (!screenKey) {
      continue;
    }

    const roomHex = map.map.toString(16).toUpperCase().padStart(2, "0");
    const roomId = `underworld::${levelId}::room::${roomHex}`;
    const mapName = `${area} - ${levelName} - ${map.name}`;

    const leftNodeId = nodeId(screenKey, "Passage - Left");
    ensureNode(context, leftNodeId, {
      label: "Passage - Left",
      nodeType: "meta",
      screenKey,
      area,
      roomId,
      metadata: {
        specialCategory: "passage",
        passageSide: "left",
        mapName,
        tile: [1, 3],
      },
    });

    if (map.screen === 0x3e) {
      const rightNodeId = nodeId(screenKey, "Passage - Right");
      ensureNode(context, rightNodeId, {
        label: "Passage - Right",
        nodeType: "meta",
        screenKey,
        area,
        roomId,
        metadata: {
          specialCategory: "passage",
          passageSide: "right",
          mapName,
          tile: [10, 3],
        },
      });

      pushEdge(context, leftNodeId, rightNodeId, "fixed", false);

      const leftRoomId = map.passage_left + offset;
      const rightRoomId = map.passage_right + offset;

      const leftConnector = resolvePassageConnector(
        context,
        leftRoomId,
        mapIndex,
        screenKeyByMap,
        screens,
        level,
      );
      if (leftConnector) {
        pushEdge(context, leftNodeId, leftConnector.nodeId, "fixed", false);
        pushMetadataListEntry(context, leftNodeId, "passageTargets", {
          destination: leftConnector.label,
          connectorType: leftConnector.connectorType,
          connectorName: leftConnector.connectorName,
        });
        pushMetadataListEntry(
          context,
          leftConnector.nodeId,
          "stairsConnections",
          {
            source: mapName,
            passageNode: "Passage - Left",
            passageType: "passage",
          },
        );
      }

      const rightConnector = resolvePassageConnector(
        context,
        rightRoomId,
        mapIndex,
        screenKeyByMap,
        screens,
        level,
      );
      if (rightConnector) {
        pushEdge(context, rightNodeId, rightConnector.nodeId, "fixed", false);
        pushMetadataListEntry(context, rightNodeId, "passageTargets", {
          destination: rightConnector.label,
          connectorType: rightConnector.connectorType,
          connectorName: rightConnector.connectorName,
        });
        pushMetadataListEntry(
          context,
          rightConnector.nodeId,
          "stairsConnections",
          {
            source: mapName,
            passageNode: "Passage - Right",
            passageType: "passage",
          },
        );
      }
    } else {
      const leftRoomId = map.passage_left + offset;
      const connector = resolvePassageConnector(
        context,
        leftRoomId,
        mapIndex,
        screenKeyByMap,
        screens,
        level,
      );

      if (connector) {
        pushEdge(context, leftNodeId, connector.nodeId, "fixed", false);
        pushMetadataListEntry(context, leftNodeId, "passageTargets", {
          destination: connector.label,
          connectorType: connector.connectorType,
          connectorName: connector.connectorName,
        });
        pushMetadataListEntry(context, connector.nodeId, "stairsConnections", {
          source: mapName,
          passageNode: "Passage - Left",
          passageType: "cellar",
        });
      }

      const itemNodeId = nodeId(screenKey, "Passage - Item");
      ensureNode(context, itemNodeId, {
        label: "Cellar Item",
        nodeType: "item",
        screenKey,
        area,
        roomId,
        metadata: {
          specialCategory: "cellar",
          sourceRoom: mapName,
          tile: [5, 3],
        },
      });

      pushEdge(context, leftNodeId, itemNodeId, "fixed", false);
      pushMetadataListEntry(context, leftNodeId, "passageTargets", {
        destination: `${mapName} (Cellar Item)`,
        connectorType: "item",
        connectorName: "Cellar Item",
      });
    }
  }
}

function resolvePassageConnector(
  context: BuildContext,
  targetRoomId: number,
  mapIndex: Map<number, ZeldaUnderworldMapYaml>,
  screenKeyByMap: Map<number, string>,
  screens: Map<number, ZeldaScreenYaml>,
  level: LevelResolution,
): PassageConnector | null {
  const targetMap = mapIndex.get(targetRoomId);
  if (!targetMap) {
    return null;
  }

  const targetScreenKey = screenKeyByMap.get(targetMap.map);
  if (!targetScreenKey) {
    return null;
  }

  const targetScreen = screens.get(targetMap.screen);
  if (!targetScreen) {
    return null;
  }

  const roomHex = targetMap.map.toString(16).toUpperCase().padStart(2, "0");
  const targetRoomIdString = `underworld::${level.id}::room::${roomHex}`;
  const area = level.area;
  const destinationLabel = formatUnderworldRoomLabel(level, targetMap);

  const stairs = targetScreen.nodes?.meta?.find(
    (entry) =>
      entry.type === "Stairs" ||
      entry.name.toLowerCase().includes("stairs") ||
      entry.name.toLowerCase().includes("passage"),
  );
  if (stairs) {
    const nodeIdValue = nodeId(targetScreenKey, stairs.name);
    ensureNode(context, nodeIdValue, {
      screenKey: targetScreenKey,
      area,
      roomId: targetRoomIdString,
    });
    mergeNodeMetadata(context, nodeIdValue, {
      metaType: "Stairs",
    });
    return {
      nodeId: nodeIdValue,
      label: destinationLabel,
      connectorType: "stairs",
      connectorName: stairs.name,
    } satisfies PassageConnector;
  }

  const topRightRegion = targetScreen.nodes?.regions?.find(
    (region) =>
      region.from[0] <= REGION_GRID_WIDTH - 1 &&
      region.to[0] >= REGION_GRID_WIDTH - 1 &&
      region.from[1] <= 1,
  );
  if (topRightRegion) {
    const nodeIdValue = nodeId(targetScreenKey, topRightRegion.name);
    ensureNode(context, nodeIdValue, {
      screenKey: targetScreenKey,
      area,
      roomId: targetRoomIdString,
    });
    return {
      nodeId: nodeIdValue,
      label: destinationLabel,
      connectorType: "region",
      connectorName: topRightRegion.name,
    } satisfies PassageConnector;
  }

  const fallbackRegion = targetScreen.nodes?.regions?.[0];
  if (fallbackRegion) {
    const nodeIdValue = nodeId(targetScreenKey, fallbackRegion.name);
    ensureNode(context, nodeIdValue, {
      screenKey: targetScreenKey,
      area,
      roomId: targetRoomIdString,
    });
    return {
      nodeId: nodeIdValue,
      label: destinationLabel,
      connectorType: "region",
      connectorName: fallbackRegion.name,
    } satisfies PassageConnector;
  }

  const fallbackExit = targetScreen.nodes?.exits?.[0];
  if (fallbackExit) {
    const nodeIdValue = nodeId(targetScreenKey, fallbackExit.name);
    ensureNode(context, nodeIdValue, {
      screenKey: targetScreenKey,
      area,
      roomId: targetRoomIdString,
    });
    return {
      nodeId: nodeIdValue,
      label: destinationLabel,
      connectorType: "exit",
      connectorName: fallbackExit.name,
    } satisfies PassageConnector;
  }

  return null;
}

function createSyntheticPassageScreen(
  map: ZeldaUnderworldMapYaml,
): ZeldaScreenYaml {
  return {
    name: `${map.name} Passage`,
    area: map.area,
    screen: map.screen,
    nodes: {
      exits: [],
      caves: [],
      regions: [],
      meta: [],
    },
    edges: {
      undirected: {},
      directed: {},
    },
  };
}

function markLevelStart(
  context: BuildContext,
  screen: ZeldaScreenYaml,
  screenKey: string,
  area: ZeldaArea,
  roomId: string,
) {
  const startExit = screen.nodes?.exits?.find(
    (entry) => entry.direction === "Down",
  );
  if (startExit) {
    const nodeIdValue = nodeId(screenKey, startExit.name);
    mergeNodeMetadata(context, nodeIdValue, {
      levelStart: true,
    });
    pushMetadataListEntry(
      context,
      nodeIdValue,
      "specialCategories",
      "level-start",
    );
    return;
  }

  const fallbackNodeId = nodeId(screenKey, "Start Location");
  ensureNode(context, fallbackNodeId, {
    label: "Start Location",
    nodeType: "meta",
    screenKey,
    area,
    roomId,
    metadata: {
      levelStart: true,
    },
  });
  pushMetadataListEntry(
    context,
    fallbackNodeId,
    "specialCategories",
    "level-start",
  );
}

function registerScreenNodes(
  context: BuildContext,
  screenKey: string,
  roomId: string,
  area: ZeldaArea,
  screen: ZeldaScreenYaml,
) {
  const exits = screen.nodes?.exits ?? [];
  const caves = screen.nodes?.caves ?? [];
  const regions = screen.nodes?.regions ?? [];
  const meta = screen.nodes?.meta ?? [];

  for (const exit of exits) {
    ensureNode(context, nodeId(screenKey, exit.name), {
      label: exit.name,
      nodeType: "exit",
      screenKey,
      area,
      roomId,
      metadata: {
        direction: exit.direction,
        exitType: exit.type,
      },
    });
  }

  for (const cave of caves) {
    ensureNode(context, nodeId(screenKey, cave.name), {
      label: cave.name,
      nodeType: "cave",
      screenKey,
      area,
      roomId,
      metadata: {
        caveType: cave.type,
      },
    });
  }

  for (const region of regions) {
    ensureNode(context, nodeId(screenKey, region.name), {
      label: region.name,
      nodeType: "region",
      screenKey,
      area,
      roomId,
      metadata: {
        from: region.from,
        to: region.to,
      },
    });
  }

  for (const entry of meta) {
    const nodeType = entry.type === "Item" ? "item" : "meta";
    const tile = parseTilePosition(entry.position);
    ensureNode(context, nodeId(screenKey, entry.name), {
      label: entry.name,
      nodeType,
      screenKey,
      area,
      roomId,
      metadata: {
        metaType: entry.type,
        item: entry.item,
        position: entry.position,
        tile,
      },
    });
  }
}

function registerScreenEdges(
  context: BuildContext,
  screenKey: string,
  roomId: string,
  area: ZeldaArea,
  screen: ZeldaScreenYaml,
) {
  const undirected = screen.edges?.undirected ?? {};
  const directed = screen.edges?.directed ?? {};

  for (const [requirement, pairs] of Object.entries(undirected)) {
    for (const pair of pairs ?? []) {
      const [fromName, toName] = pair;
      const fromId = nodeId(screenKey, fromName);
      const toId = nodeId(screenKey, toName);

      ensureNode(context, fromId, {
        label: fromName,
        screenKey,
        area,
        roomId,
      });
      ensureNode(context, toId, {
        label: toName,
        screenKey,
        area,
        roomId,
      });

      pushEdge(context, fromId, toId, requirement, false);
    }
  }

  for (const [requirement, pairs] of Object.entries(directed)) {
    for (const pair of pairs ?? []) {
      const [fromName, toName] = pair;
      const fromId = nodeId(screenKey, fromName);
      const toId = nodeId(screenKey, toName);

      ensureNode(context, fromId, {
        label: fromName,
        screenKey,
        area,
        roomId,
      });
      ensureNode(context, toId, {
        label: toName,
        screenKey,
        area,
        roomId,
      });

      pushEdge(context, fromId, toId, requirement, true);
    }
  }
}

function ensureNode(
  context: BuildContext,
  nodeIdValue: string,
  data: Partial<NodeRegistryEntry>,
): NodeRegistryEntry {
  const existing = context.nodeRegistry.get(nodeIdValue);
  if (existing) {
    if (data.label && existing.label !== data.label) {
      existing.label = data.label;
    }
    if (
      data.nodeType &&
      existing.nodeType !== data.nodeType &&
      existing.nodeType === "unknown"
    ) {
      existing.nodeType = data.nodeType;
    }
    if (data.screenKey) {
      existing.screenKey = data.screenKey;
    }
    if (data.area) {
      existing.area = data.area;
    }
    if (data.roomId) {
      existing.roomId = data.roomId;
    }
    if (data.metadata) {
      existing.metadata = { ...existing.metadata, ...data.metadata };
    }
    return existing;
  }

  const node: NodeRegistryEntry = {
    id: nodeIdValue,
    label: data.label ?? nodeIdValue,
    nodeType: data.nodeType ?? "unknown",
    screenKey: data.screenKey,
    area: data.area,
    roomId: data.roomId,
    metadata: data.metadata ?? {},
  };

  context.nodeRegistry.set(nodeIdValue, node);
  return node;
}

function mergeNodeMetadata(
  context: BuildContext,
  nodeId: string,
  metadata: Record<string, unknown>,
): NodeRegistryEntry {
  const node = context.nodeRegistry.get(nodeId);
  if (node) {
    node.metadata = { ...node.metadata, ...metadata };
    return node;
  }

  return ensureNode(context, nodeId, { metadata });
}

function pushMetadataListEntry(
  context: BuildContext,
  nodeId: string,
  key: string,
  entry: unknown,
): NodeRegistryEntry {
  const node = mergeNodeMetadata(context, nodeId, {});
  const current = node.metadata[key];
  const nextList = Array.isArray(current)
    ? current.includes(entry)
      ? current
      : [...current, entry]
    : [entry];
  node.metadata[key] = nextList;
  return node;
}

function pushEdge(
  context: BuildContext,
  from: string,
  to: string,
  weight: string,
  directed: boolean,
) {
  const id = `${from}::${to}::${weight}::${directed ? "d" : "u"}`;
  if (context.edgeIds.has(id)) {
    return;
  }

  context.edgeIds.add(id);
  context.edges.push({
    id,
    from,
    to,
    weight,
    directed,
  });
}

function buildLevelOptions(levels: ZeldaLevelYaml[]): GraphLevelOption[] {
  const options: GraphLevelOption[] = [];

  const overworld = levels.find((level) => level.level === 0);
  if (overworld) {
    options.push({
      id: "overworld",
      label: overworld.name ?? "Overworld",
      area: "Overworld",
      level: overworld.level,
    });
  } else {
    options.push({
      id: "overworld",
      label: "Overworld",
      area: "Overworld",
      level: 0,
    });
  }

  const dungeons = levels
    .filter((level) => level.level > 0)
    .sort((a, b) => a.level - b.level);

  for (const level of dungeons) {
    options.push({
      id: levelId(level.level),
      label: level.name,
      area: level.area,
      level: level.level,
    });
  }

  return options;
}

function resolveLevelId(
  options: GraphLevelOption[],
  requestedId?: string | null,
): string {
  if (requestedId && options.some((option) => option.id === requestedId)) {
    return requestedId;
  }
  return options[0]?.id ?? "overworld";
}

function resolveLevel(levels: ZeldaLevelYaml[], id: string): LevelResolution {
  if (id === "overworld") {
    const overworld = levels.find((level) => level.level === 0) ?? null;
    return {
      id,
      level: overworld,
      area: "Overworld",
    };
  }

  const target = levels.find((level) => levelId(level.level) === id) ?? null;
  return {
    id,
    level: target,
    area: "Underworld",
  };
}

function levelId(level: number): string {
  return `level-${level.toString(16).toUpperCase().padStart(2, "0")}`;
}

function formatUnderworldRoomLabel(
  level: LevelResolution,
  map: ZeldaUnderworldMapYaml,
): string {
  const levelName = level.level?.name ?? "Level";
  return `${level.area} - ${levelName} - ${map.name}`;
}

function nodeId(screenKey: string, name: string): string {
  return `${screenKey}::${name}`;
}

function indexMaps<T extends { map: number }>(maps: T[]): Map<number, T> {
  const map = new Map<number, T>();
  for (const entry of maps) {
    map.set(entry.map, entry);
  }
  return map;
}

function mapToCoordinate(mapId: number): Coordinate {
  return {
    x: mapId % GRID_WIDTH,
    y: Math.floor(mapId / GRID_WIDTH),
  };
}

function indexScreens(
  screens: ZeldaScreenYaml[],
): Map<number, ZeldaScreenYaml> {
  const map = new Map<number, ZeldaScreenYaml>();
  for (const screen of screens) {
    map.set(screen.screen, screen);
  }
  return map;
}

function doorTypeName(value: number): string {
  return DoorType[value] ?? `Door-${value}`;
}

function formatHex(value: number, width = 2): string {
  return `0x${value.toString(16).toUpperCase().padStart(width, "0")}`;
}

function parseTilePosition(value?: string): [number, number] | null {
  if (!value) {
    return null;
  }

  const parts = value
    .split(",")
    .map((part) => Number.parseInt(part.trim(), 10))
    .filter((part) => !Number.isNaN(part));

  if (parts.length !== 2) {
    return null;
  }

  return [parts[0], parts[1]];
}

function doorRequirement(door: number, map: ZeldaUnderworldMapYaml): string {
  switch (door) {
    case DoorType.Open:
    case DoorType.PassThrough:
    case DoorType.PassThroughNoSound:
      return "fixed";
    case DoorType.Wall:
      return "Never";
    case DoorType.Bombable:
      return "UseBombs";
    case DoorType.Locked:
    case DoorType.Locked2:
      return "Key";
    case DoorType.Shutter: {
      if (map.level_nine_check) {
        return "Triforce";
      }

      const behaviour = map.behaviour as RoomBehaviour;
      if (behaviour === RoomBehaviour.None) {
        return "Never";
      }

      return "fixed";
    }
    default:
      return "fixed";
  }
}

async function loadYamlFiles<T>(dir: string): Promise<T[]> {
  const entries: T[] = [];
  const dirEntries = await readdir(dir);

  for (const entry of dirEntries) {
    const fullPath = join(dir, entry);
    const stats = await stat(fullPath);

    if (stats.isDirectory()) {
      entries.push(...(await loadYamlFiles<T>(fullPath)));
    } else if (stats.isFile() && entry.endsWith(".yml")) {
      const raw = await readFile(fullPath, "utf-8");
      entries.push(parse(raw) as T);
    }
  }

  return entries;
}

async function loadYamlFile<T>(filePath: string): Promise<T | null> {
  try {
    const raw = await readFile(filePath, "utf-8");
    return parse(raw) as T;
  } catch (error) {
    const err = error as NodeJS.ErrnoException;
    if (err.code === "ENOENT") {
      return null;
    }
    throw error;
  }
}
