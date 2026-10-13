@echo off
setlocal enabledelayedexpansion
color 0A
title Dead Signal Monitor

set "U=%USERNAME%"
set /a "last=!random! %% 48 + 2"

cls
echo   DEAD SIGNAL MONITOR  [carrier 666.6 MHz]
echo.
echo   ^> last confirmed contact: %last%h ago
ping -n 2 127.0.0.1 >nul
echo   ^> the station went dark. the signal did not.
echo.
ping -n 2 127.0.0.1 >nul

set /a "snr=92"
:loop
set /a "d=!random! %% 4"
set /a "snr=!snr! - !d!"
if !snr! lss 6 ( set /a "snr=!random! %% 30 + 70" & echo   !! CARRIER LOST - REACQUIRING !! & powershell -c "[console]::beep(220,300)" >nul 2>&1 )
set /a "f=!snr! / 5"
set "bar="
for /l %%i in (1,1,20) do ( if %%i leq !f! ( set "bar=!bar!#" ) else ( set "bar=!bar!." ) )
echo   SNR [!bar!] !snr!%%
set /a "w=!random! %% 5"
if !w!==0 (
  set /a "v=!random! %% 5"
  if !v!==0 echo   EVP^> ...is anyone there...
  if !v!==1 echo   EVP^> ...%U%...can you hear us...
  if !v!==2 echo   EVP^> ...do not answer...
  if !v!==3 echo   EVP^> ...it followed the signal home...
  if !v!==4 echo   EVP^> ...we never left...
  powershell -c "[console]::beep(180,200)" >nul 2>&1
) else (
  set "line="
  for /l %%i in (1,1,40) do ( set /a "n=!random! %% 100" & if !n! lss !snr! ( set "line=!line!:" ) else ( set "line=!line! " ) )
  echo   !line!
)
ping -n 1 127.0.0.1 >nul
goto loop
