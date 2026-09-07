# Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
# Author: Herman Schoenfeld
#
# Distributed under the MIT NON-AI software license, see the accompanying file
# LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
#
# This notice must not be removed when duplicating this file or its contents, in whole or in part.

[CmdletBinding()]
param(
	[Parameter(Mandatory)]
	[string] $Assembly,
	[Parameter(Mandatory)]
	[ValidateRange(1, 32)]
	[int] $Shard,
	[Parameter(Mandatory)]
	[ValidateRange(1, 32)]
	[int] $ShardCount,
	[Parameter(Mandatory)]
	[string] $ResultsDirectory
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
if ($Shard -gt $ShardCount) {
	throw 'Shard must not exceed ShardCount.'
}
$AssemblyPath = (Resolve-Path $Assembly).Path
$ResultsPath = [IO.Path]::GetFullPath($ResultsDirectory)
if (Test-Path -LiteralPath $ResultsPath) {
	throw "Results directory already exists: $ResultsPath. Choose a fresh directory to avoid stale results."
}
New-Item -ItemType Directory -Path $ResultsPath -Force | Out-Null

$Settings = [xml](Get-Content -LiteralPath (Join-Path $PSScriptRoot '../ci.runsettings') -Raw)
$Where = $Settings.CreateElement('Where')
$Where.InnerText = "partition == $Shard/$ShardCount"
$Settings.RunSettings.NUnit.AppendChild($Where) | Out-Null
$SettingsPath = Join-Path $ResultsPath 'partition.runsettings'
$Settings.Save($SettingsPath)
$LogPath = Join-Path $ResultsPath 'test.log'

# Capture complete output for diagnostics; GitHub gets a concise TRX summary in the following step.
Write-Host "Running $(Split-Path $AssemblyPath -Leaf), partition $Shard/$ShardCount"
Write-Host "Full output: $LogPath"
$Arguments = @(
	'test', $AssemblyPath,
	'--settings', $SettingsPath,
	'--results-directory', $ResultsPath,
	'--logger', 'trx;LogFileName=results.trx',
	'--logger', 'console;verbosity=minimal',
	'--blame-hang', '--blame-hang-timeout', '10m', '--blame-hang-dump-type', 'none'
)
& dotnet @Arguments *> $LogPath
$TestExitCode = $LASTEXITCODE
$ResultFiles = @(Get-ChildItem -LiteralPath $ResultsPath -Filter '*.trx' -Recurse -File)
if ($ResultFiles.Count -eq 0) {
	Get-Content -LiteralPath $LogPath -Tail 40
	throw "No TRX results were produced (test runner exit $TestExitCode). See $LogPath"
}
# Fail closed on empty partitions so a broken adapter or filter cannot silently pass CI.
$ResultCount = 0
foreach ($ResultFile in $ResultFiles) {
	$Result = [xml](Get-Content -LiteralPath $ResultFile.FullName -Raw)
	$ResultCount += $Result.SelectNodes('//*[local-name()="UnitTestResult"]').Count
}
if ($ResultCount -eq 0) {
	Get-Content -LiteralPath $LogPath -Tail 40
	throw "The partition produced no test results. See $LogPath"
}
if ($TestExitCode -ne 0) {
	Write-Host "::error::Test partition failed (exit $TestExitCode). See the test summary and uploaded test.log."
}
exit $TestExitCode
