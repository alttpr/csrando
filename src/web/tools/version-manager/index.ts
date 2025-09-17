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
  normalizeRandomizerId,
  sanitizeBaseUrl,
} from "./utils";

export interface CreateArgs {
  version: string;
  tag?: string;
  randomizer?: string;
  patch?: string;
  ips?: string;
  patchDir?: string;
  ipsDir?: string;
  patchMap?: string;
  ipsMap?: string;
  metadata?: string;
  metadataDir?: string;
  metadataMap?: string;
  metadataUrl?: string;
  backend?: string;
  activate?: boolean;
  dryRun?: boolean;
}

function describeRandomizer(id: string | null): string {
  return id ? id : "<none>";
}

export async function main(argvFromNode: string[] = hideBin(process.argv)) {
  const parser = yargs(argvFromNode)
    .scriptName("rando:version:create")
    .usage("$0 --version <tag> --patch <path> [options]")
    .version(false)
    .option("version", {
      type: "string",
      demandOption: true,
      description: "Base version label (e.g. v2025-09-11)",
    })
    .option("tag", {
      type: "string",
      description:
        "Override the stored versionTag. Defaults to <version>-<randomizer>.",
    })
    .option("randomizer", {
      alias: ["game"],
      type: "string",
      description: "Randomizer/game identifier (e.g. alttpr).",
    })
    .option("patch", {
      alias: ["ips"],
      type: "string",
      description:
        "Path to the IPS or BPS base patch. Provide this or --patchDir/--patchMap with --randomizer.",
    })
    .option("patchDir", {
      alias: ["ipsDir"],
      type: "string",
      description:
        "Directory containing <randomizer>.bps or <randomizer>.ips files. Defaults to ./static if not supplied.",
    })
    .option("patchMap", {
      alias: ["ipsMap"],
      type: "string",
      description: "JSON file mapping randomizer ids to base patch paths.",
    })
    .option("metadata", {
      type: "string",
      description: "Path to metadata JSON file.",
    })
    .option("metadataDir", {
      type: "string",
      description: "Directory containing <randomizer>.json metadata snapshots.",
    })
    .option("metadataMap", {
      type: "string",
      description: "JSON file mapping randomizer ids to metadata paths.",
    })
    .option("metadataUrl", {
      type: "string",
      description: "Full metadata URL to fetch.",
    })
    .option("backend", {
      type: "string",
      description:
        "Backend base URL used when fetching metadata (defaults to PRIVATE_DOTNET_API_BASE_URL).",
    })
    .option("activate", {
      alias: ["active"],
      type: "boolean",
      description: "Mark the created version active for the randomizer.",
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

  const argv = (await parser.parseAsync()) as unknown as CreateArgs &
    Record<string, unknown>;

  const randomizerId = normalizeRandomizerId(argv.randomizer);
  const patchMapPath = argv.patchMap ?? argv.ipsMap;
  const patchMap = patchMapPath ? parsePathMap(patchMapPath) : undefined;
  const metadataMap = argv.metadataMap
    ? parsePathMap(argv.metadataMap)
    : undefined;
  const backend = sanitizeBaseUrl(
    argv.backend || process.env.PRIVATE_DOTNET_API_BASE_URL || undefined,
  );

  const versionTag = computeVersionTag({
    baseVersion: argv.version,
    randomizerId,
    explicitTag: argv.tag,
  });

  const basePatchPath = resolveBasePatchPath({
    explicitPath: argv.patch ?? argv.ips,
    directory: argv.patchDir ?? argv.ipsDir,
    map: patchMap,
    randomizerId,
  });
  const { base64: basePatchBase64, sha256 } = readPatchData(basePatchPath);

  const metadata = await loadMetadata({
    explicitPath: argv.metadata,
    directory: argv.metadataDir,
    map: metadataMap,
    metadataUrl: argv.metadataUrl,
    backendBaseUrl: backend,
    randomizerId,
  });

  const { optionsMetadata, postGenSettings } = toMetadataPair(metadata.data);

  const result = await insertVersion({
    randomizerId,
    versionTag,
    optionsMetadata,
    postGenSettings,
    basePatchBase64,
    patchSha256: sha256,
    activate: argv.activate,
    dryRun: argv.dryRun,
  });

  if (argv.dryRun) {
    console.log(
      `[dryRun] Would create version ${result.versionTag} (${result.versionId}) for ${describeRandomizer(result.randomizerId)} using ${basePatchPath}`,
    );
    console.log(`[dryRun] Patch sha256: ${result.patchSha256}`);
    console.log(`[dryRun] Metadata source: ${metadata.source}`);
    if (argv.activate) {
      console.log(
        `[dryRun] Would mark ${describeRandomizer(result.randomizerId)} active with ${result.versionTag}`,
      );
    }
    return;
  }

  console.log(
    `Created version ${result.versionTag} (${result.versionId}) for ${describeRandomizer(result.randomizerId)}`,
  );
  console.log(`Base patch: ${basePatchPath} (sha256=${result.patchSha256})`);
  console.log(`Metadata source: ${metadata.source}`);
  if (argv.activate) {
    console.log(
      `Activated ${describeRandomizer(result.randomizerId)} -> ${result.versionTag}`,
    );
  }
}

main().catch((err) => {
  console.error("Failed creating randomizer version:", err);
  process.exit(1);
});
