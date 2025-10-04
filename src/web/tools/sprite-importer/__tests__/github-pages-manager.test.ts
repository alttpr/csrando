import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import simpleGit from "simple-git";
import fs from "fs-extra";
import path from "path";
import { GitHubPagesManager } from "../github-pages-manager";

// Mock simple-git
// We need to be able to mock the return of the default export, and then chainable methods
const mockGitInstance = {
  clone: vi.fn().mockResolvedValue(undefined),
  pull: vi.fn().mockResolvedValue(undefined),
  checkoutLocalBranch: vi.fn().mockResolvedValue(undefined),
  add: vi.fn().mockResolvedValue(undefined),
  commit: vi.fn().mockResolvedValue(undefined),
  push: vi.fn().mockResolvedValue(undefined),
  status: vi.fn().mockResolvedValue({ files: [] }), // Default to no changes
};

vi.mock("simple-git", () => ({
  // Default export is a function that returns a mockable SimpleGit instance
  default: vi.fn(() => mockGitInstance),
}));

// Mock fs-extra
vi.mock("fs-extra", async (importOriginal) => {
  const originalFs = (await importOriginal()) as typeof fs;
  const mocked = {
    pathExists: vi.fn(originalFs.pathExists.bind(originalFs)),
    ensureDir: vi.fn(async (p: string) => originalFs.ensureDir(p)),
    remove: vi.fn(async (p: string) => originalFs.remove(p)),
    writeJson: vi.fn(async (...args: any[]) =>
      (originalFs as any).writeJson(...args),
    ),
    readJson: vi.fn(async (...args: any[]) =>
      (originalFs as any).readJson(...args),
    ),
    existsSync: vi.fn(originalFs.existsSync.bind(originalFs)),
    readdir: vi.fn(originalFs.readdir.bind(originalFs)),
    writeFile: vi.fn(originalFs.writeFile.bind(originalFs)),
    readFile: vi.fn(originalFs.readFile.bind(originalFs)),
  } satisfies Partial<typeof fs>;
  const merged = { ...originalFs, ...mocked } as typeof fs;
  return {
    __esModule: true,
    ...merged,
    default: merged,
  } as unknown as typeof fs & {
    default: typeof fs;
  };
});

describe("GitHubPagesManager", () => {
  const testRepoUrl = "https://github.com/user/test-repo.git";
  const testLocalPath = "/tmp/test-repo-clone";
  let originalEnv: NodeJS.ProcessEnv;

  beforeEach(() => {
    vi.resetAllMocks();
    originalEnv = { ...process.env }; // Store original environment
    // Default mocks for fs-extra
    (fs.pathExists as any).mockResolvedValue(false); // Default to path not existing
    (fs.ensureDir as any).mockResolvedValue(undefined);
    // Restore simpleGit default to return the top-level mock instance unless overridden per-test
    (simpleGit as any).mockImplementation(() => mockGitInstance);
  });

  afterEach(() => {
    process.env = originalEnv; // Restore original environment
  });

  describe("Constructor", () => {
    it("should construct remoteUrlWithPat when PAT is provided via environment variable for HTTPS URL", () => {
      process.env.SPRITE_IMPORTER_GITHUB_PAT = "test_pat_123";
      const manager = new GitHubPagesManager(testRepoUrl, testLocalPath); // no branch -> no pull
      // Accessing private member for test, or add a getter if preferred
      expect((manager as any).remoteUrlWithPat).toBe(
        "https://test_pat_123@github.com/user/test-repo.git",
      );
    });

    it("should use original URL if PAT is provided but URL is not HTTPS", () => {
      process.env.SPRITE_IMPORTER_GITHUB_PAT = "test_pat_123";
      const sshRepoUrl = "git@github.com:user/test-repo.git";
      const consoleWarnSpy = vi
        .spyOn(console, "warn")
        .mockImplementation(() => {});
      const manager = new GitHubPagesManager(sshRepoUrl, testLocalPath);
      expect((manager as any).remoteUrlWithPat).toBe(sshRepoUrl);
      expect(consoleWarnSpy).toHaveBeenCalledWith(
        expect.stringContaining(
          "GitHub PAT provided, but the repository URL is not HTTPS",
        ),
      );
      consoleWarnSpy.mockRestore();
    });

    it("should use original URL if PAT is not provided", () => {
      delete process.env.SPRITE_IMPORTER_GITHUB_PAT;
      const manager = new GitHubPagesManager(
        testRepoUrl,
        testLocalPath,
        "main",
      );
      expect((manager as any).remoteUrlWithPat).toBe(testRepoUrl);
    });

    it("should warn if PAT is missing (implicitly via getGitHubPat call)", () => {
      delete process.env.SPRITE_IMPORTER_GITHUB_PAT;
      const consoleWarnSpy = vi
        .spyOn(console, "warn")
        .mockImplementation(() => {});
      new GitHubPagesManager(testRepoUrl, testLocalPath); // Constructor calls getGitHubPat
      expect(consoleWarnSpy).toHaveBeenCalledWith(
        expect.stringContaining(
          "SPRITE_IMPORTER_GITHUB_PAT environment variable is not set",
        ),
      );
      consoleWarnSpy.mockRestore();
    });

    it("should initialize a git instance (lazy baseDir setup occurs on operations)", () => {
      new GitHubPagesManager(testRepoUrl, testLocalPath);
      expect(simpleGit).toHaveBeenCalled();
    });
  });

  describe("cloneOrUpdateRepo", () => {
    it("Scenario 1: New Clone - should clone if repo does not exist", async () => {
      (fs.pathExists as any).mockResolvedValue(false); // .git does not exist
      const manager = new GitHubPagesManager(
        testRepoUrl,
        testLocalPath,
        "main",
      );
      process.env.SPRITE_IMPORTER_GITHUB_PAT = "test_pat_123"; // Ensure PAT for cloning
      (manager as any).remoteUrlWithPat =
        "https://test_pat_123@github.com/user/test-repo.git"; // Simulate PAT being set

      await manager.cloneOrUpdateRepo();

      expect(fs.ensureDir).toHaveBeenCalledWith(path.resolve(testLocalPath));
      expect(simpleGit().clone).toHaveBeenCalledWith(
        // simpleGit() returns the top-level mockGitInstance for clone
        "https://test_pat_123@github.com/user/test-repo.git",
        path.resolve(testLocalPath),
      );
      expect(simpleGit().pull).not.toHaveBeenCalled();
    });

    it("Scenario 2: Existing Repo (Update) - should pull if repo exists", async () => {
      (fs.pathExists as any).mockImplementation(
        async (p: string) =>
          p === path.join(path.resolve(testLocalPath), ".git"),
      ); // .git exists

      const manager = new GitHubPagesManager(
        testRepoUrl,
        testLocalPath,
        "main",
      );
      // We need to mock simpleGit(this.localPath).pull()
      // The simpleGit mock needs to return a different instance for the repo-specific git ops
      const localGitMock = {
        checkoutLocalBranch: vi.fn().mockResolvedValue(undefined),
        pull: vi.fn().mockResolvedValue(undefined),
      };
      (simpleGit as any).mockImplementation((optionsOrPath: any) => {
        if (optionsOrPath === path.resolve(testLocalPath)) {
          return localGitMock;
        }
        return mockGitInstance; // Default instance for clone
      });

      await manager.cloneOrUpdateRepo();

      expect(fs.ensureDir).toHaveBeenCalledWith(path.resolve(testLocalPath));
      expect(localGitMock.pull).toHaveBeenCalled();
      expect(mockGitInstance.clone).not.toHaveBeenCalled();
    });

    it("Scenario 3: Clone Fails (PAT missing for private repo)", async () => {
      (fs.pathExists as any).mockResolvedValue(false); // .git does not exist
      delete process.env.SPRITE_IMPORTER_GITHUB_PAT;
      const manager = new GitHubPagesManager(
        "https://github.com/private/repo.git",
        testLocalPath,
      );
      // Simulate remoteUrlWithPat becoming the original URL due to missing PAT
      (manager as any).remoteUrlWithPat = "https://github.com/private/repo.git";

      // Mock clone to fail
      const cloneError = new Error("Authentication failed for private repo");
      (simpleGit().clone as any).mockRejectedValue(cloneError);
      const consoleErrorSpy = vi
        .spyOn(console, "error")
        .mockImplementation(() => {});

      await expect(manager.cloneOrUpdateRepo()).rejects.toThrow(cloneError);

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        expect.stringContaining(
          "Error during clone/update of repository: Authentication failed for private repo",
        ),
      );
      consoleErrorSpy.mockRestore();
    });

    it("Scenario 4: Pull Fails", async () => {
      (fs.pathExists as any).mockImplementation(
        async (p: string) =>
          p === path.join(path.resolve(testLocalPath), ".git"),
      );
      const manager = new GitHubPagesManager(
        testRepoUrl,
        testLocalPath,
        "main",
      );

      const pullError = new Error("Pull failed due to conflict");
      const localGitMock = {
        checkoutLocalBranch: vi.fn().mockResolvedValue(undefined),
        pull: vi.fn().mockRejectedValue(pullError),
      };
      (simpleGit as any).mockImplementation((optionsOrPath: any) => {
        if (optionsOrPath === path.resolve(testLocalPath)) return localGitMock;
        return mockGitInstance;
      });
      const consoleErrorSpy = vi
        .spyOn(console, "error")
        .mockImplementation(() => {});

      await expect(manager.cloneOrUpdateRepo()).rejects.toThrow(pullError);
      expect(localGitMock.pull).toHaveBeenCalled();
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        expect.stringContaining(
          "Error during clone/update of repository: Pull failed due to conflict",
        ),
      );
      consoleErrorSpy.mockRestore();
    });
  });

  describe("commitChanges", () => {
    it("should add all and commit when changes exist", async () => {
      const manager = new GitHubPagesManager(
        testRepoUrl,
        testLocalPath,
        "main",
      );
      const localGitMock = {
        status: vi.fn().mockResolvedValue({
          files: [{ path: "file.txt", working_dir: "M" }],
        }),
        add: vi.fn().mockResolvedValue(undefined),
        commit: vi.fn().mockResolvedValue({ commit: "sha1hash" }),
      };
      (simpleGit as any).mockReturnValue(localGitMock); // All calls to simpleGit(path) get this

      await manager.commitChanges("Test commit message");
      expect(localGitMock.status).toHaveBeenCalled();
      expect(localGitMock.add).toHaveBeenCalledWith("./*");
      expect(localGitMock.commit).toHaveBeenCalledWith("Test commit message");
    });

    it("should not commit when no changes exist", async () => {
      const manager = new GitHubPagesManager(
        testRepoUrl,
        testLocalPath,
        "main",
      );
      const localGitMock = {
        status: vi.fn().mockResolvedValue({ files: [] }), // No changes
        add: vi.fn(),
        commit: vi.fn(),
      };
      (simpleGit as any).mockReturnValue(localGitMock);

      await manager.commitChanges("Test commit message");
      expect(localGitMock.status).toHaveBeenCalled();
      expect(localGitMock.add).not.toHaveBeenCalled();
      expect(localGitMock.commit).not.toHaveBeenCalled();
    });

    it("should handle commit failure", async () => {
      const manager = new GitHubPagesManager(testRepoUrl, testLocalPath);
      const commitError = new Error("Commit failed");
      const localGitMock = {
        status: vi.fn().mockResolvedValue({
          files: [{ path: "file.txt", working_dir: "M" }],
        }),
        add: vi.fn().mockResolvedValue(undefined),
        commit: vi.fn().mockRejectedValue(commitError),
      };
      (simpleGit as any).mockReturnValue(localGitMock);
      const consoleErrorSpy = vi
        .spyOn(console, "error")
        .mockImplementation(() => {});

      await expect(manager.commitChanges("Test commit")).rejects.toThrow(
        commitError,
      );
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        "Error committing changes: Commit failed",
      );
      consoleErrorSpy.mockRestore();
    });
  });

  describe("pushChanges", () => {
    it("should call git.push", async () => {
      const manager = new GitHubPagesManager(testRepoUrl, testLocalPath);
      const localGitMock = { push: vi.fn().mockResolvedValue(undefined) };
      (simpleGit as any).mockReturnValue(localGitMock);

      await manager.pushChanges();
      expect(localGitMock.push).toHaveBeenCalled();
    });

    it("should handle push failure and log auth advice for auth errors", async () => {
      const manager = new GitHubPagesManager(testRepoUrl, testLocalPath);
      const pushError = new Error("Push failed: authentication failed 403");
      const localGitMock = { push: vi.fn().mockRejectedValue(pushError) };
      (simpleGit as any).mockReturnValue(localGitMock);
      const consoleErrorSpy = vi
        .spyOn(console, "error")
        .mockImplementation(() => {});

      await expect(manager.pushChanges()).rejects.toThrow(pushError);
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        "Error pushing changes: Push failed: authentication failed 403",
      );
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        expect.stringContaining(
          "Authentication failed. If using PAT, ensure it's correct",
        ),
      );
      consoleErrorSpy.mockRestore();
    });
  });

  describe("getFilePath", () => {
    it("should return the correct joined path", () => {
      const manager = new GitHubPagesManager(testRepoUrl, testLocalPath);
      const relative = "some/file.json";
      const expectedPath = path.join(path.resolve(testLocalPath), relative);
      expect(manager.getFilePath(relative)).toBe(expectedPath);
    });
  });

  // Test for _ensureLocalPathExists (though private, its effect is testable via cloneOrUpdateRepo)
  describe("_ensureLocalPathExists", () => {
    it("should call fs.ensureDir with the local path", async () => {
      // This private method is called by cloneOrUpdateRepo.
      // We've already tested fs.ensureDir is called in cloneOrUpdateRepo tests.
      // A direct test would require making it public or using a spy if possible.
      // For now, its correct functioning is implicitly tested.
      const manager = new GitHubPagesManager(testRepoUrl, testLocalPath);
      await manager.cloneOrUpdateRepo(); // This calls _ensureLocalPathExists
      expect(fs.ensureDir).toHaveBeenCalledWith(path.resolve(testLocalPath));
    });

    it("should throw if fs.ensureDir fails", async () => {
      const ensureDirError = new Error("Failed to ensure directory");
      (fs.ensureDir as any).mockRejectedValue(ensureDirError);
      const manager = new GitHubPagesManager(testRepoUrl, testLocalPath);
      const consoleErrorSpy = vi
        .spyOn(console, "error")
        .mockImplementation(() => {});

      await expect(manager.cloneOrUpdateRepo()).rejects.toThrow(ensureDirError);
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        expect.stringContaining(
          `Error ensuring local path ${path.resolve(testLocalPath)} exists: Failed to ensure directory`,
        ),
      );
      consoleErrorSpy.mockRestore();
    });
  });
});
