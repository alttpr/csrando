namespace RandomizerTests.Logic.SuperMetroid.Basic;

[TestClass]
public class NorfairTest : BasicTests
{
    private static readonly string[] _baseItems = new string[]
    {
        "Morph",
        "PowerBomb",
        "PowerBomb",
        "Super",
        "Super",
        "Missile",
        "Missile",
        "ETank",
        "ETank",
        "ETank",
        "ETank",
        "ETank",
        "ETank",
        "ETank",
        "ETank",
        "Reserve",
        "HiJump",
        "Ice",
        "SpeedBooster",
        "Gravity",
        "Charge",
        "Grapple",
        "SpringBall",
        "Spazer",
        "SpaceJump",
        "XRayScope",
        "Bombs",
        "ScrewAttack"
    };

    public static IEnumerable<object[]> TestData => [
        ["Norfair - Bubble Mountain - Left Side - Top Door", false, _baseItems],
        ["Norfair - Bubble Mountain - Left Side - Bottom Door", false, _baseItems],
        ["Norfair - Bubble Mountain - Left Side - Top Middle Door", false, _baseItems],
        ["Norfair - Bubble Mountain - Left Side - Bottom Middle Door", false, _baseItems],
        ["Norfair - Bubble Mountain - Bottom Door", false, _baseItems],
        ["Norfair - Bubble Mountain - Middle Right Door", false, _baseItems],
        ["Norfair - Bubble Mountain - Top Right Door", false, _baseItems],
        ["Norfair - Bubble Mountain - Bottom Right Item", false, _baseItems],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(RandomizerTests.Logic.SuperMetroid.LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
