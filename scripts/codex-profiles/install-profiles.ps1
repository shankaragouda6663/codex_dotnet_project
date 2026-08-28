param(
    [string]$CodeHome = $env:CODEX_HOME
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CodeHome)) {
    $CodeHome = Join-Path $HOME ".codex"
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$sourceDir = Join-Path $repoRoot ".codex\profiles"

if (-not (Test-Path -LiteralPath $sourceDir)) {
    throw "Source profile directory not found: $sourceDir"
}

New-Item -ItemType Directory -Path $CodeHome -Force | Out-Null

$profiles = @(
    "rx-analyst.config.toml",
    "rx-developer.config.toml",
    "rx-security.config.toml",
    "rx-ci.config.toml",
    "rx-remediator.config.toml"
)

foreach ($name in $profiles) {
    $src = Join-Path $sourceDir $name
    $dst = Join-Path $CodeHome $name
    Copy-Item -LiteralPath $src -Destination $dst -Force
}

Write-Output "Installed profile files to $CodeHome"
