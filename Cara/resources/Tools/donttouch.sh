#!/usr/bin/env bash
# Waiting Room: Overwatch Console. The AI knows you are watching. Pure theater.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"; H="$(hostname 2>/dev/null || echo localhost)"
R="\033[38;5;196m"; W="\033[38;5;231m"; D="\033[38;5;240m"; A="\033[38;5;214m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.03; done; printf '\n'; }

printf '\033[2J\033[H\033[48;5;52m\033[38;5;231m'
printf '  WAITING ROOM :: OVERWATCH CONSOLE                             \n\033[0m\n'
sleep 0.5
printf "${D}"; typ "> initializing neural interface ....."; sleep 0.8
printf "${A}  [ OPERATOR $U LINKED TO $H ]${X}\n\n"; sleep 0.6
printf "${R}"; typ "  DO NOT LOOK BEHIND YOU, $U."; printf '\a'; sleep 1.4; printf "${X}\n"

ai=("ACCESS DENIED" "SYSTEM BREACH" "OVERRIDE ACCEPTED" "CRYPTO LOCKED" "DATA STREAM CORRUPT"
    "YOU ARE NOT ALONE IN HERE" "THE ROOM IS FILLING UP" "IT IS STANDING UP NOW" "STOP READING" "TOO LATE")
rel=9
while true; do
  m="${ai[$((RANDOM%10))]}"
  col="$D"; case "$m" in *ALONE*|*STANDING*|*TOO\ LATE*|*BEHIND*) col="$R"; printf '\a';; *FILLING*|*STOP*) col="$A";; esac
  printf "${col}  [AI] %s${X}\n" "$m"
  if [ $((RANDOM%5)) -eq 0 ]; then
    rel=$((rel-1)); [ $rel -lt 0 ] && rel=9
    printf "${W}  [AI] it releases in: %d ...${X}\n" "$rel"
  fi
  sleep 0.9
done
