@echo off

rem the following resets %ERRORLEVEL% to 0 prior to running powershell
verify >nul
echo. %ERRORLEVEL%

if [%1]==[] (
	SET BUILDCOUNTER=0
) ELSE (
	SET BUILDCOUNTER=%1
)
if [%2]==[] (
	SET GITPATH="''"
) ELSE (
	SET GITPATH=%2
)
if [%3]==[] (
	SET MSBUILD="C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
) ELSE (
	SET MSBUILD=%3
)
powershell.exe -NoProfile -ExecutionPolicy unrestricted -command "Import-Module .\Start-Build.psm1; Invoke-Compile -solutionFiles "Femah.sln" -buildCounter %BUILDCOUNTER% -msbuildPath %MSBUILD% -gitPath %GITPATH%; Remove-Module Start-Build;exit $LASTEXITCODE"
echo. %ERRORLEVEL%

if %ERRORLEVEL% == 0 goto OK
echo ##teamcity[buildStatus status='FAILURE' text='{build.status.text} in execution']
exit /b %ERRORLEVEL%

:OK