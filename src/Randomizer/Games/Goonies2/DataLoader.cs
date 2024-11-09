namespace Randomizer.Games.Goonies2;
using BaseVertex = Randomizer.Graph.Vertex;

using Randomizer.Graph;

internal static class DataLoader
{
    public static BaseVertex Fill(World world)
    {
        var graph = world.Graph;

        foreach (var vertex in LoadVertices(world))
            graph.AddVertex(vertex);

        ModifyEdgeConditions(graph, world);

        return world.GetLocation("start");
    }

    private static IEnumerable<Vertex> LoadVertices(World world)
    {
        var vertexData = YamlReader.LoadVertices();
        var structuredVertices = new Dictionary<string, Vertex>();
        var fixedCondition = new ItemCondition(world.GetItem("fixed"), 1);
        var pendingConnections = new List<(Vertex Source, string Target, ItemCondition Condition)>();

        foreach (var meta in vertexData.Meta)
        {
            var metaVertex = new Vertex
            {
                Type = VertexType.Meta,
                Name = meta.Name,
                World = world,
            };
            structuredVertices.Add(meta.Name, metaVertex);

            foreach (var connection in meta.Connections)
            {
                foreach (var target in connection.Value)
                {
                    pendingConnections.Add((metaVertex, target, ConditionFrom(world, connection.Key)));
                }
            }

            foreach (var (index, item) in meta.Items.Indexed())
            {
                var metaItemVertex = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = $"{meta.Name} - {index} - {item}",
                    World = world,
                    Item = world.GetItem(item),
                };
                structuredVertices.Add(metaItemVertex.Name, metaItemVertex);
                metaVertex.Edges.Add(new Edge(metaVertex, metaItemVertex, fixedCondition));
            }
        }

        foreach (var region in vertexData.Regions)
        {
            var regionVertex = new Vertex
            {
                Type = VertexType.Region,
                Name = region.Name,
                World = world,
                Dark = region.Dark,
                Water = region.Water,
            };
            structuredVertices.Add(region.Name, regionVertex);

            //foreach (var item in region.Items)
            //{
            //    var itemVertex = new Vertex
            //    {
            //        Type = VertexType.Item,
            //        Name = item.Name,
            //        World = world,
            //        Item = world.GetItemOrNull(item.Item),
            //        ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
            //        Addresses = item.Addresses.ToArray(),
            //    };
            //    structuredVertices.Add(item.Name, itemVertex);
            //    foreach (var condition in item.Conditions.DefaultIfEmpty("fixed"))
            //    {
            //        regionVertex.Edges.Add(new Edge(regionVertex, itemVertex, ConditionFrom(world, condition)));
            //    }
            //}

            foreach (var connection in region.Connections)
            {
                foreach (var target in connection.Value)
                {
                    pendingConnections.Add((regionVertex, target, ConditionFrom(world, connection.Key)));
                }
            }
        }

        // Link all the pending edges
        foreach (var (source, target, condition) in pendingConnections)
            source.Edges.Add(new Edge(source, structuredVertices[target], condition));

        return structuredVertices.Values;
    }

    private static void ModifyEdgeConditions(Graph graph, World world)
    {
        (Func<Vertex, bool> ModifyCondition, ItemCondition ItemCondition)[] edgeModifiers =
        [
            (v => v.Dark, new(world.GetItem("CanSeeInTheDark"), 1)),
            (v => v.Water, new(world.GetItem("CanDive"), 1)),
        ];

        foreach (var (modifyCond, condition) in edgeModifiers)
        {
            var transitionRooms = graph.GetVertices().OfType<Vertex>().Where(v => !modifyCond(v) && v.World == world).ToList();
            foreach (var transitionRoom in transitionRooms)
            {
                foreach (var originalEdge in transitionRoom.Edges.Where(e => modifyCond((Vertex)e.To)))
                {
                    var targetRoom = (Vertex)originalEdge.To;
                    var transition = new Vertex
                    {
                        Type = VertexType.Region,
                        Name = $"{targetRoom.Name} - Transition from {transitionRoom.Name}",
                        World = world,
                    };

                    var edgesToModify = targetRoom.Edges.Where(e => e.To == transitionRoom);

                    world.Graph.AddVertex(transition);
                    originalEdge.To = transition;
                    world.Graph.AddDirected(targetRoom, transition, world.GetItem("fixed"));
                    world.Graph.AddDirected(transition, targetRoom, condition);

                    if (edgesToModify.Count() > 1)
                        throw new Exception("Uh oh, is the code really correct there?");

                    foreach (var newEdge in edgesToModify)
                    {
                        newEdge.To = transition;
                        var oldCondition = newEdge.Condition;
                        newEdge.Condition = new ItemCondition(world.GetItem("fixed"), 1);

                        transition.Edges.Add(new Edge(transition, transitionRoom, oldCondition));
                    }
                }
            }
        }
    }

    private static ItemCondition ConditionFrom(World world, string condition)
    {
        var conditionSplit = condition.Split("|");
        var itemCount = 1;

        if (conditionSplit.Length > 1)
            itemCount = int.Parse(conditionSplit[1]);

        return new ItemCondition(world.GetItem(conditionSplit[0]), itemCount);
    }
}
