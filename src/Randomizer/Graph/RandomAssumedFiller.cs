namespace Randomizer.Graph;

using Microsoft.Extensions.Logging;
using Randomizer.Games.Combo;
using Randomizer.Games.SuperMetroid;
using Randomizer.Games.SuperMetroid.Model;
using static Randomizer.Games.Metroid.YamlReader;

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


        // Do special things for SM in combo
        if (_randomizer.Worlds[0] is Games.Combo.World comboWorld && comboWorld.SMWorld != null)
        {
            if (comboWorld.Config.InitialGame == "sm")
            {
                for (int i = 0; i < _randomizer.Worlds.Length; ++i)
                {
                    var world = _randomizer.Worlds[i];
                    if (world != null)
                    {
                        var smWorld = world switch
                        {
                            Games.SuperMetroid.World sm => sm,
                            Games.Combo.World c => c.SMWorld,
                            _ => null
                        };

                        if (smWorld != null)
                        {
                            SmartFrontFill(smWorld, world.StartingItems, flatItems, 5);

                            // If we didn't fill morph, then fill it
                            if (flatItems.Any(f => f.Item.Name == "Morph"))
                            {
                                var flatMorph = flatItems.First(i => i.Item == smWorld.GetItem("Morph"));
                                SmartFrontFill(smWorld, world.StartingItems, [flatMorph], 1);
                                flatItems.Remove(flatMorph);
                            }
                        }
                    }
                }
            }
            else
            {
                string[] frontFillItemNames = ["Morph"];
                foreach (var frontFillItemName in frontFillItemNames)
                {
                    var flatItemToPlace = flatItems.FirstOrDefault(i => i.Item.Name == frontFillItemName);
                    FrontFillCrossWorld(_randomizer.Worlds[0], _randomizer.Worlds[0].StartingItems, _randomizer.Graph, flatItemToPlace);
                    flatItems.Remove(flatItemToPlace);
                }

            }
        }

        flatItemsArray = flatItems.ToArray();

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
                flatItems.Where(i => i.Weight <= 9000 && item.World.Id == i.Item.World.Id)
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
                        Games.Combo.ComboSearcher { SMSearcher: { } smSearcher } => smSearcher,
                        _ => throw new Exception("Invalid searcher type")
                    };

                    if (statefulSearcher is null)
                        throw new InvalidOperationException("Super Metroid searcher is required for backtracking");

                    var backtrackItems = flatItems.Where(i => i.Weight <= 9000 && item.World.Id == i.Item.World.Id)
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
                }
                else
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

    // Finds a location available with only the starting items and fills it, without checking if it's a good candidate
    private void FrontFillCrossWorld(IWorld world, Inventory startingItems, Graph graph, (ItemSetName, int, IItem) flatItem)
    {
        var searcher = _randomizer.GetSearcherForInventory(world, startingItems.All().Select(x => x.Key), world.Start);
        var locations = searcher.GetEmptyLocationsInSet(flatItem.Item1, null, true).ToList();
        if (locations.Count == 0)
        {
            throw new Exception("No valid location for any item");
        }

        while (locations.Count > 0)
        {
            var location = _prng.GetRandomElement(locations);

            // Backtrack check if the location is in SM
            if (location.World.GameId == "sm")
            {
                // Test backtracking
                var backtrackInventory = startingItems.Clone();
                var statefulSearcher = (StatefulSearcher)location.World.GetSearcherForWorld(graph, location.World.Start, startingItems);
                var backtrackCheck = statefulSearcher.BacktrackLocation((Games.SuperMetroid.Vertex)location, backtrackInventory, (Games.SuperMetroid.Vertex)location.World.Start, flatItem.Item3);

                if (!backtrackCheck)
                {
                    locations.Remove(location);
                    continue;
                }
            }

            location.Item = flatItem.Item3;
            location.TrackPlacedItem();
            _logger.LogInformation("[FF] Placing: `{Item}` in `{Location}` ({ItemSet}:{AvailableLocations})",
                flatItem.Item3,
                location,
                flatItem.Item1,
                locations.Count
            );
            break;
        }

    }


    private void SmartFrontFill(IWorld world, Inventory inventory, List<(ItemSetName, int, IItem)> flatItems, int count)
    {
        int bestLocationCount = 0;
        while (bestLocationCount < count)
        {
            var filteredFlatItems = flatItems.Where(f => f.Item3.World.GameId == world.GameId).ToList();
            var bestLocations = GetBestLocationsForItems(world, filteredFlatItems, inventory);
            if (bestLocations.Count() == 0)
            {
                throw new Exception("No valid location for any item");
            }

            var bestLocation = bestLocations.OrderByDescending(x => x.newLocationCount).First();
            var (itemSet, itemWeight, item) = bestLocation.Item1;
            bestLocation.location.Item = item;
            bestLocation.location.TrackPlacedItem();
            flatItems.Remove(bestLocation.Item1);
            _logger.LogInformation("(0%) [SFF] Placing: `{Item}` in `{Location}` ({ItemSet}:{AvailableLocations})",
                item,
                bestLocation.location,
                itemSet,
                bestLocation.newLocationCount
            );

            bestLocationCount = bestLocation.newLocationCount;
        }
    }

    private IEnumerable<((ItemSetName, int, IItem), Vertex location, int newLocationCount)> GetBestLocationsForItems(IWorld world, List<(ItemSetName, int, IItem)> flatItems, Inventory inventory)
    {
        var placementCandidates = new List<((ItemSetName, int, IItem), Vertex location, int newLocationCount)>();
        foreach (var itemKey in flatItems)
        {
            var (itemSet, itemWeight, item) = itemKey;
            var location = GetBestLocationForItem(world, item, inventory);
            if (location.HasValue)
            {
                placementCandidates.Add((itemKey, location.Value.location, location.Value.newLocationCount));
            }
        }

        return placementCandidates;
    }

    private (Vertex location, int newLocationCount)? GetBestLocationForItem(IWorld world, IItem item, Inventory inventory)
    {
        var searcher = _randomizer.GetSearcherForInventory(world, inventory.All().Select(x => x.Key), world.Start);
        var locations = searcher.GetEmptyLocationsInSet(ItemSetName.DefaultSet, null, true).ToList();
        if (locations.Count == 0)
        {
            return null;
        }
        var locationCandidates = _prng.Shuffle(locations).ToList();
        var locationCandidateResults = new List<(Vertex Location, int newLocations)>();

        while (locationCandidates.Count > 0)
        {
            var locationCandidate = locationCandidates.First();
            locationCandidates.Remove(locationCandidate);

            // Test backtracking
            var backtrackInventory = inventory.Clone();
            var statefulSearcher = (StatefulSearcher)searcher;
            var backtrackCheck = statefulSearcher.BacktrackLocation((Games.SuperMetroid.Vertex)locationCandidate, backtrackInventory, (Games.SuperMetroid.Vertex)locationCandidate.World.Start, item);

            if (!backtrackCheck)
            {
                continue;
            }

            // Verify that placing this item opens up at least one new location
            locationCandidate.Item = item;
            var newSearcher = _randomizer.GetSearcherForInventory(world, inventory.All().Select(x => x.Key), world.Start);
            var newLocations = newSearcher.GetEmptyLocationsInSet(ItemSetName.DefaultSet, null, true).ToList();
            locationCandidate.Item = null;
            locationCandidateResults.Add((locationCandidate, newLocations.Count));
        }

        if (locationCandidateResults.Count == 0)
        {
            return null;
        }

        return locationCandidateResults.OrderBy(x => x.newLocations).First();
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
