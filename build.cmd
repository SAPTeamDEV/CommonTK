@echo off
setlocal

SET "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

SET "CACHE_DIR=%SCRIPT_DIR%__pycache__"

:: Create cache directory if it doesn't exist
IF NOT EXIST "%CACHE_DIR%" (
    mkdir "%CACHE_DIR%"
)

:: Set PYBITE_APP_NAME to the current script file name
SET "PYBITE_APP_NAME=%~nx0"

:: Set env var for this process and launch Python
SET PYTHONPYCACHEPREFIX=%CACHE_DIR%

python "%SCRIPT_DIR%build.py" %*

endlocal