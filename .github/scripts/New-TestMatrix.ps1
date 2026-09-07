# Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
# Author: Herman Schoenfeld
#
# Distributed under the MIT NON-AI software license, see the accompanying file
# LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
#
# This notice must not be removed when duplicating this file or its contents, in whole or in part.

[CmdletBinding()]
param(
	[string] $Solution = 'src/Sphere10.Framework (CrossPlatform).sln',
	[string] $Configuration = 'Debug',
	[string] $OutputDirectory = 'TestResults/ci-build',
	[ValidateRange(1, 1000000)]
	[int] $TestsPerShard = 1000,
	[ValidateRange(1, 32)]
	[int] $MaxShardsPerAssembly = 8
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$RepositoryDirectory = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$SolutionPath = (Resolve-Path $Solution).Path
$SolutionDirectory = Split-Path $SolutionPath
$OutputPath = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $OutputPath) {
	throw "Output directory already exists: $OutputPath. Choose a fresh directory to avoid stale test binaries."
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Enumerate the actual solution and ask MSBuild which projects are tests, including imported SDK properties.
$ProjectList = & dotnet sln $SolutionPath list
if ($LASTEXITCODE -ne 0) {
	throw 'Unable to list solution projects.'
}
$Projects = @($ProjectList | Where-Object { $_.Trim().EndsWith('.csproj') } | Sort-Object)
if ($Projects.Count -eq 0) {
	throw 'The solution contains no C# projects.'
}

$Matrix = [Collections.Generic.List[object]]::new()
$Inventory = [Collections.Generic.List[object]]::new()
$DiscoverySettings = [xml](Get-Content -LiteralPath (Join-Path $RepositoryDirectory '.github/ci.runsettings') -Raw)
$DumpSetting = $DiscoverySettings.CreateElement('DumpXmlTestDiscovery')
$DumpSetting.InnerText = 'true'
$DiscoverySettings.RunSettings.NUnit.AppendChild($DumpSetting) | Out-Null
$SettingsPath = Join-Path $OutputPath 'discovery.runsettings'
$DiscoverySettings.Save($SettingsPath)
$AssemblyIndex = 0

foreach ($Project in $Projects) {
	$ProjectPath = [IO.Path]::GetFullPath((Join-Path $SolutionDirectory $Project.Trim()))
	$MetadataJson = & dotnet msbuild $ProjectPath -nologo "-p:Configuration=$Configuration" '-getProperty:IsTestProject,TargetFramework,TargetFrameworks'
	if ($LASTEXITCODE -ne 0) {
		throw "Unable to evaluate project: $ProjectPath"
	}
	$Metadata = ($MetadataJson | Out-String | ConvertFrom-Json).Properties
	if ($Metadata.IsTestProject -ne 'true') {
		continue
	}
	$Frameworks = if ($Metadata.TargetFrameworks) { $Metadata.TargetFrameworks.Split(';') } else { @($Metadata.TargetFramework) }
	foreach ($Framework in $Frameworks) {
		$TargetPath = & dotnet msbuild $ProjectPath -nologo "-p:Configuration=$Configuration" "-p:TargetFramework=$Framework" '-getProperty:TargetPath'
		if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $TargetPath)) {
			throw "Build the solution before discovering tests: $ProjectPath ($Framework)"
		}
		$AssemblyIndex++
		$AssemblyId = "assembly-$AssemblyIndex"
		$AssemblyDirectory = Join-Path $OutputPath $AssemblyId
		New-Item -ItemType Directory -Path $AssemblyDirectory | Out-Null
		# Include runtime dependencies, native libraries, resources, and test data; runners need no restore or rebuild.
		Get-ChildItem -LiteralPath (Split-Path $TargetPath) -Force |
			Where-Object { $_.Name -ne 'Dump' } |
			Copy-Item -Destination $AssemblyDirectory -Recurse -Force
		$AssemblyName = Split-Path $TargetPath -Leaf
		$AssemblyPath = Join-Path $AssemblyDirectory $AssemblyName
		$DiscoveryLog = Join-Path $OutputPath "$AssemblyId-discovery.log"
		& dotnet test $AssemblyPath --list-tests --settings $SettingsPath --logger 'console;verbosity=quiet' *> $DiscoveryLog
		if ($LASTEXITCODE -ne 0) {
			Get-Content -LiteralPath $DiscoveryLog -Tail 40
			throw "Test discovery failed for $AssemblyName. See $DiscoveryLog"
		}

		# The NUnit adapter exports its full discovery tree, including generated, inherited, ignored, and invalid cases.
		$DiscoveryPath = Join-Path $AssemblyDirectory "Dump/D_$AssemblyName.dump"
		if (-not (Test-Path -LiteralPath $DiscoveryPath)) {
			throw "NUnit did not produce discovery XML for $AssemblyName. Check the adapter and $DiscoveryLog"
		}
		$Discovery = [xml](Get-Content -LiteralPath $DiscoveryPath -Raw)
		$InvalidTests = @($Discovery.SelectNodes('//*[@runstate="NotRunnable"]'))
		if ($InvalidTests.Count -gt 0) {
			throw "Invalid NUnit tests discovered in ${AssemblyName}: $(($InvalidTests | ForEach-Object { $_.GetAttribute('fullname') }) -join ', '). See $DiscoveryPath"
		}
		# Explicit tests remain opt-in; Ignore and runtime Assert.Ignore retain their normal NUnit behavior.
		$TestCount = $Discovery.SelectNodes('//test-case[not(ancestor-or-self::*[@runstate="Explicit"])]').Count
		$ShardCount = [Math]::Min($MaxShardsPerAssembly, [int][Math]::Ceiling($TestCount / [double]$TestsPerShard))
		$ProjectName = [IO.Path]::GetFileNameWithoutExtension($ProjectPath)
		$Inventory.Add([ordered]@{ project = $ProjectName; framework = $Framework; tests = $TestCount; shards = $ShardCount })
		Write-Host "$ProjectName ($Framework): $TestCount discovered tests, $ShardCount shards"
		for ($Shard = 1; $Shard -le $ShardCount; $Shard++) {
			$Matrix.Add([ordered]@{
				id = "$AssemblyId-$Shard"
				name = "$ProjectName ($Framework) $Shard/$ShardCount"
				assembly = "$AssemblyId/$AssemblyName"
				shard = $Shard
				shardCount = $ShardCount
			})
		}
	}
}

if ($Matrix.Count -eq 0 -or $Matrix.Count -gt 256) {
	throw "Expected 1-256 test jobs, found $($Matrix.Count). Check discovery or increase TestsPerShard."
}
$MatrixJson = ConvertTo-Json -InputObject @{ include = $Matrix.ToArray() } -Depth 5 -Compress
Set-Content -LiteralPath (Join-Path $OutputPath 'matrix.json') -Value $MatrixJson -Encoding utf8
ConvertTo-Json -InputObject $Inventory.ToArray() -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputPath 'inventory.json') -Encoding utf8
if ($env:GITHUB_OUTPUT) {
	Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value "matrix=$MatrixJson" -Encoding utf8
}
if ($env:GITHUB_STEP_SUMMARY) {
	$Rows = @('## Test plan', '', '| Project | Framework | Discovered cases | Jobs |', '| --- | --- | ---: | ---: |')
	$Rows += $Inventory | ForEach-Object { "| $($_.project) | $($_.framework) | $($_.tests) | $($_.shards) |" }
	$Rows += '', 'Projects with zero non-explicit test cases are listed above and do not create empty jobs.'
	$Rows | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}
Write-Host "Created $($Matrix.Count) independent test jobs."
