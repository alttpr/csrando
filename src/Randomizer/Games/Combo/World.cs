namespace Randomizer.Games.Combo;

using Randomizer.Graph;
using Randomizer.RomModifications;
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

    public List<(BaseVertex, BaseVertex)> Portals { get; } = new();

    /// <summary>
    /// The cross-game portal connections of this world. PortalWriter emits every game's
    /// transition table from these; randomized portal layouts only need to build a
    /// different list here.
    /// </summary>
    public List<PortalConnection> PortalConnections { get; } = new();

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

        if (WorldConfig.Alttp != null && WorldConfig.Zelda1 != null)
        {
            Graph.AddDirected(AlttpWorld!.GetLocation("start"), Z1World!.Start, AlttpWorld!.GetItem("fixed"));
            PortalConnections.Add(new(VanillaPortalSides.Z1, VanillaPortalSides.AlttpToZ1));
        }

        if (WorldConfig.Alttp != null && WorldConfig.Metroid != null)
        {
            // M1 is entered physically through its portal anchors, not the spawn platform —
            // under map shuffle the spawn can sit deep inside the generated map. The Meta
            // hub (ability derivations and the win condition) is wired directly, since the
            // start vertex that used to provide it is no longer the entry.
            var alttpSide = AlttpWorld!.GetLocation("start");
            Graph.AddDirected(alttpSide, M1World!.GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), AlttpWorld!.GetItem("fixed"));

            foreach (var anchor in M1World.PortalAnchors)
            {
                var portalDoor = M1World.GetLocation(anchor.VertexName);
                Graph.AddDirected(alttpSide, portalDoor, AlttpWorld!.GetItem("fixed"));
                Graph.AddDirected(portalDoor, alttpSide, M1World.GetItem("fixed"));
                Portals.Add((portalDoor, alttpSide));

                PortalConnections.Add(new(
                    new PortalSide("m1", 3, [(uint)anchor.RoomWord, (uint)anchor.Direction],
                        anchor.DestinationId, anchor.DestinationArgs, anchor.VertexName),
                    VanillaPortalSides.AlttpToM1));
            }
        }

        if (WorldConfig.SuperMetroid != null && WorldConfig.Alttp != null)
        {
            foreach (var (sm, alttp) in VanillaPortalSides.SmAlttp)
                PortalConnections.Add(new(sm, alttp));

            // Create the portal entrances for the cross-game portals in the four rooms we need to connect for SM
            var crateriaMapStationPortalIn = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Crateria - Crateria Map Room - Portal - In",
                Type = VertexType.Entrance,
                World = SMWorld!,
                Addresses = [((SNES)0x83AE00).Value],
            });

            var crateriaMapStationPortalOut = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Crateria - Crateria Map Room - Portal - Out",
                Type = VertexType.Outlet,
                World = SMWorld!,
                Addresses = [((SNES)0x83AE0A).Value]
            });

            var crateriaMapStation = (SuperMetroid.Vertex)SMWorld!.GetLocation("Crateria - Crateria Map Room - Left Door");
            Graph.AddDirected(crateriaMapStationPortalIn, crateriaMapStation, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(crateriaMapStation, crateriaMapStationPortalOut, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(crateriaMapStationPortalOut, crateriaMapStationPortalIn, SMWorld!.GetItem("fixed"));

            Graph.AddDirected(crateriaMapStationPortalOut, AlttpWorld!.GetLocation("start"), SMWorld!.GetItem("fixed"));
            Graph.AddDirected(AlttpWorld!.GetLocation("start"), crateriaMapStationPortalIn, AlttpWorld!.GetItem("fixed"));

            //Portals.Add((crateriaMapStationPortalOut, AlttpWorld!.GetLocation("start")));
            //Portals.Add((AlttpWorld!.GetLocation("start"), crateriaMapStationPortalIn));

            var norfairMapPortalIn = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Norfair - Norfair Map Room - Portal - In",
                Type = VertexType.Entrance,
                World = SMWorld!,
                Addresses = [((SNES)0x83AF00).Value]
            });

            var norfairMapPortalOut = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Norfair - Norfair Map Room - Portal - Out",
                Type = VertexType.Outlet,
                World = SMWorld!,
                Addresses = [((SNES)0x83AF0A).Value]
            });

            var norfairMap = (SuperMetroid.Vertex)SMWorld!.GetLocation("Norfair - Norfair Map Room - Right Door");
            Graph.AddDirected(norfairMapPortalIn, norfairMap, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(norfairMap, norfairMapPortalOut, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(norfairMapPortalOut, norfairMapPortalIn, SMWorld!.GetItem("fixed"));

            Graph.AddDirected(norfairMapPortalOut, AlttpWorld!.GetLocation("West Death Mountain"), SMWorld!.GetItem("fixed"));
            Graph.AddDirected(AlttpWorld!.GetLocation("West Death Mountain"), norfairMapPortalIn, AlttpWorld!.GetItem("fixed"));

            //Portals.Add((norfairMapPortalOut, AlttpWorld!.GetLocation("West Death Mountain")));
            //Portals.Add((AlttpWorld!.GetLocation("West Death Mountain"), norfairMapPortalIn));

            var maridiaMissileRefillPortalIn = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Maridia - Maridia Missile Refill Room - Portal - In",
                Type = VertexType.Entrance,
                World = SMWorld!,
                Addresses = [((SNES)0x83AF80).Value]
            });

            var maridiaMissileRefillPortalOut = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Maridia - Maridia Missile Refill Room - Portal - Out",
                Type = VertexType.Outlet,
                World = SMWorld!,
                Addresses = [((SNES)0x83AF8A).Value]
            });

            var maridiaMissileRefill = (SuperMetroid.Vertex)SMWorld!.GetLocation("Maridia - Maridia Missile Refill Room - Left Door");
            Graph.AddDirected(maridiaMissileRefillPortalIn, maridiaMissileRefill, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(maridiaMissileRefill, maridiaMissileRefillPortalOut, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(maridiaMissileRefillPortalOut, maridiaMissileRefillPortalIn, GetItem("fixed"));

            Graph.AddDirected(maridiaMissileRefillPortalOut, AlttpWorld!.GetLocation("Dark Shopping Mall"), SMWorld!.GetItem("fixed"));
            Graph.AddDirected(AlttpWorld!.GetLocation("Dark Shopping Mall"), maridiaMissileRefillPortalIn, AlttpWorld!.GetItem("fixed"));

            //Portals.Add((maridiaMissileRefillPortalOut, AlttpWorld!.GetLocation("Dark Shopping Mall")));
            //Portals.Add((AlttpWorld!.GetLocation("Dark Shopping Mall"), maridiaMissileRefillPortalIn));


            var lowerNorfairRefillPortalIn = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Norfair - Golden Torizo Energy Recharge - Portal - In",
                Type = VertexType.Entrance,
                World = SMWorld!,
                Addresses = [((SNES)0x83B000).Value]
            });

            var lowerNorfairRefillPortalOut = graph.AddVertex(new SuperMetroid.Vertex()
            {
                Name = "Norfair - Golden Torizo Energy Recharge - Portal - Out",
                Type = VertexType.Outlet,
                World = SMWorld!,
                Addresses = [((SNES)0x83B00A).Value]
            });

            var lowerNorfairRefill = (SuperMetroid.Vertex)SMWorld!.GetLocation("Norfair - Golden Torizo Energy Recharge - Left Door");
            Graph.AddDirected(lowerNorfairRefillPortalIn, lowerNorfairRefill, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(lowerNorfairRefill, lowerNorfairRefillPortalOut, SMWorld!.GetItem("fixed"));
            Graph.AddDirected(lowerNorfairRefillPortalOut, lowerNorfairRefillPortalIn, SMWorld!.GetItem("fixed"));

            Graph.AddDirected(lowerNorfairRefillPortalOut, AlttpWorld!.GetLocation("Mire"), SMWorld!.GetItem("fixed"));
            Graph.AddDirected(AlttpWorld!.GetLocation("Mire"), lowerNorfairRefillPortalIn, AlttpWorld!.GetItem("fixed"));

            //Portals.Add((lowerNorfairRefillPortalOut, AlttpWorld!.GetLocation("Mire")));
            //Portals.Add((AlttpWorld!.GetLocation("Mire"), lowerNorfairRefillPortalIn));
        }
    }

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

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new ComboSearcher(graph, (Vertex)(start ?? Start), inventory, setLocations);
    }

}
