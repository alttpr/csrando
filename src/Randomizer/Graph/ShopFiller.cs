namespace Randomizer.Graph;

/// <summary>Fill initial shop state.</summary>
internal sealed class ShopFiller : IWorldModifier
{
    public void AdjustEdges(World world, PRNG prng)
    {
        var shops = world.GetLocationsOfType(VertexType.Shop);
        var graph = world.Graph;

        if (world.Config.RegionShopSupply == ShopSupplyOption.Shuffled)
        {
            foreach (var shop in shops)
            {
                // Potion shop canot be modified at this time.
                if (shop.Name == "Potion Shop")
                    continue;

                // TODO: Fix
                // var inventory = graph.GetTargets(shop).Where(target => target.Type == VertexType.ShopItem);
                // foreach (var shopItem in inventory)
                // {
                //     shopItem.Item = null;
                //     shopItem.Cost = null;
                // }
            }
        }

        if (world.Config.State == StateOption.Inverted)
        {
            // put blue potion in DW shop.
        }
    }
}
