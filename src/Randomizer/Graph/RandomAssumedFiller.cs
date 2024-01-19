namespace Randomizer.Graph;

using System.Diagnostics;

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
        var set_counts = items.ToDictionary(k => k.Key, set => set.Value.SelectMany(x => x.Value).Count());

        var flat_items_a = items
            .SelectMany(set => set.Value.SelectMany(weight => weight.Value
                .Select(item => (Set: set.Key, Weight: weight.Key, Item: item))))
            .ToArray();
        flat_items_a = _prng.Shuffle(flat_items_a).OrderBy(i => i.Weight).ToArray();
        // fix placement groups
        //Array.Sort(flat_items_a, (a, b) => a.Weight - b.Weight);
        var flat_items = flat_items_a.ToList();

        foreach (var item_key in flat_items_a)
        {
            var (item_set, item_weight, item) = item_key;
            if (item_weight > 9000)
            {
                break;
            }

            // When placing an item that is constrained to an item set from a specific world,
            // only add items to the inventory from that world to speed up the search as
            // we don't care to search other worlds.
            flat_items.Remove(item_key);
            var searcher = _randomizer.GetSearcherForInventory(
                flat_items.Where(i => i.Weight <= 9000 && (item_set.World == null || item_set.World == i.Item.World))
                    .Select(i => i.Item)
                    .ToList()
                );
            bool required = item.World.Config.Accessibility != AccessibilityOption.None
                || !searcher.HasFound(item.World.GetItem("Triforce"));
            var locations = searcher.GetEmptyLocationsInSet(item_set, set_counts, required);

            if (!locations.Any())
            {
                throw new Exception($"No locations for `{item}` in set `{item_set}`");
            }

            var location = _prng.GetRandomElement(locations);
            System.Console.WriteLine("[{0}] [{1}] Placing `{2}` in `{3}` ({4}:{5})",
                item_weight,
                required ? "R" : " ",
                item,
                location.Name,
                item_set,
                locations.Count()
            );

            location.Item = item;
            set_counts[item_set]--;
        }

        FastFillItemsInLocations(flat_items);
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

        ItemSetName? current_key = null;
        var searcher = _randomizer.GetSearcherForInventory(Enumerable.Empty<Item>());
        var locations = new List<Vertex>();
        foreach (var (item_set, _, item) in fillItems)
        {
            if (current_key != item_set)
            {
                locations = _prng.Shuffle(searcher.GetEmptyLocationsInSet(item_set, null, false).ToArray()).ToList();
                current_key = item_set;
            }

            var location = locations.LastOrDefault();
            if (location is null)
            {
                System.Console.WriteLine("No Location: `{0}` `{1}`", item, item_set);
                continue;
            }
            location.Item = item;
            locations.Remove(location);
            System.Console.WriteLine("[FF] Placing: `{0}` in `{1}` ({2}:{3})",
                item,
                location.Name,
                item_set,
                locations.Count + 1
            );
        }
    }
}
