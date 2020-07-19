$fileContents = @"
#if ENABLE_UNITTESTS
using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Logic;

namespace Logic.Tests
{
	[TestFixture()]
	public class TestXls
	{
<#=Tests#>
	}
}
#endif // ENABLE_UNITTESTS
"@

$testContents = @"
		[Test()]
		public void <#=fileNameOnly#>()
		{
			const string path = @"..\..\..\..\..\TestCases\<#=fileNameOnly#>.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

"@

$testCasesPath = Join-Path $PSScriptRoot '..\..\TestCases'
$tests = ''
$numTests = 0
Get-ChildItem $testCasesPath -Filter "*.xlsx" | ForEach-Object {
	$fileNameOnly = $_.BaseName
	$tests += $testContents.Replace('<#=fileNameOnly#>',$fileNameOnly)
	$numTests++
}

Write-Host "Adding $numTests to TestXls.cs"
$fileContents.Replace('<#=Tests#>',$tests) | Set-Content '..\..\TestXls.cs'