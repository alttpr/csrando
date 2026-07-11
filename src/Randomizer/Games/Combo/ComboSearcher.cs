namespace Randomizer.Games.Combo;

using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Games.SuperMetroid;
using Randomizer.Graph;

public class ComboSearcher : ISearcher
{
    private readonly World _world;
    private readonly Graph _graph;
    private readonly Inventory _inventory;

    private readonly ISearcher? _alttpSearcher;
    private readonly ISearcher? _smSearcher;
    private readonly ISearcher? _m1Searcher;
    private readonly ISearcher? _z1Searcher;

    public StatefulSearcher? SMSearcher => _smSearcher as StatefulSearcher;

    public ComboSearcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null)
    {
        _world = (World)start.World;
        _graph = graph;
        _inventory = inventory;

        Inventory prevInventory;
        HashSet<Randomizer.Graph.Vertex> otherWorldVertices = new();


        var initialGame = _world.EffectiveInitialGame;

        switch (initialGame)
        {
            case "alttp":
                _alttpSearcher = _world.AlttpWorld!.GetSearcherForWorld(graph, _world.AlttpWorld.Start, inventory, setLocations);
                otherWorldVertices.UnionWith(_alttpSearcher.GetOtherWorld().ToHashSet());
                break;
            case "sm":
                _smSearcher = _world.SMWorld!.GetSearcherForWorld(graph, _world.SMWorld.Start, inventory, setLocations);
                otherWorldVertices.UnionWith(_smSearcher.GetOtherWorld().ToHashSet());
                break;
            case "m1":
                _m1Searcher = _world.M1World!.GetSearcherForWorld(graph, _world.M1World.Start, inventory, setLocations);
                otherWorldVertices.UnionWith(_m1Searcher.GetOtherWorld().ToHashSet());
                break;
            case "z1":
                _z1Searcher = _world.Z1World!.GetSearcherForWorld(graph, _world.Z1World.Start, inventory, setLocations);
                otherWorldVertices.UnionWith(_z1Searcher.GetOtherWorld().ToHashSet());
                break;
            default:
                throw new ArgumentException("Unknown initial game");
        }

        do
        {
            prevInventory = inventory.Clone();
            // A game's searcher can only be created once another game has reached one of
            // its vertices; until then there is nothing to resume.
            _alttpSearcher = ResumeGame(_world.AlttpWorld, _alttpSearcher, otherWorldVertices, inventory, prevInventory, setLocations);
            _z1Searcher = ResumeGame(_world.Z1World, _z1Searcher, otherWorldVertices, inventory, prevInventory, setLocations);
            _m1Searcher = ResumeGame(_world.M1World, _m1Searcher, otherWorldVertices, inventory, prevInventory, setLocations);
            _smSearcher = ResumeGame(_world.SMWorld, _smSearcher, otherWorldVertices, inventory, prevInventory, setLocations);
        } while (prevInventory.All().Count() != inventory.All().Count());

    }

    private ISearcher? ResumeGame(IWorld? world, ISearcher? searcher, HashSet<Randomizer.Graph.Vertex> otherWorldVertices,
        Inventory inventory, Inventory prevInventory, SetLocations? setLocations)
    {
        if (world == null)
            return searcher;

        var starts = otherWorldVertices.Where(v => v.World == world).ToList();
        if (starts.Count == 0 && searcher == null)
            return null;

        searcher ??= world.GetSearcherForWorld(_graph, starts[0], inventory, setLocations);
        searcher.ResumeSearch(starts, prevInventory);
        otherWorldVertices.UnionWith(searcher.GetOtherWorld().ToHashSet());
        return searcher;
    }

    public bool HasFound(IItem item)
    {
        return _inventory.Has(item);
    }

    public bool HasVisited(Randomizer.Graph.Vertex vertex)
    {
        return vertex switch
        {
            Games.Alttp.Vertex alttpVertex => _alttpSearcher?.HasVisited(alttpVertex) ?? false,
            Games.SuperMetroid.Vertex smVertex => _smSearcher?.HasVisited(smVertex) ?? false,
            Games.Metroid.Vertex m1Vertex => _m1Searcher?.HasVisited(m1Vertex) ?? false,
            Games.Zelda1.Vertex z1Vertex => _z1Searcher?.HasVisited(z1Vertex) ?? false,
            _ => throw new ArgumentException("Unknown vertex type"),
        };
    }

    public void ResumeSearch(IEnumerable<Randomizer.Graph.Vertex> startAt, Inventory prevInventory)
    {
        throw new NotImplementedException();
    }

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetEmptyLocationsInSet(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets, bool onlyReachable)
    {
        HashSet<Randomizer.Graph.Vertex> locations =
        [
            .. _alttpSearcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? [],
            .. _smSearcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? [],
            .. _m1Searcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? [],
            .. _z1Searcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? [],
        ];
        return locations;
    }

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetVisited()
    {
        List<Randomizer.Graph.Vertex> locations =
        [
            .. _alttpSearcher?.GetVisited() ?? [],
            .. _smSearcher?.GetVisited() ?? [],
            .. _m1Searcher?.GetVisited() ?? [],
            .. _z1Searcher?.GetVisited() ?? [],
        ];
        return locations;
    }

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetOtherWorld()
    {
        List<Randomizer.Graph.Vertex> locations =
        [
            .. _alttpSearcher?.GetOtherWorld() ?? [],
            .. _smSearcher?.GetOtherWorld() ?? [],
            .. _m1Searcher?.GetOtherWorld() ?? [],
            .. _z1Searcher?.GetOtherWorld() ?? [],
        ];
        return locations;
    }
}
