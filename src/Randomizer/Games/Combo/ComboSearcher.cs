namespace Randomizer.Games.Combo;

using Randomizer.Games.SuperMetroid;
using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ComboSearcher : ISearcher
{
    private World _world;
    private Graph _graph;
    private Inventory _inventory;

    private ISearcher? _alttpSearcher;
    private ISearcher? _smSearcher;
    private ISearcher? _m1Searcher;
    private ISearcher? _z1Searcher;

    public StatefulSearcher? SMSearcher => _smSearcher as StatefulSearcher;

    public ComboSearcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null)
    {
        _world = (World)start.World;
        _graph = graph;
        _inventory = inventory;

        Inventory prevInventory;
        HashSet<Randomizer.Graph.Vertex> otherWorldVertices = new();

        if (_world.AlttpWorld != null)
        {
            _alttpSearcher = _world.AlttpWorld.GetSearcherForWorld(graph, _world.AlttpWorld.Start, inventory, setLocations);
            otherWorldVertices.UnionWith(_alttpSearcher.GetOtherWorld().ToHashSet());
        }

        if (_world.Z1World != null)
        {
            _z1Searcher = _world.Z1World.GetSearcherForWorld(graph, _world.Z1World.Start, inventory, setLocations);
            otherWorldVertices.UnionWith(_z1Searcher.GetOtherWorld().ToHashSet());
        }

        if (_world.M1World != null)
        {
            _m1Searcher = _world.M1World.GetSearcherForWorld(graph, _world.M1World.Start, inventory, setLocations);
            otherWorldVertices.UnionWith(_m1Searcher.GetOtherWorld().ToHashSet());
        }

        if (_world.SMWorld!= null)
        {
            _smSearcher = _world.SMWorld.GetSearcherForWorld(graph, _world.SMWorld.Start, inventory, setLocations);
            otherWorldVertices.UnionWith(_smSearcher.GetOtherWorld().ToHashSet());
        }

        do
        {
            prevInventory = inventory.Clone();
            if(_alttpSearcher != null)
            {
                var starts = otherWorldVertices.Where(v => v.World == _world.AlttpWorld).ToList();
                _alttpSearcher.ResumeSearch(starts, prevInventory);
                otherWorldVertices.UnionWith(_alttpSearcher.GetOtherWorld().ToHashSet());
            }

            if (_z1Searcher != null)
            {
                var starts = otherWorldVertices.Where(v => v.World == _world.Z1World).ToList();
                _z1Searcher.ResumeSearch(starts, prevInventory);
                otherWorldVertices.UnionWith(_z1Searcher.GetOtherWorld().ToHashSet());
            }

            if (_m1Searcher != null)
            {
                var starts = otherWorldVertices.Where(v => v.World == _world.M1World).ToList();
                _m1Searcher.ResumeSearch(starts, prevInventory);
                otherWorldVertices.UnionWith(_m1Searcher.GetOtherWorld().ToHashSet());
            }

            if (_smSearcher != null)
            {
                var starts = otherWorldVertices.Where(v => v.World == _world.SMWorld).ToList();
                _smSearcher.ResumeSearch(starts, prevInventory);
                otherWorldVertices.UnionWith(_smSearcher.GetOtherWorld().ToHashSet());
            }
        } while(prevInventory.All().Count() != inventory.All().Count());

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
