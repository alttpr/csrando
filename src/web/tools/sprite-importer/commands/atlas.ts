import fs from "fs-extra";
import path from "path";
import os from "os";
import { GitHubPagesManager } from "../github-pages-manager";
import { buildAtlas } from "../atlas-builder";
import type { GameSpriteConfig, SpriteInfoFrontend } from "../types";
import { loadGameConfig } from "../utils";

interface AtlasArgsBase {
  repo?: string;
  game?: string;
  all?: boolean;
  tmpDir?: string;
  branch?: string;
  dryRun?: boolean;
  maxWidth?: number;
  keep?: number;
}

export async function handleRemoteAtlas(argv: unknown) {
  const args = argv as AtlasArgsBase;
  if (!args.repo) throw new Error("Missing --repo");
  if (!args.all && !args.game)
    throw new Error("Must supply --game <id> or --all");

  const repoUrl = args.repo;
  const dryRun = Boolean(args.dryRun);
  const tmpDir = args.tmpDir
    ? path.resolve(args.tmpDir)
    : path.join(os.tmpdir(), `sprite-importer-atlas-${Date.now()}`);
  console.log(`Using temporary directory: ${tmpDir}`);
  const manager = new GitHubPagesManager(repoUrl, tmpDir, args.branch);

  // Clone/update repo (even for dry-run so we can inspect current sprites.json files)
  await manager.cloneOrUpdateRepo();

  // Determine game list
  let games: string[] = [];
  if (args.all) {
    // Detect directories that contain sprites.json
    const rootEntries = await fs.readdir(manager.getFilePath("."));
    for (const entry of rootEntries) {
      const spritesJson = manager.getFilePath(path.join(entry, "sprites.json"));
      if (await fs.pathExists(spritesJson)) games.push(entry);
    }
  } else if (args.game) {
    games = [args.game];
  }
  games = games.filter((g) =>
    ["alttp", "sm", "zelda1", "metroid1"].includes(g),
  );
  if (games.length === 0) {
    console.log("No game directories found to build atlases for.");
    return;
  }

  const results: Array<{
    game: string;
    spriteCount: number;
    hash?: string;
    skipped?: string;
  }> = [];

  for (const game of games) {
    const gameDir = manager.getFilePath(game);
    const spritesJsonPath = manager.getFilePath(
      path.join(game, "sprites.json"),
    );
    if (!(await fs.pathExists(spritesJsonPath))) {
      console.warn(`Skipping ${game}: sprites.json not found.`);
      results.push({ game, spriteCount: 0, skipped: "missing sprites.json" });
      continue;
    }
    let cfg: GameSpriteConfig = { sprites: [] };
    try {
      cfg = await loadGameConfig(spritesJsonPath);
    } catch (e) {
      console.warn(`Skipping ${game}: failed to read/parse sprites.json`, e);
      results.push({ game, spriteCount: 0, skipped: "invalid sprites.json" });
      continue;
    }
    if (!cfg.sprites || cfg.sprites.length === 0) {
      console.log(`Skipping ${game}: no sprites in config.`);
      results.push({ game, spriteCount: 0, skipped: "no sprites" });
      continue;
    }

    // Transform to minimal atlas config format expected by buildAtlas
    const atlasSprites = cfg.sprites.map((s: SpriteInfoFrontend) => ({
      value: s.value,
      name: s.name,
      author: s.author,
      imagePath: s.imagePath, // relative within game directory
    }));

    // Validate PNG existence
    for (const spr of atlasSprites) {
      const pngAbs = manager.getFilePath(path.join(game, spr.imagePath));
      if (!(await fs.pathExists(pngAbs))) {
        throw new Error(
          `Sprite PNG missing for game ${game}: ${spr.imagePath} (expected at ${pngAbs})`,
        );
      }
    }

    // Write a temporary synthesized sprites file for atlas-builder
    const synthPath = manager.getFilePath(
      path.join(game, "__atlas_sprites.temp.json"),
    );
    await fs.writeJson(synthPath, { sprites: atlasSprites }, { spaces: 2 });

    console.log(
      `Building atlas for ${game} with ${atlasSprites.length} sprites (maxWidth=${
        args.maxWidth ?? 2048
      }, keep=${args.keep ?? 2})${dryRun ? " [dry-run]" : ""}`,
    );
    try {
      const atlas = await buildAtlas({
        game,
        gameDir,
        spritesJsonPath: synthPath,
        maxWidth: args.maxWidth,
        keep: args.keep,
        dryRun,
        logger: (m) => console.log(`[${game}] ${m}`),
      });
      if (!dryRun) {
        // Remove temp synth file; not needed after build
        await fs.remove(synthPath).catch(() => {});
      }
      if (atlas)
        results.push({
          game,
          spriteCount: atlas.sprites.length,
          hash: atlas.hash,
        });
      else
        results.push({
          game,
          spriteCount: atlasSprites.length,
          skipped: "no-atlas-returned",
        });
    } catch (e: any) {
      console.error(`Failed to build atlas for ${game}: ${e.message || e}`);
      results.push({ game, spriteCount: 0, skipped: "error" });
    }
  }

  if (dryRun) {
    console.log("Dry-run complete. No files written, no commit performed.");
    return;
  }

  // Commit & push if any atlases built
  const built = results.filter((r) => r.hash);
  if (built.length > 0) {
    const summary = built.map((b) => `${b.game}:${b.hash}`).join(", ");
    await manager.commitChanges(`Build sprite atlases: ${summary}`);
    await manager.pushChanges();
  } else {
    console.log("No atlases built; skipping commit.");
  }

  console.log("Atlas build summary:");
  for (const r of results) {
    if (r.hash)
      console.log(`  ${r.game}: ${r.spriteCount} sprites (hash ${r.hash})`);
    else console.log(`  ${r.game}: skipped (${r.skipped})`);
  }
}
