#  yaml-esque definition file describing how to patch the randomizer
#  with custom player sprites for Zelda 1 and Metroid 1.
#  Use each item in the writes collections to write
#  "bytes" at rom offset "offset" totaling "length" bytes.

zelda1:
  defines:
    CHR_BYTE: 0x00  # a nes chr format byte
    TILE: [ # 16 bytes to an 8x8 2bpp chr tile
        $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
        $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
        $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
        $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
      ]

    #  Two tiles written in order 0, 1 with the following on-screen layout:
    #  +---------+
    #  |         |
    #  |    0    |
    #  |         |
    #  +---------+
    #  |         |
    #  |    1    |
    #  |         |
    #  +---------+
    2TILE_PATTERN: [ $TILE, $TILE ]

    #  Four-tile pattern written in order 0, 1, 2, 3 with the following on-screen layout:
    #  +---------+---------+
    #  |         |         |
    #  |    0    |    1    |
    #  |         |         |
    #  +---------+---------+
    #  |         |         |
    #  |    2    |    3    |
    #  |         |         |
    #  +---------+---------+
    4TILE_PATTERN: [ $TILE, $TILE, $TILE, $TILE ]

    #  Two tiles written in order 0, 1 used to construct a 4-tile mirrored sprite pattern.  Screen layout:
    #  +---------+---------+
    #  |         |         |
    #  |    0    |    0    |
    #  |         |         |
    #  +---------+---------+
    #  |         |         |
    #  |    1    |    1    |
    #  |         |         |
    #  +---------+---------+
    2TILE_MIRRORED_PATTERN: [ $TILE, $TILE ]
    COLOR:                  0x00                           # a nes palette color byte (0x00 - 0x3f)
    COLOR1:                 $COLOR                         # first color in set, tunic color
    COLOR2:                 $COLOR                         # second color in set, secondary or skin color
    COLOR3:                 $COLOR                         # third color in set, tertiary or shadow color
    BASE_COLORS:            [ $COLOR1, $COLOR2, $COLOR3 ]
    LEVEL2_COLORS:          [ $COLOR1, $COLOR2, $COLOR3 ]
    LEVEL3_COLORS:          [ $COLOR1, $COLOR2, $COLOR3 ]
    TUNIC_COLORS:           [ $COLOR1, $COLOR1, $COLOR1 ]     # tunic levels 1, 2, and 3 respectively

    LIFTING_ITEM:            $2TILE_MIRRORED_PATTERN
    WALK1_PROFILE_BIGSHIELD: $2TILE_PATTERN
    WALK1_PROFILE:           $4TILE_PATTERN
    WALK2_PROFILE:           $4TILE_PATTERN
    FACING_DOWN_NOSHIELD:    $4TILE_PATTERN
    FACING_UP:               $4TILE_PATTERN
    ATTACKING_PROFILE:       $4TILE_PATTERN
    ATTACKING_DOWN:          $4TILE_PATTERN
    ATTACKING_UP:            $4TILE_PATTERN
    WALK2_PROFILE_BIGSHIELD: $2TILE_PATTERN
    WALK1_DOWN_SMALLSHIELD:  $2TILE_PATTERN
    WALK2_DOWN_SMALLSHIELD:  $2TILE_PATTERN
    FACING_DOWN_BIGSHIELD:   $2TILE_PATTERN

  writes:
    - offset: 0x608e34
      length: 32
      bytes: [ $LIFTING_ITEM ]

    - offset: 0x608eb4
      length: 32
      bytes: [ $WALK1_PROFILE_BIGSHIELD ]

    - offset: 0x61007f
      length: 448
      bytes: [ $WALK1_PROFILE, $WALK2_PROFILE, $FACING_DOWN_NOSHIELD, $FACING_UP, $ATTACKING_PROFILE, $ATTACKING_DOWN, $ATTACKING_UP ]

    - offset: 0x6105bf
      length: 32
      bytes: [ $WALK2_PROFILE_BIGSHIELD ]

    - offset: 0x6105ff
      length: 64
      bytes: [ $WALK1_DOWN_SMALLSHIELD, $WALK2_DOWN_SMALLSHIELD ]

    - offset: 0x61067f
      length: 32
      bytes: [ $FACING_DOWN_BIGSHIELD ]

    - offset: [ 0x631314, 0x631410, 0x63150c, 0x631608, 0x631704, 0x631800, 0x6318f0, 0x6319f0, 0x631af0, 0x631bf0, 0x631cec, 0x3d3804 ]
      length: 3
      bytes: $BASE_COLORS

    - offset: [ 0x631cf0 ]
      length: 3
      bytes: $LEVEL2_COLORS

    - offset: [ 0x631cf4 ]
      length: 3
      bytes: $LEVEL3_COLORS

    - offset: 0x612287
      length: 3
      bytes: $TUNIC_COLORS

metroid1:
  defines:
    CHR_BYTE: 0x00  # a nes chr format byte
    TILE: [ # 16 bytes to an 8x8 2bpp chr tile
      $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
      $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
      $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
      $CHR_BYTE, $CHR_BYTE, $CHR_BYTE, $CHR_BYTE
    ]
    1TILE_PATTERN:        $TILE
    2TILE_PATTERN:        [ $TILE, $TILE ]
    3TILE_PATTERN:        [ $TILE, $TILE, $TILE ]
    COLOR:                0x00                           # a nes palette color byte (0x00 - 0x3f)
    COLOR1:               $COLOR                         # first color in set, typically highlight, outline, shadow, or accent color
    COLOR2:               $COLOR                         # second color in set, primary or suit color
    COLOR3:               $COLOR                         # third color in set, secondary or skin color
    BASE_COLORS:          [ $COLOR1, $COLOR2, $COLOR3 ]
    NORMAL_COLORS:        [ $COLOR2, $COLOR3 ]
    MISSILE_COLORS:       [ $COLOR2, $COLOR3 ]
    VARIA_COLORS:         [ $COLOR2, $COLOR3 ]
    VARIA_MISSILE_COLORS: [ $COLOR2, $COLOR3 ]

    RUN1_HEAD:           $2TILE_PATTERN
    RUN2_HEAD:           $2TILE_PATTERN
    RUN3_HEAD:           $2TILE_PATTERN
    JUMP_LEGS:           $2TILE_PATTERN
    FACING_HEAD:         $1TILE_PATTERN
    EXPLODING_HEAD:      $2TILE_PATTERN
    IDLE_HEAD:           $2TILE_PATTERN
    IDLE_WEAPON:         $1TILE_PATTERN
    FACING_SHOULDERS:    $2TILE_PATTERN
    EXPLODING_SHOULDERS: $2TILE_PATTERN
    IDLE_SHOULDERS:      $2TILE_PATTERN
    RUN1_TORSO:          $2TILE_PATTERN
    RUN2_TORSO:          $2TILE_PATTERN
    RUN3_TORSO:          $3TILE_PATTERN
    FACING_TORSO:        $2TILE_PATTERN
    EXPLODING_TORSO:     $2TILE_PATTERN
    IDLE_TORSO:          $2TILE_PATTERN
    RUN1_BACKLEG:        $1TILE_PATTERN
    RUN2_LEGS:           $3TILE_PATTERN
    RUN3_LEGS:           $2TILE_PATTERN
    FACING_LEG:          $1TILE_PATTERN
    IDLE_LEGS:           $2TILE_PATTERN
    RUN1_SHOULDERS:      $2TILE_PATTERN
    RUN2_SHOULDERS:      $2TILE_PATTERN
    RUN3_SHOULDERS:      $2TILE_PATTERN
    FIRING_TORSO:        $3TILE_PATTERN
    MORPHBALL_TOP:       $2TILE_PATTERN
    SPINJUMP1_TOP:       $2TILE_PATTERN
    SPINJUMP2_TOP:       $3TILE_PATTERN
    MORPHBALL_BOTTOM:    $2TILE_PATTERN
    SPINJUMP1_MIDDLE:    $2TILE_PATTERN
    SPINJUMP2_BOTTOM:    $3TILE_PATTERN
    POINTUP_WEAPON:      $1TILE_PATTERN
    SPINJUMP1_BOTTOM:    $2TILE_PATTERN
    POINTUP_SHOULDERS:   $2TILE_PATTERN
    POINTUP_HEAD:        $2TILE_PATTERN

  writes:
    - offset: 0x6b0000
      length: 64
      bytes: [ $RUN1HEAD, $RUN2HEAD ]

    - offset: 0x6b0050
      length: 80
      bytes: [ $RUN3_HEAD, $JUMP_LEGS, $FACING_HEAD ]

    - offset: 0x6b00b0
      length: 64
      bytes: [ $EXPLODING_HEAD, $IDLE_HEAD ]

    - offset: 0x6b0170
      length: 16
      bytes: [ $IDLE_WEAPON ]

    - offset: 0x6b0190
      length: 96
      bytes: [ $FACING_SHOULDERS, $EXPLODING_SHOULDERS, $IDLE_SHOULDERS ]

    - offset: 0x6b0200
      length: 64
      bytes: [ $RUN1_TORSO, $RUN2_TORSO ]

    - offset: 0x6b0250
      length: 48
      bytes: [ $RUN3_TORSO ]

    - offset: 0x6b0290
      length: 96
      bytes: [ $FACING_TORSO, $EXPLODING_TORSO, $IDLE_TORSO ]

    - offset: 0x6b0310
      length: 96
      bytes: [ $RUN1_BACKLEG, $RUN2_LEGS, $RUN3_LEGS ]

    - offset: 0x6b0390
      length: 16
      bytes: [ $FACING_LEG ]

    - offset: 0x6b03b0
      length: 32
      bytes: [ $IDLE_LEGS ]

    - offset: 0x6b0400
      length: 96
      bytes: [ $RUN1_SHOULDERS, $RUN2_SHOULDERS, $RUN3_SHOULDERS ]

    - offset: 0x6b0490
      length: 48
      bytes: [ $FIRING_TORSO ]

    - offset: 0x6b0500
      length: 112
      bytes: [ $MORPHBALL_TOP, $SPINJUMP1_TOP, $SPINJUMP2_TOP ]

    - offset: 0x6b0600
      length: 112
      bytes: [ $MORPHBALL_BOTTOM, $SPINJUMP1_MIDDLE, $SPINJUMP2_BOTTOM ]

    - offset: 0x6b0690
      length: 16
      bytes: [ $POINTUP_WEAPON ]

    - offset: 0x6b0720
      length: 32
      bytes: [ $SPINJUMP1_BOTTOM ]

    - offset: 0x6b0770
      length: 64
      bytes: [ $POINTUP_SHOULDERS, $POINTUP_HEAD ]

    - offset: [ 0x68a285, 0x68a2e8, 0x69218c, 0x6921ef, 0x69a72c, 0x69a7a5, 0x6a2169, 0x6a21a9, 0x6aa0ff, 0x6aa153 ]
      length: 3
      bytes: $BASE_COLORS

    - offset: [ 0x68a298, 0x69219f, 0x69a73f, 0x6a217c, 0x6aa112 ]
      length: 2
      bytes: $NORMAL_COLORS

    - offset: [ 0x68a29e, 0x6921a5, 0x69a745, 0x6a2182, 0x6aa118 ]
      length: 2
      bytes: $MISSILE_COLORS

    - offset: [ 0x68a2a4, 0x6921ab, 0x69a74b, 0x6a2188, 0x6aa11e ]
      length: 2
      bytes: $VARIA_COLORS

    - offset: [ 0x68a2aa, 0x6921b1, 0x69a751, 0x6a218e, 0x6aa124 ]
      length: 2
      bytes: $VARIA_MISSILE_COLORS
