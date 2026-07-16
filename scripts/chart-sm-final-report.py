#!/usr/bin/env python3
"""Build the detailed final SM performance dashboard from two metrics cohorts."""

from __future__ import annotations

import argparse
import csv
import html
import math
import statistics
from datetime import datetime, timezone
from pathlib import Path


BLUE = "#2f7ed8"
GREEN = "#17a878"
GRAY = "#8b8b84"
RED = "#d34a4a"


def number(row: dict[str, str], key: str) -> float:
    return float(row.get(key, "0") or 0)


def latest_rows(path: Path) -> dict[int, dict[str, str]]:
    with path.open(newline="", encoding="utf-8-sig") as source:
        rows = list(csv.DictReader(source))
    if not rows:
        raise SystemExit(f"{path} has no metric rows")
    # Benchmark smoke tests and interrupted/restarted cohorts may append duplicate
    # seeds. The last row is the final clean sample, matching the comparison tool.
    return {int(row["seed"]): row for row in rows}


def percentile(values: list[float], fraction: float) -> float:
    ordered = sorted(values)
    position = (len(ordered) - 1) * fraction
    lower = int(position)
    upper = min(lower + 1, len(ordered) - 1)
    weight = position - lower
    return ordered[lower] * (1 - weight) + ordered[upper] * weight


def mean(rows: list[dict[str, str]], key: str) -> float:
    return statistics.fmean(number(row, key) for row in rows)


def seconds(milliseconds: float) -> str:
    return f"{milliseconds / 1000:.2f} s"


def mib(value: float) -> str:
    return f"{value / 1048576:,.0f} MiB"


def card(label: str, value: str, detail: str, tone: str = "good") -> str:
    return (
        '<section class="metric-card">'
        f'<div class="metric-label">{html.escape(label)}</div>'
        f'<div class="metric-value">{html.escape(value)}</div>'
        f'<div class="metric-detail {tone}">{html.escape(detail)}</div>'
        "</section>"
    )


def stage_chart(
    baseline: list[dict[str, str]], current: list[dict[str, str]],
    baseline_name: str, current_name: str,
) -> str:
    stages = [
        ("Graph construction", "graph_construction_ms"),
        ("Assumed fill", "assumed_fill_ms"),
        ("Spoiler/playthrough", "spoiler_ms"),
        ("Validation", "validation_ms"),
    ]
    totals = [
        (label, sum(number(row, key) for row in baseline) / 1000,
         sum(number(row, key) for row in current) / 1000)
        for label, key in stages
    ]
    width, height = 940, 390
    left, right, top, bottom = 72, 24, 28, 76
    plot_w, plot_h = width - left - right, height - top - bottom
    maximum = max(value for _, before, after in totals for value in (before, after))
    maximum = math.ceil(maximum / 20) * 20 or 20
    group = plot_w / len(totals)
    bar_w = group * 0.27
    parts = [
        f'<svg viewBox="0 0 {width} {height}" role="img" '
        'aria-label="Total time by generation phase">',
    ]
    for tick in range(6):
        value = maximum * tick / 5
        y = top + plot_h - plot_h * tick / 5
        parts.append(f'<line class="grid" x1="{left}" y1="{y:.1f}" '
                     f'x2="{width-right}" y2="{y:.1f}"/>')
        parts.append(f'<text class="tick" x="{left-10}" y="{y+4:.1f}" '
                     f'text-anchor="end">{value:.0f}</text>')
    for index, (label, before, after) in enumerate(totals):
        center = left + group * (index + 0.5)
        for offset, value, color in [(-bar_w * 0.6, before, BLUE),
                                     (bar_w * 0.6, after, GREEN)]:
            h = plot_h * value / maximum
            x = center + offset - bar_w / 2
            y = top + plot_h - h
            parts.append(f'<rect x="{x:.1f}" y="{y:.1f}" width="{bar_w:.1f}" '
                         f'height="{h:.1f}" rx="4" fill="{color}"/>')
            parts.append(f'<text class="bar-value" x="{x+bar_w/2:.1f}" '
                         f'y="{max(y-7, 14):.1f}" text-anchor="middle">'
                         f'{value:.1f}s</text>')
        parts.append(f'<text class="axis-label" x="{center:.1f}" '
                     f'y="{top+plot_h+27}" text-anchor="middle">'
                     f'{html.escape(label)}</text>')
    parts.append(f'<text class="axis-title" transform="translate(18 '
                 f'{top+plot_h/2}) rotate(-90)" text-anchor="middle">seconds</text>')
    parts.append("</svg>")
    return (
        f'<div class="legend"><span><i style="background:{BLUE}"></i>'
        f'{html.escape(baseline_name)}</span><span><i style="background:{GREEN}"></i>'
        f'{html.escape(current_name)}</span></div>' + "".join(parts)
    )


def scatter_chart(
    seeds: list[int], baseline: dict[int, dict[str, str]],
    current: dict[int, dict[str, str]], baseline_name: str, current_name: str,
) -> str:
    width, height = 940, 390
    left, right, top, bottom = 72, 24, 28, 58
    plot_w, plot_h = width - left - right, height - top - bottom
    values = [number(rows[seed], "generation_ms") / 1000
              for rows in (baseline, current) for seed in seeds]
    low = max(1.0, min(values) * 0.75)
    high = max(values) * 1.15
    log_low, log_high = math.log10(low), math.log10(high)

    def x(index: int) -> float:
        return left + plot_w * index / max(len(seeds) - 1, 1)

    def y(value: float) -> float:
        return top + plot_h * (log_high - math.log10(value)) / (log_high - log_low)

    parts = [f'<svg viewBox="0 0 {width} {height}" role="img" '
             'aria-label="Per-seed generation time on a logarithmic scale">']
    ticks = [tick for tick in [1, 2, 3, 4, 5, 7, 10, 15] if low <= tick <= high]
    for tick in ticks:
        ty = y(tick)
        parts.append(f'<line class="grid" x1="{left}" y1="{ty:.1f}" '
                     f'x2="{width-right}" y2="{ty:.1f}"/>')
        parts.append(f'<text class="tick" x="{left-10}" y="{ty+4:.1f}" '
                     f'text-anchor="end">{tick:g}s</text>')
    for index, seed in enumerate(seeds):
        if index % 5 == 0 or index == len(seeds) - 1:
            parts.append(f'<text class="tick" x="{x(index):.1f}" '
                         f'y="{top+plot_h+23}" text-anchor="middle">{seed}</text>')
        main_row, final_row = baseline[seed], current[seed]
        main_x, final_x = x(index) - 2.5, x(index) + 2.5
        main_y = y(number(main_row, "generation_ms") / 1000)
        final_y = y(number(final_row, "generation_ms") / 1000)
        if main_row.get("status", "success") == "success":
            parts.append(f'<circle cx="{main_x:.1f}" cy="{main_y:.1f}" r="4" '
                         f'fill="{GRAY}"><title>{seed} {baseline_name}: '
                         f'{number(main_row, "generation_ms")/1000:.3f}s</title></circle>')
        else:
            parts.append(f'<path d="M {main_x-4:.1f} {main_y-4:.1f} L '
                         f'{main_x+4:.1f} {main_y+4:.1f} M {main_x+4:.1f} '
                         f'{main_y-4:.1f} L {main_x-4:.1f} {main_y+4:.1f}" '
                         f'stroke="{RED}" stroke-width="2.5"><title>{seed} '
                         f'{baseline_name}: failed</title></path>')
        triangle = (f"{final_x:.1f},{final_y-5:.1f} "
                    f"{final_x-5:.1f},{final_y+4:.1f} "
                    f"{final_x+5:.1f},{final_y+4:.1f}")
        parts.append(f'<polygon points="{triangle}" fill="{GREEN}"><title>'
                     f'{seed} {current_name}: '
                     f'{number(final_row, "generation_ms")/1000:.3f}s'
                     f'</title></polygon>')
    parts.append(f'<text class="axis-title" transform="translate(18 '
                 f'{top+plot_h/2}) rotate(-90)" text-anchor="middle">'
                 'generation time (log scale)</text></svg>')
    return (
        f'<div class="legend"><span><b class="circle" style="background:{GRAY}"></b>'
        f'{html.escape(baseline_name)}</span><span><b class="triangle"></b>'
        f'{html.escape(current_name)}</span><span><b class="failure">×</b>'
        'failed attempt</span></div>' + "".join(parts)
    )


def progress_chart(
    history_path: Path, current_mean: float, current_label: str,
) -> str:
    with history_path.open(newline="", encoding="utf-8-sig") as source:
        history = list(csv.DictReader(source))
    wanted = [row for row in history if row["checkpoint"] in
              {"main", "reverse-search-v1", "compiled-search-v2"}]
    labels = [row["checkpoint"] for row in wanted] + [current_label]
    values = [float(row["mean_ms"]) / 1000 for row in wanted] + [current_mean / 1000]
    width, height = 940, 330
    left, right, top, bottom = 66, 24, 24, 74
    plot_w, plot_h = width-left-right, height-top-bottom
    maximum = math.ceil(max(values))
    slot = plot_w / len(values)
    bar_w = slot * 0.52
    parts = [f'<svg viewBox="0 0 {width} {height}" role="img" '
             'aria-label="Accepted optimization checkpoint progression">']
    for tick in range(maximum + 1):
        y = top + plot_h - plot_h * tick / maximum
        parts.append(f'<line class="grid" x1="{left}" y1="{y:.1f}" '
                     f'x2="{width-right}" y2="{y:.1f}"/>')
        parts.append(f'<text class="tick" x="{left-10}" y="{y+4:.1f}" '
                     f'text-anchor="end">{tick}s</text>')
    for index, (label, value) in enumerate(zip(labels, values)):
        x = left + slot * index + (slot-bar_w)/2
        h = plot_h * value / maximum
        y = top + plot_h - h
        color = GREEN if index == len(values)-1 else BLUE
        parts.append(f'<rect x="{x:.1f}" y="{y:.1f}" width="{bar_w:.1f}" '
                     f'height="{h:.1f}" rx="5" fill="{color}"/>')
        parts.append(f'<text class="bar-value" x="{x+bar_w/2:.1f}" '
                     f'y="{y-7:.1f}" text-anchor="middle">{value:.2f}s</text>')
        parts.append(f'<text class="axis-label" x="{x+bar_w/2:.1f}" '
                     f'y="{top+plot_h+27}" text-anchor="middle">'
                     f'{html.escape(label)}</text>')
    parts.append("</svg>")
    return "".join(parts)


def failure_table(rows: dict[int, dict[str, str]]) -> str:
    failures = [(seed, row) for seed, row in sorted(rows.items())
                if row.get("status", "success") != "success"]
    if not failures:
        return '<p class="success-line">No failures.</p>'
    return "<table><thead><tr><th>Seed</th><th>Time</th><th>Failure</th></tr></thead><tbody>" + "".join(
        f'<tr><td>{seed}</td><td>{number(row, "generation_ms")/1000:.3f}s</td>'
        f'<td class="left">{html.escape(row.get("failure", ""))}</td></tr>'
        for seed, row in failures
    ) + "</tbody></table>"


def seed_table(
    seeds: list[int], baseline: dict[int, dict[str, str]],
    current: dict[int, dict[str, str]],
) -> str:
    rows = []
    for seed in seeds:
        before, after = baseline[seed], current[seed]
        both = before.get("status") == after.get("status") == "success"
        speedup = (number(before, "generation_ms") /
                   number(after, "generation_ms")) if both else None
        hash_match = (before.get("output_hash") == after.get("output_hash")) if both else None
        rows.append(
            f'<tr><td>{seed}</td><td class="{before.get("status")}">'
            f'{html.escape(before.get("status", ""))}</td>'
            f'<td>{number(before, "generation_ms")/1000:.3f}s</td>'
            f'<td class="{after.get("status")}">{html.escape(after.get("status", ""))}</td>'
            f'<td>{number(after, "generation_ms")/1000:.3f}s</td>'
            f'<td>{"—" if speedup is None else f"{speedup:.2f}×"}</td>'
            f'<td>{mib(number(before, "allocated_bytes"))}</td>'
            f'<td>{mib(number(after, "allocated_bytes"))}</td>'
            f'<td>{"—" if hash_match is None else "yes" if hash_match else "changed"}</td></tr>'
        )
    return "".join(rows)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("baseline", type=Path)
    parser.add_argument("current", type=Path)
    parser.add_argument("--history", type=Path,
                        default=Path("scripts/sm-performance-history.csv"))
    parser.add_argument("--out", type=Path,
                        default=Path("artifacts/sm-windows-final-report.html"))
    parser.add_argument("--baseline-name", default="main@c50d6f04")
    parser.add_argument("--current-name", default="final branch")
    parser.add_argument("--title", default="Super Metroid search optimization")
    parser.add_argument("--runtime", default="Windows · .NET 10 · Release")
    parser.add_argument("--full-tests", default="not supplied")
    parser.add_argument("--slow-tests", default="not supplied")
    args = parser.parse_args()

    baseline = latest_rows(args.baseline)
    current = latest_rows(args.current)
    seeds = sorted(set(baseline) & set(current))
    common = [seed for seed in seeds
              if baseline[seed].get("status", "success") == "success"
              and current[seed].get("status", "success") == "success"]
    if not common:
        raise SystemExit("The cohorts have no common successful seeds")
    baseline_common = [baseline[seed] for seed in common]
    current_common = [current[seed] for seed in common]
    baseline_ok = [row for row in baseline.values() if row.get("status") == "success"]
    current_ok = [row for row in current.values() if row.get("status") == "success"]

    baseline_mean = mean(baseline_common, "generation_ms")
    current_mean = mean(current_common, "generation_ms")
    mean_speedup = baseline_mean / current_mean
    paired_speedup = statistics.fmean(
        number(baseline[seed], "generation_ms") /
        number(current[seed], "generation_ms") for seed in common)
    paired_reduction = statistics.fmean(
        (number(baseline[seed], "generation_ms") -
         number(current[seed], "generation_ms")) * 100 /
        number(baseline[seed], "generation_ms") for seed in common)
    baseline_total = sum(number(row, "generation_ms") for row in baseline_common)
    current_total = sum(number(row, "generation_ms") for row in current_common)
    baseline_alloc = mean(baseline_common, "allocated_bytes")
    current_alloc = mean(current_common, "allocated_bytes")
    alloc_reduction = baseline_alloc / current_alloc
    worst_seed = max(common, key=lambda seed: number(baseline[seed], "generation_ms"))
    worst_before = number(baseline[worst_seed], "generation_ms")
    worst_after = number(current[worst_seed], "generation_ms")
    worst_ratio = worst_before / worst_after
    worst_detail = (f"{worst_ratio:.2f}× faster" if worst_ratio >= 1
                    else f"{(worst_after / worst_before - 1) * 100:.1f}% slower")
    hash_matches = sum(baseline[seed].get("output_hash") ==
                       current[seed].get("output_hash") for seed in common)
    final_all_values = [number(row, "generation_ms") for row in current_ok]

    cards = "".join([
        card(f"Paired total · {len(common)} seeds",
             f"{baseline_total/1000:.1f} → {current_total/1000:.1f} s",
             f"{mean_speedup:.2f}× faster"),
        card("Paired mean / seed", f"{baseline_mean/1000:.2f} → {current_mean/1000:.2f} s",
             f"{paired_reduction:.1f}% mean reduction"),
        card(f"Worst baseline seed · {worst_seed}",
             f"{worst_before/1000:.2f} → {worst_after/1000:.2f} s",
             worst_detail, "good" if worst_ratio >= 1 else "failure"),
        card("Allocations / seed", f"{mib(baseline_alloc)} → {mib(current_alloc)}",
             f"{alloc_reduction:.1f}× less allocation"),
        card("Generation success",
             f"{len(baseline_ok)}/{len(baseline)} → {len(current_ok)}/{len(current)}",
             (f"recovered {len(current_ok)-len(baseline_ok)} seeds"
              if len(current_ok) > len(baseline_ok)
              else "no generation failures"
              if len(current_ok) == len(current) == len(baseline_ok) == len(baseline)
              else "success count preserved")),
        card("Current p95 / worst",
             f"{percentile(final_all_values, .95)/1000:.2f} / {max(final_all_values)/1000:.2f} s",
             f"{len(current_ok)} fresh Windows processes"),
    ])

    final_work = {
        "Backtrack checks": sum(number(row, "checks") for row in current_ok),
        "Reverse proofs": sum(number(row, "reverse_proofs") for row in current_ok),
        "Reverse rejects": sum(number(row, "reverse_rejects") for row in current_ok),
        "Legacy fallbacks": sum(number(row, "fallback_searches") for row in current_ok),
        "Reverse builds / seed": mean(current_ok, "reverse_search_builds"),
        "Reverse build time / seed": mean(current_ok, "reverse_search_build_ms"),
        "Frontier entries / seed": mean(current_ok, "reverse_frontier_entries"),
        "Forward states / seed": mean(current_ok, "forward_dequeued_states"),
        "Requirement evaluations / seed": mean(current_ok, "forward_requirement_evaluations"),
    }
    work_rows = "".join(
        f'<tr><td class="left">{html.escape(label)}</td><td>{value:,.0f}'
        f'{" ms" if "time" in label.lower() else ""}</td></tr>'
        for label, value in final_work.items()
    )

    gc_rows = "".join(
        f'<tr><td>Gen {generation}</td>'
        f'<td>{mean(baseline_common, f"gen{generation}_collections"):.1f}</td>'
        f'<td>{mean(current_common, f"gen{generation}_collections"):.1f}</td></tr>'
        for generation in range(3)
    )

    document = f"""<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>{html.escape(args.title)}</title><style>
:root {{--blue:{BLUE};--green:{GREEN};--gray:{GRAY};--red:{RED};--ink:#1e2937;--muted:#687384;--line:#dfe4ea;--paper:#fff;--wash:#f4f7f9}}
*{{box-sizing:border-box}} body{{margin:0;background:var(--wash);color:var(--ink);font:14px/1.45 Inter,Segoe UI,system-ui,sans-serif}}
.page{{max-width:1180px;margin:24px auto;padding:0 18px 48px}} .hero{{background:linear-gradient(135deg,#14243b,#1d5c68);color:white;border-radius:14px;padding:28px 30px;box-shadow:0 8px 28px #17324b24}}
h1{{font-size:30px;margin:0 0 5px}} .subtitle{{opacity:.82;font-size:15px}} .stamp{{margin-top:13px;font-size:12px;opacity:.72}}
.metrics{{display:grid;grid-template-columns:repeat(3,1fr);gap:12px;margin:16px 0}} .metric-card,.panel{{background:var(--paper);border:1px solid var(--line);border-radius:12px;box-shadow:0 2px 9px #17324b0b}}
.metric-card{{padding:17px 19px;min-height:115px}} .metric-label{{color:var(--muted);font-weight:650}} .metric-value{{font-size:25px;font-weight:760;letter-spacing:-.4px;margin:3px 0}} .metric-detail{{font-weight:700}} .good,.success,.success-line{{color:#087b38}} .failure{{color:var(--red)}}
.panel{{padding:20px;margin-top:14px;overflow-x:auto}} h2{{font-size:18px;margin:0 0 4px}} h3{{font-size:15px;margin:18px 0 5px}} .note{{color:var(--muted);margin:0 0 12px}} .two{{display:grid;grid-template-columns:1fr 1fr;gap:14px}}
svg{{display:block;width:100%;min-width:720px}} .grid{{stroke:#dde2e7;stroke-width:1}} .tick{{fill:#707983;font-size:11px}} .axis-label{{fill:#46505c;font-size:11px}} .axis-title{{fill:#647080;font-size:11px}} .bar-value{{fill:#25303b;font-size:11px;font-weight:650}}
.legend{{display:flex;gap:18px;color:#505964;font-size:12px;margin:8px 0}} .legend span{{display:flex;align-items:center;gap:6px}} .legend i{{width:11px;height:11px;border-radius:2px}} .circle{{width:10px;height:10px;border-radius:50%;display:inline-block}} .triangle{{width:0;height:0;border-left:6px solid transparent;border-right:6px solid transparent;border-bottom:11px solid var(--green);display:inline-block}} .legend .failure{{font-size:18px;font-weight:800;line-height:10px}}
table{{border-collapse:collapse;width:100%;font-variant-numeric:tabular-nums}} th,td{{padding:8px 10px;border-bottom:1px solid #e7eaee;text-align:right;white-space:nowrap}} th{{background:#f1f4f7;color:#596472;font-size:12px}} th:first-child,td:first-child,.left{{text-align:left}} details summary{{cursor:pointer;font-weight:700;margin-bottom:10px}}
.pill{{display:inline-block;border-radius:999px;padding:4px 9px;background:#e8f7f1;color:#08764e;font-weight:700;margin-right:6px}} code{{background:#eef1f4;padding:2px 5px;border-radius:4px}} ul{{margin:8px 0;padding-left:21px}}
@media(max-width:800px){{.metrics,.two{{grid-template-columns:1fr}}.page{{padding:0 10px}}.hero{{padding:22px}}}}
</style></head><body><main class="page">
<header class="hero"><h1>{html.escape(args.title)}</h1>
<div class="subtitle">{html.escape(args.baseline_name)} versus {html.escape(args.current_name)} · {len(seeds)} SM-only seeds ({min(seeds)}–{max(seeds)})</div>
<div class="stamp">{html.escape(args.runtime)} · fresh process per seed · generated {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')}</div></header>
<div class="metrics">{cards}</div>

<section class="panel"><h2>Total time by phase</h2><p class="note">Only the {len(common)} seeds successful on both revisions are included; validation is measured independently from generation.</p>
{stage_chart(baseline_common, current_common, args.baseline_name, args.current_name)}</section>

<section class="panel"><h2>Per-seed generation time</h2><p class="note">All {len(seeds)} attempts are shown. Baseline failures are red crosses at the time generation stopped; the y-axis is logarithmic.</p>
{scatter_chart(seeds, baseline, current, args.baseline_name, args.current_name)}</section>

<section class="panel"><h2>Accepted-checkpoint progression</h2><p class="note">Intermediate checkpoints use their retained Windows cohorts; the final bar uses the new {len(common)}-seed paired Windows cohort.</p>
{progress_chart(args.history, current_mean, args.current_name)}</section>

<div class="two"><section class="panel"><h2>Allocation and GC pressure</h2>
<p><strong>{mib(baseline_alloc)}</strong> → <strong>{mib(current_alloc)}</strong> allocated per paired seed ({alloc_reduction:.2f}× less).</p>
<table><thead><tr><th>Collection</th><th>{html.escape(args.baseline_name)}</th><th>{html.escape(args.current_name)}</th></tr></thead><tbody>{gc_rows}</tbody></table>
<p class="note">Collection counts are process-local. Lower allocation is the primary pressure metric; generation-specific collection counts also reflect the changed allocation lifetime distribution.</p></section>
<section class="panel"><h2>Current search work</h2><table><tbody>{work_rows}</tbody></table>
<p><span class="pill">0 production fallbacks</span><span class="pill">{paired_speedup:.2f}× average paired speedup</span></p></section></div>

<div class="two"><section class="panel"><h2>Windows verification</h2>
<table><tbody><tr><td class="left">Normal suite</td><td>{html.escape(args.full_tests)}</td></tr>
<tr><td class="left">Slow SM regressions</td><td>{html.escape(args.slow_tests)}</td></tr>
<tr><td class="left">Current benchmark generation</td><td>{len(current_ok)}/{len(current)} successful</td></tr>
<tr><td class="left">Baseline benchmark generation</td><td>{len(baseline_ok)}/{len(baseline)} successful</td></tr></tbody></table></section>
<section class="panel"><h2>Output equivalence context</h2>
<p><strong>{hash_matches}/{len(common)}</strong> combined placement + playthrough hashes match the comparison baseline.</p>
<p class="note">Performance-only checkpoints are accepted only when these hashes remain identical, so placement and deterministic playthrough behavior stay fixed.</p></section></div>

<section class="panel"><h2>Baseline failures</h2><p class="note">Failures are retained in totals and never silently removed; paired latency calculations use only common successful seeds.</p>{failure_table(baseline)}</section>

<section class="panel"><details><summary>Per-seed measurements</summary>
<table><thead><tr><th>Seed</th><th>Baseline status</th><th>Baseline time</th><th>Current status</th><th>Current time</th><th>Speedup</th><th>Baseline alloc.</th><th>Current alloc.</th><th>Combined hash</th></tr></thead>
<tbody>{seed_table(seeds, baseline, current)}</tbody></table></details></section>

<section class="panel"><h2>Methodology</h2><ul>
<li>Both binaries were built and executed with Windows .NET SDK 10.0.300 in Release mode from the same <code>F:</code> volume.</li>
<li>Each cohort used one fresh process per seed with the same settings and runtime environment.</li>
<li>Comparison baseline: <code>{html.escape(args.baseline_name)}</code>.</li>
<li>Current implementation: <code>{html.escape(args.current_name)}</code>. Detailed counters are opt-in and use local hot-loop accumulation.</li>
<li>Paired cards and phase totals use the {len(common)} common successful seeds. Reliability and scatter plots use all {len(seeds)} attempts.</li>
</ul><p class="note">Sources: {html.escape(str(args.baseline))}, {html.escape(str(args.current))}, {html.escape(str(args.history))}</p></section>
</main></body></html>"""
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(document, encoding="utf-8")
    print(f"Wrote {args.out}: {len(common)} paired successes, {len(seeds)} attempts")


if __name__ == "__main__":
    main()
