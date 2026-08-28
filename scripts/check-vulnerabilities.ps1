$ErrorActionPreference = "Stop"
$projects = @(
  "RxFlow.Api/RxFlow.Api.csproj",
  "RxFlow.Application/RxFlow.Application.csproj",
  "RxFlow.Contracts/RxFlow.Contracts.csproj",
  "RxFlow.Domain/RxFlow.Domain.csproj",
  "RxFlow.Infrastructure/RxFlow.Infrastructure.csproj",
  "RxFlow.Workers/RxFlow.Workers.csproj"
)

$failed = $false
foreach ($project in $projects) {
  Write-Host "Scanning $project"
  $output = dotnet list $project package --vulnerable --include-transitive | Out-String
  Write-Host $output
  if ($output -match "Severity\s+High" -or $output -match "Severity\s+Critical") {
    Write-Error "High/Critical vulnerability detected in $project"
    $failed = $true
  }
}

if ($failed) {
  exit 1
}

Write-Host "Vulnerability policy check passed (no High/Critical in production projects)."
