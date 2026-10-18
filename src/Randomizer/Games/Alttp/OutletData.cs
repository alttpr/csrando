namespace Randomizer.Games.Alttp;

// TODO question during review. Do we want a new folder/namespace for these game struct handlers? Maybe Randomizer.Games.Alttp.GameStructs?
internal class OutletData(byte id)
{
    public byte OutletId { get; set; } = id;

    /// <summary>
    /// The underworld room ID this outlet exits from.
    /// </summary>
    public ushort RoomId { get; set; }

    /// <summary>
    /// The overworld screen ID this outlet exits to.
    /// </summary>
    public byte OverworldId { get; set; }

    public ushort LinkX { get; set; }
    public ushort LinkY { get; set; }

    public ushort DoorGraphic { get; set; }
    public ushort DoorLocation { get; set; }
    public ushort TilemapDrawOffset { get; set; }
    public ushort CameraPositionX { get; set; }
    public ushort CameraPositionY { get; set; }
    public ushort CameraTriggerY { get; set; }
    public ushort CameraTriggerX { get; set; }
    public ushort ScrollModY { get; set; }
    public ushort ScrollModX { get; set; }

/* TODO this isn't ready yet and needs some investigation...
 * A lot of these properties should be derivable, and that's a good eventually goal.

    /// <summary>
    /// Places Link at the given coordinates on the given screen
    /// then automatically recalculates all other properties from this.
    /// </summary>
    public void RecalculateProperties()
    {
    }
*/

    private void RecalculateCamera()
    {
    }
}
