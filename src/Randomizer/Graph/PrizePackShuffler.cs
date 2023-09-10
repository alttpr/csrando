namespace AlttpRandomizer.Graph;

/**
 * Modify Prizepacks based on configuration.
 */
internal sealed class PrizePackShuffler
{
    private readonly World _world;
    /**
     * @param World world 
     *
     * @return void
     */
    public PrizePackShuffler(World world)
    {
        _world = world;
    }

    /**
     * Pick items for each prize pack.
     */
    public void AdjustEdges()
    {
        var prizepacks = _world.GetLocationsOfType("prizepack");

        if (!_world.Config("customPrizePacks", false))
        {
            var random_vanilla_packs = new Stack<string>(PHP.fy_shuffle(new[]
            {
                new[] { "Heart", "Heart", "Heart", "Heart", "RupeeGreen", "Heart", "Heart", "RupeeGreen" },
                new[] { "RupeeBlue", "RupeeGreen", "RupeeBlue", "RupeeRed", "RupeeBlue", "RupeeGreen", "RupeeBlue", "RupeeBlue" },
                new[] { "MagicRefillFull", "MagicRefillSmall", "MagicRefillSmall", "RupeeBlue", "MagicRefillFull", "MagicRefillSmall", "Heart", "MagicRefillSmall" },
                new[] { "BombRefill1", "BombRefill1", "BombRefill1", "BombRefill4", "BombRefill1", "BombRefill1", "BombRefill8", "BombRefill1" },
                new[] { "ArrowRefill5", "Heart", "ArrowRefill5", "ArrowRefill10", "ArrowRefill5", "Heart", "ArrowRefill5", "ArrowRefill10" },
                new[] { "MagicRefillSmall", "RupeeGreen", "Heart", "ArrowRefill5", "MagicRefillSmall", "BombRefill1", "RupeeGreen", "Heart" },
                new[] { "Heart", "Fairy", "MagicRefillFull", "RupeeRed", "BombRefill8", "Heart", "RupeeRed", "ArrowRefill10" },
            }).SelectMany(s => s));

            var prizepacksOrdered = prizepacks.OrderBy(v => v.Offset);
            foreach (var pack in prizepacksOrdered)
            {
                if (!random_vanilla_packs.TryPop(out string? spriteName))
                {
                    pack.Sprite = null;
                }
                else
                {
                    pack.Sprite = Sprite.Get(spriteName);
                }
            }
        }

        var emptypacks = prizepacks.Where((pack) => pack.Sprite is null);

        if (emptypacks.Any())
        {
            // TODO refactor this
            var drops = new List<Sprite>();
            // TODO: what is the type of "item.drop"?
            foreach (var (sprite_name, count) in _world.Config("item.drop", Enumerable.Empty<(string SpriteName, int Count)>()))
            {
                drops.AddRange(Enumerable.Repeat(Sprite.Get(sprite_name), Math.Min(_world.Config("drop.count." + sprite_name, count), 63)));
            }
            var drop_pool = new Stack<Sprite>(PHP.fy_shuffle(drops.ToArray()));

            foreach (var pack in emptypacks)
            {
                if (drop_pool.TryPop(out var sprite))
                    pack.Sprite = sprite;
            }
        }

        // hard+ does not allow fairies/full magics
        if (_world.Config("rom.HardMode", 0) >= 2)
        {
            var fairy = Sprite.Get("Fairy");
            var heart = Sprite.Get("Heart");
            var magic = Sprite.Get("MagicRefillFull");
            var small_magic = Sprite.Get("MagicRefillSmall");
            foreach (var prizepack in prizepacks)
            {
                if (prizepack.Sprite == fairy)
                {
                    prizepack.Sprite = heart;
                }
                if (prizepack.Sprite == magic)
                {
                    prizepack.Sprite = small_magic;
                }
            }
        }

        if (_world.Config("rom.rupeeBow", false))
        {
            var arrows5 = Sprite.Get("ArrowRefill5");
            var arrows10 = Sprite.Get("ArrowRefill10");
            var rupeeBlue = Sprite.Get("RupeeBlue");
            var rupeeRed = Sprite.Get("RupeeRed");
            foreach (var prizepack in prizepacks)
            {
                if (prizepack.Sprite == arrows5)
                {
                    prizepack.Sprite = rupeeBlue;
                }
                if (prizepack.Sprite == arrows10)
                {
                    prizepack.Sprite = rupeeRed;
                }
            }
        }
    }
}
