# Development Setup

This guide covers running the app locally, running tests, and using the sprite tools.

## Prerequisites

- Node.js 20+
- npm
- Optional: running .NET API for real metadata/seed generation (otherwise set `LOCAL_TEST_MODE=true`)

## Install and Run

1. Install dependencies

```bash
npm ci
```

2. Configure environment variables (create `.env` if missing)

```env
PRIVATE_DOTNET_API_BASE_URL=http://localhost:5000
PUBLIC_SPRITES_BASE_URL=/sprites
DATABASE_URL=sqlite:dev.db
```

3. Type-check, lint, test

```bash
npm run check
npm run lint
npm run test:run
```

4. Start the dev server

```bash
npm run dev
```

The app runs on http://localhost:5173 by default. If your backend API is local, configure your API base URL via env (e.g., a `.env` with your API URL) or a proxy rule in Vite if needed.

## Database

This project uses Drizzle with SQLite in development.

```bash
# Apply pending migrations
npm run db:migrate

# Open Drizzle Studio
npm run db:studio
```

## Manual Validation (pre-PR checklist)

1. `npm run build`
2. `npm run test:run`
3. `npm run check`
4. `npm run lint`
5. `npm run format`
6. Launch `npm run dev` and smoke test the UI (landing page + configuration flow)

## Tests

Use Vitest for unit/integration tests.

```bash
npm run test        # watch mode
npm run test:run    # run once (CI)
npm run coverage    # with coverage
```

## Sprite Tools

Sprite CLI lives under `tools/sprite-importer/` (run via `npm run import-sprites -- ...`).

```bash
npm run import-sprites -- --game alttp --sourceDir ./path/to/zspr_or_rdc --outputDir ./static/sprites
```

```bash
# List remote sprites for a game
npm run import-sprites -- remote list --repo https://github.com/user/gh-pages.git --game alttp --sourceDir .

# Safe preview (no changes)
npm run import-sprites -- remote add --repo <url> --game alttp --spriteName Test --sourceRdc ./out/test.rdc --dryRun --sourceDir .

### Sprite Atlas (Remote Spritesheet) Workflow

Sprites (preview PNG + RDC + patch files) are **not** bundled into the app. They live in a separate GitHub Pages repository referenced by `PUBLIC_SPRITES_BASE_URL` (see `.env`).

We now support building a per-game spritesheet atlas remotely to reduce network requests and speed up the sprite selector.

#### Key Artifacts (per game directory in remote repo)

- `sprites.json` – authoritative list of sprites and their preview PNG / patch details
- `sheet-<game>-<hash>.png` – packed spritesheet image (content-hash in filename)
- `sheet-<game>-<hash>.json` – atlas manifest with coordinates and metadata
- `sheet-<game>-latest.png` / `sheet-<game>-latest.json` – copies pointing to most recent build for easy consumption

Older hashed sheets are pruned automatically (keeping the most recent N, default 2) to keep the repo tidy.

#### Building Atlases

Run via the existing importer CLI using the `remote atlas` subcommand (it clones/updates the remote repo, writes atlas artifacts, commits & pushes):

```

npm run import-sprites -- remote atlas --repo https://github.com/<owner>/<repo> --game alttp

```

Options:

| Flag | Description | Default |
|------|-------------|---------|
| `--game <id>` | Specific game (`alttp`, `sm`, `zelda1`, `metroid1`) | required unless `--all` |
| `--all` | Build atlases for all game folders containing `sprites.json` | false |
| `--maxWidth <px>` | Maximum sheet width; grid wraps after this | 2048 |
| `--keep <n>` | Number of historical sheet versions (hash variants) to keep | 2 |
| `--dryRun` | Compute and log without writing or committing | false |

Examples:

```

# Dry run every game

npm run import-sprites -- remote atlas --repo https://github.com/<owner>/<repo> --all --dryRun

# Custom width & keep more history

npm run import-sprites -- remote atlas --repo https://github.com/<owner>/<repo> --game sm --maxWidth 1024 --keep 4

````

#### Atlas JSON Structure

`sheet-<game>-<hash>.json` example (trimmed):

```json
{
	"version": 1,
	"game": "alttp",
	"hash": "a1b2c3d4e5f6g7h8",
	"generatedAt": "2025-09-02T10:15:00.000Z",
	"sheet": { "image": "sheet-alttp-a1b2c3d4e5f6g7h8.png", "width": 1024, "height": 768 },
	"cell": { "w": 16, "h": 24 },
	"sprites": [
		{ "value": "link_classic", "name": "Classic Link", "author": "Nintendo", "x": 0, "y": 0, "w": 16, "h": 24, "originalImagePath": "link_classic.png" }
	],
	"byValue": {
		"link_classic": { "x": 0, "y": 0, "w": 16, "h": 24, "i": 0 }
	}
}
````

#### Front-End Consumption

`PUBLIC_SPRITES_BASE_URL` (in `.env`) is the absolute base to the remote repository root (e.g. `https://tewtal.github.io/quad-sprites`).

For a given game `alttp`, load the latest atlas manifest:

```ts
const base = PUBLIC_SPRITES_BASE_URL.replace(/\/$/, "");
const res = await fetch(`${base}/alttp/sheet-alttp-latest.json`, {
  cache: "no-cache",
});
if (res.ok) {
  const atlas = await res.json();
  // Use atlas.sheet.image for the PNG, atlas.byValue[value] for coordinates.
}
```

To render a sprite as a div background (simplified):

```ts
function styleForSprite(atlas, value) {
  const entry = atlas.byValue[value];
  if (!entry) return {};
  return {
    width: `${entry.w}px`,
    height: `${entry.h}px`,
    backgroundImage: `url(${base}/alttp/${atlas.sheet.image})`,
    backgroundPosition: `-${entry.x}px -${entry.y}px`,
    backgroundRepeat: "no-repeat",
    imageRendering: "pixelated",
    backgroundSize: `${atlas.sheet.width}px ${atlas.sheet.height}px`,
  };
}
```

Fallback strategy (if atlas not found): continue using individual PNGs at `${base}/<game>/<sprite>.png`.

#### Using the `<SpriteIcon>` Component

The component automatically attempts to use the atlas (lazy-loading on hover/open) and falls back to the individual PNG if the atlas is missing or the sprite value is not found.

Example:

```svelte
<script lang="ts">
	import SpriteIcon from '$lib/components/ui/SpriteIcon.svelte';
	export let game = 'alttp';
	export let spriteValue = 'link_classic';
	// imagePath is the direct PNG (as provided in normalized sprite config) for fallback.
	export let imagePath = 'https://example.github.io/quad-sprites/alttp/link_classic.png';
</script>

<SpriteIcon {game} value={spriteValue} {imagePath} size={32} />
```

Inside selection UIs, the project now uses `<SpriteIcon>` (see `SpriteSelect.svelte`) instead of `<img>` tags.

#### Error Handling & Validation

- Build aborts if any sprite PNG has mismatched dimensions.
- Missing PNG referenced in `sprites.json` throws (future: could add `--allowMissing`).
- Dry run prints intended sheet size & sprite count.

#### Future Enhancements

- Multi-sheet support if counts exceed a single sheet's dimension constraints.
- WebP / AVIF alternative formats.
- Incremental updates (currently always full rebuild).
- Aggregated manifest across games for one-shot fetch.

#### CI Integration (Example Idea)

Add a scheduled workflow in the remote repo to rebuild atlases nightly or on new sprite commits by invoking the importer with `remote atlas --all`.

```

```

## Additional Notes

- Sprites Base URL: use `getPublicSpritesBaseUrl()` from `src/lib/env.ts` in client code instead of reading envs directly. The layout injects `window.__PUBLIC_SPRITES_BASE_URL__` at runtime, and a single global typing lives in `src/global.d.ts`.
- Theme handling: initial theme is applied early in `src/app.html` (to avoid FOUC). Ongoing changes and system-follow behavior are managed in `src/lib/themeStore.ts`. Avoid duplicating early theme scripts elsewhere.
