# Code-Flow Global Skill

## Purpose
Create consistent, high-signal code-flow and runtime architecture diagrams across repositories, independent of language or framework.

## When To Use
- User asks for code-flow, runtime flow, architecture flow, or request lifecycle visualization.
- User asks to convert implementation details into a diagram with trust boundaries.
- User asks for a primary path plus concise supporting context.

## Inputs
- Repository source files.
- Existing architecture docs (if present).
- Optional user constraints (component count, boundaries, path focus, format).

## Outputs
- One diagram artifact with:
  - 8-12 core components by default.
  - Exactly one primary path unless user explicitly requests more.
  - External dependencies and trust boundaries.
  - Supporting detail in cards/notes, not extra edges.
- A short evidence section naming source files used for inference.

## Required Rules
1. Keep the edge set sparse. If a relationship is not critical to the primary path, move it to a component card.
2. Use stable component names from code, not invented platform names.
3. Mark trust boundaries explicitly:
   - Client/Public
   - Application Runtime
   - Data Plane
   - External/Partner
4. Separate synchronous and asynchronous transitions in labels.
5. Prefer runtime truth from `Program.cs`, DI registration, job schedulers, connectors, repositories, and workers.
6. If code and docs conflict, code wins.
7. Include one "primary path" section with ordered steps.
8. Avoid speculative components unless clearly labeled `assumed`.

## Diagram Standards
- Direction: left-to-right for request entry to persistence/event publication.
- Cardinality: one node per deployable or major runtime responsibility.
- Labels: action-oriented (`Validate request`, `Persist order`, `Publish event`).
- Styling: legible in light and dark themes.

## Validation Checklist
- Component count in range (8-12 unless overridden).
- Exactly one primary path.
- Trust boundaries present.
- External dependencies present.
- Supporting detail moved into cards.
- All major path nodes map to real source files.

## Failure Handling
- If repository evidence is insufficient:
  - Produce a minimal draft with explicit `assumed` markers.
  - List missing files needed for a definitive diagram.

## Preferred Rendering
- Use `archify` for standalone explorable output.
- Store artifacts under `docs/architecture/`.
