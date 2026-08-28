# Testing Standards

## Required

- For payment-path changes, include idempotency and duplicate-submission coverage.
- For defect fixes, add a regression test that fails on old behavior and passes after the fix.
- Do not weaken assertions to force green tests.

## .NET Notes

- Primary test framework is xUnit in `RxFlow.Tests`.
- Prefer deterministic tests (no sleeps, no wall-clock coupling).
