import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import fs from "fs-extra";
import path from "path";

import { main } from "../index";

vi.mock("fs-extra", async (importOriginal) => {
  const original = (await importOriginal()) as typeof fs;
  const mocked = {
    ensureDir: vi.fn().mockResolvedValue(undefined),
    readJson: vi.fn().mockResolvedValue({}),
    readdir: vi.fn().mockResolvedValue([]),
    writeJson: vi.fn().mockResolvedValue(undefined),
    existsSync: vi.fn().mockReturnValue(false),
  } satisfies Partial<typeof fs>;
  const merged = { ...original, ...mocked } as typeof fs;
  return {
    __esModule: true,
    ...merged,
    default: merged,
  } as unknown as typeof fs & { default: typeof fs };
});

vi.mock("../zspr", () => ({
  ZsprParser: {
    parse: vi.fn(),
  },
}));

vi.mock("../image-renderer", () => ({
  renderZ3AvatarImage: vi.fn(() => ({ data: Buffer.alloc(0) })),
  renderSMAvatarImage: vi.fn(() => ({ data: Buffer.alloc(0) })),
  renderNESAvatarImage: vi.fn(() => ({ data: Buffer.alloc(0) })),
}));

vi.mock("../rdc", () => ({
  Rdc: {
    parse: vi.fn(),
    write: vi.fn(),
  },
}));

vi.mock("../rdc-types", async (importOriginal) => {
  const original = (await importOriginal()) as any;
  return {
    ...original,
    LinkSprite: vi.fn().mockImplementation(() => ({
      setContent: vi.fn(),
    })),
    SamusSprite: vi.fn().mockImplementation(() => ({
      setContent: vi.fn(),
    })),
    Zelda1SpriteDataBlock: vi.fn().mockImplementation(() => ({
      setContent: vi.fn(),
    })),
    Metroid1SpriteDataBlock: vi.fn().mockImplementation(() => ({
      setContent: vi.fn(),
    })),
    MetaDataBlock: vi.fn().mockImplementation(() => ({
      content: {},
    })),
  };
});

describe("sprite-importer CLI", () => {
  const consoleError = vi.spyOn(console, "error");
  const consoleLog = vi.spyOn(console, "log");

  beforeEach(() => {
    vi.clearAllMocks();
    consoleError.mockImplementation(() => {});
    consoleLog.mockImplementation(() => {});
    vi.spyOn(process, "exit").mockImplementation(((code?: number) => {
      throw new Error(`process.exit(${code})`);
    }) as never);
    (fs.existsSync as unknown as vi.Mock).mockImplementation(
      (target: string) => {
        const normalized = path.resolve(target);
        return normalized.includes("/sprites-source");
      },
    );
  });

  afterEach(() => {
    consoleError.mockReset();
    consoleLog.mockReset();
  });

  it("requires --sourceDir for local imports", async () => {
    await expect(main(["--game", "alttp"])).rejects.toThrow("process.exit(1)");
    expect(consoleError).toHaveBeenCalledWith(
      "Error: --sourceDir is required for sprite importing.",
    );
  });

  it("creates the game output directory when input exists", async () => {
    const ensureDir = fs.ensureDir as unknown as vi.Mock;
    const readdir = fs.readdir as unknown as vi.Mock;
    readdir.mockResolvedValueOnce([]);

    await main([
      "--game",
      "alttp",
      "--sourceDir",
      "/tmp/sprites-source",
      "--outputDir",
      "/tmp/out",
    ]);

    expect(ensureDir).toHaveBeenCalledWith(path.resolve("/tmp/out/alttp"));
    expect(readdir).toHaveBeenCalled();
  });
});
