@echo off
REM ----------------------------------------------------------------
REM Windows batch wrapper to invoke the PowerShell Bite script
REM ----------------------------------------------------------------
setlocal

REM Determine script folder
set "SCRIPT_DIR=%~dp0"

REM Choose PowerShell host: prefer pwsh (PowerShell Core), fallback to Windows PowerShell
where pwsh >nul 2>&1
if %errorlevel%==0 (
    set "PS_CMD=pwsh"
) else (
    set "PS_CMD=powershell"
)

REM Check if arguments are provided
if "%~1"=="" (
    REM No arguments provided, set default argument
    set args=build
) else (
    REM Use provided arguments
    set args=%*
)

REM Invoke the PowerShell wrapper with all passed arguments
"%PS_CMD%" -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%build.ps1" %args%

endlocal
