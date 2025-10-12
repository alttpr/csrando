import { join, resolve } from "node:path";
import { readdir, readFile, stat } from "node:fs/promises";
import { parse } from "yaml";

import type {
  Coordinate,
  GraphData,
  GraphEdge,
  GraphNode,
  LogicGameId,
  RoomInstance,
  ScreenInstance,
} from "./types";

type MetroidArea =
  | "Brinstar"
  | "Norfair"
  | "Kraid"
  | "Tourian"
  | "Ridley"
  | "Meta";

type MetroidDirection = "Up" | "Down" | "Left" | "Right";
type MetroidDoorType = "Blue" | "Red" | "Purple" | "Orange";
type MetroidExitType = "Scroll" | "Tunnel" | "Elevator";
type MetroidScrolling = "Vertical" | "Horizontal";

interface MetroidRoomYaml {
  name: string;
  area: MetroidArea;
  scroll: MetroidScrolling;
  position: [number, number];
  screens: number[];
  sprites?: MetroidSpriteYaml[];
}

interface MetroidSpriteYaml {
  name: string;
  screen: number;
  location: string;
  slot: number;
  type: "Item" | "Elevator";
  item: string;
}

interface MetroidScreenYaml {
  name: string;
  area: MetroidArea;
  screen: number;
  scroll: MetroidScrolling;
  nodes?: {
    doors?: MetroidDoorYaml[];
    exits?: MetroidExitYaml[];
    locations?: MetroidLocationYaml[];
  };
  edges?: {
    undirected?: Record<string, string[][]>;
    directed?: Record<string, string[][]>;
  };
}

interface MetroidDoorYaml {
  name: string;
  type: MetroidDoorType;
  direction: MetroidDirection;
}

interface MetroidExitYaml {
  name: string;
  type: MetroidExitType;
  direction: MetroidDirection;
}

interface MetroidLocationYaml {
  name: string;
  type: "Meta" | "Item" | "Boss" | "Elevator" | "Start";
  position?: number[];
  item?: string;
}

interface ScreenInstanceInternal extends ScreenInstance {
  rawScreen: MetroidScreenYaml;
  screenIndex: number;
  room: MetroidRoomYaml;
}

interface NodeRegistryEntry extends GraphNode {
  metadata: Record<string, unknown>;
}

const DOOR_REQUIREMENTS: Record<MetroidDoorType, string> = {
  Blue: "fixed",
  Red: "Missile",
  Purple: "Missile|2",
  Orange: "Missile|3",
};

const OPPOSITE_DIRECTION: Record<MetroidDirection, MetroidDirection> = {
  Up: "Down",
  Down: "Up",
  Left: "Right",
  Right: "Left",
};

const DATA_ROOT = resolve(process.cwd(), "../Randomizer/Games/Metroid/data");

export async function loadMetroidGraph(): Promise<GraphData> {
  const [rooms, screens] = await Promise.all([
    loadRooms(join(DATA_ROOT, "Rooms")),
    loadScreens(join(DATA_ROOT, "Screens")),
  ]);

  const screenMap = new Map<string, MetroidScreenYaml>();
  for (const screen of screens) {
    screenMap.set(makeScreenKey(screen.area, screen.screen), screen);
  }

  const screenInstances: ScreenInstanceInternal[] = [];
  const positionMap = new Map<string, ScreenInstanceInternal[]>();

  for (const room of rooms) {
    room.screens.forEach((screenId, index) => {
      const screen = screenMap.get(makeScreenKey(room.area, screenId));
      if (!screen) {
        throw new Error(
          `Metroid screen ${room.area} 0x${screenId.toString(16).toUpperCase()} referenced by room ${room.name} not found`,
        );
      }

      const coordinates = calculateScreenCoordinates(room, index);
      const instance: ScreenInstanceInternal = {
        key: `${room.area}::${room.name}::${index}`,
        name: `${room.area} - ${room.name} - ${screen.name} (${index})`,
        area: room.area,
        roomId: `${room.area}::${room.name}`,
        roomName: room.name,
        screenId,
        screenLabel: `0x${screenId.toString(16).toUpperCase()}`,
        scroll: screen.scroll,
        coordinates,
        rawScreen: screen,
        screenIndex: index,
        room,
      };

      screenInstances.push(instance);

      const positionKey = makePositionKey(room.area, coordinates);
      const entries = positionMap.get(positionKey) ?? [];
      entries.push(instance);
      positionMap.set(positionKey, entries);
    });
  }

  const nodeRegistry = new Map<string, NodeRegistryEntry>();
  const edges: GraphEdge[] = [];
  const roomsOut: RoomInstance[] = [];

  for (const room of rooms) {
    const screenKeys = room.screens.map(
      (_, index) => `${room.area}::${room.name}::${index}`,
    );
    roomsOut.push({
      id: `${room.area}::${room.name}`,
      name: room.name,
      area: room.area,
      scroll: room.scroll,
      position: { x: room.position[0], y: room.position[1] },
      screenKeys,
    });
  }

  for (const instance of screenInstances) {
    buildScreenGraph(
      instance,
      positionMap,
      nodeRegistry,
      edges,
      screenInstances,
      rooms,
    );
  }

  const nodes = Array.from(nodeRegistry.values());

  return {
    game: "metroid" satisfies LogicGameId,
    rooms: roomsOut,
    screens: screenInstances,
    nodes,
    edges,
  };
}

async function loadRooms(dir: string): Promise<MetroidRoomYaml[]> {
  return loadYamlFiles<MetroidRoomYaml>(dir);
}

async function loadScreens(dir: string): Promise<MetroidScreenYaml[]> {
  return loadYamlFiles<MetroidScreenYaml>(dir);
}

async function loadYamlFiles<T>(dir: string): Promise<T[]> {
  const collected: T[] = [];
  const entries = await readdir(dir);

  for (const entry of entries) {
    const fullPath = join(dir, entry);
    const entryStat = await stat(fullPath);

    if (entryStat.isDirectory()) {
      collected.push(...(await loadYamlFiles<T>(fullPath)));
    } else if (entryStat.isFile() && entry.endsWith(".yml")) {
      const raw = await readFile(fullPath, "utf-8");
      const parsed = parse(raw) as T;
      collected.push(parsed);
    }
  }

  return collected;
}

function makeScreenKey(area: MetroidArea, screenId: number): string {
  return `${area}:${screenId}`;
}

function makePositionKey(area: MetroidArea, coordinates: Coordinate): string {
  return `${area}:${coordinates.x},${coordinates.y}`;
}

function calculateScreenCoordinates(
  room: MetroidRoomYaml,
  screenIndex: number,
): Coordinate {
  const [x, y] = room.position;
  if (room.scroll === "Horizontal") {
    return { x: x + screenIndex, y };
  }

  return { x, y: y + screenIndex };
}

function ensureNode(
  registry: Map<string, NodeRegistryEntry>,
  nodeId: string,
  data: Partial<NodeRegistryEntry>,
): NodeRegistryEntry {
  const existing = registry.get(nodeId);
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
    id: nodeId,
    label: data.label ?? nodeId,
    nodeType: data.nodeType ?? "meta",
    screenKey: data.screenKey,
    area: data.area,
    roomId: data.roomId,
    metadata: data.metadata ?? {},
  };

  registry.set(nodeId, node);
  return node;
}

function pushEdge(
  edges: GraphEdge[],
  from: string,
  to: string,
  weight: string,
  directed: boolean,
) {
  edges.push({
    id: `${from}::${to}::${weight}::${directed ? "d" : "u"}`,
    from,
    to,
    weight,
    directed,
  });
}

function buildScreenGraph(
  instance: ScreenInstanceInternal,
  positionMap: Map<string, ScreenInstanceInternal[]>,
  registry: Map<string, NodeRegistryEntry>,
  edges: GraphEdge[],
  allScreens: ScreenInstanceInternal[],
  rooms: MetroidRoomYaml[],
) {
  const { rawScreen: screen, room } = instance;
  const screenName = instance.name;

  // Register door nodes and connect to matching doors
  for (const door of screen.nodes?.doors ?? []) {
    const doorNodeName = `${screenName} - ${door.name}`;
    ensureNode(registry, doorNodeName, {
      label: door.name,
      nodeType: "door",
      screenKey: instance.key,
      area: instance.area,
      roomId: instance.roomId,
      metadata: {
        direction: door.direction,
        doorType: door.type,
      },
    });

    const targetPosition = offsetCoordinate(
      instance.coordinates,
      door.direction,
    );
    const matchingInstance = findMatchingDoorScreen(
      room.area,
      targetPosition,
      positionMap,
      door.direction,
    );
    if (!matchingInstance) {
      continue;
    }

    const targetDoor = matchingInstance.rawScreen.nodes?.doors?.find(
      (d) => d.direction === OPPOSITE_DIRECTION[door.direction],
    );

    if (!targetDoor) {
      continue;
    }

    const targetDoorNodeName = `${matchingInstance.name} - ${targetDoor.name}`;
    ensureNode(registry, targetDoorNodeName, {
      label: targetDoor.name,
      nodeType: "door",
      screenKey: matchingInstance.key,
      area: matchingInstance.area,
      roomId: matchingInstance.roomId,
      metadata: {
        direction: targetDoor.direction,
        doorType: targetDoor.type,
      },
    });

    pushEdge(
      edges,
      doorNodeName,
      targetDoorNodeName,
      DOOR_REQUIREMENTS[door.type],
      true,
    );
  }

  // Elevator connections
  const elevator = screen.nodes?.exits?.find(
    (exit) =>
      exit.type === "Elevator" &&
      exit.direction === "Down" &&
      instance.room.screens.length === 1,
  );

  if (elevator) {
    const targetRoom = rooms.find(
      (candidate) =>
        candidate.position[0] === instance.coordinates.x &&
        candidate.position[1] === instance.coordinates.y + 1,
    );

    if (targetRoom && targetRoom.screens.length > 0) {
      const targetScreenId = targetRoom.screens[0];
      const targetScreen = allScreens.find(
        (s) =>
          s.roomId === `${targetRoom.area}::${targetRoom.name}` &&
          s.screenId === targetScreenId,
      );

      if (targetScreen) {
        const targetExit = targetScreen.rawScreen.nodes?.exits?.find(
          (exit) => exit.type === "Elevator" && exit.direction === "Up",
        );

        if (targetExit) {
          const elevatorNodeName = `${screenName} - ${elevator.name}`;
          const targetExitNodeName = `${targetScreen.name} - ${targetExit.name}`;

          ensureNode(registry, elevatorNodeName, {
            label: elevator.name,
            nodeType: "exit",
            screenKey: instance.key,
            area: instance.area,
            roomId: instance.roomId,
            metadata: {
              type: elevator.type,
              direction: elevator.direction,
            },
          });

          ensureNode(registry, targetExitNodeName, {
            label: targetExit.name,
            nodeType: "exit",
            screenKey: targetScreen.key,
            area: targetScreen.area,
            roomId: targetScreen.roomId,
            metadata: {
              type: targetExit.type,
              direction: targetExit.direction,
            },
          });

          pushEdge(edges, elevatorNodeName, targetExitNodeName, "fixed", false);
        }
      }
    }
  }

  // Connect exits between sequential screens
  if (instance.screenIndex > 0) {
    const previousScreen = allScreens.find(
      (candidate) =>
        candidate.roomId === instance.roomId &&
        candidate.screenIndex === instance.screenIndex - 1,
    );

    if (previousScreen) {
      const [fromDirection, toDirection] =
        room.scroll === "Horizontal"
          ? (["Left", "Right"] as const)
          : (["Up", "Down"] as const);

      for (const exit of screen.nodes?.exits?.filter(
        (e) => e.direction === fromDirection,
      ) ?? []) {
        const exitNodeName = `${screenName} - ${exit.name}`;
        ensureNode(registry, exitNodeName, {
          label: exit.name,
          nodeType: "exit",
          screenKey: instance.key,
          area: instance.area,
          roomId: instance.roomId,
          metadata: {
            type: exit.type,
            direction: exit.direction,
          },
        });

        const targetExits =
          previousScreen.rawScreen.nodes?.exits?.filter(
            (e) => e.direction === toDirection,
          ) ?? [];

        for (const targetExit of targetExits) {
          const targetExitNodeName = `${previousScreen.name} - ${targetExit.name}`;
          ensureNode(registry, targetExitNodeName, {
            label: targetExit.name,
            nodeType: "exit",
            screenKey: previousScreen.key,
            area: previousScreen.area,
            roomId: previousScreen.roomId,
            metadata: {
              type: targetExit.type,
              direction: targetExit.direction,
            },
          });

          pushEdge(edges, exitNodeName, targetExitNodeName, "fixed", false);
        }
      }
    }
  }

  // Locations and item sprites
  for (const location of screen.nodes?.locations ?? []) {
    const locationNodeName = `${screenName} - ${location.name}`;
    ensureNode(registry, locationNodeName, {
      label: location.name,
      nodeType: "location",
      screenKey: instance.key,
      area: instance.area,
      roomId: instance.roomId,
      metadata: {
        type: location.type,
        item: location.item ?? null,
        position: location.position ?? null,
      },
    });

    const itemSprite = instance.room.sprites?.find(
      (sprite) =>
        sprite.screen === instance.screenIndex &&
        sprite.location === location.name &&
        sprite.type === "Item",
    );

    if (itemSprite) {
      const itemNodeName = `${screenName} - ${itemSprite.name}`;
      ensureNode(registry, itemNodeName, {
        label: itemSprite.name,
        nodeType: "item",
        screenKey: instance.key,
        area: instance.area,
        roomId: instance.roomId,
        metadata: {
          item: itemSprite.item,
          itemSet: ["metroid"],
          slot: itemSprite.slot,
        },
      });

      pushEdge(edges, locationNodeName, itemNodeName, "fixed", false);
    }
  }

  const edgeCollections: Array<{
    type: "directed" | "undirected";
    weight: string;
    connections: string[][];
  }> = [];

  for (const [weight, connections] of Object.entries(
    screen.edges?.undirected ?? {},
  )) {
    edgeCollections.push({
      type: "undirected",
      weight,
      connections,
    });
  }

  for (const [weight, connections] of Object.entries(
    screen.edges?.directed ?? {},
  )) {
    edgeCollections.push({
      type: "directed",
      weight,
      connections,
    });
  }

  for (const collection of edgeCollections) {
    for (const [fromName, toName] of collection.connections) {
      const fromNodeName = `${screenName} - ${fromName}`;
      const toNodeName = `${screenName} - ${toName}`;

      ensureNode(registry, fromNodeName, {
        label: fromName,
        nodeType: inferNodeType(registry, fromNodeName),
        screenKey: instance.key,
        area: instance.area,
        roomId: instance.roomId,
      });

      ensureNode(registry, toNodeName, {
        label: toName,
        nodeType: inferNodeType(registry, toNodeName),
        screenKey: instance.key,
        area: instance.area,
        roomId: instance.roomId,
      });

      pushEdge(
        edges,
        fromNodeName,
        toNodeName,
        collection.weight,
        collection.type === "directed",
      );
    }
  }
}

function findMatchingDoorScreen(
  area: MetroidArea,
  coordinates: Coordinate,
  positionMap: Map<string, ScreenInstanceInternal[]>,
  direction: MetroidDirection,
): ScreenInstanceInternal | undefined {
  const candidates = positionMap.get(makePositionKey(area, coordinates)) ?? [];
  const opposite = OPPOSITE_DIRECTION[direction];

  return candidates.find((candidate) =>
    candidate.rawScreen.nodes?.doors?.some(
      (door) => door.direction === opposite,
    ),
  );
}

function offsetCoordinate(
  coordinate: Coordinate,
  direction: MetroidDirection,
): Coordinate {
  switch (direction) {
    case "Up":
      return { x: coordinate.x, y: coordinate.y - 1 };
    case "Down":
      return { x: coordinate.x, y: coordinate.y + 1 };
    case "Left":
      return { x: coordinate.x - 1, y: coordinate.y };
    case "Right":
      return { x: coordinate.x + 1, y: coordinate.y };
    default:
      return coordinate;
  }
}

function inferNodeType(
  registry: Map<string, NodeRegistryEntry>,
  nodeName: string,
): NodeRegistryEntry["nodeType"] {
  return registry.get(nodeName)?.nodeType ?? "unknown";
}
