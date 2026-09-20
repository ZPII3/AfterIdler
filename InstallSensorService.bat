@echo off

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Administrator privileges are required.
    echo Please run this file as Administrator.
    pause
    exit /b 1
)

set SERVICE_NAME=AfterIdlerSensorService
set SERVICE_EXE=%~dp0AfterIdlerSensorService.exe

echo.
echo Installing %SERVICE_NAME%...
echo.

if not exist "%SERVICE_EXE%" (
    echo ERROR:
    echo Service executable was not found:
    echo "%SERVICE_EXE%"
    echo.
    pause
    exit /b 1
)

sc.exe create "%SERVICE_NAME%" ^
    binPath= "\"%SERVICE_EXE%\"" ^
    start= auto ^
    DisplayName= "AfterIdler Sensor Service"

if %errorlevel% neq 0 (
    echo.
    echo Service installation failed.
    pause
    exit /b 1
)

sc.exe description "%SERVICE_NAME%" ^
    "Temperature monitoring service for AfterIdler."

echo.
echo Service installed successfully.
echo.

sc.exe start "%SERVICE_NAME%"

if %errorlevel% neq 0 (
    echo.
    echo Service start failed.
    echo.
    echo Registered configuration:
    sc.exe qc "%SERVICE_NAME%"
    echo.
)

echo.
pause