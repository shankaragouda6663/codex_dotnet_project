param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("rx-analyst", "rx-developer", "rx-security", "rx-ci", "rx-remediator")]
    [string]$Profile,

    [Parameter(Mandatory = $true)]
    [string]$Command,

    [string]$TargetPath,

    [string]$RemediatorRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-AbsolutePath {
    param([string]$Path, [string]$Base)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $null
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $Base $Path))
}

function Test-UnderRoot {
    param([string]$Candidate, [string[]]$Roots)

    foreach ($root in $Roots) {
        $resolvedRoot = [System.IO.Path]::GetFullPath($root)
        if (
            $Candidate.Equals($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
            $Candidate.StartsWith($resolvedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
        ) {
            return $true
        }
    }

    return $false
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$artifactsRoot = Join-Path $repoRoot "artifacts\codex-profiles"

$blockedPatterns = @(
    "(^|\s)git\s+reset\s+--hard(\s|$)",
    "(^|\s)git\s+clean\s+-[xfdX]+(\s|$)",
    "(^|\s)dotnet\s+publish(\s|$)",
    "(^|\s)dotnet\s+nuget\s+push(\s|$)",
    "(^|\s)kubectl(\s|$)",
    "(^|\s)helm(\s|$)",
    "(^|\s)terraform\s+(apply|destroy)(\s|$)",
    "(^|\s)rm\s+-rf(\s|$)",
    "(^|\s)Remove-Item\s+.+-Recurse(\s|$)"
)

foreach ($pattern in $blockedPatterns) {
    if ($Command -match $pattern) {
        Write-Error "Command blocked by policy pattern: $pattern"
        exit 200
    }
}

$allowedRoots = switch ($Profile) {
    "rx-developer" {
        @(
            (Join-Path $repoRoot "RxFlow.Api"),
            (Join-Path $repoRoot "RxFlow.Application"),
            (Join-Path $repoRoot "RxFlow.Contracts"),
            (Join-Path $repoRoot "RxFlow.Domain"),
            (Join-Path $repoRoot "RxFlow.Infrastructure"),
            (Join-Path $repoRoot "RxFlow.Tests"),
            (Join-Path $repoRoot "RxFlow.Workers"),
            (Join-Path $repoRoot "docs"),
            (Join-Path $repoRoot "scripts"),
            (Join-Path $repoRoot "tests"),
            $artifactsRoot
        )
    }
    "rx-remediator" {
        if ([string]::IsNullOrWhiteSpace($RemediatorRoot)) {
            Write-Error "Remediator profile requires -RemediatorRoot."
            exit 201
        }

        @(
            ([System.IO.Path]::GetFullPath($RemediatorRoot)),
            $artifactsRoot
        )
    }
    default {
        @()
    }
}

if ($Profile -in @("rx-analyst", "rx-security", "rx-ci") -and -not [string]::IsNullOrWhiteSpace($TargetPath)) {
    Write-Error "Read-only profile '$Profile' cannot write to target path '$TargetPath'."
    exit 202
}

if (-not [string]::IsNullOrWhiteSpace($TargetPath) -and $allowedRoots.Count -gt 0) {
    $candidate = Resolve-AbsolutePath -Path $TargetPath -Base $repoRoot
    if (-not (Test-UnderRoot -Candidate $candidate -Roots $allowedRoots)) {
        Write-Error "Target path '$candidate' is outside allowed roots for profile '$Profile'."
        exit 203
    }
}

& pwsh -NoProfile -Command $Command | Out-Null
exit [int]$LASTEXITCODE
