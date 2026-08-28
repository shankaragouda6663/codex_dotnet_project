param(
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [string]$Model = "gpt-5.3-codex_codeassist",
    [switch]$Execute,
    [switch]$Sequential
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$manifestFull = [System.IO.Path]::GetFullPath($ManifestPath)
$manifest = Get-Content -Raw $manifestFull | ConvertFrom-Json
$reviewDir = [System.IO.Path]::GetDirectoryName($manifestFull)
$executionSummaryPath = Join-Path $reviewDir "specialist-execution-summary.json"

$reviewers = @("1","2","3","4","5")
$scriptPath = Join-Path $PSScriptRoot "run-specialist-review.ps1"

$results = New-Object System.Collections.Generic.List[object]

if ($Sequential -or -not $Execute) {
    foreach ($r in $reviewers) {
        $args = @("-NoProfile", "-File", $scriptPath, "-ManifestPath", $manifestFull, "-Reviewer", $r, "-Model", $Model)
        if ($Execute) { $args += "-Execute" }
        & pwsh @args
        $results.Add([ordered]@{ reviewer = $r; mode = "sequential"; exit_code = [int]$LASTEXITCODE })
    }
}
else {
    $jobs = @()
    foreach ($r in $reviewers) {
        $jobs += Start-Process -FilePath "pwsh" -ArgumentList @("-NoProfile", "-File", $scriptPath, "-ManifestPath", $manifestFull, "-Reviewer", $r, "-Model", $Model, "-Execute") -PassThru -WindowStyle Hidden
    }

    foreach ($j in $jobs) {
        $j.WaitForExit()
        $reviewer = $j.StartInfo.Arguments -replace '.*-Reviewer\s+([1-5]).*', '$1'
        $results.Add([ordered]@{ reviewer = $reviewer; mode = "parallel"; exit_code = $j.ExitCode })
    }
}

$summary = [ordered]@{
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    execute = [bool]$Execute
    sequential = [bool]$Sequential
    reviewers = $results
}

Set-Content -Path $executionSummaryPath -Value ($summary | ConvertTo-Json -Depth 8)
Write-Output ($summary | ConvertTo-Json -Depth 8)
