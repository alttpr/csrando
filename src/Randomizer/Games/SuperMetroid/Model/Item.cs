namespace Randomizer.Games.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record Item(
    string StartingRoom,
    int StartingNode,
    List<string> StartingItems,
    List<string> StartingFlags,
    List<string> StartingLocks,
    List<StartingResource> StartingResources,
    List<string> ImplicitItems,
    List<UpgradeItem> UpgradeItems,
    List<ExpansionItem> ExpansionItems,
    List<string> GameFlags
);

public record ConsumableResource(string Type);

public record StartingResource(
    ConsumableResource Resource,
    int MaxAmount
);

public record UpgradeItem(
    string Name,
    string Data
);

public record ExpansionItem(
    string Name,
    string Data,
    ConsumableResource Resource,
    int ResourceAmount
);

public class Items
{
    public static List<Item> Parse(string fileName)
    {
        return new List<Item>();
    }
}
