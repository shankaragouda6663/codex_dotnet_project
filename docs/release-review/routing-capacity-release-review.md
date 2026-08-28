# Routing-Capacity Parallel Release Review Framework

This repository includes a five-lane release-review framework for the routing-capacity patch.

## Scripts

1. `scripts/release-review/prepare-review.ps1`
- Resolves immutable candidate identity (`BASE_SHA`, `HEAD_SHA`), patch hash, and stack detection.
- Produces `manifest.json` with lane paths and diff evidence.

2. `scripts/release-review/create-worktrees.ps1`
- Creates five isolated worktrees pinned to `HEAD_SHA`.
- Verifies lane SHA and patch hash consistency.

3. `scripts/release-review/run-specialist-review.ps1`
- Runs one bounded specialist lane using schema-constrained output.
- Uses reviewer template and lane-specific artifacts.

4. `scripts/release-review/run-five-reviewers.ps1`
- Runs exactly five specialists in parallel (or sequential fallback).

5. `scripts/release-review/consolidate-release-review.ps1`
- Generates final report skeleton with required release sections.
- Collects critical/high ownership from specialist outputs.

6. `scripts/release-review/cleanup-worktrees.ps1`
- Removes only recorded reviewer worktrees.

## Safety Design

- All specialist execution paths are read-only by default.
- Reviewer outputs are schema constrained.
- No write-capable MCP operations are invoked by framework scripts.
- Worktree and artifact paths are lane-bound and manifest-backed.

## Example Flow

```powershell
pwsh -NoProfile -File scripts/release-review/prepare-review.ps1 -BaseRef <BASE_REF> -HeadRef HEAD
pwsh -NoProfile -File scripts/release-review/create-worktrees.ps1 -ManifestPath artifacts/release-review/<review-id>/manifest.json
pwsh -NoProfile -File scripts/release-review/run-five-reviewers.ps1 -ManifestPath artifacts/release-review/<review-id>/manifest.json
pwsh -NoProfile -File scripts/release-review/consolidate-release-review.ps1 -ManifestPath artifacts/release-review/<review-id>/manifest.json
pwsh -NoProfile -File scripts/release-review/cleanup-worktrees.ps1 -ManifestPath artifacts/release-review/<review-id>/manifest.json
```

## MCP Audit Expectations

Each specialist output must include:
- source/tool used,
- retrieval timestamp,
- object ID,
- environment,
- limitations,
- read-only operation assurance.

If MCP is unavailable, mark external evidence as `UNVERIFIED`.
