<script lang="ts">
  import { onDestroy } from "svelte";
  import type {
    GraphData,
    GraphEdge,
    GraphNode,
    LogicGameId,
    Coordinate,
    GraphLevelOption,
  } from "$lib/server/logic-viewer/types";

  const gameOptions: {
    id: LogicGameId;
    label: string;
    implemented: boolean;
  }[] = [
    { id: "metroid", label: "Metroid (NES)", implemented: true },
    { id: "zelda1", label: "The Legend of Zelda (NES)", implemented: true },
  ];

  export let data: {
    initialGame: LogicGameId;
    initialLevel: string | null;
    initialData: GraphData | null;
  };

  let game: LogicGameId = data.initialGame;
  let graph: GraphData | null = data.initialData;
  let selectedLevelId =
    data.initialData?.metadata?.selectedLevelId ?? data.initialLevel ?? "";
  let levelOptions: GraphLevelOption[] = [];
  let loading = false;
  let error: string | null = graph
    ? null
    : "Unable to load initial graph data.";
  let selectedNodeId: string | null = null;
  const minZoom = 0.25;
  const maxZoom = 64;
  let zoom = 1;
  let viewBoxX = 0;
  let viewBoxY = 0;
  let svgElement: SVGSVGElement;
  let isPanning = false;
  let activePointerId: number | null = null;
  let panOrigin: {
    pointerX: number;
    pointerY: number;
    viewBoxX: number;
    viewBoxY: number;
  } | null = null;
  let lastGraphSignature: string | null = null;
  let viewerContainer: HTMLDivElement | null = null;
  let containerWidth = 0;
  let containerHeight = 0;
  let autoZoom = true;
  let resizeObserver: ResizeObserver | null = null;
  $: levelOptions = graph?.metadata?.levels ?? [];

  type PositionedNode = {
    node: GraphNode;
    x: number;
    y: number;
  };

  type DrawableEdge = {
    edge: GraphEdge;
    x1: number;
    y1: number;
    x2: number;
    y2: number;
  };

  type Rect = {
    x: number;
    y: number;
    width: number;
    height: number;
  };

  type RoomShape = {
    roomId: string;
    rect: Rect;
    name: string;
    area: string;
    label: Coordinate;
  };

  type RegionOverlay = {
    nodeId: string;
    rect: Rect;
  };

  type ConnectionGroup = {
    targetId: string;
    targetLabel: string;
    outgoing: Set<string>;
    incoming: Set<string>;
    bidirectional: Set<string>;
    roomLabel?: string;
    screenLabel?: string;
    isExternalRoom: boolean;
    isExternalScreen: boolean;
  };

  $: screenMap = new Map(graph?.screens?.map((screen) => [screen.key, screen]));
  $: nodeMap = new Map(graph?.nodes?.map((node) => [node.id, node]));

  const SCREEN_WIDTH = 180;
  const SCREEN_HEIGHT = 140;
  const SCREEN_MARGIN = 80;
  const ZELDA_REGION_COLS = 12;
  const ZELDA_REGION_ROWS = 7;

  $: bounds = graph
    ? computeBounds(graph)
    : {
        minX: 0,
        minY: 0,
        maxX: 1,
        maxY: 1,
      };

  $: viewWidth =
    (bounds.maxX - bounds.minX + 1) * SCREEN_WIDTH + SCREEN_MARGIN * 2;
  $: viewHeight =
    (bounds.maxY - bounds.minY + 1) * SCREEN_HEIGHT + SCREEN_MARGIN * 2;

  $: effectiveWidth =
    viewWidth > 0 ? Math.max(1, viewWidth / zoom) : Math.max(1, 1 / zoom);
  $: effectiveHeight =
    viewHeight > 0 ? Math.max(1, viewHeight / zoom) : Math.max(1, 1 / zoom);

  $: {
    const maxX = Math.max(0, viewWidth - effectiveWidth);
    if (viewBoxX < 0) {
      viewBoxX = 0;
    } else if (viewBoxX > maxX) {
      viewBoxX = maxX;
    }
  }

  $: {
    const maxY = Math.max(0, viewHeight - effectiveHeight);
    if (viewBoxY < 0) {
      viewBoxY = 0;
    } else if (viewBoxY > maxY) {
      viewBoxY = maxY;
    }
  }

  onDestroy(() => {
    resizeObserver?.disconnect();
    resizeObserver = null;
  });

  $: if (viewerContainer) {
    if (!resizeObserver) {
      resizeObserver = new ResizeObserver((entries) => {
        const entry = entries[0];
        if (!entry) {
          return;
        }

        containerWidth = entry.contentRect.width;
        containerHeight = entry.contentRect.height;

        if (autoZoom) {
          fitToViewport();
        }
      });

      const rect = viewerContainer.getBoundingClientRect();
      containerWidth = rect.width;
      containerHeight = rect.height;

      resizeObserver.observe(viewerContainer);

      if (autoZoom) {
        fitToViewport();
      }
    }
  } else if (resizeObserver) {
    resizeObserver.disconnect();
    resizeObserver = null;
    containerWidth = 0;
    containerHeight = 0;
  }

  $: if (
    autoZoom &&
    graph &&
    containerWidth &&
    containerHeight &&
    viewWidth &&
    viewHeight
  ) {
    fitToViewport();
  }

  $: positionedNodes = graph
    ? buildPositionedNodes(graph)
    : ([] as PositionedNode[]);

  $: nodePositionMap = new Map(
    positionedNodes.map(({ node, x, y }) => [node.id, { x, y }]),
  );

  $: drawableEdges = graph
    ? buildDrawableEdges(graph, nodePositionMap)
    : ([] as DrawableEdge[]);

  $: roomShapes = graph ? buildRoomShapes(graph) : ([] as RoomShape[]);
  $: regionOverlays = graph
    ? buildRegionOverlays(graph)
    : ([] as RegionOverlay[]);
  $: selectedPassageTargets = selectedNode?.metadata
    ? toRecordArray(selectedNode.metadata.passageTargets)
    : [];
  $: selectedStairsConnections = selectedNode?.metadata
    ? toRecordArray(selectedNode.metadata.stairsConnections)
    : [];
  $: selectedSpecialCategories = Array.isArray(
    selectedNode?.metadata?.specialCategories,
  )
    ? (selectedNode?.metadata?.specialCategories as string[])
    : [];

  $: selectedNode = selectedNodeId
    ? (nodeMap.get(selectedNodeId) ?? null)
    : null;
  $: connectionGroups =
    selectedNode && graph ? buildConnectionGroups(selectedNode, graph) : [];

  async function onGameChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value as LogicGameId;
    game = value;
    selectedNodeId = null;

    if (!gameOptions.find((option) => option.id === value)?.implemented) {
      error = "Viewer for this game has not been implemented yet.";
      graph = null;
      return;
    }

    await loadGraph(
      value,
      value === "zelda1" && selectedLevelId.length > 0
        ? selectedLevelId
        : undefined,
    );
  }

  async function loadGraph(gameId: LogicGameId, levelId?: string) {
    loading = true;
    error = null;

    try {
      const params = new URLSearchParams();
      if (gameId === "zelda1" && levelId && levelId.length > 0) {
        params.set("level", levelId);
      }

      const query = params.size > 0 ? `?${params.toString()}` : "";
      const response = await fetch(`/api/logic-viewer/${gameId}${query}`);
      if (!response.ok) {
        throw new Error(await response.text());
      }

      const nextGraph = (await response.json()) as GraphData;
      graph = nextGraph;

      if (gameId === "zelda1") {
        const metadataLevel =
          nextGraph.metadata?.selectedLevelId ??
          levelId ??
          nextGraph.metadata?.levels?.[0]?.id ??
          "";
        selectedLevelId = metadataLevel;
      }
    } catch (err) {
      console.error(err);
      error =
        err instanceof Error
          ? err.message
          : "Failed to load graph data. See console for details.";
      graph = null;
      if (gameId === "zelda1" && levelId !== undefined) {
        selectedLevelId = levelId;
      }
    } finally {
      loading = false;
    }

    resetViewport();
  }

  async function onLevelChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    selectedNodeId = null;
    await loadGraph(game, value);
  }

  function clamp(value: number, min: number, max: number) {
    return Math.min(Math.max(value, min), max);
  }

  function resetViewport() {
    zoom = 1;
    viewBoxX = 0;
    viewBoxY = 0;
    isPanning = false;
    activePointerId = null;
    panOrigin = null;
    autoZoom = true;
    fitToViewport();
  }

  $: {
    const signature =
      graph != null
        ? `${graph.game}:${graph.rooms.length}:${graph.nodes.length}`
        : null;
    if (signature !== lastGraphSignature) {
      lastGraphSignature = signature;
      resetViewport();
    }
  }

  function fitToViewport() {
    if (
      !graph ||
      !containerWidth ||
      !containerHeight ||
      viewWidth === 0 ||
      viewHeight === 0
    ) {
      return;
    }

    const widthPadding = containerWidth * 0.9;
    const heightPadding = containerHeight * 0.9;

    const targetZoomWidth = widthPadding > 0 ? viewWidth / widthPadding : 1;
    const targetZoomHeight = heightPadding > 0 ? viewHeight / heightPadding : 1;

    const targetZoom = clamp(
      Math.max(1.5, targetZoomWidth, targetZoomHeight),
      minZoom,
      maxZoom,
    );

    applyZoom(targetZoom, undefined, { fromAuto: true });
  }

  function computeBounds(currentGraph: GraphData) {
    const xs = currentGraph.screens.map((screen) => screen.coordinates.x);
    const ys = currentGraph.screens.map((screen) => screen.coordinates.y);

    if (!xs.length || !ys.length) {
      return {
        minX: 0,
        maxX: 0,
        minY: 0,
        maxY: 0,
      };
    }

    return {
      minX: Math.min(...xs),
      maxX: Math.max(...xs),
      minY: Math.min(...ys),
      maxY: Math.max(...ys),
    };
  }

  function screenRect(screenKey: string | undefined): Rect | null {
    if (!screenKey || !graph) {
      return null;
    }

    const screen = screenMap.get(screenKey);
    if (!screen) {
      return null;
    }

    return {
      x: (screen.coordinates.x - bounds.minX) * SCREEN_WIDTH + SCREEN_MARGIN,
      y: (screen.coordinates.y - bounds.minY) * SCREEN_HEIGHT + SCREEN_MARGIN,
      width: SCREEN_WIDTH,
      height: SCREEN_HEIGHT,
    };
  }

  function buildPositionedNodes(currentGraph: GraphData): PositionedNode[] {
    type PositionedNodeWithRect = PositionedNode & { rect: Rect };

    const positioned = currentGraph.nodes
      .map((node) => {
        const rect = screenRect(node.screenKey);
        if (!rect) {
          return null;
        }

        const { x, y } = resolveNodeCoordinates(node, rect);
        return { node, x, y, rect } satisfies PositionedNodeWithRect;
      })
      .filter((entry): entry is PositionedNodeWithRect => entry !== null);

    const clusters = new Map<string, PositionedNodeWithRect[]>();

    for (const entry of positioned) {
      const clusterKey = `${entry.node.screenKey ?? "global"}:${Math.round(entry.x)}:${Math.round(entry.y)}`;
      const cluster = clusters.get(clusterKey);
      if (cluster) {
        cluster.push(entry);
      } else {
        clusters.set(clusterKey, [entry]);
      }
    }

    clusters.forEach((items) => {
      if (items.length <= 1) {
        return;
      }

      const radius = Math.max(
        8,
        Math.min(items[0].rect.width, items[0].rect.height) * 0.2,
      );

      items.forEach((item, index) => {
        const angle = (index / items.length) * Math.PI * 2;
        const offsetX = Math.cos(angle) * radius;
        const offsetY = Math.sin(angle) * radius;

        item.x = clamp(
          item.x + offsetX,
          item.rect.x + 6,
          item.rect.x + item.rect.width - 6,
        );
        item.y = clamp(
          item.y + offsetY,
          item.rect.y + 6,
          item.rect.y + item.rect.height - 6,
        );
      });
    });

    return positioned.map(
      (entry) =>
        ({
          node: entry.node,
          x: entry.x,
          y: entry.y,
        }) satisfies PositionedNode,
    );
  }

  function resolveNodeCoordinates(
    node: GraphNode,
    rect: {
      x: number;
      y: number;
      width: number;
      height: number;
    },
  ) {
    const tile = node.metadata?.tile as unknown;
    if (isNumberArray(tile) && tile.length === 2) {
      const [tileX, tileY] = tile;
      const x =
        rect.x + ((tileX + 0.5) / ZELDA_REGION_COLS) * rect.width;
      const y =
        rect.y + ((tileY + 0.5) / ZELDA_REGION_ROWS) * rect.height;
      return { x, y };
    }

    const regionFrom = node.metadata?.from as unknown;
    const regionTo = node.metadata?.to as unknown;
    if (
      isNumberArray(regionFrom) &&
      isNumberArray(regionTo) &&
      regionFrom.length === 2 &&
      regionTo.length === 2
    ) {
      const centerX = (regionFrom[0] + regionTo[0] + 1) / 2;
      const centerY = (regionFrom[1] + regionTo[1] + 1) / 2;
      const x = rect.x + (centerX / ZELDA_REGION_COLS) * rect.width;
      const y = rect.y + (centerY / ZELDA_REGION_ROWS) * rect.height;
      return { x, y };
    }

    const direction = node.metadata?.direction as string | undefined;

    let x = rect.x + rect.width / 2;
    let y = rect.y + rect.height / 2;

    const padding = Math.min(rect.width, rect.height) * 0.1;

    if (direction === "Left") {
      x = rect.x + padding;
    } else if (direction === "Right") {
      x = rect.x + rect.width - padding;
    } else if (direction === "Up") {
      y = rect.y + padding;
    } else if (direction === "Down") {
      y = rect.y + rect.height - padding;
    } else if (node.nodeType === "location" || node.nodeType === "item") {
      x = rect.x + rect.width / 2;
      y = rect.y + rect.height / 2;
    }

    return { x, y };
  }

  function buildDrawableEdges(
    currentGraph: GraphData,
    positions: Map<string, Coordinate>,
  ): DrawableEdge[] {
    return currentGraph.edges
      .map((edge) => {
        const fromRect = screenRect(nodeMap.get(edge.from)?.screenKey);
        const toRect = screenRect(nodeMap.get(edge.to)?.screenKey);
        const fromNode = nodeMap.get(edge.from);
        const toNode = nodeMap.get(edge.to);

        if (!fromRect || !toRect || !fromNode || !toNode) {
          return null;
        }

        const start =
          positions.get(fromNode.id) ??
          resolveNodeCoordinates(fromNode, fromRect);
        const end =
          positions.get(toNode.id) ?? resolveNodeCoordinates(toNode, toRect);

        return {
          edge,
          x1: start.x,
          y1: start.y,
          x2: end.x,
          y2: end.y,
        };
      })
      .filter((entry): entry is DrawableEdge => entry !== null);
  }

  function buildRoomShapes(currentGraph: GraphData): RoomShape[] {
    return currentGraph.rooms
      .map((room) => {
        const rects = room.screenKeys
          .map((key) => screenRect(key))
          .filter((rect): rect is Rect => rect !== null);

        if (rects.length === 0) {
          return null;
        }

        const minX = Math.min(...rects.map((rect) => rect.x));
        const minY = Math.min(...rects.map((rect) => rect.y));
        const maxX = Math.max(...rects.map((rect) => rect.x + rect.width));
        const maxY = Math.max(...rects.map((rect) => rect.y + rect.height));

        const padding = 6;
        const shrink = Math.min(padding, (maxX - minX) / 2, (maxY - minY) / 2);

        const rect: Rect = {
          x: minX + shrink,
          y: minY + shrink,
          width: Math.max(0, maxX - minX - shrink * 2),
          height: Math.max(0, maxY - minY - shrink * 2),
        } satisfies Rect;

        const labelX = rect.x + 6;
        const labelY = Math.max(rect.y - 8, SCREEN_MARGIN / 2);

        return {
          roomId: room.id,
          name: room.name,
          area: room.area,
          rect,
          label: {
            x: labelX,
            y: labelY,
          },
        } satisfies RoomShape;
      })
      .filter((shape): shape is RoomShape => shape !== null);
  }

  function buildRegionOverlays(currentGraph: GraphData): RegionOverlay[] {
    const overlays: RegionOverlay[] = [];

    for (const node of currentGraph.nodes) {
      if (node.nodeType !== "region" || !node.screenKey) {
        continue;
      }

      const from = node.metadata?.from as unknown;
      const to = node.metadata?.to as unknown;
      if (
        !isNumberArray(from) ||
        !isNumberArray(to) ||
        from.length !== 2 ||
        to.length !== 2
      ) {
        continue;
      }

      const rect = screenRect(node.screenKey);
      if (!rect) {
        continue;
      }

      overlays.push({
        nodeId: `${node.id}::region`,
        rect: regionRectFromTiles(rect, from, to),
      });
    }

    return overlays;
  }

  function regionRectFromTiles(
    screenRectValue: Rect,
    from: number[],
    to: number[],
  ): Rect {
    const originX =
      screenRectValue.x + (from[0] / ZELDA_REGION_COLS) * screenRectValue.width;
    const originY =
      screenRectValue.y + (from[1] / ZELDA_REGION_ROWS) * screenRectValue.height;
    const width =
      ((to[0] - from[0] + 1) / ZELDA_REGION_COLS) * screenRectValue.width;
    const height =
      ((to[1] - from[1] + 1) / ZELDA_REGION_ROWS) * screenRectValue.height;

    return {
      x: originX,
      y: originY,
      width: Math.max(4, width),
      height: Math.max(4, height),
    };
  }

  function buildConnectionGroups(
    node: GraphNode,
    graphData: GraphData,
  ): ConnectionGroup[] {
    const map = new Map<string, ConnectionGroup>();
    const roomMap = new Map(graphData.rooms.map((room) => [room.id, room]));

    const ensureGroup = (targetId: string): ConnectionGroup => {
      let group = map.get(targetId);
      if (!group) {
        const targetNode = nodeMap.get(targetId);
        const targetRoomId = targetNode?.roomId;
        const targetScreenKey = targetNode?.screenKey;
        const targetRoom = targetRoomId ? roomMap.get(targetRoomId) : undefined;
        const targetScreen = targetScreenKey
          ? screenMap.get(targetScreenKey)
          : undefined;

        const roomLabel = targetRoom
          ? `${targetRoom.area} · ${targetRoom.name}`
          : (targetRoomId ?? undefined);
        const screenLabel = targetScreen?.name ?? targetScreenKey ?? undefined;

        const isExternalRoom =
          targetRoomId !== undefined &&
          node.roomId !== undefined &&
          targetRoomId !== node.roomId;

        const isExternalScreen =
          targetScreenKey !== undefined &&
          node.screenKey !== undefined &&
          targetScreenKey !== node.screenKey;

        group = {
          targetId,
          targetLabel: targetNode?.label ?? targetId,
          outgoing: new Set<string>(),
          incoming: new Set<string>(),
          bidirectional: new Set<string>(),
          roomLabel,
          screenLabel,
          isExternalRoom,
          isExternalScreen,
        } satisfies ConnectionGroup;
        map.set(targetId, group);
      }
      return group;
    };

    for (const edge of graphData.edges) {
      if (edge.from !== node.id && edge.to !== node.id) {
        continue;
      }

      const targetId = edge.from === node.id ? edge.to : edge.from;
      const group = ensureGroup(targetId);

      if (!edge.directed) {
        group.bidirectional.add(edge.weight);
        continue;
      }

      if (edge.from === node.id) {
        group.outgoing.add(edge.weight);
      } else {
        group.incoming.add(edge.weight);
      }
    }

    return Array.from(map.values()).sort((a, b) =>
      a.targetLabel.localeCompare(b.targetLabel),
    );
  }

  function pointerToViewBox(clientX: number, clientY: number) {
    if (!svgElement) {
      return null;
    }

    const rect = svgElement.getBoundingClientRect();
    if (rect.width === 0 || rect.height === 0) {
      return null;
    }

    return {
      x: viewBoxX + ((clientX - rect.left) / rect.width) * effectiveWidth,
      y: viewBoxY + ((clientY - rect.top) / rect.height) * effectiveHeight,
    };
  }

  function applyZoom(
    nextZoom: number,
    focus?: { x: number; y: number },
    options: { fromAuto?: boolean } = {},
  ) {
    const clampedZoom = clamp(nextZoom, minZoom, maxZoom);
    if (clampedZoom === zoom) {
      return;
    }

    if (!options.fromAuto) {
      autoZoom = false;
    }

    const previousWidth = effectiveWidth;
    const previousHeight = effectiveHeight;
    const nextWidth = viewWidth / clampedZoom;
    const nextHeight = viewHeight / clampedZoom;

    let nextViewBoxX = viewBoxX;
    let nextViewBoxY = viewBoxY;

    if (focus) {
      const ratioX =
        previousWidth > 0 ? (focus.x - viewBoxX) / previousWidth : 0.5;
      const ratioY =
        previousHeight > 0 ? (focus.y - viewBoxY) / previousHeight : 0.5;

      nextViewBoxX = focus.x - ratioX * nextWidth;
      nextViewBoxY = focus.y - ratioY * nextHeight;
    } else {
      nextViewBoxX = viewBoxX + (previousWidth - nextWidth) / 2;
      nextViewBoxY = viewBoxY + (previousHeight - nextHeight) / 2;
    }

    zoom = clampedZoom;

    const maxX = Math.max(0, viewWidth - nextWidth);
    const maxY = Math.max(0, viewHeight - nextHeight);

    viewBoxX = clamp(nextViewBoxX, 0, maxX);
    viewBoxY = clamp(nextViewBoxY, 0, maxY);

    if (options.fromAuto) {
      autoZoom = true;
    }
  }

  function handleWheel(event: WheelEvent) {
    if (!graph) {
      return;
    }

    event.preventDefault();

    const focus = pointerToViewBox(event.clientX, event.clientY);
    const zoomFactor = event.deltaY > 0 ? 0.85 : 1.15;
    applyZoom(zoom * zoomFactor, focus ?? undefined);
  }

  function handlePointerDown(event: PointerEvent) {
    if (event.button !== 0 || !svgElement) {
      return;
    }

    const interactiveTarget = (event.target as Element | null)?.closest(
      "[data-logic-node]",
    );
    if (interactiveTarget) {
      return;
    }

    event.preventDefault();

    const rect = svgElement.getBoundingClientRect();
    if (rect.width === 0 || rect.height === 0) {
      return;
    }

    panOrigin = {
      pointerX: event.clientX,
      pointerY: event.clientY,
      viewBoxX,
      viewBoxY,
    };

    isPanning = true;
    autoZoom = false;
    activePointerId = event.pointerId;
    svgElement.setPointerCapture(event.pointerId);
  }

  function handlePointerMove(event: PointerEvent) {
    if (!isPanning || panOrigin === null || !svgElement) {
      return;
    }

    if (event.pointerId !== activePointerId) {
      return;
    }

    const rect = svgElement.getBoundingClientRect();
    if (rect.width === 0 || rect.height === 0) {
      return;
    }

    const scaleX = effectiveWidth / rect.width;
    const scaleY = effectiveHeight / rect.height;

    const dx = (event.clientX - panOrigin.pointerX) * scaleX;
    const dy = (event.clientY - panOrigin.pointerY) * scaleY;

    const maxX = Math.max(0, viewWidth - effectiveWidth);
    const maxY = Math.max(0, viewHeight - effectiveHeight);

    viewBoxX = clamp(panOrigin.viewBoxX - dx, 0, maxX);
    viewBoxY = clamp(panOrigin.viewBoxY - dy, 0, maxY);
  }

  function handlePointerUp(event: PointerEvent) {
    if (!svgElement) {
      return;
    }

    if (activePointerId !== null && event.pointerId === activePointerId) {
      svgElement.releasePointerCapture(event.pointerId);
      activePointerId = null;
      isPanning = false;
      panOrigin = null;
    } else if (
      event.type === "pointerleave" ||
      event.type === "pointercancel"
    ) {
      isPanning = false;
      panOrigin = null;
    }
  }

  function nodeColor(node: GraphNode) {
    const specialCategory = node.metadata?.specialCategory as string | undefined;
    const metaType = node.metadata?.metaType as string | undefined;

    if (specialCategory === "passage") {
      return "#0ea5e9";
    }

    if (specialCategory === "cellar") {
      return "#f97316";
    }

    if (metaType === "Stairs") {
      return "#14b8a6";
    }

    if (metaType === "Armos") {
      return "#d946ef";
    }

    if (node.nodeType === "cave") {
      return "#8b5cf6";
    }

    switch (node.nodeType) {
      case "door":
        return "#2563eb";
      case "exit":
        return "#16a34a";
      case "location":
        return "#f59e0b";
      case "item":
        return "#dc2626";
      default:
        return "#4b5563";
    }
  }

  function edgeColor(edge: GraphEdge) {
    switch (edge.weight) {
      case "fixed":
        return "#9ca3af";
      case "Missile":
        return "#ec4899";
      case "Missile|2":
        return "#db2777";
      case "Missile|3":
        return "#be185d";
      default:
        return "#6b7280";
    }
  }

  function nodeRadius(node: GraphNode) {
    switch (node.nodeType) {
      case "item":
        return 7;
      case "location":
        return 8;
      default:
        return 6;
    }
  }

  function handleNodeKeydown(event: KeyboardEvent, nodeId: string) {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      selectedNodeId = nodeId;
    } else if (event.key === "Escape" && selectedNodeId === nodeId) {
      event.preventDefault();
      selectedNodeId = null;
    }
  }

  function resetViewportAndSelection() {
    resetViewport();
    selectedNodeId = null;
  }

  $: zoomLabel = `${Math.round(zoom * 100)}%`;

  function parseScreenIndex(screenKey: string): number | null {
    const parts = screenKey.split("::");
    const last = parts[parts.length - 1];
    const value = Number.parseInt(last, 10);
    return Number.isInteger(value) ? value : null;
  }

  function isNumberArray(value: unknown): value is number[] {
    return (
      Array.isArray(value) &&
      value.every((entry) => typeof entry === "number" && Number.isFinite(entry))
    );
  }

  function toRecordArray(value: unknown): Record<string, unknown>[] {
    return Array.isArray(value)
      ? (value as Record<string, unknown>[])
      : [];
  }

  function formatWeightSet(weights: Set<string>): string {
    return Array.from(weights.values()).join(", ");
  }
</script>

<section class="page logic-viewer">
  <div class="viewer-container">
    <header>
      <br />
      <h1>Logic Viewer</h1>
      <p>
        Explore the randomizer logic graph. Rooms and screens are positioned
        based on their in-game coordinates, with graph nodes representing doors,
        exits, items, and more.
      </p>
    </header>

    <div class="controls">
      <label for="game-select">Game</label>
      <select
        id="game-select"
        on:change={onGameChange}
        bind:value={game}
        disabled={loading}
      >
        {#each gameOptions as option (option.id)}
          <option value={option.id} disabled={!option.implemented}>
            {option.label}{option.implemented ? "" : " (coming soon)"}
          </option>
        {/each}
      </select>
      {#if levelOptions.length && game === "zelda1"}
        <label for="level-select">Level</label>
        <select
          id="level-select"
          on:change={onLevelChange}
          bind:value={selectedLevelId}
          disabled={loading}
        >
          {#each levelOptions as option (option.id)}
            <option value={option.id}>{option.label}</option>
          {/each}
        </select>
      {/if}
      {#if loading}
        <span class="status">Loading…</span>
      {/if}
      {#if error}
        <span class="status error">{error}</span>
      {/if}
      <div class="zoom-controls" aria-live="polite">
        <button
          type="button"
          class="zoom"
          on:click={() => applyZoom(zoom / 1.2)}
          aria-label="Zoom out"
          disabled={!graph}
        >
          −
        </button>
        <span class="zoom-label">{zoomLabel}</span>
        <button
          type="button"
          class="zoom"
          on:click={() => applyZoom(zoom * 1.2)}
          aria-label="Zoom in"
          disabled={!graph}
        >
          +
        </button>
        <button
          type="button"
          class="reset"
          on:click={resetViewportAndSelection}
          disabled={!graph}
        >
          Reset
        </button>
      </div>
    </div>
  </div>

  {#if graph}
    <div class="viewer">
      <aside class="sidebar">
        {#if selectedNode}
          <div class="panel">
            <h2>{selectedNode.label}</h2>
            <dl>
              <div>
                <dt>Node Type</dt>
                <dd>{selectedNode.nodeType}</dd>
              </div>
              <div>
                <dt>Node Name</dt>
                <dd>{selectedNode.id}</dd>
              </div>
              {#if selectedNode.screenKey}
                <div>
                  <dt>Screen</dt>
                  <dd>{screenMap.get(selectedNode.screenKey)?.name}</dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.direction}
                <div>
                  <dt>Direction</dt>
                  <dd>{selectedNode.metadata.direction as string}</dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.doorType}
                <div>
                  <dt>Door Type</dt>
                  <dd>{selectedNode.metadata.doorType as string}</dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.item}
                <div>
                  <dt>Item</dt>
                  <dd>{selectedNode.metadata.item as string}</dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.metaType}
                <div>
                  <dt>Meta Type</dt>
                  <dd>{selectedNode.metadata.metaType as string}</dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.specialCategory}
                <div>
                  <dt>Category</dt>
                  <dd>
                    {selectedNode.metadata.specialCategory as string}
                    {#if selectedNode.metadata?.passageSide}
                      ({selectedNode.metadata.passageSide as string})
                    {/if}
                  </dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.caveType}
                <div>
                  <dt>Cave Type</dt>
                  <dd>{selectedNode.metadata.caveType as string}</dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.caveName}
                <div>
                  <dt>Cave Name</dt>
                  <dd>{selectedNode.metadata.caveName as string}</dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.caveItems}
                <div>
                  <dt>Cave Items</dt>
                  <dd>
                    {(selectedNode.metadata.caveItems as string[]).join(", ")}
                  </dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.cavePrices}
                <div>
                  <dt>Cave Prices</dt>
                  <dd>
                    {(selectedNode.metadata.cavePrices as number[])
                      .map((price) => `${price} rupees`)
                      .join(", ")}
                  </dd>
                </div>
              {/if}
              {#if selectedNode.metadata?.linkedLevel}
                <div>
                  <dt>Linked Level</dt>
                  <dd>{selectedNode.metadata.linkedLevel as number}</dd>
                </div>
              {/if}
              {#if selectedSpecialCategories.length}
                <div>
                  <dt>Tags</dt>
                  <dd class="meta-group">
                    {#each selectedSpecialCategories as category, index (index)}
                      <span class="meta-chip">
                        {category as string}
                      </span>
                    {/each}
                  </dd>
                </div>
              {/if}
              {#if selectedPassageTargets.length}
                <div>
                  <dt>Passage Targets</dt>
                  <dd class="meta-group">
                    {#each selectedPassageTargets as target, index (index)}
                      <span class="meta-chip">
                        {(target.destination as string) ?? "Unknown"}
                        {#if target.connectorType}
                          &nbsp;· {(target.connectorType as string)}
                        {/if}
                        {#if target.connectorName}
                          &nbsp;({target.connectorName as string})
                        {/if}
                      </span>
                    {/each}
                  </dd>
                </div>
              {/if}
              {#if selectedStairsConnections.length}
                <div>
                  <dt>Stairs Connections</dt>
                  <dd class="meta-group">
                    {#each selectedStairsConnections as connection, index (index)}
                      <span class="meta-chip">
                        {(connection.source as string) ?? "Unknown"}
                        {#if connection.passageType}
                          &nbsp;· {(connection.passageType as string)}
                        {/if}
                        {#if connection.passageNode}
                          &nbsp;({connection.passageNode as string})
                        {/if}
                      </span>
                    {/each}
                  </dd>
                </div>
              {/if}
            </dl>

            {#if connectionGroups.length}
              <div class="edge-list">
                <h3>Connected edges</h3>
                <ul>
                  {#each connectionGroups as group (group.targetId)}
                    <li>
                      <span class="edge-target">
                        {group.targetLabel}
                        {#if group.isExternalRoom || group.isExternalScreen}
                          <span class="edge-target-meta">
                            {#if group.isExternalRoom && group.roomLabel}
                              Room: {group.roomLabel}
                            {/if}
                            {#if group.isExternalScreen && group.screenLabel}
                              <span>
                                Screen: {group.screenLabel}
                              </span>
                            {/if}
                          </span>
                        {/if}
                      </span>
                      <div class="edge-links">
                        {#if group.outgoing.size}
                          <span class="edge-chip">
                            → {formatWeightSet(group.outgoing)}
                          </span>
                        {/if}
                        {#if group.incoming.size}
                          <span class="edge-chip">
                            ← {formatWeightSet(group.incoming)}
                          </span>
                        {/if}
                        {#if group.bidirectional.size}
                          <span class="edge-chip">
                            ↔ {formatWeightSet(group.bidirectional)}
                          </span>
                        {/if}
                      </div>
                    </li>
                  {/each}
                </ul>
              </div>
            {/if}
          </div>
        {:else}
          <div class="panel muted">
            <h2>No node selected</h2>
            <p>
              Click any node in the diagram to inspect its metadata and
              connected edges.
            </p>
          </div>
        {/if}
      </aside>
      <div class="graph-wrapper" bind:this={viewerContainer}>
        <svg
          bind:this={svgElement}
          viewBox={`${viewBoxX} ${viewBoxY} ${effectiveWidth} ${effectiveHeight}`}
          preserveAspectRatio="xMidYMid meet"
          class="graph"
          class:panning={isPanning}
          on:wheel={handleWheel}
          on:pointerdown={handlePointerDown}
          on:pointermove={handlePointerMove}
          on:pointerup={handlePointerUp}
          on:pointerleave={handlePointerUp}
          on:pointercancel={handlePointerUp}
        >
          <defs>
            <marker
              id="arrowhead"
              markerWidth="8"
              markerHeight="6"
              refX="8"
              refY="3"
              orient="auto"
              markerUnits="strokeWidth"
            >
              <path d="M0,0 L8,3 L0,6 z" fill="var(--logic-outline-strong)" />
            </marker>
          </defs>

          {#each roomShapes as room (room.roomId)}
            <g class="room-outline">
              <rect
                x={room.rect.x}
                y={room.rect.y}
                width={room.rect.width}
                height={room.rect.height}
                rx="16"
                ry="16"
              />
              <text
                x={room.label.x}
                y={room.label.y}
                class="room-label"
                text-anchor="start"
                dominant-baseline="hanging"
              >
                {room.area} · {room.name}
              </text>
            </g>
          {/each}

          {#each graph.screens as screen (screen.key)}
            {@const rect = screenRect(screen.key)}
            {#if rect}
              {@const screenIndex = parseScreenIndex(screen.key)}
              <g class="screen">
                <rect
                  class="screen-rect"
                  x={rect.x}
                  y={rect.y}
                  width={rect.width}
                  height={rect.height}
                  rx="8"
                  ry="8"
                />
                <text class="screen-label">
                  <tspan x={rect.x + 8} y={rect.y + 18}>
                    {screen.roomName}
                  </tspan>
                  <tspan x={rect.x + 8} dy="1.1em" class="screen-subline">
                    {screen.screenLabel}
                    {screenIndex !== null ? ` · Screen ${screenIndex}` : ""}
                  </tspan>
                </text>
              </g>
            {/if}
          {/each}

          {#each regionOverlays as overlay (overlay.nodeId)}
            <rect
              class="region-overlay"
              x={overlay.rect.x}
              y={overlay.rect.y}
              width={overlay.rect.width}
              height={overlay.rect.height}
              rx="6"
              ry="6"
            />
          {/each}

          {#each drawableEdges as { edge, x1, y1, x2, y2 } (edge.id)}
            <line
              {x1}
              {y1}
              {x2}
              {y2}
              stroke={edgeColor(edge)}
              stroke-width={selectedNodeId &&
              (edge.from === selectedNodeId || edge.to === selectedNodeId)
                ? 3
                : 1.6}
              marker-end={edge.directed ? "url(#arrowhead)" : undefined}
              class:highlighted={selectedNodeId &&
                (edge.from === selectedNodeId || edge.to === selectedNodeId)}
              opacity={selectedNodeId
                ? edge.from === selectedNodeId || edge.to === selectedNodeId
                  ? 1
                  : 0.2
                : 1}
            />
          {/each}

          {#each positionedNodes as { node, x, y } (node.id)}
            <g
              data-logic-node
              class:active={selectedNodeId === node.id}
              role="button"
              tabindex="0"
              on:click={() => (selectedNodeId = node.id)}
              on:keydown={(event) => handleNodeKeydown(event, node.id)}
            >
              <circle
                cx={x}
                cy={y}
                r={nodeRadius(node)}
                fill={nodeColor(node)}
                stroke="var(--logic-outline-strong)"
                stroke-width="1.5"
              />
              <title>{node.label}</title>
            </g>
          {/each}
        </svg>
      </div>
    </div>
  {:else if !loading}
    <div class="empty-state">
      <p>Select a supported game to load its logic graph.</p>
    </div>
  {/if}
</section>

<style>
  .logic-viewer {
    --logic-surface: #f8fafc;
    --logic-border: #e2e8f0;
    --logic-screen-fill: rgba(59, 130, 246, 0.12);
    --logic-screen-stroke: rgba(59, 130, 246, 0.38);
    --logic-room-fill: rgba(14, 165, 233, 0.12);
    --logic-room-stroke: rgba(14, 165, 233, 0.55);
    --logic-room-label: #0284c7;
    --logic-panel-bg: #ffffff;
    --logic-panel-border: #e2e8f0;
    --logic-text-muted: #64748b;
    --logic-chip-bg: #eef2ff;
    --logic-chip-border: #c7d2fe;
    --logic-chip-text: #312e81;
    --logic-text: #0f172a;
    --logic-text-strong: #0f172a;
    --logic-outline-strong: rgba(15, 23, 42, 0.4);
    color: var(--logic-text);
  }

  :global(.dark) .logic-viewer {
    --logic-surface: rgba(15, 23, 42, 0.75);
    --logic-border: rgba(148, 163, 184, 0.35);
    --logic-screen-fill: rgba(96, 165, 250, 0.18);
    --logic-screen-stroke: rgba(96, 165, 250, 0.45);
    --logic-room-fill: rgba(56, 189, 248, 0.16);
    --logic-room-stroke: rgba(56, 189, 248, 0.55);
    --logic-room-label: #38bdf8;
    --logic-panel-bg: rgba(15, 23, 42, 0.85);
    --logic-panel-border: rgba(148, 163, 184, 0.4);
    --logic-text-muted: #cbd5f5;
    --logic-chip-bg: rgba(59, 130, 246, 0.2);
    --logic-chip-border: rgba(59, 130, 246, 0.4);
    --logic-chip-text: #bfdbfe;
    --logic-text: #e2e8f0;
    --logic-text-strong: #f8fafc;
    --logic-outline-strong: rgba(255, 255, 255, 0.35);
    color: var(--logic-text);
  }

  .page {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
  }

  .viewer-container {
    width: min(1400px, 100%);
    margin: 0 auto;
    padding: 0 1.5rem;
  }

  header h1 {
    font-size: 1.6rem;
    font-weight: 600;
    margin: 0;
    color: var(--logic-text-strong);
  }

  header p {
    margin: 0;
    color: var(--logic-text-muted);
  }

  .controls {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    flex-wrap: wrap;
  }

  .zoom-controls {
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    margin-left: auto;
    padding: 0.25rem 0.5rem;
    border: 1px solid var(--logic-border);
    border-radius: 9999px;
    background: var(--logic-panel-bg);
    box-shadow: 0 10px 20px -18px var(--logic-outline-strong);
  }

  .zoom-controls .zoom,
  .zoom-controls .reset {
    border: none;
    background: none;
    cursor: pointer;
    padding: 0.25rem 0.5rem;
    border-radius: 9999px;
    font-size: 0.95rem;
    font-weight: 600;
    color: var(--logic-text-strong);
  }

  .zoom-controls .zoom:hover,
  .zoom-controls .reset:hover {
    background: rgba(148, 163, 184, 0.18);
  }

  .zoom-controls .zoom:active,
  .zoom-controls .reset:active {
    background: rgba(148, 163, 184, 0.28);
  }

  .zoom-label {
    font-size: 0.85rem;
    font-weight: 600;
    color: var(--logic-text-strong);
    min-width: 3.5ch;
    text-align: center;
  }

  .zoom-controls button:disabled {
    opacity: 0.45;
    cursor: not-allowed;
    background: none;
  }

  select {
    border: 1px solid var(--logic-border);
    border-radius: 0.5rem;
    padding: 0.35rem 0.75rem;
    background: var(--logic-panel-bg);
    font-size: 0.95rem;
    color: var(--logic-text);
  }

  .status {
    font-size: 0.9rem;
    color: var(--logic-text-muted);
  }

  .status.error {
    color: #dc2626;
  }

  .meta-group {
    display: flex;
    flex-wrap: wrap;
    gap: 0.35rem;
  }

  .meta-chip {
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    padding: 0.15rem 0.45rem;
    border-radius: 9999px;
    background: rgba(59, 130, 246, 0.12);
    color: var(--logic-text-strong);
    font-size: 0.78rem;
    font-weight: 500;
  }

  :global(.dark) .logic-viewer .meta-chip {
    background: rgba(96, 165, 250, 0.22);
    color: var(--logic-text);
  }

  .viewer {
    display: grid;
    grid-template-columns: minmax(320px, 360px) minmax(0, 1fr);
    gap: 1rem;
    min-height: calc(100vh - 16rem);
    align-items: stretch;
    margin: 0 auto;
    width: min(1400px, 100%);
    padding: 0 1.5rem 2rem;
  }

  .graph-wrapper {
    position: relative;
    width: 100%;
    min-height: 640px;
    height: clamp(640px, 75vh, 880px);
  }

  .graph {
    width: 100%;
    height: 100%;
    background: var(--logic-surface);
    border: 1px solid var(--logic-border);
    border-radius: 0.75rem;
    touch-action: none;
    cursor: grab;
  }

  .graph.panning {
    cursor: grabbing;
  }

  .screen-rect {
    fill: var(--logic-screen-fill);
    stroke: var(--logic-screen-stroke);
    stroke-width: 1.2;
  }

  .screen-label {
    fill: var(--logic-text-strong);
    font-size: 0.7rem;
    font-weight: 600;
  }

  .screen-subline {
    fill: var(--logic-text-muted);
    font-size: 0.62rem;
    font-weight: 500;
  }

  .region-overlay {
    fill: rgba(59, 130, 246, 0.16);
    stroke: rgba(59, 130, 246, 0.28);
    stroke-width: 1;
  }

  :global(.dark) .logic-viewer .region-overlay {
    fill: rgba(59, 130, 246, 0.24);
    stroke: rgba(96, 165, 250, 0.45);
  }

  .room-outline rect {
    fill: var(--logic-room-fill);
    stroke: var(--logic-room-stroke);
    stroke-width: 1.2;
    stroke-dasharray: 6 5;
    vector-effect: non-scaling-stroke;
  }

  .room-label {
    fill: var(--logic-room-label);
    font-size: 0.66rem;
    font-weight: 600;
    opacity: 0.95;
    paint-order: stroke fill;
    stroke: rgba(255, 255, 255, 0.85);
    stroke-width: 0.6;
    stroke-linejoin: round;
  }

  line {
    stroke-linecap: round;
  }

  line.highlighted {
    stroke-width: 3;
  }

  g.active circle {
    stroke: #f59e0b;
    stroke-width: 2;
  }

  .sidebar {
    display: flex;
    flex-direction: column;
    width: 100%;
  }

  .panel {
    border: 1px solid var(--logic-panel-border);
    border-radius: 0.75rem;
    padding: 1.25rem;
    background: var(--logic-panel-bg);
    box-shadow: 0 10px 25px -15px rgba(15, 23, 42, 0.35);
  }

  .panel h2 {
    margin: 0;
    font-size: 1rem;
    color: var(--logic-text-strong);
  }

  .panel h3 {
    margin-top: 1.25rem;
    margin-bottom: 0.5rem;
    font-size: 0.95rem;
    font-weight: 600;
  }

  .panel p {
    margin: 0.4rem 0 0;
  }

  dl {
    display: grid;
    gap: 0.5rem;
    margin: 1rem 0 0;
  }

  dt {
    font-size: 0.8rem;
    text-transform: uppercase;
    color: var(--logic-text-muted);
  }

  dd {
    margin: 0.15rem 0 0;
    font-size: 0.95rem;
    font-weight: 500;
    color: var(--logic-text);
  }

  .edge-list ul {
    padding-left: 1rem;
    margin: 0;
    display: grid;
    gap: 0.35rem;
    font-size: 0.9rem;
  }

  .edge-target {
    display: block;
    font-weight: 600;
    color: var(--logic-text-strong);
  }

  .edge-target-meta {
    display: block;
    margin-top: 0.2rem;
    font-size: 0.75rem;
    color: var(--logic-text-muted);
    font-weight: 500;
  }

  .edge-target-meta span {
    display: block;
  }

  .edge-links {
    display: flex;
    flex-wrap: wrap;
    gap: 0.4rem;
    margin-top: 0.35rem;
  }

  .edge-chip {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.12rem 0.5rem;
    border-radius: 9999px;
    background: var(--logic-chip-bg);
    color: var(--logic-chip-text);
    font-size: 0.75rem;
    font-weight: 600;
    border: 1px solid var(--logic-chip-border);
  }

  .empty-state {
    border: 1px dashed var(--logic-border);
    border-radius: 0.75rem;
    padding: 2rem;
    text-align: center;
    color: var(--logic-text-muted);
    background: var(--logic-panel-bg);
  }

  @media (max-width: 960px) {
    .viewer {
      grid-template-columns: minmax(0, 1fr);
      padding: 0 1rem 2rem;
    }

    .sidebar {
      order: 0;
    }
  }
</style>
