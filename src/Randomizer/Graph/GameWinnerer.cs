namespace Randomizer.Graph;

/// <summary>Set Game win conditions.</summary>
internal sealed class GameWinnerer : IWorldModifier
{
    public static void AdjustEdges(World world, PRNG prng)
    {
        switch (world.Config.Goal)
        {
            case GoalOption.Ganon:
            case GoalOption.FastGanon:
            case GoalOption.Dungeons:
                var ganon = world.GetLocation("Ganon");
                ganon.Item = world.GetItem("Triforce");
                break;
            case GoalOption.Pedestal:
                var pedestal = world.GetLocation("Master Sword Pedestal");
                pedestal.Item = world.GetItem("Triforce");
                break;
            case GoalOption.TriforceHunt:
                var murahdahla = world.Graph.AddVertex(new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "Murahdahla",
                    World = world,
                    Item = world.GetItem("Triforce"),
                });
                var courtyard = world.GetLocation("Hyrule Castle - Courtyard");
                world.Graph.AddDirected(courtyard, murahdahla, world.GetItem("TriforcePiece"), world.Config.GoalRequiredCount);
                break;
            default:
                throw new ArgumentException("Unknown Goal option: " + world.Config.Goal);
        }

    }
}
