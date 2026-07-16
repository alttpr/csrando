namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;

/// <summary>
/// Inventory information that can affect a concrete reverse traversal. Item
/// counts are capped at the largest capacity threshold referenced by the
/// world's requirements; above that threshold all requirement outcomes are
/// identical.
/// </summary>
internal sealed class ReverseCapabilityKey : IEquatable<ReverseCapabilityKey>
{
    private readonly ushort[] _counts;
    private readonly int _hashCode;

    public ReverseCapabilityKey(ushort[] counts)
    {
        _counts = counts;
        var hash = new HashCode();
        foreach (ushort count in counts)
            hash.Add(count);
        _hashCode = hash.ToHashCode();
    }

    public bool Equals(ReverseCapabilityKey? other) =>
        other != null
        && _hashCode == other._hashCode
        && _counts.AsSpan().SequenceEqual(other._counts);

    public override bool Equals(object? obj) =>
        obj is ReverseCapabilityKey other && Equals(other);

    public override int GetHashCode() => _hashCode;
}

internal sealed class ReverseCapabilityProfile
{
    private readonly IItem[] _items;
    private readonly ushort[] _countCaps;

    private ReverseCapabilityProfile(IItem[] items, ushort[] countCaps)
    {
        _items = items;
        _countCaps = countCaps;
    }

    public static ReverseCapabilityProfile Build(World world)
    {
        var maximumCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var visitedRequirements = new HashSet<Requirement>(
            ReferenceEqualityComparer.Instance);

        foreach (var vertex in world.GetLocations().OfType<Vertex>())
        {
            foreach (var edge in vertex.Edges.OfType<Edge>())
            {
                foreach (var strategy in edge.Strats ?? [])
                    world.RequirementHandler.CollectCapacityThresholds(
                        strategy.Requires, maximumCounts, visitedRequirements);
            }

            foreach (var nodeLock in vertex.Node?.Locks ?? [])
            {
                if (nodeLock.Lock != null)
                    world.RequirementHandler.CollectCapacityThresholds(
                        nodeLock.Lock, maximumCounts, visitedRequirements);
                foreach (var strategy in nodeLock.UnlockStrats ?? [])
                    world.RequirementHandler.CollectCapacityThresholds(
                        strategy.Requires, maximumCounts, visitedRequirements);
            }
        }

        foreach (var weapon in world.JsonData.Weapons.Weapons)
            world.RequirementHandler.CollectCapacityThresholds(
                weapon.UseRequires, maximumCounts, visitedRequirements);

        var items = world.GetAllItems().OrderBy(item => item.Id).Cast<IItem>().ToArray();
        var caps = items.Select(item => checked((ushort)Math.Max(
                1, maximumCounts.GetValueOrDefault(item.Name, 1))))
            .ToArray();
        return new ReverseCapabilityProfile(items, caps);
    }

    public ReverseCapabilityKey CreateKey(Inventory inventory)
    {
        var counts = new ushort[_items.Length];
        for (int index = 0; index < _items.Length; index++)
        {
            counts[index] = (ushort)Math.Min(
                inventory.GetCount(_items[index]), _countCaps[index]);
        }
        return new ReverseCapabilityKey(counts);
    }
}
