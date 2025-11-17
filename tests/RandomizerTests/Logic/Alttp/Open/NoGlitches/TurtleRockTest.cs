namespace RandomizerTests.Logic.Alttp.Open.NoGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public class TurtleRockTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Turtle Rock - Chain Chomps", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Chain Chomps", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Quake", "UncleSword", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7" }],
        ["Turtle Rock - Chain Chomps", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7" }],

        ["Turtle Rock - Compass Chest", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Compass Chest", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "Hammer", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria" }],

        ["Turtle Rock - Roller Room - Left", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Roller Room - Left", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],

        ["Turtle Rock - Roller Room - Right", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "FireRod" }],

        ["Turtle Rock - Big Chest", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],

        ["Turtle Rock - Big Key Chest", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Big Key Chest", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Big Key Chest", false, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7" }],

        ["Turtle Rock - Crystaroller Room Chest", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "OcarinaActive", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room Chest", true, new string[] { "TurtleRockEntry", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "BigKeyD7" }],

        ["Turtle Rock - Eye Bridge - Bottom Left", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],

        ["Turtle Rock - Eye Bridge - Bottom Right", false, new string[] { "TurtleRockEntry" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", false, new string[] { "TurtleRockEntry", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],

        ["Turtle Rock - Eye Bridge - Top Left", false, new string[] {  }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],

        ["Turtle Rock - Eye Bridge - Top Right", false, new string[] {  }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "Cape", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "CaneOfByrna", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "MirrorShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "TurtleRockEntry", "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],

        ["Turtle Rock - Boss", false, new string[] {  }],
        ["Turtle Rock - Boss", false, new string[] { "TurtleRockEntry", "FireRod", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", false, new string[] { "TurtleRockEntry", "IceRod", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "TurtleRockEntry", "HalfMagic", "Bottle", "IceRod", "FireRod", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "TurtleRockEntry", "HalfMagic", "Bottle", "IceRod", "FireRod", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "TurtleRockEntry", "HalfMagic", "Bottle", "IceRod", "FireRod", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "UncleSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "TurtleRockEntry", "HalfMagic", "Bottle", "IceRod", "FireRod", "Lamp", "Hookshot", "MoonPearl", "TitansMitt", "Hammer", "Quake", "ProgressiveSword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    [TestMethod]
    public void TestMedallions()
    {
        var randomizer = GetRandomizerForConfig([GetWorldConfig()]);

        var world = randomizer.Worlds[0];

        var vertex = world.GetLocation("Turtle Rock - Entry");
        vertex.Item = world.GetExistingItem("TurtleRockEntryEther");

        var inventory = new[] { "Hammer", "MoonPearl", "Ether", "ProgressiveSword", "OcarinaActive", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7" };

        var searcher = randomizer.GetSearcherForInventory(world,inventory.Select(world.GetItem), world.Start);
        Assert.AreEqual(true, searcher.GetVisited().Any(v => v.Name == "Turtle Rock - Big Key Chest"));
    }
}
