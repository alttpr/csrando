namespace Randomizer.Games.SuperMetroid.Model;

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
