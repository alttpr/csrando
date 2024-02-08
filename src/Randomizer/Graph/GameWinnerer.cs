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
                SetGanonGoal(world);
                break;
            case GoalOption.Pedestal:
                SetPedestalGoal(world);
                break;
            case GoalOption.TriforceHunt:
                SetTriforcePiecesGoal(world);
                break;
            case GoalOption.Trifecta:
                SetGanonGoal(world);
                SetPedestalGoal(world);
                SetTriforcePiecesGoal(world);
                break;
            default:
                throw new ArgumentException("Unknown Goal option: " + world.Config.Goal);
        }

    }

    private static void SetTriforcePiecesGoal(World world)
    {
        var murahdahla = world.Graph.AddVertex(new Vertex
        {
            Type = VertexType.Meta,
            Name = "Murahdahla",
            World = world,
            Item = world.GetItem("Triforce"),
        });
        var courtyard = world.GetLocation("Hyrule Castle - Courtyard");
        world.Graph.AddDirected(courtyard, murahdahla, world.GetItem("TriforcePiece"), world.Config.GoalRequiredCount);
    }

    private static void SetPedestalGoal(World world)
    {
        var pedestal = world.GetLocation("Master Sword Pedestal");
        pedestal.Item = world.GetItem("Triforce");
    }

    private static void SetGanonGoal(World world)
    {
        var ganon = world.GetLocation("Ganon");
        ganon.Item = world.GetItem("Triforce");
    }
}
