param(
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [ValidateSet("1","2","3","4","5")][string]$Reviewer,
    [string]$Model = "gpt-5.3-codex_codeassist",
    [int]$TimeoutSeconds = 900,
    [switch]$Execute
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$manifestFull = [System.IO.Path]::GetFullPath($ManifestPath)
$manifest = Get-Content -Raw $manifestFull | ConvertFrom-Json
$reviewDir = [System.IO.Path]::GetDirectoryName($manifestFull)
$lane = @($manifest.lanes | Where-Object { $_.reviewer_id -eq ("reviewer-" + $Reviewer) })[0]
if ($null -eq $lane) {
    throw "Reviewer lane not found: reviewer-$Reviewer"
}

$templatePath = Join-Path $PSScriptRoot "templates\reviewer-$Reviewer.prompt.md"
if (-not (Test-Path $templatePath)) {
    throw "Template not found: $templatePath"
}

$prompt = Get-Content -Raw $templatePath
$prompt = $prompt.Replace("{{BASE_SHA}}", [string]$manifest.base_sha)
$prompt = $prompt.Replace("{{HEAD_SHA}}", [string]$manifest.head_sha)
$prompt = $prompt.Replace("{{PATCH_HASH}}", [string]$manifest.patch_hash_sha256)
$prompt = $prompt.Replace("{{WORKTREE_PATH}}", [string]$lane.worktree_path)
$prompt = $prompt.Replace("{{ARTIFACT_PATH}}", [string]$lane.artifact_path)
$prompt = $prompt.Replace("{{STACK_DOTNET}}", ([string]$manifest.detected_stacks.dotnet))
$prompt = $prompt.Replace("{{STACK_JAVA}}", ([string]$manifest.detected_stacks.java))

$promptPath = Join-Path ([string]$lane.artifact_path) "reviewer-$Reviewer.prompt.txt"
Set-Content -Path $promptPath -Value $prompt

$resultPath = Join-Path ([string]$lane.artifact_path) "reviewer-$Reviewer.result.json"
$jsonlPath = Join-Path ([string]$lane.artifact_path) "reviewer-$Reviewer.events.jsonl"
$stderrPath = Join-Path ([string]$lane.artifact_path) "reviewer-$Reviewer.stderr.log"
$schemaPath = Join-Path $PSScriptRoot "templates\specialist-output.schema.json"

if (-not $Execute) {
    $preview = [ordered]@{
        reviewer_id = "reviewer-$Reviewer"
        execute = $false
        status = "UNVERIFIED"
        reason = "Execute switch not provided; command prepared only."
        prepared_command = "codex exec --sandbox read-only --ask-for-approval never --json --output-schema <schema>"
        prompt_path = $promptPath
        expected_output = $resultPath
    }
    Set-Content -Path $resultPath -Value ($preview | ConvertTo-Json -Depth 8)
    Write-Output ($preview | ConvertTo-Json -Depth 8)
    exit 2
}

$args = @(
    "exec",
    "--model", $Model,
    "--sandbox", "read-only",
    "--ask-for-approval", "never",
    "--json",
    "--output-schema", $schemaPath,
    "--output-last-message", $resultPath,
    "--cd", ([string]$lane.worktree_path),
    "--ephemeral"
)

$start = Get-Date
$proc = Start-Process -FilePath "codex" -ArgumentList $args -RedirectStandardInput $promptPath -RedirectStandardOutput $jsonlPath -RedirectStandardError $stderrPath -PassThru -WindowStyle Hidden
if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
    try { $proc.Kill() } catch {}
    $timeout = [ordered]@{
        reviewer_id = "reviewer-$Reviewer"
        execute = $true
        status = "FAILED"
        exit_code = -1
        reason = "Timed out"
        duration_seconds = [int]((Get-Date) - $start).TotalSeconds
        jsonl_path = $jsonlPath
        stderr_path = $stderrPath
    }
    Set-Content -Path $resultPath -Value ($timeout | ConvertTo-Json -Depth 8)
    Write-Output ($timeout | ConvertTo-Json -Depth 8)
    exit 1
}

$status = if ($proc.ExitCode -eq 0) { "PASSED" } else { "FAILED" }
$result = [ordered]@{
    reviewer_id = "reviewer-$Reviewer"
    execute = $true
    status = $status
    exit_code = $proc.ExitCode
    duration_seconds = [int]((Get-Date) - $start).TotalSeconds
    jsonl_path = $jsonlPath
    stderr_path = $stderrPath
    result_path = $resultPath
}

Set-Content -Path (Join-Path ([string]$lane.artifact_path) "reviewer-$Reviewer.execution.json") -Value ($result | ConvertTo-Json -Depth 8)
Write-Output ($result | ConvertTo-Json -Depth 8)
if ($proc.ExitCode -ne 0) { exit $proc.ExitCode }
