# Super Metroid backtracking benchmark

The randomizer can append one telemetry row per seed to a CSV. Use identical
seed ranges and settings for every implementation label.

For a complete baseline-versus-current run on Windows, use the convenience
script. It builds Release once, runs both modes in separate processes, preserves
their logs, and creates the report:

```powershell
./scripts/benchmark-sm-backtracking.ps1 -StartSeed 1000 -Count 50
```

The output is written under `artifacts/sm-backtracking/` by default.
The script defaults to `scripts/settings-sm-only.json`, which keeps unrelated
game generation from obscuring SM search costs. Pass
`-Settings scripts/settings-sm-m1.json` for a smaller cross-game portal test.

The equivalent individual commands are shown below for adding more labels or
benchmarking builds from other commits.

```powershell
dotnet run --project src/Randomizer -- randomize `
  --settings scripts/settings-sm-only.json --seed 1000 --bulk 50 --increment-seed `
  --sm-backtrack-metrics artifacts/sm-backtracking.csv `
  --sm-backtrack-label baseline --sm-backtrack-mode Legacy
```

After making an improvement, run the same seeds with a new label:

```powershell
dotnet run --project src/Randomizer -- randomize `
  --settings scripts/settings-sm-only.json --seed 1000 --bulk 50 --increment-seed `
  --sm-backtrack-metrics artifacts/sm-backtracking.csv `
  --sm-backtrack-label reverse-v2 --sm-backtrack-mode Reverse
```

Generate the self-contained comparison report. The chart tool accepts multiple
CSV files, which is useful when a large exact cohort was collected in batches:

```powershell
python scripts/chart-sm-backtracking.py `
  artifacts/sm-backtracking-1.csv artifacts/sm-backtracking-2.csv `
  --baseline baseline --out artifacts/sm-backtracking-report.html
```

For the detailed final dashboard (headline cards, phase totals, log-scale
per-seed scatter, allocation/GC pressure, failures, search work, and test
results), compare two exact Windows cohorts with:

```powershell
python scripts/chart-sm-final-report.py `
  artifacts/sm-windows-main-50.csv artifacts/sm-windows-final-50.csv `
  --history scripts/sm-performance-history.csv `
  --baseline-name main@c50d6f04 --current-name final@a16a44c8 `
  --runtime "Windows 11 · .NET SDK 10.0.300 · Release" `
  --out artifacts/sm-windows-final-report.html
```

The report compares only successful seeds present under every label. It includes
full seed generation mean/median/p95, paired baseline-relative speedup, phase
timings, allocation and GC counts, output-hash mismatches, explicit failure
lists, reverse-proof coverage, fallback rate, reverse traversal build count/time,
and frontier size.

`generation_ms` covers graph/world construction and item placement/spoiler
generation. `validation_ms` records the independent post-generation winnability
check, while `total_ms` includes both. ROM writing remains excluded so it does
not hide search/fill changes. Schema 9 also records a canonical hash of sorted
placements and playthrough output.

For useful results, use a Release build, close other CPU-heavy applications,
and run at least 30 seeds. Run each label from a fresh process if comparing code
from different commits.

`Legacy` bypasses the reverse region and reverse traversal but still records the
same check/fallback counters. This provides a stable baseline from the current
binary. `Reverse` runs the implementation being improved.

During reverse-model development, use `--sm-backtrack-mode Validate`. Every
definitive reverse result is checked with the legacy search and generation stops
at the first mismatch. This mode is for correctness checks, not speed results.

## Retained checkpoints

The aggregate history in `scripts/sm-performance-history.csv` permanently keeps
the main, first reverse-search, compiled-search, and final correctness
checkpoints. The full implementation report is in
`scripts/SM_PERFORMANCE_PROGRESS.md`. Ignored files under `artifacts/` retain the
local per-seed details and self-contained HTML charts.

| Checkpoint | Legacy mean | Reverse mean | Reverse reduction | Report |
| --- | ---: | ---: | ---: | --- |
| General optimization pass | 7788.409 ms | 4333.307 ms | 44.36% | `artifacts/sm-general-final-report.html` |
| Profiler-guided pass | 6109.033 ms | 3639.060 ms | 40.43% | `artifacts/sm-profiler-final-report.html` |

The profiler-guided Reverse mean is 16.02% lower than the previous Reverse
checkpoint. The baseline also moved because requirement-result allocation
improvements benefit both Legacy and Reverse modes, and short wall-clock runs
are subject to machine-load variance. Prefer the within-run Legacy comparison
when evaluating the backtracking architecture itself.

### Actual main comparison

On 2026-07-16, local `main` at `c50d6f04` and compiled-search checkpoint
`68f24942` were run as separate Release processes over the same 30 SM-only
seeds, 9876 through 9905. Timing statistics use the 26 seeds for which both
revisions completed the post-generation winnability check.

| Revision | Successful attempts | Mean | Median | p95 |
| --- | ---: | ---: | ---: | ---: |
| main | 26 / 30 | 7.518 s | 7.321 s | 9.945 s |
| compiled search | 27 / 30 | 3.099 s | 3.040 s | 3.368 s |

Across the paired successful seeds, the compiled-search branch reduced mean
full-seed generation time by 58.71% and provided 2.42x throughput. The final
checkpoint `ece12a88` adds deterministic whole-fill retries and completes all
100 SM-only seeds 9876 through 9975. It retains identical output hashes for all
90 seeds that the pre-retry 100-seed checkpoint generated successfully.

Add one row to `scripts/sm-performance-history.csv` after each accepted
optimization pass so progress remains available without depending on ignored
artifact files. Do not compare wall-clock rows collected on different operating
systems/filesystems; retain their correctness and work metrics but leave the
paired speedup fields blank.

## CPU profiling

Install `dotnet-trace` into the artifacts directory so it does not alter the
machine-wide tool set:

```powershell
dotnet tool install --tool-path artifacts/tools dotnet-trace
```

Build Release, then launch the compiled randomizer under the sampling profiler
so build and `dotnet run` overhead are excluded:

```powershell
dotnet build src/Randomizer --configuration Release

./artifacts/tools/dotnet-trace collect `
  --providers Microsoft-DotNETCore-SampleProfiler --format Speedscope `
  --output artifacts/sm-profiler-cpu.nettrace -- `
  dotnet ./src/Randomizer/bin/Release/net10.0/Randomizer.dll randomize `
  --settings ./scripts/settings-sm-only.json --seed 9876 `
  --sm-backtrack-mode Reverse

./artifacts/tools/dotnet-trace report `
  artifacts/sm-profiler-cpu.nettrace topN --number 40
```

The collection also emits a `*.speedscope.json` file that can be opened in the
Speedscope web UI for caller/callee inspection. Keep the profiler seed and
settings fixed when comparing profiles.
