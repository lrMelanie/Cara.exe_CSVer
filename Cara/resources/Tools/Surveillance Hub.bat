@echo off
setlocal enabledelayedexpansion
color 1F
title Deep Web Surveillance Hub v9.3

set "U=%USERNAME%"
set "H=%COMPUTERNAME%"
set "IP="
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4"') do if not defined IP set "IP=%%a"
if defined IP set "IP=%IP: =%"
if not defined IP ( set /a "o=!random! %% 254 + 1" & set "IP=10.0.0.!o!" )

cls
echo   DEEP WEB SURVEILLANCE HUB  v9.3
echo.
echo   ^> establishing remote viewing session .............
ping -n 2 127.0.0.1 >nul
echo     [ LINK OPEN ]
echo.
echo     SUBJECT ... %U%
echo     NODE ...... %H%
echo     ADDRESS ... %IP%
echo.
echo   ^> acquiring optical device ........
ping -n 2 127.0.0.1 >nul
echo     CAMERA ONLINE
powershell -c "[console]::beep(600,180)" >nul 2>&1
echo     running facial recognition ...
ping -n 2 127.0.0.1 >nul
echo     MATCH: %U%  (confidence 98.7%%)
echo.
ping -n 2 127.0.0.1 >nul
echo     I can see you, %U%.
ping -n 2 127.0.0.1 >nul
echo     You should smile for the camera.
powershell -c "[console]::beep(300,400)" >nul 2>&1
echo.

:loop
set /a "r=!random! %% 7"
set /a "f=!random! %% 9000 + 1000"
if !r!==0 echo   [%TIME:~0,8%] frame !f! captured  -^>  cam_!f!.jpg
if !r!==1 ( set /a "dx=!random! %% 40 - 20" & set /a "dy=!random! %% 40 - 20" & echo   [%TIME:~0,8%] motion vector  dx=!dx!  dy=!dy! )
if !r!==2 echo   [%TIME:~0,8%] face still in frame. good.
if !r!==3 ( echo   [%TIME:~0,8%] subject looked away. look back. & powershell -c "[console]::beep(900,120)" >nul 2>&1 )
if !r!==4 echo   [%TIME:~0,8%] uploading to node %IP% ...
if !r!==5 echo   [%TIME:~0,8%] why are you still reading this, %U% ?
if !r!==6 ( set /a "b=!random! %% 30 + 70" & echo   [%TIME:~0,8%] heartbeat !b! bpm )
ping -n 2 127.0.0.1 >nul
goto loop
