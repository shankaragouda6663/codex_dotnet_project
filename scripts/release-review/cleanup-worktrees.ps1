param(
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [switch]$KeepArtifacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$manifestFull = [System.IO.Path]::GetFullPath($ManifestPath)
$manifest = Get-Content -Raw $manifestFull | ConvertFrom-Json

foreach ($lane in $manifest.lanes) {
    $path = [string]$lane.worktree_path
    if (-not (Test-Path -LiteralPath $path)) { continue }

    Push-Location ([string]$manifest.repository_root)
    try {
        & git worktree remove --force $path
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to remove worktree: $path"
        }
    }
    finally {
        Pop-Location
    }
}

if (-not $KeepArtifacts) {
    $reviewDir = [System.IO.Path]::GetDirectoryName($manifestFull)
    $cleanupMarker = Join-Path $reviewDir "cleanup-complete.txt"
    Set-Content -Path $cleanupMarker -Value ((Get-Date).ToUniversalTime().ToString("o"))
}

Write-Output "Worktree cleanup complete."
