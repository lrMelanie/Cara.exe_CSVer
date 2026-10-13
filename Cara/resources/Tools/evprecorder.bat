@echo off
setlocal enabledelayedexpansion
color 0C
title Electronic Voice Phenomenon Recorder v5.0

set "U=%USERNAME%"

cls
echo   ELECTRONIC VOICE PHENOMENON RECORDER  v5.0
echo.
echo   ^> input: default mic   gain: +42 dB   filter: none
echo   ^> recording ...
echo.
ping -n 2 127.0.0.1 >nul

:loop
set "bar="
set /a "lvl=!random! %% 30 + 2"
for /l %%i in (1,1,30) do ( if %%i leq !lvl! ( set "bar=!bar!#" ) else ( set "bar=!bar!." ) )
set /a "db=!random! %% 60 - 60"
echo   !db! dB [!bar!]
set /a "c=!random! %% 6"
if !c!==0 (
  set /a "v=!random! %% 10"
  set "say=get out"
  if !v!==1 set "say=behind you"
  if !v!==2 set "say=%U%"
  if !v!==3 set "say=we see the light"
  if !v!==4 set "say=don't turn around"
  if !v!==5 set "say=it is cold here"
  if !v!==6 set "say=let us in"
  if !v!==7 set "say=you opened the door"
  if !v!==8 set "say=help me"
  if !v!==9 set "say=run"
  echo   ^>^> CLASS A EVP CAPTURED
  powershell -c "[console]::beep(160,180)" >nul 2>&1
  echo      "!say!"
)
set /a "j=!random! %% 40"
if !j!==0 (
  echo   !! the recording contains a voice that is not yours, %U%
  powershell -c "[console]::beep(110,600)" >nul 2>&1
  ping -n 2 127.0.0.1 >nul
)
ping -n 1 127.0.0.1 >nul
goto loop
