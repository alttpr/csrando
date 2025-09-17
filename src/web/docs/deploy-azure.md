Azure deployment overview

Resources (suggested, low cost)

- Resource group: small region close to users.
- App Service plan (Linux, Basic B1 or Free F1 while testing).
- Web App (Node): hosts SvelteKit SSR.
- Web App (Windows/Linux .NET): hosts C# API.
- Optional: Azure Storage (File Share) if you decide to mount a dedicated volume for SQLite.

Why this setup

- Uses adapter-node so App Service can run `node build` directly.
- Keeps SQLite persistent by pointing `DATABASE_URL` to `/home/site/local.db` (persistent per-instance storage).
- Builds happen in CI and we deploy the prebuilt server (`build/`) plus runtime `node_modules` to ensure all native modules (e.g., `better-sqlite3`) are available at runtime.

Prerequisites

- Create two Web Apps and one App Service plan (Node for web, .NET for API). Note the names and resource group.
- From each Web App in Azure Portal, download its Publish Profile XML.
- In GitHub repo Settings → Secrets and variables → Actions, add:
  - `AZURE_WEBAPP_NAME_WEB`: name of the web app.
  - `AZURE_WEBAPP_PUBLISH_PROFILE_WEB`: contents of the web app publish profile.
  - `AZURE_WEBAPP_NAME_API`: name of the API app.
  - `AZURE_WEBAPP_PUBLISH_PROFILE_API`: contents of the API app publish profile.
  - `BACKEND_REPO_TOKEN`: a GitHub token with read access to the private API repo.
  - Optional (for config via CLI): `AZURE_CREDENTIALS` (Service Principal JSON) and `AZURE_RESOURCE_GROUP`.

Configure App Settings (Web App)

- In Azure Portal → Your Web App → Configuration → Application settings, add:
  - `DATABASE_URL`: `/home/site/local.db` (persistent SQLite path)
  - `NODE_ENV`: `production` (runtime)
  - `PRIVATE_DOTNET_API_BASE_URL`: `https://<your-api-app>.azurewebsites.net`
  - (optional) `PUBLIC_SPRITES_BASE_URL`: e.g. `https://tewtal.github.io/quad-sprites`
- In Configuration → General settings:
  - Stack: Node 20 LTS
  - Startup command: `node build`

Notes on env usage

- `PRIVATE_DOTNET_API_BASE_URL` is read at runtime via `$env/dynamic/private`.
  - Set it in App Settings; no rebuild required to change it.
- `DATABASE_URL` is read at runtime (`$env/dynamic/private`), so you can change it without rebuilding.

Migrations with Drizzle (SQLite)

- Auto‑migration is enabled at server startup using Drizzle’s migrator (see `src/lib/server/db/index.ts`). It reads SQL files from the `drizzle/` folder and applies them once per process start.
- If migrations or the folder are missing, the app logs an error but continues to run.

Workflow

- See `.github/workflows/deploy-azure.yml`. It installs deps, builds in CI, prunes dev deps, packages `build/`, `node_modules/`, and `drizzle/` into `azure_wwwroot/`, and deploys that folder to the Web App.
- Backend deploy has been removed from this workflow; handle it from the backend repo.

Local checks before first deploy

- Ensure the app builds with `npm run build` and starts with `node build`.
- Ensure `.env` has suitable values; production values come from Azure App Settings.
