#!/usr/bin/env node
import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import {
  computeVersionTag,
  parsePathMap,
  resolveBasePatchPath,
  readPatchData,
  loadMetadata,
  toMetadataPair,
  insertVersion,
  sanitizeBaseUrl,
  normalizeRandomizerId,
} from "./utils";

interface BatchArgs {
  version: string;
  backend?: string;
  randomizers?: string;
  ids?: string;
  patchDir?: string;
  ipsDir?: string;
  patchMap?: string;
  ipsMap?: string;
  metadataDir?: string;
  metadataMap?: string;
  metadataUrlTemplate?: string;
  dryRun?: boolean;
  setActive?: string;
  activateAll?: boolean;
  commit?: string;
  buildDate?: string;
}

interface MetaIndexEntry {
  randomizer?: string;
}

function parseList(value?: string): string[] {
  if (!value) return [];
  return value
    .split(",")
    .map((s) => s.trim())
    .filter((s) => s.length > 0);
}

export async function main(argvFromNode: string[] = hideBin(process.argv)) {
  const parser = yargs(argvFromNode)
    .scriptName("rando:version:create:all")
    .usage("$0 --version <tag> [options]")
    .version(false)
    .option("version", {
      type: "string",
      demandOption: true,
      description: "Base version label shared by all snapshots.",
    })
    .option("backend", {
      type: "string",
      description:
        "Backend base URL for metadata (defaults to PRIVATE_DOTNET_API_BASE_URL).",
    })
    .option("randomizers", {
      alias: ["ids"],
      type: "string",
      description:
        "Comma-separated list of randomizer ids to process. If omitted, fetch from backend /meta.",
    })
    .option("patchDir", {
      alias: ["ipsDir"],
      type: "string",
      description:
        "Directory containing <randomizer>.bps or <randomizer>.ips files (defaults to ./static).",
    })
    .option("patchMap", {
      alias: ["ipsMap"],
      type: "string",
      description: "JSON map of randomizer ids to base patch paths.",
    })
    .option("metadataDir", {
      type: "string",
      description: "Directory with <randomizer>.json metadata files.",
    })
    .option("metadataMap", {
      type: "string",
      description: "JSON map of randomizer ids to metadata file paths.",
    })
    .option("metadataUrlTemplate", {
      type: "string",
      description: "Optional template URL (use {id}) when fetching metadata.",
    })
    .option("commit", {
      type: "string",
      description:
        "Git commit hash for the backend build associated with all created versions.",
    })
    .option("buildDate", {
      type: "string",
      description:
        "Build date/time for the backend artifact (ISO 8601 or 'now'). Defaults to now.",
    })
    .option("setActive", {
      type: "string",
      description:
        "Comma-separated ids (or 'all') to mark active after creation.",
    })
    .option("activateAll", {
      type: "boolean",
      description: "Shortcut for --setActive all.",
      default: false,
    })
    .option("dryRun", {
      type: "boolean",
      description: "Show actions without inserting into the database.",
      default: false,
    })
    .help()
    .alias("h", "help")
    .strict();

  const argv = (await parser.parseAsync()) as unknown as BatchArgs &
    Record<string, unknown>;

  const backend = sanitizeBaseUrl(
    argv.backend || process.env.PRIVATE_DOTNET_API_BASE_URL || undefined,
  );
  const patchMapPath = argv.patchMap ?? argv.ipsMap;
  const basePatchMap = patchMapPath ? parsePathMap(patchMapPath) : undefined;
  const metadataMap = argv.metadataMap
    ? parsePathMap(argv.metadataMap)
    : undefined;

  const requestedIds = parseList(argv.randomizers || argv.ids)
    .map((id) => normalizeRandomizerId(id))
    .filter((id): id is string => Boolean(id));

  let ids: string[] = requestedIds;
  if (ids.length === 0) {
    if (!backend) {
      throw new Error(
        "No randomizers provided. Supply --randomizers or configure --backend / PRIVATE_DOTNET_API_BASE_URL.",
      );
    }
    const indexUrl = `${backend}/meta`;
    const res = await fetch(indexUrl);
    if (!res.ok) {
      throw new Error(`Failed to fetch ${indexUrl}: HTTP ${res.status}`);
    }
    const index = (await res.json()) as MetaIndexEntry[];
    ids = index
      .map((entry) => normalizeRandomizerId(entry.randomizer))
      .filter((id): id is string => Boolean(id));
  }

  if (ids.length === 0) {
    throw new Error("No randomizer ids resolved; nothing to do.");
  }

  console.log(`[batch] Processing ids: ${ids.join(", ")}`);

  const activateArg = argv.setActive?.trim();
  const activateAll = argv.activateAll || activateArg?.toLowerCase() === "all";
  const activateTargets = new Set<string>();
  if (!activateAll && activateArg) {
    for (const id of parseList(activateArg)) {
      const normalized = normalizeRandomizerId(id);
      if (normalized) activateTargets.add(normalized);
    }
  }

  const created: {
    id: string;
    versionId: string;
    versionTag: string;
    sha: string;
    patchPath: string;
    metadataSource: string;
    activated: boolean;
    gitCommitHash: string | null;
    buildDate: string;
  }[] = [];
  const failures: { id: string; error: unknown }[] = [];

  for (const id of ids) {
    try {
      const tag = computeVersionTag({
        baseVersion: argv.version,
        randomizerId: id,
      });
      const metadataUrl = argv.metadataUrlTemplate
        ? argv.metadataUrlTemplate.replace("{id}", id)
        : undefined;

      const basePatchPath = resolveBasePatchPath({
        explicitPath: undefined,
        directory: argv.patchDir ?? argv.ipsDir,
        map: basePatchMap,
        randomizerId: id,
      });

      const { base64: basePatchBase64, sha256 } = readPatchData(basePatchPath);
      const metadata = await loadMetadata({
        explicitPath: undefined,
        directory: argv.metadataDir,
        map: metadataMap,
        metadataUrl,
        backendBaseUrl: backend,
        randomizerId: id,
      });

      const { optionsMetadata, postGenSettings } = toMetadataPair(
        metadata.data,
      );
      const activate = activateAll || activateTargets.has(id);
      const result = await insertVersion({
        randomizerId: id,
        versionTag: tag,
        optionsMetadata,
        postGenSettings,
        basePatchBase64,
        patchSha256: sha256,
        activate,
        dryRun: argv.dryRun,
        gitCommitHash: argv.commit,
        buildDate: argv.buildDate,
      });

      if (argv.dryRun) {
        console.log(
          `[dryRun] Would create ${tag} (${result.versionId}) for ${id} from ${basePatchPath}`,
        );
        console.log(`[dryRun] Metadata source: ${metadata.source}`);
        if (result.gitCommitHash) {
          console.log(`[dryRun] Git commit: ${result.gitCommitHash}`);
        }
        console.log(`[dryRun] Build date: ${result.buildDate}`);
        if (activate) {
          console.log(`[dryRun] Would mark ${id} active with ${tag}`);
        }
      } else {
        console.log(
          `[batch] Created ${tag} (${result.versionId}) for ${id} sha=${result.patchSha256}`,
        );
        if (result.gitCommitHash) {
          console.log(`[batch] Git commit: ${result.gitCommitHash}`);
        }
        console.log(`[batch] Build date: ${result.buildDate}`);
        console.log(`[batch] Metadata source: ${metadata.source}`);
        if (activate) {
          console.log(`[batch] Activated ${id} -> ${tag}`);
        }
      }

      created.push({
        id,
        versionId: result.versionId,
        versionTag: tag,
        sha: result.patchSha256,
        patchPath: basePatchPath,
        metadataSource: metadata.source,
        activated: activate && !argv.dryRun,
        gitCommitHash: result.gitCommitHash,
        buildDate: result.buildDate,
      });
    } catch (err) {
      console.error(`[batch] Failed to create version for ${id}:`, err);
      failures.push({ id, error: err });
    }
  }

  console.log(
    `[batch] Completed. ${created.length} succeeded${argv.dryRun ? " (dry run)" : ""}.`,
  );
  if (failures.length > 0) {
    console.error(`[batch] ${failures.length} randomizer(s) failed.`);
    throw new Error("Batch version creation finished with errors.");
  }
}

main().catch((err) => {
  console.error("Batch version creation failed:", err);
  process.exit(1);
});
