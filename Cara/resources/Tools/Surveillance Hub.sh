#!/usr/bin/env bash
# Deep Web Surveillance Hub: fake remote-viewing session. Pure theater, no real capture.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"; H="$(hostname 2>/dev/null || echo localhost)"
IP="$(hostname -I 2>/dev/null | awk '{print $1}')"; [ -z "$IP" ] && IP="10.0.0.$((RANDOM%254+1))"
R="\033[38;5;196m"; W="\033[38;5;231m"; D="\033[38;5;240m"; Y="\033[38;5;46m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.03; done; printf '\n'; }

printf '\033[2J\033[H\033[48;5;17m\033[38;5;231m'
printf '  DEEP WEB SURVEILLANCE HUB  v9.3                               \n\033[0m\n'
sleep 0.6
printf "${D}"; typ "> establishing remote viewing session ............."
sleep 0.4; printf "${Y}  [ LINK OPEN ]${X}\n\n"; sleep 0.5
printf "${W}"; typ "  SUBJECT ... $U"
typ "  NODE ...... $H"
typ "  ADDRESS ... $IP"; printf '\n'; sleep 0.5
printf "${D}> acquiring optical device ........"; sleep 1.1; printf "${R} CAMERA ONLINE${X}\n"; printf '\a'
sleep 0.8; printf "${W}"; typ "  motion detected in frame."
sleep 0.6; typ "  running facial recognition ..."
sleep 1.0; printf "${Y}  MATCH: $U  (confidence 98.7%%)${X}\n\n"; printf '\a'
sleep 1.0; printf "${R}"; typ "  I can see you, $U."; sleep 1.2
typ "  you should smile for the camera."; sleep 1.4; printf "${X}\n"

while true; do
  t="$(date '+%H:%M:%S')"
  f=$((RANDOM%9000+1000))
  case $((RANDOM%7)) in
    0) printf "${D}[$t] frame $f captured  ->  cam_%s.jpg\n" "$f";;
    1) printf "${D}[$t] motion vector  dx=%+d  dy=%+d\n" $((RANDOM%40-20)) $((RANDOM%40-20));;
    2) printf "${W}[$t] face still in frame. good.\n";;
    3) printf "${R}[$t] subject looked away. look back.\n"; printf '\a';;
    4) printf "${D}[$t] uploading to node $IP ...\n";;
    5) printf "${Y}[$t] why are you still reading this, $U ?\n";;
    6) printf "${D}[$t] heartbeat %d bpm\n" $((RANDOM%30+70));;
  esac
  sleep 0.6
done
