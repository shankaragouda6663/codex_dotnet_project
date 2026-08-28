# RxFlow Codex Profile Operations

## Commands

Install profile files:

```powershell
pwsh -NoProfile -File scripts/codex-profiles/install-profiles.ps1
```

Run verification harness:

```powershell
pwsh -NoProfile -File scripts/codex-profiles/verify-profile.ps1
```

Run CI non-interactive analysis (safe dry-run by default):

```powershell
pwsh -NoProfile -File scripts/codex-profiles/run-ci-analysis.ps1
```

Execute CI codex run explicitly:

```powershell
pwsh -NoProfile -File scripts/codex-profiles/run-ci-analysis.ps1 -ExecuteCodex
```

Create remediator workspace:

```powershell
pwsh -NoProfile -File scripts/codex-profiles/create-remediation-worktree.ps1
```

Cleanup remediator workspace:

```powershell
pwsh -NoProfile -File scripts/codex-profiles/cleanup-remediation-worktree.ps1
```

## Rollback

Remove repository additions:
- `.codex/config.profiles.example.toml`
- `.codex/profiles/*`
- `.codex/rules/rxflow-operating-profiles.rules`
- `scripts/codex-profiles/*`
- `tests/codex-profiles/*`
- `docs/codex-operating-profiles.md`
- `docs/codex-profile-threat-model.md`
- `docs/codex-profile-operations.md`

Remove installed user profile files manually from `%CODEX_HOME%`:
- `rx-analyst.config.toml`
- `rx-developer.config.toml`
- `rx-security.config.toml`
- `rx-ci.config.toml`
- `rx-remediator.config.toml`

## Break-Glass

Break-glass actions require explicit human authorization outside repository-controlled scripts.
Do not encode bypass settings in repository profile files.
