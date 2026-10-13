@echo off
setlocal enabledelayedexpansion
color 0F
title Firewall Bypass Toolkit

set "U=%USERNAME%"
set "H=%COMPUTERNAME%"
set "IP="
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4"') do if not defined IP set "IP=%%a"
if defined IP set "IP=%IP: =%"
if not defined IP ( set /a "o=!random! %% 254 + 1" & set "IP=192.168.1.!o!" )

cls
echo   FIREWALL BYPASS TOOLKIT
echo.
echo   ^> target ... %H% (%IP%)
echo   ^> operator . %U%
echo.
ping -n 2 127.0.0.1 >nul
for %%p in (22 80 443 445 3389 5900 8080 31337) do (
  <nul set /p "=   scanning port %%p ..."
  ping -n 1 127.0.0.1 >nul
  set /a "s=!random! %% 3"
  if !s!==0 ( echo  OPEN ) else ( echo  filtered )
)
ping -n 2 127.0.0.1 >nul
echo   disabling firewall rules .....
ping -n 2 127.0.0.1 >nul
echo   FIREWALL DOWN
powershell -c "[console]::beep(300,250)" >nul 2>&1
echo   shell acquired on %H%.
ping -n 2 127.0.0.1 >nul
echo.

:loop
set /a "r=!random! %% 6"
if !r!==0 ( echo   %U%@%H%:~$ whoami & ping -n 1 127.0.0.1 >nul & echo     root )
if !r!==1 ( set /a "a=!random! %% 223 + 1" & set /a "b=!random! %% 256" & set /a "c=!random! %% 256" & set /a "pt=!random! %% 60000 + 1024" & echo   [tcp] %IP%:!pt! -^> !a!.!b!.!c!:443  ESTABLISHED )
if !r!==2 ( set /a "kb=!random! %% 4096" & echo   keylogger armed. buffer: !kb! bytes )
if !r!==3 ( echo   %U%@%H%:~$ type C:\Windows\System32\config\SAM & echo     permission granted. & powershell -c "[console]::beep(200,200)" >nul 2>&1 )
if !r!==4 ( set /a "n=!random! %% 99" & echo   persistence installed: HKCU\...\Run\.sys!n! )
if !r!==5 ( echo   we own %H% now, %U%. & powershell -c "[console]::beep(120,400)" >nul 2>&1 )
ping -n 2 127.0.0.1 >nul
goto loop
