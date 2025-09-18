import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import path from "path";
import fs from "fs-extra";

// Mock fs-extra
vi.mock("fs-extra", async (importOriginal) => {
  const original = (await importOriginal()) as typeof fs;
  const mocked: Partial<typeof fs> = {
    ensureDir: vi.fn().mockResolvedValue(undefined),
    pathExists: vi.fn(),
    readJson: vi.fn(),
  };
  const merged = { ...original, ...mocked } as typeof fs;
  return {
    __esModule: true,
    ...merged,
    default: merged,
  } as unknown as typeof fs & {
    default: typeof fs;
  };
});

// Mock manager
vi.mock("../github-pages-manager", () => {
  class GitHubPagesManager {
    repoUrl: string;
    localPath: string;
    constructor(repoUrl: string, localPath: string) {
      this.repoUrl = repoUrl;
      this.localPath = localPath;
    }
    async cloneOrUpdateRepo() {}
    getFilePath(relative: string) {
      return path.join("/mocked/repo", relative);
    }
  }
  return { GitHubPagesManager };
});

import { main } from "../index";

describe("remote list CLI", () => {
  let logSpy: vi.SpyInstance;
  let errSpy: vi.SpyInstance;

  beforeEach(() => {
    vi.resetAllMocks();
    logSpy = vi.spyOn(console, "log").mockImplementation(() => {});
    errSpy = vi.spyOn(console, "error").mockImplementation(() => {});
  });

  afterEach(() => {
    logSpy.mockRestore();
    errSpy.mockRestore();
  });

  it("lists sprites for a game when sprites.json exists", async () => {
    // Setup fs mocks
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(true);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({
      sprites: [
        { value: "LinkSprite", name: "Link", imagePath: "linksprite.png" },
        { value: "Other", name: "Other", imagePath: "other.png" },
      ],
    });

    await main([
      "remote",
      "list",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      ".",
      "--tmpDir",
      "/tmp/mock",
    ]);

    const logs = (console.log as unknown as vi.Mock).mock.calls
      .flat()
      .join("\n");
    expect(logs).toContain("Sprites for game: alttp");
    expect(logs).toContain("Sprite: LinkSprite");
    expect(logs).toContain("Sprite: Other");
  });

  it("prints a friendly message when sprites.json is missing", async () => {
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(false);
    await main([
      "remote",
      "list",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      ".",
    ]);
    const logs = (console.log as unknown as vi.Mock).mock.calls
      .flat()
      .join("\n");
    expect(logs).toMatch(/No sprites\.json found/);
  });
});
