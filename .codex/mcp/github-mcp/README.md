# GitHub MCP (Project Scope)

This folder contains a project-scoped MCP server that integrates Codex with GitHub using the GitHub REST API.

## What it provides
- `github_get_repo`: Fetch repository metadata.
- `github_list_pull_requests`: List pull requests by state.
- `github_create_issue`: Create an issue.

## Prerequisites
- Node.js 20+
- A GitHub token with repo permissions:
  - Read repo metadata and pull requests
  - Write issues if you use `github_create_issue`

## Setup
1. Set environment variables in your shell/session:
   - `GITHUB_TOKEN`
   - Optional defaults: `GITHUB_OWNER`, `GITHUB_REPO`
2. Use the config snippet in `codex.mcp.json` in your Codex MCP settings.

## Security
- Do not commit real tokens.
- Keep token scope minimal (fine-grained PAT recommended).
- Token is read only from environment variables.

## Notes
- The server communicates over stdio using JSON-RPC framing (`Content-Length` headers), compatible with MCP clients.
- API calls use a 15-second timeout.
