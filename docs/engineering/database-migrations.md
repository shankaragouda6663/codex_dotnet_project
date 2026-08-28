# Database Migrations

## Required

- Every schema change must include:
  - upgrade steps,
  - rollback plan or explicit forward-fix plan,
  - migration validation evidence.
- Keep migration names descriptive and ordered.

## Validation

- Validate migration list and execution in isolated local environment.
- Record commands, exit codes, and short results.
