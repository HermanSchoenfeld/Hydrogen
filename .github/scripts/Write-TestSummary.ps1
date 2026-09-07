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
	[ValidateNotNullOrEmpty()]
	[string] $ResultsDirectory,
	[string] $Title = 'Unit test results'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function ConvertTo-SummaryText {
	param([AllowEmptyString()][string] $Text, [int] $Limit = 2000)
	if ($Text.Length -gt $Limit) {
		$Text = $Text.Substring(0, $Limit) + '... [truncated; see TRX artifact]'
	}
	return [System.Net.WebUtility]::HtmlEncode($Text).Replace('|', '&#124;')
}

function Write-FailureAnnotation {
	param([string] $Message)
	if ($Message.Length -gt 3000) {
		$Message = $Message.Substring(0, 3000) + '... [see TRX artifact]'
	}
	$Message = $Message.Replace('%', '%25').Replace("`r", '%0D').Replace("`n", '%0A')
	Write-Host "::error::$Message"
}

function Get-TrxCounter {
	param([System.Xml.XmlElement] $Counters, [string] $Name, [switch] $Required)
	$Text = $Counters.GetAttribute($Name)
	if (-not $Text -and -not $Required) {
		return 0L
	}
	$Value = 0L
	if (-not [long]::TryParse($Text, [ref] $Value) -or $Value -lt 0) {
		throw "Invalid or missing TRX counter '$Name'."
	}
	return $Value
}

function Add-SummaryBlock {
	param([string] $Block)
	$BlockBytes = [System.Text.Encoding]::UTF8.GetByteCount($Block)
	if ($Script:SummaryBytes + $BlockBytes -gt $Script:SummaryLimit) {
		$Script:SummaryTruncated = $true
		return
	}
	[void] $Script:Summary.Append($Block)
	$Script:SummaryBytes += $BlockBytes
}

$Reports = [System.Collections.Generic.List[object]]::new()
$TestResults = [System.Collections.Generic.List[object]]::new()
$InfrastructureErrors = [System.Collections.Generic.List[string]]::new()
$FailureOutcomes = @('Failed', 'Error', 'Timeout', 'Aborted', 'Disconnected', 'NotRunnable', 'PassedButRunAborted')
$SkippedOutcomes = @('NotExecuted', 'Inconclusive')

if (-not (Test-Path -LiteralPath $ResultsDirectory -PathType Container)) {
	$InfrastructureErrors.Add("Test results directory does not exist: $ResultsDirectory")
	$TrxFiles = @()
} else {
	$TrxFiles = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -File -Recurse | Sort-Object FullName)
	if ($TrxFiles.Count -eq 0) {
		$InfrastructureErrors.Add("No TRX results were produced in: $ResultsDirectory")
	}
}

foreach ($TrxFile in $TrxFiles) {
	try {
		# Read only the TRX schema; prohibit document types and external resources.
		$ReaderSettings = [System.Xml.XmlReaderSettings]::new()
		$ReaderSettings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
		$ReaderSettings.XmlResolver = $null
		$Reader = [System.Xml.XmlReader]::Create($TrxFile.FullName, $ReaderSettings)
		try {
			$Document = [System.Xml.XmlDocument]::new()
			$Document.XmlResolver = $null
			$Document.Load($Reader)
		} finally {
			$Reader.Dispose()
		}
		$Namespaces = [System.Xml.XmlNamespaceManager]::new($Document.NameTable)
		$Namespaces.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
		$RunSummary = $Document.SelectSingleNode('/t:TestRun/t:ResultSummary', $Namespaces)
		$Counters = $Document.SelectSingleNode('/t:TestRun/t:ResultSummary/t:Counters', $Namespaces)
		if ($null -eq $RunSummary -or $null -eq $Counters -or -not $RunSummary.GetAttribute('outcome')) {
			throw 'Missing TRX ResultSummary, outcome, or Counters, or unsupported XML namespace.'
		}

		$Total = Get-TrxCounter $Counters 'total' -Required
		$Executed = Get-TrxCounter $Counters 'executed' -Required
		$Passed = Get-TrxCounter $Counters 'passed' -Required
		$Failed = Get-TrxCounter $Counters 'failed' -Required
		foreach ($CounterName in @('error', 'timeout', 'aborted', 'disconnected', 'notRunnable', 'passedButRunAborted')) {
			$Failed += Get-TrxCounter $Counters $CounterName
		}
		$Skipped = (Get-TrxCounter $Counters 'notExecuted') + (Get-TrxCounter $Counters 'inconclusive')
		if ($Executed -gt $Total -or $Passed + $Failed + $Skipped -gt $Total) {
			throw 'TRX counters are inconsistent.'
		}
		$Outcome = $RunSummary.GetAttribute('outcome')

		# Use leaf results so data-driven parent containers do not duplicate their children.
		$Results = $Document.SelectNodes('/t:TestRun/t:Results//t:UnitTestResult[not(t:InnerResults/t:UnitTestResult)]', $Namespaces)
		foreach ($Result in $Results) {
			$Duration = [TimeSpan]::Zero
			[void] [TimeSpan]::TryParse($Result.GetAttribute('duration'), [System.Globalization.CultureInfo]::InvariantCulture, [ref] $Duration)
			$MessageNode = $Result.SelectSingleNode('t:Output/t:ErrorInfo/t:Message', $Namespaces)
			$StackNode = $Result.SelectSingleNode('t:Output/t:ErrorInfo/t:StackTrace', $Namespaces)
			$TestResults.Add([pscustomobject]@{
				Name = $Result.GetAttribute('testName')
				Outcome = $Result.GetAttribute('outcome')
				Duration = $Duration
				Message = $(if ($null -ne $MessageNode) { $MessageNode.InnerText } else { '' })
				Stack = $(if ($null -ne $StackNode) { $StackNode.InnerText } else { '' })
				File = $TrxFile.Name
			})
		}

		# VSTest can leave notExecuted at zero even when NUnit emits skipped result nodes.
		$Skipped = [Math]::Max($Skipped, @($Results | Where-Object { $_.GetAttribute('outcome') -in $SkippedOutcomes }).Count)
		$Failed = [Math]::Max($Failed, @($Results | Where-Object { $_.GetAttribute('outcome') -in $FailureOutcomes }).Count)
		if ($Passed + $Failed + $Skipped -gt $Total) {
			throw 'TRX result outcomes conflict with the summary counters.'
		}
		$Reports.Add([pscustomobject]@{ File = $TrxFile.Name; Total = $Total; Executed = $Executed; Passed = $Passed; Failed = $Failed; Skipped = $Skipped; Outcome = $Outcome })

		# Runner errors may appear only at the run level, even when every completed test passed.
		foreach ($ErrorInfo in $RunSummary.SelectNodes('.//t:ErrorInfo', $Namespaces)) {
			$InfrastructureErrors.Add("$($TrxFile.Name): $($ErrorInfo.InnerText)")
		}
		foreach ($RunInfo in $RunSummary.SelectNodes('t:RunInfos/t:RunInfo', $Namespaces)) {
			if ($RunInfo.GetAttribute('outcome') -in $FailureOutcomes) {
				$InfrastructureErrors.Add("$($TrxFile.Name): $($RunInfo.InnerText)")
			}
		}
		if (($Outcome -eq 'Failed' -and $Failed -eq 0) -or $Outcome -notin @('Passed', 'Completed', 'Failed', 'NotExecuted', 'Inconclusive', 'Warning')) {
			$InfrastructureErrors.Add("$($TrxFile.Name): test run outcome is '$Outcome' (passed $Passed, failed $Failed, skipped $Skipped).")
		}
	} catch {
		$InfrastructureErrors.Add("Cannot read $($TrxFile.Name): $($_.Exception.Message)")
	}
}

$Total = 0L
$Executed = 0L
$Passed = 0L
$Failed = 0L
$Skipped = 0L
foreach ($Report in $Reports) {
	$Total += $Report.Total
	$Executed += $Report.Executed
	$Passed += $Report.Passed
	$Failed += $Report.Failed
	$Skipped += $Report.Skipped
}
$Failures = @($TestResults | Where-Object { $_.Outcome -in $FailureOutcomes })
$Other = $Total - $Passed - $Failed - $Skipped
$Counts = "TRX results: $Passed passed, $Failed failed, $Skipped skipped, $Other other; $Executed executed / $Total total."
Write-Host $Counts

# Keep the step summary under GitHub's 1 MiB limit, including any pre-existing step content.
$Summary = [System.Text.StringBuilder]::new()
$SummaryBytes = 0
$SummaryLimit = 900000
$SummaryTruncated = $false
if ($env:GITHUB_STEP_SUMMARY -and (Test-Path -LiteralPath $env:GITHUB_STEP_SUMMARY -PathType Leaf)) {
	$SummaryLimit = [Math]::Max(0, $SummaryLimit - (Get-Item -LiteralPath $env:GITHUB_STEP_SUMMARY).Length)
}
Add-SummaryBlock "<h3>$(ConvertTo-SummaryText $Title 300)</h3>`n`n$Counts`n`n"
if ($InfrastructureErrors.Count -gt 0) {
	Add-SummaryBlock "**Test infrastructure or report errors: $($InfrastructureErrors.Count). Results may be incomplete.**`n`n"
	foreach ($InfrastructureError in ($InfrastructureErrors | Select-Object -First 20)) {
		Add-SummaryBlock "<pre>$(ConvertTo-SummaryText $InfrastructureError 3000)</pre>`n`n"
	}
}
if ($Reports.Count -gt 0) {
	Add-SummaryBlock "| Report | Run outcome | Passed | Failed | Skipped |`n| --- | --- | ---: | ---: | ---: |`n"
	foreach ($Report in ($Reports | Select-Object -First 50)) {
		Add-SummaryBlock "| $(ConvertTo-SummaryText $Report.File 200) | $(ConvertTo-SummaryText $Report.Outcome 80) | $($Report.Passed) | $($Report.Failed) | $($Report.Skipped) |`n"
	}
	Add-SummaryBlock "`n"
}
if ($Failures.Count -gt 0) {
	Add-SummaryBlock "<h4>Failed tests</h4>`n`n"
	foreach ($Failure in ($Failures | Select-Object -First 40)) {
		$FailureName = ConvertTo-SummaryText ($Failure.Name -replace '[\r\n]+', ' ') 500
		$FailureMessage = ConvertTo-SummaryText $Failure.Message 2000
		$FailureStack = ConvertTo-SummaryText $Failure.Stack 4000
		Add-SummaryBlock "<details><summary>$FailureName — $(ConvertTo-SummaryText $Failure.Outcome 80)</summary>`n<pre>$FailureMessage`n$FailureStack</pre>`n</details>`n`n"
	}
	if ($Failures.Count -gt 40) {
		Add-SummaryBlock "Displaying up to 40 of $($Failures.Count) failed tests. Full failures are in the TRX artifacts.`n`n"
	}
}
$SlowTests = @($TestResults | Where-Object { $_.Outcome -notin $SkippedOutcomes } | Sort-Object Duration -Descending | Select-Object -First 10)
if ($SlowTests.Count -gt 0) {
	Add-SummaryBlock "<h4>Slowest tests</h4>`n`n| Test | Duration | Outcome |`n| --- | ---: | --- |`n"
	foreach ($SlowTest in $SlowTests) {
		$TestName = ConvertTo-SummaryText ($SlowTest.Name -replace '[\r\n]+', ' ') 500
		$Seconds = $SlowTest.Duration.TotalSeconds.ToString('0.000', [System.Globalization.CultureInfo]::InvariantCulture)
		Add-SummaryBlock "| $TestName | ${Seconds}s | $(ConvertTo-SummaryText $SlowTest.Outcome 80) |`n"
	}
	Add-SummaryBlock "`n"
}
if ($SummaryTruncated) {
	Add-SummaryBlock "Some details were omitted to fit the summary size limit. Full results are in the TRX artifacts.`n`n"
}
Add-SummaryBlock "Complete test results and diagnostic output are available in the test artifacts.`n"
if ($SummaryTruncated) {
	Write-Host 'Summary size limit reached; full results remain in the TRX artifacts.'
}
if ($env:GITHUB_STEP_SUMMARY -and $Summary.Length -gt 0) {
	[System.IO.File]::AppendAllText($env:GITHUB_STEP_SUMMARY, $Summary.ToString(), [System.Text.UTF8Encoding]::new($false))
} elseif (-not $env:GITHUB_STEP_SUMMARY -and (Test-Path -LiteralPath $ResultsDirectory -PathType Container)) {
	$SummaryPath = Join-Path $ResultsDirectory 'summary.md'
	[System.IO.File]::WriteAllText($SummaryPath, $Summary.ToString(), [System.Text.UTF8Encoding]::new($false))
	Write-Host "Test summary: $SummaryPath"
}

$AnnotationCount = 0
foreach ($InfrastructureError in $InfrastructureErrors) {
	if ($AnnotationCount -ge 20) {
		break
	}
	Write-FailureAnnotation $InfrastructureError
	$AnnotationCount++
}
foreach ($Failure in $Failures) {
	if ($AnnotationCount -ge 20) {
		break
	}
	Write-FailureAnnotation "$($Failure.Name): $($Failure.Outcome). $($Failure.Message)"
	$AnnotationCount++
}
if ($InfrastructureErrors.Count -gt 0) {
	throw "Test summary detected $($InfrastructureErrors.Count) infrastructure or report errors; see the summary and TRX artifacts."
}
