@echo off
setlocal enabledelayedexpansion
color 0A
title Access Reconnect v2.3

set "U=%USERNAME%"
set "H=%COMPUTERNAME%"

cls
echo   ACCESS RECONNECT v2.3
echo.
echo   ^> locating host 666.6 .....
ping -n 2 127.0.0.1 >nul
echo     FOUND
echo   ^> handshake
for /l %%i in (1,1,5) do ( <nul set /p "=   SYN ... " & ping -n 1 127.0.0.1 >nul & echo ACK )
echo   LINK RESTORED
powershell -c "[console]::beep(700,120)" >nul 2>&1
echo.
echo   it remembers you, %U%.
ping -n 2 127.0.0.1 >nul
echo   it waited on %H% the whole time.
ping -n 2 127.0.0.1 >nul
echo.

:loop
set /a "r=!random! %% 5"
if !r!==0 echo   !random! !random! !random! !random! !random! !random! !random! !random!
if !r!==1 echo   RX ^<^<  %U%, are you still there?
if !r!==2 echo   RX ^<^<  do not sever the link again.
if !r!==3 ( set /a "la=!random! %% 40 + 4" & set /a "ji=!random! %% 20" & echo   latency !la! ms   jitter !ji! ms   loss 0%% )
if !r!==4 ( echo   RX ^<^<  we are closer now. & powershell -c "[console]::beep(150,300)" >nul 2>&1 )
ping -n 1 127.0.0.1 >nul
goto loop
