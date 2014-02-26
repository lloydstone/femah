function Invoke-NUnitTestsForProject{
<#
 
.SYNOPSIS
    Given the path to a VisualStudio project will execute all NUnit tests within that project.
.DESCRIPTION
    Given the path to a VisualStudio project will execute all NUnit tests within that project, optionally allowing you to specify the path to the NUnit console executable.

.NOTES
	Requirements: Copy this module to any location found in $env:PSModulePath
.PARAMETER projectPath
	Required. The path to a VisualStudio project to run NUnit tests for, relative to the calling scripts location.
.PARAMETER nUnitPath
	Optional. The full path to nunit-console.exe.  Defaults to 'packages\NUnit.Runners.2.6.3\tools\nunit-console.exe', i.e. the bundled version of NUnit.
.EXAMPLE 
	Import-Module Invoke-NUnitTestsForProject
	Import the module
.EXAMPLE	
	Get-Command -Module Invoke-NUnitTestsForProject
	List available functions
.EXAMPLE
	Invoke-NUnitTestsForProject -projectPath "\Femah.Core.Tests\BuildOutput\Femah.Core.Tests.dll"
	Execute the module
#>
	[cmdletbinding()]
		Param(
			[Parameter(
				Position = 0,
				Mandatory = $True )]
				[string]$projectPath,		
			[Parameter(
				Position = 1,
				Mandatory = $False )]
				[string]$nUnitPath
			)
	Begin {
			$DebugPreference = "Continue"
		}	
	Process {
				Try 
				{
					#Set the basePath to the calling scripts path (using Resolve-Path .)
					$basePath = Resolve-Path .
					if ($nUnitPath -eq "")
					{
						#Set our default value for nunit-console.exe
						$nUnitPath = "$basePath\packages\NUnit.Runners.2.6.3\tools\nunit-console.exe"
					}
					
					Write-Host "Executing Tests for project: $projectPath"
					$projectPath = "$basePath\$projectPath"
					
					if (Test-Path "$basePath\TestResult.xml")
					{
						Write-Warning "Removing previous test results from: $basePath\TestResult.xml"
						Remove-Item "$basePath\TestResult.xml" -Force -ErrorAction SilentlyContinue
					}

					& $nUnitPath $projectPath
				}
				catch [Exception] {
					throw "Error executing Tests for supplied project: $projectPath using NUnit from: $nUnitPath `r`n $_.Exception.ToString()"
				}
		}
}