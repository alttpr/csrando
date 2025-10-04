import { describe, it, expect, vi, beforeEach } from "vitest";
import path from "path";
import fs from "fs-extra";

// Mock fs-extra
vi.mock("fs-extra", async (importOriginal) => {
  const original = (await importOriginal()) as typeof fs;
  const mocked: Partial<typeof fs> = {
    ensureDir: vi.fn().mockResolvedValue(undefined),
    pathExists: vi.fn(),
    writeJson: vi.fn().mockResolvedValue(undefined),
    // @ts-expect-error simplified mock signature
    writeFile: vi.fn(async () => {}),
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
    async cloneOrUpdateRepo() {}
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

import { main } from "../index";

describe("remote init CLI", () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it("creates game directory and sprites.json when missing, then commits", async () => {
    // @ts-expect-error test mock casting
    (fs.pathExists as any).mockResolvedValue(false);
    const args = [
      "remote",
      "init",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      ".",
      "--tmpDir",
      "/tmp/mock",
    ];
    await main(args);
    expect(fs.ensureDir).toHaveBeenCalledWith("/mocked/repo/alttp");
    expect(fs.writeJson).toHaveBeenCalledWith(
      "/mocked/repo/alttp/sprites.json",
      {},
      { spaces: 2 },
    );
    expect(commitSpy).toHaveBeenCalledWith("Init game directory: alttp");
    expect(pushSpy).toHaveBeenCalled();
  });

  it("dry-run logs but does not write or commit", async () => {
    // @ts-expect-error test mock casting
    (fs.pathExists as any).mockResolvedValue(false);
    const ensureSpy = vi.spyOn(fs, "ensureDir");
    const args = [
      "remote",
      "init",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      ".",
      "--dryRun",
    ];
    await main(args);
    expect(ensureSpy).not.toHaveBeenCalled();
    expect(fs.writeJson).not.toHaveBeenCalled();
    expect(commitSpy).not.toHaveBeenCalled();
    expect(pushSpy).not.toHaveBeenCalled();
  });

  it("does nothing if sprites.json already exists", async () => {
    // @ts-expect-error test mock casting
    (fs.pathExists as any).mockResolvedValue(true);
    const args = [
      "remote",
      "init",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      ".",
    ];
    await main(args);
    expect(fs.writeJson).not.toHaveBeenCalled();
    // It still ensures the directory exists (clone path) but we don't assert ensureDir here
    expect(commitSpy).not.toHaveBeenCalled();
    expect(pushSpy).not.toHaveBeenCalled();
  });
});
