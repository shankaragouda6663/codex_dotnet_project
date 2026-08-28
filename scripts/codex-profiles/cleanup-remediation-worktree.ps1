param(
    [switch]$KeepState
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$worktreesRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot "artifacts\remediation-worktrees"))
$stateFile = Join-Path $PSScriptRoot ".state\remediator-state.json"

if (-not (Test-Path -LiteralPath $stateFile)) {
    Write-Output "No remediator state file found."
    exit 0
}

$state = Get-Content -Raw $stateFile | ConvertFrom-Json
$target = [System.IO.Path]::GetFullPath([string]$state.worktree_path)
$marker = [System.IO.Path]::GetFullPath([string]$state.marker_path)

if (-not $target.StartsWith($worktreesRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing cleanup: target path is outside remediation root: $target"
}

if (-not (Test-Path -LiteralPath $marker)) {
    throw "Refusing cleanup: marker file not found: $marker"
}

if (([string]$state.mode) -eq "git-worktree") {
    Push-Location $repoRoot
    try {
        & git worktree remove --force $target
        if ($LASTEXITCODE -ne 0) {
            throw "git worktree remove failed for $target"
        }
    }
    finally {
        Pop-Location
    }
}
else {
    Remove-Item -LiteralPath $target -Recurse -Force
}

if (-not $KeepState) {
    Remove-Item -LiteralPath $stateFile -Force
}

Write-Output "Cleaned remediation workspace: $target"
