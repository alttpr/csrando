using Randomizer.Graph;

namespace Randomizer.Games.Alttp.WorldModifiers;

/// <summary>Compute final shop state. This runs after randomization and only prepares the actual shops.</summary>
internal sealed class ShopFiller
{
    private static readonly byte[] _shopkeeperChoices = [
        0x00, // light world shopkeeper
        0x01, // dark world shopkeeper
        // 0x02, // broken with our current dynamic sprite sheeting
        // 0x03, // broken with our current dynamic sprite sheeting
    ];
    // NOTE: these mostly affect the light world shopkeeper, the dark world one doesn't seem to care a whole lot.
    private static readonly byte[] _paletteChoices = [
        // 0x00, // inverted colors, look broken
        0x01, // orange
        0x02, // light blue
        0x03, // red
        0x04, // blue/purple
        0x05, // green
        0x06, // yellow
        0x07, // red/orange
    ];
    public static void Update(World world, PRNG prng)
    {
        if (world.Config.RegionShopSupply == ShopSupplyOption.Normal)
            return;

        var shops = world.GetLocationsOfType(VertexType.Shop);
        var randomizedShops = new List<Shop>();
        foreach (var shop in shops)
        {
            var inventory = shop.Edges.Where(e => e.To is Vertex { Type: VertexType.ShopItem }).ToArray();
            var shopData = new Shop(shop)
            {
                AltVram = shop.AlternativeVRAM,
                InfiniteStock = false, // vanilla: shop.InfiniteStock,
                InletId = (byte)shop.InletId.GetValueOrDefault(),
                IsTakeAll = false,
                ObtainableInventorySize = (byte)inventory.Length,
                RoomId = (ushort)shop.RoomId.GetValueOrDefault(),
                SkipDoorCheck = !shop.InletId.HasValue,
                SpriteType = shop.AlternativeVRAM ? shop.Shopkeeper.GetValueOrDefault() : prng.GetRandomElement(_shopkeeperChoices),
                Palette = shop.AlternativeVRAM ? shop.ShopPalette.GetValueOrDefault() : prng.GetRandomElement(_paletteChoices),
            };

            shopData.Inventory.AddRange(inventory.Select(e => e.To).OfType<Vertex>().Select(v => BuildShopItem(v, prng)));
            randomizedShops.Add(shopData);
        }

        if (world.Config.State == StateOption.Inverted)
        {
            // put blue potion in DW shop.
        }

        world.RandomizedShops = randomizedShops.ToArray();
    }

    private static ShopItem BuildShopItem(Vertex vertex, PRNG prng)
    {
        if (vertex.Item is not Item { Price: ushort price } item)
        {
            price = 420;
            item = null!;
        }

        return new ShopItem
        {
            Id = item?.Bytes?[0] ?? 0x53,
            Max = 1,
            Price = RandomizePrize(prng, price),
            Item = item,
        };
    }

    // borrowed from Door Rando
    private static ushort RandomizePrize(PRNG prng, ushort basePrice)
    {
        if (basePrice <= 10)
            return basePrice;

        // get a price that's a multiple of 5, rounded up.
        int halfPrice = (((basePrice / 2) + 4) / 5) * 5;

        // rando-tax for the shopkeeper
        int maxSteps = (basePrice - halfPrice) / 5;
        return (ushort)(prng.GetRandomInt(0..maxSteps) * 5 + halfPrice);
    }
}
