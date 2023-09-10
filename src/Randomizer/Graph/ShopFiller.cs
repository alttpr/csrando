namespace AlttpRandomizer.Graph;

/**
 * Fill initial shop state.
 */
internal sealed class ShopFiller
{
    /**
     * @param World world world to reduce graph for
     * 
     * @return void
     */
    public ShopFiller(World world)
    {
        var shops = world.GetLocationsOfType("shop");
        var graph = world.Graph;

        if (world.RandomizerConfig.RegionShopSupply == ShopSupplyOption.Shuffled)
        {
            foreach (var shop in shops)
            {
                // Potion shop canot be modified at this time.
                if (shop.Name == $"Potion Shop:{world.Id}")
                {
                    continue;
                }
                var inventory = graph.GetTargets(shop).Where(target => target.Type == "shopitem");
                foreach (var shop_item in inventory)
                {
                    shop_item.Item = null;
                    shop_item.Cost = null;
                }
            }
        }

        if (world.RandomizerConfig.State == StateOption.Inverted)
        {
            // put blue potion in DW shop.
        }
    }

    /**
     * No edge adjustment is necessary with shop inventories.
     */
    public void AdjustEdges()
    {
    }
}
