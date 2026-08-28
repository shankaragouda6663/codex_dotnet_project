# Code-Flow Module Overlay Template

## Purpose
Specialize the global code-flow skill for one bounded module without changing the global standards.

## Module Metadata (Fill In)
- `module_name`: `<module>`
- `module_root_paths`: `<path1>`, `<path2>`
- `entrypoints`: `<api endpoints/jobs/events>`
- `core_services`: `<service names>`
- `data_stores`: `<db/cache/topic>`
- `external_dependencies`: `<partner APIs/systems>`
- `sensitive_data_tags`: `<pii/phi/payment/etc>`

## Scope Rules
1. Only include components that materially affect this module's runtime behavior.
2. Reuse global trust boundaries; add module sub-boundaries only if they reduce ambiguity.
3. Keep the global edge budget discipline; use cards for detail.

## Module-Specific Extraction Steps
1. Find entrypoints under `module_root_paths`.
2. Trace DI registrations and interface bindings to runtime implementations.
3. Trace persistence and event publication points.
4. Trace outbound connectors/SDK calls and retry semantics.
5. Trace background processing handoffs (queues/jobs/schedulers).

## Module Diagram Contract
- 6-10 components typical for a focused module.
- One primary path from module entrypoint to terminal side effect.
- Optional 1-3 supporting edges for non-primary dependencies.
- Component cards must include:
  - Responsibility
  - Key files
  - Risk or reliability note

## Security/Resilience Notes
- Call out where untrusted input crosses into trusted runtime.
- Call out retry behavior, timeouts, and idempotency risks.
- Call out raw SQL or dynamic query paths if present.

## Evidence Block Template
- `evidence_files`:
  - `<abs-or-repo-relative-path-1>`
  - `<abs-or-repo-relative-path-2>`
  - `<abs-or-repo-relative-path-3>`

## Output Location
- `docs/architecture/<module-name>-runtime.archify.md`
