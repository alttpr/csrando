import path from "path";
import fs from "fs-extra";
import { PNG } from "pngjs";
import crypto from "crypto";

export interface AtlasSpriteEntry {
  value: string; // sprite key
  name?: string;
  author?: string;
  x: number;
  y: number;
  w: number;
  h: number;
  originalImagePath: string; // relative path in repo (as in sprites.json entry)
}

export interface AtlasJson {
  version: 1;
  game: string;
  hash: string;
  generatedAt: string; // ISO string
  sheet: {
    image: string; // relative path to sheet image (same folder)
    width: number;
    height: number;
  };
  cell: { w: number; h: number };
  sprites: AtlasSpriteEntry[];
  byValue: Record<
    string,
    { x: number; y: number; w: number; h: number; i: number }
  >; // compact index
}

export interface BuildAtlasOptions {
  game: string;
  gameDir: string; // absolute path to game folder inside remote repo clone
  spritesJsonPath: string; // absolute path to sprites.json
  maxWidth?: number; // default 2048
  dryRun?: boolean;
  keep?: number; // number of old sheets to keep (default 2)
  logger?: (msg: string) => void;
}

interface GameSpriteConfigSprite {
  value: string;
  name: string;
  imagePath: string; // relative to game folder
  author?: string;
  // other fields ignored
}
interface GameSpriteConfig {
  sprites?: GameSpriteConfigSprite[];
}

function hashBuffer(buf: Buffer): string {
  return crypto.createHash("sha256").update(buf).digest("hex").slice(0, 16);
}

export async function buildAtlas(
  opts: BuildAtlasOptions,
): Promise<AtlasJson | undefined> {
  const log = opts.logger || (() => {});
  const maxWidth = opts.maxWidth ?? 2048;
  const keep = opts.keep ?? 2;

  if (!(await fs.pathExists(opts.spritesJsonPath))) {
    log(`sprites.json not found at ${opts.spritesJsonPath}; nothing to build.`);
    return;
  }
  const raw = (await fs
    .readJson(opts.spritesJsonPath)
    .catch(() => undefined)) as GameSpriteConfig | undefined;
  if (!raw || !Array.isArray(raw.sprites) || raw.sprites.length === 0) {
    log("sprites.json has no sprites; skipping atlas build.");
    return;
  }

  // Collect candidate PNGs
  const sprites: GameSpriteConfigSprite[] = raw.sprites.filter(
    (s) => !!s.imagePath,
  );
  if (sprites.length === 0) {
    log("No sprites with imagePath; aborting.");
    return;
  }

  // Load first image to determine cell size
  const firstPngAbs = path.join(opts.gameDir, sprites[0].imagePath);
  if (!(await fs.pathExists(firstPngAbs))) {
    log(`First sprite image missing: ${firstPngAbs}`);
    return;
  }
  const firstPng = PNG.sync.read(await fs.readFile(firstPngAbs));
  const cellW = firstPng.width;
  const cellH = firstPng.height;

  // Verify uniform dimensions
  for (const s of sprites) {
    const abs = path.join(opts.gameDir, s.imagePath);
    if (!(await fs.pathExists(abs))) {
      throw new Error(`Missing sprite PNG: ${abs}`);
    }
    const png = PNG.sync.read(await fs.readFile(abs));
    if (png.width !== cellW || png.height !== cellH) {
      throw new Error(
        `Sprite dimension mismatch for ${s.value} (${png.width}x${png.height}) expected ${cellW}x${cellH}`,
      );
    }
  }

  // Packing: single sheet grid
  const columns = Math.max(1, Math.floor(maxWidth / cellW));
  const rows = Math.ceil(sprites.length / columns);
  const sheetW = Math.min(columns * cellW, maxWidth);
  const sheetH = rows * cellH;
  log(
    `Packing ${sprites.length} sprites: cell ${cellW}x${cellH}, columns=${columns}, rows=${rows}, sheet=${sheetW}x${sheetH}`,
  );

  const sheetPng = new PNG({ width: sheetW, height: sheetH });
  // Initialize transparent
  sheetPng.data.fill(0);

  const atlasSprites: AtlasSpriteEntry[] = [];

  // Deterministic order: sort by value
  sprites.sort((a, b) => a.value.localeCompare(b.value));

  for (let i = 0; i < sprites.length; i++) {
    const s = sprites[i];
    const col = i % columns;
    const row = Math.floor(i / columns);
    const x = col * cellW;
    const y = row * cellH;
    const abs = path.join(opts.gameDir, s.imagePath);
    const png = PNG.sync.read(await fs.readFile(abs));
    PNG.bitblt(png, sheetPng, 0, 0, cellW, cellH, x, y);
    atlasSprites.push({
      value: s.value,
      name: s.name,
      author: s.author,
      x,
      y,
      w: cellW,
      h: cellH,
      originalImagePath: s.imagePath,
    });
  }

  const sheetBuffer = PNG.sync.write(sheetPng);
  const hash = hashBuffer(sheetBuffer);
  const sheetBaseName = `sheet-${opts.game}-${hash}`;
  const sheetImageRel = `${sheetBaseName}.png`;
  const sheetJsonRel = `${sheetBaseName}.json`;

  const atlas: AtlasJson = {
    version: 1,
    game: opts.game,
    hash,
    generatedAt: new Date().toISOString(),
    sheet: { image: sheetImageRel, width: sheetW, height: sheetH },
    cell: { w: cellW, h: cellH },
    sprites: atlasSprites,
    byValue: {},
  };
  atlasSprites.forEach((s, i) => {
    atlas.byValue[s.value] = { x: s.x, y: s.y, w: s.w, h: s.h, i };
  });

  if (opts.dryRun) {
    log(`[dry-run] Would write ${sheetImageRel} & ${sheetJsonRel}`);
    return atlas;
  }

  await fs.writeFile(path.join(opts.gameDir, sheetImageRel), sheetBuffer);
  await fs.writeJson(path.join(opts.gameDir, sheetJsonRel), atlas, {
    spaces: 2,
  });

  // Update latest aliases (copy, not symlink for cross-platform GH Pages compatibility)
  const latestImage = path.join(opts.gameDir, `sheet-${opts.game}-latest.png`);
  const latestJson = path.join(opts.gameDir, `sheet-${opts.game}-latest.json`);
  await fs.copyFile(path.join(opts.gameDir, sheetImageRel), latestImage);
  await fs.copyFile(path.join(opts.gameDir, sheetJsonRel), latestJson);

  // Prune old sheets (keep newest N by mtime) excluding latest aliases
  const files = await fs.readdir(opts.gameDir);
  const sheetFiles = files.filter(
    (f) =>
      f.startsWith(`sheet-${opts.game}-`) &&
      (f.endsWith(".png") || f.endsWith(".json")) &&
      !f.endsWith("latest.png") &&
      !f.endsWith("latest.json"),
  );
  // Group by hash baseName
  const grouped = new Map<string, { paths: string[]; latestMtime: number }>();
  for (const f of sheetFiles) {
    const base = f.replace(/\.(png|json)$/i, "");
    const rec = grouped.get(base) || { paths: [], latestMtime: 0 };
    const full = path.join(opts.gameDir, f);
    const stat = await fs.stat(full);
    rec.paths.push(full);
    rec.latestMtime = Math.max(rec.latestMtime, stat.mtimeMs);
    grouped.set(base, rec);
  }
  const groups = Array.from(grouped.entries()).sort(
    (a, b) => b[1].latestMtime - a[1].latestMtime,
  );
  const toRemove = groups.slice(keep); // keep newest 'keep'
  for (const [_base, info] of toRemove) {
    for (const p of info.paths) {
      try {
        await fs.remove(p);
        log(`Pruned old sheet file: ${p}`);
      } catch {
        /* ignore */
      }
    }
  }

  log(
    `Atlas built: ${sheetImageRel} (hash ${hash}) with ${atlasSprites.length} sprites.`,
  );
  return atlas;
}
