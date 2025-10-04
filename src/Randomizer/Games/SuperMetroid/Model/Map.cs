namespace Randomizer.Games.SuperMetroid.Model;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(CoordConverter))]
public record MapCoord(int x, int y);

public record MapDoorPair(int? exit_ptr, int? entrance_ptr);

[JsonConverter(typeof(DoorConverter))]
public record MapDoor(MapDoorPair from, MapDoorPair to, bool bidirectional);

public record Map(
    MapCoord[] rooms,
    MapDoor[] doors,
    int[] toilet_intersections,
    int[] area,
    int[] subarea,
    int[] subsubarea
);


public class CoordConverter : JsonConverter<MapCoord>
{
    public override MapCoord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        int x = reader.GetInt32();
        reader.Read();
        int y = reader.GetInt32();
        reader.Read();
        return new MapCoord(x, y);
    }

    public override void Write(Utf8JsonWriter writer, MapCoord value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

public class DoorConverter : JsonConverter<MapDoor>
{
    public override MapDoor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();

        reader.Read();
        int? from_exit_ptr = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
        reader.Read();
        int? from_entrance_ptr = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();

        reader.Read();
        reader.Read();

        reader.Read();
        int? to_exit_ptr = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
        reader.Read();
        int? to_entrance_ptr = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();

        reader.Read();

        reader.Read();
        bool bidirectional = reader.GetBoolean();

        reader.Read();
        return new MapDoor(new MapDoorPair(from_exit_ptr, from_entrance_ptr), new MapDoorPair(to_exit_ptr, to_entrance_ptr), bidirectional);
    }

    public override void Write(Utf8JsonWriter writer, MapDoor value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
