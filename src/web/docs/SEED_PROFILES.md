# Seed Profiles

For user-facing instructions, see [Using profiles](PROFILES.md). This document
covers the implementation and operational details.

Developer notes for the Seed Profiles feature: named, saved seed
configurations on the config page (`/config/[id]`), with private user profiles
and administrator-curated official presets.

## Data model

Four tables in `src/lib/server/db/schema.ts` (migration
`drizzle/0005_seed-profiles.sql`):

- `configuration_profile` — the logical profile. `scope` is `official`
  (admin-curated, `owner_user_id` null, stable `slug`) or `user` (private,
  owned). `config_id` is the config page it belongs to (e.g. `combo`).
  `current_revision_id` points at the newest revision (no FK — circular
  reference; integrity is enforced in the service layer). Deletion is a soft
  delete via `deleted_at`.
- `configuration_profile_revision` — **immutable** settings snapshots.
  Updating a profile appends a revision and moves `current_revision_id`;
  revisions are never mutated or deleted (except by profile cascade). Each
  stores `config_schema_version` and the normalized settings JSON.
- `user_profile_preference` — per-user default and last-used profile.
- `user_profile_favorite` — per-user pins (composite PK).

The `seed` table gained nullable attribution columns: `profile_id`,
`profile_revision_id` (intentionally **no FKs** — a seed's stored
configuration must never change when profiles change), a server-computed
`differed_from_revision` flag, `config_schema_version`, and
`settings_snapshot` (the normalized configuration at generation time;
`options` still stores the exact generator payload, which is lossy).

`user.is_admin` (boolean) gates official-preset administration, exposed on
`locals.user.isAdmin` through Lucia (`src/lib/server/auth.ts`,
`src/lib/server/auth-guards.ts`).

## Canonical configuration and normalization

`src/lib/config/normalize.ts` is the single source of truth for the
configuration shape. The canonical `NormalizedConfig` mirrors the form state:

```jsonc
{ "selectedGames": ["Alttpr", "Sm"], "global": { ... }, "perGame": { "Alttpr": { ... } } }
```

- `normalizeConfig(form, metadata)` — fills defaults, coerces types, sorts
  order-free collections; deterministic.
- `configsEqual` / `stableStringify` — the only sanctioned way to compare
  configurations (recursively sorted keys). Dirty state on the config page is
  `!configsEqual(normalizeConfig(liveForm), loadedRevisionBaseline)`.
- `buildRandomizePayload(config, metadata, opts)` — derives the generator
  request (extracted verbatim from the old inline submit logic; parity-tested
  in `tests/lib/config-normalize.test.ts`).
- `hydrateFormState(config, metadata)` — inverse mapping used to load a
  profile/draft into the form; reports removed/reset settings instead of
  silently replacing them.

## Schema versions and migrations

`src/lib/config/constants.ts` defines `CONFIG_SCHEMA_VERSION` (currently 1).
Every revision, draft, and seed snapshot stores it.

To change the settings shape in the future:

1. Bump `CONFIG_SCHEMA_VERSION`.
2. Append a `{ from, to, description, migrate }` step to `CONFIG_MIGRATIONS`
   in `src/lib/config/migrations.ts` (pure function, must not mutate input).
3. Done — profile/draft loads run `migrateConfig` sequentially and surface
   the applied steps to the user; versions newer than the app are rejected.

## Server layer

- `src/lib/server/profiles/service.ts` — all DB access and authorization:
  officials readable by everyone (archived ones stay readable by id), user
  profiles only by their owner (foreign → 404), official mutations
  admin-only (403). Revisions use optimistic concurrency: saving with a stale
  `baseRevisionId` returns 409. Enforced limits live in
  `src/lib/config/constants.ts` (name 1–60, description ≤240, max 100
  profiles/user, 64 KiB settings JSON).
- `src/lib/server/profiles/validate.ts` — validates submitted settings
  against the real generator metadata (active `randomizer_version` snapshot,
  falling back to the live backend metadata).
- API endpoints under `src/routes/api/profiles/**` and
  `src/routes/api/user/profile-{preferences,favorites}` (see the handlers for
  request/response shapes; client wrappers in `src/lib/services/data.ts`).
- `/api/randomize` accepts an optional `Profile` block, strips it before
  forwarding to the .NET generator, verifies the referenced revision, and
  computes `differed_from_revision` server-side. Attribution is best-effort
  and never fails generation.

## Official presets

Fixtures in `src/lib/server/profiles/official-fixtures.ts` are seeded
idempotently (keyed by `slug`) at boot (`src/lib/server/db/index.ts`) and
lazily from the profiles list. Fixture settings are computed from the
generator's metadata defaults — never hardcoded game-design values. Exactly
one preset should have `isRecommended`; the read side falls back
deterministically (lowest `display_order`, then slug) if none does.

**To add or update an official preset:**

- As an admin in the UI: configure the settings, click "Save as new profile",
  and tick "Save as official preset" (requires a unique slug; optionally mark
  it recommended). Manage existing presets from the toolbar overflow menu
  (rename, archive, set recommended, delete) and update their settings with
  the normal "Save changes" button (creates a new revision).
- Alternatively: add a fixture (name/description/tags/order) and restart, or
  `POST /api/profiles` with `scope: "official"` and a `slug`. Description,
  tags, difficulty and display order are editable via
  `PATCH /api/profiles/:id`.

## Admin panel

`/admin` (visible in the navbar for admin accounts) bundles the admin
surfaces: a stats dashboard, user management (promote/demote admins —
self-demotion is blocked for sessions), official-profile curation
(archive/unarchive, edit incl. curation fields, delete, set recommended, and
**promote a profile to an official preset** — one of your own via
`POST /api/profiles/:id/promote`, or any user's shared profile via its share
link with `POST /api/profiles/promote-shared`),
a global seed list (search by id; race seeds expose their spoiler on
`/admin/seed/[id]`), and randomizer-version management (activate/deactivate,
upload). Every panel page and sensitive admin endpoint requires a logged-in
administrator session (`requirePanelAccess` in
`src/lib/server/admin/guard.ts`).

Password management is deliberately email-free (no addresses are stored):
admins reset a forgotten password from the Users page
(`POST /api/admin/users/:id/reset-password` — returns an autogenerated
password once and invalidates the user's sessions), and users change their
own on `/profile/security` (`POST /api/user/password`; current password required
whenever one is set, other sessions are invalidated).

**To promote the first admin** use the server-level admin token
(`PRIVATE_ADMIN_VERSION_TOKEN`):

```
curl -X POST https://the-site/api/admin/promote \
  -H "Authorization: Bearer $PRIVATE_ADMIN_VERSION_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"username": "somebody"}'
```

The token works only while there are no administrators and can only grant the
first administrator role. After bootstrap, signed-in administrators manage
roles from the Users panel; the token cannot be reused, demote users, access
the panel, inspect spoilers, reset passwords, or create versions.

## Share links

Profiles can be shared by link from the toolbar's overflow menu:

- Official presets share as readable `/config/{configId}/{slug}` links
  (they are publicly readable anyway). Legacy `?profile=` links remain
  supported.
- Private profiles use a revocable capability token
  (`configuration_profile.share_token`, 24-char nanoid):
  `/config/{configId}?share={token}`. `POST/DELETE /api/profiles/:id/share`
  creates/revokes it (owner only, idempotent).

Opening a share link loads the profile's **current revision** as a Custom
configuration with a dismissible notice — the recipient never gets the
profile itself or its ids, and saving creates an independent copy (no live
inheritance). Revoked, deleted, or unknown tokens degrade to an
"invalid link" notice; the regular profile API stays owner-only regardless
of the token.

## Seed attribution display

The seed page (`/seed/[id]`) shows which profile a seed was generated from,
resolved server-side by `getSeedAttribution` in the profiles service:

- Official presets are attributed for every viewer; private profiles only for
  their owner (everyone else sees no attribution at all).
- Deleted profiles keep their name but lose the link. Attribution is hidden
  when `differed_from_revision` is set so the source profile is not mistaken
  for the seed's actual settings.
- "Open these settings in the configurator" opens the profile directly when
  it is accessible and unchanged. Official profile URLs use their readable
  slug. Otherwise it navigates to `/config/{configId}?fromSeed={seedId}` and
  loads the stored snapshot as an unowned Custom configuration (the same
  mechanism as share links, `applySharedSettings`). Older seeds without an
  accessible profile or snapshot simply don't offer the link.

## API keys and external tools

Users create personal API keys at `/profile/api-keys` (table `api_key`,
helpers in `src/lib/server/api-keys.ts`). Only the SHA-256 hash is stored; the
secret (`qr_` + 40 chars) is shown once. `hooks.server.ts` resolves
`Authorization: Bearer qr_...` on `/api/` routes to `locals.user` with
`locals.session = null`; `requireSessionUser` guards session-only operations
(key management itself). API-key principals never get `isAdmin`.

Bots can generate straight from a profile: `POST /api/randomize` with
`{ProfileId, Seed?, IncludeSpoiler?, RevisionId?}` — the server migrates and
hydrates the revision, builds the generator payload with the same
`buildRandomizePayload`, and records full attribution. See
`docs/PUBLIC_API.md` for the public contract.

## Client

- `src/lib/config/profile-state.svelte.ts` — `ProfileState` rune class: lists,
  selection, dirty baseline, save/load/duplicate/pin/default actions.
- `src/lib/config/profile-storage.ts` — localStorage drafts (debounced from
  the config page, cleared when clean), recently-used list, logged-out
  last-used. Drafts are never presented as saved profiles.
- `src/lib/config/profile-selection.ts` — startup order: explicit profile link >
  recoverable draft > default > last-used (server, then local) > recommended
  preset > metadata defaults.
- UI in `src/lib/components/config/profiles/` (toolbar, searchable ARIA
  combobox, save/unsaved-changes/delete dialogs, overflow menu) on top of new
  `ui/Modal.svelte` and `ui/Badge.svelte`.
- Account UI uses independent tabbed routes so failures remain isolated:
  overview (`/profile`), paginated seeds (`/profile/seeds`), saved profiles
  (`/profile/profiles`), API keys (`/profile/api-keys`), and password/account
  security (`/profile/security`).

## Tests

`tests/lib/config-normalize.test.ts` (incl. legacy-payload parity),
`config-migrations`, `profile-selection`, `profile-storage`, `Modal`,
`ProfileToolbar` (full load→modify→save→switch scenario against a fake
fetch backend), and API tests `tests/api/profiles-crud`, `profiles-revisions`,
`profile-preferences`, `randomize-profile-attribution` (real in-memory SQLite).
