@echo off

rem the following resets %ERRORLEVEL% to 0 prior to running powershell
verify >nul
echo. %ERRORLEVEL%

powershell.exe -NoProfile -ExecutionPolicy unrestricted -command ".\Start-FemahBuild.ps1;exit $LASTEXITCODE"

echo. %ERRORLEVEL%

if %ERRORLEVEL% == 0 goto OK
echo ##teamcity[buildStatus status='FAILURE' text='{build.status.text} in execution']
exit /b %ERRORLEVEL%

:OK