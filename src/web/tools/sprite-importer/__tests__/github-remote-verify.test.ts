import { describe, it, expect, vi, beforeEach } from "vitest";
import path from "path";
import fs from "fs-extra";

vi.mock("fs-extra", async (importOriginal) => {
  const original = (await importOriginal()) as typeof fs;
  const mocked: Partial<typeof fs> = {
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

describe("remote verify CLI", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    // Reset exit code between tests
    // @ts-expect-error process exitCode is writable
    process.exitCode = undefined;
  });

  it("passes when all assets exist", async () => {
    (fs.pathExists as unknown as vi.Mock).mockImplementation(
      async (p: string) => {
        if (p.endsWith("sprites.json")) return true;
        return true;
      },
    );
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({
      sprites: [
        {
          value: "LinkSprite",
          name: "Link",
          imagePath: "linksprite.png",
          rdcPath: "linksprite.rdc",
        },
      ],
    });
    await main([
      "remote",
      "verify",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      ".",
    ]);
    expect(process.exitCode).toBeUndefined();
  });

  it("fails when assets are missing", async () => {
    const errSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    (fs.pathExists as unknown as vi.Mock).mockImplementation(
      async (p: string) => {
        if (p.endsWith("sprites.json")) return true;
        // Pretend PNG missing
        if (p.endsWith("linksprite.png")) return false;
        return true;
      },
    );
    (fs.readJson as unknown as vi.Mock).mockResolvedValue({
      sprites: [
        {
          value: "LinkSprite",
          name: "Link",
          imagePath: "linksprite.png",
          rdcPath: "linksprite.rdc",
        },
      ],
    });
    await main([
      "remote",
      "verify",
      "--repo",
      "https://example.com/repo.git",
      "--game",
      "alttp",
      "--sourceDir",
      ".",
    ]);
    const errs = (errSpy as unknown as vi.Mock).mock.calls.flat().join("\n");
    expect(errs).toMatch(/Verification failed/i);
  });
});
