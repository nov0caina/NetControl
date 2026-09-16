@echo off
REM ═══════════════════════════════════════════════════════
REM  NetControl - Windows Launcher
REM  Runs NetControl as Administrator
REM ═══════════════════════════════════════════════════════

echo.
echo ╔══════════════════════════════════════╗
echo ║    NetControl - Windows Launcher     ║
echo ╚══════════════════════════════════════╝
echo.

REM ── Check for Administrator ──
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] NetControl requires Administrator privileges.
    echo     Relaunching as Administrator...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

REM ── Check if built ──
if not exist "%~dp0SelfishNet\bin\Release\net8.0\NetControl.dll" if not exist "%~dp0SelfishNet\bin\Release\net8.0\NetControl.exe" if not exist "%~dp0SelfishNet\bin\Release\net8.0\SelfishNet.dll" (
    echo [ERROR] NetControl not built. Run install_windows.bat first.
    pause
    exit /b 1
)

REM ── Launch ──
echo [OK] Launching NetControl...
echo.
cd /d "%~dp0SelfishNet"
dotnet run --configuration Release --no-build
exit_code=%errorlevel%

echo.
echo NetControl closed.
pause
exit /b %exit_code%
