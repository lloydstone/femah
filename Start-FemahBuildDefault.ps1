#*==========================================================================================
#* Created: 09/04/2014
#* Author: Lloyd Holman

#* Requirements:
#* 1. Install PowerShell 2.0+ on local machine
#* 2. Execute from build.bat
#*==========================================================================================
#* Purpose: Performs the orchestration of the psake deployment pipeline
#*==========================================================================================
Properties { 
	$basePath = Resolve-Path .
	$baseModulePath = "$basePath\lib\powershell\modules"

	$msbuildPath = $p1
	$configMode = $p2
	$solutionFiles = $p3
	$buildCounter = $p4
	$gitPath = $p5
	
	$major = 0
	$minor = 1
}

$ErrorActionPreference = 'Stop'

$basePath = Resolve-Path .
$baseModulePath = "$basePath\lib\powershell\modules"
$script:assemblyInformationalVersion = ""

Task default -depends Invoke-Commit

#*================================================================================================
#* Purpose: A top-level task that orchestrates the Commit phase of the deployment pipeline.
#*================================================================================================
Task Invoke-Commit -depends Invoke-Compile, Invoke-UnitTests, New-Packages {

	Write-Host "Undoing AssemblyInfo.cs file changes"
	Import-Module "$baseModulePath\Undo-GitFileModifications.psm1"
	Undo-GitFileModifications -fileName AssemblyInfo.cs -gitPath $gitPath
	Remove-Module Undo-GitFileModifications	
}

#*================================================================================================
#* Purpose: Performs a full rebuild of the supplied Visual Studio Solution or Project, 
#* removing any previously built assemblies and setting the global build number along the way.
#*================================================================================================
Task Invoke-Compile -depends Invoke-HardcoreClean, Set-VersionNumber {

	if($configMode -ne "Debug" -or $configMode -ne "Release")
	{ 
		Write-Host "Unknown configMode $configMode supplied.  Valid values are 'Debug' or 'Release', changing to default 'Release'"
		$configMode = "Release"
	}
	Write-Host "configMode : $configMode"

	foreach($solutionFile in $solutionFiles)
	{
		Write-Host "Building solution: $solutionFile in $configMode mode"
		exec { & $msbuildPath $solutionFile /t:ReBuild /t:Clean /p:Configuration=$configMode /p:PlatformTarget=AnyCPU /m}
	}
}

#*================================================================================================
#* Purpose: Does what msbuild/VS can't do consistently.  Aggressively and recursively deletes 
#* all /obj and /bin folders from the build path as well as the \BuildOutput folder.
#*================================================================================================
Task Invoke-HardcoreClean {

	Import-Module "$baseModulePath\Remove-FoldersRecursively.psm1"
	Remove-FoldersRecursively
	Remove-Module Remove-FoldersRecursively
}

#*================================================================================================
#* Purpose: Sets the consistent build number of the form [major].[minor].[buildCounter].[revision]
#*================================================================================================
Task Set-VersionNumber {

	Import-Module "$baseModulePath\Set-BuildNumberWithGitCommitDetail.psm1"
	$script:assemblyInformationalVersion = Set-BuildNumberWithGitCommitDetail -major $major -minor $minor -buildCounter $buildCounter -gitPath $gitPath
	Remove-Module Set-BuildNumberWithGitCommitDetail
}

#*================================================================================================
#* Purpose: Executes the unit tests for the Femah.Core.Tests project
#*================================================================================================
Task Invoke-UnitTests {

	Import-Module "$baseModulePath\Invoke-NUnitTestsForProject.psm1"
	Invoke-NUnitTestsForProject -projectPath "\Femah.Core.Tests\BuildOutput\Femah.Core.Tests.dll"
	Remove-Module Invoke-NUnitTestsForProject
} 

#*================================================================================================
#* Purpose: Generates a new Nuget (Femah.Core.[version].nupkg) and Symbols package 
#* (Femah.Core.[version].symbols.nupkg) for the currently building version.
#*================================================================================================
Task New-Packages {

	$versionLabels = $script:assemblyInformationalVersion.Split(".")
	$nuGetPackageVersion = $versionLabels[0] + "." + $versionLabels[1] + "." + $versionLabels[2] + "-beta" 
	
	Write-Host "Will use version: $nuGetPackageVersion to build NuGet package"
	Import-Module "$baseModulePath\New-NuGetPackage.psm1"
	New-NuGetPackage -versionNumber $nuGetPackageVersion -specFilePath "Femah.Core\Femah.Core.nuspec" -includeSymbolPackage
	Remove-Module New-NuGetPackage
}



