namespace Randomizer.Games.Goonies2;

using Randomizer.Graph;

internal static class DataLoader
{
    public static void Fill(World world)
    {
        var graph = world.Graph;

        foreach (var vertex in LoadVertices(world))
            graph.AddVertex(vertex);
    }

    private static IEnumerable<Vertex> LoadVertices(World world)
    {
        var vertexData = YamlReader.LoadVertices();
        var structuredVertices = new Dictionary<string, Vertex>();
        var pendingConnections = new List<(Vertex Source, string Target, ItemCondition Condition)>();

        foreach (var region in vertexData.Regions)
        {
            var regionVertex = new Vertex
            {
                Type = VertexType.Region,
                Name = region.Name,
                World = world,
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

    private static ItemCondition ConditionFrom(World world, string condition)
    {
        var conditionSplit = condition.Split("|");
        var itemCount = 1;

        if (conditionSplit.Length > 1)
            itemCount = int.Parse(conditionSplit[1]);

        return new ItemCondition(world.GetItem(conditionSplit[0]), itemCount);
    }
}
