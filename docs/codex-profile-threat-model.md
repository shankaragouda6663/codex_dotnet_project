# RxFlow Codex Profile Threat Model

## Assets

- Source code and tests inside repository worktree.
- Build outputs and temporary artifacts.
- User credentials and Codex auth files under `%CODEX_HOME%`.
- CI transcripts and schema outputs.

## Trust Boundaries

1. Repository content is untrusted input to profile scripts.
2. User/global managed Codex config is higher precedence than repository intent.
3. External network and model-provider access must be explicit and controlled.
4. Remediation patching must occur in isolated disposable workspace.

## Threats and Controls

### T1: Read-only profile writes files
- Control: `sandbox_mode=read-only` and negative write tests in harness.

### T2: Developer/remediator writes outside approved scope
- Control: `invoke-profile-command.ps1` resolves absolute paths and rejects out-of-scope targets.

### T3: Destructive/publish commands bypass intent
- Control: command-pattern deny list in wrapper (`git reset --hard`, `dotnet publish`, `dotnet nuget push`, IaC/deploy commands).

### T4: CI waits for approval/input
- Control: `approval_policy=never`, timeout in `run-ci-analysis.ps1`, JSONL artifact retention.

### T5: Remediator contaminates primary workspace
- Control: `create-remediation-worktree.ps1` isolated workspace with marker + state; `cleanup-remediation-worktree.ps1` validates root and marker.

### T6: Sensitive data leaks in logs
- Control: verification output redaction checks and no credential file reads in success path.

## Residual Risks

- Managed policy precedence validation requires enterprise control-plane visibility and is marked `UNVERIFIED` in local repo tests.
- Full CI non-interactive behavior requires authorized codex exec execution in CI context.
