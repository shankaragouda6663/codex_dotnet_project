param(
    [string]$PromptFile = "tests/codex-profiles/fixtures/ci/prompt.txt",
    [string]$SchemaFile = "tests/codex-profiles/fixtures/ci/result-schema.json",
    [string]$OutputDir = "artifacts/codex-profiles/ci",
    [int]$TimeoutSeconds = 120,
    [switch]$ExecuteCodex
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$fullOutputDir = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDir))
New-Item -ItemType Directory -Path $fullOutputDir -Force | Out-Null

$jsonlPath = Join-Path $fullOutputDir "run.jsonl"
$lastMessagePath = Join-Path $fullOutputDir "last-message.json"
$resultPath = Join-Path $fullOutputDir "result.json"

if (-not $ExecuteCodex) {
    $result = [ordered]@{
        executed = $false
        status = "UNVERIFIED"
        reason = "ExecuteCodex switch not provided to avoid unintended network/model egress in untrusted contexts."
        command = "codex exec --json --output-schema ..."
        timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
    }

    $json = $result | ConvertTo-Json -Depth 6
    Set-Content -Path $resultPath -Value $json
    Write-Output $json
    exit 2
}

$promptPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $PromptFile))
$schemaPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $SchemaFile))

$cmdArgs = @(
    "exec",
    "--sandbox", "read-only",
    "--ask-for-approval", "never",
    "--json",
    "--output-schema", $schemaPath,
    "--output-last-message", $lastMessagePath,
    "--ignore-user-config",
    "--ignore-rules",
    "--cd", $repoRoot,
    "--skip-git-repo-check",
    "--ephemeral"
)

$prompt = Get-Content -Raw $promptPath
$tmpPrompt = Join-Path $fullOutputDir "prompt.txt"
Set-Content -Path $tmpPrompt -Value $prompt

$start = Get-Date
$proc = Start-Process -FilePath "codex" -ArgumentList $cmdArgs -RedirectStandardInput $tmpPrompt -RedirectStandardOutput $jsonlPath -RedirectStandardError (Join-Path $fullOutputDir "stderr.log") -PassThru -WindowStyle Hidden

if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
    try { $proc.Kill() } catch {}
    $timeoutResult = [ordered]@{
        executed = $true
        status = "FAILED"
        reason = "Timed out waiting for non-interactive CI run."
        exit_code = -1
        duration_seconds = [int]((Get-Date) - $start).TotalSeconds
        timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
    }

    $json = $timeoutResult | ConvertTo-Json -Depth 6
    Set-Content -Path $resultPath -Value $json
    Write-Output $json
    exit 1
}

$status = if ($proc.ExitCode -eq 0) { "PASSED" } else { "FAILED" }
$result = [ordered]@{
    executed = $true
    status = $status
    exit_code = $proc.ExitCode
    duration_seconds = [int]((Get-Date) - $start).TotalSeconds
    jsonl_path = $jsonlPath
    last_message_path = $lastMessagePath
    timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
}

$json = $result | ConvertTo-Json -Depth 6
Set-Content -Path $resultPath -Value $json
Write-Output $json

if ($proc.ExitCode -ne 0) {
    exit $proc.ExitCode
}

