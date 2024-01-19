namespace Randomizer.Graph;

// NOTE: same as in ItemPooler, except we cannot reuse aliases this way
using ItemSet = Dictionary<ItemSetName, /* WeightedSet */ Dictionary<int, List<Item>>>;

internal sealed class RandomAssumedFiller
{
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
    public void FillGraph(ItemSet items)
    {
        var setCounts = items.ToDictionary(k => k.Key, set => set.Value.SelectMany(x => x.Value).Count());

        var flatItemsArray = items
            .SelectMany(set => set.Value.SelectMany(weight => weight.Value
                .Select(item => (Set: set.Key, Weight: weight.Key, Item: item))))
            .ToArray();
        // fix placement groups
        flatItemsArray = _prng.Shuffle(flatItemsArray).OrderBy(i => i.Weight).ToArray();
        var flatItems = flatItemsArray.ToList();

        foreach (var itemKey in flatItemsArray)
        {
            var (itemSet, itemWeight, item) = itemKey;
            if (itemWeight > 9000)
                break;

            // When placing an item that is constrained to an item set from a specific world,
            // only add items to the inventory from that world to speed up the search as
            // we don't care to search other worlds.
            flatItems.Remove(itemKey);
            var searcher = _randomizer.GetSearcherForInventory(
                flatItems.Where(i => i.Weight <= 9000 && (itemSet.World == null || itemSet.World == i.Item.World))
                    .Select(i => i.Item)
                    .ToList()
                );
            bool onlyReachable = item.World.Config.Accessibility != AccessibilityOption.None
                || !searcher.HasFound(item.World.GetItem("Triforce"));
            var locations = searcher.GetEmptyLocationsInSet(itemSet, setCounts, onlyReachable).ToList();
            if (item.World.Config.Accessibility != AccessibilityOption.Locations && (item.Type == ItemType.SmallKey || item.Type == ItemType.BigKey))
            {
                if (_randomizer.Graph.KeyForKeys.TryGetValue(item, out var keyForKeys))
                {
                    var chests = keyForKeys.Where(v => v.Chest.Item == null && (v.Regions.Count == 0 || v.Regions.Any(v2 => searcher.HasVisited(v2)))).Select(v => v.Chest);
                    locations.AddRange(chests);
                }
            }

            if (!locations.Any())
                throw new Exception($"No locations for `{item}` in set `{itemSet}`");

            var location = _prng.GetRandomElement(locations);
            System.Console.WriteLine("[{0}] [{1}] Placing `{2}` in `{3}` ({4}:{5})",
                itemWeight,
                onlyReachable ? "R" : " ",
                item,
                location,
                itemSet,
                locations.Count()
            );

            location.Item = item;
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
        System.Console.WriteLine("Fast Filling {0} items", fillItems.Count);
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
                System.Console.WriteLine("No Location: `{0}` `{1}`", item, itemSet);
                continue;
            }
            location.Item = item;
            locations.Remove(location);
            System.Console.WriteLine("[FF] Placing: `{0}` in `{1}` ({2}:{3})",
                item,
                location,
                itemSet,
                locations.Count + 1
            );
        }
    }
}
