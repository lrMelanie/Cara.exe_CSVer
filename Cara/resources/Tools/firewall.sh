#!/usr/bin/env bash
# Firewall Bypass Toolkit: it is already inside. Pure theater, no real network action.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"; H="$(hostname 2>/dev/null || echo localhost)"
IP="$(hostname -I 2>/dev/null | awk '{print $1}')"; [ -z "$IP" ] && IP="192.168.1.$((RANDOM%254+1))"
G="\033[38;5;46m"; R="\033[38;5;196m"; D="\033[38;5;240m"; W="\033[38;5;231m"; A="\033[38;5;214m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.02; done; printf '\n'; }

printf '\033[2J\033[H'
printf "${W}  FIREWALL BYPASS TOOLKIT${X}\n\n"; sleep 0.4
printf "${D}"; typ "> target ... $H ($IP)"; typ "> operator . $U"; printf "${X}\n"; sleep 0.6
ports=(22 80 443 445 3389 5900 8080 31337)
for p in "${ports[@]}"; do
  printf "${D}  scanning port %-5d ..." "$p"; sleep 0.18
  if [ $((RANDOM%3)) -eq 0 ]; then printf "${G} OPEN${X}\n"; else printf "${R} filtered${X}\n"; fi
done
sleep 0.4; printf "${A}"; typ "  disabling firewall rules ....."; sleep 1.0; printf "${R}  FIREWALL DOWN${X}\n"; printf '\a'
sleep 0.5; printf "${G}"; typ "  shell acquired on $H."; sleep 0.8; printf "${X}\n"

while true; do
  case $((RANDOM%6)) in
    0) printf "${G}%s@%s:~\$ ${W}" "$U" "$H"; typ "whoami"; printf "${D}  root${X}\n";;
    1) printf "${D}  [tcp] %s:%d -> %d.%d.%d.%d:%d  ESTABLISHED\n" "$IP" $((RANDOM%60000+1024)) $((RANDOM%223+1)) $((RANDOM%256)) $((RANDOM%256)) $((RANDOM%60000+1024));;
    2) printf "${A}  keylogger armed. buffer: %d bytes\n" $((RANDOM%4096));;
    3) printf "${G}%s@%s:~\$ ${W}" "$U" "$H"; typ "cat /etc/shadow"; printf "${R}  permission granted.${X}\n"; printf '\a';;
    4) printf "${D}  persistence installed: /etc/cron.d/.sys%d\n" $((RANDOM%99));;
    5) printf "${R}  we own %s now, %s.\n" "$H" "$U"; printf '\a';;
  esac
  sleep 0.55
done
