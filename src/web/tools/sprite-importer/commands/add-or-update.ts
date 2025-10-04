import fs from "fs-extra";
import path from "path";
import os from "os";
import { GitHubPagesManager } from "../github-pages-manager";
import {
  sanitizeId,
  isGzippedRdc,
  stripRdcOrGzExtension,
  readRdcFileDecompressed,
  getTitleAuthorFromRdc,
  renderPreviewImageForGame,
  loadGameConfig,
  saveGameConfig,
} from "../utils";
import type { GameSpriteConfig, SpriteInfoFrontend } from "../types";

export async function handleRemoteAddOrUpdate(
  argv: unknown,
  mode: "add" | "update",
) {
  const args = argv as {
    spriteName?: string;
    game?: string;
    repo?: string;
    sourceRdc?: string;
    sourcePng?: string;
    tmpDir?: string;
    dryRun?: boolean;
    branch?: string;
  };
  if (!args.repo) throw new Error("Missing --repo");
  if (!args.game) throw new Error("Missing --game");
  if (!args.sourceRdc) throw new Error("Missing --sourceRdc");
  const repoUrl = args.repo;
  const game = args.game as "alttp" | "supermetroid" | "zelda1" | "metroid";
  const sourceRdcPath = path.resolve(args.sourceRdc);
  const sourcePngPath = args.sourcePng
    ? path.resolve(args.sourcePng)
    : undefined;
  const dryRun = Boolean(args.dryRun);

  if (!fs.existsSync(sourceRdcPath)) {
    console.error(`Error: RDC not found at ${sourceRdcPath}`);
    process.exitCode = 1;
    return;
  }
  if (sourcePngPath && !fs.existsSync(sourcePngPath)) {
    console.error(`Error: PNG not found at ${sourcePngPath}`);
    process.exitCode = 1;
    return;
  }

  const tmpDir = args.tmpDir
    ? path.resolve(args.tmpDir)
    : path.join(os.tmpdir(), `sprite-importer-${mode}-${Date.now()}`);
  console.log(`Using temporary directory: ${tmpDir}`);
  const manager = new GitHubPagesManager(repoUrl, tmpDir, args.branch);
  await manager.cloneOrUpdateRepo();

  const gameDir = manager.getFilePath(game);
  await fs.ensureDir(gameDir);
  const spritesJsonPath = manager.getFilePath(path.join(game, "sprites.json"));
  const gameConfig: GameSpriteConfig = await loadGameConfig(spritesJsonPath);

  // Derive spriteName if omitted
  let derivedTitle: string | undefined;
  let spriteName = args.spriteName;
  if (!spriteName) {
    try {
      const rdcBuffer = await readRdcFileDecompressed(sourceRdcPath);
      const meta = getTitleAuthorFromRdc(rdcBuffer);
      if (meta.title) derivedTitle = meta.title;
    } catch {
      /* ignore */
    }
    if (!derivedTitle) derivedTitle = stripRdcOrGzExtension(sourceRdcPath);
    spriteName = sanitizeId(derivedTitle);
    console.log(`Derived spriteName "${spriteName}" from metadata/file name.`);
  }
  const base = sanitizeId(spriteName);

  // Copy RDC
  const sourceIsGz = isGzippedRdc(sourceRdcPath);
  const destRdcRel = path.join(game, `${base}.rdc`);
  const destPngRel = path.join(game, `${base}.png`);
  const destRdcAbs = manager.getFilePath(destRdcRel);
  const destPngAbs = manager.getFilePath(destPngRel);
  if (dryRun) {
    console.log(
      `[dry-run] Would write RDC (decompressed=${sourceIsGz}) to ${destRdcAbs}`,
    );
  } else {
    if (sourceIsGz) {
      try {
        const raw = await fs.readFile(sourceRdcPath);
        const { gunzipSync } = await import("zlib");
        const decompressed = gunzipSync(raw);
        await fs.writeFile(destRdcAbs, decompressed);
        console.log(`Decompressed and wrote RDC to ${destRdcAbs}`);
      } catch (e) {
        console.error("Failed to decompress source .rdc.gz file.", e);
        process.exitCode = 1;
        return;
      }
    } else {
      await fs.copy(sourceRdcPath, destRdcAbs);
      console.log(`Copied RDC to ${destRdcAbs}`);
    }
  }

  // PNG handling
  if (sourcePngPath) {
    if (dryRun) console.log(`[dry-run] Would copy PNG to ${destPngAbs}`);
    else {
      await fs.copy(sourcePngPath, destPngAbs);
      console.log(`Copied PNG to ${destPngAbs}`);
    }
  } else {
    try {
      const rdcBuffer = await readRdcFileDecompressed(sourceRdcPath);
      const png = renderPreviewImageForGame(rdcBuffer, game);
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
          "Could not render PNG preview from RDC (unsupported game or missing data). Provide --sourcePng to include a preview.",
        );
      }
    } catch (e) {
      console.warn(
        "Failed to render PNG from RDC. Provide --sourcePng to include a preview.",
        e,
      );
    }
  }

  // Build sprite entry
  let title = derivedTitle || spriteName;
  let author = "Unknown";
  try {
    const rdcBuffer = await readRdcFileDecompressed(sourceRdcPath);
    const meta = getTitleAuthorFromRdc(rdcBuffer);
    title = meta.title || title;
    author = meta.author || author;
  } catch (e) {
    console.warn("Failed to parse RDC or build sprite entry:", e);
  }
  const kind =
    game === "alttp"
      ? "rdc/link"
      : game === "supermetroid"
        ? "rdc/samus"
        : game === "zelda1"
          ? "rdc/nes-z1"
          : game === "metroid"
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
  const existingIdx = gameConfig.sprites.findIndex(
    (s) => s.value === spriteName,
  );
  if (existingIdx >= 0) gameConfig.sprites[existingIdx] = spriteEntry;
  else gameConfig.sprites.push(spriteEntry);
  if (dryRun)
    console.log(
      `[dry-run] Would update ${spritesJsonPath} with entry "${spriteName}".`,
    );
  else {
    await saveGameConfig(spritesJsonPath, gameConfig);
    console.log(`Updated ${spritesJsonPath} with entry "${spriteName}".`);
  }
  const action = mode === "add" ? "Add" : "Update";
  if (dryRun)
    console.log(
      `[dry-run] Would commit and push: ${action} sprite: ${spriteName} for game ${game}`,
    );
  else {
    await manager.commitChanges(
      `${action} sprite: ${spriteName} for game ${game}`,
    );
    await manager.pushChanges();
  }
  console.log(`${action} complete for sprite "${spriteName}" in ${repoUrl}`);
}
