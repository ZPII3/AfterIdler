@echo off

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Administrator privileges are required.
    echo Please run this file as Administrator.
    pause
    exit /b 1
)

set SERVICE_NAME=AfterIdlerSensorService

echo.
echo Stopping %SERVICE_NAME%...
echo.

sc.exe stop "%SERVICE_NAME%"

timeout /t 2 /nobreak >nul

echo.
echo Removing %SERVICE_NAME%...
echo.

sc.exe delete "%SERVICE_NAME%"

echo.
echo Service removed.
echo.

pause