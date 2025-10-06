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
