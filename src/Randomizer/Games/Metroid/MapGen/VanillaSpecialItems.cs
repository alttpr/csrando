namespace Randomizer.Games.Metroid.MapGen;

using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// The vanilla per-area special-items tables, transcribed from
/// asm/multirando-asm/src/m1/randomizer/tables.asm (which is the vanilla data relocated to
/// bank 88). Each entry is keyed by its vanilla world-map coordinate; payloads are the raw
/// bytes after the in-row offset byte, without the $00 terminator.
///
/// The emitter never uses the coordinates directly: it joins each entry to the vanilla
/// screen at that coordinate (via the vanilla room list) so the payload can be re-emitted
/// wherever the generated map places that screen. Position bytes inside the payloads are
/// screen-relative (power-ups: high nibble Y, low nibble X in 16px tiles, per the
/// PowerUpHandler at $EE20), so they stay valid at any map coordinate.
///
/// Elevator entries (type $04) and palette-change rooms (type $0A) are intentionally NOT
/// transcribed: elevators are computed from the generated elevator links, and palette
/// changes are cosmetic markers tied to vanilla map regions that make no sense on a
/// generated layout.
/// </summary>
public static class VanillaSpecialItems
{
    /// <param name="Area">Area whose table the entry came from.</param>
    /// <param name="Y">Vanilla world-map row.</param>
    /// <param name="X">Vanilla world-map column.</param>
    /// <param name="Payload">Type byte plus data, excluding the $00 terminator.</param>
    public record Entry(Area Area, int Y, int X, byte[] Payload);

    public static readonly Entry[] Entries =
    [
        // ------------------------------------------------------------------ Brinstar
        new(Area.Brinstar, 0x02, 0x0F, [0x02, 0x05, 0x37]),                   // Varia
        new(Area.Brinstar, 0x03, 0x18, [0x02, 0x09, 0x67]),                   // Missiles
        new(Area.Brinstar, 0x03, 0x1B, [0x02, 0x08, 0x87]),                   // Energy tank
        new(Area.Brinstar, 0x05, 0x07, [0x02, 0x02, 0x37]),                   // Long beam
        new(Area.Brinstar, 0x05, 0x19, [0x02, 0x00, 0x37]),                   // Bombs
        new(Area.Brinstar, 0x07, 0x19, [0x02, 0x08, 0x87]),                   // Energy tank
        new(Area.Brinstar, 0x09, 0x13, [0x02, 0x07, 0x37]),                   // Ice beam
        new(Area.Brinstar, 0x09, 0x15, [0x03]),                               // Mellows
        new(Area.Brinstar, 0x0B, 0x12, [0x02, 0x09, 0x67]),                   // Missiles
        new(Area.Brinstar, 0x0E, 0x02, [0x02, 0x04, 0x96]),                   // Maru Mari
        new(Area.Brinstar, 0x0E, 0x09, [0x02, 0x08, 0x12]),                   // Energy tank

        // ------------------------------------------------------------------ Norfair
        new(Area.Norfair, 0x0A, 0x1B, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0A, 0x1C, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0B, 0x1A, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0B, 0x1B, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0B, 0x1C, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0C, 0x1A, [0x02, 0x07, 0x37]),                    // Ice beam
        new(Area.Norfair, 0x0E, 0x12, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0F, 0x11, [0x02, 0x09, 0x34, 0x03]),              // Missiles + Melias
        new(Area.Norfair, 0x0F, 0x13, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0F, 0x14, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x0F, 0x15, [0x41, 0x8B, 0xE9, 0x51, 0x02, 0x9B]),  // Squeept
        new(Area.Norfair, 0x10, 0x0F, [0x02, 0x03, 0x37]),                    // Screw attack
        new(Area.Norfair, 0x11, 0x18, [0x31, 0x0B, 0xE9, 0x41, 0x02, 0x9A]),  // Squeept
        new(Area.Norfair, 0x11, 0x19, [0x21, 0x8B, 0xE9, 0x51, 0x02, 0x9A]),  // Squeept
        new(Area.Norfair, 0x11, 0x1B, [0x02, 0x01, 0x37]),                    // High jump
        new(Area.Norfair, 0x11, 0x1D, [0x09, 0xA0]),                          // Right door
        new(Area.Norfair, 0x11, 0x1E, [0x09, 0xB0]),                          // Left door
        new(Area.Norfair, 0x13, 0x1A, [0x02, 0x08, 0x42]),                    // Energy tank
        new(Area.Norfair, 0x14, 0x0D, [0x09, 0xA0]),                          // Right door
        new(Area.Norfair, 0x14, 0x0E, [0x09, 0xB0]),                          // Left door
        new(Area.Norfair, 0x14, 0x1C, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x15, 0x12, [0x02, 0x06, 0x37]),                    // Wave beam
        // Vanilla also has a right door at (0x17,0x15) labeled "undefined room" -- that
        // coordinate is outside every vanilla room, so there is no screen to join it to.
        new(Area.Norfair, 0x16, 0x13, [0x02, 0x09, 0x34]),                    // Missiles
        new(Area.Norfair, 0x16, 0x14, [0x02, 0x09, 0x34]),                    // Missiles

        // ------------------------------------------------------------------ Tourian
        new(Area.Tourian, 0x07, 0x03, [0x09, 0xA2]),                          // 10-missile right door
        new(Area.Tourian, 0x07, 0x04, [0x08]),                                // Rinkas
        new(Area.Tourian, 0x07, 0x09, [0x08]),                                // Rinkas
        new(Area.Tourian, 0x08, 0x0A, [0x18]),                                // Rinkas
        new(Area.Tourian, 0x09, 0x0A, [0x08]),                                // Rinkas
        new(Area.Tourian, 0x0A, 0x0A, [0x18]),                                // Rinkas
        new(Area.Tourian, 0x0B, 0x01, [0x09, 0xA3]),                          // Escape shaft bottom door
        // Mother Brain, cannons and rinkas.
        new(Area.Tourian, 0x0B, 0x02, [0x06, 0x47, 0x18, 0x05, 0x49, 0x15, 0x4B, 0x25, 0x3E]),
        // 2 zeebetites, 6 cannons and rinkas.
        new(Area.Tourian, 0x0B, 0x03, [0x37, 0x27, 0x08, 0x05, 0x41, 0x15, 0x43, 0x25, 0x36, 0x05, 0x49, 0x15, 0x4B, 0x35, 0x3E]),
        // Right door, 2 zeebetites, 6 cannons and rinkas.
        new(Area.Tourian, 0x0B, 0x04, [0x09, 0xA3, 0x17, 0x07, 0x08, 0x05, 0x41, 0x15, 0x43, 0x25, 0x36, 0x05, 0x49, 0x15, 0x4B, 0x35, 0x3E]),
        new(Area.Tourian, 0x0B, 0x05, [0x09, 0xB3]),                          // Left door

        // ------------------------------------------------------------------ Kraid
        new(Area.Kraid, 0x15, 0x04, [0x02, 0x09, 0x47]),                      // Missiles
        new(Area.Kraid, 0x15, 0x09, [0x02, 0x09, 0x47]),                      // Missiles
        new(Area.Kraid, 0x16, 0x0A, [0x02, 0x08, 0x66]),                      // Energy tank
        new(Area.Kraid, 0x19, 0x0A, [0x02, 0x09, 0x47]),                      // Missiles
        new(Area.Kraid, 0x1B, 0x05, [0x02, 0x09, 0x47]),                      // Missiles
        new(Area.Kraid, 0x1C, 0x07, [0x03]),                                  // Memus
        new(Area.Kraid, 0x1D, 0x08, [0x02, 0x08, 0xBE]),                      // Energy tank (Kraid's lair)

        // ------------------------------------------------------------------ Ridley
        new(Area.Ridley, 0x18, 0x12, [0x02, 0x09, 0x6D]),                     // Missiles
        new(Area.Ridley, 0x19, 0x11, [0x02, 0x08, 0x74]),                     // Energy tank
        new(Area.Ridley, 0x1B, 0x18, [0x02, 0x09, 0x6D]),                     // Missiles
        new(Area.Ridley, 0x1D, 0x0F, [0x02, 0x08, 0x66]),                     // Energy tank
        new(Area.Ridley, 0x1E, 0x14, [0x02, 0x09, 0x6D]),                     // Missiles
    ];
}
