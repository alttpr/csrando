# Super Metroid search performance

This document summarizes the Super Metroid search and fill rework and its
measured performance progression. The full measurement history — per-seed
cohorts, benchmark tooling, HTML dashboards, and the validation harness (a
legacy reference implementation plus a cross-checking Validate mode) — is
retained on the reference branch `tewtal/sm-perf`; this branch carries only
the functional changes.

## What changed

- **Reverse backtracking.** "Can the player return to the start after this
  pickup" was previously answered by running a full forward search from every
  candidate location. It is now answered by one concrete reverse traversal of
  the incoming-edge graph per settled inventory, built once and shared through
  a per-graph cache keyed by capability-capped inventories (item counts above
  the highest threshold any requirement can distinguish are collapsed).
  Cross-world portals count as return terminals.
- **Compiled search model.** A per-graph, vertex-indexed model owns the
  topology, event-flag bit assignments, and lazily compiled requirement plans
  that record each requirement's obstacle/door/flag dependencies as bit masks.
- **Forward search.** The stateful searcher stores capacity-independent
  resource debt (capacity pickups update a search context instead of rewriting
  stored states), keeps visited/queued frontiers in vertex-indexed inline
  storage, and resolves whole pickup spheres against a single shared reverse
  pass. A final settled-inventory pass recomputes arrival states and removes
  incremental false positives.
- **Requirement caching.** Results are cached per search pass: inventory-only
  plans by plan id, graph-state readers by their plan-masked obstacle/door
  bits, with ids shared across value-equal requirement instances. The reverse
  traversal caches evaluations the same way.
- **Allocation discipline.** Path-step requirement/resource collections are
  allocated only when they carry content, single-item missing sets are
  interned, set-overlap checks avoid boxed enumerators, and detailed used-item
  and predecessor data is allocated only for path/spoiler searches.
- **Fill hardening.** Assumed fill retries the whole placement pass with a
  deterministically derived PRNG when a greedy ordering dead-ends. The first
  attempt keeps the original PRNG path, so previously successful seeds are
  byte-identical while previously impossible orderings now complete.

## Measured progression

All rows are Windows 11 / .NET SDK 10.0.300 / Release, seeds 9876–9905 (or
9876–9925 where noted), one fresh process per revision and seed, interleaved.
Full per-seed cohorts and dashboards live on `tewtal/sm-perf`
(`scripts/sm-performance-history.csv` is the machine-readable aggregate).

| Stage (reference commit) | Seeds | Mean | Median | p95 | Paired result |
| --- | ---: | ---: | ---: | ---: | ---: |
| main baseline (`c50d6f04`) | 26/30 | 7.518 s | 7.321 s | 9.945 s | — |
| Reverse backtracking (`28917857`) | 24/30 | 4.174 s | 4.097 s | 4.726 s | −44.5% / 1.80x vs main |
| Compiled search model (`68f24942`) | 27/30 | 3.099 s | 3.040 s | 3.368 s | −58.7% / 2.42x vs main |
| Fill retries + 50-seed run (`a16a44c8`) | 50/50 | 3.093 s | 3.060 s | 3.572 s | −55.0% / 2.28x vs main |
| Local frontiers (`e305a7a5`) | 50/50 | 2.865 s | 2.797 s | 3.710 s | −9.5% / 1.11x vs prior |
| Inventory-epoch cache (`ac5d235c`) | 30/30 | 2.858 s | 2.707 s | 3.764 s | −2.9% / 1.03x vs prior |
| Masked evaluation cache (`82c4c114`) | 30/30 | 2.515 s | 2.429 s | 3.151 s | −12.3% / 1.14x vs prior |
| Allocation trim (`8eaff955`) | 30/30 | 2.322 s | 2.259 s | 2.695 s | −6.4% / 1.07x vs prior |

Net effect against main on the common 44-of-50-seed Windows cohort measured at
the `a16a44c8` checkpoint, carried forward through the later per-checkpoint
comparisons:

- Mean full-seed generation time: **≈7.0–7.5 s → ≈2.3 s (≈3x)**.
- Allocations: **18.7 GB → 0.52 GB per seed (≈36x less)**.
- Seed success: main failed seeds 9886, 9890, 9898, 9902, 9915, and 9924 in
  the 50-seed cohort; this implementation completes **50/50** (and 100/100 on
  the extended 9876–9975 cohort), with each recovered seed independently
  validated as winnable.

## Correctness verification

- Every performance-only stage on the reference branch was accepted only with
  exact placement + playthrough output hashes across the full seed cohort.
- Reverse backtracking was developed against a Validate mode that cross-checked
  every definitive reverse answer with the legacy forward search and aborted
  on the first disagreement (retained on `tewtal/sm-perf`).
- This branch was verified to produce byte-identical canonical spoiler output
  to the reference branch head for seeds 9876, 9877, 9881, 9890, 9898, 9902,
  9915, and 9924.
- Test gates: full suite plus slow SM generation regressions (including the
  formerly failing seeds), and the SM-focused unit tests covering reverse
  traversal semantics, cache sharing, dominance rules, and result ownership.
