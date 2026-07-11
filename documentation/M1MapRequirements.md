# Metroid 1 Map Randomizer Implementation Specification

This document captures the requirements learned from the two current M1 map-randomizer attempts:

- current working tree on `tewtal/m1-map-builder`, especially `src/Randomizer/Games/Metroid/MapGen/`
- WIP commit `1462a588` on `tewtal/m1-map-generation`, especially the older `AbstractLayout`, `MapBuilder`, `WorldLayout`, and diagnostics

The goal is not to preserve either implementation. The goal is to define what a fresh implementation must satisfy to generate maps that the M1 engine, ROM writer, logic graph, and item filler all agree on.

## Scope

The implementation must support full Metroid 1 map randomization inside the existing combo randomizer pipeline.

It must:

- Generate a complete M1 world layout for Brinstar, Norfair, Kraid, Ridley, and Tourian.
- Preserve M1 engine constraints instead of inventing impossible room geometry.
- Rebuild the M1 logic graph from generated room data.
- Write the generated map, room, door/elevator, item, and relevant sprite data to the ROM.
- Keep M1 portals/Combo integration valid when vanilla room names and coordinates no longer exist.
- Provide deterministic output for a seed and retry/fail cleanly when no valid layout can be found.
- Create rooms that are coherent and makes sense from a player perspective by guiding screen assignment.

## Engine Model

M1 is not a generic four-direction room graph.

Hard requirements:

- The world uses one shared 32x32 room grid.
- Empty grid cells must remain engine-empty, equivalent to `0xFF`.
- Areas are cell metadata, not independent maps.
- Doors are only left/right. Up/down doors must never be generated.
- Vertical movement between normal screens is scroll along vertical screens.
- Horizontal movement between normal screens is scroll along horizontal screens.
- A side door is the legal turn between a vertical shaft and a horizontal run.
- The engine has an active scroll axis.
- A scroll transition preserves the active axis.
- A door transition flips the active axis.

### Engine model refinements (verified against the disassembly, 2026-06)

The blanket "door flips the axis" rule above is a simplification. The verified behavior
(Bank07: `DoOneDoorScroll`/`ToggleScroll`/`IsWalkableTile`, `ElevatorStop`):

- Door tiles come in two kinds baked into each screen's tile data, detected on crash:
  tile `$A0` ("scroll toggling door") leaves the axis flipped relative to the room you
  came from; tile `$A1` ("horizontal scrolling door") forces the axis horizontal on exit.
  `$A1` is what makes vanilla item rooms, lairs, the statues chain and direct
  horizontal-to-horizontal door links (e.g. Mother Brain corridor to Bottom Metroids) work.
- Elevator rides force the axis vertical while riding; `ElevatorStop` toggles it on
  up-ride arrival, so down rides leave you vertical and up rides leave you horizontal.
- Single-screen rooms never scroll, so any arrival axis works inside them; they are
  axis-neutral pass-throughs.

The door tile kind is not present in the screen YAML. The practical consequence for map
generation: a door may only be paired with the kind of neighbor (multi-cell vertical run,
multi-cell horizontal run, or single-cell room) that it had in vanilla, because that is
what its tile kind was designed for. Single-cell rooms are safe neighbors for any door.
The implementation derives this "door context" per (screen, side) from the vanilla rooms
data and enforces it during generation and fitting. Examples this catches:

- Brinstar `0x1C` (Kraid elevator) faced a shaft in vanilla and must face one again,
  while `0x0B` (Norfair elevator) faced a corridor and needs a corridor inserted.
- The statues gate `0x2B` must have a corridor on its east side, not a shaft.
- Ridley's lair needs corridors on both sides; Kraid's lair needs the `[cap, 0x1B]`
  mini-shaft plus corridor pattern.
- Every vanilla elevator pair has a `0x01` elevator-shaft spacer between the upper
  elevator room and the lower platform screen; generated pairs should keep one.
- Elevators are vertical transitions between paired elevator cells and move between areas.
- Morph tunnels are not structural connectors for map generation. Treat them as ability-gated screen internals handled by screen logic, not as topology edges.
- Openings facing an empty cell are sealed by the engine and must not create graph edges.
- Not all combinations of exits/doors are available for each area. A model must be built before map generation to build the "puzzle pieces" allowed to be placed for each area when generating the overall map. (This is the tourian case where only a few types of screens exist).

The generator must validate physical reachability with an axis-aware model, not just adjacency or undirected connectivity.

## Screen Catalog Requirements

Before generating any layout, build a normalized catalog from `Games/Metroid/data/Screens`.

For every vanilla screen, classify:

- area
- screen id
- display name
- scroll axis, vertical or horizontal
- connector on each edge: none, scroll, door, elevator, tunnel
- door color by side
- whether it can host item, boss, start, elevator platform, or meta nodes
- item and elevator location node names
- solid-wall cap status
- one-way scroll status

Validation requirements:

- Catalog loading must cover all five playable areas plus meta screens.
- No screen may expose an up/down door. If YAML ever does, fail during catalog construction.
- Scroll exits must match screen axis: vertical screens scroll up/down, horizontal screens scroll left/right.
- Elevator exits must be vertical only.
- Each playable area must have at least one usable vertical shaft body.
- Do not require every area to have a cap screen. Tourian currently lacks a normal solid-wall cap.
- One-way scroll screens must be detected from directed-only scroll-to-scroll edges. Do not use one-way screens as generic two-way shaft or corridor bodies.

Known landmark expectations:

- Brinstar `0x09` is the start screen and is horizontal with left/right scroll.
- Brinstar `0x2B` is the statues/Silver Two gate.
- Brinstar `0x2C` is the Tourian elevator room.
- Kraid `0x1D` is Kraid's lair, boss plus item, with a red side door.
- Ridley `0x12` is Ridley's lair.
- Tourian `0x04` is Mother Brain.
- Tourian `0x0F`/`0x10` are the escape shaft pieces.
- Ridley `0x04` is a one-way fall shaft and must not be used as a generic two-way body.

## Topology Requirements

Generate an abstract layout before choosing concrete screens.

Each abstract cell must store:

- coordinate
- area
- required axis
- role, such as shaft, horizontal, item, boss, start, elevator, gate, escape
- required connector per edge
- required door color per door edge
- optional forced screen id for fixed landmarks

Abstract links must always be symmetric:

- scroll link means both adjacent cells require matching scroll exits
- door link means both adjacent cells require matching side doors
- elevator link means both adjacent cells require matching up/down elevator exits

Do not create a cell first and repair connectivity later. Every committed cell should be attached to an existing component, or the whole planned feature should be abandoned before mutation.

The topology must satisfy:

- Every non-cap cell is reachable from the generated start under axis-aware physical traversal.
- Every area is one connected component through legal scroll/door/elevator traversal, except explicitly non-progression post-Mother-Brain escape visuals if implemented outside normal traversal.
- Every generated item location is physically reachable with full movement/resources.
- Kraid, Ridley, and Mother Brain exist and are physically reachable.
- Tourian access is gated through the statues room on the DefeatedSilverTwo requirement.
- All inter-area elevator links exist exactly once unless a deliberate setting changes topology.

Recommended elevator topology:

- Brinstar to Norfair
- Brinstar to Kraid
- Brinstar to Tourian through the statues gate
- Norfair to Ridley

The implementation may use spatial elevator pairing or explicit destination metadata, but there must be one source of truth and the logic graph plus ROM writer must follow the same rule. The WIP branch showed explicit `destination` metadata on elevator sprites; the current branch shows spatial vertical adjacency. Either is acceptable only if it is consistently emitted, validated, and written.

## Screen Fitting Requirements

After generating abstract topology, assign concrete vanilla screens.

Screen fitting must:

- Use only screens from the same area as the cell.
- Match the cell axis.
- Support the cell role. For example, item cells require a screen with item locations, boss cells require boss locations, start requires the start location, and elevator cells require elevator platform locations.
- Provide every required connector on the exact required edge.
- Preserve required door colors where color matters.
- Reject one-way screens for cells that require two-way traversal along both ends of the scroll axis.
- Prefer fewer extra openings, but allow extra openings if they are safe.

Extra openings are safe only if:

- they face an empty cell and the graph builder emits no edge, or
- they face a cap/inert cell that cannot create unintended progression, or
- they intentionally connect to a matching neighbor required by topology.

The fitter must produce diagnostics on failure:

- empty-domain cells with coordinate, area, role, axis, and required connectors
- cells assigned a screen whose actual connectors violate required connectors
- graph/axis reachability mismatch details

## Room Decomposition Requirements

The generated grid must be converted into `YamlReader.Room` objects because `YamlReader.BuildRoom` expects room-local scroll chains.

Rules:

- A room is a maximal scroll-run of same-area cells along one axis.
- Horizontal scroll-runs become horizontal rooms with `position = first cell` and `screens[]` ordered left to right.
- Vertical scroll-runs become vertical rooms with `position = first cell` and `screens[]` ordered top to bottom.
- Door-connected cells must be in separate rooms, then reconnected by spatial door lookup.
- Elevator-connected cells may be separate rooms, but graph and ROM emission must agree on pairing.
- The generated room list must replace all non-Meta vanilla rooms.
- The Meta room must be preserved so existing meta/win logic still exists.

Important graph-builder requirement:

- Generated map graph building must not skip/collapse trivial pass-through scroll screens. The old WIP exposed this as `SkipTrivialRooms = false`; the current attempt uses `!config.MapShuffle`. Collapsing pass-through screens in generated scroll-runs can sever graph chains.

Door lookup requirements:

- When a door faces a neighboring cell with no matching back door, do not throw.
- Treat it as sealed: emit no edge.
- If a door is part of required topology, validation must ensure the neighbor has the matching back door before graph build.

## Logic Graph Requirements

The logic graph must be rebuilt from the generated rooms.

Graph requirements:

- The start vertex must connect to the generated start location, not the vanilla Morph Room name.
- The Meta location must remain connected.
- Item sprites must create item vertices with the M1 item set.
- Elevator platform/location sprites must create graph vertices needed by screen logic.
- Door edges must use correct requirements:
  - blue door: fixed
  - red door: Missile
  - purple door: Missile|2
  - orange door: Missile|3
- Boss defeat and win logic must remain compatible with generated boss rooms.
- The Tourian elevator/statues gate must require DefeatedSilverTwo before Mother Brain/Tourian progression.
- A full-inventory search from generated start must be able to reach DefeatedSilverTwo and complete M1.

The implementation must include a graph-vs-physical reachability comparison:

- Run the axis-aware physical solver from the generated start.
- Run the real `Searcher` with generous/full inventory.
- Map visited graph vertices back to grid coordinates.
- Fail if graph reachability misses physically reachable required cells, bosses, elevators, or item locations.

The old branch diagnostics showed large differences between physical/abstract connectivity and graph connectivity. This comparison is a required acceptance test, not optional debugging.

## ROM Writing Requirements

The current working tree only mutates in-memory YAML data and writes item bytes plus existing `PatchData`. A working implementation must also write the generated map to ROM.

The ROM writer must emit, or patch existing tables for:

- the 32x32 area/screen grid used by M1 room lookup
- generated room screen ids at their coordinates
- empty cells as `0xFF`
- generated item sprite placements
- generated elevator sprites and destination/pairing data
- generated boss/landmark placements if their room data is table-driven
- any portal rooms or combo transition doors needed for M1 in combo mode
- any minimap/map-display data if the UI depends on vanilla coordinates

ROM emission must be validated against the same generated `WorldGrid` and `Room` list that built the logic graph. Do not generate one map for logic and independently derive a different map for ROM.

### Address/format inventory (verified against the disassembly and multirando-asm, 2026-06)

The M1 NES PRG is embedded in the combo ROM with NES bank `n` at PC `0x680000 + n*0x8000`
(each SNES bank holds one 16KB area bank at `$8000-$BFFF` plus the fixed bank at `$C000`,
see `asm/multirando-asm/src/m1/rom.asm`). Banks: 0 title/map, 1 Brinstar, 2 Norfair,
3 Tourian, 4 Kraid, 5 Ridley.

- **World map grid**: bank 0 `$A53E` = PC `0x68253E`, `0x400` bytes, one screen id per
  cell indexed `y*32 + x`, `$FF` = empty (`GetRoomNum`, Bank07 `$E733`).
- **Special-items tables**: pointer word at `$9598` in each area bank, repointed by
  multirando to the bank-88 window SNES `$98:8000` = PC `0x6C0000` (reserved through
  `$98:9000`). Format (`ScanForItems` `$ED98`): rows sorted ascending by Y —
  `[Y][word next row, $FFFF ends]` then entries sorted ascending by X —
  `[X][offset to next entry's X byte, $FF ends row][payload chain][$00]`. One entry can
  chain several payloads (type byte routes through the handler table at `$EDE2`).
- **Power-up payload**: `[$02, itemId, position]`; position high nibble Y / low nibble X
  in 16px tiles (`PowerUpHandler` `$EE20`). The screen-YAML location `position: [x, y]`
  maps to this byte as `(y << 4) | x`.
- **Elevator payload**: `[$04, data]`; arrival handler `$D8BF`: data bit7 set = up
  elevator (destination Brinstar, except `$84` = up to Norfair, `$8F` = ending), bit7
  clear = destination area index (Norfair 1, Kraid 2, Tourian 3, Ridley 4). Vanilla emits
  three entries per pair: the down byte at the upper room, an `$81` mirror at the same
  coordinate in the lower area's table, and `$80|area` at the platform two cells below.
- **Door payload**: `[$09, info]`; info high nibble `$A`=right/`$B`=left, low 2 bits door
  palette (0 blue, 2 ten-missile, 3 the Mother Brain room type). Only rooms whose object
  blob lacks the door object need these (a handful of Norfair/Tourian screens).
- **Respawn position**: `$95D7` (X) / `$95D8` (Y) in *each* area bank — where continuing
  in that area respawns Samus (the area elevator in vanilla).
- **Item tracking**: multirando flags taken items by map coordinate (`m1_ItemBitArray`,
  bit index `y*32 + x` in `newitems.asm`), so table entry order carries no identity —
  one item per grid cell is the only constraint.
- **Minimap**: the Metroid Plus map hack (`src/m1/mps`) is vendored but not applied — the
  base `metroid.nes` is a stock 8-bank PRG and the build only `incbin`s it, so there is
  no minimap data to regenerate.

This is implemented by `MapGen/RomEmitter.cs` (+ `MapGen/VanillaSpecialItems.cs`, the
transcribed vanilla tables joined to screens so door/enemy/Mother-Brain payloads follow
their screens onto the generated map).

## Portal And Combo Requirements

Vanilla-specific portal patching cannot assume named vanilla rooms such as `Brinstar - Left Vertical Shaft`.

For map shuffle:

- Skip legacy vanilla portal room patches unless the target vanilla room still exists.
- Generate portal anchors as first-class abstract cells or emitted special rooms.
- Ensure portal logic vertices and ROM transitions point to the generated locations.
- In combo mode, M1 start/return behavior must use generated start and generated portal nodes.

### Implemented portal model (2026-06)

- The generator places `CellRole.Portal` anchors (default 1): the vanilla portal room
  0x1F hanging east off a Brinstar shaft cell — exactly the arrangement vanilla combo
  creates, so the engine transition code works unchanged. The cell east of the portal
  room is reserved empty for 0x1F's sealed scroll opening.
- `Metroid.World.PortalAnchors` carries each anchor's graph vertex name plus its ROM
  fields: `RoomWord` (door cell `X<<8|Y`, matched by `SamusInDoor_extended` when leaving),
  `Direction` (`$0004` = east door), `DestinationId` (portal cell `X<<8|Y`, what other
  games target), and `DestinationArgs` (`D|S|area`, see transition_in.asm). The vanilla
  (non-shuffle) path exposes its fixed anchor through the same record.
- The combo layer models every cross-game portal as a `PortalConnection` between two
  `PortalSide`s. All four games' transition rows share one shape —
  `[source row prefix..., partner game index, partner destination id, partner args]` —
  so `PortalWriter` emits every table from the connection list, and randomizing portal
  locations/connections later only means building a different list. SM/ALttP/Z1 sides
  are still the fixed vanilla anchors (`VanillaPortalSides`).
- Combo graph wiring for M1 goes through the anchor's door vertex (both directions), with
  the M1 Meta hub wired directly; the old `alttp start -> M1 start` edge is gone because
  under map shuffle the spawn platform is not freely reachable from the portal.
- Standalone seeds terminate the transition table so the base patch's vanilla entry
  cannot fire from a generated door at the vanilla portal coordinate.

## Tourian Requirements

Tourian should be generated with stricter structure than normal areas.

Requirements:

- Tourian must have a valid entrance from Brinstar gated by the statues/Silver Two requirement.
- Tourian must contain Mother Brain `0x04`.
- Tourian must provide a route from the elevator entrance to Mother Brain using legal M1 connectors.
- Tourian should not rely on general branch/item filler unless the screen vocabulary supports it.
- Escape shaft visuals or logic must be explicitly modeled:
  - If progression ends at Mother Brain, escape can be non-progression visual data.
  - If the game requires escape traversal, the escape shaft must be in graph and ROM data with valid transitions.
- Do not place Tourian door/shaft combinations that no Tourian screen can fit. Its screen vocabulary is thin.

## Generation Strategy Requirements

A fresh implementation can use any generator shape, but it should respect these constraints learned from the attempts:

- Use a retry loop around whole-world generation.
- Retry before committing partial invalid features.
- Prefer region allocation to prevent one area starving another.
- Region boundaries can be soft, but occupied cells must never overlap.
- Grow Brinstar early because it owns multiple outbound elevator links.
- Place elevators before growing lower areas if lower areas seed from elevator entrances.
- Place forced landmarks before filler so they do not get boxed out.
- Fill branches after spines/elevators/bosses exist.
- Target approximate vanilla area size ratios, but physical/graph validity is more important than exact size.
- Avoid layouts where an area consists only of an elevator entrance.

Minimum per-seed generation diagnostics:

- area cell counts
- forced landmark coordinates
- elevator link coordinates
- failed fit domains, if any
- axis-reachable cell count
- graph-reachable cell count
- generated rooms sorted by area/coordinate

## Acceptance Tests

The implementation is not done until these pass over a meaningful seed range, not just seed 1.

Catalog tests:

- catalog loads all areas
- no up/down doors
- scroll exits match axis
- elevators are vertical only
- known landmark screens classify correctly
- one-way scroll screens are flagged

Abstract/generation tests:

- world builds without throwing for many seeds
- all generated areas have nontrivial size
- no occupied coordinate belongs to more than one area
- all forced landmarks exist
- all required elevator links exist exactly once
- the Tourian elevator is gated by the statues room
- every required connector can be fitted by a real screen

Physical reachability tests:

- every non-cap cell is axis-reachable from generated start
- every generated item cell is axis-reachable from generated start with physical rules
- all bosses are axis-reachable
- all elevator destinations are axis-reachable through their links

Logic graph tests:

- generated world builds graph
- generated start vertex resolves
- graph-vs-axis reachability has no required-cell gaps
- full-inventory M1 world is winnable
- KraidDefeated, RidleyDefeated, DefeatedSilverTwo, and Mother Brain/win events are reachable as expected

ROM tests:

- generated ROM table decodes back to the same grid used by logic
- item addresses written by the randomizer correspond to generated item vertices
- elevator transitions in ROM match graph elevator edges
- doors that should be sealed cannot transition in ROM and have no graph edge
- smoke test booting/entering generated M1 if emulator automation is available

## Known Failure Modes From Attempts

Do not repeat these:

- Treating grid adjacency as connectivity. It ignores active scroll axis and door axis flips.
- Letting general generation use impossible connectors, especially up/down doors.
- Placing one-way fall shafts as if they are two-way shafts.
- Building abstract topology that fits no vanilla screen.
- Letting the fitter choose a screen whose extra openings accidentally create or remove graph connectivity.
- Skipping trivial pass-through rooms in generated scroll-runs.
- Hardcoding the vanilla start location after generated rooms replace vanilla rooms.
- Hardcoding vanilla portal room patches when vanilla rooms are absent.
- Letting logic use one elevator pairing rule while ROM uses another.
- Generating only the in-memory logic map without writing the actual ROM map.
- Comparing success only by graph construction. A generated graph can build and still be unwinnable or disconnected.

## Implementation Neutrality

This specification intentionally does not require a particular class layout, phase breakdown, or generation algorithm. A fresh implementation can be procedural, staged, constraint-based, solver-based, template-based, or a hybrid.

The important design constraint is consistency: the same generated map data must drive graph integration, ROM emission, spoiler output, and diagnostics. Any internal structure is acceptable if it satisfies the engine constraints, graph/ROM agreement, and acceptance tests above.
