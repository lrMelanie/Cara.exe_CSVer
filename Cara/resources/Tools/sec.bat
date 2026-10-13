@echo off
setlocal enabledelayedexpansion
color 0B
title Static Echo Chamber

set "U=%USERNAME%"

cls
echo   STATIC ECHO CHAMBER
echo.
echo   ^> listening .....
ping -n 2 127.0.0.1 >nul
echo.

:loop
set /a "p=!random! %% 8"
set "base=hello"
if !p!==1 set "base=is anyone there"
if !p!==2 set "base=%U%"
if !p!==3 set "base=let me out"
if !p!==4 set "base=i am fine"
if !p!==5 set "base=i want to go home"
if !p!==6 set "base=stop"
if !p!==7 set "base=who said that"
echo   you: "!base!"
ping -n 1 127.0.0.1 >nul
for /l %%e in (1,1,5) do (
  echo       echo: "!base!"
  ping -n 1 127.0.0.1 >nul
)
set /a "w=!random! %% 5"
if !w!==0 (
  echo       echo: "that was not your voice, %U%"
  powershell -c "[console]::beep(110,500)" >nul 2>&1
  ping -n 2 127.0.0.1 >nul
)
powershell -c "[console]::beep(400,80)" >nul 2>&1
echo.
goto loop
