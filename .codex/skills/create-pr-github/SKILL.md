---
name: create-pr-github
description: Create a GitHub pull request safely from the current branch with preflight checks, push validation, and CLI/API fallback.
metadata:
  short-description: Create PRs to GitHub safely.
---

# Create PR on GitHub

## When to use
- User asks to create a pull request for current repository changes.
- Branch is ready and should be proposed to a target base branch (usually `main`).

## Required inputs
- `head` branch (default: current branch).
- `base` branch (default: `main`).
- PR `title`.
- PR `body` (summary, testing notes, risks).

## Safety checks
1. Confirm repo and branch state:
   - `git status --short --branch`
   - `git rev-parse --abbrev-ref HEAD`
2. Ensure branch is pushed:
   - `git push -u origin <head>` (if needed)
3. Confirm remote:
   - `git remote -v`
4. Never expose tokens in output.
5. Do not force-push or rewrite history unless explicitly requested.

## Preferred execution path
1. If GitHub CLI is installed and authenticated:
   - `gh pr create --base <base> --head <head> --title "<title>" --body "<body>"`
2. If `gh` is unavailable, use GitHub REST API:
   - `POST /repos/{owner}/{repo}/pulls`
   - payload: `title`, `head`, `base`, `body`
   - auth via `GITHUB_TOKEN` from environment only.

## Failure handling
- `head` equals `base`: stop and ask for correct source branch.
- Branch not pushed: push first, then retry PR creation.
- Auth missing:
  - `gh`: run `gh auth login`
  - API: set `GITHUB_TOKEN`
- “A pull request already exists”: return existing PR URL.
- Permission denied/repo not found: verify repo URL and account access.

## Response contract
- Return:
  - PR URL
  - PR number
  - `head -> base`
  - Commands executed + exit codes
  - Any unresolved follow-up actions
