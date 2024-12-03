namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using BaseEdge = Graph.Edge;

public class Edge : BaseEdge
{
    public Edge(Vertex from, Vertex to, ItemCondition condition) : base(from, to, condition) { }
    public Edge(Vertex from, Vertex to, IEnumerable<Strat> strats) : base(from, to, new ItemCondition(from.World.GetItem("fixed"), 1)) { Strats = strats.ToArray(); }

    public Strat[]? Strats;
}
