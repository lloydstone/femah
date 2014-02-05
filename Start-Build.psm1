function Invoke-Compile{
<#
 
.SYNOPSIS
    Performs an msbuild compilation of the supplied Visual Studio solution (.sln) or project (.csproj|.vbproj) files.
.DESCRIPTION
    Performs an msbuild compilation of the supplied Visual Studio solution (.sln) or project (.csproj|.vbproj) files.
.NOTES
    Author: Lloyd Holman
	Requirements: Copy this module to any location found in $env:PSModulePath
.PARAMETER msbuildPath
	Optional. The full path to the msbuild.exe to compile with.  Defaults to "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
.PARAMETER configMode
	Optional. The config mode to build the solution in
.PARAMETER solutionFiles
	Required.
.EXAMPLE 
	Import-Module Start-Build
	Import the module
.EXAMPLE	
	Get-Command -Module Start-Build
	List available functions
.EXAMPLE
	Invoke-Compile 
	Execute the module
#>
	[cmdletbinding()]
		Param(
			[Parameter(
				Position = 0,
				Mandatory = $False )]
				[string]$msbuildPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe",
			[Parameter(
				Position = 1,
				Mandatory = $False )]
				[string]$configMode = "Release",
			[Parameter(
				Position = 2,
				Mandatory = $True )]
				[array]$solutionFiles,
			[Parameter(
				Position = 3,
				Mandatory = $False )]
				[string]$buildCounter = "0",
			[Parameter(
				Position = 4,
				Mandatory = $False )]
				[string]$gitPath					
			)
			
	Begin {
			$DebugPreference = "Continue"
		}	
	Process {

				if($configMode -ne "Debug" -or $configMode -ne "Release")
				{ 
					Write-Host "Unknown configMode $configMode supplied.  Valid values are 'Debug' or 'Release', changing to default 'Release'"
					$configMode = "Release"
				}
				Write-Host "configMode : $configMode"
				
				$basePath = Resolve-Path .
				$baseModulePath = "$basePath\lib\powershell\modules"
				$major = 0
				$minor = 1
				
				foreach($solutionFile in $solutionFiles)
				{
					Try{
						
						#Does what msbuild/VS can't do consistently.  Aggressively and recursively deletes all /obj and /bin folders from the build path as well as the \BuildOutput folder
						Import-Module "$baseModulePath\Remove-FoldersRecursively.psm1"
						Remove-FoldersRecursively
						Remove-Module Remove-FoldersRecursively
						
						#Set build number
						Import-Module "$baseModulePath\Set-BuildNumberWithGitCommitDetail.psm1"
						$assemblyInformationalVersion = Set-BuildNumberWithGitCommitDetail -major $major -minor $minor -buildCounter $buildCounter -gitPath $gitPath
						Remove-Module Set-BuildNumberWithGitCommitDetail
						
						#Compile
						Write-Host "Building solution: $solutionFile in $configMode mode"
						& $msbuildPath $solutionFile /t:ReBuild /t:Clean /p:Configuration=$configMode /p:PlatformTarget=x64 /m
						
						#Run Unit Tests
						Import-Module "$baseModulePath\Invoke-NUnitTestsForProject.psm1"
						Invoke-NUnitTestsForProject -projectPath "\Femah.Core.Tests\BuildOutput\Femah.Core.Tests.dll"
						Remove-Module Invoke-NUnitTestsForProject
						
						#Create NuGet package
						$versionLabels = $assemblyInformationalVersion.Split(".")
						$nuGetPackageVersion = $versionLabels[0] + "." + $versionLabels[1] + "." + $versionLabels[2] + "-beta" 
						
						Write-Host "Will use version: $nuGetPackageVersion to build NuGet package"
						Import-Module "$baseModulePath\New-NuGetPackage.psm1"
						New-NuGetPackage -versionNumber $nuGetPackageVersion -specFilePath "Femah.Core\Femah.Core.nuspec"
						Remove-Module New-NuGetPackage
						
					}
					Catch [Exception]
					{
						throw "Error building: $solutionFile. `r`n $_.Exception.ToString()"
					}
					Finally
					{
						Write-Host "Undoing AssemblyInfo.cs file changes"
						Import-Module "$baseModulePath\Undo-GitFileModifications.psm1"
						Undo-GitFileModifications -fileName AssemblyInfo.cs
						Remove-Module Undo-GitFileModifications
					}
					
					#Allows us to trigger TeamCity to begin to publish artifacts
					#Write-Output "##teamcity[publishArtifacts '**/*.* => Release/']"
				}

				
				#Get the compiled AssemblyFileVersion from a known assembly
				#if (Test-Path "$basePath\Web\MoonpigWebSite\bin\MoonpigWebSite.dll" ) { 
				#	$script:buildNumber = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$basePath\Web\WebSite\bin\Bla.dll").FileVersion
				#}
				#else
				#{
				#	$script:buildNumber = "unknown"
				#}

			}
}

