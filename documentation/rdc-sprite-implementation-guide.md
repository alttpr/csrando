# Retro Data Container (RDC) Sprite Implementation Guide

This guide describes the RDC binary format used to bundle replacement player sprites for four games:

- **Zelda 3** (A Link to the Past) – Link sprite block (`typeId = 1`)
- **Zelda 1** (NES) – Link sprite block (`typeId = 2`)
- **Metroid 1** (NES) – Samus sprite block (`typeId = 3`)
- **Super Metroid** – Samus sprite block (`typeId = 4`)

The intent is to let external tools implement RDC import/export without depending on an existing codebase. All numbers are hexadecimal unless noted otherwise.

## 1. Container Layout

An RDC file is a little-endian binary blob with the following structure:

| Offset | Size | Description |
| --- | --- | --- |
| 0x00 | 18 | ASCII header `RETRODATACONTAINER` |
| 0x12 | 1 | Format version (`0x01`) |
| 0x13 | 4 | `numBlocks` – number of directory entries |
| 0x17 | `numBlocks × 8` | Block directory entries: `<uint32 blockType><uint32 fileOffset>` |
| … | variable | Author string – ASCII, null-terminated |
| … | variable | Block payloads concatenated in directory order |

### Reading

1. Verify the header and version.
2. Read `numBlocks` entries. Each entry provides a block type and the absolute file offset where that block payload begins.
3. After the directory, read bytes until a `0x00` to obtain the author string.
4. For each block, seek to its offset and parse the payload according to the block specification below. The payload ends at the next block’s offset or EOF for the last block.

### Writing

1. Prepare all block payloads in the order you wish to list them.
2. Compute the block offsets: header (18 bytes) + version (1) + directory count (4) + directory (`n × 8`) + author string (`len + 1`).
3. Emit header, version, block count, directory entries, author string, then append each payload sequentially.

## 2. Block Directory

Known block types:

| Type ID | Name | Contents |
| --- | --- | --- |
| 0 | `MetadataBlock` | Optional UTF-8 JSON preceded by a 32-bit length (little endian). |
| 1 | `link_sprite` | Zelda 3 Link graphics, palette, gloves. |
| 2 | `zelda1_sprite` | Zelda 1 Link CHR graphics and palettes. |
| 3 | `metroid1_sprite` | Metroid 1 Samus CHR graphics and palettes. |
| 4 | `samus_sprite` | Super Metroid Samus DMA banks, palettes, auxiliary assets. |

The metadata block is optional and can be ignored by sprite tools.

## 3. Sprite Block Specifications

Each sprite block is a linear sequence of segments. When **parsing**, advance a cursor through the block payload and slice the specified number of bytes for each segment. When **applying** the block to a ROM, copy each segment to every destination listed for that segment (handling mirroring or stride rules as described). Exporting back to an RDC payload simply reads the same segments in order and concatenates them.

### 3.1 Zelda 3 Link (`typeId = 1`)

| Segment | Length | Destination (SNES LoROM) | Description |
| --- | --- | --- | --- |
| 0 | `0x7000` | `0x108000` | Link sprite graphics.
| 1 | `0x0078` | `0x1BD308` | Palette data (four 15-colour sets).
| 2 | `0x0004` | `0x1BEDF5` | Gloves colour overrides.

When targeting HiROM mirrors used by combo ROMs, convert the SNES addresses to PC offsets (`0x480000`, `0x4DD308`, `0x4DEDF5`). Otherwise convert as standard LoROM addresses.

### 3.2 Zelda 1 Link (`typeId = 2`)

Segments map directly to NES PRG-ROM offsets. Palette segments appear multiple times for mirrored tables.

| Segment | Length | Destination(s) | Label |
| --- | --- | --- | --- |
| 0 | 32 | `0x608E34` | `LIFTING_ITEM`
| 1 | 32 | `0x608EB4` | `WALK1_PROFILE_BIGSHIELD`
| 2 | 448 | `0x61007F` | `WALK1_PROFILE`, `WALK2_PROFILE`, `FACING_DOWN_NOSHIELD`, `FACING_UP`, `ATTACKING_PROFILE`, `ATTACKING_DOWN`, `ATTACKING_UP`
| 3 | 32 | `0x6105BF` | `WALK2_PROFILE_BIGSHIELD`
| 4 | 64 | `0x6105FF` | `WALK1_DOWN_SMALLSHIELD`, `WALK2_DOWN_SMALLSHIELD`
| 5 | 32 | `0x61067F` | `FACING_DOWN_BIGSHIELD`
| 6 | 3 | `0x631314`, `0x631410`, `0x63150C`, `0x631608`, `0x631704`, `0x631800`, `0x6318FC`, `0x6319F8`, `0x631AF4`, `0x631BF0`, `0x631CEC`, `0x3D3804` | `BASE_COLORS`
| 7 | 3 | `0x631CF0` | `LEVEL2_COLORS`
| 8 | 3 | `0x631CF4` | `LEVEL3_COLORS`
| 9 | 3 | `0x612287` | `TUNIC_COLORS`

Apply palette triplets to every listed destination. Exporters should keep the exact order.

### 3.3 Metroid 1 Samus (`typeId = 3`)

Segments 0–17 contain CHR graphics; the remaining segments are palette data mirrored across several offsets.

| Segment | Length | Destination(s) | Label |
| --- | --- | --- | --- |
| 0 | 64 | `0x6B0000` | `RUN1_HEAD`, `RUN2_HEAD`
| 1 | 80 | `0x6B0050` | `RUN3_HEAD`, `JUMP_LEGS`, `FACING_HEAD`
| 2 | 64 | `0x6B00B0` | `EXPLODING_HEAD`, `IDLE_HEAD`
| 3 | 16 | `0x6B0170` | `IDLE_WEAPON`
| 4 | 96 | `0x6B0190` | `FACING_SHOULDERS`, `EXPLODING_SHOULDERS`, `IDLE_SHOULDERS`
| 5 | 64 | `0x6B0200` | `RUN1_TORSO`, `RUN2_TORSO`
| 6 | 48 | `0x6B0250` | `RUN3_TORSO`
| 7 | 96 | `0x6B0290` | `FACING_TORSO`, `EXPLODING_TORSO`, `IDLE_TORSO`
| 8 | 96 | `0x6B0310` | `RUN1_BACKLEG`, `RUN2_LEGS`, `RUN3_LEGS`
| 9 | 16 | `0x6B0390` | `FACING_LEG`
| 10 | 32 | `0x6B03B0` | `IDLE_LEGS`
| 11 | 96 | `0x6B0400` | `RUN1_SHOULDERS`, `RUN2_SHOULDERS`, `RUN3_SHOULDERS`
| 12 | 48 | `0x6B0490` | `FIRING_TORSO`
| 13 | 112 | `0x6B0500` | `MORPHBALL_TOP`, `SPINJUMP1_TOP`, `SPINJUMP2_TOP`
| 14 | 112 | `0x6B0600` | `MORPHBALL_BOTTOM`, `SPINJUMP1_MIDDLE`, `SPINJUMP2_BOTTOM`
| 15 | 16 | `0x6B0690` | `POINTUP_WEAPON`
| 16 | 32 | `0x6B0720` | `SPINJUMP1_BOTTOM`
| 17 | 64 | `0x6B0770` | `POINTUP_SHOULDERS`, `POINTUP_HEAD`
| 18 | 3 | `0x68A285`, `0x68A2E8`, `0x69218C`, `0x6921EF`, `0x69A72C`, `0x69A7A5`, `0x6A2169`, `0x6A21A9`, `0x6AA0FF`, `0x6AA153` | `BASE_COLORS`
| 19 | 2 | `0x68A298`, `0x69219F`, `0x69A73F`, `0x6A217C`, `0x6AA112` | `NORMAL_COLORS`
| 20 | 2 | `0x68A29E`, `0x6921A5`, `0x69A745`, `0x6A2182`, `0x6AA118` | `MISSILE_COLORS`
| 21 | 2 | `0x68A2A4`, `0x6921AB`, `0x69A74B`, `0x6A2188`, `0x6AA11E` | `VARIA_COLORS`
| 22 | 2 | `0x68A2AA`, `0x6921B1`, `0x69A751`, `0x6A218E`, `0x6AA124` | `VARIA_MISSILE_COLORS`

### 3.4 Super Metroid Samus (`typeId = 4`)

Many segments provide multiple SNES addresses for different mapping modes. The table lists ExHiROM and LoROM addresses; SA-1 uses the same LoROM banks except where noted.

#### 3.4.1 DMA Banks & Major Animations

| Segment | Length | ExHiROM | LoROM | Label |
| --- | --- | --- | --- | --- |
| 0 | `0x8000` | `0x440000` | `0x9C8000` | `DMA bank 1`
| 1 | `0x8000` | `0x450000` | `0x9D8000` | `DMA bank 2`
| 2 | `0x8000` | `0x460000` | `0x9E8000` | `DMA bank 3`
| 3 | `0x8000` | `0x470000` | `0x9F8000` | `DMA bank 4`
| 4 | `0x8000` | `0x480000` | `0xF58000` | `DMA bank 5`
| 5 | `0x8000` | `0x490000` | `0xF68000` | `DMA bank 6`
| 6 | `0x8000` | `0x4A0000` | `0xF78000` | `DMA bank 7`
| 7 | `0x8000` | `0x4B0000` | `0xF88000` | `DMA bank 8`
| 8 | `0x8000` | `0x540000` | `0xF98000` | `DMA bank 9`
| 9 | `0x8000` | `0x550000` | `0xFA8000` | `DMA bank 10`
| 10 | `0x8000` | `0x560000` | `0xFB8000` | `DMA bank 11`
| 11 | `0x8000` | `0x570000` | `0xFC8000` | `DMA bank 12`
| 12 | `0x8000` | `0x580000` | `0xFD8000` | `DMA bank 13`
| 13 | `0x7880` | `0x590000` | `0xFE8000` | `DMA bank 14`
| 14 | `0x3F60` | `0x5A0000` | `0xFF8000` | `Death DMA left`
| 15 | `0x3F60` | `0x5A4000` | `0xFFC000` | `Death DMA right`
| 16 | `0x03C0` | `0x9A9A00` | `0x9A9A00` | `Gun port`
| 17 | `0x0600` | `0xB6DA00` | `0xB6DA00` | `File select sprites`
| 18 | `0x0020` | `0xB6D900` | `0xB6D900` | `File select missile`
| 19 | `0x0020` | `0xB6D980` | `0xB6D980` | `File select missile head`

#### 3.4.2 Palette Anchors

Single-destination palette groups (30 bytes each):

| Segment | Destination | Label |
| --- | --- | --- |
| 20 | `0x9B9402` | `Power Standard`
| 21 | `0x9B9522` | `Varia Standard`
| 22 | `0x9B9802` | `Gravity Standard`

#### 3.4.3 Loader Palettes with Explicit Offsets

Use entry offsets `[0x00, 0x24, 0x4F, 0x73, 0x9E, 0xC2, 0xED, 0x111, 0x139]`:

| Segment | Destination | Label |
| --- | --- | --- |
| 23 | `0x8DDB6D` | `Power Loader`
| 24 | `0x8DDCD3` | `Varia Loader`
| 25 | `0x8DDE39` | `Gravity Loader`

#### 3.4.4 Remaining Palette & Table Segments

| Segment | Length | Entries | Stride/Offsets | Destination(s) | Label |
| --- | --- | --- | --- | --- | --- |
| 26 | 30 | 16 | `0x22` | `0x8DE468` | `Power Heat`
| 27 | 30 | 16 | `0x22` | `0x8DE694` | `Varia Heat`
| 28 | 30 | 16 | `0x22` | `0x8DE8C0` | `Gravity Heat`
| 29 | 30 | 8 | `0x20` | `0x9B9822` | `Power Charge`
| 30 | 30 | 8 | `0x20` | `0x9B9922` | `Varia Charge`
| 31 | 30 | 8 | `0x20` | `0x9B9A22` | `Gravity Charge`
| 32 | 30 | 4 | `0x20` | `0x9B9B22` | `Power Speed boost`
| 33 | 30 | 4 | `0x20` | `0x9B9D22` | `Varia Speed boost`
| 34 | 30 | 4 | `0x20` | `0x9B9F22` | `Gravity Speed boost`
| 35 | 30 | 4 | `0x20` | `0x9B9BA2` | `Power Speed squat`
| 36 | 30 | 4 | `0x20` | `0x9B9DA2` | `Varia Speed squat`
| 37 | 30 | 4 | `0x20` | `0x9B9FA2` | `Gravity Speed squat`
| 38 | 30 | 4 | `0x20` | `0x9B9C22` | `Power Shinespark`
| 39 | 30 | 4 | `0x20` | `0x9B9E22` | `Varia Shinespark`
| 40 | 30 | 4 | `0x20` | `0x9BA022` | `Gravity Shinespark`
| 41 | 30 | 4 | `0x20` | `0x9B9CA2` | `Power Screw attack`
| 42 | 30 | 4 | `0x20` | `0x9B9EA2` | `Varia Screw attack`
| 43 | 30 | 4 | `0x20` | `0x9BA0A2` | `Gravity Screw attack`
| 44 | 30 | 6 | `0x20` | `0x9B96C2` | `Crystal flash`
| 45 | 30 | 9 | `0x20` | `0x9BA122` | `Death`
| 46 | 30 | 10 | `0x20` | `0x9BA242` | `Hyper beam`
| 47 | 30 | 1 | — | `0x9BA3A2`, `0x8CE56B` | `Sepia`
| 48 | 30 | 1 | — | `0x9BA382` | `Sepia hurt`
| 49 | 6 | 2 | offsets `[0x00, 0x06]` | `0x9BA3C0`, `0x9BA3C6` | `Xray`
| 50 | 2 | 1 | — | `0x82E52C` | `Door Visor`
| 51 | 30 | 1 | — | `0x8EE5E2` | `File select`
| 52 | 30 | 1 | — | `0x8CE68B` | `Ship Intro`
| 53 | 30 | 16 | `0x24` | `0x8DD6C2` | `Ship Outro`
| 54 | 28 | 1 | — | `0xA2A5A0` | `Ship Standard`
| 55 | 2 | 14 | `0x06` | `0x8DCA54` | `Ship Glow`

## 4. Applying an RDC block

For each segment:

1. Determine the destination(s). When a segment lists multiple addresses, write the same byte range to each one.
2. If `entries > 1`, copy the payload repeatedly. The nth copy begins at `address + stride × n` or `address + entryOffsets[n]` when explicit offsets are provided.
3. Convert SNES addresses to PC offsets using the mapping that matches the target ROM (LoROM, ExHiROM, or SA-1). NES addresses can be treated as direct PRG-ROM offsets.
4. Maintain segment order—the reader/writer relies on streaming the payload sequentially.

## 5. Exporting to RDC

1. Read bytes from the ROM for every segment listed above, concatenating them in numerical order to form the block payload.
2. Write the RDC container header, directory, author string, and payloads as described in Section 1.

## 6. Metadata Block (Optional)

A type `0` block starts with a 32-bit length (little-endian) followed by that many bytes of UTF-8 JSON. Tools may include or ignore this block as desired.

## 7. Checklist

- [ ] Verify RDC header/version.
- [ ] Respect segment order for each block type.
- [ ] Copy palette segments to every target address.
- [ ] Use stride or explicit offsets when `entries > 1`.
- [ ] Convert SNES addresses to PC offsets using the correct mapping.
- [ ] Validate lengths before reading or writing.

Following this guide will create RDC sprite files fully compatible with existing randomizer tooling.
