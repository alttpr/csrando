param(
    [string]$Settings = "scripts/settings-sm-only.json",
    [int]$StartSeed = 1000,
    [int]$Count = 50,
    [string]$OutputDirectory = "artifacts/sm-backtracking",
    [string]$Project = "src/Randomizer"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$settingsPath = [IO.Path]::GetFullPath((Join-Path $root $Settings))
$projectPath = [IO.Path]::GetFullPath((Join-Path $root $Project))
$outputPath = [IO.Path]::GetFullPath((Join-Path $root $OutputDirectory))
$csvPath = Join-Path $outputPath "metrics.csv"
$reportPath = Join-Path $outputPath "report.html"

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null
Remove-Item $csvPath, $reportPath -Force -ErrorAction SilentlyContinue

Write-Host "Building Release benchmark binary..."
& dotnet build $projectPath --configuration Release
if ($LASTEXITCODE -ne 0) { throw "Release build failed" }

function Invoke-SeedRun([string]$Label, [string]$Mode) {
    $logPath = Join-Path $outputPath "$Label.log"
    Write-Host "Generating $Count seeds in $Mode mode as '$Label'..."
    & dotnet run --project $projectPath --configuration Release --no-build --no-launch-profile -- `
        randomize --settings $settingsPath --seed $StartSeed --bulk $Count --increment-seed `
        --sm-backtrack-metrics $csvPath --sm-backtrack-label $Label `
        --sm-backtrack-mode $Mode *> $logPath
    if ($LASTEXITCODE -ne 0) {
        throw "Benchmark '$Label' failed; see $logPath"
    }
}

Invoke-SeedRun "baseline" "Legacy"
Invoke-SeedRun "reverse-current" "Reverse"

Write-Host "Building comparison report..."
& python (Join-Path $PSScriptRoot "chart-sm-backtracking.py") $csvPath `
    --baseline baseline --out $reportPath
if ($LASTEXITCODE -ne 0) { throw "Report generation failed" }

Write-Host "Metrics: $csvPath"
Write-Host "Report:  $reportPath"
