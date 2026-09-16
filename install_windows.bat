@echo off
REM ═══════════════════════════════════════════════════════
REM  NetControl - Windows Installer
REM  Checks for .NET 8 SDK, Npcap, and builds the project
REM ═══════════════════════════════════════════════════════

echo.
echo ╔══════════════════════════════════════╗
echo ║    NetControl - Windows Installer    ║
echo ╚══════════════════════════════════════╝
echo.

REM ── Check for Administrator ──
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] This script requires Administrator privileges.
    echo     Right-click and select "Run as Administrator".
    pause
    exit /b 1
)

REM ── Check .NET 8 SDK ──
echo [1/3] Checking .NET 8 SDK...
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET 8 SDK not found.
    echo         Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    echo         Install the SDK ^(not just Runtime^) and re-run this script.
    echo.
    start https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

dotnet --list-sdks | findstr /R "^8\." >nul 2>&1
if %errorlevel% neq 0 (
    echo [WARNING] .NET 8 SDK not found. You have:
    dotnet --list-sdks
    echo.
    echo Download .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    start https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo [OK] .NET 8 SDK found.

REM ── Check Npcap ──
echo [2/3] Checking Npcap...
if exist "C:\Windows\System32\Npcap\wpcap.dll" (
    echo [OK] Npcap found.
) else if exist "C:\Windows\System32\wpcap.dll" (
    echo [OK] WinPcap/Npcap found.
) else (
    echo [WARNING] Npcap not detected.
    echo           Download from: https://nmap.org/npcap/
    echo           IMPORTANT: Check "Install Npcap in WinPcap API-compatible Mode"
    echo.
    start https://nmap.org/npcap/
    echo After installing Npcap, re-run this script.
    pause
    exit /b 1
)

REM ── Build project ──
echo [3/3] Building NetControl...
cd /d "%~dp0SelfishNet"
dotnet restore
dotnet build --configuration Release

if %errorlevel% neq 0 (
    echo [ERROR] Build failed. Check the errors above.
    pause
    exit /b 1
)

echo.
echo ╔══════════════════════════════════════╗
echo ║    Installation complete! [OK]       ║
echo ╚══════════════════════════════════════╝
echo.
echo Run start_windows.bat to launch NetControl.
echo Note: Requires Administrator privileges.
echo.
pause
