namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;

/// <summary>
/// A vanilla save station Samus can start (and initially respawn) at: the game boots
/// from the initial SRAM template, so a start location is just the station's
/// (area, load-station slot) pair written into that template — the station's own
/// 14-byte load table row already exists in the vanilla ROM, and the map randomizer
/// rewires every row's entrance door when it shuffles the world.
/// </summary>
/// <param name="RoomName">The sm-json-data room name (also the config option value).</param>
/// <param name="VertexName">The station node's graph vertex ("{Area} - {Room} - {Node}").</param>
/// <param name="Area">Vanilla area index (0 Crateria .. 5 Tourian).</param>
/// <param name="Slot">Load-station slot within the area (the save PLM's argument).</param>
/// <param name="RoomId">sm-json-data room id, for map-randomizer area lookups.</param>
public sealed record SaveStation(string RoomName, string VertexName, int Area, int Slot, int RoomId);

public static class SaveStations
{
    private const ushort SaveStationPlmId = 0xB76F;
    private const int TourianArea = 5;

    /// <summary>
    /// Every vanilla save station a seed may start at, ordered by (area, slot) for
    /// deterministic random picks. Tourian stations are excluded: the lower one starts
    /// the seed inside the endgame area and the upper one is converted into a map
    /// station by the map randomizer.
    /// </summary>
    public static List<SaveStation> EligibleStartStations(JsonReader data)
    {
        var geometryByHeader = data.RoomGeometries
            .ToDictionary(g => (ushort)(0x8000 | (g.rom_address & 0x7FFF)));

        var stations = new List<SaveStation>();
        foreach (var room in data.Rooms)
        {
            var saveNode = room.Nodes.FirstOrDefault(n => n.NodeSubType == "save");
            if (saveNode == null || room.RoomAddress == null)
                continue;

            int romAddress = Convert.ToInt32(room.RoomAddress, 16);
            if (!geometryByHeader.TryGetValue((ushort)(0x8000 | (romAddress & 0x7FFF)), out var geometry))
                continue; // e.g. Upper Tourian Save Room, absent from room_geometry
            if (geometry.area == TourianArea)
                continue;

            var plm = data.RoomPLMs.FirstOrDefault(p =>
                p.PlmId == SaveStationPlmId && p.Room == (ushort)(0x8000 | (geometry.rom_address & 0x7FFF)));
            if (plm == null)
                continue;

            stations.Add(new SaveStation(
                room.Name,
                $"{room.Area} - {room.Name} - {saveNode.Name}",
                geometry.area,
                plm.MainPlmVariable,
                geometry.room_id));
        }

        return stations.OrderBy(s => s.Area).ThenBy(s => s.Slot).ToList();
    }
}
