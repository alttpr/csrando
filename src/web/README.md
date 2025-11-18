# Rando-Web: A Game Randomizer Interface

This project provides a web interface for a game randomizer, allowing users to configure randomization options, generate seeds, and manage user accounts.

## Prerequisites

- Node.js 20+
- npm (ships with Node)
- Access to the .NET backend API (the web UI talks to it for randomizer data and seed generation)

## Setup and Installation

1.  **Clone the repository:**

    ```bash
    git clone <repository-url>
    cd <repository-directory>
    ```

2.  **Install dependencies:**

    ```bash
    npm ci
    ```

3.  **Database setup:**
    This project uses SQLite as its database. To set up the initial database schema, run:

    ```bash
    npm run db:migrate
    ```

    This command will apply any pending database migrations. If you need to create new migrations, you might use a command like `npm run db:push` (refer to `package.json` for exact Drizzle ORM commands).

4.  **Environment Variables:**
    Create a `.env` file in the project root (if it doesn’t exist yet) with the required variables:

    ```env
    PRIVATE_DOTNET_API_BASE_URL=http://localhost:5000
    PUBLIC_SPRITES_BASE_URL=/sprites
    DATABASE_URL=sqlite:dev.db
    ```

    Optional extras:

    - `PRIVATE_ADMIN_VERSION_TOKEN` enables the `/admin/new-version` UI for managing randomizer snapshots.
    - `LOCAL_TEST_MODE=true` switches various services (metadata, randomizer calls) to bundled mock data.

## Running the Development Server

To start the SvelteKit development server:

```bash
npm run dev
```

The app is served at `http://localhost:5173` by default.

## Linting, Formatting, and Type Safety

```bash
npm run check       # svelte-check + kit sync
npm run lint        # ESLint
npm run format      # Prettier (write mode)
npm run test:run    # Vitest in CI mode
```

## Building for Production

Create an optimized build with:

```bash
npm run build
```

Artifacts are emitted to `.svelte-kit/` and the adapter target (`build/` for the Node adapter).

## Other Available Scripts

The `package.json` includes additional scripts:

- `npm run preview` – preview the production build locally.
- `npm run db:migrate` / `npm run db:push` – manage Drizzle migrations.
- `npm run db:studio` – open Drizzle Studio for inspecting the SQLite database.
- `npm run import-sprites` – CLI for the remote sprite workflow.
- `npm run user:admin <username>` – set a user's role to admin.
- `npm run user:list` – list all users and their roles.

## User Administration

The project includes a CLI tool for managing user roles. See [tools/user-admin/README.md](./tools/user-admin/README.md) for full documentation.

Quick usage:

```bash
# Set a user as admin
npm run user:admin john_doe

# List all users and their roles
npm run user:list

# Set a user back to regular user role
npm run user:set-role set-user john_doe
```

## Randomizer Versions (Snapshots)

Need to add or hotfix a version without redeploying? With `PRIVATE_ADMIN_VERSION_TOKEN` configured you can visit `/admin/new-version` on a running site, upload the IPS or BPS base patch, and fetch the latest metadata directly from the backend to create a new snapshot. You can optionally record the backend git commit hash and build timestamp to simplify debugging.

Use the CLI to store immutable snapshots of the randomizer (base IPS/BPS patch + metadata) so old seeds remain compatible even after updates.

- Create a snapshot from local files. The tool automatically looks for `./static/<id>.bps` or `./static/<id>.ips` unless `--patch` or `--patchDir` is supplied.

  ```bash
  npm run rando:version:create -- \
    --version v2025-09-11 \
    --randomizer alttpr \
    --metadata ./metadata/alttpr.json \
    --activate
  ```

- Fetch metadata directly from the backend API (preferred). Omit `--backend` to fall back to `PRIVATE_DOTNET_API_BASE_URL`.

  ```bash
  npm run rando:version:create -- \
    --version v2025-09-11 \
    --randomizer alttpr \
    --backend https://your-api.example.com \
    --activate
  ```

  You can also point at a single metadata resource via `--metadataUrl https://api/meta/alttpr`. `--patchMap` / `--metadataMap` accept JSON objects that map ids to paths (relative paths resolve from the map file’s folder). Legacy `--ips*` aliases still work.

When generating seeds, the app links each new seed to the currently active snapshot; patching then uses that snapshot’s base IPS/BPS data and options metadata to ensure compatibility with older seeds.

Batch create snapshots for all games

- Discover supported game IDs from the backend `/meta` index and create one snapshot per game. Base patch paths can be provided via a JSON map or by pointing to a directory that contains `<id>.bps` or `<id>.ips` files. Metadata can likewise be discovered via `--metadataDir`, `--metadataMap`, or fetched from the backend.

  ```bash
  # Using env PRIVATE_DOTNET_API_BASE_URL, patches in ./static/<id>.bps (or .ips)
  npm run rando:version:create:all -- \
    --version v2025-09-11 \
    --activateAll        # activates each created game's snapshot

  # Offline example using local resources for a subset of ids
  npm run rando:version:create:all -- \
    --version v2025-09-11 \
    --randomizers alttpr,z1r,g2r \
    --patchMap ./patch-map.json \
    --metadataDir ./metadata \
    --dryRun
  ```

Notes

- Each row’s `versionTag` defaults to `<version>-<gameId>`; provide `--tag` (single) or ensure your base version already contains the suffix to override.
- Only one snapshot can be active per randomizer; use `--activate`, `--activateAll`, or `--setActive <id>` to control which ones should be marked live.
- Snapshots now store `randomizerId` (e.g., `alttpr`, `z1r`) to indicate which game the version targets.
- Active versions are enforced per randomizer: tools only deactivate other versions with the same `randomizerId`, and seed generation picks the active version for the inferred game id in the request.
- Provide `--commit <hash>` (7-40 hex) to capture the backend git commit and `--buildDate <iso|now>` to stamp the build timestamp (defaults to the current time).
- Append `--dryRun` to preview the operations without touching the database.

Refer to the `package.json` for a full list and their specific functions.

## Manual Validation Checklist

Before opening a PR or deploying, run the suite:

1. `npm run build`
2. `npm run test:run`
3. `npm run check`
4. `npm run lint`
5. `npm run format`
6. Launch `npm run dev`, visit `http://localhost:5173`, verify the landing page, and navigate to the config flow (expect the “Failed to communicate with backend” banner if the .NET API isn’t running).

## Sprite Importer: Remote Workflow

The sprite tools include a CLI to manage sprite collections hosted in a GitHub Pages repo. Paths in `sprites.json` are relative to the game directory (e.g., `<repo>/alttp/sprites.json`).

- Prereqs: set `SPRITE_IMPORTER_GITHUB_PAT` with push permissions; use HTTPS repo URLs.
- List sprites:
  - `npm run import-sprites -- remote list --repo https://github.com/user/gh-pages.git --game alttp`
- Verify assets exist:
  - `npm run import-sprites -- remote verify --repo <url> --game alttp`
- Add or update a sprite:
  - `npm run import-sprites -- remote add --repo <url> --game alttp --spriteName MySprite --sourceRdc ./out/my.rdc --sourcePng ./out/my.png`
  - If no PNG provided, the tool attempts to render one for ALttP/SM RDCs.
- Bulk add/update from a directory (single commit):
  - `npm run import-sprites -- remote bulk-add --repo <url> --game alttp --sourceDir ./out/rdc`
- Remove a sprite:
  - `npm run import-sprites -- remote remove --repo <url> --game alttp --spriteName MySprite`
- Safe preview (no changes):
  - Add/update: append `--dryRun` to see actions without writing or pushing.
  - Remove: append `--dryRun` to see which files and JSON entries would be removed.
- Branch: use `--branch gh-pages` (default) to target a specific branch.

Frontend integration

- Set `PUBLIC_SPRITES_BASE_URL` (Vite/SvelteKit public env) to the base URL where the sprite repo is hosted (default `/sprites`).
- The app fetches `<base>/<game>/sprites.json` and binary files referenced there for patching.

## Contributor Guide

For coding conventions, repository layout, and agent-focused workflows (including sprite tools), see AGENTS.md.
For developer setup and self-hosted deployment instructions, see:

- docs/DEVELOPMENT.md
- docs/DEPLOYMENT.md

## Continuous Integration & Release

- CI runs on push/PR (see `.github/workflows/ci.yml`):
  - Installs deps, lints, type-checks, runs tests, and builds.
  - Uploads build artifacts for inspection.
- Manual release build (see `.github/workflows/release.yml`):
  - Trigger via GitHub Actions “Release Build” to produce artifacts you can deploy.
- Automated dependency updates via Dependabot (`.github/dependabot.yml`).

## Self-Hosting (Docker Compose + Traefik)

This repo includes a turn-key Docker setup for self-hosting with automatic TLS and routing.

1. Configure environment

- Create a `.env` file in the root of the project and set the following variables:
  - `DOMAIN` (e.g., `example.com`)
  - `TRAEFIK_ACME_EMAIL` (for Let’s Encrypt)
  - `API_IMAGE` (Docker image for your C# API)

2. Build and run

```bash
docker compose up -d
```

Traefik will obtain TLS certificates automatically and route:

- `https://DOMAIN/` → SvelteKit frontend
- `https://DOMAIN/api` → .NET API (prefix `/api` is stripped before the API)

3. Frontend image (optional)

- A GitHub Action builds and pushes a frontend image to GHCR (`ghcr.io/<owner>/rando-web-frontend`).
- You can point a remote Compose stack at that image (instead of building locally) for seamless updates + Watchtower.
