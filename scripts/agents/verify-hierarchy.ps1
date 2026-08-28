Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

function Get-InstructionChain {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TargetDirectory,
        [string[]]$FallbackNames
    )

    $resolvedTarget = (Resolve-Path $TargetDirectory).Path
    $current = $resolvedTarget
    $chain = New-Object System.Collections.Generic.List[string]

    while ($true) {
        $override = Join-Path $current "AGENTS.override.md"
        $agents = Join-Path $current "AGENTS.md"

        if (Test-Path -LiteralPath $override) {
            [void]$chain.Add($override)
        }

        if (Test-Path -LiteralPath $agents) {
            [void]$chain.Add($agents)
        }

        foreach ($fallbackName in $FallbackNames) {
            $candidate = Join-Path $current $fallbackName
            if (Test-Path -LiteralPath $candidate -and -not $chain.Contains($candidate)) {
                [void]$chain.Add($candidate)
            }
        }

        $parent = Split-Path -Parent $current
        if ($parent -eq $current -or [string]::IsNullOrWhiteSpace($parent)) {
            break
        }

        $current = $parent
    }

    return $chain
}

function Assert-ContainsAllRules {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Chain,
        [Parameter(Mandatory = $true)]
        [string[]]$RuleIds,
        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    $content = ($Chain | ForEach-Object { Get-Content -Raw $_ }) -join "`n"
    foreach ($ruleId in $RuleIds) {
        if (-not $content.Contains($ruleId)) {
            throw "Coverage failure for '$Label': missing $ruleId in effective chain."
        }
    }
}

function Measure-ChainBytes {
    param([string[]]$Chain)
    return ($Chain | ForEach-Object { (Get-Item -LiteralPath $_).Length } | Measure-Object -Sum).Sum
}

$repoRoot = Get-RepoRoot
$fallbackFromConfig = @()

if ($env:CODEX_AGENTS_FALLBACK_FILENAMES) {
    $fallbackFromConfig = $env:CODEX_AGENTS_FALLBACK_FILENAMES.Split(",") | ForEach-Object { $_.Trim() } | Where-Object { $_ }
}

$representativeTargets = @(
    @{ Name = "order_api"; Path = Join-Path $repoRoot "RxFlow.Api"; Required = @("[RULE-1]", "[RULE-2]", "[RULE-3]", "[RULE-4]", "[RULE-5]", "[RULE-7]", "[RULE-8]") },
    @{ Name = "routing"; Path = Join-Path $repoRoot "RxFlow.Application\Labs"; Required = @("[RULE-1]", "[RULE-2]", "[RULE-4]", "[RULE-5]", "[RULE-7]", "[RULE-8]") },
    @{ Name = "pricing"; Path = Join-Path $repoRoot "RxFlow.Application\Pricing"; Required = @("[RULE-1]", "[RULE-2]", "[RULE-4]", "[RULE-5]", "[RULE-7]", "[RULE-8]") },
    @{ Name = "workers"; Path = Join-Path $repoRoot "RxFlow.Workers"; Required = @("[RULE-1]", "[RULE-2]", "[RULE-4]", "[RULE-5]", "[RULE-7]", "[RULE-8]") },
    @{ Name = "analytics"; Path = Join-Path $repoRoot "RxFlow.Application\Reporting"; Required = @("[RULE-1]", "[RULE-2]", "[RULE-4]", "[RULE-5]", "[RULE-7]", "[RULE-8]") },
    @{ Name = "infra"; Path = Join-Path $repoRoot "RxFlow.Infrastructure"; Required = @("[RULE-1]", "[RULE-2]", "[RULE-4]", "[RULE-5]", "[RULE-6]", "[RULE-7]", "[RULE-8]") }
)

$results = @()
foreach ($target in $representativeTargets) {
    $chain = Get-InstructionChain -TargetDirectory $target.Path -FallbackNames $fallbackFromConfig
    Assert-ContainsAllRules -Chain $chain -RuleIds $target.Required -Label $target.Name
    $results += [pscustomobject]@{
        Component = $target.Name
        TargetPath = $target.Path
        Chain = $chain
        CombinedBytes = Measure-ChainBytes -Chain $chain
    }
}

# Explicit precedence proof fixture: AGENTS.override.md must be preferred over AGENTS.md at same level.
$fixtureTarget = Join-Path $repoRoot "tests\agents\fixtures\precedence\root\sub\deeper"
$fixtureChain = Get-InstructionChain -TargetDirectory $fixtureTarget -FallbackNames @()
if ($fixtureChain.Count -lt 2) {
    throw "Precedence fixture failed: expected at least two project docs in chain."
}

$first = Split-Path -Leaf $fixtureChain[0]
$second = Split-Path -Leaf $fixtureChain[1]
if ($first -ne "AGENTS.override.md" -or $second -ne "AGENTS.md") {
    throw "Precedence fixture failed: expected AGENTS.override.md then AGENTS.md, got '$first' then '$second'."
}

$summary = [pscustomobject]@{
    codexDiscovery = [pscustomobject]@{
        verifiedBaseNames = @("AGENTS.override.md", "AGENTS.md")
        configuredFallbackFileNames = if ($fallbackFromConfig.Count -gt 0) { $fallbackFromConfig } else { @("UNVERIFIED") }
        configuredByteLimit = "UNVERIFIED"
        note = "Codex binary inspection verified AGENTS base names and byte-budget truncation behavior; explicit numeric limit was not discoverable from local config."
    }
    components = $results
    precedenceFixtureChain = $fixtureChain
}

$summary | ConvertTo-Json -Depth 8
