namespace Randomizer.Games.Alttp;

internal class EntranceData(byte id)
{
    public byte Id { get; set; } = id;

    public virtual ushort RoomId { get; set; }
    public byte EntranceGraphics { get; set; }
    public byte Floor { get; set; }
    public byte DungeonId { get; set; }
    public byte LandsInDoor { get; set; }
    public byte LayerControl { get; set; }
    public ushort OverworldDoorLocation { get; set; }
    public byte Music { get; set; }

    public ushort LinkX
    {
        get;
        set
        {
            field = value;
            _needsRecalculation = true;
        }
    }

    public ushort LinkY
    {
        get;
        set
        {
            field = value;
            _needsRecalculation = true;
        }
    }

    public byte CameraControl
    {
        get;
        set
        {
            field = value;
            _needsRecalculation = true;
        }
    }

    private bool _needsRecalculation = true;

    // kan:
    // I've made these setters private to discourage/prevent anyone from changing the values.
    // The data these represent is redundant and easily derived from other properties,
    // so I'd rather elide it completely instead of having these properties be mutable and complicating the API surface.
    // This implements the same algorithms as the practice hack/Zscream, which have been stress tested to hell and back.
    public ReadOnlySpan<byte> CameraBoundaries => _cameraBoundaries;
    private System.Runtime.CompilerServices.InlineArray8<byte> _cameraBoundaries;

    public ushort CameraX
    {
        get
        {
            RecalculatePositionBoundaries();
            return field;
        }
        private set;
    }

    public ushort CameraY
    {
        get
        {
            RecalculatePositionBoundaries();
            return field;
        }
        private set;
    }

    public ushort CameraTriggerX
    {
        get
        {
            RecalculatePositionBoundaries();
            return field;
        }
        private set;
    }

    public ushort CameraTriggerY
    {
        get
        {
            RecalculatePositionBoundaries();
            return field;
        }
        private set;
    }

    public byte Quadrant
    {
        get
        {
            RecalculatePositionBoundaries();
            return field;
        }
        private set;
    }

    const ushort PositionResolutionMask = 0xFE00;

    /// <summary>
    /// Validate position.
    /// </summary>
    public bool PositionMatchesRoom()
    {
        int posX = LinkX & PositionResolutionMask;
        int posY = LinkY & PositionResolutionMask;

        int expectedX = (RoomId & 0x000F) << 9;
        int expectedY = (RoomId & 0xFFF0) << 5;

        return posX == expectedX && posY == expectedY;
    }

    private void RecalculatePositionBoundaries()
    {
        if (!_needsRecalculation)
        {
            return;
        }

        int posX = LinkX;
        int posY = LinkY;

        const ushort CameraTriggerMask = 0x1F8;
        ushort triggerX = ushort.Clamp((ushort)(posX & CameraTriggerMask), 128, 383);
        ushort triggerY = ushort.Clamp((ushort)(posY & CameraTriggerMask), 112, 392);

        CameraTriggerX = triggerX;
        CameraTriggerY = triggerY;

        ushort cameraX = (ushort)(triggerX + (posX & PositionResolutionMask));
        ushort cameraY = (ushort)(triggerY + (posY & PositionResolutionMask));

        // make sure camera is locked to inside the room
        cameraX = (ushort)((cameraX & 0x1FF) switch
        {
            <= 150 => posX & PositionResolutionMask,
            >= 350 => (posX & PositionResolutionMask) + 256 + 16,
            _ => cameraX - 112,
        });

        cameraY = (ushort)((cameraY & 0x1FF) switch
        {
            <= 150 => posY & PositionResolutionMask,
            >= 350 => (posY & PositionResolutionMask) + 256 + 16,
            _ => cameraY - 112,
        });

        CameraX = cameraX;
        CameraY = cameraY;

        _cameraBoundaries[0] = (byte)(cameraY >> 8);
        _cameraBoundaries[1] = (byte)((cameraY >> 8) & 0xFE);
        _cameraBoundaries[2] = (byte)(cameraY >> 8);
        _cameraBoundaries[3] = (byte)((cameraY >> 8) | 1);

        _cameraBoundaries[4] = (byte)(cameraX >> 8);
        _cameraBoundaries[5] = (byte)((cameraX >> 8) & 0xFE);
        _cameraBoundaries[6] = (byte)(cameraX >> 8);
        _cameraBoundaries[7] = (byte)((cameraX >> 8) | 1);

        // calculate quadrant
        posX &= 0x100;
        posX >>= 4; // put X high byte bit 0 into bit 4 of quadrant

        posY &= 0x100;
        posY >>= 7; // put Y high byte bit 0 into bit 1 of quadrant

        Quadrant = (byte)(posX | posY);

        _needsRecalculation = false;
    }
}

// These share all the same data, so no need to duplicate it.
// We may also end up having cases where we want to change both the spawn point
// and its associated entrance.
internal class SpawnPointData(byte id) : EntranceData(id)
{
    public ushort AssociatedEntranceID { get; set; }
}
