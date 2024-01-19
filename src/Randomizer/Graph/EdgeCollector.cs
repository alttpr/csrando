namespace Randomizer.Graph;

/// <summary>
/// Pull data files to create all edges for a given world configuration.
/// </summary>
internal class EdgeCollector
{
    /// <summary>
    /// Given a particular world (configuration), read all the edge data files
    /// and create edges based on the world to connect the vertices.
    /// </summary>
    /// <param name="world">world to attach preset items to</param>
    public Dictionary<ItemCondition, DirectedUndirectedPair> GetForWorld(World world)
    {
        var edgeData = new Dictionary<string, DirectedUndirectedPair>();
        YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("base"));

        switch (world.Config.State)
        {
            case StateOption.Standard:
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("normal"));
                edgeData["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                break;
            case StateOption.Inverted:
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("inverted"));
                // @todo move these once we have the nodes made
                edgeData["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                edgeData["fixed"].Directed.Add(new() { "start", "Dark Sanctuary" });
                break;
            case StateOption.Open:
            default:
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("normal"));
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("open"));
                edgeData["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                edgeData["fixed"].Directed.Add(new() { "start", "Sanctuary Hall" });
                break;
        }

        foreach (var tech in world.Config.Techs)
        {
            var fileName = tech switch
            {
                TechOption.DungeonBunnyRevival => "dungeon_bunny_revival",
                _ => throw new Exception("Missing tech enum to file mapping for value: " + tech),
            };
            YamlReader.MergeEdges(edgeData, YamlReader.LoadEdgesFromTech(fileName));
        }

        var returnData = new Dictionary<ItemCondition, DirectedUndirectedPair>();
        foreach (var (conditionString, edges) in edgeData)
        {
            var parts = conditionString.Split("|");
            var item = world.GetItem(parts[0]);
            var itemCountPair = new ItemCondition(item, parts.Length > 1 ? int.Parse(parts[1]) : 1);

            returnData[itemCountPair] = new DirectedUndirectedPair
            {
                Directed = edges.Directed,
                Undirected = edges.Undirected,
            };
        }

        return returnData;
    }
}
