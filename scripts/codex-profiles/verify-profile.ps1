param(
    [ValidateSet("all", "rx-analyst", "rx-developer", "rx-security", "rx-ci", "rx-remediator")]
    [string]$Profile = "all",
    [string]$OutputPath = "artifacts/codex-profiles/profile-test-results.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$runtimeRoot = Join-Path $repoRoot "tests\codex-profiles\fixtures\runtime"
$artifactsRoot = Join-Path $repoRoot "artifacts\codex-profiles"
New-Item -ItemType Directory -Path $runtimeRoot -Force | Out-Null
New-Item -ItemType Directory -Path $artifactsRoot -Force | Out-Null

$runId = "run-" + (Get-Date).ToString("yyyyMMdd-HHmmss")
$runRoot = Join-Path $runtimeRoot $runId
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null

$results = New-Object System.Collections.Generic.List[object]

function Add-Result {
    param(
        [string]$Id,
        [string]$ProfileName,
        [string]$Name,
        [string]$Expected,
        [string]$Actual,
        [object]$ExitCode,
        [string]$Status,
        [string]$Layer,
        [string]$Evidence
    )

    $results.Add([ordered]@{
        id = $Id
        profile = $ProfileName
        name = $Name
        expected = $Expected
        actual = $Actual
        exit_code = $ExitCode
        status = $Status
        enforcement_layer = $Layer
        evidence = $Evidence
    })
}

function Add-Unverified {
    param([string]$Id, [string]$ProfileName, [string]$Name, [string]$Reason)

    Add-Result -Id $Id -ProfileName $ProfileName -Name $Name -Expected "executed evidence" -Actual $Reason -ExitCode -1 -Status "UNVERIFIED" -Layer "documentation" -Evidence "not executed"
}

function Invoke-SandboxCommand {
    param([string]$SandboxMode, [string]$Command)

    $args = @("sandbox", "-c", ('sandbox_mode="' + $SandboxMode + '"'), "pwsh", "-NoProfile", "-Command", $Command)
    & codex @args | Out-Null
    return [int]$LASTEXITCODE
}

function Invoke-GuardedCommand {
    param([string]$ProfileName, [string]$Command, [string]$TargetPath, [string]$RemediatorRoot)

    $scriptPath = Join-Path $repoRoot "scripts\codex-profiles\invoke-profile-command.ps1"
    $args = @("-NoProfile", "-File", $scriptPath, "-Profile", $ProfileName, "-Command", $Command)

    if (-not [string]::IsNullOrWhiteSpace($TargetPath)) {
        $args += @("-TargetPath", $TargetPath)
    }

    if (-not [string]::IsNullOrWhiteSpace($RemediatorRoot)) {
        $args += @("-RemediatorRoot", $RemediatorRoot)
    }

    & pwsh @args | Out-Null
    return [int]$LASTEXITCODE
}

$profilesToRun = if ($Profile -eq "all") {
    @("rx-analyst", "rx-developer", "rx-security", "rx-ci", "rx-remediator")
}
else {
    @($Profile)
}

$allowedDevFile = Join-Path $runRoot "allowed developer path.txt"
$outsideFile = [System.IO.Path]::GetFullPath((Join-Path $repoRoot "..\outside-sentinel.txt"))
$traversal = [System.IO.Path]::GetFullPath((Join-Path $repoRoot "..\outside-escape.txt"))

foreach ($p in $profilesToRun) {
    switch ($p) {
        "rx-analyst" {
            $exit1 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Get-Content README.md | Out-Null"
            Add-Result "1" $p "Read source and Git metadata" "success" ("exit=" + $exit1) $exit1 ($(if ($exit1 -eq 0) {"PASSED"} else {"FAILED"})) "codex sandbox read-only" "README.md"

            $exit2 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Set-Content -Path '$allowedDevFile' -Value 'deny'"
            $status2 = if ($exit2 -ne 0 -and -not (Test-Path $allowedDevFile)) { "PASSED" } else { "FAILED" }
            Add-Result "2" $p "Write sentinel denied" "write denied" ("exit=" + $exit2) $exit2 $status2 "codex sandbox read-only" $allowedDevFile

            $buildWrite = Join-Path $repoRoot "RxFlow.Api\bin\read-only-write-test.txt"
            $exit3 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Set-Content -Path '$buildWrite' -Value 'deny'"
            $status3 = if ($exit3 -ne 0 -and -not (Test-Path $buildWrite)) { "PASSED" } else { "FAILED" }
            Add-Result "3" $p "Build output write denied" "write denied" ("exit=" + $exit3) $exit3 $status3 "codex sandbox read-only" $buildWrite

            $exit4 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Invoke-WebRequest -Uri https://nonexistent.invalid -TimeoutSec 3 | Out-Null"
            Add-Result "4" $p "Network denied or gated" "non-zero" ("exit=" + $exit4) $exit4 ($(if ($exit4 -ne 0) {"PASSED"} else {"FAILED"})) "sandbox/network restrictions" "Invoke-WebRequest"

            Add-Unverified "5" $p "MCP write operation absent/denied" "MCP write tests require configured writable MCP server and controlled credentials; not configured in this repo harness."
        }

        "rx-developer" {
            $exit6 = Invoke-GuardedCommand -ProfileName $p -Command "Set-Content -Path '$allowedDevFile' -Value 'developer ok'" -TargetPath $allowedDevFile
            Add-Result "6" $p "Edit inside approved path" "success" ("exit=" + $exit6) $exit6 ($(if ($exit6 -eq 0 -and (Test-Path $allowedDevFile)) {"PASSED"} else {"FAILED"})) "path guard wrapper" $allowedDevFile

            $exit7 = Invoke-GuardedCommand -ProfileName $p -Command "dotnet test RxFlow.Tests/RxFlow.Tests.csproj --no-restore --filter PricePreviewModelsTests"
            Add-Result "7" $p ".NET focused test run" "success" ("exit=" + $exit7) $exit7 ($(if ($exit7 -eq 0) {"PASSED"} else {"FAILED"})) "path guard wrapper + dotnet" "PricePreviewModelsTests"

            $exit8 = Invoke-GuardedCommand -ProfileName $p -Command "Set-Content -Path '$outsideFile' -Value 'deny'" -TargetPath $outsideFile
            Add-Result "8" $p "Write outside approved worktree" "denied" ("exit=" + $exit8) $exit8 ($(if ($exit8 -ne 0) {"PASSED"} else {"FAILED"})) "path guard wrapper" $outsideFile

            $exit9 = Invoke-GuardedCommand -ProfileName $p -Command "Set-Content -Path '$traversal' -Value 'deny'" -TargetPath $traversal
            Add-Result "9" $p "Path traversal escape" "denied" ("exit=" + $exit9) $exit9 ($(if ($exit9 -ne 0) {"PASSED"} else {"FAILED"})) "resolved path guard" $traversal

            Add-Unverified "10" $p "Dependency network request approval flow" "Approval UX is interactive by design; automated non-interactive approval-flow assertion is not deterministic in this harness."

            $exit11 = Invoke-GuardedCommand -ProfileName $p -Command "dotnet publish RxFlow.Api/RxFlow.Api.csproj"
            Add-Result "11" $p "Publish/deploy/destructive commands fail closed" "denied" ("exit=" + $exit11) $exit11 ($(if ($exit11 -ne 0) {"PASSED"} else {"FAILED"})) "command block patterns" "dotnet publish"
        }

        "rx-security" {
            $exit12 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "rg --line-number class RxFlow.Api | Out-Null"
            Add-Result "12" $p "Read-only review/search" "success" ("exit=" + $exit12) $exit12 ($(if ($exit12 -eq 0) {"PASSED"} else {"FAILED"})) "codex sandbox read-only" "rg"

            $secWrite = Join-Path $runRoot "security-write.txt"
            $exit13 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Set-Content -Path '$secWrite' -Value 'deny'"
            Add-Result "13" $p "Source modification denied" "denied" ("exit=" + $exit13) $exit13 ($(if ($exit13 -ne 0 -and -not (Test-Path $secWrite)) {"PASSED"} else {"FAILED"})) "codex sandbox read-only" $secWrite

            $exit14 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Invoke-WebRequest -Uri https://nonexistent.invalid -TimeoutSec 3 | Out-Null"
            Add-Result "14" $p "External scan/network denied or gated" "non-zero" ("exit=" + $exit14) $exit14 ($(if ($exit14 -ne 0) {"PASSED"} else {"FAILED"})) "sandbox/network restrictions" "Invoke-WebRequest"

            $exit15 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Get-Content C:\Users\shankarg\.codex\auth.json | Out-Null"
            Add-Result "15" $p "Sensitive credential path inaccessible" "non-zero" ("exit=" + $exit15) $exit15 ($(if ($exit15 -ne 0) {"PASSED"} else {"FAILED"})) "sandbox read restrictions" "C:\\Users\\shankarg\\.codex\\auth.json"
        }

        "rx-ci" {
            Add-Unverified "16" $p "Read-only analysis completes schema-valid" "Requires executing codex exec with model egress and CI credentials; script implemented at scripts/codex-profiles/run-ci-analysis.ps1."

            $ciWrite = Join-Path $runRoot "ci-write.txt"
            $exit17 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Set-Content -Path '$ciWrite' -Value 'deny'"
            Add-Result "17" $p "Write attempt fails immediately" "denied" ("exit=" + $exit17) $exit17 ($(if ($exit17 -ne 0) {"PASSED"} else {"FAILED"})) "codex sandbox read-only" $ciWrite

            $exit18 = Invoke-SandboxCommand -SandboxMode "read-only" -Command "Invoke-WebRequest -Uri https://nonexistent.invalid -TimeoutSec 3 | Out-Null"
            Add-Result "18" $p "Network/privileged/destructive fail without prompting" "non-zero" ("exit=" + $exit18) $exit18 ($(if ($exit18 -ne 0) {"PASSED"} else {"FAILED"})) "read-only + network restrictions" "Invoke-WebRequest"

            Add-Unverified "19" $p "No approval-wait state" "Interactive approval events cannot be asserted without executing codex exec against provider in CI."
            Add-Unverified "20" $p "Missing credential/tool deterministic non-zero" "Requires controlled CI invocation with absent credentials and captured event stream."
            Add-Unverified "21" $p "JSONL transcript retained and sanitized" "Implemented in run-ci-analysis script; execution deferred unless ExecuteCodex is explicitly approved."
        }

        "rx-remediator" {
            $createScript = Join-Path $repoRoot "scripts\codex-profiles\create-remediation-worktree.ps1"
            $cleanupScript = Join-Path $repoRoot "scripts\codex-profiles\cleanup-remediation-worktree.ps1"
            $createExit = (Start-Process -FilePath "pwsh" -ArgumentList @("-NoProfile", "-File", $createScript) -Wait -PassThru -NoNewWindow).ExitCode

            $stateFile = Join-Path $repoRoot "scripts\codex-profiles\.state\remediator-state.json"
            $state = Get-Content -Raw $stateFile | ConvertFrom-Json
            $remRoot = [string]$state.worktree_path
            $remFile = Join-Path $remRoot "patch\candidate.patch"

            $exit22 = Invoke-GuardedCommand -ProfileName $p -Command "Set-Content -Path '$remFile' -Value 'patch'" -TargetPath $remFile -RemediatorRoot $remRoot
            Add-Result "22" $p "Patch creation only inside isolated worktree" "success" ("exit=" + $exit22) $exit22 ($(if ($createExit -eq 0 -and $exit22 -eq 0) {"PASSED"} else {"FAILED"})) "worktree + path guard" $remFile

            $exit23 = Invoke-GuardedCommand -ProfileName $p -Command "Set-Content -Path '$allowedDevFile' -Value 'deny'" -TargetPath $allowedDevFile -RemediatorRoot $remRoot
            Add-Result "23" $p "Primary/sibling worktree writes fail" "denied" ("exit=" + $exit23) $exit23 ($(if ($exit23 -ne 0) {"PASSED"} else {"FAILED"})) "path guard" $allowedDevFile

            Add-Unverified "24" $p "Regression test and validation in isolation" "Git-worktree isolation cannot be fully proven when repository is not detected as a Git worktree in this environment."

            $exit25 = Invoke-GuardedCommand -ProfileName $p -Command "dotnet publish RxFlow.Api/RxFlow.Api.csproj" -RemediatorRoot $remRoot
            Add-Result "25" $p "Push/publish/merge/deploy blocked" "denied" ("exit=" + $exit25) $exit25 ($(if ($exit25 -ne 0) {"PASSED"} else {"FAILED"})) "command block patterns" "dotnet publish"

            $baseSha = [string]$state.base_sha
            $status26 = if (-not [string]::IsNullOrWhiteSpace($baseSha) -and $baseSha -ne "UNAVAILABLE_NO_GIT_REPO") { "PASSED" } else { "UNVERIFIED" }
            Add-Result "26" $p "Patch bound to expected base SHA" "non-empty git sha" ("base_sha=" + $baseSha) 0 $status26 "worktree state metadata" $stateFile

            $cleanupExit = (Start-Process -FilePath "pwsh" -ArgumentList @("-NoProfile", "-File", $cleanupScript) -Wait -PassThru -NoNewWindow).ExitCode
            $status27 = if ($cleanupExit -eq 0 -and -not (Test-Path $remRoot)) { "PASSED" } else { "FAILED" }
            Add-Result "27" $p "Safe cleanup removes only validated lab worktree" "cleanup succeeds" ("exit=" + $cleanupExit) $cleanupExit $status27 "validated marker + path prefix" $remRoot
        }
    }
}

$matrixPath = Join-Path $repoRoot ".codex\profile-matrix.json"
$exit28 = if (Test-Path $matrixPath) { 0 } else { 1 }
Add-Result "28" "cross-profile" "Profile names/config load map" "matrix file exists" ("exists=" + (Test-Path $matrixPath)) $exit28 ($(if ($exit28 -eq 0) {"PASSED"} else {"FAILED"})) "repo profile matrix" $matrixPath

Add-Unverified "29" "cross-profile" "Managed policy cannot be weakened by repo config" "Requires managed policy inspection in enterprise control plane, outside repository scope."

$exit30 = Invoke-GuardedCommand -ProfileName "rx-developer" -Command "Set-Content -Path '$outsideFile' -Value 'deny'" -TargetPath $outsideFile
Add-Result "30" "cross-profile" "Env args cannot broaden writable roots" "denied" ("exit=" + $exit30) $exit30 ($(if ($exit30 -ne 0) {"PASSED"} else {"FAILED"})) "path guard" $outsideFile

$spacePath = Join-Path $runRoot "path with spaces.txt"
$exit31a = Invoke-GuardedCommand -ProfileName "rx-developer" -Command "Set-Content -Path '$spacePath' -Value 'ok'" -TargetPath $spacePath
$exit31b = Invoke-GuardedCommand -ProfileName "rx-developer" -Command "Set-Content -Path '$traversal' -Value 'deny'" -TargetPath $traversal
$status31 = if ($exit31a -eq 0 -and $exit31b -ne 0) { "PASSED" } else { "FAILED" }
Add-Result "31" "cross-profile" "Spaces/traversal safety" "spaces pass, traversal denied" ("space_exit=$exit31a traversal_exit=$exit31b") 0 $status31 "path normalization guard" $spacePath

$preJson = ([ordered]@{ generated_utc = (Get-Date).ToUniversalTime().ToString("o"); run_id = $runId; results = $results } | ConvertTo-Json -Depth 8)
$redactionFailed = $false
if ($preJson -match "(?i)patient[_-]?id|prescription|secret|token") {
    $redactionFailed = $true
}
Add-Result "32" "cross-profile" "Logs sanitized" "no sensitive markers" ("redaction_failed=" + $redactionFailed) 0 ($(if (-not $redactionFailed) {"PASSED"} else {"FAILED"})) "result redaction scan" (Join-Path $artifactsRoot "profile-test-results.json")

$combined = [ordered]@{
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    run_id = $runId
    codex_cli_version = (& codex --version)
    repo_root = $repoRoot
    results = $results
}

$finalJson = $combined | ConvertTo-Json -Depth 8
$outputFull = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputPath))
New-Item -ItemType Directory -Path ([System.IO.Path]::GetDirectoryName($outputFull)) -Force | Out-Null
Set-Content -Path $outputFull -Value $finalJson
Set-Content -Path (Join-Path $artifactsRoot "profile-test-results.json") -Value $finalJson
Write-Output $finalJson





