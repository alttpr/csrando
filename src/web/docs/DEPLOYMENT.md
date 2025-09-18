# Deployment (Self-Hosted)

This project ships with a Docker Compose + Traefik stack for easy self-hosting.

## Requirements

- Linux server with Docker Engine + Docker Compose plugin
- A domain pointing to the server (ports 80/443 open)

## 1) Configure environment

Copy the template and set values:

```bash
cp .env.template .env
```

 Set:

- `DOMAIN`: the site domain (e.g., `example.com`)
- `TRAEFIK_ACME_EMAIL`: email for Let’s Encrypt
- `API_IMAGE`: your .NET API Docker image (e.g., `ghcr.io/<owner>/csrando-api:latest`)
- `FRONTEND_IMAGE` (recommended in production): `ghcr.io/<owner>/rando-web-frontend:latest` (built by Actions)

## 2) Run with Docker Compose

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

Traefik will automatically obtain TLS certificates and route:

- `https://DOMAIN/` → frontend
- `https://DOMAIN/api` → API (strip prefix `/api`)

## 3) Zero-downtime updates

- Use Watchtower (included) to auto-update images tagged `:latest`.
- Or pull and restart explicitly:

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml pull

docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 4) GitHub Actions deploy (optional)

A Deploy workflow (`.github/workflows/deploy.yml`) uploads compose files and restarts the stack on your server.

Secrets you must configure in the repo:

- `SSH_HOST`: server hostname or IP
- `SSH_USER`: SSH username
- `SSH_KEY`: private key for SSH auth
- `SSH_PORT` (optional): SSH port (default 22)

Before running the Action:

- Create the target directory on the server (default `/opt/rando-web`).
- Place a `.env` file there (copied from `.env.template` and filled with your values).

Then trigger the “Deploy to Self-Hosted Server” workflow and set `target_dir` (e.g., `/opt/rando-web`).

## Troubleshooting

- TLS fails: ensure ports 80/443 are open; domain DNS points to server; `TRAEFIK_ACME_EMAIL` set.
- 502/Bad Gateway: check logs via `docker logs` for `proxy`, `frontend`, `api` services.
- API route mismatch: confirm your API listens on port `8080` in the container; adjust Traefik labels if needed.
