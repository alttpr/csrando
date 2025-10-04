# Combined Web + Backend Setup

This repo contains a SvelteKi1) Create `src/web/.env` from the template and set values:

```
cp src/web/.env.example src/web/.env
```

Required variables:

- `DOMAIN` – your site domain (e.g., `randomizer.example.com`)
- `TRAEFIK_ACME_EMAIL` – email for Let's Encrypt certificate notifications
- `API_IMAGE` – the API image (e.g., `ghcr.io/alttpr/csrando-api:latest`)
- `FRONTEND_IMAGE` – (production only) prebuilt frontend image (e.g., `ghcr.io/alttpr/rando-web-frontend:latest`)

Optional variables:

- `PUBLIC_SPRITES_BASE_URL` – base URL for sprite images (defaults to `https://alttpr.com/sprites`)

2) Run the stack on your server:rc/web`) and a .NET API backend (`src/Randomizer`). This guide explains how to run them together for local development and how to deploy in production.

## Local Development (one command)

Prereqs: Node.js, npm, .NET 9 SDK.

Run both services together:

```
./scripts/dev.sh
```

On Windows (PowerShell):

```
scripts\dev.ps1
```

What it does:

- Starts the .NET API in API mode on `http://localhost:5000`.
- Runs DB migrations for the web app (SQLite via Drizzle).
- Starts the SvelteKit dev server on `http://localhost:5173` with env pointing to the local API.

You can also run the pieces manually:

```
# 1) Backend
dotnet run --project src/Randomizer --configuration Debug -- api --urls http://localhost:5000

# 2) Frontend (in another terminal)
cd src/web
npm ci
export PRIVATE_DOTNET_API_BASE_URL=http://localhost:5000
export DATABASE_URL=sqlite:dev.db
export PUBLIC_SPRITES_BASE_URL=/sprites
npm run db:migrate
npm run dev
```

Notes:

- The frontend talks to the backend using `PRIVATE_DOTNET_API_BASE_URL` (server-only env). In dev we set it to `http://localhost:5000`.
- If you want to work without the backend, set `LOCAL_TEST_MODE=true` in `src/web/.env` to enable mock data (see `src/web/docs/LOCAL_TEST_MODE.md`).

## Building the API Docker Image

The backend includes a Dockerfile at `src/Randomizer/Dockerfile` that publishes the app in API mode and listens on `8080`.

```
docker build -t csrando-api:local ./src/Randomizer
```

This image can be used in production or for a local Compose stack.

## Production Deployment (Docker Compose + Traefik)

Production compose files live in `src/web/` and run a Traefik reverse proxy, the frontend, and the API container.

1) Create `src/web/.env` from the template and set values:

```
cp src/web/.env.template src/web/.env
```

Set:

- `DOMAIN` – your site domain (e.g., `example.com`)
- `TRAEFIK_ACME_EMAIL` – email for Let’s Encrypt
- `API_IMAGE` – the API image (e.g., `csrando-api:local` or `ghcr.io/<owner>/csrando-api:latest`)
- Optionally set `FRONTEND_IMAGE` in production to use a prebuilt frontend image

2) Run the stack on your server:

```
cd src/web
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

Routing:

- `https://DOMAIN/` → frontend (SvelteKit adapter-node server on port 3000)
- `https://DOMAIN/api` → .NET API (Traefik strips the `/api` prefix and forwards to port 8080)

3) Updating:

- If using `:latest` tags, Watchtower (included) will auto-update.
- Or pull and restart:

```
docker compose -f docker-compose.yml -f docker-compose.prod.yml pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

For more details and optional GitHub Actions deploy flow, see:

- Frontend docs: `src/web/docs/DEPLOYMENT.md`
- Frontend dev: `src/web/docs/DEVELOPMENT.md`
- Local test mode: `src/web/docs/LOCAL_TEST_MODE.md`

### Automated Image Publishing

The workflow `.github/workflows/publish-images.yml` builds and pushes multi-arch images to GHCR on pushes to `main` and tags:

- Frontend: `ghcr.io/<owner>/rando-web-frontend:{latest|<sha>|<tag>}`
- API: `ghcr.io/<owner>/csrando-api:{latest|<sha>|<tag>}`

Use these tags directly in your `src/web/.env` for Compose deployment.
