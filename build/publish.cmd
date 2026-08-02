@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish.ps1" %*
set EXITCODE=%ERRORLEVEL%
if not "%EXITCODE%"=="0" (
  echo.
  echo Veröffentlichung fehlgeschlagen. Fehlercode: %EXITCODE%
)
exit /b %EXITCODE%
