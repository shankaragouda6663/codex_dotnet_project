param(
    [Parameter(Mandatory = $true)][string]$BaseRef,
    [string]$HeadRef = "HEAD",
    [string]$ReviewId,
    [string]$OutputDir = "artifacts/release-review",
    [switch]$AllowUncommittedCandidate
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$repoRoot = Get-RepoRoot -StartPath (Join-Path $PSScriptRoot "..\..")
$timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
if ([string]::IsNullOrWhiteSpace($ReviewId)) {
    $ReviewId = "release-review-$timestamp"
}

$headStable = Assert-HeadStable -RepoRoot $repoRoot
$baseSha = Get-ResolvedSha -RepoRoot $repoRoot -Ref $BaseRef
$headSha = Get-ResolvedSha -RepoRoot $repoRoot -Ref $HeadRef

if ($headSha -ne $headStable) {
    throw "Resolved HEAD_SHA differs from stable HEAD check. Refusing ambiguous candidate."
}

$status = Get-WorkingTreeStatus -RepoRoot $repoRoot
if ($status.Count -gt 0 -and -not $AllowUncommittedCandidate) {
    throw "Repository has uncommitted changes. Provide immutable patch artifact or pass -AllowUncommittedCandidate explicitly."
}

$patchHash = Get-PatchDigest -RepoRoot $repoRoot -BaseSha $baseSha -HeadSha $headSha
$detectedStacks = Get-DetectedStacks -RepoRoot $repoRoot

$reviewRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDir))
$reviewDir = Join-Path $reviewRoot $ReviewId
$worktreeRoot = Join-Path $reviewDir "worktrees"
$artifactRoot = Join-Path $reviewDir "artifacts"

New-Item -ItemType Directory -Path $worktreeRoot -Force | Out-Null
New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null

$lanes = @(1,2,3,4,5) | ForEach-Object {
    [ordered]@{
        reviewer_id = "reviewer-$_"
        worktree_path = Join-Path $worktreeRoot ("reviewer-" + $_)
        artifact_path = Join-Path $artifactRoot ("reviewer-" + $_)
    }
}

$diffNameStatus = @()
$diffStat = @()
Push-Location $repoRoot
try {
    $diffNameStatus = @(& git diff --name-status "$baseSha..$headSha")
    $diffStat = @(& git diff --stat "$baseSha..$headSha")
}
finally {
    Pop-Location
}

$manifest = [ordered]@{
    created_utc = (Get-Date).ToUniversalTime().ToString("o")
    review_id = $ReviewId
    repository_root = $repoRoot
    base_ref = $BaseRef
    head_ref = $HeadRef
    base_sha = $baseSha
    head_sha = $headSha
    patch_hash_sha256 = $patchHash
    uncommitted_status_count = $status.Count
    allow_uncommitted_candidate = [bool]$AllowUncommittedCandidate
    detected_stacks = $detectedStacks
    diff_name_status = $diffNameStatus
    diff_stat = $diffStat
    worktree_root = $worktreeRoot
    artifact_root = $artifactRoot
    lanes = $lanes
    mcp_sources = @()
    evidence_limitations = @()
}

$manifestPath = Join-Path $reviewDir "manifest.json"
$manifestJson = $manifest | ConvertTo-Json -Depth 8
Set-Content -Path $manifestPath -Value $manifestJson

Write-Output $manifestJson
