# Super Metroid search optimization progress

Windows-benchmarked implementation: `ac5d235c`

Historical baselines: `main@c50d6f04`, `reverse-search-v1@28917857`

Exact-output search checkpoint: `68f24942`

Frozen historical baseline: tag `sm-search-baseline-v2`, described by
`scripts/sm-performance-baseline.json`. Its raw Windows cohorts, generated
dashboard, and preview are intentionally retained in Git so the next
optimization pass can compare against the identical per-seed measurements.

Current comparison baseline: tag `sm-search-baseline-v3` at `fcd37dea`.

## Result

The inventory-only requirement cache checkpoint replaces a per-search-pass
dictionary with one vertex-model-owned result array and epoch array. A fresh
epoch isolates every settled inventory without clearing the arrays or growing
them again for each `StatefulSearcher`.

In an interleaved 30-seed Windows comparison against exact tag
`sm-search-baseline-v3`, mean generation time fell from **2.946 s to 2.858 s**
(**2.89% paired reduction**, **1.03x average paired speedup**), median fell
**4.70%**, and p95 fell **0.37%**. Assumed fill improved **3.60%**. Allocations
fell **14.91%**, from 1,107.3 MiB to 942.2 MiB per seed. Both revisions
generated 30/30 seeds and all 30 placement + playthrough hashes match.

The detailed dashboard and retained raw cohorts are
`artifacts/sm-inventory-cache-v4-report.html`,
`artifacts/sm-v3-contemporary-30.csv`, and
`artifacts/sm-model-array-cache-30.csv`.

The next SM-only checkpoint improves on the frozen `sm-search-baseline-v2`
state by another **9.50% paired mean per-seed reduction** and **1.11x average
paired speedup** across the same 50 Windows seeds. Mean generation time fell
from 3.175 s to 2.865 s, and allocations fell **52.07%**, from 2,305 MiB to
1,105 MiB per seed. All 50 placement + playthrough hashes remain identical.

The new detailed dashboard is
`artifacts/sm-windows-sm-local-v3-report.html`, backed by the retained raw
cohort `artifacts/sm-windows-sm-local-50.csv`.

The final 50-seed Windows run reduced paired mean full-seed generation time by
**54.95%** relative to main, with an average paired speedup of **2.28x**.
Allocations fell by **8.43x**, from 18,727 MiB to 2,220 MiB per paired seed.
Pinned main generated 44/50 seeds; the final branch generated **50/50**.

The larger correctness pass also recovered every remaining SM-only failure:
seeds 9876–9975 completed **100/100**, with no production legacy backtracking
fallback and no output-hash changes among the 90 seeds that the pre-retry
checkpoint already generated successfully.

| Checkpoint | Seeds completed | Mean | Median | p95 | Paired result vs main |
| --- | ---: | ---: | ---: | ---: | ---: |
| `main@c50d6f04` | 26/30 | 7.518 s | 7.321 s | 9.945 s | baseline |
| `reverse-search-v1@28917857` | 24/30 | 4.174 s | 4.097 s | 4.726 s | 44.48% / 1.80x |
| `compiled-search-v2@68f24942` | 27/30 | 3.099 s | 3.040 s | 3.368 s | 58.71% / 2.42x |
| `final-windows-50@a16a44c8` | 50/50 | 3.093 s | 3.060 s | 3.572 s | 54.95% / 2.28x |
| `sm-local-frontiers-v3@e305a7a5` | 50/50 | 2.865 s | 2.797 s | 3.710 s | 9.50% / 1.11x vs frozen v2 |
| `sm-inventory-cache-v4@ac5d235c` | 30/30 | 2.858 s | 2.707 s | 3.764 s | 2.89% / 1.03x vs v3 |
| `fill-retry-final@ece12a88` | 100/100 | 5.451 s | 5.301 s | 6.522 s | WSL correctness cohort |

The 100-seed row is the WSL correctness/work cohort, not a wall-clock comparison
with the Windows rows. In that environment graph JSON construction
averaged 1.998 s instead of 0.263 s at the prior checkpoint. Search output and
work metrics, rather than its raw total, are the valid comparison.

The machine-readable aggregate history is in
`scripts/sm-performance-history.csv`. The detailed final HTML work report is
`artifacts/sm-windows-final-report.html`; its full-page preview is
`artifacts/sm-windows-final-report.png`. The separate 100-seed work report is
`artifacts/sm-search-v2-final-100seed-report.html`.

## Final Windows comparison

Both revisions were built and run with Windows .NET SDK 10.0.300 in Release
mode from the same `F:` volume. Revisions were alternated per seed, main first
and final second, with a fresh process for every attempt. Pinned main contained
measurement-only instrumentation; its search and fill behavior was unchanged.

| Metric, 44 common successful seeds | Main | Final | Change |
| --- | ---: | ---: | ---: |
| Total generation time | 307.99 s | 136.07 s | 2.26x faster |
| Mean generation time | 7.000 s | 3.093 s | 54.95% paired reduction |
| Median generation time | 6.879 s | 3.060 s | 55.5% lower |
| p95 generation time | 8.850 s | 3.572 s | 59.6% lower |
| Allocations per seed | 18,727 MiB | 2,220 MiB | 8.43x less |
| Mean validation time | 443.4 ms | 47.3 ms | 9.37x faster |

The worst main latency among common successes was seed 9879: 9.421 seconds on
main and 2.815 seconds on final, a 3.35x speedup. Across all final attempts, p95
was 4.08 seconds and the worst seed was 5.24 seconds.

Main failed seeds 9886, 9890, 9898, 9902, 9915, and 9924. Final generated and
independently validated all six. Combined placement + playthrough hashes are
not expected to match main because this branch includes documented correctness
and route-selection fixes; `68f24942` remains the exact-output baseline for the
performance-only stages.

## What changed

- Backtracking now follows the actual incoming directed SM strategies in one
  concrete reverse traversal. Cross-world portals are return terminals and
  production never falls back to the legacy traversal.
- `SmSearchModel` owns immutable vertex-ID topology, incoming edges, event flag
  metadata, and lazily compiled obstacle/door/flag dependency plans.
- Reverse frontiers use arrays, bitsets, integer queues, reusable scratch
  storage, and collision-safe capability keys with resource-count caps.
- Forward search uses vertex-indexed frontiers and capacity-independent resource
  debt. Capacity pickups update the search context rather than rewriting every
  stored state.
- Inventory-only requirement results are cached per settled inventory. Detailed
  used-item and predecessor data is allocated only for path/spoiler searches.
- Room/node lookups are indexed during preprocessing, and assumed fill keeps
  per-world assumed item lists instead of repeatedly materializing the whole
  pool.
- Search telemetry is opt-in and aggregated outside hot loops. CSV schema 9
  records graph, fill, spoiler, validation, allocation/GC, status, failure, and
  canonical placement/playthrough hashes.
- A final settled-inventory pass removes incremental false positives. Bounded,
  deterministic whole-fill retries handle greedy placement dead ends without
  changing the first-attempt PRNG path of successful seeds.
- SM forward visited/queued frontiers now keep their common first four states
  inline and allocate overflow storage only for wider nondominated sets.
- Reverse traversal reuses per-search candidate, pruning, and unlock buffers
  instead of constructing temporary lists for every incoming edge and lock.
- Enemy-kill checks reuse precomputed invulnerability sets and avoid candidate
  or used-weapon collections when spoiler details are disabled. Ammo
  substitution uses exact integer ceiling arithmetic in the hot path.

The parsed-JSON template optimization was not introduced: the profile used for
the architecture decision put graph construction below the 5% stop threshold.
The later WSL graph time is dominated by the host-mounted filesystem and is not
evidence that mutable template sharing would be safe or faster in production.

## Final 100-seed work profile

Seeds 9876–9975, SM-only, Release, reverse mode:

| Metric | Per seed / result |
| --- | ---: |
| Successful generation | 100/100 |
| Reverse proofs / rejects | 96.5% / 3.5% |
| Production legacy fallback | 0 |
| Reverse builds | 128.1 |
| Reverse build time | 854.8 ms |
| Reverse frontier entries | 101,971 |
| Forward passes | 1,477.2 |
| Forward dequeued states | 1,242,754 |
| Forward requirement evaluations | 6,025,192 |
| Allocated memory | 2,335.8 MiB |
| Mean graph / fill / spoiler / validation | 1998.4 / 3022.2 / 402.5 / 48.1 ms |

Relative to the initial reverse profile (about 248 builds, 197,000 frontier
entries, and 1.35 s of reverse construction per seed), reverse builds and
frontier entries are both down about 48%, and reverse construction time is down
about 37%. The correctness-required settled pass increases visible forward work;
removing it reintroduced the false-positive arrival exposed by seed 9881, so it
was retained.

## Correctness gates

- Final Windows normal suite: 4,354 passed, 4,520 skipped, 0 failed (1m 1s).
- Final Windows slow SM regressions: 8 passed, 0 failed (26s).
- Focused SM suite: 34 passed.
- SM-local frontier checkpoint normal suite: 4,355 passed, 4,520 skipped,
  0 failed; slow SM regressions: 8 passed, 0 failed.
- Slow SM generation regressions: 7 passed, including the formerly failing
  9881, 9903, 9908, 9917, 9931, 9955, and 9970 cases.
- Full non-slow suite: 4,352 passed, 4,522 skipped, 0 failed.
- Slow SM-map/M1 portal playthrough regression: passed.
- Validate mode seeds 9876–9878, 9881, and 9903: passed with stable canonical
  hashes and no reverse/reference disagreement.
- Inventory-cache checkpoint: focused SM suite 33/33, full Windows suite 4,355
  passed / 4,520 skipped / 0 failed, and Validate mode 5/5.
- Final SM-only cohort: 100/100.
- 30-seed SM/M1, keycard, Medium, and Hard cohorts: 30/30 each.

The SM map-randomizer plus M1 map-shuffle cohort completed 25/30. The five
failures (9877, 9889, 9890, 9895, and 9899) reproduce on pinned main: two are
unwinnable generated maps and three are impossible opening/fill topologies.
They occur outside the SM search equivalence boundary and are recorded rather
than attributed to this optimization. The slow portal regression is pinned to
a valid topology so it tests portal-state retention rather than an impossible
front-fill start.

## Reproducing reports

Run at least 30 seeds in fresh Release processes as described in
`scripts/SM_BACKTRACKING_BENCHMARK.md`. Multiple exact-cohort CSV files can be
combined without rewriting them:

```powershell
python scripts/chart-sm-backtracking.py `
  artifacts/sm-search-v2-final-30seed.csv `
  artifacts/sm-search-v2-final-70seed.csv `
  --baseline search-v2-final `
  --out artifacts/sm-search-v2-final-100seed-report.html
```

Keep performance-only checkpoints only when hashes remain stable and the
documented mean/target-work acceptance gate is met. Failures remain explicit in
the CSV and are never excluded silently.

For the next optimization pass, use `artifacts/sm-windows-final-50.csv` as the
dashboard baseline, rerun seeds 9876–9925 on Windows with the same settings and
runtime, append the accepted checkpoint to `scripts/sm-performance-history.csv`,
and include both the frozen baseline and new cohort in the regenerated report.
