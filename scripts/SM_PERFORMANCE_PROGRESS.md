# Super Metroid search optimization progress

Final implementation: `ece12a88`  
Historical baselines: `main@c50d6f04`, `reverse-search-v1@28917857`  
Exact-output search checkpoint: `68f24942`

## Result

The directly comparable 30-seed Windows run reduced paired mean full-seed
generation time by **58.71%** relative to main, improving throughput by
**2.42x**. The final correctness pass then recovered every remaining SM-only
failure: seeds 9876–9975 completed **100/100**, with no production legacy
backtracking fallback and no output-hash changes among the 90 seeds that the
pre-retry checkpoint already generated successfully.

| Checkpoint | Seeds completed | Mean | Median | p95 | Paired result vs main |
| --- | ---: | ---: | ---: | ---: | ---: |
| `main@c50d6f04` | 26/30 | 7.518 s | 7.321 s | 9.945 s | baseline |
| `reverse-search-v1@28917857` | 24/30 | 4.174 s | 4.097 s | 4.726 s | 44.48% / 1.80x |
| `compiled-search-v2@68f24942` | 27/30 | 3.099 s | 3.040 s | 3.368 s | 58.71% / 2.42x |
| `fill-retry-final@ece12a88` | 100/100 | 5.451 s | 5.301 s | 6.522 s | different environment |

The last row is the final WSL correctness/work cohort, not a wall-clock
comparison with the Windows rows. In that environment graph JSON construction
averaged 1.998 s instead of 0.263 s at the prior checkpoint. Search output and
work metrics, rather than its raw total, are the valid comparison.

The machine-readable aggregate history is in
`scripts/sm-performance-history.csv`. The detailed final HTML work report is
`artifacts/sm-search-v2-final-100seed-report.html` when generated locally from
the retained metrics CSV files.

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

- Focused SM suite: 31 passed.
- Slow SM generation regressions: 7 passed, including the formerly failing
  9881, 9903, 9908, 9917, 9931, 9955, and 9970 cases.
- Full non-slow suite: 4,352 passed, 4,522 skipped, 0 failed.
- Slow SM-map/M1 portal playthrough regression: passed.
- Validate mode seeds 9876–9878, 9881, and 9903: passed with stable canonical
  hashes and no reverse/reference disagreement.
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
