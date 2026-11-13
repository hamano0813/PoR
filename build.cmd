@echo off
setlocal

cd /d "%~dp0"

set "PROFILE=win-x64-standalone"
set "OUTDIR=publish\win-x64"
if /I "%~1"=="framework" (
    set "PROFILE=win-x64-framework-dependent"
    set "OUTDIR=publish\win-x64-framework-dependent"
)

echo Building PoR.App (%PROFILE%)...
echo.

"%SystemRoot%\System32\tasklist.exe" /FI "IMAGENAME eq PoR.App.exe" 2>nul | "%SystemRoot%\System32\findstr.exe" /I /C:"PoR.App.exe" >nul
if not errorlevel 1 (
    echo [WARN] PoR.App.exe is running. Close it if publishing fails.
    echo.
)

dotnet publish src\PoR.App\PoR.App.csproj -c Release -p:PublishProfile=%PROFILE% --nologo
if errorlevel 1 (
    echo.
    echo Build failed.
    echo Press any key to close...
    pause >nul
    exit /b 1
)

rem Remove redundant publish folders generated inside each project.
for %%D in (src\PoR.App src\PoR.Core src\PoR.Game src\PoR.Memory.Dolphin) do (
    if exist "%%D\publish" rmdir /s /q "%%D\publish" 2>nul
)

echo.
echo Build finished: %CD%\%OUTDIR%\PoR.App.exe
if /I "%~1"=="framework" (
    echo This build requires the .NET 10 Desktop Runtime.
) else (
    echo This build is self-contained and does not require a .NET runtime.
)
echo.
echo Press any key to close...
pause >nul

endlocal
