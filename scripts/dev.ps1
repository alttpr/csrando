#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Combined local dev runner (Windows): .NET API + SvelteKit dev server
# - API on http://localhost:5000
# - Web on http://localhost:5173 (uses PRIVATE_DOTNET_API_BASE_URL)

$RootDir = Resolve-Path (Join-Path $PSScriptRoot '..')
$WebDir  = Join-Path $RootDir 'src/web'
$ApiDir  = Join-Path $RootDir 'src/Randomizer'

$ApiUrl = 'http://localhost:5000'

Write-Host "[dev] Ensuring web deps installed (npm ci)" -ForegroundColor Cyan
Push-Location $WebDir
try {
  npm ci --no-audit --no-fund
} finally {
  Pop-Location
}

Write-Host "[dev] Applying DB migrations (Drizzle)" -ForegroundColor Cyan
Push-Location $WebDir
try {
  try { npm run db:migrate } catch { Write-Warning "db:migrate failed or unavailable; continuing" }
} finally {
  Pop-Location
}

Write-Host "[dev] Starting .NET API on $ApiUrl" -ForegroundColor Cyan
$apiProc = Start-Process -FilePath 'dotnet' `
  -ArgumentList @('run','--configuration','Debug','--','api','--urls', $ApiUrl) `
  -WorkingDirectory $ApiDir `
  -NoNewWindow `
  -PassThru

function Stop-Api {
  if ($null -ne $apiProc -and -not $apiProc.HasExited) {
    Write-Host "[dev] Stopping API (PID $($apiProc.Id))" -ForegroundColor Yellow
    try { Stop-Process -Id $apiProc.Id -Force -ErrorAction SilentlyContinue } catch {}
    try { $apiProc.WaitForExit() | Out-Null } catch {}
  }
}

try {
  # Wait briefly for API to come up
  for ($i = 0; $i -lt 60; $i++) {
    try {
      Invoke-WebRequest -Uri "$ApiUrl/meta" -UseBasicParsing -TimeoutSec 2 | Out-Null
      break
    } catch {
      Start-Sleep -Milliseconds 250
    }
  }

  Write-Host "[dev] Starting SvelteKit (http://localhost:5173)" -ForegroundColor Cyan
  $env:PRIVATE_DOTNET_API_BASE_URL = $ApiUrl
  $env:DATABASE_URL = 'sqlite:dev.db'
  $env:PUBLIC_SPRITES_BASE_URL = '/sprites'

  Push-Location $WebDir
  try {
    npm run dev
  } finally {
    Pop-Location
  }
}
finally {
  Stop-Api
}

