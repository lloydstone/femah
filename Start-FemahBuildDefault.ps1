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
	$buildCounter = $p3
	$gitPath = $p4
	$publishToLive = $p5
	$githubApiKey = $p6
	$nugetApiKey = $p7
	
	$major = 0
	$minor = 1
	$beta = "-beta"
	
	$script:githubRelease = ""
	$script:versionNumber = ""
}

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$basePath = Resolve-Path .
$baseModulePath = "$basePath\lib\powershell\modules"
$script:assemblyInformationalVersion = ""

Task default -depends Invoke-Commit

#*================================================================================================
#* Purpose: A task that orchestrates the Publish phase of the deployment pipeline.
#*================================================================================================
Task Publish-Femah -depends Check-PublishToLiveSwitches, Publish-GithubRelease, Publish-Packages {

}

#*================================================================================================
#* Purpose: Checks whether we have explicitly supplied the -publishToLive switch (hopefully helping
#* to avoid accidently pushing to nuget.org) along with keys for -githubApiKey and -nugetApiKey.
#*================================================================================================
Task Check-PublishToLiveSwitches {

	if ($publishToLive -eq $False) {
		throw "Please supply the -publishToLive switch to confirm execution of the Publish-Femah task."
		return
	}
	
	if ($githubApiKey -eq "") {
		throw "Please supply a valid -githubApiKey to execute the Publish-Femah task."
		return
	}
	
	if ($nugetApiKey -eq "") {
		throw "Please supply a valid -nugetApiKey to execute the Publish-Femah task."
		return
	}	
}

#*================================================================================================
#* Purpose: Checks whether we have explicitly supplied the -publishToLive switch, hopefully helping
#* to avoid accidently pushing to nuget.org
#*================================================================================================
Task Check-PublishToLiveSwitch {

	if ($publishToLive -eq $False) {
		throw "Supply the -publishToLive switch to confirm execution of the Publish-Femah task."
		return
	}
}

#*================================================================================================
#* Purpose: Updates the status of the matching Release in Github that we intend to publish to 
#* Nuget.org to be 'Published'.
#*================================================================================================
Task Publish-GithubRelease -depends Update-GithubReleaseTagName {

	#Check the release obtained by the task Update-GithubReleaseTagName
	if ($script:githubRelease.draft -eq $false){
		Write-Debug "Release: ""$($script:githubRelease.name)"" with tag_name: ""$($script:githubRelease.tag_name)"" already set to ""Published"" status. Nothing more to do."
		return
	}
	
	$githubReleaseId = $script:githubRelease.Id
		
	$jsonBody = "{""draft"":""false""}" 
	Write-Debug "Updating Github release: ""$($script:githubRelease.name)"" with tag_name: ""$($script:githubRelease.tag_name)"" to ""Published"" status."
	
	Import-Module "$baseModulePath\Invoke-GithubApiRequest.psm1"
	$updatedRelease = Invoke-GithubApiRequest -uri "https://api.github.com/repos/lloydstone/femah/releases/$githubReleaseId" -method Post -githubApiKey $githubApiKey -body $jsonBody
	Remove-Module Invoke-GithubApiRequest
	
	if ($updatedRelease.draft -eq $false){
		Write-Debug "Successfully updated release: ""$($updatedRelease.name)"" with tag_name: ""$($updatedRelease.tag_name)"" to ""Published"" status."
	}
	else {
		throw "Error updating Github release: ""$($script:githubRelease.name)"" with tag_name: $($script:githubRelease.tag_name) to ""Published"" status."
	}
}

#*================================================================================================
#* Purpose: Updates the tag_name of the matching Release in Github that we intend to publish to 
#* Nuget.org with the current build number, i.e. v[major].[minor].[counter], e.g. v0.1.57-beta
#*================================================================================================
Task Update-GithubReleaseTagName -depends Get-GithubRelease {

	#Determined by the task Get-GithubRelease
	$githubReleaseId = $script:githubRelease.Id
	
	#Get the complete build number [major].[minor].[buildCounter] from the dependant packaged Nuget package (generated as part of the Invoke-Commit task) 
	$packageToPublish = Get-ChildItem "$basePath\BuildOutput" -Filter "*.nupkg" | Sort-Object length | Select-Object -First 1
	if ($packageToPublish.Count -ne 1) {
		throw "Unable to find a Nuget package to publish in folder ""$basePath\BuildOutput"". Please run "".\Start-FemahBuildDefault.ps1 -task Invoke-Commit"" to generate a Nuget package."
		return
	}
	
	$script:versionNumber = $packageToPublish.Name.Replace('Femah.Core.', '').Replace('.nupkg','')
		
	$buildNumberToTagReleaseWith = "v$($script:versionNumber)"
	$jsonBody = "{""tag_name"":""$buildNumberToTagReleaseWith""}" 
	Write-Debug "Updating Github release: ""$($script:githubRelease.name)"" from tag_name: ""$($script:githubRelease.tag_name)"" to tag_name: ""$buildNumberToTagReleaseWith"""
	
	Import-Module "$baseModulePath\Invoke-GithubApiRequest.psm1"
	$script:githubRelease = Invoke-GithubApiRequest -uri "https://api.github.com/repos/lloydstone/femah/releases/$githubReleaseId" -method Post -githubApiKey $githubApiKey -body $jsonBody
	Remove-Module Invoke-GithubApiRequest
	
	if ($script:githubRelease.tag_name -eq $buildNumberToTagReleaseWith){
		Write-Debug "Successfully updated release: ""$($script:githubRelease.name)"" with tag_name: ""$($script:githubRelease.tag_name)"""
	}
	else {
		throw "Error updating github release with tag_name: $buildNumberToTagReleaseWith"
	}
		
}

#*================================================================================================
#* Purpose: Publishes the Nuget and Symbol packages to Nuget.org and Symbolsource.org respectively
#*================================================================================================
Task Publish-Packages {

	$packageToPublish = Get-ChildItem "$basePath\BuildOutput" -Filter "*.nupkg" | Sort-Object length | Select-Object -First 1
	
	Write-Host "##teamcity[buildNumber '$($script:versionNumber)']"
	
	Import-Module "$baseModulePath\Publish-NuGetPackage.psm1"
	Publish-NuGetPackage -nugetPackagePath "BuildOutput\$($packageToPublish.Name)" -nugetApiKey $nugetApiKey
	Remove-Module Publish-NuGetPackage

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
#* Purpose: Performs a full rebuild of the Femah.sln Visual Studio Solution, 
#* removing any previously built assemblies and setting the global build number along the way.
#*================================================================================================
Task Invoke-Compile -depends Invoke-HardcoreClean, Set-VersionNumber {

	if(($configMode -ne "Debug") -and ($configMode -ne "Release"))
	{ 
		Write-Host "Unknown configMode ""$configMode"" supplied.  Valid values are ""Debug"" or ""Release"", changing to default 'Release'"
		$configMode = "Release"
	}
	$solutionFile = "Femah.sln"
	Write-Host "Building ""$solutionFile"" in ""$configMode"" mode."
	exec { & $msbuildPath $solutionFile /t:ReBuild /t:Clean /p:Configuration=$configMode /p:PlatformTarget=AnyCPU /m }
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
#* Conditionally runs only if a value is provided for the -githubApiKey parameter
#*================================================================================================
Task Update-ReleaseNotesFromGithub -depends Get-GithubRelease -precondition { return ($githubApiKey -ne "") } {

	$femahCoreNuspecPath = "$basePath\Femah.Core\Femah.Core.nuspec"
	Write-Debug "Updating release notes in ""$femahCoreNuspecPath"" for release with tag_name: ""$($script:githubRelease.tag_name)""."
	
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
Task Get-GithubRelease -precondition { return ($githubApiKey -ne "") } {

	Import-Module "$baseModulePath\Invoke-GithubApiRequest.psm1"
	$releases = Invoke-GithubApiRequest -uri "https://api.github.com/repos/lloydstone/femah/releases" -method GET -githubApiKey $githubApiKey
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


