# Public API

External services can use the website's SvelteKit API routes to generate seeds and link users to the normal seed permalink page. The .NET randomizer API stays internal; public clients should call the website domain.

Use HTTPS and replace `https://example.com` with the deployed site origin.

## Generate a seed

```http
POST /api/randomize
Content-Type: application/json
```

Request body:

```json
{
  "Seed": 0,
  "IncludeSpoiler": true,
  "Configs": [
    {
      "Language": "en",
      "Game": "Alttpr",
      "Alttpr": {}
    }
  ]
}
```

Fields:

- `Seed`: non-negative integer. Use `0` to let the server choose a seed.
- `IncludeSpoiler`: boolean. When `false`, the response and public seed permalink omit `spoilerLog`. The server still generates and securely stores the spoiler for admin diagnostics.
- `Configs`: array with at least one world config object. The website currently generates one world config.
- `Configs[].Language`: language code, normally `en`.
- `Configs[].Game`: randomizer target, such as `Alttpr` or `Combo`. Use the `randomizer` value returned by `GET /api/metadata`. It may be omitted to let a configured random-target mode choose the target.
- `Configs[].Alttpr`, `Configs[].Sm`, and other game keys: per-game settings objects. Their exact names, valid keys, and defaults come from `gameSettings` in metadata; do not derive them from display names.

Example with `curl`:

```bash
curl -sS https://example.com/api/randomize \
  -H 'Content-Type: application/json' \
  -d '{
    "Seed": 0,
    "IncludeSpoiler": true,
    "Configs": [
      {
        "Language": "en",
        "Game": "Alttpr",
        "Alttpr": {}
      }
    ]
  }'
```

Successful response:

```json
{
  "id": "abc123",
  "seed": 123456789,
  "worlds": {
    "0": {
      "bpsPatch": "base64-encoded patch data",
      "ipsPatch": "base64-encoded patch data"
    }
  },
  "spoilerLog": {
    "Locations": {
      "Example Location": "Example Item"
    }
  }
}
```

Use the returned `id` to build the public permalink:

```text
https://example.com/seed/abc123
```

Each entry in `worlds` contains at least one base64-encoded `ipsPatch` or `bpsPatch`; clients must not assume both are present. Bots that only need to announce the generated seed can ignore `worlds` and only use `id` and `seed`. Bots generating a race seed should set `IncludeSpoiler` to `false`; `spoilerLog` is then omitted from this response and hidden on the public permalink, while remaining available to authorized administrators for diagnostics.

### Logical playthrough spoiler data

When a spoiler is included, `spoilerLog.playthrough.data` contains a JSON-encoded logical playthrough. It identifies the pickups required to finish the seed and groups them into progression spheres. The seed page renders this data as the **Logical playthrough** panel.

The decoded object has these top-level fields:

- `complete`: whether the generated playthrough reached every victory item.
- `victoryItems`: the victory items required for the seed.
- `startingItems`: inventory available before the first sphere.
- `spheres`: ordered progression spheres. Each pickup includes its item, location, required items, and route.
- `warnings`: generation or analysis warnings, if any.

Each route step can include item requirements, resource expenditure, a Super Metroid strategy, and `crossGame: true` for portal travel. `meta` items and pickups are logic events or flags rather than normal item locations; clients that present a simplified playthrough should hide them except for victory items. The field is optional so clients must continue to handle spoiler logs that do not contain it.

For a Discord bot, the common flow is:

1. Fetch or cache metadata for the randomizer.
2. Build a `POST /api/randomize` request.
3. Read `id` from the response.
4. Post `https://example.com/seed/{id}` back to the channel.

## Discover randomizers

```http
GET /api/metadata
```

Returns the available randomizers. Each entry includes a display name and backend randomizer id.

Example:

```bash
curl -sS https://example.com/api/metadata
```

## Discover settings

```http
GET /api/metadata/{id}
```

Returns the option metadata for one randomizer. Use the `randomizer` value from `GET /api/metadata` as the canonical id for this route. Current backend ids are enum-style names such as `Alttpr`, `Combo`, `Z1R`, and `G2R`.

Example:

```bash
curl -sS https://example.com/api/metadata/Alttpr
```

The response contains:

- `settings`: global settings. These map to top-level fields in each object under `Configs`.
- `gameSettings`: per-game setting groups. Each group maps to a nested object in the world config, for example `Configs[0].Alttp`.
- `postGenSettings`: optional client-side patching/cosmetic settings. These are not part of seed generation. Entries are keyed by game id, with the extra key `combo` holding options that configure the combined ROM as a whole (for example MSU-1 volume) rather than a single game. Each option is a `toggle`, a `select`, or a `number`; `number` options declare `min`/`max`/`step` and write the chosen value itself as little-endian bytes at each entry in `patches`.

Each setting includes a `key`, `name`, `type`, and usually either `default`, `values`, or `range`. External services should prefer metadata defaults and only override settings they intentionally expose to users.

## Fetch a generated seed

```http
GET /api/seed/{id}
```

Returns the stored seed row for a generated seed. This includes `id`, `options`, `patchData`, `placementInfo`, `spoilerLog`, version/preset attribution fields, the normalized `settingsSnapshot` when available, and `createdAt`. This is useful if a bot needs to look up a previously generated seed by permalink id. For seeds created with `IncludeSpoiler: false`, `spoilerLog` is `null` in this public response.

Example:

```bash
curl -sS https://example.com/api/seed/abc123
```

## Fetch the base patch for a seed

```http
GET /api/seed/{id}/base-patch
```

Returns the immutable base patch associated with the randomizer version used for that seed as `application/octet-stream`, when available. The response is publicly cacheable and immutable; a seed without a stored randomizer version/base patch returns `404`.

Most external services do not need this route. The website's seed page uses it when patching ROMs in the browser.

## Authenticate with an API key

Logged-in users can create personal API keys at `/profile/api-keys`. The plain secret (`qr_...`) is shown exactly once at creation.

Send the key as a Bearer token on any `/api/` route:

```http
Authorization: Bearer qr_XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

Key-authenticated requests act as the key's owner: they can read and manage that user's private seed presets, and generated seeds are linked to the user's history. Deliberate restrictions:

- API keys never grant administrator rights, even if the owning user is an admin.
- API keys cannot call session-only account/security operations, including creating or revoking API keys and changing the account password.

## Seed presets

```http
GET /api/presets?configId=combo
```

`configId` is required and case-insensitive. The response contains:

- `officials`: active official preset summaries (available without authentication).
- `mine`: the authenticated user's active private preset summaries, otherwise `[]`.
- `preferences`: the authenticated user's defaults/favorites, otherwise `null`.
- `recommendedId`: the internal id of the recommended official preset.
- `configSchemaVersion`: the current normalized-settings schema version.

Preset summaries include `id`, `scope`, `slug`, `configId`, `name`, `description`, revision/schema information, `selectedGames`, curation tags/status, ordering, and timestamps. Mutations use the internal `id`. Public reads and seed generation can use an official preset's slug, matching human-facing links such as `/config/combo/recommended`.

```http
GET    /api/presets/{id}
GET    /api/presets/{id}?revision={revisionId}
POST   /api/presets                   # create a private preset
POST   /api/presets/{id}/revisions    # save new settings (owner)
POST   /api/presets/{id}/duplicate    # copy a readable preset
PATCH  /api/presets/{id}              # update metadata (owner)
DELETE /api/presets/{id}              # soft delete (owner)
```

`GET /api/presets/{id}` returns `{ "preset": PresetSummary, "revision": PresetRevision }`. Official presets are readable anonymously. Private presets return `404` to non-owners, and deleted presets return `404` to everyone, without revealing whether they exist.

Create a private preset with:

```json
{
  "configId": "combo",
  "name": "My tournament settings",
  "description": "Optional description",
  "settings": {
    "selectedGames": ["Alttpr", "Sm"],
    "global": { "Game": "Combo", "Language": "en" },
    "perGame": {
      "Alttpr": {},
      "Sm": {},
      "Combo": {}
    }
  },
  "setAsDefault": false,
  "favorite": false
}
```

Preset settings use the normalized configuration shape shown above and are validated against current generator metadata. Creation returns `{ "preset": ..., "revision": ... }` with status `201`.

Revision updates send `{ "settings": ..., "baseRevisionId": "...", "changeSummary": "optional" }`. `baseRevisionId` must be the preset's current revision id (or `null` only when the preset has no current revision); a stale value returns `409`. Successful revision creation returns the updated preset and revision with status `201`.

`POST /api/presets/{id}/duplicate` accepts an optional `{ "name": "..." }` body and creates an independent private copy. `PATCH` accepts preset metadata such as `name` and `description`. Official-preset curation remains browser-admin functionality because API keys never carry admin rights.

Owners can create or revoke the capability link used by the website with `POST` or `DELETE /api/presets/{id}/share`. The POST response is `{ "token": "..." }`; the browser URL is `/config/{configId}?share={token}`. The token grants access only to an unowned settings snapshot through that page—it does not authorize the regular preset API.

## Generate a seed from a preset

External tools can generate directly from a saved preset without reconstructing the full `Configs` payload:

```http
POST /api/randomize
Content-Type: application/json
Authorization: Bearer qr_...   (required for private presets)
```

```json
{
  "PresetId": "recommended",
  "Seed": 0,
  "IncludeSpoiler": false
}
```

Optional fields: `Seed` (default `0` = random), `IncludeSpoiler` (default `true`), `RevisionId` (generate from an older revision; defaults to the preset's current one). Official presets work without authentication; private presets require an API key owned by the preset's owner. The response is identical to a regular `/api/randomize` call, and the stored seed records which preset and revision it came from.

`PresetId` accepts either the internal id returned by the presets API or an official preset's readable slug. Private presets must use their internal id.

## Fetch authenticated seed history

```http
GET /api/user/seeds
Authorization: Bearer qr_...
```

Returns the authenticated user's generated seeds in newest-first order. Each entry contains `id`, generation `options`, and `createdAt`. Anonymous requests return `401`.

## Errors

The API uses normal HTTP status codes:

- `400`: invalid request body or missing required data.
- `401`: authentication required or invalid credentials.
- `403`: authenticated but not allowed (e.g. admin-only or session-only operations).
- `404`: requested metadata, seed, or preset was not found.
- `409`: preset revision conflict (stale `baseRevisionId`).
- `503`: generator metadata required for preset expansion is temporarily unavailable.
- `500`: backend randomizer, database, or server error.

Error responses usually contain a JSON body with a `message` field, but clients should also handle plain text error bodies.

## Operational notes

- These routes are public when the website is public.
- Server-side clients such as Discord bots do not need CORS.
- Browser-based third-party clients may require explicit CORS support if they run from another origin.
- Treat request and response fields as case-sensitive.
- Cache metadata where practical; it changes far less often than generated seeds.
