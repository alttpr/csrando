import fs from "fs-extra";
import path from "path";
import os from "os";
import { GitHubPagesManager } from "../github-pages-manager";
import type { GameSpriteConfig } from "../types";

export async function remoteList(argv: any) {
  console.log(`Remote list for game: ${argv.game}`);
  console.log(`Repository: ${argv.repo}`);
  let resolvedTmpDirPath: string;
  if (argv.tmpDir) resolvedTmpDirPath = path.resolve(argv.tmpDir);
  else
    resolvedTmpDirPath = path.join(
      os.tmpdir(),
      `sprite-importer-clone-${Date.now()}`,
    );
  console.log(`Using temporary directory: ${resolvedTmpDirPath}`);

  const manager = new GitHubPagesManager(
    argv.repo as string,
    resolvedTmpDirPath,
    argv.branch as string | undefined,
  );

  try {
    await manager.cloneOrUpdateRepo();
    const spritesJsonPathInRepo = manager.getFilePath(
      path.join(argv.game as string, "sprites.json"),
    );
    if (await fs.pathExists(spritesJsonPathInRepo)) {
      const cfg: GameSpriteConfig = await fs.readJson(spritesJsonPathInRepo);
      console.log(`\nSprites for game: ${argv.game}`);
      console.log("------------------------------------");
      const arr = Array.isArray(cfg?.sprites) ? cfg.sprites : [];
      if (arr.length === 0) {
        console.log("No sprites found in sprites.json for this game.");
      }
      for (const s of arr) {
        console.log(`Sprite: ${s.value}`);
        console.log(`  Title: ${s.name || "N/A"}`);
        console.log(`  Author: ${s.author || "N/A"}`);
        console.log("---");
      }
      console.log(`Found ${arr.length} sprite(s).`);
      console.log("------------------------------------");
    } else {
      console.log(
        `No sprites.json found for game "${argv.game}" in the repository at path: ${path.join(argv.game as string, "sprites.json")}`,
      );
    }
  } catch (error: unknown) {
    if (typeof error === "object" && error !== null && "message" in error) {
      console.error(
        `Error during remote list operation: ${(error as { message?: string }).message}`,
      );
      if ("stack" in error) console.error((error as { stack?: string }).stack);
    } else {
      console.error("Error during remote list operation:", error);
    }
    process.exitCode = 1;
  }
}
