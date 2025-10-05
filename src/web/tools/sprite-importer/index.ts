#!/usr/bin/env node
import fs from "fs-extra"; // Ensure 'fs-extra' is installed
import path from "path";
import yargs from "yargs"; // Ensure 'yargs' is installed
import { hideBin } from "yargs/helpers"; // Ensure 'yargs' is installed
import { ZsprParser } from "./zspr";
import { fileURLToPath } from "url";
import { handleRemoteAddOrUpdate } from "./commands/add-or-update";
import { remoteInit } from "./commands/init";
import { remoteList } from "./commands/list";
import { remoteVerify } from "./commands/verify";
import { remoteRemove } from "./commands/remove";
import { handleRemoteAtlas } from "./commands/atlas";
import type { SpritesJson } from "./types";

import { Rdc } from "./rdc";
import {
  MetaDataBlock,
  LinkSprite,
  SamusSprite,
  Zelda1SpriteDataBlock,
  Metroid1SpriteDataBlock,
} from "./rdc-types";
import { readRdcFileDecompressed, sanitizeId } from "./utils";
import {
  loadZelda1AssetModule,
  extractZelda1SegmentsFromAsset,
} from "./zelda1-assets";
import {
  loadMetroidAssetModule,
  extractMetroidSegmentsFromAsset,
} from "./metroid-assets";
import {
  renderZ3AvatarImage,
  renderSMAvatarImage,
  renderNESAvatarImage,
} from "./image-renderer";
import { PNG } from "pngjs";
import { SpriteInfoEntry } from "./types";

// --- Types and common helpers are imported from ./types and ./utils

export async function main(argvFromNode: string[] = hideBin(process.argv)) {
  const argv = await yargs(argvFromNode)
    .option("game", {
      alias: "g",
      type: "string",
      description:
        'Game identifier (e.g., "alttp", "supermetroid", "zelda1", "metroid"). Required.',
      choices: ["alttp", "supermetroid", "zelda1", "metroid"],
      demandOption: true,
    })
    .option("sourceDir", {
      alias: "s",
      type: "string",
      description: "Source directory containing .zspr/.rdc files to import.",
    })
    .option("outputDir", {
      alias: "o",
      type: "string",
      description:
        'Base output directory. Defaults to "static/sprites". Processed files/RDCs will be in a subfolder named after --game (e.g., "static/sprites/alttp").',
    })
    .help()
    .alias("help", "h")
    // Adding the remote command structure
    .command(
      "remote",
      "Manage sprites on a remote GitHub Pages repository.",
      (y) => {
        return y
          .option("repo", {
            type: "string",
            description: "HTTPS URL of the GitHub repository.",
            demandOption: true,
          })
          .option("game", {
            alias: "g",
            type: "string",
            description:
              "Game identifier (alttp, supermetroid, zelda1, metroid).",
            choices: ["alttp", "supermetroid", "zelda1", "metroid"],
          })
          .option("tmpDir", {
            type: "string",
            description: "Temporary directory for clone",
          })
          .option("branch", {
            type: "string",
            description: "Remote branch",
            default: "gh-pages",
          })
          .option("dryRun", {
            type: "boolean",
            description: "Print actions only",
            default: false,
          })
          .command(
            "atlas",
            "Build or update sprite atlas assets for one game (--game) or all (--all).",
            (z) =>
              z
                .option("all", {
                  type: "boolean",
                  description: "Build for all games with sprites.json",
                })
                .option("maxWidth", {
                  type: "number",
                  description: "Maximum sheet width (px)",
                  default: 2048,
                })
                .option("keep", {
                  type: "number",
                  description: "Number of historical sheets to keep",
                  default: 2,
                })
                .conflicts("all", "game"),
            async (argv) => handleRemoteAtlas(argv),
          )
          .command(
            "init",
            "Initialize a game directory with an empty sprites.json if missing.",
            (z) => z.demandOption("game"),
            async (argv) => remoteInit(argv),
          )
          .command(
            "verify",
            "Verify sprites.json entries and file paths for a game.",
            (z) => z.demandOption("game"),
            async (argv) => remoteVerify(argv),
          )
          .command(
            "list",
            "List sprites in the remote collection for a game.",
            (z) => z.demandOption("game"),
            async (argv) => remoteList(argv),
          )
          .command(
            "bulk-add",
            "Bulk add or update multiple sprites from a directory in a single commit.",
            (z) =>
              z
                .demandOption("game")
                .option("sourceDir", {
                  type: "string",
                  demandOption: true,
                  description:
                    "Directory containing .rdc or .rdc.gz files to add",
                })
                .option("recursive", {
                  type: "boolean",
                  description: "Recurse into subdirectories when scanning",
                  default: false,
                }),
            async (argv) => {
              const { bulkAddRemote } = await import("./commands/bulk-add");
              return bulkAddRemote(argv);
            },
          )
          .command(
            "add",
            "Add or update a sprite in the remote collection.",
            (z) =>
              z
                .demandOption("game")
                .option("sourceRdc", {
                  type: "string",
                  demandOption: true,
                  description: "Local RDC path",
                })
                .option("sourcePng", {
                  type: "string",
                  description: "Local PNG path (optional)",
                })
                .option("spriteName", {
                  type: "string",
                  description: "Unique sprite name (optional)",
                }),
            async (argv) => handleRemoteAddOrUpdate(argv, "add"),
          )
          .command(
            "update",
            "Update an existing sprite (alias for add).",
            (z) =>
              z
                .demandOption("game")
                .option("sourceRdc", { type: "string", demandOption: true })
                .option("sourcePng", { type: "string" })
                .option("spriteName", { type: "string" }),
            async (argv) => handleRemoteAddOrUpdate(argv, "update"),
          )
          .command(
            "remove",
            "Remove a sprite from the remote collection.",
            (z) =>
              z
                .demandOption("game")
                .option("spriteName", {
                  type: "string",
                  description: "Name/key of the sprite to remove",
                })
                .option("sourceRdc", {
                  type: "string",
                  description: "RDC path to derive name if omitted",
                }),
            async (argv) => remoteRemove(argv),
          )
          .demandCommand(
            1,
            "You need to specify a remote subcommand (init, verify, list, bulk-add, add, update, remove).",
          );
      },
    )
    .parseAsync();

  // Determine if a remote command was called
  // yargs populates argv._ with positional arguments. The first one is the command.
  // If 'remote' is the command, then argv.subcommand will be populated by the remote command's setup.
  // yargs ensures that if 'remote' command is used, one of its subcommands must be provided due to .demandCommand().
  const isRemoteCommand = argv._[0] === "remote";

  if (!isRemoteCommand) {
    if (!argv.sourceDir) {
      console.error("Error: --sourceDir is required for sprite importing.");
      process.exit(1);
    }
    const game = argv.game as string;
    const sourceDir = path.resolve(argv.sourceDir);
    const outputDirRoot = argv.outputDir
      ? path.resolve(argv.outputDir)
      : path.resolve(process.cwd(), "static", "sprites");
    const outputBaseDir = path.join(outputDirRoot, game);

    console.log(`Processing game for import: ${game}`);
    console.log(`Source directory: ${sourceDir}`);
    console.log(`Output target directory: ${outputBaseDir}`);

    if (!fs.existsSync(sourceDir)) {
      console.error(`Source directory ${sourceDir} does not exist.`);
      process.exit(1);
    }
    await fs.ensureDir(outputBaseDir);

    const spritesJsonPath = path.join(outputBaseDir, "sprites.json");
    console.log(`Sprites JSON will be at: ${spritesJsonPath}`);

    let existingSpritesJson: SpritesJson = {};
    try {
      if (fs.existsSync(spritesJsonPath)) {
        existingSpritesJson = (await fs.readJson(
          spritesJsonPath,
        )) as SpritesJson;
        console.log(`Loaded existing sprites from ${spritesJsonPath}`);
      }
    } catch (err) {
      console.warn(
        `Warning: Could not read existing ${spritesJsonPath}, starting fresh. Error:`,
        err,
      );
    }
    const newEntriesMap = new Map<string, SpriteInfoEntry>();
    const files = await fs.readdir(sourceDir);

    for (const file of files) {
      const filePath = path.join(sourceDir, file); // No longer potentially null
      const fileExt = path.extname(file).toLowerCase();
      const baseName = path.basename(file, fileExt);

      console.log(`\nProcessing file: ${filePath}`);

      if (game === "alttp") {
        if (fileExt === ".zspr") {
          try {
            const zsprData = ZsprParser.parse(filePath);
            console.log(
              `  Parsed ZSPR: ${zsprData.title} by ${zsprData.authorAscii}`,
            );

            const spriteKey = baseName;
            const pngPath = path.join(outputBaseDir, `${spriteKey}.png`);
            const gfxBinPath = path.join(outputBaseDir, `${spriteKey}_gfx.bin`);
            const paletteBinPath = path.join(
              outputBaseDir,
              `${spriteKey}_palette.bin`,
            );
            const glovesBinPath = path.join(
              outputBaseDir,
              `${spriteKey}_gloves.bin`,
            );

            const spriteGfx = zsprData.content.subarray(0, 0x7000);
            const palette = zsprData.content.subarray(0x7000, 0x7000 + 120);
            const gloves = zsprData.content.subarray(
              0x7000 + 120,
              0x7000 + 120 + 4,
            );

            await fs.writeFile(gfxBinPath, spriteGfx);
            console.log(`  Saved sprite GFX to ${gfxBinPath}`);
            await fs.writeFile(paletteBinPath, palette);
            console.log(`  Saved palette to ${paletteBinPath}`);
            await fs.writeFile(glovesBinPath, gloves);
            console.log(`  Saved gloves to ${glovesBinPath}`);

            const pngImage = renderZ3AvatarImage(zsprData);
            await fs.writeFile(pngPath, PNG.sync.write(pngImage));
            console.log(`  Saved rendered Z3 avatar to ${pngPath}`);

            const newEntry: SpriteInfoEntry = {
              title: zsprData.title || spriteKey,
              author: zsprData.authorAscii || "Unknown",
              game: game,
              path: path.relative(outputDirRoot, pngPath).replace(/\\/g, "/"),
              files: {
                gfx: path
                  .relative(outputDirRoot, gfxBinPath)
                  .replace(/\\/g, "/"),
                palette: path
                  .relative(outputDirRoot, paletteBinPath)
                  .replace(/\\/g, "/"),
                gloves: path
                  .relative(outputDirRoot, glovesBinPath)
                  .replace(/\\/g, "/"),
              },
            };
            newEntriesMap.set(spriteKey, newEntry);
            console.log(`  Prepared data for sprites.json for ${spriteKey}`);

            try {
              const linkSprite = new LinkSprite();
              linkSprite.setContent([
                zsprData.content.subarray(0, 0x7000),
                zsprData.content.subarray(0x7000, 0x7000 + 120),
                zsprData.content.subarray(0x7000 + 120, 0x7000 + 120 + 4),
              ]);
              const metaDataBlock = new MetaDataBlock({
                title: zsprData.title,
                author: zsprData.authorAscii,
                sourceFile: file,
              });
              const rdcBuffer = Rdc.write(zsprData.authorAscii, [
                metaDataBlock,
                linkSprite,
              ]);
              const rdcPath = path.join(outputBaseDir, `${baseName}.rdc`);
              await fs.writeFile(rdcPath, rdcBuffer);
              console.log(`  Saved converted RDC to ${rdcPath}`);
            } catch (rdcErr) {
              console.error(`  Error converting ZSPR ${file} to RDC:`, rdcErr);
            }
          } catch (err) {
            console.error(`  Error processing ZSPR file ${file}:`, err);
          }
        } else if (
          fileExt === ".rdc" ||
          file.toLowerCase().endsWith(".rdc.gz")
        ) {
          try {
            const fileBuffer = await readRdcFileDecompressed(filePath);
            const rdc = Rdc.parse(fileBuffer);
            console.log(`  Parsed RDC: Author ${rdc.author}`);

            if (rdc.contains(LinkSprite.RDC_TYPE_ID)) {
              const linkSprite = rdc.tryParseBlock(fileBuffer, LinkSprite);
              if (linkSprite) {
                const spriteKey = baseName;
                const pngPath = path.join(outputBaseDir, `${spriteKey}.png`);
                const gfxBinPath = path.join(
                  outputBaseDir,
                  `${spriteKey}_gfx.bin`,
                );
                const paletteBinPath = path.join(
                  outputBaseDir,
                  `${spriteKey}_palette.bin`,
                );
                const glovesBinPath = path.join(
                  outputBaseDir,
                  `${spriteKey}_gloves.bin`,
                );

                await fs.writeFile(gfxBinPath, linkSprite.content[0]);
                console.log(`  Saved LinkSprite GFX to ${gfxBinPath}`);
                await fs.writeFile(paletteBinPath, linkSprite.content[1]);
                console.log(`  Saved LinkSprite palette to ${paletteBinPath}`);
                await fs.writeFile(glovesBinPath, linkSprite.content[2]);
                console.log(`  Saved LinkSprite gloves to ${glovesBinPath}`);

                const pngImage = renderZ3AvatarImage(linkSprite);
                await fs.writeFile(pngPath, PNG.sync.write(pngImage));
                console.log(
                  `  Saved rendered Z3 avatar from RDC to ${pngPath}`,
                );

                let title = spriteKey;
                let author = rdc.author || "Unknown";
                if (rdc.contains(MetaDataBlock.RDC_TYPE_ID)) {
                  const meta = rdc.tryParseBlock(fileBuffer, MetaDataBlock);
                  if (
                    meta?.content?.title &&
                    typeof meta.content.title === "string"
                  )
                    title = meta.content.title;
                  if (
                    meta?.content?.author &&
                    typeof meta.content.author === "string"
                  )
                    author = meta.content.author;
                }

                const newEntry: SpriteInfoEntry = {
                  title: title,
                  author: author,
                  game: game,
                  path: path
                    .relative(outputDirRoot, pngPath)
                    .replace(/\\/g, "/"),
                  files: {
                    gfx: path
                      .relative(outputDirRoot, gfxBinPath)
                      .replace(/\\/g, "/"),
                    palette: path
                      .relative(outputDirRoot, paletteBinPath)
                      .replace(/\\/g, "/"),
                    gloves: path
                      .relative(outputDirRoot, glovesBinPath)
                      .replace(/\\/g, "/"),
                  },
                };
                newEntriesMap.set(spriteKey, newEntry);
                console.log(
                  `  Prepared data for sprites.json for ${spriteKey} from RDC LinkSprite`,
                );
              }
            } else {
              console.log(
                `  RDC file ${file} does not contain LinkSprite data (type ${LinkSprite.RDC_TYPE_ID}).`,
              );
            }
          } catch (err) {
            console.error(
              `  Error processing RDC file ${file} for ALttP:`,
              err,
            );
          }
        }
      } else if (game === "supermetroid") {
        if (fileExt === ".rdc" || file.toLowerCase().endsWith(".rdc.gz")) {
          try {
            const fileBuffer = await readRdcFileDecompressed(filePath);
            const rdc = Rdc.parse(fileBuffer);
            console.log(`  Parsed RDC: Author ${rdc.author}`);

            if (rdc.contains(SamusSprite.RDC_TYPE_ID)) {
              const samusSprite = rdc.tryParseBlock(fileBuffer, SamusSprite);
              if (samusSprite) {
                const spriteKey = baseName;
                const pngPath = path.join(outputBaseDir, `${spriteKey}.png`);
                const binaryFiles: { [type: string]: string } = {};

                console.log(
                  `  Samus sprite has ${samusSprite.content.length} data blocks.`,
                );
                for (let i = 0; i < samusSprite.content.length; i++) {
                  const dataBuffer = samusSprite.content[i];
                  const binFileName = `${spriteKey}_data_${i}.bin`;
                  const binPath = path.join(outputBaseDir, binFileName);
                  await fs.writeFile(binPath, dataBuffer);
                  binaryFiles[`data_${i}`] = path
                    .relative(outputDirRoot, binPath)
                    .replace(/\\/g, "/");
                }
                console.log(
                  `  Saved ${samusSprite.content.length} binary files for ${spriteKey}`,
                );

                const pngImage = renderSMAvatarImage(samusSprite);
                await fs.writeFile(pngPath, PNG.sync.write(pngImage));
                console.log(
                  `  Saved rendered SM avatar from RDC to ${pngPath}`,
                );

                let title = spriteKey;
                let author = rdc.author || "Unknown";
                if (rdc.contains(MetaDataBlock.RDC_TYPE_ID)) {
                  const meta = rdc.tryParseBlock(fileBuffer, MetaDataBlock);
                  if (
                    meta?.content?.title &&
                    typeof meta.content.title === "string"
                  )
                    title = meta.content.title;
                  if (
                    meta?.content?.author &&
                    typeof meta.content.author === "string"
                  )
                    author = meta.content.author;
                }

                const newEntry: SpriteInfoEntry = {
                  title: title,
                  author: author,
                  game: game,
                  path: path
                    .relative(outputDirRoot, pngPath)
                    .replace(/\\/g, "/"),
                  files: binaryFiles,
                };
                newEntriesMap.set(spriteKey, newEntry);
                console.log(
                  `  Prepared data for sprites.json for ${spriteKey} from RDC SamusSprite`,
                );
              }
            } else {
              console.log(
                `  RDC file ${file} does not contain SamusSprite data (type ${SamusSprite.RDC_TYPE_ID}).`,
              );
            }
          } catch (err) {
            console.error(
              `  Error processing RDC file ${file} for supermetroid:`,
              err,
            );
          }
        }
      } else if (game === "zelda1") {
        const ensureUniqueSpriteKey = (candidate: string): string => {
          const base = candidate || sanitizeId(`${baseName}`);
          let attempt = base || "sprite";
          let counter = 1;

          while (
            existingSpritesJson[attempt] !== undefined ||
            newEntriesMap.has(attempt)
          ) {
            attempt = `${base || "sprite"}_${counter++}`;
          }
          return attempt;
        };

        if ([".js", ".mjs", ".cjs"].includes(fileExt)) {
          try {
            const assets = await loadZelda1AssetModule(filePath);
            if (!assets.length) {
              console.log("  No Zelda 1 sprite definitions found in module.");
              continue;
            }

            let assetIndex = 0;
            for (const asset of assets) {
              assetIndex += 1;
              const titleFromAsset = asset.name?.trim();
              const spriteTitle = titleFromAsset || `${baseName}_${assetIndex}`;
              console.log(`  Processing Zelda 1 asset: ${spriteTitle}`);

              const { buffers, unusedWrites } =
                extractZelda1SegmentsFromAsset(asset);
              if (unusedWrites.length > 0) {
                console.warn(
                  `  Warning: ${unusedWrites.length} unused write segments found for "${spriteTitle}".`,
                );
              }

              const z1Block = new Zelda1SpriteDataBlock();
              z1Block.setContent(buffers);

              const authorName =
                asset.creator?.trim() || asset.originalBy?.trim() || "Unknown";
              const metaDataBlock = new MetaDataBlock({
                title: spriteTitle,
                author: authorName,
                sourceFile: file,
                category: asset.category,
                originalBy: asset.originalBy,
                creator: asset.creator,
              });
              const rdcBuffer = Rdc.write(authorName, [metaDataBlock, z1Block]);

              const candidateKey = sanitizeId(spriteTitle);
              const spriteKey = ensureUniqueSpriteKey(
                candidateKey || sanitizeId(`${baseName}_${assetIndex}`),
              );

              const rdcPath = path.join(outputBaseDir, `${spriteKey}.rdc`);

              await fs.writeFile(rdcPath, rdcBuffer);
              const pngPath = path.join(outputBaseDir, `${spriteKey}.png`);
              const pngImage = renderNESAvatarImage(z1Block);
              await fs.writeFile(pngPath, PNG.sync.write(pngImage));
              console.log(
                `  Wrote Zelda 1 asset "${spriteTitle}" to ${rdcPath} with preview ${pngPath}`,
              );

              const newEntry: SpriteInfoEntry = {
                title: spriteTitle,
                author: authorName,
                game,
                path: path.relative(outputDirRoot, pngPath).replace(/\\/g, "/"),
                files: {
                  rdc: path
                    .relative(outputDirRoot, rdcPath)
                    .replace(/\\/g, "/"),
                },
              };
              newEntriesMap.set(spriteKey, newEntry);
            }
          } catch (err) {
            console.error(
              `  Error processing Zelda 1 asset module ${file}:`,
              err,
            );
          }
        } else if (
          fileExt === ".rdc" ||
          file.toLowerCase().endsWith(".rdc.gz")
        ) {
          try {
            const fileBuffer = await readRdcFileDecompressed(filePath);
            const rdc = Rdc.parse(fileBuffer);
            console.log(`  Parsed RDC: Author ${rdc.author}`);

            if (rdc.contains(Zelda1SpriteDataBlock.RDC_TYPE_ID)) {
              const z1 = rdc.tryParseBlock(fileBuffer, Zelda1SpriteDataBlock);
              if (z1) {
                const spriteKey = baseName;
                const pngPath = path.join(outputBaseDir, `${spriteKey}.png`);
                const rdcOutPath = path.join(outputBaseDir, `${spriteKey}.rdc`);
                await fs.writeFile(rdcOutPath, fileBuffer);
                const pngImage = renderNESAvatarImage(z1);
                await fs.writeFile(pngPath, PNG.sync.write(pngImage));
                console.log(
                  `  Saved z1 CHR/palette and preview for ${spriteKey}`,
                );

                let title = spriteKey;
                let author = rdc.author || "Unknown";
                if (rdc.contains(MetaDataBlock.RDC_TYPE_ID)) {
                  const meta = rdc.tryParseBlock(fileBuffer, MetaDataBlock);
                  if (
                    meta?.content?.title &&
                    typeof meta.content.title === "string"
                  )
                    title = meta.content.title;
                  if (
                    meta?.content?.author &&
                    typeof meta.content.author === "string"
                  )
                    author = meta.content.author;
                }
                const newEntry: SpriteInfoEntry = {
                  title,
                  author,
                  game,
                  path: path
                    .relative(outputDirRoot, pngPath)
                    .replace(/\\/g, "/"),
                  files: {
                    rdc: path
                      .relative(outputDirRoot, rdcOutPath)
                      .replace(/\\/g, "/"),
                  },
                };
                newEntriesMap.set(spriteKey, newEntry);
              }
            } else {
              console.log(
                `  RDC file ${file} does not contain Zelda1SpriteDataBlock data (type ${Zelda1SpriteDataBlock.RDC_TYPE_ID}).`,
              );
            }
          } catch (err) {
            console.error(
              `  Error processing RDC file ${file} for zelda1:`,
              err,
            );
          }
        }
      } else if (game === "metroid") {
        const ensureUniqueSpriteKey = (candidate: string): string => {
          const base = candidate || sanitizeId(`${baseName}`);
          let attempt = base || "sprite";
          let counter = 1;
          while (
            existingSpritesJson[attempt] !== undefined ||
            newEntriesMap.has(attempt)
          ) {
            attempt = `${base || "sprite"}_${counter++}`;
          }
          return attempt;
        };

        if ([".js", ".mjs", ".cjs"].includes(fileExt)) {
          try {
            const assets = await loadMetroidAssetModule(filePath);
            if (!assets.length) {
              console.log("  No Metroid sprite definitions found in module.");
              continue;
            }

            let assetIndex = 0;
            for (const asset of assets) {
              assetIndex += 1;
              const titleFromAsset = asset.name?.trim();
              const spriteTitle = titleFromAsset || `${baseName}_${assetIndex}`;
              console.log(`  Processing Metroid asset: ${spriteTitle}`);

              const { buffers, unusedWrites } =
                extractMetroidSegmentsFromAsset(asset);
              if (unusedWrites.length > 0) {
                console.warn(
                  `  Warning: ${unusedWrites.length} unused write segments found for "${spriteTitle}".`,
                );
              }

              const m1Block = new Metroid1SpriteDataBlock();
              m1Block.setContent(buffers);

              const authorName =
                asset.creator?.trim() || asset.originalBy?.trim() || "Unknown";
              const metaDataBlock = new MetaDataBlock({
                title: spriteTitle,
                author: authorName,
                sourceFile: file,
                category: asset.category,
                originalBy: asset.originalBy,
                creator: asset.creator,
              });
              const rdcBuffer = Rdc.write(authorName, [metaDataBlock, m1Block]);

              const candidateKey = sanitizeId(spriteTitle);
              const spriteKey = ensureUniqueSpriteKey(
                candidateKey || sanitizeId(`${baseName}_${assetIndex}`),
              );

              const rdcPath = path.join(outputBaseDir, `${spriteKey}.rdc`);
              await fs.writeFile(rdcPath, rdcBuffer);

              const pngPath = path.join(outputBaseDir, `${spriteKey}.png`);
              const pngImage = renderNESAvatarImage(m1Block);
              await fs.writeFile(pngPath, PNG.sync.write(pngImage));
              console.log(
                `  Wrote Metroid asset "${spriteTitle}" to ${rdcPath} with preview ${pngPath}`,
              );

              const newEntry: SpriteInfoEntry = {
                title: spriteTitle,
                author: authorName,
                game,
                path: path.relative(outputDirRoot, pngPath).replace(/\\/g, "/"),
                files: {
                  rdc: path
                    .relative(outputDirRoot, rdcPath)
                    .replace(/\\/g, "/"),
                },
              };
              newEntriesMap.set(spriteKey, newEntry);
            }
          } catch (err) {
            console.error(
              `  Error processing Metroid asset module ${file}:`,
              err,
            );
          }
        } else if (
          fileExt === ".rdc" ||
          file.toLowerCase().endsWith(".rdc.gz")
        ) {
          try {
            const fileBuffer = await readRdcFileDecompressed(filePath);
            const rdc = Rdc.parse(fileBuffer);
            console.log(`  Parsed RDC: Author ${rdc.author}`);

            if (rdc.contains(Metroid1SpriteDataBlock.RDC_TYPE_ID)) {
              const m1 = rdc.tryParseBlock(fileBuffer, Metroid1SpriteDataBlock);
              if (m1) {
                const spriteKey = baseName;
                const pngPath = path.join(outputBaseDir, `${spriteKey}.png`);
                const rdcOutPath = path.join(outputBaseDir, `${spriteKey}.rdc`);
                await fs.writeFile(rdcOutPath, fileBuffer);
                const pngImage = renderNESAvatarImage(m1);
                await fs.writeFile(pngPath, PNG.sync.write(pngImage));
                console.log(
                  `  Saved m1 CHR/palette and preview for ${spriteKey}`,
                );

                let title = spriteKey;
                let author = rdc.author || "Unknown";
                if (rdc.contains(MetaDataBlock.RDC_TYPE_ID)) {
                  const meta = rdc.tryParseBlock(fileBuffer, MetaDataBlock);
                  if (
                    meta?.content?.title &&
                    typeof meta.content.title === "string"
                  )
                    title = meta.content.title;
                  if (
                    meta?.content?.author &&
                    typeof meta.content.author === "string"
                  )
                    author = meta.content.author;
                }
                const newEntry: SpriteInfoEntry = {
                  title,
                  author,
                  game,
                  path: path
                    .relative(outputDirRoot, pngPath)
                    .replace(/\\/g, "/"),
                  files: {
                    rdc: path
                      .relative(outputDirRoot, rdcOutPath)
                      .replace(/\\/g, "/"),
                  },
                };
                newEntriesMap.set(spriteKey, newEntry);
              }
            } else {
              console.log(
                `  RDC file ${file} does not contain Metroid1SpriteDataBlock data (type ${Metroid1SpriteDataBlock.RDC_TYPE_ID}).`,
              );
            }
          } catch (err) {
            console.error(
              `  Error processing RDC file ${file} for metroid:`,
              err,
            );
          }
        }
      }
    } // End of for...of files loop

    if (newEntriesMap.size > 0) {
      const updatedSpritesJson = {
        ...existingSpritesJson,
        ...Object.fromEntries(newEntriesMap),
      };
      try {
        await fs.writeJson(spritesJsonPath, updatedSpritesJson, {
          spaces: 2,
        });
        console.log(`\nSuccessfully updated ${spritesJsonPath}`);
      } catch (err) {
        console.error(`Error writing ${spritesJsonPath}:`, err);
      }
    } else {
      console.log("\nNo new sprites processed; sprites.json not updated.");
    }
  } // End of local import block

  console.log("\nSprite processing complete.");
}

// Only run when executed directly (not when imported for tests)
try {
  const isDirectRun =
    typeof process !== "undefined" &&
    process.argv &&
    process.argv[1] &&
    fileURLToPath(import.meta.url) === path.resolve(process.argv[1]);
  if (isDirectRun) {
    main().catch((error) => {
      console.error("Unhandled error in main function:", error);
      process.exit(1);
    });
  }
} catch {
  // Best-effort guard; if URL utilities fail, do nothing (safer for tests)
}
