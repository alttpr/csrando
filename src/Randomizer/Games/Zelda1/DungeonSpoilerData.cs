namespace Randomizer.Games.Zelda1;

using System.Collections.Generic;

internal record DungeonSpoilerData(
    int Level,
    int Width,
    int Height,
    List<RoomSpoilerData> Rooms
)
{
    /// <summary>
    /// Populated later during AppendSpoiler, maps room coordinates "x,y" to placed item name.
    /// </summary>
    public Dictionary<string, string> ItemPlacements { get; set; } = [];
}

internal record RoomSpoilerData(
    int X,
    int Y,
    string[] Roles,
    Dictionary<string, string> Doors,
    string? Enemy,
    int EnemyCount,
    int Segment,
    string Screen,
    int[][]? ConnectedTo
);
