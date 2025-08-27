namespace Randomizer.Graph;

using Microsoft.Extensions.Logging;

internal sealed class RandomAssumedFiller
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly GameRandomizer _randomizer;
    private readonly PRNG _prng;

    public RandomAssumedFiller(GameRandomizer randomizer, PRNG prng)
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

        // Sort by weight and shuffle ties only (equivalent random tie-breaker, fewer allocations)
        var flatItemsArray = items.ToArray();
        Array.Sort(flatItemsArray, (a, b) => a.Weight.CompareTo(b.Weight));
        int start = 0;
        while (start < flatItemsArray.Length)
        {
            int end = start + 1;
            int w = flatItemsArray[start].Weight;
            while (end < flatItemsArray.Length && flatItemsArray[end].Weight == w) end++;
            if (end - start > 1)
            {
                for (int i = end - 1; i > start; --i)
                {
                    int r = _prng.GetRandomInt(start, i);
                    (flatItemsArray[i], flatItemsArray[r]) = (flatItemsArray[r], flatItemsArray[i]);
                }
            }
            start = end;
        }
        var flatItems = flatItemsArray.ToList();

        var searchers = new Searcher[_randomizer.Worlds.Length];
        for (int i = 0; i < _randomizer.Worlds.Length; ++i)
        {
            searchers[i] = _randomizer.GetSearcherForInventory(
                flatItems.Where(item => item.Weight <= 9000 && (item.Item.World.Id == i))
                    .Select(i => i.Item)
                    .ToList(),
                _randomizer.Worlds[i].Start
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
                item.World.Start
                );

            // Build per-world candidate lists; pick world proportionally, then pick within it
            int total = 0;
            var perWorldCounts = new int[_randomizer.Worlds.Length];
            for (int i = 0; i < _randomizer.Worlds.Length; ++i)
            {
                int c = searchers[i].GetEmptyLocationsInSetCount(itemSet, setCounts);
                perWorldCounts[i] = c;
                total += c;
            }
            if (total == 0)
                throw new Exception($"No locations for `{item}` in set `{itemSet}`");

            int pick = _prng.GetRandomInt(total);
            int chosenWorld = 0;
            for (; chosenWorld < perWorldCounts.Length; ++chosenWorld)
            {
                pick -= perWorldCounts[chosenWorld];
                if (pick < 0) break;
            }
            // Build only the chosen world's candidate list now
            var worldLocations = searchers[chosenWorld].GetEmptyLocationsInSetList(itemSet, setCounts);
            var location = worldLocations[_prng.GetRandomInt(worldLocations.Count)];
            _logger.LogInformation("({Percentage}%) [{Weight}] Placing `{Item}` in `{Location}` ({ItemSet}:{AvailableLocations})",
                (flatItemsArray.Length - flatItems.Count) * 100 / itemsToPlaceCount,
                itemWeight,
                item,
                location,
                itemSet,
                perWorldCounts[chosenWorld]
            );

            location.Item = item;
            location.TrackPlacedItem();
            _randomizer.NotifyPlacement(location);
            setCounts[itemSet]--;
        }

        FastFillItemsInLocations(flatItems);
    }

    /// <summary>
    /// Quickly place items in locations respecting placemenmt groups.
    /// </summary>
    /// <param name="fillItems">Items to be placed</param>
    private void FastFillItemsInLocations(List<PooledItem> fillItems)
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
        var searcher = _randomizer.GetSearcherForInventory([]);
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
            location.TrackPlacedItem();
            _randomizer.NotifyPlacement(location);
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
