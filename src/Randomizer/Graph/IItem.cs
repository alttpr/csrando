namespace Randomizer.Graph;

public interface IItem
{
    int Id { get; set; }
    string Name { get; }
    IWorld World { get; }
    /// <summary>The permanent health value this item grants the player. Use <c>0</c> (zero) to indicate this item does not add health.</summary>
    float HealthValue { get; }
    /// <summary>Coarse gameplay classification from the game's item data; <see cref="ItemTier.Major"/> when undeclared.</summary>
    ItemTier Tier { get; }
    byte[]? Bytes { get; set; }

    /// <summary>
    /// A twin item for logic purposes when multiple distinct items represent the same logical condition; if there is one.
    /// Return <c>null</c> to indicate the item itself is the logical item (or no logical implications are attached to it).
    /// </summary>
    /// <remarks>
    /// Some games feature items such as bottles or item bags that may contain a series of different contents,
    /// but either of them is good enough to unlock locations. This can be seen as the base or common trait of this item.
    /// </remarks>
    /// <example>
    /// The Legend of Zelda: A Link to the Past requires a bottle to receive an item. Bottles can be found as just the empty bottle,
    /// bought with a potion inside or otherwise obtained with contents.
    /// The contents do not matter for logic purposes, and represent as just the logical bottle.
    /// </example>
    IItem? LogicalItem { get; }
}
