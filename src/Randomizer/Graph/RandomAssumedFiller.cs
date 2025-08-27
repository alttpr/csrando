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

        // Fix placement groups and prepare working sets
        // Sort by weight, and randomize only ties to avoid global shuffle cost
        var flatItemsArray = items.ToArray();
        Array.Sort(flatItemsArray, (a, b) => a.Weight.CompareTo(b.Weight));
        // Shuffle equal-weight runs to keep previous semantics (random order within same weight)
        int start = 0;
        while (start < flatItemsArray.Length)
        {
            int end = start + 1;
            int w = flatItemsArray[start].Weight;
            while (end < flatItemsArray.Length && flatItemsArray[end].Weight == w) end++;
            if (end - start > 1)
            {
                // Fisher-Yates within [start, end)
                for (int i = end - 1; i > start; --i)
                {
                    int r = _prng.GetRandomInt(start, i);
                    (flatItemsArray[i], flatItemsArray[r]) = (flatItemsArray[r], flatItemsArray[i]);
                }
            }
            start = end;
        }
        var flatItems = flatItemsArray.ToList();

        int worldsLength = _randomizer.Worlds.Length;
        // Maintain per-world candidate inventories to avoid re-filtering each iteration
        var itemsByWorld = new List<IItem>[worldsLength];
        int approxPerWorld = Math.Max(1, flatItemsArray.Length / Math.Max(1, worldsLength));
        for (int i = 0; i < worldsLength; i++)
        {
            itemsByWorld[i] = new List<IItem>(approxPerWorld + 4);
        }
        foreach (var pi in flatItems)
        {
            if (pi.Weight <= 9000)
                itemsByWorld[pi.Item.World.Id].Add(pi.Item);
        }

        var searchers = new Searcher[worldsLength];
        // Per-world cache of locations by item set for this FillGraph pass
        // Use location count from cached arrays to avoid building lists twice per iteration
        var locationCache = new Dictionary<ItemSetName, List<Vertex>>[worldsLength];
        for (int i = 0; i < worldsLength; ++i)
        {
            searchers[i] = _randomizer.GetSearcherForInventory(
                itemsByWorld[i],
                _randomizer.Worlds[i].Start
            );
            locationCache[i] = new Dictionary<ItemSetName, List<Vertex>>(64);
        }

        int itemsToPlaceCount = itemsByWorld.Sum(l => l.Count);

        foreach (var itemKey in flatItemsArray)
        {
            var (itemSet, itemWeight, item) = itemKey;
            if (itemWeight > 9000)
                break;

            // When placing an item that is constrained to an item set from a specific world,
            // only add items to the inventory from that world to speed up the search as
            // we don't care to search other worlds.
            flatItems.Remove(itemKey);

            // Update the per-world candidate inventory incrementally instead of re-filtering
            var worldId = item.World.Id;
            // Remove the just-placed item from the candidate list
            itemsByWorld[worldId].Remove(item);
            // Recompute the searcher for this world from the updated candidate list
            var updatedInventory = _randomizer.BuildInventoryForItems(itemsByWorld[worldId]);
            searchers[worldId].Recompute(updatedInventory, item.World.Start);
            // Invalidate cached locations for this world due to searcher change
            locationCache[worldId].Clear();

            // Choose world by weighted counts, then realize only that world's locations
            int total = 0;
            var perWorldCounts = new int[_randomizer.Worlds.Length];
            for (int i = 0; i < _randomizer.Worlds.Length; ++i)
            {
                // Populate and cache the actual location arrays up-front; derive counts from them
                if (!locationCache[i].TryGetValue(itemSet, out var cachedLocs))
                {
                    cachedLocs = searchers[i].GetEmptyLocationsInSetList(itemSet, setCounts);
                    // Ensure no capacity growth if we shuffle/remove
                    cachedLocs.EnsureCapacity(cachedLocs.Count);
                    locationCache[i][itemSet] = cachedLocs;
                }
                int c = cachedLocs.Count;
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

            // Fetch locations for chosen world + set from cache
            var worldLocations = locationCache[chosenWorld][itemSet];
            var location = worldLocations[_prng.GetRandomInt(worldLocations.Count)];
            _logger.LogDebug("({Percentage}%) [{Weight}] Placing `{Item}` in `{Location}` ({ItemSet}:{AvailableLocations})",
                (flatItemsArray.Length - flatItems.Count) * 100 / itemsToPlaceCount,
                itemWeight,
                item,
                location,
                itemSet,
                perWorldCounts[chosenWorld]
            );

            location.Item = item;
            location.TrackPlacedItem();
            setCounts[itemSet]--;

            // Invalidate caches impacted by placing into this location
            // 1) remove caches for the used set across all worlds (setCounts changed)
            for (int i = 0; i < worldsLength; i++)
            {
                locationCache[i].Remove(itemSet);
            }
            // 2) remove caches for any sets this location belongs to in its world
            foreach (var s in location.ItemSet)
                locationCache[chosenWorld].Remove(s);
        }

        FastFillItemsInLocations(flatItems);
    }

    /// <summary>
    /// Quickly place items in locations respecting placemenmt groups.
    /// </summary>
    /// <param name="fillItems">Items to be placed</param>
    private void FastFillItemsInLocations(List<PooledItem> fillItems)
    {
        _logger.LogDebug("Fast Filling {ItemCount} items", fillItems.Count);
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
                // Fetch list directly to avoid extra ToList/array snapshots
                locations = searcher.GetEmptyLocationsInSetList(itemSet, null, false);
                locations.EnsureCapacity(locations.Count);
                _prng.ShuffleInPlace(locations);
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
            // Remove last element in O(1) instead of linear search
            if (locations.Count > 0) locations.RemoveAt(locations.Count - 1);
            _logger.LogDebug("[FF] Placing: `{Item}` in `{Location}` ({ItemSet}:{AvailableLocations})",
                item,
                location,
                itemSet,
                locations.Count + 1
            );
        }
    }
}
