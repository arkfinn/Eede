<#
.SYNOPSIS
    Eede プロジェクト向けのミューテーションテスト実行スクリプト (Stryker.NET)

.DESCRIPTION
    dotnet-stryker を使用して意図的なコード変異（Mutant）を注入し、
    テストスイートのアサーション強度と有効性を検証します。

.PARAMETER Module
    対象モジュール名または glob パターン。
    指定可能なプリセット:
      - Blending     : 画像ブレンド処理 (Eede.Domain/ImageEditing/Blending)
      - DrawingTools : 描画ツール群 (Eede.Domain/ImageEditing/DrawingTools)
      - Animations   : アニメーション管理 (Eede.Domain/Animations)
      - Files        : ファイル識別・パス管理 (Eede.Domain/Files)
      - DomainAll    : Eede.Domain 全体 (時間がかかります)
    未指定時の既定値は "Blending" です。

.PARAMETER OpenReport
    テスト完了後に生成された HTML レポートを既定のブラウザで開きます。

.PARAMETER BreakAt
    ミューテーションスコアがこの値を下回った場合に非ゼロの終了コードを返します（CI 用）。

.EXAMPLE
    .\scripts\run-mutation-tests.ps1 -Module Blending -OpenReport
    .\scripts\run-mutation-tests.ps1 -Module DrawingTools
#>

param(
    [string]$Module = "Blending",
    [switch]$OpenReport,
    [int]$BreakAt = 0,
    [int]$Concurrency = 0
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$testsDir = Join-Path $repoRoot "Eede.Tests"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Eede Mutation Testing Runner (Stryker) " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# 1. dotnet-stryker のローカルツール確認 & 復元
Write-Host "[1/3] ローカルツールの健全性確認中..." -ForegroundColor Yellow
Push-Location $repoRoot
try {
    dotnet tool restore | Out-Null
}
finally {
    Pop-Location
}

# 2. モジュール指定から glob パターンの解決
$mutatePattern = switch ($Module) {
    "Blending"     { "ImageEditing/Blending/**/*.cs" }
    "DrawingTools" { "ImageEditing/DrawingTools/**/*.cs" }
    "Animations"   { "Animations/**/*.cs" }
    "Files"        { "Files/**/*.cs" }
    "Positions"    { "Positions/**/*.cs" }
    "DomainAll"    { "**/*.cs" }
    Default        { $Module }
}

Write-Host "[2/3] 対象モジュール: $Module ($mutatePattern)" -ForegroundColor Green

# 3. 引数の組み立て
$strykerArgs = @(
    "-p", "Eede.Domain.csproj",
    "--target-framework", "net10.0",
    "-m", $mutatePattern,
    "-r", "Progress",
    "-r", "ClearText",
    "-r", "Html",
    "-r", "Json"
)

if ($BreakAt -gt 0) {
    $strykerArgs += @("-b", $BreakAt.ToString())
}

if ($Concurrency -gt 0) {
    $strykerArgs += @("-c", $Concurrency.ToString())
}

if ($OpenReport) {
    $strykerArgs += @("-o")
}

# 4. Stryker の実行
Write-Host "[3/3] dotnet stryker を実行中..." -ForegroundColor Cyan
Push-Location $testsDir
try {
    dotnet stryker @strykerArgs
    $exitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

# 5. レポート出力場所の案内
$strykerOutputDir = Join-Path $testsDir "StrykerOutput"
if (Test-Path $strykerOutputDir) {
    $latestReport = Get-ChildItem -Path $strykerOutputDir -Recurse -Filter "mutation-report.html" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($latestReport) {
        Write-Host ""
        Write-Host "📊 HTML Report: $($latestReport.FullName)" -ForegroundColor Green
    }
}

exit $exitCode
