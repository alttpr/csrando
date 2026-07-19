namespace RandomizerTests.Games.SuperMetroid;

using Randomizer.Games;
using Randomizer.Games.SuperMetroid;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using Randomizer.RomModifications;
using World = Randomizer.Games.SuperMetroid.World;

[TestClass]
public sealed class RomTest
{
    private static readonly HashSet<ushort> EyeDoorPlmIds =
    [
        0xDB48, 0xDB4C, 0xDB52,
        0xDB56, 0xDB5A, 0xDB60,
    ];

    private static readonly byte[] NothingPlm = [0x2F, 0xB6, 0x00, 0x00, 0x00, 0x00];

    private static readonly Dictionary<ushort, byte[]> BossKeycardDoorHeaders = new()
    {
        [0xA56B] = [0x14, 0xD4, 0x1E, 0x16], // Kraid: left-facing keycard door
        [0xB37A] = [0x1A, 0xD4, 0x01, 0x06], // Ridley: right-facing keycard door
        [0xCC6F] = [0x14, 0xD4, 0x4E, 0x06], // Phantoon: left-facing keycard door
        [0xD78F] = [0x1A, 0xD4, 0x01, 0x26], // Draygon: right-facing keycard door
    };

    private sealed class MemoryRom : IRom
    {
        private readonly byte[] _data = Enumerable.Repeat((byte)0xCC, 8 * 1024 * 1024).ToArray();

        public byte[] Read(Address address, int length) => _data[address.Value..(address.Value + length)];
        public void Write(Address address, in ReadOnlySpan<byte> data) => data.CopyTo(_data.AsSpan(address.Value));
        public void ApplyBasePatch(FileInfo baseBPS) => throw new NotSupportedException();
        public void Resize(int size) { }
        public void UpdateChecksum() { }
        public void Dispose() { }
    }

    [TestMethod]
    public void MapRando_ReplacesEveryCompleteGadoraPlmRecord()
    {
        var world = CreateMapRandoWorld(Keycards.None);
        var rom = new MemoryRom();

        new Rom(rom, 0).WritePlms(world);

        var eyeDoorPlms = world.JsonData.RoomPLMs
            .Where(plm => EyeDoorPlmIds.Contains(plm.PlmId))
            .ToList();
        Assert.AreEqual(15, eyeDoorPlms.Count, "Expected three PLMs for each of the five vanilla Gadoras");
        Assert.AreEqual(5, eyeDoorPlms.GroupBy(plm => (plm.Room, plm.State)).Count(),
            "Expected five vanilla Gadora room states");

        foreach (var plm in eyeDoorPlms)
        {
            CollectionAssert.AreEqual(NothingPlm,
                rom.Read((SNES)(plm.Address + plm.PlmIndex * 6), NothingPlm.Length),
                $"Gadora PLM {plm.PlmId:X4} in room {plm.Room:X4}, state {plm.State:X4}");
        }
    }

    [TestMethod]
    public void MapRando_KeycardsReplaceBossGadorasAfterCleanup()
    {
        var world = CreateMapRandoWorld(Keycards.All);
        var rom = new MemoryRom();

        new Rom(rom, 0).WritePlms(world);

        var groups = world.JsonData.RoomPLMs
            .Where(plm => EyeDoorPlmIds.Contains(plm.PlmId))
            .GroupBy(plm => (plm.Room, plm.State))
            .ToList();
        Assert.AreEqual(5, groups.Count);

        foreach (var group in groups)
        {
            var plms = group.OrderBy(plm => plm.PlmIndex).ToList();
            Assert.AreEqual(3, plms.Count);

            // The Tourian Gadora has no keycard replacement. The four boss Gadoras do:
            // their eye PLM becomes the keycard door and both shutter PLMs stay deleted.
            if (group.Key.Room == 0xDDC4)
            {
                foreach (var plm in plms)
                {
                    CollectionAssert.AreEqual(NothingPlm,
                        rom.Read((SNES)(plm.Address + plm.PlmIndex * 6), NothingPlm.Length));
                }
            }
            else
            {
                CollectionAssert.AreEqual(BossKeycardDoorHeaders[group.Key.Room],
                    rom.Read((SNES)(plms[0].Address + plms[0].PlmIndex * 6), 4),
                    $"Boss Gadora in room {group.Key.Room:X4} should become a keycard door");
                foreach (var plm in plms.Skip(1))
                {
                    CollectionAssert.AreEqual(NothingPlm,
                        rom.Read((SNES)(plm.Address + plm.PlmIndex * 6), NothingPlm.Length));
                }
            }
        }
    }

    private static World CreateMapRandoWorld(Keycards keycards)
    {
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config { Keycards = keycards },
        }, new Randomizer.Graph.Graph(), new PRNG(1234));
        world.Map = new Map([], [], [], [], [], [], [], [], [], [], []);
        return world;
    }
}
