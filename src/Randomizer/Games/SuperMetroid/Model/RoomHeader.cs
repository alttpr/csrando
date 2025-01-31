namespace Randomizer.Games.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

public class RoomHeader
{
    [JsonPropertyName("address_24_bit")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public int Address { get; set; }

    [JsonPropertyName("room_index")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte RoomIndex { get; set; }

    [JsonPropertyName("room_area")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte RoomArea { get; set; }

    [JsonPropertyName("x_on_map")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte XOnMap { get; set; }

    [JsonPropertyName("y_on_map")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte YOnMap { get; set; }

    [JsonPropertyName("width_of_room")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte WidthOfRoom { get; set; }

    [JsonPropertyName("height_of_room")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte HeightOfRoom { get; set; }

    [JsonPropertyName("up_scroller_value")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte UpScrollerValue { get; set; }

    [JsonPropertyName("down_scroller_value")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte DownScrollerValue { get; set; }

    [JsonPropertyName("special_graphics_bitflag")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte SpecialGraphicsBitflag { get; set; }

    [JsonPropertyName("door_out_pointer_16_bit")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public ushort DoorOutPointer { get; set; }
}

public class DoorPLMMap
{
    public int RoomAddress { get; set; }
    public int DoorAddress { get; set; }
    public int RoomId { get; set; }
    public int NodeId { get; set; }
    public int XPosition { get; set; }
    public int YPosition { get; set; }
    public List<RoomPLM>? PLMs { get; set; }
}

public class RoomPLM
{
    [JsonPropertyName("room")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public ushort Room { get; set; }

    [JsonPropertyName("state")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public ushort State { get; set; }

    [JsonPropertyName("address")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public int Address { get; set; }

    [JsonPropertyName("plm_index")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte PlmIndex { get; set; }

    [JsonPropertyName("plm_id")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public ushort PlmId { get; set; }

    [JsonPropertyName("x_position")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte XPosition { get; set; }

    [JsonPropertyName("y_position")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public byte YPosition { get; set; }

    [JsonPropertyName("main_plm_variable")]
    [JsonConverter(typeof(HexStringToIntConverter))]
    public ushort MainPlmVariable { get; set; }

    // Try to use the DoorTypePlm enum instead of a raw ushort, and return DoorType.None if the conversion fails
    public DoorTypePlm DoorType => Enum.IsDefined(typeof(DoorTypePlm), PlmId) ? (DoorTypePlm)PlmId : DoorTypePlm.Unknown;
}

public enum DoorTypePlm : ushort
{
    Unknown = 0,
    GreyDoorLeft = 0xC842,
    GreyDoorRight = 0xC848,
    GreyDoorUp = 0xC84E,
    GreyDoorDown = 0xC854,
    YellowDoorLeft = 0xC85A,
    YellowDoorRight = 0xC860,
    YellowDoorUp = 0xC866,
    YellowDoorDown = 0xC86C,
    GreenDoorLeft = 0xC872,
    GreenDoorRight = 0xC878,
    GreenDoorUp = 0xC87E,
    GreenDoorDown = 0xC884,
    RedDoorLeft = 0xC88A,
    RedDoorRight = 0xC890,
    RedDoorUp = 0xC896,
    RedDoorDown = 0xC89C,
    Nothing = 0xB62F
}

public static class DoorTypeExtensions
{
    public static bool IsRed(this DoorTypePlm door) => door.ToString().StartsWith("Red");
    public static bool IsYellow(this DoorTypePlm door) => door.ToString().StartsWith("Yellow");
    public static bool IsGreen(this DoorTypePlm door) => door.ToString().StartsWith("Green");
    public static bool IsGrey(this DoorTypePlm door) => door.ToString().StartsWith("Grey");
}

public class HexStringToIntConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(int) ||
        typeToConvert == typeof(ushort) ||
        typeToConvert == typeof(byte);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(int))
            return new HexStringConverter<int>(int.Parse, System.Globalization.NumberStyles.HexNumber);
        if (typeToConvert == typeof(ushort))
            return new HexStringConverter<ushort>(ushort.Parse, System.Globalization.NumberStyles.HexNumber);
        if (typeToConvert == typeof(byte))
            return new HexStringConverter<byte>(byte.Parse, System.Globalization.NumberStyles.HexNumber);

        throw new NotSupportedException();
    }
}

public class HexStringConverter<T> : JsonConverter<T>
{
    private readonly Func<string, System.Globalization.NumberStyles, T> _parser;
    private readonly System.Globalization.NumberStyles _numberStyles;

    public HexStringConverter(Func<string, System.Globalization.NumberStyles, T> parser, System.Globalization.NumberStyles numberStyles)
    {
        _parser = parser;
        _numberStyles = numberStyles;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string hexString = reader.GetString().Replace("0x", "");
        return _parser(hexString, _numberStyles);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"0x{value:X}");
    }
}
