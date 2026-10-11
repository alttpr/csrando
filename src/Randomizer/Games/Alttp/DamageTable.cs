namespace Randomizer.Games.Alttp;

// TODO we should eventually make this data driven
public class DamageTable
{
    public static bool[] DamageSubclassesCanRemove(SpriteProperties props)
    {
        return [
            SubclassCanRemoveSprite(0x0, props.DamageClass0Subclass),
            SubclassCanRemoveSprite(0x1, props.DamageClass1Subclass),
            SubclassCanRemoveSprite(0x2, props.DamageClass2Subclass),
            SubclassCanRemoveSprite(0x3, props.DamageClass3Subclass),
            SubclassCanRemoveSprite(0x4, props.DamageClass4Subclass),
            SubclassCanRemoveSprite(0x5, props.DamageClass5Subclass),
            SubclassCanRemoveSprite(0x6, props.DamageClass6Subclass),
            SubclassCanRemoveSprite(0x7, props.DamageClass7Subclass),
            SubclassCanRemoveSprite(0x8, props.DamageClass8Subclass),
            SubclassCanRemoveSprite(0x9, props.DamageClass9Subclass),
            SubclassCanRemoveSprite(0xA, props.DamageClassASubclass),
            SubclassCanRemoveSprite(0xB, props.DamageClassBSubclass),
            SubclassCanRemoveSprite(0xC, props.DamageClassCSubclass),
            SubclassCanRemoveSprite(0xD, props.DamageClassDSubclass),
            SubclassCanRemoveSprite(0xE, props.DamageClassESubclass),
            SubclassCanRemoveSprite(0xF, props.DamageClassFSubclass),
         ];
    }

    public static bool SubclassCanRemoveSprite(byte damageClass, byte subclass)
    {
        ReadOnlySpan<byte> subclassTable;

        if (damageClass is 0x00)
        {
            subclassTable = [0x00, 0x01, 0x20, 0xFF, 0xFC, 0xFB, 0x00, 0x00];
        }
        else if (damageClass is 0x01)
        {
            subclassTable = [0x00, 0x02, 0x40, 0x04, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x02)
        {
            subclassTable = [0x00, 0x04, 0x40, 0x02, 0x03, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x03)
        {
            subclassTable = [0x00, 0x08, 0x40, 0x04, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x04)
        {
            subclassTable = [0x00, 0x10, 0x40, 0x08, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x05)
        {
            subclassTable = [0x00, 0x10, 0x40, 0x08, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x06)
        {
            subclassTable = [0x00, 0x04, 0x40, 0x10, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x07)
        {
            subclassTable = [0x00, 0xFF, 0x40, 0xFF, 0xFC, 0xFB, 0x00, 0x00];
        }
        else if (damageClass is 0x08)
        {
            subclassTable = [0x00, 0x04, 0x40, 0xFF, 0xFC, 0xFB, 0x20, 0x00];
        }
        else if (damageClass is 0x09)
        {
            subclassTable = [0x00, 0x64, 0x18, 0x64, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x0A)
        {
            subclassTable = [0x00, 0xF9, 0xFA, 0xFF, 0x64, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x0B)
        {
            subclassTable = [0x00, 0x08, 0x40, 0xFD, 0x04, 0x10, 0x00, 0x00];
        }
        else if (damageClass is 0x0C)
        {
            subclassTable = [0x00, 0x08, 0x40, 0xFE, 0x04, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x0D)
        {
            subclassTable = [0x00, 0x10, 0x40, 0xFD, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x0E)
        {
            subclassTable = [0x00, 0xFE, 0x40, 0x10, 0x00, 0x00, 0x00, 0x00];
        }
        else if (damageClass is 0x0F)
        {
            subclassTable = [0x00, 0x20, 0x40, 0xFF, 0x00, 0x00, 0x00, 0xFA];
        }
        else
        {
            return false;
        }

        if (subclass > 7)
        {
            return false;
        }

        return DamageValueCanRemoveSprite(subclassTable[subclass]);
    }

    public static bool DamageValueIsDamaging(byte value) => value switch
    {
        0x00 => false,

        < 0xF9 => true,
        _ => false
    };

    public static bool DamageValueCanRemoveSprite(byte value) => value switch
    {
        0x00 => false,

        < 0xF9 => true,
        0xF9 => true,
        0xFA => true,
        0xFD => true,
        _ => false
    };
}
