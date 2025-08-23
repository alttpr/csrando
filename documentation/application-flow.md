# CSRandomizer Application Flow Documentation

## Overview
CSRandomizer is a C# application that generates randomized versions of classic games (primarily The Legend of Zelda: A Link to the Past) by manipulating ROM files and game logic. The application uses a graph-based approach to ensure games remain winnable while randomizing item locations and game progression.

## Primary Process Flow

### 1. Application Entry Point
- **Program.cs**: Main entry point using System.CommandLine
- Two primary commands: `randomize` and `assemblebaserom`
- Initializes logging system and handles command execution

### 2. ROM Assembly Process (`assemblebaserom`)
- **AssembleBaseRom.cs**: Prepares base ROM for randomization
- Copies vanilla Japanese ROM to data directory
- Uses ASAR assembler to apply bugfixes and patches
- Supports optional Patreon supporter features
- Outputs `randomizer.sfc` as base ROM

### 3. Randomization Process (`randomize`)

#### 3.1 Configuration Loading
- **Randomize.cs**: Main randomization command handler
- Loads settings from JSON configuration files
- Supports multiworld configurations (multiple players)
- Validates command line parameters and settings

#### 3.2 Randomizer Factory
- **RandomizerFactory.cs**: Creates appropriate game randomizer
- Determines game type from configuration (ALttP, Goonies2, Zelda1)
- Instantiates game-specific randomizer with PRNG seed

#### 3.3 World Generation
- **GameRandomizer.cs**: Abstract base class for all game randomizers
- Creates graph representation of game world
- Establishes starting vertices and connections
- Manages multiple worlds for multiworld support
- Tracks starting items across all worlds

#### 3.4 Graph Construction
- **Graph.cs**: Core data structure representing game world
- Vertices represent locations, rooms, or game states
- Directed edges represent progression paths
- Edges require specific items or conditions to traverse
- Supports item placement and location tracking

#### 3.5 Item Pooling
- **ItemPooler.cs**: Manages available items for placement
- Creates weighted item sets (progressive items, keys, etc.)
- Maps item types to valid placement locations
- Handles game-specific item pools (ALttP, Goonies2, Zelda1)

#### 3.6 Item Placement Algorithm
- **RandomAssumedFiller.cs**: Core randomization algorithm
- Uses "assumed fill" approach for item placement
- Places items in first available valid location
- Assumes unplaced items will be reachable
- Respects placement groups and item weights
- Ensures logical progression through game

#### 3.7 Winability Verification
- **GameRandomizer.IsWinnable()**: Validates generated game
- Performs graph search from starting position
- Verifies all required items are reachable
- Ensures game completion is possible

#### 3.8 ROM Generation
- **RomModifications/Rom.cs**: Handles ROM file manipulation
- Applies game-specific modifications
- Updates checksums and saves final ROM
- Generates spoiler logs for tracking changes

## Key Components

### Graph System
- **Vertex**: Represents game locations, states, or checkpoints
- **Edge**: Represents progression paths with item requirements
- **Searcher**: Performs graph traversal for reachability analysis

### Item Management
- **IItem**: Interface for all game items
- **Inventory**: Tracks available items and their counts
- **ItemCondition**: Defines requirements for edge traversal

### World System
- **IWorld**: Interface for game world representation
- **World**: ALttP-specific world implementation
- **RootWorld**: Connects multiple worlds in multiworld scenarios

### Randomization Engine
- **PRNG**: Seeded random number generator
- **RandomAssumedFiller**: Item placement algorithm
- **ItemPooler**: Item distribution management

## Supported Games

### The Legend of Zelda: A Link to the Past (ALttP)
- Primary supported game
- Complex dungeon and overworld logic
- Multiple item types and progression paths
- Extensive configuration options

### Goonies 2
- Secondary supported game
- Different world structure and item system

### The Legend of Zelda (Zelda 1)
- Classic Zelda game support
- Overworld and dungeon-based progression

## Configuration System
- JSON-based settings files
- Support for single and multiworld configurations
- Game-specific configuration options
- Command-line parameter overrides

## Output Generation
- Randomized ROM files (.sfc format)
- Spoiler logs for tracking changes
- Multiworld support with separate ROMs per player
- Bulk generation capabilities

## Technical Architecture
- **Console Application**: Command-line interface
- **Modular Design**: Game-specific implementations
- **Graph Algorithms**: Pathfinding and reachability
- **ROM Manipulation**: Binary file editing
- **Logging**: Comprehensive operation tracking
- **Error Handling**: Validation and winability checks

## Key Algorithms

### Random Assumed Fill
1. Sort items by weight and placement priority
2. For each item, find all valid placement locations
3. Place item in first available location
4. Update graph state and continue until all items placed
5. Verify final placement maintains winability

### Graph Search
1. Start from initial vertex with starting inventory
2. Traverse edges based on available items
3. Mark reachable vertices
4. Continue until no new vertices accessible
5. Validate all required locations are reachable

This architecture ensures that generated games are always winnable while maintaining the randomness and challenge that players expect from a randomizer.
