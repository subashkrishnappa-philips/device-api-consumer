<#
.SYNOPSIS
    Runs the consumer pact tests for device-api-consumer.

.DESCRIPTION
    1. Restores and builds DeviceApi.Consumer.sln
    2. Runs consumer tests — writes contracts/DeviceApi-Consumer-DeviceApi.json
    3. Prints the path to the generated pact file

.PARAMETER Configuration
    Build configuration. Default: Debug

.PARAMETER Verbosity
    MSBuild verbosity for dotnet test. Default: normal

.EXAMPLE
    .\run-consumer-tests.ps1
    .\run-consumer-tests.ps1 -Configuration Release
#>
[CmdletBinding()]
param(
    [string] $Configuration = "Debug",
    [string] $Verbosity      = "normal"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root          = $PSScriptRoot
$SolutionFile  = Join-Path $Root "DeviceApi.Consumer.sln"
$ContractsDir  = Join-Path $Root "contracts"
$ResultsDir    = Join-Path $Root "TestResults"

function Write-Banner([string]$msg) {
    $line = "─" * 70
    Write-Host "`n$line"  -ForegroundColor Cyan
    Write-Host "  $msg"   -ForegroundColor Cyan
    Write-Host "$line`n"  -ForegroundColor Cyan
}

# ── Pre-flight ─────────────────────────────────────────────────────────────────
Write-Banner "device-api-consumer — Consumer Contract Test Runner"
Write-Host "  Root          : $Root"
Write-Host "  Contracts dir : $ContractsDir"
Write-Host "  Configuration : $Configuration"

New-Item -ItemType Directory -Path $ContractsDir -Force | Out-Null
New-Item -ItemType Directory -Path $ResultsDir   -Force | Out-Null

# ── Step 1: Restore ────────────────────────────────────────────────────────────
Write-Banner "Step 1 — Restore"
dotnet restore $SolutionFile --verbosity quiet
if ($LASTEXITCODE -ne 0) { Write-Host "[FAIL] Restore failed." -ForegroundColor Red; exit 1 }

# ── Step 2: Build ──────────────────────────────────────────────────────────────
Write-Banner "Step 2 — Build"
dotnet build $SolutionFile `
    --configuration $Configuration `
    --no-restore `
    --verbosity $Verbosity
if ($LASTEXITCODE -ne 0) { Write-Host "[FAIL] Build failed." -ForegroundColor Red; exit 1 }
Write-Host "[PASS] Build succeeded." -ForegroundColor Green

# ── Step 3: Run consumer tests ─────────────────────────────────────────────────
Write-Banner "Step 3 — Consumer Tests (generates pact file)"
$logFile = Join-Path $ResultsDir "DeviceApi.Consumer.Tests.trx"

dotnet test $SolutionFile `
    --no-build `
    --configuration $Configuration `
    --verbosity $Verbosity `
    --logger "trx;LogFileName=$logFile" `
    --results-directory $ResultsDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[FAIL] Consumer tests failed. See: $logFile" -ForegroundColor Red
    exit 1
}
Write-Host "[PASS] Consumer tests passed." -ForegroundColor Green

# ── Output ─────────────────────────────────────────────────────────────────────
$pactFiles = Get-ChildItem $ContractsDir -Filter "*.json" -ErrorAction SilentlyContinue
if ($pactFiles) {
    Write-Banner "Generated pact contracts"
    $pactFiles | ForEach-Object { Write-Host "  $($_.FullName)" -ForegroundColor Green }
    Write-Host "`n  Share these files with device-api (provider) for verification."
} else {
    Write-Host "[WARN] No pact files found in $ContractsDir" -ForegroundColor Yellow
}

Write-Host ""
exit 0
