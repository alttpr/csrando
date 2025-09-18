import fs from "fs-extra";
import path from "path";
import os from "os";
import { GitHubPagesManager } from "../github-pages-manager";
import {
  getTitleAuthorFromRdc,
  isGzippedRdc,
  loadGameConfig,
  readRdcFileDecompressed,
  renderPreviewImageForGame,
  sanitizeId,
  saveGameConfig,
  stripRdcOrGzExtension,
} from "../utils";
import type { GameSpriteConfig, SpriteInfoFrontend } from "../types";

async function scanDir(dir: string, recursive: boolean): Promise<string[]> {
  const entries = await fs.readdir(dir, { withFileTypes: true } as any);
  const files: string[] = [];
  for (const e of entries as any[]) {
    const p = path.join(dir, e.name);
    if ((e as any).isDirectory?.()) {
      if (recursive) files.push(...(await scanDir(p, true)));
    } else {
      const low = e.name.toLowerCase();
      if (low.endsWith(".rdc") || low.endsWith(".rdc.gz")) files.push(p);
    }
  }
  return files.sort();
}

export async function bulkAddRemote(argv: unknown) {
  const args = argv as {
    repo?: string;
    game?: "alttp" | "sm" | "zelda1" | "metroid1";
    sourceDir?: string;
    tmpDir?: string;
    dryRun?: boolean;
    branch?: string;
    recursive?: boolean;
  };

  if (!args.repo) throw new Error("Missing --repo");
  if (!args.game) throw new Error("Missing --game");
  if (!args.sourceDir) throw new Error("Missing --sourceDir");

  const repoUrl = args.repo;
  const game = args.game;
  const sourceDir = path.resolve(args.sourceDir);
  const dryRun = Boolean(args.dryRun);
  const recursive = Boolean(args.recursive);

  const tmpDir = args.tmpDir
    ? path.resolve(args.tmpDir)
    : path.join(os.tmpdir(), `sprite-importer-bulk-add-${Date.now()}`);
  console.log(`Using temporary directory: ${tmpDir}`);
  const manager = new GitHubPagesManager(repoUrl, tmpDir, args.branch);
  await manager.cloneOrUpdateRepo();

  // Discover RDC files
  let rdcFiles: string[] = [];
  try {
    rdcFiles = await scanDir(sourceDir, recursive);
  } catch (e) {
    console.error(`Error reading sourceDir ${sourceDir}:`, e);
    process.exitCode = 1;
    return;
  }
  if (rdcFiles.length === 0) {
    console.log(
      `No .rdc/.rdc.gz files found in ${sourceDir}${recursive ? " (recursive)" : ""}.`,
    );
    return;
  }
  console.log(`Found ${rdcFiles.length} RDC file(s) to add.`);

  const gameDir = manager.getFilePath(game);
  await fs.ensureDir(gameDir);
  const spritesJsonPath = manager.getFilePath(path.join(game, "sprites.json"));
  const gameConfig: GameSpriteConfig = await loadGameConfig(spritesJsonPath);

  const addedNames: string[] = [];
  const updatedNames: string[] = [];

  for (const rdcPath of rdcFiles) {
    // Derive sprite name from RDC metadata or file name
    let derivedTitle: string | undefined;
    try {
      const buf = await readRdcFileDecompressed(rdcPath);
      const meta = getTitleAuthorFromRdc(buf);
      if (meta.title) derivedTitle = meta.title;
    } catch {
      /* ignore */
    }
    if (!derivedTitle) derivedTitle = stripRdcOrGzExtension(rdcPath);
    const spriteName = sanitizeId(derivedTitle);
    const base = sanitizeId(spriteName);

    const destRdcRel = path.join(game, `${base}.rdc`);
    const destPngRel = path.join(game, `${base}.png`);
    const destRdcAbs = manager.getFilePath(destRdcRel);
    const destPngAbs = manager.getFilePath(destPngRel);

    // Write RDC (always decompressed)
    if (dryRun) {
      console.log(
        `[dry-run] Would write RDC${isGzippedRdc(rdcPath) ? " (decompressed)" : ""} to ${destRdcAbs}`,
      );
    } else {
      const decompressed = await readRdcFileDecompressed(rdcPath);
      await fs.writeFile(destRdcAbs, decompressed);
      console.log(`Wrote RDC to ${destRdcAbs}`);
    }

    // PNG: try sibling <base>.png; else render from RDC
    const siblingPng = path.join(
      path.dirname(rdcPath),
      `${stripRdcOrGzExtension(rdcPath)}.png`,
    );
    if (await fs.pathExists(siblingPng)) {
      if (dryRun) console.log(`[dry-run] Would copy PNG to ${destPngAbs}`);
      else {
        await fs.copy(siblingPng, destPngAbs);
        console.log(`Copied PNG to ${destPngAbs}`);
      }
    } else {
      try {
        const buf = await readRdcFileDecompressed(rdcPath);
        const png = renderPreviewImageForGame(buf, game);
        if (png) {
          if (dryRun)
            console.log(`[dry-run] Would render preview PNG to ${destPngAbs}`);
          else {
            const { PNG } = await import("pngjs");
            await fs.writeFile(destPngAbs, (PNG as any).sync.write(png));
            console.log(`Rendered preview PNG to ${destPngAbs}`);
          }
        } else {
          console.warn(
            `No preview PNG generated for ${rdcPath}; provide a sibling PNG if desired.`,
          );
        }
      } catch (e) {
        console.warn(`Failed to render PNG from ${rdcPath}:`, e);
      }
    }

    // Build/update entry
    let title = derivedTitle || spriteName;
    let author: string | undefined = undefined;
    try {
      const buf = await readRdcFileDecompressed(rdcPath);
      const meta = getTitleAuthorFromRdc(buf);
      title = meta.title || title;
      author = meta.author || author;
    } catch {
      /* ignore */
    }
    const kind =
      game === "alttp"
        ? "rdc/link"
        : game === "sm"
          ? "rdc/samus"
          : game === "zelda1"
            ? "rdc/nes-z1"
            : game === "metroid1"
              ? "rdc/nes-m1"
              : undefined;
    const spriteEntry: SpriteInfoFrontend = {
      value: spriteName,
      name: title!,
      imagePath: `${base}.png`,
      author,
      rdcPath: `${base}.rdc`,
      kind,
    };

    const idx = gameConfig.sprites.findIndex((s) => s.value === spriteName);
    if (idx >= 0) {
      gameConfig.sprites[idx] = spriteEntry;
      updatedNames.push(spriteName);
    } else {
      gameConfig.sprites.push(spriteEntry);
      addedNames.push(spriteName);
    }
  }

  if (dryRun)
    console.log(
      `[dry-run] Would update ${spritesJsonPath} with ${addedNames.length + updatedNames.length} entries.`,
    );
  else await saveGameConfig(spritesJsonPath, gameConfig);

  if (dryRun) {
    console.log(
      `[dry-run] Would commit and push: Add ${addedNames.length} and update ${updatedNames.length} sprites for game ${game}`,
    );
    return;
  }

  // Single commit for all changes
  const namesForMsg = [...addedNames, ...updatedNames];
  let detail = namesForMsg.join(", ");
  if (detail.length > 180) detail = detail.slice(0, 177) + "...";
  const commitMsg = `Bulk add sprites (${namesForMsg.length}) for game ${game}: ${detail}`;
  await manager.commitChanges(commitMsg);
  await manager.pushChanges();
  console.log(
    `Bulk add complete for ${namesForMsg.length} sprite(s) in ${repoUrl}`,
  );
}
