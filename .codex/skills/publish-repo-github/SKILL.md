---
name: publish-repo-github
description: Safely initialize, connect, and publish a local repository to GitHub with explicit checks, non-destructive git behavior, and clear evidence reporting.
metadata:
  short-description: Publish a local repo to GitHub safely.
---

# Publish Repo to GitHub

## When to use
- User asks to publish or push a local project to GitHub.
- Repository may be new, uninitialized, or missing a remote.
- User wants a safe, auditable flow with clear command evidence.

## Inputs to collect
- Local project path.
- Target GitHub repo URL.
- Target branch (default: `main`).
- Whether to include all current changes or only selected files.

## Safety and security guardrails
- Never print, echo, or store tokens/PATs in output.
- Prefer GitHub CLI auth (`gh auth status`) when available.
- Do not use destructive commands (`git reset --hard`, forced pushes, history rewrite) unless explicitly requested.
- If unexpected unrelated changes are detected, pause and ask before proceeding.
- Show executed commands, exit codes, and concise outcomes.

## Execution flow
1. Validate context:
   - Confirm working directory exists.
   - Check Git availability: `git --version`.
   - Check authentication readiness:
     - Preferred: `gh auth status`
     - Alternative: existing credential manager session for `github.com`.
2. Ensure repository state:
   - If `.git` is missing: `git init`.
   - Inspect status: `git status --short --branch`.
   - If no commits exist, create initial commit:
     - `git add -A`
     - `git commit -m "Initial commit"`
3. Ensure target branch:
   - If branch missing, create/switch to target branch:
     - `git branch -M main` (or selected branch).
4. Configure remote:
   - Check remotes: `git remote -v`.
   - If `origin` missing: `git remote add origin <repo-url>`.
   - If `origin` exists but differs, do not overwrite silently; confirm before change.
5. Publish:
   - `git push -u origin <branch>`
6. Verify:
   - `git status -sb`
   - `git remote -v`
   - `git log --oneline -n 1`

## Failure handling
- Auth failure:
  - Prompt user to run `gh auth login` or configure GitHub credentials; retry publish.
- Permission denied / repo not found:
  - Verify URL, account access, and repo existence.
- Remote already exists with different URL:
  - Ask user before running `git remote set-url origin <repo-url>`.
- Non-fast-forward rejection:
  - Fetch and explain divergence; ask whether to pull/rebase or push a new branch.

## Response contract
- Return:
  - Final branch and remote URL.
  - Whether repo is clean or has pending changes.
  - Commands executed with exit codes.
  - Any follow-up required from the user.
