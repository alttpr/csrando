import fs from "fs-extra";
import path from "path";
import os from "os";
import { GitHubPagesManager } from "../github-pages-manager";
import { readRdcFileDecompressed, stripRdcOrGzExtension } from "../utils";
import { Rdc } from "../rdc";
import { MetaDataBlock } from "../rdc-types";
import type { GameSpriteConfig } from "../types";

export async function remoteRemove(argv: any) {
  const dryRun = Boolean(argv.dryRun);
  const repo = argv.repo as string;
  const game = argv.game as string;
  let spriteName = argv.spriteName as string | undefined;

  if (!spriteName && argv.sourceRdc) {
    try {
      const src = path.resolve(argv.sourceRdc as string);
      const rdcBuf = await readRdcFileDecompressed(src);
      const rdcParsed = Rdc.parse(rdcBuf);
      if (rdcParsed.contains(MetaDataBlock.RDC_TYPE_ID)) {
        const meta = rdcParsed.tryParseBlock(rdcBuf, MetaDataBlock);
        const t = (meta as any)?.content?.title;
        if (typeof t === "string" && t.trim()) {
          spriteName = t
            .trim()
            .toLowerCase()
            .replace(/[^a-z0-9-_]+/g, "_");
        }
      }
      if (!spriteName) {
        spriteName = stripRdcOrGzExtension(src)
          .trim()
          .toLowerCase()
          .replace(/[^a-z0-9-_]+/g, "_");
      }
    } catch {
      // ignore
    }
  }

  if (!spriteName) {
    console.error(
      "Error: --spriteName (or --sourceRdc to derive it) is required for remove.",
    );
    process.exitCode = 1;
    return;
  }

  const tmpDir = argv.tmpDir
    ? path.resolve(argv.tmpDir)
    : path.join(os.tmpdir(), `sprite-importer-remove-${Date.now()}`);
  const manager = new GitHubPagesManager(
    repo,
    tmpDir,
    argv.branch as string | undefined,
  );
  await manager.cloneOrUpdateRepo();

  const spritesJsonPath = manager.getFilePath(path.join(game, "sprites.json"));
  if (!(await fs.pathExists(spritesJsonPath))) {
    console.error(
      `Error: sprites.json not found for game "${game}" in the repository.`,
    );
    process.exitCode = 1;
    return;
  }

  const cfg: GameSpriteConfig = await fs.readJson(spritesJsonPath);
  const sprites = Array.isArray(cfg?.sprites) ? cfg.sprites : [];
  const index = sprites.findIndex((s) => s.value === spriteName);
  if (index === -1) {
    console.log(`Sprite "${spriteName}" not found; nothing to remove.`);
    return;
  }
  const entry = sprites[index];

  if (!dryRun) {
    // Remove PNG
    const pngAbs = manager.getFilePath(path.join(game, entry.imagePath));
    try {
      await fs.remove(pngAbs);
    } catch (e) {
      void e; // ignore best-effort delete errors
    }
    // Remove RDC or patch files
    if (entry.rdcPath) {
      const rdcAbs = manager.getFilePath(path.join(game, entry.rdcPath));
      try {
        await fs.remove(rdcAbs);
      } catch (e) {
        void e; // ignore best-effort delete errors
      }
    } else if (entry.patchDetails?.files) {
      for (const f of entry.patchDetails.files) {
        const abs = manager.getFilePath(path.join(game, f.path));
        try {
          await fs.remove(abs);
        } catch (e) {
          void e; // ignore best-effort delete errors
        }
      }
    }
  }

  const updated = sprites.filter((s) => s.value !== spriteName);
  if (!dryRun) {
    await fs.writeJson(
      spritesJsonPath,
      { ...cfg, sprites: updated },
      { spaces: 2 },
    );
    await manager.commitChanges(
      `Remove sprite: ${spriteName} for game ${game}`,
    );
    await manager.pushChanges();
  } else {
    console.log(
      `[dry-run] Would remove files for "${spriteName}" and update ${spritesJsonPath}`,
    );
  }
}
