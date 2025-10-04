declare module "@maseya/z3pr" {
  export function randomize(
    rom: Uint8Array,
    options?: {
      mode?:
        | "none"
        | "maseya"
        | "grayscale"
        | "negative"
        | "blackout"
        | "classic"
        | "dizzy"
        | "sick"
        | "puke";
      randomize_overworld?: boolean;
      randomize_dungeon?: boolean;
      randomize_link_sprite?: boolean;
      randomize_sword?: boolean;
      randomize_shield?: boolean;
      randomize_hud?: boolean;
      seed?: number | number[];
    },
    next_blend?: unknown,
  ): Uint8Array;

  export function randomize_copy(
    rom: Uint8Array,
    options?: Parameters<typeof randomize>[1],
    next_blend?: unknown,
  ): Uint8Array;
}
