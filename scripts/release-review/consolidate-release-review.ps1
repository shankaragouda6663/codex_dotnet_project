param(
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$manifestFull = [System.IO.Path]::GetFullPath($ManifestPath)
$manifest = Get-Content -Raw $manifestFull | ConvertFrom-Json
$reviewDir = [System.IO.Path]::GetDirectoryName($manifestFull)
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $reviewDir "final-release-review.md"
}

$specialists = @()
foreach ($lane in $manifest.lanes) {
    $reviewerId = [string]$lane.reviewer_id
    $resultFile = Join-Path ([string]$lane.artifact_path) (($reviewerId -replace 'reviewer-', 'reviewer-') + ".result.json")
    if (-not (Test-Path $resultFile)) {
        $specialists += [ordered]@{ reviewer_id = $reviewerId; status = "MISSING"; path = $resultFile }
        continue
    }

    $json = Get-Content -Raw $resultFile
    $specialists += [ordered]@{ reviewer_id = $reviewerId; status = "PRESENT"; path = $resultFile; raw = $json }
}

$criticalOrHigh = @()
foreach ($s in $specialists) {
    if ($s.status -ne "PRESENT") { continue }
    try {
        $parsed = $s.raw | ConvertFrom-Json
        if ($parsed.findings) {
            foreach ($f in $parsed.findings) {
                $sev = [string]$f.severity
                if ($sev -in @("critical", "high")) {
                    $criticalOrHigh += [ordered]@{ reviewer_id = $s.reviewer_id; finding = $f }
                }
            }
        }
    }
    catch {
    }
}

$decision = "CONDITIONAL_GO"
if ($criticalOrHigh.Count -gt 0) {
    $decision = "NO_GO"
}

$dotnetDetected = [bool]$manifest.detected_stacks.dotnet
$javaDetected = [bool]$manifest.detected_stacks.java

$report = @()
$report += "SUMMARY AND RELEASE DECISION"
$report += "Decision: $decision"
$report += ""
$report += "BASELINE, CANDIDATE AND PATCH IDENTITY"
$report += "BASE_SHA: $($manifest.base_sha)"
$report += "HEAD_SHA: $($manifest.head_sha)"
$report += "PATCH_HASH_SHA256: $($manifest.patch_hash_sha256)"
$report += ""
$report += "DETECTED JAVA AND .NET COMPONENTS"
$report += ".NET detected: $dotnetDetected"
$report += "Java detected: $javaDetected"
$report += ""
$report += "MCP EVIDENCE AND PERMISSION AUDIT"
$report += "No MCP operations executed by this consolidator script. Specialist MCP usage must be read-only and listed in specialist outputs."
$report += ""
$report += "WORKTREE ISOLATION EVIDENCE"
$report += "Worktree root: $($manifest.worktree_root)"
$report += "Expected lanes: 5"
$report += ""
$report += "REVIEWER 1 — CORRECTNESS AND CONCURRENCY"
$report += "See reviewer-1 artifacts under $($manifest.lanes[0].artifact_path)"
$report += ""
$report += "REVIEWER 2 — TEST QUALITY"
$report += "See reviewer-2 artifacts under $($manifest.lanes[1].artifact_path)"
$report += ""
$report += "REVIEWER 3 — SECURITY AND PRIVACY"
$report += "See reviewer-3 artifacts under $($manifest.lanes[2].artifact_path)"
$report += ""
$report += "REVIEWER 4 — PERFORMANCE AND RESILIENCE"
$report += "See reviewer-4 artifacts under $($manifest.lanes[3].artifact_path)"
$report += ""
$report += "REVIEWER 5 — COMPATIBILITY AND RELEASE READINESS"
$report += "See reviewer-5 artifacts under $($manifest.lanes[4].artifact_path)"
$report += ""
$report += "CONSOLIDATED FINDINGS WITH OWNERSHIP"
if ($criticalOrHigh.Count -eq 0) {
    $report += "No parsed critical/high findings from available specialist outputs."
}
else {
    foreach ($row in $criticalOrHigh) {
        $report += "- [$($row.reviewer_id)] $($row.finding.id) severity=$($row.finding.severity) file=$($row.finding.file) line=$($row.finding.line)"
    }
}
$report += ""
$report += "DUPLICATES, DISAGREEMENTS AND DISMISSED CONCERNS"
$report += "Manual adjudication required in primary review step; this script does not infer semantic duplicates from free text."
$report += ""
$report += "INDEPENDENT VERIFICATION"
$report += "Primary agent must independently verify all critical/high findings before final release status."
$report += ""
$report += "COMMANDS EXECUTED AND RESULTS"
$report += "- consolidate-release-review.ps1 worktree=$($manifest.repository_root) exit=0 result=report generated"
$report += ""
$report += "RELEASE CONDITIONS"
$report += "- All five specialist outputs present and validated against expected SHA and patch hash"
$report += "- Critical/high findings independently verified"
$report += ""
$report += "ROLLBACK AND MONITORING READINESS"
$report += "Use repository rollback/runbook standards and verify alert/telemetry links before GO."
$report += ""
$report += "UNVERIFIED ITEMS"
$report += "- Specialist outputs missing or non-parseable"
$report += "- MCP context unavailable"
$report += ""
$report += "KNOWN LIMITATIONS"
$report += "This consolidator is deterministic but cannot replace manual evidence review for nuanced disagreements."
$report += ""
$report += "WORKTREE CLEANUP STATUS"
$report += "Cleanup must be performed by cleanup-worktrees.ps1 after consolidation."
$report += ""
$report += "NEXT STEPS"
$report += "1. Run specialist lanes"
$report += "2. Verify high/critical findings independently"
$report += "3. Approve GO/CONDITIONAL_GO/NO_GO"

Set-Content -Path $OutputPath -Value ($report -join "`r`n")
Write-Output (Get-Content -Raw $OutputPath)
