namespace Randomizer.Games.Alttp.WorldModifiers;

using System.Buffers;
using Randomizer.Graph;

internal sealed class EnemyStatRandomizer : IAlttpWorldModifier
{
    public void AdjustEdges(World world, PRNG prng)
    {
        RandomizeEnemyDamage(world, prng);
        RandomizeEnemyHealth(world, prng);
    }

    /// <summary>
    /// Randomizes enemy damage data
    /// </summary>
    public void RandomizeEnemyDamage(World world, PRNG prng)
    {
        var opt = world.Config.EnemyDamage;

        if (opt is EnemyDamageOption.Default)
        {
            return;
        }


        var sprites = world.SpriteData.Values;

        if (opt is EnemyDamageOption.Shuffled)
        {
            byte[] tmp = ArrayPool<byte>.Shared.Rent(sprites.Count);

            // TODO is there a cleaner way to write this?
            try
            {
                Span<byte> dmg = tmp.AsSpan(0, sprites.Count);

                int i = 0;

                foreach (var sprite in sprites)
                {
                    dmg[i++] = sprite.BumpDamageClass;
                }

                prng.ShuffleSpan(dmg);

                i = 0;
                foreach (var sprite in sprites)
                {
                    sprite.BumpDamageClass = dmg[i++];
                }
            } finally
            {
                ArrayPool<byte>.Shared.Return(tmp);
            }
        } else if (opt is EnemyDamageOption.Random)
        {
            foreach (var sprite in sprites)
            {
                sprite.BumpDamageClass = (byte) prng.GetRandomInt(10);
            }
        }
    }

    /// <summary>
    /// Randomizes enemy health data
    /// </summary>
    public void RandomizeEnemyHealth(World world, PRNG prng)
    {
        if (world.Config.EnemyHealth == EnemyHealthOption.Default)
            return;

        Range range = world.Config.EnemyHealth switch
        {
            EnemyHealthOption.Easy => 1..4,
            EnemyHealthOption.Medium => 2..15,
            EnemyHealthOption.Hard => 2..25,
            EnemyHealthOption.Expert => 4..50,
            _ => 1..1,
        };

        foreach (var (_, sprite) in world.SpriteData)
        {
            // TODO only sprite worth skipping right now; nothing else from the previous banned list was necessary
            if (sprite.ID is 0xA3) continue; // Kholdstare shell

            sprite.Property_HP = (byte) prng.GetRandomInt(range);
        }
    }
}
