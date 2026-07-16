#!/usr/bin/env python3
"""Build a self-contained HTML comparison report from SM backtracking metrics CSV."""

from __future__ import annotations

import argparse
import csv
import html
import statistics
from collections import defaultdict
from pathlib import Path


def number(row: dict[str, str], key: str) -> float:
    return float(row.get(key, "0") or 0)


def percentile(values: list[float], fraction: float) -> float:
    ordered = sorted(values)
    if not ordered:
        return 0
    position = (len(ordered) - 1) * fraction
    lower = int(position)
    upper = min(lower + 1, len(ordered) - 1)
    weight = position - lower
    return ordered[lower] * (1 - weight) + ordered[upper] * weight


def aggregate(label: str, rows: list[dict[str, str]]) -> dict[str, float | str]:
    generation = [number(row, "generation_ms") for row in rows]
    checks = sum(number(row, "checks") for row in rows)
    structural = sum(number(row, "structural_rejects") for row in rows)
    proofs = sum(number(row, "reverse_proofs") for row in rows)
    rejects = sum(number(row, "reverse_rejects") for row in rows)
    fallbacks = sum(number(row, "fallback_searches") for row in rows)
    return {
        "label": label,
        "mode": rows[0].get("mode", "unknown"),
        "seeds": len(rows),
        "mean_ms": statistics.fmean(generation),
        "median_ms": statistics.median(generation),
        "p95_ms": percentile(generation, 0.95),
        "min_ms": min(generation),
        "max_ms": max(generation),
        "graph_ms": statistics.fmean(number(row, "graph_construction_ms") for row in rows),
        "fill_ms": statistics.fmean(number(row, "assumed_fill_ms") for row in rows),
        "spoiler_ms": statistics.fmean(number(row, "spoiler_ms") for row in rows),
        "validation_ms": statistics.fmean(number(row, "validation_ms") for row in rows),
        "allocated_mb": statistics.fmean(number(row, "allocated_bytes") for row in rows)
        / (1024 * 1024),
        "gen0": statistics.fmean(number(row, "gen0_collections") for row in rows),
        "gen1": statistics.fmean(number(row, "gen1_collections") for row in rows),
        "gen2": statistics.fmean(number(row, "gen2_collections") for row in rows),
        "checks": checks,
        "resolved_pct": 0
        if checks == 0
        else (structural + proofs + rejects) * 100 / checks,
        "proof_pct": 0 if checks == 0 else proofs * 100 / checks,
        "reject_pct": 0 if checks == 0 else rejects * 100 / checks,
        "fallback_pct": 0 if checks == 0 else fallbacks * 100 / checks,
        "fallback_ms": sum(number(row, "fallback_ms") for row in rows),
        "fallback_states": sum(number(row, "fallback_states") for row in rows),
        "reverse_builds": sum(number(row, "reverse_search_builds") for row in rows),
        "reverse_build_ms": sum(number(row, "reverse_search_build_ms") for row in rows),
        "frontier_entries": sum(number(row, "reverse_frontier_entries") for row in rows),
    }


def bar_chart(
    summaries: list[dict[str, float | str]], key: str, title: str, suffix: str
) -> str:
    width, height = 820, 300
    left, right, top, bottom = 70, 20, 42, 70
    plot_width = width - left - right
    plot_height = height - top - bottom
    values = [float(summary[key]) for summary in summaries]
    maximum = max(values, default=1) or 1
    slot = plot_width / max(len(summaries), 1)
    bar_width = slot * 0.62
    colors = ["#4f8cff", "#48c78e", "#f5a623", "#e76f91", "#9b7ede"]
    parts = [
        f'<svg viewBox="0 0 {width} {height}" role="img" aria-label="{html.escape(title)}">',
        f'<text x="{left}" y="25" class="chart-title">{html.escape(title)}</text>',
        f'<line x1="{left}" y1="{top + plot_height}" x2="{width-right}" '
        f'y2="{top + plot_height}" class="axis"/>',
    ]
    for index, (summary, value) in enumerate(zip(summaries, values)):
        bar_height = plot_height * value / maximum
        x = left + index * slot + (slot - bar_width) / 2
        y = top + plot_height - bar_height
        label = str(summary["label"])
        parts.extend(
            [
                f'<rect x="{x:.1f}" y="{y:.1f}" width="{bar_width:.1f}" '
                f'height="{bar_height:.1f}" fill="{colors[index % len(colors)]}" rx="3"/>',
                f'<text x="{x + bar_width/2:.1f}" y="{max(y-7, top+12):.1f}" '
                f'class="value" text-anchor="middle">{value:.1f}{suffix}</text>',
                f'<text x="{x + bar_width/2:.1f}" y="{top + plot_height + 22}" '
                f'class="label" text-anchor="middle">{html.escape(label)}</text>',
            ]
        )
    parts.append("</svg>")
    return "".join(parts)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("csv", type=Path, help="metrics CSV emitted by the randomizer")
    parser.add_argument("--out", type=Path, default=Path("sm-backtracking-report.html"))
    parser.add_argument("--baseline", help="label to use as the performance baseline")
    parser.add_argument("--title", default="Super Metroid backtracking performance")
    args = parser.parse_args()

    with args.csv.open(newline="", encoding="utf-8") as source:
        rows = list(csv.DictReader(source))
    if not rows:
        raise SystemExit("Metrics CSV has no data rows")

    # Keep the latest sample for a label/seed pair and compare only the common
    # seed intersection, preventing a lucky seed mix from looking like a speedup.
    latest: dict[tuple[str, str], dict[str, str]] = {}
    label_order: list[str] = []
    for row in rows:
        label = row["label"]
        if label not in label_order:
            label_order.append(label)
        latest[(label, row["seed"])] = row

    baseline = args.baseline or label_order[0]
    if baseline not in label_order:
        raise SystemExit(f"Baseline label {baseline!r} is not present in the CSV")
    labels = [baseline, *[label for label in label_order if label != baseline]]
    successful = {
        key: row for key, row in latest.items()
        if row.get("status", "success") == "success"
    }
    seed_sets = [{seed for label, seed in successful if label == current} for current in labels]
    common_seeds = set.intersection(*seed_sets)
    if not common_seeds:
        raise SystemExit("Labels have no common seeds to compare")

    grouped: dict[str, list[dict[str, str]]] = defaultdict(list)
    for label in labels:
        grouped[label] = [successful[(label, seed)] for seed in sorted(common_seeds, key=int)]
    summaries = [aggregate(label, grouped[label]) for label in labels]
    baseline_mean = float(summaries[0]["mean_ms"])
    baseline_rows = {row["seed"]: row for row in grouped[baseline]}

    table_rows = []
    for summary in summaries:
        mean_ms = float(summary["mean_ms"])
        current_rows = {row["seed"]: row for row in grouped[str(summary["label"])]}
        paired_speedup = statistics.fmean(
            number(baseline_rows[seed], "generation_ms")
            / number(current_rows[seed], "generation_ms")
            for seed in common_seeds
        )
        paired_gain = statistics.fmean(
            (number(baseline_rows[seed], "generation_ms")
             - number(current_rows[seed], "generation_ms"))
            * 100 / number(baseline_rows[seed], "generation_ms")
            for seed in common_seeds
        )
        hash_mismatches = sum(
            bool(baseline_rows[seed].get("output_hash"))
            and baseline_rows[seed].get("output_hash")
            != current_rows[seed].get("output_hash")
            for seed in common_seeds
        )
        failed_seeds = sorted(
            (seed for (label, seed), row in latest.items()
             if label == summary["label"] and row.get("status", "success") != "success"),
            key=int,
        )
        table_rows.append(
            "<tr>"
            f"<td>{html.escape(str(summary['label']))}</td>"
            f"<td>{html.escape(str(summary['mode']))}</td>"
            f"<td>{int(summary['seeds'])}</td>"
            f"<td>{mean_ms:.1f}</td>"
            f"<td>{float(summary['median_ms']):.1f}</td>"
            f"<td>{float(summary['p95_ms']):.1f}</td>"
            f"<td>{paired_speedup:.2f}×</td>"
            f"<td>{paired_gain:+.1f}%</td>"
            f"<td>{float(summary['graph_ms']):.1f}</td>"
            f"<td>{float(summary['fill_ms']):.1f}</td>"
            f"<td>{float(summary['spoiler_ms']):.1f}</td>"
            f"<td>{float(summary['validation_ms']):.1f}</td>"
            f"<td>{float(summary['allocated_mb']):.1f}</td>"
            f"<td>{float(summary['gen0']):.1f}/{float(summary['gen1']):.1f}/{float(summary['gen2']):.1f}</td>"
            f"<td>{hash_mismatches}</td>"
            f"<td>{html.escape(', '.join(failed_seeds) or '—')}</td>"
            f"<td>{float(summary['resolved_pct']):.1f}%</td>"
            f"<td>{float(summary['proof_pct']):.1f}%</td>"
            f"<td>{float(summary['reject_pct']):.1f}%</td>"
            f"<td>{float(summary['fallback_pct']):.1f}%</td>"
            f"<td>{float(summary['fallback_ms']):.1f}</td>"
            f"<td>{int(summary['fallback_states'])}</td>"
            f"<td>{int(summary['reverse_builds'])}</td>"
            f"<td>{float(summary['reverse_build_ms']):.1f}</td>"
            f"<td>{int(summary['frontier_entries'])}</td>"
            "</tr>"
        )

    document = f"""<!doctype html>
<html lang="en"><head><meta charset="utf-8"><title>{html.escape(args.title)}</title>
<style>
body {{ font: 14px system-ui, sans-serif; margin: 2rem; color: #20242c; background: #f7f8fb; }}
h1 {{ margin-bottom: .25rem; }} .note {{ color: #5d6470; }}
.card {{ background: white; border: 1px solid #dfe3eb; border-radius: 8px; padding: 1rem; margin: 1rem 0; overflow-x: auto; }}
table {{ border-collapse: collapse; width: 100%; }} th, td {{ padding: .55rem; border-bottom: 1px solid #e8eaf0; text-align: right; white-space: nowrap; }}
th:first-child, td:first-child {{ text-align: left; }} th {{ background: #f1f4f9; }}
svg {{ width: 100%; min-width: 620px; }} .axis {{ stroke: #89919e; }}
.chart-title {{ font-size: 16px; font-weight: 650; }} .value {{ font-size: 12px; }} .label {{ font-size: 12px; }}
</style></head><body>
<h1>{html.escape(args.title)}</h1>
<p class="note">Baseline: <strong>{html.escape(baseline)}</strong>. Comparing {len(common_seeds)} identical seeds per label.</p>
<div class="card"><table><thead><tr>
<th>Label</th><th>Mode</th><th>Seeds</th><th>Mean ms</th><th>Median ms</th><th>P95 ms</th>
<th>Paired speedup</th><th>Paired gain</th><th>Graph ms</th><th>Fill ms</th>
<th>Spoiler ms</th><th>Validation ms</th><th>Allocated MiB</th><th>GC 0/1/2</th>
<th>Hash mismatches</th><th>Failures</th><th>No fallback</th><th>Reverse proof</th>
<th>Reverse reject</th><th>Fallback rate</th><th>Fallback ms</th><th>Fallback states</th>
<th>Reverse builds</th><th>Reverse build ms</th><th>Frontier entries</th>
</tr></thead><tbody>{''.join(table_rows)}</tbody></table></div>
<div class="card">{bar_chart(summaries, 'mean_ms', 'Mean full seed generation time', ' ms')}</div>
<div class="card">{bar_chart(summaries, 'reverse_build_ms', 'Total reverse traversal build time', ' ms')}</div>
<div class="card">{bar_chart(summaries, 'resolved_pct', 'Backtrack checks resolved without fallback', '%')}</div>
<p class="note">Generated from {html.escape(str(args.csv))}. Lower generation time and fallback rate are better.</p>
</body></html>"""
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(document, encoding="utf-8")
    print(f"Wrote {args.out} ({len(common_seeds)} common seeds, {len(labels)} labels)")


if __name__ == "__main__":
    main()
