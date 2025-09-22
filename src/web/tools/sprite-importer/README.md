# Sprite Importer Tool

## Overview

This CLI processes sprite assets for A Link to the Past (`alttp`), Super Metroid (`supermetroid`), The Legend of Zelda (`zelda1`), and Metroid (`metroid`). It ingests ZSPR and RDC files, writes normalized binary assets and preview PNGs, and keeps a `sprites.json` manifest up to date. The same entry point also exposes `remote` subcommands for working with sprite collections hosted in a Git repository.

## Prerequisites

- Node.js (version 18.x or later recommended)
- npm (typically comes with Node.js)

## Setup

All necessary dependencies are listed in the main project's `package.json`. Ensure you have run `npm install` at the root of the project. No additional setup is typically required for this tool.

## Usage

The script is run using the `npm run import-sprites` command (or `ts-node ./tools/sprite-importer/index.ts` for development), followed by specific arguments for the tool.

```bash
# Import sprites from a local folder
npm run import-sprites -- --game alttp --sourceDir path/to/alttp_sprites --outputDir static/sprites
```

**Note:** The `--` before the script arguments is important when using `npm run` to pass arguments directly to the script.

### Command-Line Arguments

- `--game <game_id>` or `-g <game_id>`
  - **Required.** Determines which game subdirectory is updated.
  - Supported values: `alttp`, `supermetroid`, `zelda1`, `metroid`.
- `--sourceDir <path>` or `-s <path>`
  - **Required.** Directory containing `.zspr` and/or `.rdc` files to import.
- `--outputDir <path>` or `-o <path>`
  - Optional base output directory. Defaults to `static/sprites/`, with artifacts written under `static/sprites/<game_id>/`.

## Import Workflow

This is the default task. It performs the following actions:

1.  **Parses Sprite Files:**
    - Processes `.zspr` files for A Link to the Past.
    - Processes `.rdc` files for A Link to the Past (Link sprites) and Super Metroid (Samus sprites).
    - _Note: Zelda 1 and Metroid files are not currently processed by the `import` task beyond basic recognition if game type is specified._
2.  **Extracts Binary Data:**
    - Raw binary data (GFX, palettes, game-specific data like Link's gloves) are extracted.
    - These are saved as separate `.bin` files (e.g., `<sprite_name>_gfx.bin`).
3.  **Generates Preview PNGs:**
    - Renders a preview PNG image of the sprite's default pose (e.g., 16x24 for ALttP Link, 32x48 for SM Samus).
4.  **Updates `sprites.json`:**
    - Updates (or creates) `sprites.json` in the game-specific output directory (e.g., `static/sprites/alttp/sprites.json`).
    - This JSON file acts as an inventory, storing title, author, relative path to the preview PNG, and relative paths to binary files for each sprite.
5.  **ZSPR to RDC Conversion (ALttP):**
    - When an ALttP `.zspr` file is processed, the tool automatically converts it into the RDC format and saves it as `<sprite_name>.rdc` in the same output directory. This RDC file includes a `MetaDataBlock` (with title, author, source filename) and a `LinkSprite` data block.

## Remote Sprite Collection Management

This tool also supports managing sprite collections hosted on remote Git repositories, initially targeting GitHub Pages. This functionality allows for listing, adding, updating, and removing sprites directly in a remote collection.

### Configuration for Remote Operations

1.  **GitHub Personal Access Token (PAT):**

    - To interact with a GitHub repository (especially private ones or for write operations), a Personal Access Token (PAT) is required.
    - **Creation:**
      1.  Go to your GitHub account settings.
      2.  Navigate to "Developer settings" > "Personal access tokens" > "Tokens (classic)".
      3.  Click "Generate new token" (or "Generate new token (classic)").
      4.  Give your token a descriptive name (e.g., "sprite_importer_pat").
      5.  Select the expiration period appropriate for your use case.
      6.  Under "Select scopes," check the `repo` scope (Full control of private repositories).
      7.  Click "Generate token."
      8.  **Important:** Copy the generated token immediately. You will not be able to see it again.
    - **Usage:** The PAT must be provided to the tool via an environment variable named `SPRITE_IMPORTER_GITHUB_PAT`.
      ```bash
      export SPRITE_IMPORTER_GITHUB_PAT="your_copied_github_pat_here"
      ```
      The tool will automatically use this PAT for authenticated HTTPS operations with GitHub. If the PAT is not set, a warning will be issued, and operations requiring authentication will likely fail.

2.  **GitHub Repository URL:**
    - The HTTPS URL of the target GitHub repository (e.g., `https://github.com/username/my-sprite-collection.git`) must be provided using the `--repo <url>` argument for all `remote` subcommands.

### `remote` Command Usage

General pattern:
`npm run import-sprites -- remote <subcommand> [options...]`

Or for development with `ts-node`:
`ts-node ./tools/sprite-importer/index.ts remote <subcommand> [options...]`

#### Common Options for `remote` Subcommands:

- `--repo <url>`: **(Required)** HTTPS URL of the GitHub repository.
- `--game <id>`: **(Required by most subcommands)** Game identifier (e.g., `alttp`, `supermetroid`, `zelda1`, `metroid`). This specifies the game-specific subdirectory within the repository where sprites are managed (e.g., `alttp/sprites.json`).
- `--tmpDir <path>`: (Optional) Specify a local directory for cloning the remote repository. Defaults to a system-generated temporary directory (e.g., `/tmp/sprite-importer-remote-<timestamp>`). The tool will attempt to clean up this directory after the operation, but manual cleanup might be needed if errors occur.
- `--provider <name>`: (Optional, defaults to `github`) Specifies the remote provider. Currently, only `github` is supported.

#### Subcommands:

1.  **`remote list`**

    - **Syntax:** `... remote list --repo <url> --game <id>`
    - **Description:** Lists all sprites for the specified `<id>` found in the remote repository. It clones/pulls the repository to the temporary directory, reads the `<id>/sprites.json` file, and prints a summary of each sprite (name, title, author).
    - **Example:**
      ```bash
      npm run import-sprites -- remote list --repo https://github.com/my-org/my-sprites.git --game alttp
      ```

2.  **`remote add`**

    - **Syntax:** `... remote add --repo <url> --game <id> --spriteName <name> --sourceRdc <path_to_rdc> [--sourcePng <path_to_png>]`
    - **Description:** Adds a new sprite to the remote collection or updates an existing one if `<name>` already exists.
      - It copies the local RDC file (specified by `--sourceRdc`) and the optional local PNG preview (specified by `--sourcePng`) into the appropriate game-specific directory in the cloned repository.
      - The `<id>/sprites.json` file in the repository is updated with the new sprite's metadata.
      - Changes are then committed and pushed to the remote repository.
    - **Options:**
      - `--spriteName <name>`: **(Required)** A unique name (key) for the sprite (e.g., "link_red_tunic"). This name will be used for the filenames in the repository (e.g., `link_red_tunic.rdc`, `link_red_tunic.png`).
      - `--sourceRdc <path>`: **(Required)** Path to the local RDC file to be uploaded.
      - `--sourcePng <path>`: (Optional) Path to a local PNG preview file. If provided, it will be uploaded and referenced in `sprites.json`.
    - **Example:**
      ```bash
      npm run import-sprites -- remote add --repo <url> --game alttp --spriteName MyCustomLink --sourceRdc ./local_sprites/custom_link.rdc --sourcePng ./local_sprites/custom_link_preview.png
      ```

3.  **`remote bulk-add`**

    - **Syntax:** `... remote bulk-add --repo <url> --game <id> --sourceDir <dir> [--recursive]`
    - **Description:** Adds or updates multiple sprites from a local directory in a single commit.
      - Scans `<dir>` for `.rdc` or `.rdc.gz` files (optionally recursively) and processes each.
      - For each RDC, derives the sprite key from RDC metadata title or the file name, copies a decompressed `.rdc` to the remote, and either copies a sibling PNG with the same base name or renders a preview PNG when possible.
      - Updates `<id>/sprites.json` for all sprites, then commits and pushes once.
    - **Options:**
      - `--sourceDir <dir>`: **(Required)** Directory containing `.rdc` or `.rdc.gz` files.
      - `--recursive`: (Optional) Recurse into subdirectories.
    - **Example:**
      ```bash
      npm run import-sprites -- remote bulk-add --repo <url> --game alttp --sourceDir ./out/rdc
      ```

4.  **`remote remove`**

    - **Syntax:** `... remote remove --repo <url> --game <id> --spriteName <name>`
    - **Description:** Removes a sprite from the remote collection.
      - It deletes the sprite's RDC file, its PNG file (if referenced), and any other associated files listed in the `sprites.json` entry for that sprite from the game-specific directory in the cloned repository.
      - The sprite's entry is removed from `<id>/sprites.json`.
      - Changes are committed and pushed.
    - **Options:**
      - `--spriteName <name>`: **(Required)** The unique name/key of the sprite to remove.
    - **Example:**
      ```bash
      npm run import-sprites -- remote remove --repo <url> --game alttp --spriteName MyCustomLink
      ```

5.  **`remote update`**
    - **Syntax:** `... remote update --repo <url> --game <id> --spriteName <name> --sourceRdc <path_to_rdc> [--sourcePng <path_to_png>]`
    - **Description:** Updates an existing sprite in the remote repository. This command requires the sprite specified by `<name>` to already exist in the collection.
      - It functions similarly to `add` but specifically targets an existing entry. If the sprite does not exist, an error will be reported.
      - If a new `--sourcePng` is provided, it replaces the existing PNG. If not provided, the existing PNG reference in `sprites.json` is preserved (if one exists).
      - The author information from the existing `sprites.json` entry is preserved unless implicitly changed by metadata within the new RDC (future enhancement).
    - **Options:** Same as `remote add`.
    - **Example:**
      ```bash
      npm run import-sprites -- remote update --repo <url> --game alttp --spriteName MyCustomLink --sourceRdc ./updated_sprites/custom_link_v2.rdc
      ```

### Structure of Sprites in Remote Repository

The `remote` commands expect sprites to be organized within the repository as follows:

- `<repository_root>/`
  - `<game_id>/` (e.g., `alttp/`, `supermetroid/`)
    - `sprites.json` (The inventory file for this game)
    - `<spriteName1>.rdc`
    - `<spriteName1>.png` (Optional preview)
    - `<spriteName2>.rdc`
    - `<spriteName2>.png`
    - ... other sprite files ...

The paths stored in `sprites.json` (e.g., under `entry.path` for the PNG and `entry.files.rdc` for the RDC) should be relative to the game-specific directory (e.g., `MySprite.png`, not `alttp/MySprite.png`).

## Output File Structure

(This section applies to the import task.)

Processed files are organized within the specified output directory (defaulting to `static/sprites/<game_id>/`). An example structure for a sprite named `my_sprite`:

- `static/sprites/<game_id>/my_sprite.png` (Preview image, generated by `import` task)
- `static/sprites/<game_id>/my_sprite.rdc` (RDC file, created from ZSPR conversion or copied from the source directory)
- `static/sprites/<game_id>/my_sprite_gfx.bin` (Example for ZSPR GFX data, generated by `import` task)
- `static/sprites/<game_id>/my_sprite_palette.bin` (Example for ZSPR palette data, generated by `import` task)
- `static/sprites/<game_id>/my_sprite_gloves.bin` (Example for ZSPR gloves data, generated by `import` task)
- `static/sprites/<game_id>/my_sprite_data_0.bin` (Example for Samus RDC data segments, generated by `import` task)
- `static/sprites/<game_id>/my_sprite_data_1.bin`
- ...
- `static/sprites/<game_id>/sprites.json` (Inventory file for all sprites of that game, managed by `import` task)

The exact names and number of binary files can vary based on the sprite format and game.

## Troubleshooting/Notes

- Ensure that the paths provided to `--sourceDir` and `--outputDir` are correct and accessible.
- If `ts-node` is not globally available or there are issues running the npm script, ensure it's listed in `devDependencies` and correctly installed.
- The default output directory `static/sprites/<game_id>/` is designed to work with the web application's static asset serving.
