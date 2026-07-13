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
      "Alttp": {}
    }
  ]
}
```

Fields:

- `Seed`: non-negative integer. Use `0` to let the server choose a seed.
- `IncludeSpoiler`: boolean. When `false`, the response and public seed permalink omit `spoilerLog`. The server still generates and securely stores the spoiler for admin diagnostics.
- `Configs`: array with at least one world config object. The website currently generates one world config.
- `Configs[].Language`: language code, normally `en`.
- `Configs[].Game`: randomizer target, such as `Alttpr` or `Combo`. Use the `randomizer` value returned by `GET /api/metadata`.
- `Configs[].Alttp`, `Configs[].Metroid`, and other game keys: per-game settings objects. Valid keys and defaults should be discovered from metadata.

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
        "Alttp": {}
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

The `worlds` object contains base64-encoded patch data. Bots that only need to announce the generated seed can ignore it and only use `id` and `seed`. Bots generating a race seed should set `IncludeSpoiler` to `false`; the generated seed can still be shared through its public permalink without exposing its spoiler log.

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
- `postGenSettings`: optional client-side patching/cosmetic settings. These are not part of seed generation.

Each setting includes a `key`, `name`, `type`, and usually either `default`, `values`, or `range`. External services should prefer metadata defaults and only override settings they intentionally expose to users.

## Fetch a generated seed

```http
GET /api/seed/{id}
```

Returns the stored seed row for a generated seed. This is useful if a bot needs to look up a previously generated seed by permalink id. Seeds created with `IncludeSpoiler: false` never include their spoiler log in this public response.

Example:

```bash
curl -sS https://example.com/api/seed/abc123
```

## Fetch the base patch for a seed

```http
GET /api/seed/{id}/base-patch
```

Returns the immutable base patch associated with the randomizer version used for that seed, when available.

Most external services do not need this route. The website's seed page uses it when patching ROMs in the browser.

## Errors

The API uses normal HTTP status codes:

- `400`: invalid request body or missing required data.
- `404`: requested metadata or seed was not found.
- `500`: backend randomizer, database, or server error.

Error responses usually contain a JSON body with a `message` field, but clients should also handle plain text error bodies.

## Operational notes

- These routes are public when the website is public.
- Server-side clients such as Discord bots do not need CORS.
- Browser-based third-party clients may require explicit CORS support if they run from another origin.
- Treat request and response fields as case-sensitive.
- Cache metadata where practical; it changes far less often than generated seeds.
