<script lang="ts">
    interface RoomData {
        x: number;
        y: number;
        roles: string[];
        doors: Record<string, string>;
        enemy: string | null;
        enemyCount: number;
        segment: number;
        screen: string;
        connectedTo: number[][] | null;
    }

    interface DungeonData {
        level: number;
        width: number;
        height: number;
        rooms: RoomData[];
        itemPlacements: Record<string, string>;
    }

    interface Props {
        dungeons: DungeonData[];
    }

    let { dungeons }: Props = $props();

    let expandedDungeons = $state<Set<number>>(new Set());

    function toggleDungeon(level: number) {
        const next = new Set(expandedDungeons);
        if (next.has(level)) next.delete(level);
        else next.add(level);
        expandedDungeons = next;
    }

    // Layout constants
    const roomW = 88;
    const roomH = 56;
    const gap = 10;
    const cellW = roomW + gap;
    const cellH = roomH + gap;
    const pad = 16;

    function svgWidth(d: DungeonData): number {
        return d.width * cellW + pad * 2 - gap;
    }
    function svgHeight(d: DungeonData): number {
        return d.height * cellH + pad * 2 - gap;
    }

    function roomColor(room: RoomData): string {
        if (room.roles.includes("Start")) return "#22c55e";
        if (room.roles.includes("Boss")) return "#dc2626";
        if (room.roles.includes("End")) return "#b91c1c";
        if (room.roles.includes("Item")) return "#eab308";
        if (room.roles.includes("Connector")) return "#06b6d4";
        if (room.roles.includes("Cellar")) return "#8b5cf6";
        if (room.roles.includes("Stairs")) return "#7c3aed";
        return "#374151";
    }

    function doorColor(type: string): string {
        switch (type) {
            case "Open":
                return "#e5e7eb";
            case "Locked":
            case "Locked2":
                return "#fbbf24";
            case "Bombable":
                return "#9ca3af";
            case "Shutter":
                return "#1f2937";
            case "PassThrough":
            case "PassThroughNoSound":
                return "#60a5fa";
            default:
                return "#4b5563";
        }
    }

    function doorLabel(type: string): string {
        switch (type) {
            case "Locked":
                return "K";
            case "Locked2":
                return "K2";
            case "Bombable":
                return "B";
            case "Shutter":
                return "S";
            default:
                return "";
        }
    }

    function doorLabelColor(type: string): string {
        return type === "Shutter" ? "#e5e7eb" : "#111827";
    }

    function roleAbbrev(room: RoomData): string {
        if (room.roles.includes("Start")) return "START";
        if (room.roles.includes("Boss")) return "BOSS";
        if (room.roles.includes("End")) return "END";
        if (room.roles.includes("Item")) return "ITEM";
        if (room.roles.includes("Connector")) return "CONN";
        if (room.roles.includes("Stairs")) return "STRS";
        if (room.roles.includes("Cellar")) return "CELR";
        return "";
    }

    function roleAbbrevColor(room: RoomData): string {
        if (room.roles.includes("Start")) return "#bbf7d0";
        if (room.roles.includes("Boss")) return "#fca5a5";
        if (room.roles.includes("End")) return "#fca5a5";
        if (room.roles.includes("Item")) return "#fef08a";
        return "#e5e7eb";
    }

    function getItemName(
        dungeon: DungeonData,
        room: RoomData,
    ): string | null {
        const key = `${room.x},${room.y}`;
        return dungeon.itemPlacements?.[key] ?? null;
    }

    // Get cellar connection lines for a dungeon
    function getCellarConnections(rooms: RoomData[]): Array<{
        cellar: RoomData;
        target: number[];
    }> {
        const connections: Array<{ cellar: RoomData; target: number[] }> = [];
        for (const room of rooms) {
            if (room.connectedTo) {
                for (const target of room.connectedTo) {
                    connections.push({ cellar: room, target });
                }
            }
        }
        return connections;
    }

    function truncate(name: string, maxLen: number = 12): string {
        if (name.length <= maxLen) return name;
        return name.slice(0, maxLen - 1) + "\u2026";
    }
</script>

{#if dungeons.length > 0}
    <div class="mt-4 space-y-3">
        <h4
            class="text-[12px] font-semibold uppercase tracking-wide text-slate-600 dark:text-slate-300 flex items-center gap-3"
        >
            <span class="inline-flex h-5 w-1 rounded bg-emerald-500"></span>
            Zelda 1 Dungeon Maps
        </h4>

        {#each dungeons as dungeon (dungeon.level)}
            {@const isExpanded = expandedDungeons.has(dungeon.level)}
            <div
                class="rounded-lg border border-slate-200 bg-slate-50 dark:border-slate-700 dark:bg-slate-800/60"
            >
                <button
                    type="button"
                    class="flex w-full items-center justify-between px-4 py-2.5 text-left text-sm font-medium text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-700/50 rounded-lg transition-colors"
                    onclick={() => toggleDungeon(dungeon.level)}
                    aria-expanded={isExpanded}
                >
                    <span>
                        Dungeon {dungeon.level}
                        <span
                            class="ml-2 text-xs font-normal text-slate-500 dark:text-slate-400"
                        >
                            {dungeon.rooms.length} rooms &middot;
                            {dungeon.width}&times;{dungeon.height} grid
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
                            width={svgWidth(dungeon)}
                            height={svgHeight(dungeon)}
                            xmlns="http://www.w3.org/2000/svg"
                            class="dungeon-svg"
                        >
                            <!-- Grid background -->
                            <rect
                                width={svgWidth(dungeon)}
                                height={svgHeight(dungeon)}
                                rx="6"
                                fill="#111827"
                                class="dark:fill-[#0f172a]"
                            />

                            <!-- Cellar connection lines -->
                            {#each getCellarConnections(dungeon.rooms) as conn (`${conn.cellar.x},${conn.cellar.y}->${conn.target[0]},${conn.target[1]}`)}
                                {@const cx =
                                    pad +
                                    conn.cellar.x * cellW +
                                    roomW / 2}
                                {@const cy =
                                    pad +
                                    conn.cellar.y * cellH +
                                    roomH / 2}
                                {@const tx =
                                    pad +
                                    conn.target[0] * cellW +
                                    roomW / 2}
                                {@const ty =
                                    pad +
                                    conn.target[1] * cellH +
                                    roomH / 2}
                                <line
                                    x1={cx}
                                    y1={cy}
                                    x2={tx}
                                    y2={ty}
                                    stroke="#c084fc"
                                    stroke-width="2"
                                    stroke-dasharray="5,4"
                                    opacity="0.7"
                                />
                            {/each}

                            <!-- Rooms -->
                            {#each dungeon.rooms as room (room.x + "," + room.y)}
                                {@const rx = pad + room.x * cellW}
                                {@const ry = pad + room.y * cellH}

                                <!-- Room background -->
                                <rect
                                    x={rx}
                                    y={ry}
                                    width={roomW}
                                    height={roomH}
                                    rx="3"
                                    fill={roomColor(room)}
                                    opacity="0.85"
                                />

                                <!-- Screen ID (top-left) -->
                                <text
                                    x={rx + 4}
                                    y={ry + 11}
                                    font-size="9"
                                    font-family="monospace"
                                    fill="#e5e7eb"
                                    opacity="0.8"
                                >
                                    {room.screen}
                                </text>

                                <!-- Segment badge (top-right) -->
                                {#if room.segment >= 0}
                                    <text
                                        x={rx + roomW - 4}
                                        y={ry + 11}
                                        font-size="8"
                                        font-family="sans-serif"
                                        fill="#e5e7eb"
                                        opacity="0.6"
                                        text-anchor="end"
                                    >
                                        S{room.segment}
                                    </text>
                                {/if}

                                <!-- Role label (center-top) -->
                                {@const abbrev = roleAbbrev(room)}
                                {@const itemName = getItemName(dungeon, room)}
                                {#if abbrev}
                                    <text
                                        x={rx + roomW / 2}
                                        y={ry + (itemName ? roomH / 2 - 8 : roomH / 2 - 2)}
                                        font-size="10"
                                        font-weight="bold"
                                        font-family="sans-serif"
                                        fill={roleAbbrevColor(room)}
                                        text-anchor="middle"
                                    >
                                        {abbrev}
                                    </text>
                                {/if}

                                <!-- Item name (center, below role) -->
                                {#if itemName}
                                    <text
                                        x={rx + roomW / 2}
                                        y={ry + roomH / 2 + 4}
                                        font-size="8"
                                        font-weight="bold"
                                        font-family="sans-serif"
                                        fill="#fef9c3"
                                        text-anchor="middle"
                                    >
                                        {truncate(itemName, 14)}
                                    </text>
                                {/if}

                                <!-- Enemy info (bottom) -->
                                {#if room.enemy}
                                    <text
                                        x={rx + roomW / 2}
                                        y={ry + roomH - 5}
                                        font-size="7.5"
                                        font-family="sans-serif"
                                        fill="#e5e7eb"
                                        text-anchor="middle"
                                        opacity="0.9"
                                    >
                                        {truncate(room.enemy)}
                                        x{room.enemyCount}
                                    </text>
                                {/if}

                                <!-- Doors -->
                                {#each Object.entries(room.doors) as [dir, type] (dir)}
                                    {@const dSize = 14}
                                    {@const dHalf = dSize / 2}
                                    {@const dx =
                                        dir === "N" || dir === "S"
                                            ? rx + roomW / 2 - dHalf
                                            : dir === "W"
                                              ? rx - gap / 2 - dHalf
                                              : rx + roomW + gap / 2 - dHalf}
                                    {@const dy =
                                        dir === "W" || dir === "E"
                                            ? ry + roomH / 2 - dHalf
                                            : dir === "N"
                                              ? ry - gap / 2 - dHalf
                                              : ry + roomH + gap / 2 - dHalf}

                                    <rect
                                        x={dx}
                                        y={dy}
                                        width={dSize}
                                        height={dSize}
                                        rx="2"
                                        fill={doorColor(type)}
                                        stroke="#000"
                                        stroke-width="0.5"
                                    />
                                    {@const label = doorLabel(type)}
                                    {#if label}
                                        <text
                                            x={dx + dHalf}
                                            y={dy + dHalf + 3}
                                            font-size="8"
                                            font-weight="bold"
                                            font-family="sans-serif"
                                            fill={doorLabelColor(type)}
                                            text-anchor="middle"
                                        >
                                            {label}
                                        </text>
                                    {/if}
                                {/each}
                            {/each}
                        </svg>

                        <!-- Legend -->
                        <div
                            class="mt-3 flex flex-wrap gap-x-4 gap-y-1 text-[10px] text-slate-500 dark:text-slate-400"
                        >
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#22c55e"
                                ></span>
                                Start
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#dc2626"
                                ></span>
                                Boss
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#b91c1c"
                                ></span>
                                End
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#eab308"
                                ></span>
                                Item
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#06b6d4"
                                ></span>
                                Connector
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#8b5cf6"
                                ></span>
                                Cellar
                            </span>
                            <span class="mx-2 text-slate-300 dark:text-slate-600"
                                >|</span
                            >
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm border border-slate-400"
                                    style="background:#e5e7eb"
                                ></span>
                                Open
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#fbbf24"
                                ></span>
                                Locked
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#9ca3af"
                                ></span>
                                Bombable
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#1f2937"
                                ></span>
                                Shutter
                            </span>
                            <span class="flex items-center gap-1">
                                <span
                                    class="inline-block h-2.5 w-2.5 rounded-sm"
                                    style="background:#60a5fa"
                                ></span>
                                PassThrough
                            </span>
                        </div>
                    </div>
                {/if}
            </div>
        {/each}
    </div>
{/if}
