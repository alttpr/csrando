<script lang="ts">
    interface MapCell {
        x: number;
        y: number;
        area: string;
        role?: string | null;
        screen: string;
        // direction ("up"/"down"/"left"/"right") -> "Scroll" | "Elevator" | "Door:<color>"
        edges: Record<string, string>;
    }

    interface M1MapData {
        cells: MapCell[];
        itemPlacements: Record<string, string>;
    }

    interface Props {
        map: M1MapData;
    }

    let { map }: Props = $props();

    let isExpanded = $state(false);

    const cellSize = 26;
    const pad = 14;

    const bounds = $derived(() => {
        const xs = map.cells.map((c) => c.x);
        const ys = map.cells.map((c) => c.y);
        return {
            minX: Math.min(...xs),
            maxX: Math.max(...xs),
            minY: Math.min(...ys),
            maxY: Math.max(...ys),
        };
    });

    const svgWidth = $derived(
        () => (bounds().maxX - bounds().minX + 1) * cellSize + pad * 2,
    );
    const svgHeight = $derived(
        () => (bounds().maxY - bounds().minY + 1) * cellSize + pad * 2,
    );

    function cx(cell: MapCell): number {
        return pad + (cell.x - bounds().minX) * cellSize;
    }
    function cy(cell: MapCell): number {
        return pad + (cell.y - bounds().minY) * cellSize;
    }

    const areaColors: Record<string, string> = {
        Brinstar: "#4a7abf",
        Norfair: "#bf6a4a",
        Kraid: "#5fa05f",
        Ridley: "#a05fa0",
        Tourian: "#b8b85a",
    };

    const doorColors: Record<string, string> = {
        Blue: "#93c5fd",
        Red: "#ef4444",
        Orange: "#f59e0b",
    };

    const roleMarks: Record<string, string> = {
        Start: "S",
        Boss: "B",
        Item: "•",
        Gate: "G",
        ElevatorTop: "E",
        ElevatorBottom: "E",
        Escape: "x",
        Portal: "P",
        Cap: "",
        DoorTube: "",
    };

    function cellColor(cell: MapCell): string {
        if (cell.role === "Cap") return "#1f2937";
        return areaColors[cell.area] ?? "#374151";
    }

    function doorColor(edge: string): string {
        const color = edge.split(":")[1] ?? "Blue";
        return doorColors[color] ?? doorColors.Blue;
    }

    function itemAt(cell: MapCell): string | null {
        return map.itemPlacements?.[`${cell.x},${cell.y}`] ?? null;
    }

    function tooltip(cell: MapCell): string {
        const parts = [`${cell.area} 0x${cell.screen} (${cell.x},${cell.y})`];
        if (cell.role) parts.push(cell.role);
        const item = itemAt(cell);
        if (item) parts.push(`Item: ${item}`);
        return parts.join(" — ");
    }

    // Wall edges get an outline stroke so scroll-connected runs read as rooms.
    function wallLines(
        cell: MapCell,
    ): Array<{ x1: number; y1: number; x2: number; y2: number }> {
        const x = cx(cell);
        const y = cy(cell);
        const s = cellSize;
        const lines = [];
        if (!cell.edges.up) lines.push({ x1: x, y1: y, x2: x + s, y2: y });
        if (!cell.edges.down)
            lines.push({ x1: x, y1: y + s, x2: x + s, y2: y + s });
        if (!cell.edges.left) lines.push({ x1: x, y1: y, x2: x, y2: y + s });
        if (!cell.edges.right)
            lines.push({ x1: x + s, y1: y, x2: x + s, y2: y + s });
        return lines;
    }
</script>

<div class="mt-4 space-y-3">
    <div
        class="rounded-lg border border-slate-200 bg-slate-50 dark:border-slate-700 dark:bg-slate-800/60"
    >
        <button
            type="button"
            class="flex w-full items-center justify-between px-4 py-2.5 text-left text-sm font-medium text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-700/50 rounded-lg transition-colors"
            onclick={() => (isExpanded = !isExpanded)}
            aria-expanded={isExpanded}
        >
            <span>
                Metroid World Map
                <span
                    class="ml-2 text-xs font-normal text-slate-500 dark:text-slate-400"
                >
                    {map.cells.length} screens
                </span>
            </span>
            <svg
                class="h-4 w-4 transition-transform text-slate-400"
                class:rotate-90={isExpanded}
                viewBox="0 0 20 20"
                fill="currentColor"
                aria-hidden="true"
            >
                <path
                    fill-rule="evenodd"
                    d="M7.293 14.707a1 1 0 0 1 0-1.414L10.586 10 7.293 6.707a1 1 0 0 1 1.414-1.414l4 4a1 1 0 0 1 0 1.414l-4 4a1 1 0 0 1-1.414 0Z"
                    clip-rule="evenodd"
                />
            </svg>
        </button>

        {#if isExpanded}
            <div class="px-4 pb-4 overflow-x-auto">
                <svg
                    width={svgWidth()}
                    height={svgHeight()}
                    xmlns="http://www.w3.org/2000/svg"
                >
                    <rect
                        width={svgWidth()}
                        height={svgHeight()}
                        rx="6"
                        fill="#111827"
                    />

                    <!-- Cells -->
                    {#each map.cells as cell (`${cell.x},${cell.y}`)}
                        <rect
                            x={cx(cell)}
                            y={cy(cell)}
                            width={cellSize}
                            height={cellSize}
                            fill={cellColor(cell)}
                            opacity="0.9"
                        >
                            <title>{tooltip(cell)}</title>
                        </rect>
                    {/each}

                    <!-- Walls -->
                    {#each map.cells as cell (`w${cell.x},${cell.y}`)}
                        {#each wallLines(cell) as line, i (i)}
                            <line
                                x1={line.x1}
                                y1={line.y1}
                                x2={line.x2}
                                y2={line.y2}
                                stroke="#0f172a"
                                stroke-width="1.5"
                            />
                        {/each}
                    {/each}

                    <!-- Doors and elevators -->
                    {#each map.cells as cell (`d${cell.x},${cell.y}`)}
                        {#if cell.edges.left?.startsWith("Door")}
                            <rect
                                x={cx(cell) - 2.5}
                                y={cy(cell) + cellSize / 2 - 4}
                                width="5"
                                height="8"
                                rx="1"
                                fill={doorColor(cell.edges.left)}
                            />
                        {/if}
                        {#if cell.edges.right?.startsWith("Door")}
                            <rect
                                x={cx(cell) + cellSize - 2.5}
                                y={cy(cell) + cellSize / 2 - 4}
                                width="5"
                                height="8"
                                rx="1"
                                fill={doorColor(cell.edges.right)}
                            />
                        {/if}
                        {#if cell.edges.down === "Elevator"}
                            <polygon
                                points={`${cx(cell) + cellSize / 2 - 4},${cy(cell) + cellSize - 5} ${cx(cell) + cellSize / 2 + 4},${cy(cell) + cellSize - 5} ${cx(cell) + cellSize / 2},${cy(cell) + cellSize + 3}`}
                                fill="#fbbf24"
                            />
                        {/if}
                    {/each}

                    <!-- Role marks and items -->
                    {#each map.cells as cell (`r${cell.x},${cell.y}`)}
                        {@const mark = cell.role
                            ? (roleMarks[cell.role] ?? "")
                            : ""}
                        {@const item = itemAt(cell)}
                        {#if item}
                            <circle
                                cx={cx(cell) + cellSize / 2}
                                cy={cy(cell) + cellSize / 2}
                                r="5"
                                fill="#fef08a"
                                stroke="#a16207"
                                stroke-width="1"
                            >
                                <title>{tooltip(cell)}</title>
                            </circle>
                        {:else if mark}
                            <text
                                x={cx(cell) + cellSize / 2}
                                y={cy(cell) + cellSize / 2 + 4}
                                font-size="11"
                                font-weight="bold"
                                font-family="monospace"
                                fill="#f8fafc"
                                text-anchor="middle"
                                pointer-events="none"
                            >
                                {mark}
                            </text>
                        {/if}
                    {/each}
                </svg>

                <!-- Legend -->
                <div
                    class="mt-3 flex flex-wrap gap-x-4 gap-y-1 text-[10px] text-slate-500 dark:text-slate-400"
                >
                    {#each Object.entries(areaColors) as [area, color] (area)}
                        <span class="flex items-center gap-1">
                            <span
                                class="inline-block h-2.5 w-2.5 rounded-sm"
                                style="background:{color}"
                            ></span>
                            {area}
                        </span>
                    {/each}
                    <span class="mx-2 text-slate-300 dark:text-slate-600"
                        >|</span
                    >
                    <span class="flex items-center gap-1">
                        <span
                            class="inline-block h-2.5 w-2.5 rounded-full border border-amber-700"
                            style="background:#fef08a"
                        ></span>
                        Item (hover for name)
                    </span>
                    <span>S start</span>
                    <span>B boss</span>
                    <span>G gate</span>
                    <span>E elevator</span>
                    <span>P portal</span>
                    <span>x escape</span>
                    <span class="mx-2 text-slate-300 dark:text-slate-600"
                        >|</span
                    >
                    {#each Object.entries(doorColors) as [name, color] (name)}
                        <span class="flex items-center gap-1">
                            <span
                                class="inline-block h-2.5 w-1.5 rounded-sm"
                                style="background:{color}"
                            ></span>
                            {name} door
                        </span>
                    {/each}
                </div>
            </div>
        {/if}
    </div>
</div>
