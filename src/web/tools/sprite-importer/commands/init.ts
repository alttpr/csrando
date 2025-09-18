import fs from "fs-extra";
import path from "path";
import os from "os";
import { GitHubPagesManager } from "../github-pages-manager";

export async function remoteInit(argv: any) {
  console.log(`Remote init for game: ${argv.game}`);
  console.log(`Repository: ${argv.repo}`);
  const dryRun = Boolean(argv.dryRun);

  const resolvedTmpDirPath = argv.tmpDir
    ? path.resolve(argv.tmpDir)
    : path.join(os.tmpdir(), `sprite-importer-init-${Date.now()}`);
  console.log(`Using temporary directory: ${resolvedTmpDirPath}`);
  const manager = new GitHubPagesManager(
    argv.repo as string,
    resolvedTmpDirPath,
    argv.branch as string | undefined,
  );
  try {
    await manager.cloneOrUpdateRepo();

    const gameDir = manager.getFilePath(argv.game as string);
    const nojekyll = manager.getFilePath(".nojekyll");
    const rootIndex = manager.getFilePath("index.html");
    const spritesJsonPath = manager.getFilePath(
      path.join(argv.game as string, "sprites.json"),
    );

    if (dryRun) {
      console.log(`[dry-run] Would ensure directory exists: ${gameDir}`);
      console.log(
        `[dry-run] Would create .nojekyll and index.html at repo root if missing.`,
      );
    } else {
      await fs.ensureDir(gameDir);
      console.log(`Ensured directory exists: ${gameDir}`);
      if (!(await fs.pathExists(nojekyll))) {
        await fs.writeFile(nojekyll, "");
        console.log("Created .nojekyll");
      }
      if (!(await fs.pathExists(rootIndex))) {
        const html =
          '<!doctype html><meta charset="utf-8"/><title>Sprites</title><h1>Sprite Library</h1><p>Per-game folders contain sprites.json and assets.</p>';
        await fs.writeFile(rootIndex, html);
        console.log("Created index.html");
      }
    }

    if (await fs.pathExists(spritesJsonPath)) {
      console.log(
        `sprites.json already exists at ${spritesJsonPath}. No changes.`,
      );
    } else {
      if (dryRun) {
        console.log(
          `[dry-run] Would create empty sprites.json at ${spritesJsonPath}`,
        );
      } else {
        await fs.writeJson(spritesJsonPath, {}, { spaces: 2 });
        console.log(`Created empty sprites.json at ${spritesJsonPath}`);
        await manager.commitChanges(`Init game directory: ${argv.game}`);
        await manager.pushChanges();
      }
    }

    console.log(`Init complete for game "${argv.game}".`);
  } catch (error: unknown) {
    if (typeof error === "object" && error !== null && "message" in error) {
      console.error(
        `Error during remote init: ${(error as { message?: string }).message}`,
      );
    } else {
      console.error("Error during remote init:", error);
    }
    process.exitCode = 1;
  }
}
