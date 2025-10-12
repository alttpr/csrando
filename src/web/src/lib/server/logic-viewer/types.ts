export type LogicGameId = "metroid" | "zelda1";

export interface GraphData {
  game: LogicGameId;
  rooms: RoomInstance[];
  screens: ScreenInstance[];
  nodes: GraphNode[];
  edges: GraphEdge[];
  metadata?: GraphMetadata;
}

export interface GraphMetadata {
  levels?: GraphLevelOption[];
  selectedLevelId?: string;
}

export interface GraphLevelOption {
  id: string;
  label: string;
  area: string;
  level?: number;
}

export interface RoomInstance {
  id: string;
  name: string;
  area: string;
  scroll?: string;
  position?: Coordinate;
  screenKeys: string[];
}

export interface ScreenInstance {
  key: string;
  name: string;
  area: string;
  roomId: string;
  roomName: string;
  screenId: number;
  screenLabel: string;
  scroll?: string;
  coordinates: Coordinate;
}

export interface GraphNode {
  id: string;
  label: string;
  nodeType: NodeType;
  screenKey?: string;
  area?: string;
  roomId?: string;
  metadata?: Record<string, unknown>;
}

export type NodeType =
  | "door"
  | "exit"
  | "location"
  | "item"
  | "meta"
  | "region"
  | "cave"
  | "unknown";

export interface GraphEdge {
  id: string;
  from: string;
  to: string;
  weight: string;
  directed: boolean;
}

export interface Coordinate {
  x: number;
  y: number;
}
