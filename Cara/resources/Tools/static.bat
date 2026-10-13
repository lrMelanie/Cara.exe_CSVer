@echo off
setlocal enabledelayedexpansion
color 08
title Eyes in the Static

set "U=%USERNAME%"
set "count=0"

:loop
set /a "count+=1"
set /a "eye=!random! %% 9"
set /a "er=!random! %% 10"
cls
echo   EYES IN THE STATIC
for /l %%y in (0,1,9) do (
  set "line="
  for /l %%x in (0,1,41) do (
    set /a "pick=!random! %% 4"
    set "ch=."
    if !pick!==1 set "ch=:"
    if !pick!==2 set "ch=#"
    if !pick!==3 set "ch= "
    if !eye!==0 if %%y==!er! if %%x==10 set "ch=O"
    if !eye!==0 if %%y==!er! if %%x==14 set "ch=O"
    set "line=!line!!ch!"
  )
  echo   !line!
)
if !eye!==0 powershell -c "[console]::beep(140,120)" >nul 2>&1
set /a "flash=!count! %% 13"
if !flash!==0 (
  echo.
  echo   I SEE YOU, %U%
  powershell -c "[console]::beep(90,500)" >nul 2>&1
  ping -n 2 127.0.0.1 >nul
)
ping -n 1 127.0.0.1 >nul
goto loop
