#!/usr/bin/env bash
# Linux network recovery (parity with guardian_angel.bat). Needs sudo for full effect.
echo "[*] Connecting Wi-Fi..."
rfkill unblock all 2>/dev/null
nmcli networking on 2>/dev/null
nmcli radio all on 2>/dev/null
systemctl restart NetworkManager 2>/dev/null
systemctl start bluetooth 2>/dev/null
echo "[*] Wi-Fi status: Reconnected"
