@echo off
setlocal enabledelayedexpansion
color 4F
title Waiting Room: Overwatch Console

set "U=%USERNAME%"
set "H=%COMPUTERNAME%"

for /f %%A in ('powershell -Command "[System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width" 2^>nul') do set "width=%%A"
for /f %%A in ('powershell -Command "[System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height" 2^>nul') do set "height=%%A"
if "%width%"=="" set "width=1920"
if "%height%"=="" set "height=1080"
set /a maxX=width-520
set /a maxY=height-320
if %maxX% lss 1 set "maxX=1"
if %maxY% lss 1 set "maxY=1"
call :move

cls
echo   WAITING ROOM :: OVERWATCH CONSOLE
echo.
echo   ^> initializing neural interface .....
ping -n 2 127.0.0.1 >nul
echo     [ OPERATOR %U% LINKED TO %H% ]
echo.
echo   DO NOT LOOK BEHIND YOU, %U%.
powershell -c "[console]::beep(120,500)" >nul 2>&1
ping -n 2 127.0.0.1 >nul
echo.

set /a "rel=9"
:loop
set /a "m=!random! %% 10"
if !m!==0 echo   [AI] ACCESS DENIED
if !m!==1 echo   [AI] SYSTEM BREACH
if !m!==2 echo   [AI] OVERRIDE ACCEPTED
if !m!==3 echo   [AI] CRYPTO LOCKED
if !m!==4 echo   [AI] DATA STREAM CORRUPT
if !m!==5 ( echo   [AI] YOU ARE NOT ALONE IN HERE & powershell -c "[console]::beep(200,200)" >nul 2>&1 )
if !m!==6 echo   [AI] THE ROOM IS FILLING UP
if !m!==7 ( echo   [AI] IT IS STANDING UP NOW & powershell -c "[console]::beep(160,250)" >nul 2>&1 & call :move )
if !m!==8 echo   [AI] STOP READING
if !m!==9 ( echo   [AI] TOO LATE & powershell -c "[console]::beep(100,400)" >nul 2>&1 )
set /a "k=!random! %% 5"
if !k!==0 (
  set /a "rel=!rel! - 1"
  if !rel! lss 0 set /a "rel=9"
  echo   [AI] it releases in: !rel! ...
)
ping -n 2 127.0.0.1 >nul
goto loop

:move
set /a "rx=!random! %% maxX"
set /a "ry=!random! %% maxY"
powershell -Command "Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;using System.Diagnostics;public class WinMover{[DllImport(\"user32.dll\")]public static extern bool MoveWindow(IntPtr hWnd,int X,int Y,int W,int H,bool bRepaint);public static void Go(int x,int y){IntPtr h=Process.GetCurrentProcess().MainWindowHandle;MoveWindow(h,x,y,520,320,true);}}';[WinMover]::Go(%rx%,%ry%)" >nul 2>&1
goto :eof
