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
	$beta = "-beta"

	$githubToken = "c2e14cc45b7977286d576b0e4d8bb5ff2767359f"
	
	$script:githubRelease = ""
}

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$basePath = Resolve-Path .
$baseModulePath = "$basePath\lib\powershell\modules"
$script:assemblyInformationalVersion = ""

Task default -depends Invoke-Commit


#*================================================================================================
#* Purpose: A task that orchestrates the Publish phase of the femah deployment pipeline.
#*================================================================================================
Task Publish-Femah -depends Update-GithubReleaseTagName {

}

#*================================================================================================
#* Purpose: Updates the tag_name of the matching Release in Github that we intend to publish to 
#* Nuget.org with the current build number, i.e. v[major].[minor].[counter], e.g. v0.1.57-beta
#*================================================================================================
Task Update-GithubReleaseTagName -depends Get-GithubRelease {

	#Determined by the task Get-GithubRelease
	$singleRelease = $script:githubRelease
	$githubReleaseId = $script:githubRelease.Id
	
	$buildNumberToTagReleaseWith = "v$major.$minor.$buildCounter$beta"
	$jsonBody = "{""tag_name"":""$buildNumberToTagReleaseWith""}" 
	Write-Debug "Updating Github release: ""$($script:githubRelease.name)"" from tag_name: ""$($script:githubRelease.tag_name)"" to tag_name: ""$buildNumberToTagReleaseWith"""
	
	Import-Module "$baseModulePath\Invoke-GithubApiRequest.psm1"
	$updatedRelease = Invoke-GithubApiRequest -uri "https://api.github.com/repos/lloydstone/femah/releases/$githubReleaseId" -method Post -githubToken $githubToken -body $jsonBody
	Remove-Module Invoke-GithubApiRequest
	
	if ($updatedRelease.tag_name -eq $buildNumberToTagReleaseWith){
		Write-Debug "Successfully updated release: ""$($updatedRelease.name)"" with tag_name: ""$($updatedRelease.tag_name)"""
	}
	else {
		throw "Error updating github release with tag_name: $buildNumberToTagReleaseWith"
	}
		
}


#*================================================================================================
#* Purpose: A top-level task that orchestrates the Commit phase of the deployment pipeline.
#*================================================================================================
Task Invoke-Commit -depends Invoke-Compile, Invoke-UnitTests, Update-ReleaseNotesFromGithub, New-Packages {

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

#*================================================================================================
#* Purpose: Updates the <releaseNotes></releaseNotes> element within the .nuspec file.
#*================================================================================================
Task Update-ReleaseNotesFromGithub -depends Get-GithubRelease {

	$femahCoreNuspecPath = "$basePath\Femah.Core\Femah.Core.nuspec"
	Write-Debug "Updating release notes in $femahCoreNuspecPath for release: $($script:githubRelease.tag_name) "
	
	Try {
		[xml]$x = Get-Content $femahCoreNuspecPath
		Select-Xml -xml $x -XPath //package/metadata/releaseNotes |
		% { $_.Node.InnerText = $script:githubRelease.body
		  }
		$x.Save($femahCoreNuspecPath)
	}
	Catch [System.Exception]
	{
		throw "Error reading from/writing to file: $femahCoreNuspecPath, unable to update release notes. `r`n $_.Exception.ToString()"
	}
	
}

#*================================================================================================
#* Purpose: Retrieves the matching Release from Github for the current [major].[minor] build.
#*================================================================================================
Task Get-GithubRelease {

	Import-Module "$baseModulePath\Invoke-GithubApiRequest.psm1"
	$releases = Invoke-GithubApiRequest -uri "https://api.github.com/repos/lloydstone/femah/releases" -method GET -githubToken $githubToken
	Remove-Module Invoke-GithubApiRequest
		
	$tagName = "v$major.$minor*"
	Write-Debug "Finding release that matches tag_name: ""$tagName"""
	$singleRelease = $releases | Select-Object | Where-Object {$_.tag_name -like $tagName}
	if ($singleRelease.Count -gt 1)
	{
		throw "More than one release exists matching the tag_name:$tagName, please rectify and try again."
	}	
	$script:githubRelease = $singleRelease
	Write-Debug "Found release with name: ""$($script:githubRelease.name)"" and tag_name: ""$($script:githubRelease.tag_name)"""

}


