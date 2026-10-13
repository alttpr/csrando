namespace Randomizer.Games.Combo;

using Randomizer.Graph;
using AlttpWorld = Randomizer.Games.Alttp.World;
using BaseVertex = Graph.Vertex;
using Graph = Graph.Graph;
using M1World = Randomizer.Games.Metroid.World;
using SMWorld = Randomizer.Games.SuperMetroid.World;
using Z1World = Randomizer.Games.Zelda1.World;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : World<Item>
{
    public Config Config { get; }
    public PRNG Prng { get; }

    public WorldConfig GameConfig { get; init; }

    public AlttpWorld? AlttpWorld { get; init; }
    public SMWorld? SMWorld { get; init; }
    public Z1World? Z1World { get; init; }
    public M1World? M1World { get; init; }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base("combo", id, graph, randomizerConfig)
    {
        Config = randomizerConfig.Combo ?? throw new ArgumentException("This world requires valid settings for Combo");
        GameConfig = randomizerConfig;
        Prng = prng;
        Start = graph.AddVertex(new Vertex()
        {
            Name = "start",
            Type = VertexType.Meta,
            World = this,
        });

        // Resolve initial game depending on config (random starts can select a random game)
        _startInitialGame = ResolveStartInitialGame(randomizerConfig, prng);
        if (randomizerConfig.SuperMetroid is { } smConfig)
            smConfig.ApplyStartLocation = _startInitialGame is null or "sm";
        if (randomizerConfig.Metroid is { } m1Config)
            m1Config.ApplyStartArea = _startInitialGame is null or "m1";

        StartingItems = new Inventory([GetItem("fixed")]);


        if (WorldConfig.Alttp != null)
        {
            AlttpWorld = new AlttpWorld(id, WorldConfig, graph, prng);
            StartingItems = StartingItems.Merge(AlttpWorld.StartingItems);
        }
        if (WorldConfig.SuperMetroid != null)
        {
            SMWorld = new SMWorld(id, WorldConfig, graph, prng);
            StartingItems = StartingItems.Merge(SMWorld.StartingItems);
        }
        if (WorldConfig.Zelda1 != null)
        {
            Z1World = new Z1World(id, WorldConfig, graph, prng);
            StartingItems = StartingItems.Merge(Z1World.StartingItems);
        }
        if (WorldConfig.Metroid != null)
        {
            M1World = new M1World(id, WorldConfig, graph, prng);
            StartingItems = StartingItems.Merge(M1World.StartingItems);
        }

        // Apply the default portal layout: create the portal rooms it needs, then wire
        // its cross-game graph edges. The graph is the source of truth from here:
        // PortalWriter derives every transition table row from the cross-game edges
        // (see DerivePortalEdges).
        DefaultPortalLayout.Apply(this);

        // Fail at graph-build time if any cross-game edge cannot become a portal.
        DerivePortalEdges();

        // Station viability includes reaching a cross-game portal from the bare start
        // pocket, so the configured SM start can only resolve now that the portal
        // edges exist.
        SMWorld?.ApplyConfiguredStartStation(prng);
    }

    /// <summary>
    /// Connects two portal-capable vertices into one bidirectional portal by adding the
    /// two cross-game graph edges. The vertices' anchors are resolved (and, where the
    /// game supports it, created on demand — any ALttP entrance vertex works); the
    /// transition table rows and the anchors' ROM patches are derived from the graph
    /// when the ROM is written.
    /// </summary>
    public void ConnectPortal(BaseVertex a, BaseVertex b) =>
        ConnectPortal(ResolveAnchor(a), ResolveAnchor(b));

    /// <summary>Connects two games' portal anchors into one bidirectional portal.</summary>
    public void ConnectPortal(PortalAnchor a, PortalAnchor b)
    {
        var worldA = WorldFor(a.GameId);
        var worldB = WorldFor(b.GameId);

        var aExit = worldA.GetLocation(a.ExitVertexName);
        var aEntry = worldA.GetLocation(a.EntryVertexName);
        var bExit = worldB.GetLocation(b.ExitVertexName);
        var bEntry = worldB.GetLocation(b.EntryVertexName);

        Graph.AddDirected(aExit, bEntry, worldA.GetItem("fixed"));
        Graph.AddDirected(bExit, aEntry, worldB.GetItem("fixed"));
    }

    /// <summary>
    /// The portal anchor behind a graph vertex, resolved by the vertex's own game world
    /// (see <see cref="IPortalHost"/>). Throws when the vertex cannot host a portal.
    /// </summary>
    public PortalAnchor ResolveAnchor(BaseVertex vertex)
    {
        if (!IsGameWorld(vertex.World) || vertex.World is not IPortalHost host)
            throw new ArgumentException($"Vertex '{vertex.Name}' is not part of a portal-capable game world");

        return host.ResolvePortalAnchor(vertex);
    }

    /// <summary>
    /// Derives the portal layout from the graph: every edge between two of this world's
    /// games must connect two portal-capable vertices (resolving anchors on demand) and
    /// becomes one transition table row. Throws when a cross-game edge cannot become a
    /// portal.
    /// </summary>
    public IReadOnlyList<PortalEdge> DerivePortalEdges()
    {
        var crossGameEdges = CrossGameEdges().ToList();

        // The graph is the source of truth: each cross-game edge must leave a portal
        // exit vertex and arrive at a portal entry vertex. Resolving the endpoints gives
        // the ROM metadata needed by the writer, but the connection itself is still the
        // edge being inspected here.
        var edges = new List<PortalEdge>();
        foreach (var (from, to) in crossGameEdges)
        {
            var fromAnchor = ResolveAnchor(from);
            var toAnchor = ResolveAnchor(to);
            var fromWorld = WorldFor(fromAnchor.GameId);
            var toWorld = WorldFor(toAnchor.GameId);

            var exit = fromWorld.GetLocation(fromAnchor.ExitVertexName);
            if (!ReferenceEquals(exit, from))
            {
                throw new Exception(
                    $"Cross-game edge '{from.Name}' -> '{to.Name}' does not start at portal exit vertex '{exit.Name}'");
            }

            var entry = toWorld.GetLocation(toAnchor.EntryVertexName);
            if (!ReferenceEquals(entry, to))
            {
                throw new Exception(
                    $"Cross-game edge '{from.Name}' -> '{to.Name}' does not lead to portal entry vertex '{entry.Name}'");
            }

            edges.Add(new PortalEdge(fromAnchor, toAnchor));
        }

        return edges.OrderBy(e => PortalOrder(e.From)).ToList();
    }

    public IEnumerable<IWorld> GameWorlds()
    {
        if (SMWorld != null)
            yield return SMWorld;
        if (AlttpWorld != null)
            yield return AlttpWorld;
        if (Z1World != null)
            yield return Z1World;
        if (M1World != null)
            yield return M1World;
    }

    private IEnumerable<(BaseVertex From, BaseVertex To)> CrossGameEdges()
    {
        foreach (var gameWorld in GameWorlds())
        {
            foreach (var vertex in gameWorld.GetLocations())
            {
                foreach (var edge in vertex.Edges)
                {
                    if (edge.To.World != vertex.World && IsGameWorld(edge.To.World))
                        yield return (vertex, edge.To);
                }
            }
        }
    }

    private bool IsGameWorld(IWorld world) => GameWorlds().Contains(world);

    /// <summary>The present game world with this game id, or null when the game is not
    /// part of this seed.</summary>
    internal IWorld? GameWorld(string gameId) => GameWorlds().FirstOrDefault(w => w.GameId == gameId);

    private IWorld WorldFor(string gameId) => GameWorld(gameId)
        ?? throw new ArgumentException($"Unknown portal game id '{gameId}'");

    private int PortalOrder(PortalAnchor anchor)
    {
        var anchors = ((IPortalHost)WorldFor(anchor.GameId)).PortalAnchors;
        int index = anchors.FindIndex(a => ReferenceEquals(a, anchor));
        if (index < 0)
            throw new Exception($"Portal anchor '{anchor.Name}' is not registered in its world");
        return index;
    }

    /// <summary>The game that won the moved-start selection, or null when no game
    /// requested a moved start.</summary>
    private readonly string? _startInitialGame;

    /// <summary>
    /// One game id among those whose configs request a moved start (random tie-break
    /// when several do), or null when none does. Uses the raw config requests — not
    /// the resolved stations/areas — so it can run before the game worlds exist.
    /// </summary>
    private static string? ResolveStartInitialGame(WorldConfig config, PRNG prng)
    {
        List<string> candidates = [];
        if (config.SuperMetroid?.StartLocationRequested == true)
            candidates.Add("sm");
        if (config.Metroid?.StartAreaRequested == true)
            candidates.Add("m1");

        return candidates.Count switch
        {
            0 => null,
            1 => candidates[0],
            _ => prng.GetRandomElement(candidates),
        };
    }

    /// <summary>
    /// The game the seed starts in: the game whose moved-start request won the
    /// selection, otherwise the configured initial game when that game is present,
    /// otherwise the first present game in sm, alttp, z1, m1 order.
    /// </summary>
    public string EffectiveInitialGame => _startInitialGame ?? Config.InitialGame switch
    {
        "" => FirstPresentGame(),
        "sm" when SMWorld != null => "sm",
        "alttp" when AlttpWorld != null => "alttp",
        "z1" when Z1World != null => "z1",
        "m1" when M1World != null => "m1",
        "sm" or "alttp" or "z1" or "m1" => FirstPresentGame(),
        _ => throw new ArgumentException("Invalid initial game", nameof(Config.InitialGame)),
    };

    private string FirstPresentGame() =>
        SMWorld != null ? "sm"
        : AlttpWorld != null ? "alttp"
        : Z1World != null ? "z1"
        : M1World != null ? "m1"
        : throw new ArgumentException("No games to play");

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed")]);
        if (AlttpWorld != null)
        {
            inventory.Merge(AlttpWorld.ComputeStartingItems());
        }
        if (SMWorld != null)
        {
            inventory.Merge(SMWorld.ComputeStartingItems());
        }
        if (Z1World != null)
        {
            inventory.Merge(Z1World.ComputeStartingItems());
        }
        if (M1World != null)
        {
            inventory.Merge(M1World.ComputeStartingItems());
        }

        return inventory;
    }

    protected override Item CreateItem(string name, IWorld world) => new(name, world);

    public new IEnumerable<BaseVertex> GetLocations() => Graph.GetVertices().Where(vertex => vertex.World.Id == Id);
    public new IEnumerable<BaseVertex> GetLocationsOfType(VertexType type) => GetLocations().Where(vertex => vertex.Type == type);


    public override bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {

        var searcher = GetSearcherForWorld(Graph, Start, startingInventory);
        if (AlttpWorld != null && !searcher.HasFound(AlttpWorld.GetItem("Triforce")))
        {
            return false;
        }

        if (Z1World != null && !searcher.HasFound(Z1World.GetItem("Zelda")))
        {
            return false;
        }

        if (SMWorld != null && !searcher.HasFound(SMWorld.GetItem("f_DefeatedMotherBrain")))
        {
            return false;
        }

        if (M1World != null && !searcher.HasFound(M1World.GetItem("DefeatedSilverTwo")))
        {
            return false;
        }

        return true;
    }

    public override IEnumerable<IItem> GetVictoryItems()
    {
        if (AlttpWorld != null) yield return AlttpWorld.GetItem("Triforce");
        if (Z1World != null) yield return Z1World.GetItem("Zelda");
        if (SMWorld != null) yield return SMWorld.GetItem("f_DefeatedMotherBrain");
        if (M1World != null) yield return M1World.GetItem("DefeatedSilverTwo");
    }

    public override BaseVertex GetPlaythroughStart() => EffectiveInitialGame switch
    {
        "alttp" => AlttpWorld!.Start,
        "sm" => SMWorld!.Start,
        "z1" => Z1World!.Start,
        "m1" => M1World!.Start,
        _ => throw new ArgumentException("Invalid initial game", nameof(EffectiveInitialGame)),
    };

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new ComboSearcher(graph, (Vertex)(start ?? Start), inventory, setLocations);
    }

}
