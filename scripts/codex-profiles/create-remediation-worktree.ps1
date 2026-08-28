param(
    [string]$BaseSha,
    [string]$Name = "rx-remediator",
    [string]$OutputPath = "artifacts/codex-profiles/remediation-worktree.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$worktreesRoot = Join-Path $repoRoot "artifacts\remediation-worktrees"
$stateDir = Join-Path $PSScriptRoot ".state"
$stateFile = Join-Path $stateDir "remediator-state.json"

New-Item -ItemType Directory -Path $worktreesRoot -Force | Out-Null
New-Item -ItemType Directory -Path $stateDir -Force | Out-Null

$timestamp = (Get-Date).ToString("yyyyMMdd-HHmmss")
$worktreePath = Join-Path $worktreesRoot "$Name-$timestamp"
$isGitRepo = $false

try {
    Push-Location $repoRoot
    & git rev-parse --is-inside-work-tree *> $null
    if ($LASTEXITCODE -eq 0) {
        $isGitRepo = $true
    }
}
finally {
    Pop-Location
}

$mode = "fallback-copy"
$resolvedBaseSha = $null

if ($isGitRepo) {
    Push-Location $repoRoot
    try {
        if ([string]::IsNullOrWhiteSpace($BaseSha)) {
            $resolvedBaseSha = (& git rev-parse HEAD).Trim()
        }
        else {
            $resolvedBaseSha = $BaseSha.Trim()
        }

        & git worktree add --detach $worktreePath $resolvedBaseSha
        if ($LASTEXITCODE -ne 0) {
            throw "git worktree add failed."
        }

        $mode = "git-worktree"
    }
    finally {
        Pop-Location
    }
}
else {
    New-Item -ItemType Directory -Path $worktreePath -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $worktreePath "patch") -Force | Out-Null
    $resolvedBaseSha = "UNAVAILABLE_NO_GIT_REPO"
}

$markerPath = Join-Path $worktreePath ".rx-remediator-lab"
Set-Content -Path $markerPath -Value "rx-remediator disposable workspace"

$record = [ordered]@{
    name = $Name
    mode = $mode
    base_sha = $resolvedBaseSha
    worktree_path = $worktreePath
    marker_path = $markerPath
    created_utc = (Get-Date).ToUniversalTime().ToString("o")
}

$recordJson = $record | ConvertTo-Json -Depth 6
Set-Content -Path $stateFile -Value $recordJson

$outputFull = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputPath))
New-Item -ItemType Directory -Path ([System.IO.Path]::GetDirectoryName($outputFull)) -Force | Out-Null
Set-Content -Path $outputFull -Value $recordJson

Write-Output $recordJson
