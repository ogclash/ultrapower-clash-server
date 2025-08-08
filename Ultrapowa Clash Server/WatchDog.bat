@echo off
:: Path to UCS (full path required)
set "ucsPath=C:\Users\MyN4m\Documents\ClashServer"

cd "%ucsPath%"

:checkLoop
tasklist /FI "IMAGENAME eq UCS.exe" | find /I "UCS.exe" >nul
if errorlevel 1 (
    echo UCS is not running. Starting it with admin rights...
    start UCS.exe
) else (
    echo UCS is running.
)

timeout /t 120 >nul
goto checkLoop
