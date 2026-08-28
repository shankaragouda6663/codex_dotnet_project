Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-GitCapture {
    param(
        [string]$RepoRoot,
        [string[]]$Args
    )

    Push-Location $RepoRoot
    try {
        $out = @(& git @Args 2>$null)
        $exit = $LASTEXITCODE
        return [ordered]@{ out = $out; exit = $exit }
    }
    finally {
        Pop-Location
    }
}

function Get-RepoRoot {
    param([string]$StartPath)

    $start = if ([string]::IsNullOrWhiteSpace($StartPath)) { (Get-Location).Path } else { $StartPath }
    $fullStart = [System.IO.Path]::GetFullPath($start)

    $probe = Invoke-GitCapture -RepoRoot $fullStart -Args @("rev-parse", "--show-toplevel")
    if ($probe.exit -ne 0 -or $probe.out.Count -eq 0) {
        throw "Current directory is not a Git repository: $fullStart"
    }

    $root = [string]$probe.out[-1]
    if ([string]::IsNullOrWhiteSpace($root)) {
        throw "Current directory is not a Git repository: $fullStart"
    }

    return [System.IO.Path]::GetFullPath($root)
}

function Get-ResolvedSha {
    param(
        [string]$RepoRoot,
        [Parameter(Mandatory = $true)][string]$Ref
    )

    $probe = Invoke-GitCapture -RepoRoot $RepoRoot -Args @("rev-parse", $Ref)
    if ($probe.exit -ne 0 -or $probe.out.Count -eq 0) {
        throw "Unable to resolve ref '$Ref'."
    }

    $sha = [string]$probe.out[-1]
    if ([string]::IsNullOrWhiteSpace($sha)) {
        throw "Unable to resolve ref '$Ref'."
    }

    return $sha
}

function Assert-HeadStable {
    param([string]$RepoRoot)

    $firstProbe = Invoke-GitCapture -RepoRoot $RepoRoot -Args @("rev-parse", "HEAD")
    Start-Sleep -Milliseconds 250
    $secondProbe = Invoke-GitCapture -RepoRoot $RepoRoot -Args @("rev-parse", "HEAD")

    if ($firstProbe.exit -ne 0 -or $secondProbe.exit -ne 0 -or $firstProbe.out.Count -eq 0 -or $secondProbe.out.Count -eq 0) {
        throw "Unable to resolve stable HEAD in repository: $RepoRoot"
    }

    $first = [string]$firstProbe.out[-1]
    $second = [string]$secondProbe.out[-1]
    if ($first -ne $second) {
        throw "HEAD changed during preparation: $first -> $second"
    }

    return $first
}

function Get-WorkingTreeStatus {
    param([string]$RepoRoot)

    $probe = Invoke-GitCapture -RepoRoot $RepoRoot -Args @("status", "--porcelain=v1")
    if ($probe.exit -ne 0) {
        throw "Unable to get working tree status in repository: $RepoRoot"
    }

    return @($probe.out)
}

function Get-PatchDigest {
    param(
        [string]$RepoRoot,
        [Parameter(Mandatory = $true)][string]$BaseSha,
        [Parameter(Mandatory = $true)][string]$HeadSha
    )

    $probe = Invoke-GitCapture -RepoRoot $RepoRoot -Args @("diff", "$BaseSha..$HeadSha")
    if ($probe.exit -ne 0) {
        throw "Failed to compute patch for $BaseSha..$HeadSha"
    }

    $patchText = [string]::Join("`n", @($probe.out))
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($patchText)
        $hash = $sha256.ComputeHash($bytes)
        return -join ($hash | ForEach-Object { $_.ToString("x2") })
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-DetectedStacks {
    param([string]$RepoRoot)

    $hasDotnet = (Test-Path (Join-Path $RepoRoot "global.json")) -or (Get-ChildItem -Path $RepoRoot -Filter *.sln* -File -ErrorAction SilentlyContinue | Measure-Object).Count -gt 0
    $hasJava = (Test-Path (Join-Path $RepoRoot "mvnw")) -or (Test-Path (Join-Path $RepoRoot "gradlew"))

    return [ordered]@{
        dotnet = $hasDotnet
        java = $hasJava
    }
}

function Get-WorktreeCleanEvidence {
    param([string]$WorktreePath)

    $shaProbe = Invoke-GitCapture -RepoRoot $WorktreePath -Args @("rev-parse", "HEAD")
    $statusProbe = Invoke-GitCapture -RepoRoot $WorktreePath -Args @("status", "--porcelain=v1")

    if ($shaProbe.exit -ne 0 -or $shaProbe.out.Count -eq 0 -or $statusProbe.exit -ne 0) {
        throw "Unable to collect clean-status evidence for worktree: $WorktreePath"
    }

    return [ordered]@{
        path = $WorktreePath
        head_sha = [string]$shaProbe.out[-1]
        clean = (@($statusProbe.out).Count -eq 0)
        status = @($statusProbe.out)
    }
}
