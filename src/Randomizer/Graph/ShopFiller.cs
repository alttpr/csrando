namespace Randomizer.Graph;

/**
 * Fill initial shop state.
 */
internal sealed class ShopFiller : IWorldModifier
{
    public static void AdjustEdges(World world, PRNG prng)
    {
        var shops = world.GetLocationsOfType(VertexType.Shop);
        var graph = world.Graph;

        if (world.Config.RegionShopSupply == ShopSupplyOption.Shuffled)
        {
            foreach (var shop in shops)
            {
                // Potion shop canot be modified at this time.
                if (shop.Name == "Potion Shop")
                {
                    continue;
                }

                // TODO: Fix
                // var inventory = graph.GetTargets(shop).Where(target => target.Type == VertexType.ShopItem);
                // foreach (var shop_item in inventory)
                // {
                //     shop_item.Item = null;
                //     shop_item.Cost = null;
                // }
            }
        }

        if (world.Config.State == StateOption.Inverted)
        {
            // put blue potion in DW shop.
        }
    }
}
