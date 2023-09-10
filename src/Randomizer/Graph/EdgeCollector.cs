namespace AlttpRandomizer.Graph;

/**
 * Pull data files to create all edges for a given world configuration.
 */
internal class EdgeCollector
{
    /**
     * Given a particular world (configuration), read all the edge data files
     * and create edges based on the world to connect the vertices.
     *
     * @param World world world to attach preset items to
     */
    public Dictionary<string, DirectedUndirectedPair> GetForWorld(World world)
    {
        var edges_data = YamlReader.LoadEdges("base");

        switch (world.RandomizerConfig.State)
        {
            case StateOption.Standard:
                YamlReader.MergeEdges(edges_data, YamlReader.LoadEdges("normal"));
                YamlReader.MergeEdges(edges_data, YamlReader.LoadEdges("standard"));
                edges_data["fixed"].Directed.Add(new() { "start", "Rain - Link's House" });
                edges_data["RescueZelda"].Directed.Add(new() { "start", "Sanctuary Hall" });
                // edges_data["OldManFound"]["directed"][] = ["start", "Old Man Cave"];

                break;
            case StateOption.Inverted:
                YamlReader.MergeEdges(edges_data, YamlReader.LoadEdges("inverted"));
                // @todo move these once we have the nodes made
                edges_data["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                edges_data["fixed"].Directed.Add(new() { "start", "Dark Sanctuary" });
                // edges_data["OldManFound"]["directed"][] = ["start", "Old Man Cave"];

                break;
            case StateOption.Open:
            default:
                YamlReader.MergeEdges(edges_data, YamlReader.LoadEdges("normal"));
                YamlReader.MergeEdges(edges_data, YamlReader.LoadEdges("open"));
                edges_data["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                edges_data["fixed"].Directed.Add(new() { "start", "Sanctuary Hall" });
                // edges_data["OldManFound"]["directed"][] = ["start", "Old Man Cave"];
                break;
        }

        foreach (var tech in world.RandomizerConfig.Techs)
        {
            var file_name = tech switch
            {
                TechOption.DungeonBunnyRevival => "dungeon_bunny_revival",
                _ => throw new Exception("Missing tech enum to file mapping for value: " + tech),
            };
            YamlReader.MergeEdges(edges_data, YamlReader.LoadEdgesFromTech(file_name));
        }

        var return_data = new Dictionary<string, DirectedUndirectedPair>();
        int world_id = world.Id;
        foreach (var (group, edges) in edges_data)
        {
            string name = group + ":" + world_id;

            if (group.Contains('|'))
            {
                name = group.Replace("|", ":" + world_id + "|");
            }

            return_data[name] = new DirectedUndirectedPair
            {
                Directed = edges.Directed.Select((es) => es.Select((v) => $"{v}:{world_id}").ToList()).ToList(),
                Undirected = edges.Undirected.Select((es) => es.Select((v) => $"{v}:{world_id}").ToList()).ToList(),
            };
        }

        return return_data;
    }
}
