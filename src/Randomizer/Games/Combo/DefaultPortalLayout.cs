namespace Randomizer.Games.Combo;

using Randomizer.Graph;
using BaseVertex = Randomizer.Graph.Vertex;
using M1Area = Randomizer.Games.Metroid.YamlReader.Area;

/// <summary>
/// The default portal layout. Every pair below becomes one bidirectional portal via
/// <see cref="World.ConnectPortal(BaseVertex, BaseVertex)"/>; SM rooms are converted
/// into portal rooms on the spot, and M1 uses generated portal rooms in the areas
/// declared by <see cref="Metroid.DataLoader.PortalRoomAreas"/>.
///
/// The games form a full mesh rather than an ALttP hub: besides the vanilla ALttP
/// connections there are direct SM&lt;-&gt;M1, SM&lt;-&gt;Z1 and M1&lt;-&gt;Z1 portals. Missing
/// games leave behind unused endpoints in the games that remain; those endpoints are
/// reassigned to other present games without reusing any concrete endpoint.
/// </summary>
internal static class DefaultPortalLayout
{
    // Z1 portal caves per link (always-reserved candidates; 0x66 pairs with ALttP).
    private const int Z1AlttpCave = 0x66;
    private const int Z1SmCave = 0x0C;
    // Map 0x70's cave (the "forest of maze" money-hint cave) is a MoneyGame cave, not a
    // shuffled shop, so the M1 portal does not consume a shop location or another copy of
    // the default Cave 0x1E shop when shop shuffle is disabled.
    private const int Z1M1Cave = 0x70;

    /// <summary>One side of a canonical link; World is null when its game is absent.</summary>
    private sealed record Endpoint(IWorld? World, Func<BaseVertex> Resolve);
    private sealed record Link(Endpoint A, Endpoint B);

    public static void Apply(World world)
    {
        BaseVertex SmRoom(string roomName) =>
            world.SMWorld!.GetLocation(SuperMetroid.Portals.ConvertRoom(world.SMWorld, roomName).VertexName);
        BaseVertex M1PortalRoom(M1Area area)
        {
            var portalRoom = world.M1World!.PortalRooms.SingleOrDefault(room => room.Area == area)
                ?? throw new InvalidOperationException($"M1 {area} portal room was not generated for this layout");
            return world.M1World.GetLocation(portalRoom.VertexName);
        }

        BaseVertex Z1Cave(int map) =>
            world.Z1World!.GetLocation($"Overworld - Map {map:X2} - Open cave");
        BaseVertex Entrance(string name) => world.AlttpWorld!.GetLocation($"{name} - In");

        Endpoint Sm(string room) => new(world.SMWorld, () => SmRoom(room));
        Endpoint Alttp(string entrance) => new(world.AlttpWorld, () => Entrance(entrance));
        Endpoint Z1(int map) => new(world.Z1World, () => Z1Cave(map));
        Endpoint M1(M1Area area) => new(world.M1World, () => M1PortalRoom(area));

        var z1Alttp = Z1(Z1AlttpCave);
        var z1Sm = Z1(Z1SmCave);
        var z1M1 = Z1(Z1M1Cave);

        var m1Alttp = M1(M1Area.Brinstar);
        var m1Sm = M1(M1Area.Norfair);
        var m1Z1 = M1(M1Area.Kraid);

        var smAlttp1 = Sm("Crateria Map Room");
        var smAlttp2 = Sm("Norfair Map Room");
        var smAlttp3 = Sm("Maridia Missile Refill Room");
        var smAlttp4 = Sm("Golden Torizo Energy Recharge");
        var smM1 = Sm("Bubble Mountain Save Room");
        var smZ1 = Sm("Caterpillar Save Room");

        var alttpZ1 = Alttp("Kakariko Fortune Teller");
        var alttpM1 = Alttp("Lumberjacks House");
        var alttpSm1 = Alttp("Lake Hylia Fortune Teller");
        var alttpSm2 = Alttp("Old Man Home Circle");
        var alttpSm3 = Alttp("Hint Giver Cave");
        var alttpSm4 = Alttp("Mire Big Fairy");

        Link[] canonicalLinks =
        [
            new(z1Alttp, alttpZ1),
            new(m1Alttp, alttpM1),
            new(smAlttp1, alttpSm1),
            new(smAlttp2, alttpSm2),
            new(smAlttp3, alttpSm3),
            new(smAlttp4, alttpSm4),
            new(smM1, m1Sm),
            new(smZ1, z1Sm),
            new(m1Z1, z1M1),
        ];

        var links = new List<Link>();
        var leftovers = new List<Endpoint>();
        foreach (var link in canonicalLinks)
        {
            if (link.A.World != null && link.B.World != null)
                links.Add(link);
            else
            {
                if (link.A.World != null)
                    leftovers.Add(link.A);
                if (link.B.World != null)
                    leftovers.Add(link.B);
            }
        }

        links.AddRange(ReallocateLeftovers(leftovers));

        var usedEndpoints = new HashSet<BaseVertex>(ReferenceEqualityComparer.Instance);

        void Connect(Link link)
        {
            var a = link.A.Resolve();
            var b = link.B.Resolve();
            if (!usedEndpoints.Add(a))
                throw new InvalidOperationException($"Default portal endpoint '{a.Name}' is used more than once");
            if (!usedEndpoints.Add(b))
                throw new InvalidOperationException($"Default portal endpoint '{b.Name}' is used more than once");
            world.ConnectPortal(a, b);
        }

        foreach (var link in links)
            Connect(link);
    }

    /// <summary>Pairs up the endpoints whose canonical partner game is absent:
    /// repeatedly link the two games with the most endpoints left (ties break in
    /// sm, alttp, z1, m1 order), until at most one game has any.</summary>
    private static IEnumerable<Link> ReallocateLeftovers(List<Endpoint> leftovers)
    {
        while (true)
        {
            var games = leftovers
                .GroupBy(endpoint => endpoint.World!)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => GameOrder(group.Key.GameId))
                .ToList();
            if (games.Count < 2)
                yield break;

            var a = games[0].First();
            var b = games[1].First();
            leftovers.Remove(a);
            leftovers.Remove(b);
            yield return new Link(a, b);
        }
    }

    private static int GameOrder(string gameId) => gameId switch
    {
        "sm" => 0,
        "alttp" => 1,
        "z1" => 2,
        "m1" => 3,
        _ => 4,
    };
}
