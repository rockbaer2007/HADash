@echo off
setlocal

set "SCRIPT=%~dp0publish.ps1"

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" %*

set "EXIT_CODE=%ERRORLEVEL%"

if not "%EXIT_CODE%"=="0" (
    echo.
    echo Veroeffentlichung fehlgeschlagen. Fehlercode: %EXIT_CODE%
    exit /b %EXIT_CODE%
)

echo.
echo Veroeffentlichung erfolgreich abgeschlossen.
exit /b 0
