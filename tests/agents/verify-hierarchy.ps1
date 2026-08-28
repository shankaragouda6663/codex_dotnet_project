Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$scriptPath = Join-Path $repoRoot "scripts\agents\verify-hierarchy.ps1"

if (-not (Test-Path -LiteralPath $scriptPath)) {
    throw "Missing verification script: $scriptPath"
}

& $scriptPath | Out-String | Write-Output
