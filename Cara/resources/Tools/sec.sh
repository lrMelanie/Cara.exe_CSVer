#!/usr/bin/env bash
# Static Echo Chamber: your own words come back wrong. Pure theater.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"
W="\033[38;5;231m"; R="\033[38;5;196m"; D="\033[38;5;240m"; C="\033[38;5;51m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.03; done; printf '\n'; }

printf '\033[2J\033[H'
printf "${C}  STATIC ECHO CHAMBER${X}\n\n"; sleep 0.4
printf "${D}"; typ "> listening ....."; printf "${X}\n"; sleep 0.6

phrases=("hello" "is anyone there" "$U" "let me out" "i am fine" "i want to go home" "stop" "who said that")
colors="240 240 244 248 231 196"
while true; do
  base="${phrases[$((RANDOM%8))]}"
  printf "${W}  you: \"%s\"${X}\n" "$base"; sleep 0.4
  for c in $colors; do
    # distort the echo: drop or double letters
    e=""; for ((i=0;i<${#base};i++)); do r=$((RANDOM%10)); ch="${base:$i:1}";
      if [ $r -lt 7 ]; then e="$e$ch"; elif [ $r -lt 9 ]; then e="$e$ch$ch"; fi; done
    printf "\033[38;5;%sm    echo: \"%s\"\033[0m\n" "$c" "$e"; sleep 0.12
  done
  if [ $((RANDOM%5)) -eq 0 ]; then printf "${R}    echo: \"that was not your voice, %s\"${X}\n" "$U"; printf '\a'; sleep 0.8; fi
  printf '\a'; sleep 0.5
done
