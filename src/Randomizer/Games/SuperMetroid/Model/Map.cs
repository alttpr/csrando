namespace Randomizer.Games.SuperMetroid.Model;

public record Map(
    List<int> room_id,
    List<int> room_x,
    List<int> room_y,
    List<int> room_area,
    List<int> room_subarea,
    List<int> room_subsubarea,
    List<int> conn_from_room_id,
    List<int> conn_from_door_id,
    List<int> conn_to_room_id,
    List<int> conn_to_door_id,
    List<bool> conn_bidirectional
);


public class MapRoom
{
    public int Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Area { get; init; }
    public int SubArea { get; init; }
    public int SubSubArea { get; init; }
    public required RoomGeometry Room { get; init; }
}
public class MapConnection
{
    public int FromRoomId { get; init; }
    public required MapRoom FromRoom { get; init; }
    public int FromDoorId { get; init; }
    public required GeometryDoor FromDoor { get; init; }
    public int ToRoomId { get; init; }
    public required MapRoom ToRoom { get; init; }
    public int ToDoorId { get; init; }
    public required GeometryDoor ToDoor { get; init; }
    public bool Bidirectional { get; init; }

}

// This takes the raw map data and roomgeometry data to build a more useful map representation
public class MapInfo
{
    public Map MapData { get; init; }
    public List<MapRoom> Rooms { get; init; }
    public List<MapConnection> Connections { get; init; }

    public MapInfo(Map mapData, List<RoomGeometry> roomGeometries)
    {
        MapData = mapData;
        Rooms = MapData.room_id.Select((id, index) => new MapRoom
        {
            Id = id,
            X = MapData.room_x[index],
            Y = MapData.room_y[index],
            Area = MapData.room_area[index],
            SubArea = MapData.room_subarea[index],
            SubSubArea = MapData.room_subsubarea[index],
            Room = roomGeometries.First(rg => rg.room_id == id)
        }).ToList();

        Connections = MapData.conn_from_room_id.Select((fromRoomId, index) =>
        {
            var toRoomId = MapData.conn_to_room_id[index];
            var fromDoorId = MapData.conn_from_door_id[index];
            var toDoorId = MapData.conn_to_door_id[index];
            var fromRoom = Rooms.First(r => r.Id == fromRoomId);
            var toRoom = Rooms.First(r => r.Id == toRoomId);
            var fromDoor = fromRoom.Room.doors[fromDoorId];
            var toDoor = toRoom.Room.doors[toDoorId];

            return new MapConnection
            {
                FromRoomId = fromRoomId,
                FromRoom = fromRoom,
                FromDoorId = fromDoorId,
                FromDoor = fromDoor,
                ToRoomId = toRoomId,
                ToRoom = toRoom,
                ToDoorId = toDoorId,
                ToDoor = toDoor,
                Bidirectional = MapData.conn_bidirectional[index]
            };
        }).ToList();
    }

    public MapRoom? GetRoomById(int id) => Rooms.FirstOrDefault(r => r.Id == id);
    public MapRoom? GetRoomByName(string name) => Rooms.FirstOrDefault(r => r.Room.name.Equals(name, StringComparison.OrdinalIgnoreCase));

    // Get the room that has a door with the given exit_ptr
    public MapRoom? GetRoomByExitPtr(int doorPtr)
    {
        return GetConnectionByExitPtr(doorPtr)?.FromRoom;
    }

    // Get the room that has a door with the given entrance_ptr
    public MapRoom? GetRoomByEntrancePtr(int doorPtr)
    {
        return GetConnectionByEntrancePtr(doorPtr)?.ToRoom;
    }

    // Get connection by exit_ptr
    public MapConnection? GetConnectionByExitPtr(int doorPtr)
    {
        return Connections.FirstOrDefault(c => c.FromDoor.exit_ptr == doorPtr);
    }

    // Get connection by entrance_ptr
    public MapConnection? GetConnectionByEntrancePtr(int doorPtr)
    {
        return Connections.FirstOrDefault(c => c.ToDoor.entrance_ptr == doorPtr);
    }

}
