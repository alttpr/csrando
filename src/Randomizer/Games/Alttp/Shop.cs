using System.Buffers.Binary;

namespace Randomizer.Games.Alttp;

public sealed class Shop
{
    public List<ShopItem> Inventory { get; } = [];

    // 1 for TakeAny caves (can only get one item, not both), 3 for regular shops
    public byte ObtainableInventorySize { get; init; }
    public ushort RoomId { get; init; }
    public byte InletId { get; init; }

    public bool InfiniteStock { get; init; }
    public bool SkipDoorCheck { get; init; }
    public bool IsTakeAll { get; init; }
    public bool AltVram { get; init; }

    // shop_config - tdav --qq
    // t - 0=Shop (buy it once, it's gone), 1=InfiniteStock (keep buying as much as you want)
    // d - 0=Check Door, 1=Skip Door Check
    // a - 0=Shop/TakeAny, 1=TakeAll
    // v - 0=normal vram, 1=alt vram
    // qq - # of items for sale
    public byte ShopConfig => (byte)(
        (InfiniteStock ? 0b1000_0000 : 0) |
        (SkipDoorCheck ? 0b0100_0000 : 0) |
        (IsTakeAll ? 0b0010_0000 : 0) |
        (AltVram ? 0b0001_0000 : 0) |
        (Inventory.Count & 0b0011)
    );

    public byte Palette { get; init; }
    public byte SpriteType { get; init; }
    // shopkeeper_config - ppp- -sss
    // ppp - palette
    // sss - sprite type
    public byte ShopKeeperConfig => (byte)(
        (Palette & 0b0111) << 5 |
        (SpriteType & 0b0111)
    );
    public byte[] GetBytes(byte shopId, byte sramOffset)
    {
        // [id][roomID-low][roomID-high][doorID][zero][shop_config][shopkeeper_config][sram_index]
        Span<byte> shopData = [shopId, 0xFF, 0xFF, InletId, 0x00, ShopConfig, ShopKeeperConfig, sramOffset];

        BinaryPrimitives.WriteUInt16LittleEndian(shopData[1..], RoomId);

        return shopData.ToArray();
    }
}
public sealed class ShopItem
{
    public byte Id { get; init; }
    public ushort Price { get; init; }
    public byte Max { get; init; }
    public byte ReplaceId { get; init; } = 0xFF;
    public ushort ReplacePrice { get; init; }

    public byte[] GetBytes(byte shopId)
    {
        // [id][item][price-low][price-high][max][repl_id][repl_price-low][repl_price-high]
        Span<byte> itemData = [shopId, Id, 0xFF, 0xFF, Max, ReplaceId, 0xFF, 0xFF];

        BinaryPrimitives.WriteUInt16LittleEndian(itemData[2..], Price);
        BinaryPrimitives.WriteUInt16LittleEndian(itemData[6..], ReplacePrice);

        return itemData.ToArray();
    }
}
