namespace Randomizer.Games.Combo;

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

    private ISearcher? _alttpSearcher;
    private ISearcher? _smSearcher;
    private ISearcher? _m1Searcher;
    private ISearcher? _z1Searcher;

    public ComboSearcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null)
    {
        _world = (World)start.World;
        _graph = graph;

        if(_world.AlttpWorld != null)
        {
            _alttpSearcher = _world.AlttpWorld.GetSearcherForWorld(graph, _world.AlttpWorld.Start, inventory, setLocations);
        }

    }

    public bool HasFound(IItem item)
    {
        return (_alttpSearcher?.HasFound(item) ?? false) ||
                (_smSearcher?.HasFound(item) ?? false) ||
                (_m1Searcher?.HasFound(item) ?? false) ||
                (_z1Searcher?.HasFound(item) ?? false);
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

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetEmptyLocationsInSet(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets, bool onlyReachable)
    {
        List<Randomizer.Graph.Vertex> locations = new List<Randomizer.Graph.Vertex>();
        locations.AddRange(_alttpSearcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? []);
        locations.AddRange(_smSearcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? []);
        locations.AddRange(_m1Searcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? []);
        locations.AddRange(_z1Searcher?.GetEmptyLocationsInSet(itemSet, itemSets, onlyReachable) ?? []);
        return locations;
    }

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetVisited()
    {
        List<Randomizer.Graph.Vertex> locations = new List<Randomizer.Graph.Vertex>();
        locations.AddRange(_alttpSearcher?.GetVisited() ?? []);
        locations.AddRange(_smSearcher?.GetVisited() ?? []);
        locations.AddRange(_m1Searcher?.GetVisited() ?? []);
        locations.AddRange(_z1Searcher?.GetVisited() ?? []);
        return locations;
    }
}
