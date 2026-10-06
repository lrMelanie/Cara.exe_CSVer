@echo off
cd /d "%~dp0"
echo [*] Publishing standalone Cara.exe (self-contained, single file)...
dotnet publish Cara\Cara.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
if errorlevel 1 ( echo [ERROR] Publish failed. & pause & exit /b 1 )
echo.
echo [OK] Done. Standalone app is in the "publish" folder:
echo     publish\Cara.exe   (double-click to run; no .NET install needed on the target PC)
pause
