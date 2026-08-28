param(
    [Parameter(Mandatory = $true)][string]$ManifestPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$manifestFull = [System.IO.Path]::GetFullPath($ManifestPath)
if (-not (Test-Path -LiteralPath $manifestFull)) {
    throw "Manifest not found: $manifestFull"
}

$manifest = Get-Content -Raw $manifestFull | ConvertFrom-Json
$repoRoot = [string]$manifest.repository_root
$headSha = [string]$manifest.head_sha
$baseSha = [string]$manifest.base_sha
$patchHash = [string]$manifest.patch_hash_sha256

foreach ($lane in $manifest.lanes) {
    $worktreePath = [string]$lane.worktree_path
    $artifactPath = [string]$lane.artifact_path

    New-Item -ItemType Directory -Path $artifactPath -Force | Out-Null

    if (Test-Path -LiteralPath $worktreePath) {
        Push-Location $worktreePath
        try {
            $existingSha = (& git rev-parse HEAD).Trim()
            if ($existingSha -ne $headSha) {
                throw "Existing worktree $worktreePath is at $existingSha, expected $headSha"
            }
        }
        finally {
            Pop-Location
        }
    }
    else {
        Push-Location $repoRoot
        try {
            & git worktree add --detach $worktreePath $headSha
            if ($LASTEXITCODE -ne 0) {
                throw "git worktree add failed for $worktreePath"
            }
        }
        finally {
            Pop-Location
        }
    }

    $cleanEvidence = Get-WorktreeCleanEvidence -WorktreePath $worktreePath
    if (-not $cleanEvidence.clean) {
        throw "Worktree $worktreePath is not clean immediately after creation."
    }

    $observedPatchHash = Get-PatchDigest -RepoRoot $worktreePath -BaseSha $baseSha -HeadSha $headSha
    if ($observedPatchHash -ne $patchHash) {
        throw "Patch hash mismatch in $worktreePath. Expected $patchHash observed $observedPatchHash"
    }
}

$after = [ordered]@{
    verified_utc = (Get-Date).ToUniversalTime().ToString("o")
    worktree_count = @($manifest.lanes).Count
    all_head_sha = $headSha
    patch_hash_sha256 = $patchHash
    isolation_namespaces = @($manifest.lanes | ForEach-Object { [ordered]@{ reviewer_id = $_.reviewer_id; worktree = $_.worktree_path; artifact = $_.artifact_path } })
}

$evidencePath = Join-Path ([System.IO.Path]::GetDirectoryName($manifestFull)) "worktree-isolation-evidence.json"
Set-Content -Path $evidencePath -Value ($after | ConvertTo-Json -Depth 8)
Write-Output ($after | ConvertTo-Json -Depth 8)
