namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[TestClass]
public sealed class IcePalaceTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Ice Palace - Big Key Chest", false, new string[] {  }],
        ["Ice Palace - Big Key Chest", true, new string[] { "ProgressiveGlove", "Hammer", "Hookshot", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "ProgressiveGlove", "CaneOfByrna", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "ProgressiveGlove", "Cape", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "PowerGlove", "Hammer", "Hookshot", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "PowerGlove", "CaneOfByrna", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "PowerGlove", "Cape", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "TitansMitt", "Hammer", "Hookshot", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "TitansMitt", "CaneOfByrna", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Big Key Chest", true, new string[] { "TitansMitt", "Cape", "Hammer", "KeyD5", "KeyD5" }],

        ["Ice Palace - Compass Chest", true, new string[] {  }],

        ["Ice Palace - Map Chest", false, new string[] {  }],
        ["Ice Palace - Map Chest", true, new string[] { "ProgressiveGlove", "Hammer", "Hookshot", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "ProgressiveGlove", "CaneOfByrna", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "ProgressiveGlove", "Cape", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "PowerGlove", "Hammer", "Hookshot", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "PowerGlove", "CaneOfByrna", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "PowerGlove", "Cape", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "TitansMitt", "Hammer", "Hookshot", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "TitansMitt", "CaneOfByrna", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Map Chest", true, new string[] { "TitansMitt", "Cape", "Hammer", "KeyD5", "KeyD5" }],

        ["Ice Palace - Spike Room", false, new string[] {  }],
        ["Ice Palace - Spike Room", true, new string[] { "Hookshot", "KeyD5" }],
        ["Ice Palace - Spike Room", true, new string[] { "KeyD5", "KeyD5" }],

        ["Ice Palace - Freezor Chest", false, new string[] {  }],
        ["Ice Palace - Freezor Chest", true, new string[] { "FireRod" }],
        ["Ice Palace - Freezor Chest", true, new string[] { "Bombos", "UncleSword" }],
        ["Ice Palace - Freezor Chest", true, new string[] { "Bombos", "ProgressiveSword" }],
        ["Ice Palace - Freezor Chest", true, new string[] { "Bombos", "MasterSword" }],
        ["Ice Palace - Freezor Chest", true, new string[] { "Bombos", "L3Sword" }],
        ["Ice Palace - Freezor Chest", true, new string[] { "Bombos", "L4Sword" }],

        ["Ice Palace - Iced T Room", true, new string[] {  }],

        ["Ice Palace - Big Chest", false, new string[] {  }],
        ["Ice Palace - Big Chest", true, new string[] { "BigKeyD5" }],

        ["Ice Palace - Boss", false, new string[] {  }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "FireRod", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "FireRod", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "UncleSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "UncleSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "ProgressiveSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "ProgressiveSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "MasterSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "MasterSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "L3Sword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "L3Sword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "L4Sword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "ProgressiveGlove", "BigKeyD5", "Bombos", "L4Sword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "FireRod", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "FireRod", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "UncleSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "UncleSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "ProgressiveSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "ProgressiveSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "MasterSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "MasterSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "L3Sword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "L3Sword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "L4Sword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "PowerGlove", "BigKeyD5", "Bombos", "L4Sword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "FireRod", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "FireRod", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "UncleSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "UncleSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "ProgressiveSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "ProgressiveSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "MasterSword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "MasterSword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "L3Sword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "L3Sword", "Hammer", "CaneOfSomaria", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "L4Sword", "Hammer", "KeyD5", "KeyD5" }],
        ["Ice Palace - Boss", true, new string[] { "TitansMitt", "BigKeyD5", "Bombos", "L4Sword", "Hammer", "CaneOfSomaria", "KeyD5" }],
    ];
}
