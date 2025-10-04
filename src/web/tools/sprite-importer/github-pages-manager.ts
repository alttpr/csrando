import simpleGit, { SimpleGit } from "simple-git";
import fs from "fs-extra";
import path from "path";

// Function to retrieve GitHub PAT from environment variable
// Replicated here as it's not directly exported from index.ts in a way that's easily importable for this module.
function loadDotEnvIfPresent() {
  try {
    const envPath = path.resolve(process.cwd(), ".env");
    if (fs.existsSync(envPath)) {
      const raw = fs.readFileSync(envPath, "utf8");
      for (const line of raw.split(/\r?\n/)) {
        const m = line.match(/^\s*([A-Z0-9_]+)\s*=\s*(.*)\s*$/i);
        if (!m) continue;
        const key = m[1];
        let val = m[2];
        if (
          (val.startsWith('"') && val.endsWith('"')) ||
          (val.startsWith("'") && val.endsWith("'"))
        ) {
          val = val.slice(1, -1);
        }
        if (!(key in process.env)) (process.env as any)[key] = val;
      }
    }
  } catch {
    // ignore
  }
}

function getGitHubPat(): string | undefined {
  loadDotEnvIfPresent();
  const pat = process.env.SPRITE_IMPORTER_GITHUB_PAT;
  if (!pat) {
    console.warn(
      "Warning: SPRITE_IMPORTER_GITHUB_PAT environment variable is not set. " +
        "GitHub operations requiring authentication will likely fail.",
    );
  }
  return pat;
}

export class GitHubPagesManager {
  private git: SimpleGit;
  private remoteUrl: string;
  private localPath: string;
  private remoteUrlWithPat: string | null = null; // Null if PAT is not available
  private branch?: string;
  private shallowDepth?: number;

  constructor(repoUrl: string, localPath: string, branch?: string) {
    this.remoteUrl = repoUrl;
    this.localPath = path.resolve(localPath); // Ensure localPath is absolute
    this.branch = branch;

    const pat = getGitHubPat();
    if (pat && this.remoteUrl.startsWith("https://")) {
      this.remoteUrlWithPat = this.remoteUrl.replace(
        "https://",
        `https://${pat}@`,
      );
    } else if (pat) {
      // If PAT is provided but URL is not HTTPS (e.g. SSH), simple-git might handle SSH key auth.
      // For explicit PAT usage with HTTPS, the URL format is key.
      console.warn(
        "GitHub PAT provided, but the repository URL is not HTTPS. PAT will not be embedded in the URL.",
      );
      // simple-git might still work if SSH keys are configured and PAT is not strictly needed for this repo.
      // Or, if the repo is public and operations don't require auth.
      this.remoteUrlWithPat = this.remoteUrl; // Use original URL
    } else {
      // No PAT, or URL not HTTPS. Operations requiring auth might fail.
      this.remoteUrlWithPat = this.remoteUrl; // Use original URL
    }

    // Lazily initialize simple-git instances where needed to avoid errors when baseDir doesn't exist yet.
    this.git = simpleGit();
  }

  private async _ensureLocalPathExists() {
    try {
      await fs.ensureDir(this.localPath);
    } catch (err: any) {
      console.error(
        `Error ensuring local path ${this.localPath} exists: ${err.message}`,
      );
      throw err;
    }
  }

  private async ensureBranch(repoGit: SimpleGit): Promise<void> {
    if (!this.branch) return;
    await repoGit.checkoutLocalBranch(this.branch);
    await repoGit.pull("origin", this.branch);
  }

  public async cloneOrUpdateRepo(): Promise<void> {
    await this._ensureLocalPathExists();
    const gitDir = path.join(this.localPath, ".git");
    const isRepo = await fs.pathExists(gitDir);

    try {
      if (!isRepo) {
        console.log(
          `No repository found at ${this.localPath}. Cloning from ${this.remoteUrl}...`,
        );
        if (!this.remoteUrlWithPat) {
          throw new Error(
            "Cannot clone repository: GitHub PAT is missing or URL is not HTTPS, and remoteUrlWithPat was not set.",
          );
        }
        await simpleGit().clone(this.remoteUrlWithPat, this.localPath);
        console.log("Clone successful.");
        // For a fresh clone we intentionally avoid pulling immediately; tests assert no pull occurs.
        // If a target branch (e.g. gh-pages) was specified, ensure we are on it so subsequent commits
        // land on the correct branch. If it does not exist remotely yet, create a new local branch.
        if (this.branch) {
          const repoGit = simpleGit(this.localPath);
          try {
            await repoGit.fetch();
            const branches = await repoGit.branch(["-a"]);
            const remoteBranchName = `remotes/origin/${this.branch}`;
            if (branches.all.includes(remoteBranchName)) {
              await repoGit.checkout(this.branch);
            } else {
              // Create new branch from current HEAD (default branch) – not orphan to keep history unless desired.
              await repoGit.checkoutLocalBranch(this.branch);
              console.log(`Created new local branch ${this.branch}`);
            }
          } catch (e: any) {
            console.warn(
              `Warning: failed to prepare branch ${this.branch}: ${e.message}`,
            );
          }
        }
      } else {
        // Only perform branch checkout/pull for existing repositories (update scenario)
        const repoGit = simpleGit(this.localPath);
        await this.ensureBranch(repoGit);
      }
      this.git = simpleGit(this.localPath); // Reset git instance to point at localPath
    } catch (err: any) {
      console.error(`Error during clone/update of repository: ${err.message}`);
      throw err;
    }
  }

  public async commitChanges(message: string): Promise<void> {
    try {
      // Ensure git instance is operating on the correct directory
      const repoGit = simpleGit(this.localPath);
      const status = await repoGit.status();
      if (status.files.length === 0) {
        console.log("No changes to commit.");
        return;
      }

      await repoGit.add("./*");
      await repoGit.commit(message);
      console.log(`Changes committed with message: "${message}"`);
    } catch (err: any) {
      console.error(`Error committing changes: ${err.message}`);
      throw err;
    }
  }

  public async pushChanges(): Promise<void> {
    try {
      const repoGit = simpleGit(this.localPath);
      console.log("Pushing changes to remote...");
      if (this.branch) {
        await repoGit.push("origin", this.branch);
      } else {
        // Push using whatever the current HEAD/upstream is (default branch)
        await (repoGit as any).push?.();
      }
      console.log("Push successful.");
    } catch (err: any) {
      console.error(`Error pushing changes: ${err.message}`);
      if (
        err.message.includes("authentication") ||
        err.message.includes("403")
      ) {
        console.error(
          "Authentication failed. If using PAT, ensure it's correct and has push permissions.",
        );
      }
      throw err;
    }
  }

  public getFilePath(relativePath: string): string {
    return path.join(this.localPath, relativePath);
  }
}
