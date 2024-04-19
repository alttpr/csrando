namespace Randomizer.Graph;

using System;
using Microsoft.Extensions.Logging;

/// <summary>Modify Prizepacks based on configuration.</summary>
internal sealed class PrizePackShuffler : IWorldModifier
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private static readonly string[][] _vanillaPrizePacks =
    [
        ["Heart", "Heart", "Heart", "Heart", "RupeeGreen", "Heart", "Heart", "RupeeGreen"],
        ["RupeeBlue", "RupeeGreen", "RupeeBlue", "RupeeRed", "RupeeBlue", "RupeeGreen", "RupeeBlue", "RupeeBlue"],
        ["MagicRefillFull", "MagicRefillSmall", "MagicRefillSmall", "RupeeBlue", "MagicRefillFull", "MagicRefillSmall", "Heart", "MagicRefillSmall"],
        ["BombRefill1", "BombRefill1", "BombRefill1", "BombRefill4", "BombRefill1", "BombRefill1", "BombRefill8", "BombRefill1"],
        ["ArrowRefill5", "Heart", "ArrowRefill5", "ArrowRefill10", "ArrowRefill5", "Heart", "ArrowRefill5", "ArrowRefill10"],
        ["MagicRefillSmall", "RupeeGreen", "Heart", "ArrowRefill5", "MagicRefillSmall", "BombRefill1", "RupeeGreen", "Heart"],
        ["Heart", "Fairy", "MagicRefillFull", "RupeeRed", "BombRefill8", "Heart", "RupeeRed", "ArrowRefill10"],
    ];
    private static readonly string[] _prizeSprites =
    [
        "ArrowRefill5", "ArrowRefill10",
        "BombRefill1", "BombRefill4", "BombRefill8",
        "Fairy", "Heart",
        "MagicRefillSmall", "MagicRefillFull",
        "RupeeGreen", "RupeeBlue", "RupeeRed",
        //"Bee", "BeeGood", // NOT THE BEES
        "Apple",
        //"Chicken", "FloppingFish", // because why not? sprite sheets, thats why.
    ];

    /// <summary>Pick items for each prize pack.</summary>
    public static void AdjustEdges(World world, PRNG prng)
    {
        var prizepacks = world.GetLocationsOfType(VertexType.PrizePack);

        if (!world.Config.CustomPrizePacks)
        {
            var randomVanillaPacks = new Stack<string>(prng.Shuffle(_vanillaPrizePacks).SelectMany(s => s));

            var packsByCount = prizepacks.ToLookup(v => v.Name.Split('-', 2)[0]);
            // 8-member groups are drop packs; the rest (crab, tree pull, stun) is filled thru the empty packs fallback
            var dropPacks = prng.Shuffle(packsByCount.Where(g => g.Count() == 8));
            foreach (var pack in dropPacks.SelectMany(g => g.OrderBy(v => v.Name)))
            {
                // TODO: this loop doesn't account for allow/deny, but the sprites are hardcoded
                //       and we don't have either on the regular prize packs right now.
                if (!randomVanillaPacks.TryPop(out string? spriteName))
                    pack.Sprite = null;
                else
                    pack.Sprite = Sprite.Get(spriteName);

                _logger.LogInformation("[PP] Placing '{Sprite}' in '{PrizePack}'", pack.Sprite?.Name, pack.Name);
            }

            // TODO: this doesn't keep multi-drop packs (crab and tree pulls) together...
            //       ...it wont matter until we want predefined packs that give something somewhat deterministic.
            var otherPacks = prng.Shuffle(packsByCount.Where(g => g.Count() != 8).SelectMany(g => g));
            foreach (var pack in otherPacks)
            {
                IEnumerable<string> prizeSprites = _prizeSprites;
                // allow-list wins over deny. don't put things in allow if you don't want it.
                if (pack.Allow?.Length > 0)
                    prizeSprites = prizeSprites.Intersect(pack.Allow); // TODO: intersect, or allow anything?
                else if (pack.Deny?.Length > 0)
                    prizeSprites = prizeSprites.Except(pack.Deny);

                pack.Sprite = Sprite.Get(prng.GetRandomElement(prizeSprites));
                _logger.LogInformation("[PP] Placing '{Sprite}' in '{PrizePack}'", pack.Sprite.Name, pack.Name);
            }
        }

        var emptypacks = prizepacks.Where((pack) => pack.Sprite is null);
        if (emptypacks.Any())
        {
            var drops = new List<Sprite>();
            // TODO: this is supposed to fill any slots are that still empty; normally used with custom prize packs.
            //       the old configuration "item.drop" was pairs of the item to use and its maximum count.
            /*
            foreach (var (sprite_name, count) in _world.Config("item.drop", Enumerable.Empty<(string SpriteName, int Count)>()))
            {
                drops.AddRange(Enumerable.Repeat(Sprite.Get(sprite_name), Math.Min(_world.Config("drop.count." + sprite_name, count), 63)));
            }*/
            var dropPool = new Stack<Sprite>(prng.Shuffle(drops.ToArray()));

            foreach (var pack in emptypacks)
            {
                if (dropPool.TryPop(out var sprite))
                    pack.Sprite = sprite;
            }
        }

        // hard+ does not allow fairies/full magics
        if (world.Config.RomHardMode >= 2)
        {
            var fairy = Sprite.Get("Fairy");
            var heart = Sprite.Get("Heart");
            var magic = Sprite.Get("MagicRefillFull");
            var smallMagic = Sprite.Get("MagicRefillSmall");
            foreach (var prizepack in prizepacks)
            {
                if (prizepack.Sprite == fairy)
                    prizepack.Sprite = heart;
                if (prizepack.Sprite == magic)
                    prizepack.Sprite = smallMagic;
            }
        }

        if (world.Config.RomRupeeBow)
        {
            var arrows5 = Sprite.Get("ArrowRefill5");
            var arrows10 = Sprite.Get("ArrowRefill10");
            var rupeeBlue = Sprite.Get("RupeeBlue");
            var rupeeRed = Sprite.Get("RupeeRed");
            foreach (var prizepack in prizepacks)
            {
                if (prizepack.Sprite == arrows5)
                    prizepack.Sprite = rupeeBlue;
                if (prizepack.Sprite == arrows10)
                    prizepack.Sprite = rupeeRed;
            }
        }
    }
}
