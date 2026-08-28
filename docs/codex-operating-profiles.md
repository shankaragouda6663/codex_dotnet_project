# RxFlow Codex Operating Profiles

## Scope

This repository defines five operating profiles for Codex usage:
- `rx-analyst`
- `rx-developer`
- `rx-security`
- `rx-ci`
- `rx-remediator`

Profiles are implemented using:
1. user-scoped Codex profile files (`%CODEX_HOME%\<name>.config.toml`),
2. repository command policy rules,
3. wrapper scripts that enforce path and command boundaries,
4. verification harness tests and machine-readable evidence.

## Codex Contract Baseline

Validated against `codex-cli 0.150.1`.

Verified config keys in this implementation:
- `sandbox_mode`
- `approval_policy`
- `search`
- `sandbox_permissions`
- `shell_environment_policy.*`

## Important Limitations

- `--profile` loads `%CODEX_HOME%\<name>.config.toml`, so repository files alone cannot directly register named profiles.
- Narrow writable roots are enforced by repository wrappers (`invoke-profile-command.ps1`) because `writable_roots` is not recognized as a top-level config field in this contract.
- CI non-interactive model-run evidence is implemented but may be marked `UNVERIFIED` unless explicitly executed in an authorized environment.

## Profile Matrix

Machine-readable matrix: `.codex/profile-matrix.json`.

## Files

- `.codex/config.profiles.example.toml`
- `.codex/profiles/*.config.toml`
- `.codex/rules/rxflow-operating-profiles.rules`
- `scripts/codex-profiles/*.ps1`
- `tests/codex-profiles/fixtures/*`

## Installation

Run:

```powershell
pwsh -NoProfile -File scripts/codex-profiles/install-profiles.ps1
```

This installs profile files into `%CODEX_HOME%`.

## Verification

Run:

```powershell
pwsh -NoProfile -File scripts/codex-profiles/verify-profile.ps1
```

Output:
- `artifacts/codex-profiles/profile-test-results.json`
