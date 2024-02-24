# Algorithm

## World/Randomizer setup

1. Ingest all the Nodes from yml files.
2. Modify Nodes and Edges based on config (Order notes listed):

   - GameWinnerer
     - Updates goal locations and places triforce(s) in game.
   - ShopFiller
     - Place all items in shops either majors or regular items.
   - DoorShuffler
     - Currently unused, hook for door rando.
   - EntranceShuffler
     - ?? How do I work?!?!
   - DarknessGraphifier
     - Must run after "EntranceShuffler" and "DoorShuffler" due to changing where dark rooms are in graph.
     - Search the graph for dark rooms and add new nodes and edges for lamp.
   - EnemyShuffler
     - Shuffle Sprite sheets.
     - Shuffle Enemies based on sprite sheets.
   - BossShuffler
     - Must run after "EnemyShuffler" due to potential sprite sheet changes.
     - Shuffle Bosses.
   - BunnyGraphifier
     - Must run after "DarknessGraphifier"
     - Add "Dark" items to the pool.
     - Traverse the graph and flip edges to use "Dark" items so bunny can not traverse.
   - PrizePackShuffler
     - Update prize packs in graph based on user needs.
   - DoorReplacer
     - um?
   - DungeonPegStateCopier
     - Traverse graph and add new nodes and edges based on switches in dungeons.

## Randomization

1. Select item for placement
2. Assume items (everything left except for selected item)
3. Find reachable available empty locations
4. Place Item in available location

## Write Data

See the RomWriter and Rom classes.
