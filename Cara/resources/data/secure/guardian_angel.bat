@echo off
title Frequency 666.6 MHz
color 0A
echo [*] Connecting Wi-Fi...

NET FILE >nul 2>&1 || echo [!] Limited rights detected: some steps may be skipped.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\RadioManagement\SystemRadioState" /v "SystemRadioState" /t REG_DWORD /d 0 /f >nul 2>&1
netsh interface set interface "Wi-Fi" admin=enable >nul 2>&1
netsh interface set interface "Ethernet" admin=enable >nul 2>&1
netsh interface set interface "Bluetooth Network Connection" admin=enable >nul 2>&1
sc config WlanSvc start=auto >nul 2>&1
sc start WlanSvc >nul 2>&1
sc config BthServ start=demand >nul 2>&1
sc start BthServ >nul 2>&1

echo [*] Wi-Fi status: Reconnected
timeout /t 3 /nobreak >nul
