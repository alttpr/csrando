import fs from "fs-extra";
import path from "path";
import os from "os";
import { GitHubPagesManager } from "../github-pages-manager";
import type { GameSpriteConfig } from "../types";

export async function remoteVerify(argv: any) {
  console.log(`Remote verify for game: ${argv.game}`);
  console.log(`Repository: ${argv.repo}`);
  const resolvedTmpDirPath = argv.tmpDir
    ? path.resolve(argv.tmpDir)
    : path.join(os.tmpdir(), `sprite-importer-verify-${Date.now()}`);
  console.log(`Using temporary directory: ${resolvedTmpDirPath}`);
  const manager = new GitHubPagesManager(
    argv.repo as string,
    resolvedTmpDirPath,
    argv.branch as string | undefined,
  );
  try {
    await manager.cloneOrUpdateRepo();
    const spritesJsonPath = manager.getFilePath(
      path.join(argv.game as string, "sprites.json"),
    );
    if (!(await fs.pathExists(spritesJsonPath))) {
      console.error(`Error: sprites.json not found for game "${argv.game}".`);
      process.exitCode = 1;
      return;
    }
    const cfg: GameSpriteConfig = await fs.readJson(spritesJsonPath);
    let total = 0;
    let missing = 0;
    console.log(`\nVerifying entries in ${spritesJsonPath}`);
    for (const s of cfg?.sprites || []) {
      total++;
      const pngAbs = manager.getFilePath(
        path.join(argv.game as string, s.imagePath),
      );
      let ok = true;
      if (!(await fs.pathExists(pngAbs))) {
        console.error(`Missing PNG for sprite "${s.value}": ${pngAbs}`);
        ok = false;
      }
      if (s.rdcPath) {
        const rdcAbs = manager.getFilePath(
          path.join(argv.game as string, s.rdcPath),
        );
        if (!(await fs.pathExists(rdcAbs))) {
          console.error(`Missing RDC for sprite "${s.value}": ${rdcAbs}`);
          ok = false;
        }
      } else if (s.patchDetails?.files) {
        for (const f of s.patchDetails.files) {
          const abs = manager.getFilePath(
            path.join(argv.game as string, f.path),
          );
          if (!(await fs.pathExists(abs))) {
            console.error(
              `Missing file (${f.id}) for sprite "${s.value}": ${abs}`,
            );
            ok = false;
          }
        }
      }
      if (!ok) missing++;
    }
    if (missing > 0) {
      console.error(
        `\nVerification failed: ${missing}/${total} sprite(s) have missing assets.`,
      );
      process.exitCode = 1;
    } else {
      console.log(`\nVerification successful: ${total} sprite(s) verified.`);
    }
  } catch (e: unknown) {
    console.error("Database error while verifying:", e);
    process.exitCode = 1;
  }
}
