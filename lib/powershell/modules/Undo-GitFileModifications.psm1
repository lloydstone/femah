function Undo-GitFileModifications{
<#
 
.SYNOPSIS
    Given a filename will attempt to recursively undo modifications to that file within the supplied Git repository
.DESCRIPTION
    Given a filename will attempt to recursively undo modifications to that file within the supplied Git repository, optionally allowing you to specify the path to git.exe.

.NOTES
	Requirements: Copy this module to any location found in $env:PSModulePath
.PARAMETER fileName
	Required. The file to undo changes for.  Note this module acts recursively so undoes modifications to all files matching the supplied name.
.PARAMETER gitPath
	Optional. For those of you who only install the portable versions of Git with the likes of 'Github for Windows' and 'Bitbucket SourceTree' then provide the full path to 'git.exe' or add the path to the PATH environment variable and use the default.  Defaults to 'git.exe', assuming the path to git.exe is in the PATH environment variable.
.EXAMPLE 
	Import-Module Undo-GitFileModifications
	Import the module
.EXAMPLE	
	Get-Command -Module Undo-GitFileModifications
	List available functions
.EXAMPLE
	Undo-GitFileModifications -fileName AssemblyInfo.cs
	Execute the module
#>
	[cmdletbinding()]
		Param(
			[Parameter(
				Position = 0,
				Mandatory = $True )]
				[string]$fileName,		
			[Parameter(
				Position = 1,
				Mandatory = $False )]
				[string]$gitPath
			)
	Begin {
			$DebugPreference = "Continue"
		}	
	Process {
				Try 
				{
					if ($gitPath -eq "")
					{
						$gitPath = "git.exe"
					}
					Write-Warning "Setting Git path to: $gitPath"
					
					Write-Host "Undoing all Git file modifications to $fileName files"
					#Performs a Git CHECKOUT on the files modified during Set-BuildNumber
					#A modules executing path is that of the calling script so this *should* always work. 
					
					#Undo AssemblyInfo.cs changes
					& $gitPath checkout -- */Properties/$fileName
				}
				catch [Exception] {
					throw "Error executing Git checkout for supplied filename: $fileName using Git from: $gitPath `r`n $_.Exception.ToString()"
				}
		}
}