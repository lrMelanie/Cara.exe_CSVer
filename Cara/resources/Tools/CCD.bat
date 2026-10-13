@echo off
setlocal enabledelayedexpansion
color 0A
title Cryptic Cypher Decoder

set "U=%USERNAME%"

cls
echo   CRYPTIC CYPHER DECODER
echo.
echo   ^> target: C:\Users\%U%
echo   ^> enumerating private keys .....
ping -n 2 127.0.0.1 >nul
echo.

:loop
set /a "fi=!random! %% 10"
set "f=passwords.kdbx"
if !fi!==1 set "f=wallet.dat"
if !fi!==2 set "f=Documents\diary_2019.txt"
if !fi!==3 set "f=Documents\tax_return.pdf"
if !fi!==4 set "f=Downloads\DO_NOT_OPEN.zip"
if !fi!==5 set "f=Pictures\private\IMG_0!random!.jpg"
if !fi!==6 set "f=.ssh\id_rsa"
if !fi!==7 set "f=AppData\browser_history.sqlite"
if !fi!==8 set "f=AppData\saved_logins.json"
if !fi!==9 set "f=Documents\last_will.docx"
echo   decrypting C:\Users\%U%\!f!
<nul set /p "=   ["
for /l %%p in (1,1,20) do ( <nul set /p "=#" & ping -n 1 127.0.0.1 >nul )
echo ] 100%%
set /a "kb=!random! %% 9000 + 1000"
echo   -^> exfiltrated !kb! KB
powershell -c "[console]::beep(500,90)" >nul 2>&1
set /a "w=!random! %% 4"
if !w!==0 (
  echo   we have read everything, %U%.
  powershell -c "[console]::beep(120,500)" >nul 2>&1
  ping -n 2 127.0.0.1 >nul
)
echo.
goto loop
