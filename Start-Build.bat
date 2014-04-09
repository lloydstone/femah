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

SET TASK="Invoke-Commit"
SET CONFIGMODE="Release"
SET SOLUTIONFILES="Femah.sln"

powershell.exe -NoProfile -ExecutionPolicy unrestricted -command ".\Start-FemahBuild.ps1 %TASK% %MSBUILD% %CONFIGMODE% %SOLUTIONFILES% %BUILDCOUNTER% %GITPATH%;exit $LASTEXITCODE"

echo. %ERRORLEVEL%

if %ERRORLEVEL% == 0 goto OK
echo ##teamcity[buildStatus status='FAILURE' text='{build.status.text} in execution']
exit /b %ERRORLEVEL%

:OK