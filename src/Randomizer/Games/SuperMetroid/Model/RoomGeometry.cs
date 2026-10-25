namespace Randomizer.Games.SuperMetroid.Model;

#pragma warning disable IDE1006 // Naming Styles: These are direct JSON member names, and it's easier to suppress the rule than sprinkling attributes
public record GeometryDoor(
    string direction,
    int x,
    int y,
    int? exit_ptr,
    int? entrance_ptr,
    string subtype,
    int? offset
);

public record GeometryItem(
    int x,
    int y,
    int addr
);

public record RoomGeometry(
    int room_id,
    string name,
    int area,
    int rom_address,
    int? twin_rom_address,
    int[][] map,
    GeometryDoor[] doors,
    int[][] parts,
    int[][] durable_part_connections,
    int[][] transient_part_connections,
    GeometryItem[] items,
    object node_tiles,
    object? twin_node_tiles,
    bool heated
);
#pragma warning restore IDE1006 // Naming Styles
