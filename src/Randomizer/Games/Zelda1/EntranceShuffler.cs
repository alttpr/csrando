namespace Randomizer.Games.Zelda1;

using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Graph;

internal class EntranceShuffler
{
    private readonly YamlReader.YamlData _data;
    private readonly PRNG _prng;
    private readonly List<int> _portalMapIds;

    public EntranceShuffler(PRNG prng, YamlReader reader, List<int> portalMapIds)
    {
        _data = reader.Data!;
        _prng = prng;
        _portalMapIds = portalMapIds;
    }

    public void Shuffle()
    {
        /* Portal caves keep their vanilla entrances */
        List<int> portalMapIds = _portalMapIds;

        /* Find all caves and dungeon levels */
        var caveIds = _data.overworld_maps.Where(m => !portalMapIds.Contains(m.map) && m.cave > 0 && (m.secret[0] == 1 || m.secret[1] == 0)).Select(m => m.cave).ToList();
        var maps = _data.overworld_maps.Where(m => !portalMapIds.Contains(m.map) && m.cave > 0 && (m.secret[0] == 1 || m.secret[1] == 0)).ToList();

        /* Shuffle the caves */
        caveIds = _prng.Shuffle(caveIds).ToList();
        maps = _prng.Shuffle(maps).ToList();

        /* Assign the shuffled caves to the world */
        var caveQueue = new Queue<int>(caveIds);
        foreach (var map in maps)
        {
            map.cave = caveQueue.Dequeue();
        }

        // Find all four any roads
        var anyRoads = maps.Where(m => m.cave == 0x14).ToList();
        if (anyRoads.Count != 4)
        {
            throw new Exception("Expected 4 any roads");
        }

        var overworldLevel = _data.levels.Single(l => l.level == 0);
        for (int i = 0; i < anyRoads.Count; i++)
        {
            overworldLevel.cellar_room_id_array[i] = anyRoads[i].map;
        }

        // Find all dungeons and write the recorder data
        var dungeons = maps.Where(m => m.cave < 9).OrderBy(m => m.cave).ToList();
        for (int i = 0; i < dungeons.Count; i++)
        {
            _data.special.recorder_dests[i] = (dungeons[i].map % 0x10 == 0) ? dungeons[i].map + 0x0f : dungeons[i].map - 1;
        }

        // The flute (recorder) drop Y is the one landing value indexed by LEVEL rather than by
        // screen, so when dungeons move it no longer matches the destination screen and can drop
        // Link in a wall/water.
        FixFluteLandings(dungeons);
    }

    private YamlReader.Screen? ScreenFor(YamlReader.OverworldMap map)
        => _data.overworld_screens.FirstOrDefault(s => s.screen == map.screen && s.walkable != null);

    // Recompute recorder_y_pos (the flute/whirlwind drop Y) for each dungeon entrance screen.
    private void FixFluteLandings(List<YamlReader.OverworldMap> dungeons)
    {
        for (int i = 0; i < dungeons.Count && i < _data.special.recorder_y_pos.Length; i++)
        {
            var screen = ScreenFor(dungeons[i]);
            if (screen == null)
                continue;

            var y = OverworldWalkability.ComputeFluteY(screen);
            if (y != null)
                _data.special.recorder_y_pos[i] = y.Value;
        }
    }
}
