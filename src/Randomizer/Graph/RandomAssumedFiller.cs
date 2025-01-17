namespace Randomizer.Graph;

using Microsoft.Extensions.Logging;
using Randomizer.Games.SuperMetroid;

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

        // fix placement groups
        var flatItemsArray = _prng.Shuffle(items).OrderBy(i => i.Weight).ToArray();
        var flatItems = flatItemsArray.ToList();

        var searchers = new ISearcher[_randomizer.Worlds.Length];
        for (int i = 0; i < _randomizer.Worlds.Length; ++i)
        {
            searchers[i] = _randomizer.GetSearcherForInventory(
                _randomizer.Worlds[i],
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
                _randomizer.Worlds[item.World.Id],
                flatItems.Where(i => i.Weight <= 9000 && item.World == i.Item.World)
                    .Select(i => i.Item)
                    .ToList(),
                _randomizer.Worlds[item.World.Id].Start
                );

            var locations = new List<Vertex>();
            for (int i = 0; i < _randomizer.Worlds.Length; ++i)
            {
                locations.AddRange(_randomizer.Worlds[i].GetEmptyLocationsInSet(searchers[i], item, itemSet, setCounts));
            }

            if (locations.Count == 0)
                throw new Exception($"No locations for `{item}` in set `{itemSet}`");

            bool backtrackCheck = false;
            var location = _prng.GetRandomElement(locations);
            while (!backtrackCheck)
            {
                if (location.World is Games.SuperMetroid.World)
                {
                    var statefulSearcher = searchers[location.World.Id] switch
                    {
                        Games.SuperMetroid.StatefulSearcher s => s,
                        Games.Combo.ComboSearcher c => c.SMSearcher,
                        _ => throw new Exception("Invalid searcher type")
                    };

                    var backtrackItems = flatItems.Where(i => i.Weight <= 9000 && item.World == i.Item.World)
                            .Select(i => i.Item)
                            .ToList();
                    var backtrackInventory = new Inventory(backtrackItems.ToArray());
                    backtrackCheck = statefulSearcher.BacktrackLocation((Games.SuperMetroid.Vertex)location, backtrackInventory, (Games.SuperMetroid.Vertex)location.World.Start, item);
                    if (!backtrackCheck)
                    {
                        locations.Remove(location);
                        if (locations.Count == 0)
                        {
                            throw new Exception($"No valid locations for `{item}` in set `{itemSet}`");
                        }
                        location = _prng.GetRandomElement(locations);
                        continue;
                    }
                } else
                {
                    backtrackCheck = true;
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
            location.TrackPlacedItem();
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
        var searcher = _randomizer.GetSearcherForInventory(_randomizer.Worlds[0], [], _randomizer.Worlds[0].Start);
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
