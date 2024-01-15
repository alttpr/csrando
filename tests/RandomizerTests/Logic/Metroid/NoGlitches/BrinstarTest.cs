namespace RandomizerTests.Logic.Metroid.NoGlitches;

using RandomizerTests.Logic.Metroid.NoGlitches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TestClass]
public class BrinstarTest : NoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Brinstar - Morph Room - Morph Pedestal (1) - Morph Ball Pedestal", true, new string[] { }],
        
        ["Brinstar - Kraid Elevator Room - Kraid Elevator (0) - Elevator Platform", false, new string[] { }],
        ["Brinstar - Kraid Elevator Room - Kraid Elevator (0) - Elevator Platform", false, new string[] { "Morph" }],
        ["Brinstar - Kraid Elevator Room - Kraid Elevator (0) - Elevator Platform", false, new string[] { "Bombs" }],
        ["Brinstar - Kraid Elevator Room - Kraid Elevator (0) - Elevator Platform", true, new string[] { "Morph", "Bombs" }],

        ["Brinstar - Bomb Room - Chozo Room (1) - Chozo Orb", false, new string[] { }],
        ["Brinstar - Bomb Room - Chozo Room (1) - Chozo Orb", false, new string[] { "Morph" }],
        ["Brinstar - Bomb Room - Chozo Room (1) - Chozo Orb", true, new string[] { "Morph", "Missile" }],

        ["Brinstar - Tourian Elevator Room - Tourian Elevator (0) - Elevator Platform", false, new string[] { }],
        ["Brinstar - Tourian Elevator Room - Tourian Elevator (0) - Elevator Platform", true, new string[] { "Morph", "Bombs", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile", "Missile" }],

    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
