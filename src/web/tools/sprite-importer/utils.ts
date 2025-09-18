import fs from "fs-extra";
import path from "path";
import zlib from "zlib";
import { PNG } from "pngjs";
import { Rdc } from "./rdc";
import {
  LinkSprite,
  SamusSprite,
  MetaDataBlock,
  Zelda1SpriteDataBlock,
  Metroid1SpriteDataBlock,
} from "./rdc-types";
import {
  renderNESAvatarImage,
  renderSMAvatarImage,
  renderZ3AvatarImage,
} from "./image-renderer";
import type { GameSpriteConfig } from "./types";

export function sanitizeId(str: string): string {
  return str
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9-_]+/g, "_");
}

export function isGzippedRdc(filePath: string): boolean {
  return /\.rdc\.gz$/i.test(filePath);
}

export function stripRdcOrGzExtension(filePath: string): string {
  return path
    .basename(filePath)
    .replace(/\.rdc\.gz$/i, "")
    .replace(/\.rdc$/i, "")
    .replace(/\.gz$/i, ""); // fallback
}

export async function readRdcFileDecompressed(
  filePath: string,
): Promise<Buffer> {
  const raw = await fs.readFile(filePath);
  if (isGzippedRdc(filePath) || filePath.toLowerCase().endsWith(".gz")) {
    try {
      return zlib.gunzipSync(raw);
    } catch (e) {
      console.warn(
        `Warning: failed to gunzip ${filePath}, attempting to parse raw buffer.`,
        e,
      );
      return raw;
    }
  }
  return raw;
}

export function getTitleAuthorFromRdc(rdcBuffer: Buffer): {
  title?: string;
  author?: string;
} {
  try {
    const rdc = Rdc.parse(rdcBuffer);
    let title: string | undefined;
    let author: string | undefined = (rdc as unknown as { author?: string })
      .author;
    if (rdc.contains(MetaDataBlock.RDC_TYPE_ID)) {
      const meta = rdc.tryParseBlock(rdcBuffer, MetaDataBlock) as
        | { content?: { title?: unknown; author?: unknown } }
        | undefined;
      const t = meta?.content?.title;
      const a = meta?.content?.author;
      if (typeof t === "string" && t.trim()) title = t.trim();
      if (typeof a === "string" && a.trim()) author = a.trim();
    }
    return { title, author };
  } catch {
    return {};
  }
}

export function renderPreviewImageForGame(
  rdcBuffer: Buffer,
  game: "alttp" | "sm" | "zelda1" | "metroid1",
): PNG | undefined {
  const rdc = Rdc.parse(rdcBuffer);
  if (rdc.contains(LinkSprite.RDC_TYPE_ID) && game === "alttp") {
    const link = rdc.tryParseBlock(rdcBuffer, LinkSprite);
    if (link) return renderZ3AvatarImage(link);
  } else if (rdc.contains(SamusSprite.RDC_TYPE_ID) && game === "sm") {
    const samus = rdc.tryParseBlock(rdcBuffer, SamusSprite);
    if (samus) return renderSMAvatarImage(samus);
  } else if (
    game === "zelda1" &&
    rdc.contains(Zelda1SpriteDataBlock.RDC_TYPE_ID)
  ) {
    const z1 = rdc.tryParseBlock(rdcBuffer, Zelda1SpriteDataBlock);
    if (z1) return renderNESAvatarImage(z1);
  } else if (
    game === "metroid1" &&
    rdc.contains(Metroid1SpriteDataBlock.RDC_TYPE_ID)
  ) {
    const m1 = rdc.tryParseBlock(rdcBuffer, Metroid1SpriteDataBlock);
    if (m1) return renderNESAvatarImage(m1);
  }
  return undefined;
}

export async function loadGameConfig(
  filePath: string,
): Promise<GameSpriteConfig> {
  if (!(await fs.pathExists(filePath))) return { sprites: [] };
  const parsed = await fs.readJson(filePath).catch(() => ({}));
  if (parsed && Array.isArray(parsed.sprites))
    return parsed as GameSpriteConfig;
  return { sprites: [] };
}

export async function saveGameConfig(
  filePath: string,
  cfg: GameSpriteConfig,
): Promise<void> {
  await fs.writeJson(filePath, cfg, { spaces: 2 });
}
