# RxFlow Repository Instructions

This file defines durable repository standards. More specific `AGENTS.md` files in child directories may add constraints.

## Scope Mapping

- `order_api` -> `RxFlow.Api`
- `routing` -> `RxFlow.Application/Labs`
- `pricing` -> `RxFlow.Application/Pricing`
- `workers` -> `RxFlow.Workers`
- `analytics` -> `RxFlow.Application/Reporting`
- `infra` -> `RxFlow.Infrastructure`

## Durable Rules

- [RULE-1] Payment-path changes must include idempotency and duplicate-submission tests. See `docs/engineering/testing-standards.md`.
- [RULE-2] Database changes must include upgrade instructions, rollback or forward-fix instructions, and migration validation evidence. See `docs/engineering/database-migrations.md`.
- [RULE-4] New external calls must define explicit timeout and retry policy. See `docs/engineering/external-call-resilience.md`.
- [RULE-5] Sensitive data (patient identifiers, prescription values, credentials, tokens) must never be logged. See `docs/engineering/sensitive-data-and-observability.md`.
- [RULE-7] Every defect fix must include a regression test that fails before and passes after the correction.
- [RULE-8] Final reports must include commands executed, exit codes, and concise results. See `docs/engineering/final-evidence-reporting.md`.

## Repository-Verified Commands

- `dotnet restore RxFlowLab.slnx --configfile NuGet.Config`
- `dotnet build RxFlowLab.slnx --no-restore`
- `dotnet test RxFlowLab.slnx --no-build`
- `dotnet format RxFlowLab.slnx --verify-no-changes`

Use these command shapes unless a scoped instruction file states a narrower command for that component.
