@echo off
setlocal enabledelayedexpansion
color 0B
title Blockchain Integrity Ledger

set "U=%USERNAME%"
set "HX=0123456789abcdef"

cls
echo   BLOCKCHAIN INTEGRITY LEDGER
echo.
echo   ^> syncing chain .....
echo   ^> actor identity: %U% (immutable)
ping -n 2 127.0.0.1 >nul
echo   every keystroke you make is now permanent.
powershell -c "[console]::beep(300,150)" >nul 2>&1
echo.

set /a "n=!random! %% 90000 + 10000"
call :hash
set "prev=!h!"
:loop
set /a "n+=1"
call :hash
echo   BLOCK #!n!
echo     hash .. !h!
echo     prev .. !prev!
echo     actor . %U%
set /a "ai=!random! %% 10"
set "act=opened terminal"
if !ai!==1 set "act=read a private file"
if !ai!==2 set "act=looked away from screen"
if !ai!==3 set "act=typed a password"
if !ai!==4 set "act=deleted evidence"
if !ai!==5 set "act=lied"
if !ai!==6 set "act=hesitated"
if !ai!==7 set "act=pressed a key"
if !ai!==8 set "act=breathed"
if !ai!==9 set "act=kept reading"
echo     act ... !act!
set "prev=!h!"
set /a "w=!random! %% 6"
if !w!==0 (
  echo     ^>^> this block cannot be deleted. neither can you.
  powershell -c "[console]::beep(120,400)" >nul 2>&1
  ping -n 2 127.0.0.1 >nul
)
ping -n 1 127.0.0.1 >nul
goto loop

:hash
set "h="
for /l %%i in (1,1,24) do (
  set /a "d=!random! %% 16"
  for %%d in (!d!) do set "h=!h!!HX:~%%d,1!"
)
goto :eof
