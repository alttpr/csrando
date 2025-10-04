import { describe, it, expect, vi, beforeEach } from "vitest";
import path from "path";

// Mock fs-extra
import fs from "fs-extra";
vi.mock("fs-extra", async (importOriginal) => {
  const original = (await importOriginal()) as typeof fs;
  const mocked: Partial<typeof fs> = {
    ensureDir: vi.fn().mockResolvedValue(undefined),
    pathExists: vi.fn(),
    readdir: vi.fn(),
    readJson: vi.fn(),
    writeJson: vi.fn().mockResolvedValue(undefined),
    copy: vi.fn().mockResolvedValue(undefined),
    writeFile: vi.fn().mockResolvedValue(undefined),
    readFile: vi.fn().mockResolvedValue(Buffer.from("")),
    existsSync: vi.fn(),
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

// Mock GitHubPagesManager
const cloneSpy = vi.fn();
const commitSpy = vi.fn();
const pushSpy = vi.fn();
vi.mock("../github-pages-manager", () => {
  class GitHubPagesManager {
    repoUrl: string;
    localPath: string;
    constructor(repoUrl: string, localPath: string) {
      this.repoUrl = repoUrl;
      this.localPath = localPath;
    }
    async cloneOrUpdateRepo() {
      return cloneSpy();
    }
    async commitChanges(msg: string) {
      return commitSpy(msg);
    }
    async pushChanges() {
      return pushSpy();
    }
    getFilePath(relative: string) {
      return path.join("/mocked/repo", relative);
    }
  }
  return { GitHubPagesManager };
});

// Mock RDC parse to provide author and avoid render path when sourcePng is provided
vi.mock("../rdc", () => ({
  Rdc: {
    parse: vi.fn().mockReturnValue({
      author: "Mock Author",
      contains: vi.fn().mockReturnValue(false),
      tryParseBlock: vi.fn(),
    }),
  },
}));

// Import after mocks
import { main } from "../index";

describe("remote add/update CLI", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(false);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({});
    (fs.existsSync as unknown as vi.Mock).mockImplementation((p: string) => {
      // Simulate local source files existing
      return p.endsWith(".rdc") || p.endsWith(".png");
    });
  });

  it("remote add with provided PNG copies files, updates sprites.json, commits and pushes", async () => {
    const args = [
      "remote",
      "add",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--spriteName",
      "LinkSprite",
      "--sourceRdc",
      "/local/path/link.rdc",
      "--sourcePng",
      "/local/path/link.png",
      "--sourceDir",
      ".",
      "--tmpDir",
      "/tmp/mock",
    ];

    await main(args);

    expect(cloneSpy).toHaveBeenCalled();

    // Copies
    expect(fs.copy).toHaveBeenCalledWith(
      "/local/path/link.rdc",
      path.join("/mocked/repo", "alttp", "linksprite.rdc"),
    );
    expect(fs.copy).toHaveBeenCalledWith(
      "/local/path/link.png",
      path.join("/mocked/repo", "alttp", "linksprite.png"),
    );

    // sprites.json write
    const spritesJsonPath = path.join("/mocked/repo", "alttp", "sprites.json");
    expect(fs.writeJson).toHaveBeenCalled();
    const call = (fs.writeJson as unknown as vi.Mock).mock.calls.find(
      (c) => c[0] === spritesJsonPath,
    );
    expect(call).toBeTruthy();
    const written = call[1];
    expect(Array.isArray(written.sprites)).toBe(true);
    expect(
      written.sprites.some(
        (s: any) =>
          s.value === "LinkSprite" && s.imagePath === "linksprite.png",
      ),
    ).toBe(true);

    expect(commitSpy).toHaveBeenCalledWith(
      "Add sprite: LinkSprite for game alttp",
    );
    expect(pushSpy).toHaveBeenCalled();
  });

  it("remote update merges existing sprites.json", async () => {
    // Simulate existing sprites.json with one entry
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(true);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({
      sprites: [
        { value: "Existing", name: "Old", author: "X", imagePath: "old.png" },
      ],
    });

    const args = [
      "remote",
      "update",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--spriteName",
      "LinkSprite",
      "--sourceRdc",
      "/local/path/link.rdc",
      "--sourcePng",
      "/local/path/link.png",
      "--sourceDir",
      ".",
    ];

    await main(args);

    const spritesJsonPath = path.join("/mocked/repo", "alttp", "sprites.json");
    const writeCall = (fs.writeJson as unknown as vi.Mock).mock.calls.find(
      (c) => c[0] === spritesJsonPath,
    );
    expect(writeCall).toBeTruthy();
    const written = writeCall[1];
    // Existing retained
    expect(written.sprites.some((s: any) => s.value === "Existing")).toBe(true);
    // New merged
    expect(written.sprites.some((s: any) => s.value === "LinkSprite")).toBe(
      true,
    );

    expect(commitSpy).toHaveBeenCalledWith(
      "Update sprite: LinkSprite for game alttp",
    );
    expect(pushSpy).toHaveBeenCalled();
  });

  it("remote add dry-run does not write or commit", async () => {
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(false);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({});
    const args = [
      "remote",
      "add",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--spriteName",
      "LinkSprite",
      "--sourceRdc",
      "/local/path/link.rdc",
      "--sourcePng",
      "/local/path/link.png",
      "--sourceDir",
      ".",
      "--tmpDir",
      "/tmp/mock",
      "--dryRun",
    ];

    await main(args);

    expect(fs.copy).not.toHaveBeenCalled();
    expect(fs.writeJson).not.toHaveBeenCalled();
    expect(commitSpy).not.toHaveBeenCalled();
    expect(pushSpy).not.toHaveBeenCalled();
  });

  it("remote update dry-run preserves existing sprites.json without writes or commits", async () => {
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(true);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({
      Existing: {
        title: "Old",
        author: "X",
        game: "alttp",
        path: "old.png",
        files: { rdc: "old.rdc" },
      },
    });
    const args = [
      "remote",
      "update",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--spriteName",
      "LinkSprite",
      "--sourceRdc",
      "/local/path/link.rdc",
      "--sourcePng",
      "/local/path/link.png",
      "--sourceDir",
      ".",
      "--dryRun",
    ];
    await main(args);
    expect(fs.copy).not.toHaveBeenCalled();
    expect(fs.writeJson).not.toHaveBeenCalled();
    expect(commitSpy).not.toHaveBeenCalled();
    expect(pushSpy).not.toHaveBeenCalled();
  });

  it("remote remove dry-run does not delete or commit", async () => {
    // sprites.json exists with entry and files
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(true);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({
      sprites: [
        {
          value: "LinkSprite",
          name: "Old",
          author: "X",
          imagePath: "linksprite.png",
          patchDetails: { files: [{ id: "rdc", path: "linksprite.rdc" }] },
        },
      ],
    });
    const removeSpy = vi
      .spyOn(fs, "remove")
      .mockResolvedValue(undefined as unknown as void);

    const args = [
      "remote",
      "remove",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--spriteName",
      "LinkSprite",
      "--sourceDir",
      ".",
      "--dryRun",
    ];
    await main(args);

    expect(removeSpy).not.toHaveBeenCalled();
    expect(fs.writeJson).not.toHaveBeenCalled();
    expect(commitSpy).not.toHaveBeenCalled();
    expect(pushSpy).not.toHaveBeenCalled();
  });

  it("remote remove deletes files, updates json, and commits", async () => {
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(true);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({
      sprites: [
        {
          value: "LinkSprite",
          name: "Old",
          author: "X",
          imagePath: "linksprite.png",
          patchDetails: { files: [{ id: "rdc", path: "linksprite.rdc" }] },
        },
      ],
    });
    const args = [
      "remote",
      "remove",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--spriteName",
      "LinkSprite",
      "--sourceDir",
      ".",
    ];
    await main(args);
    // fs.remove should be called at least once (for png)
    expect(fs.remove as unknown as vi.Mock).toHaveBeenCalled();
    expect(fs.writeJson).toHaveBeenCalled();
    expect(commitSpy).toHaveBeenCalled();
    expect(pushSpy).toHaveBeenCalled();
  });
});

describe("remote bulk-add CLI", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    (fs.pathExists as unknown as vi.Mock).mockResolvedValue(false);
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({});
    (fs.existsSync as unknown as vi.Mock).mockImplementation((p: string) => {
      return p.endsWith(".rdc") || p.endsWith(".png");
    });
  });

  it("bulk-add writes many, updates once, commits once", async () => {
    // Mock directory scan
    const readdirSpy = vi.spyOn(fs as any, "readdir").mockResolvedValue([
      { name: "A.rdc", isDirectory: () => false },
      { name: "B.rdc", isDirectory: () => false },
      { name: "not_me.txt", isDirectory: () => false },
    ]);

    const args = [
      "remote",
      "bulk-add",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      "/local/path",
    ];

    await main(args);

    expect(cloneSpy).toHaveBeenCalled();

    // JSON updated once with two entries
    const spritesJsonPath = path.join("/mocked/repo", "alttp", "sprites.json");
    expect(fs.writeJson).toHaveBeenCalled();
    const call = (fs.writeJson as unknown as vi.Mock).mock.calls.find(
      (c) => c[0] === spritesJsonPath,
    );
    expect(call).toBeTruthy();
    const written = call[1];
    const values = written.sprites.map((s: any) => s.value).sort();
    expect(values).toEqual(["a", "b"]);

    // Single commit
    expect(commitSpy).toHaveBeenCalledTimes(1);
    expect(pushSpy).toHaveBeenCalledTimes(1);
    expect((commitSpy as any).mock.calls[0][0]).toContain(
      "Bulk add sprites (2)",
    );

    readdirSpy.mockRestore();
  });

  it("bulk-add dry-run does not write or commit", async () => {
    const readdirSpy = vi
      .spyOn(fs as any, "readdir")
      .mockResolvedValue([{ name: "A.rdc", isDirectory: () => false }]);
    const args = [
      "remote",
      "bulk-add",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      "/local/path",
      "--dryRun",
    ];
    await main(args);
    expect(fs.writeJson).not.toHaveBeenCalled();
    expect(commitSpy).not.toHaveBeenCalled();
    expect(pushSpy).not.toHaveBeenCalled();
    readdirSpy.mockRestore();
  });
});
