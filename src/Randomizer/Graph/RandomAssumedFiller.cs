namespace Randomizer.Graph;

using Microsoft.Extensions.Logging;

// NOTE: same as in ItemPooler, except we cannot reuse aliases this way
using PooledItem = (ItemSetName Set, int Weight, Item Item);

internal sealed class RandomAssumedFiller
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly Randomizer _randomizer;
    private readonly PRNG _prng;

    public RandomAssumedFiller(Randomizer randomizer, PRNG prng)
    {
        _randomizer = randomizer;
        _prng = prng;
    }

    /// <summary>
    /// This fill places items in the first available location that it can
    /// possibly be in, assuming that unplaced items will be reachable. Those
    /// items will then have a smaller set of places that they can be placed.
    /// </summary>
    /// <param name="items">items to be placed</param>
    public void FillGraph(PooledItem[] items)
    {
        var setCounts = items.GroupBy(k => k.Set).ToDictionary(k => k.Key, set => set.Count());

        // fix placement groups
        var flatItemsArray = _prng.Shuffle(items).OrderBy(i => i.Weight).ToArray();
        var flatItems = flatItemsArray.ToList();

        var searchers = new Searcher[_randomizer.Worlds.Length];
        for (int i = 0; i < _randomizer.Worlds.Length; ++i)
        {
            searchers[i] = _randomizer.GetSearcherForInventory(
                flatItems.Where(item => item.Weight <= 9000 && (item.Item.World.Id == i))
                    .Select(i => i.Item)
                    .ToList(),
                _randomizer.Worlds[i]
                );
        }

        int itemsToPlaceCount = flatItems.Where(i => i.Weight <= 9000).Count();

        foreach (var itemKey in flatItemsArray)
        {
            var (itemSet, itemWeight, item) = itemKey;
            if (itemWeight > 9000)
                break;

            // When placing an item that is constrained to an item set from a specific world,
            // only add items to the inventory from that world to speed up the search as
            // we don't care to search other worlds.
            flatItems.Remove(itemKey);

            searchers[item.World.Id] = _randomizer.GetSearcherForInventory(
                flatItems.Where(i => i.Weight <= 9000 && item.World == i.Item.World)
                    .Select(i => i.Item)
                    .ToList(),
                item.World
                );

            var locations = new List<Vertex>();
            for (int i = 0; i < _randomizer.Worlds.Length; ++i)
            {
                bool onlyReachable = _randomizer.Worlds[i].Config.Accessibility != AccessibilityOption.None
                    || !searchers[i].HasFound(_randomizer.Worlds[i].GetItem("Triforce"));
                locations.AddRange(searchers[i].GetEmptyLocationsInSet(itemSet, setCounts, onlyReachable));

                if (_randomizer.Worlds[i].Config.Accessibility != AccessibilityOption.Locations && (item.Type == ItemType.SmallKey || item.Type == ItemType.BigKey))
                {
                    if (_randomizer.Graph.KeyForKeys.TryGetValue(item, out var keyForKeys))
                    {
                        var chests = keyForKeys.Where(v => v.Chest.Item == null && (v.Regions.Count == 0 || v.Regions.Any(v2 => searchers[i].HasVisited(v2)))).Select(v => v.Chest);
                        locations.AddRange(chests);
                    }
                }
            }

            if (locations.Count == 0)
                throw new Exception($"No locations for `{item}` in set `{itemSet}`");

            locations = locations.Where(l => l.CanPlace(item, itemWeight, searchers[item.World.Id])).ToList();

            var location = _prng.GetRandomElement(locations);
            if (location.Game == Game.SuperMetroid)
            {
                while (true)
                {
                    //System.Console.WriteLine("Backtracking: `{0}` in `{1}`", item, location);
                    var backtrackLocations = searchers[item.World.Id].BacktrackSearch(location);
                    //System.Console.WriteLine("Backtrack Location Count: {0}", backtrackLocations.Count);
                    if (backtrackLocations.Any(location => location.Game != Game.SuperMetroid || location.Name.Contains("Ship")))
                    {
                        break;
                    }

                    System.Console.WriteLine("Backtrack Failed: `{0}` in `{1}`", item, location);
                    location = _prng.GetRandomElement(locations);
                }
            }

            _logger.LogInformation("({Percentage}%) [{Weight}] Placing `{Item}` in `{Location}` ({ItemSet}:{AvailableLocations})",
                (flatItemsArray.Length - flatItems.Count) * 100 / itemsToPlaceCount,
                itemWeight,
                item,
                location,
                itemSet,
                locations.Count
            );

            location.Item = item;
            if (location.SubType is not VertexType.Medallion and not VertexType.Refill and not VertexType.Prize)
                location.World.PlacedItemCount++;
            setCounts[itemSet]--;
        }

        FastFillItemsInLocations(flatItems);
    }

    /// <summary>
    /// Quickly place items in locations respecting placemenmt groups.
    /// </summary>
    /// <param name="fillItems">Items to be placed</param>
    private void FastFillItemsInLocations(List<(ItemSetName Set, int Weight, Item Item)> fillItems)
    {
        _logger.LogInformation("Fast Filling {ItemCount} items", fillItems.Count);
        // assure smaller location groups are filled first
        fillItems.Sort((a, b) =>
        {
            int aweight = a.Weight + (a.Set.World == null ? 9999 : 0);
            int bweight = b.Weight + (b.Set.World == null ? 9999 : 0);
            return aweight - bweight;
        });

        ItemSetName? currentKey = null;
        var searcher = _randomizer.GetSearcherForInventory(Enumerable.Empty<Item>());
        var locations = new List<Vertex>();
        foreach (var (itemSet, _, item) in fillItems)
        {
            if (currentKey != itemSet)
            {
                locations = _prng.Shuffle(searcher.GetEmptyLocationsInSet(itemSet, null, false).ToArray()).ToList();
                currentKey = itemSet;
            }

            var location = locations.LastOrDefault();
            if (location is null)
            {
                _logger.LogWarning("No Location: `{Item}` `{ItemSet}`", item, itemSet);
                continue;
            }
            location.Item = item;
            location.World.PlacedItemCount++;
            locations.Remove(location);
            _logger.LogInformation("[FF] Placing: `{Item}` in `{Location}` ({ItemSet}:{AvailableLocations})",
                item,
                location,
                itemSet,
                locations.Count + 1
            );
        }
    }
}
