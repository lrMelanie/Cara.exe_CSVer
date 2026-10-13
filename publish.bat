@echo off
cd /d "%~dp0"
echo [*] Closing any running Cara.exe (main window + CARA:: pane windows)...
taskkill /IM Cara.exe /F >nul 2>&1
timeout /t 1 /nobreak >nul
echo [*] Publishing standalone Cara.exe (self-contained, single file)...
dotnet publish Cara\Cara.csproj -c Release -f net8.0-windows -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
if errorlevel 1 ( echo [ERROR] Publish failed. If it says the file is in use, close Cara.exe and every CARA:: pane window, then run this again. & pause & exit /b 1 )
echo.
echo [OK] Done. Standalone app is in the "publish" folder:
echo     publish\Cara.exe   (double-click to run; no .NET install needed on the target PC)
pause
