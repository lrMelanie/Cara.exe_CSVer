#!/usr/bin/env bash
# EVP Recorder: electronic voice phenomenon capture. Pure theater.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"
R="\033[38;5;196m"; W="\033[38;5;231m"; D="\033[38;5;240m"; A="\033[38;5;214m"; C="\033[38;5;51m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.02; done; printf '\n'; }

printf '\033[2J\033[H'
printf "${R}  ELECTRONIC VOICE PHENOMENON RECORDER  v5.0${X}\n\n"; sleep 0.5
printf "${D}"; typ "> input: default mic   gain: +42 dB   filter: none"
typ "> recording ..."; printf "${X}\n"; sleep 0.6

voices=("get out" "behind you" "$U" "we see the light" "don't turn around" "it is cold here" "let us in" "you opened the door" "help me" "run")
while true; do
  w=20; bar=""
  for ((i=0;i<40;i++)); do l=$((RANDOM%w)); if [ $i -lt $l ]; then bar="$bar█"; else bar="$bar▁"; fi; done
  db=$((RANDOM%60-60))
  printf "${C}  %+3d dB [%b]${X}\n" "$db" "$bar"
  if [ $((RANDOM%6)) -eq 0 ]; then
    v="${voices[$((RANDOM%10))]}"
    printf "${A}  >> CLASS A EVP CAPTURED${X}\n"; printf '\a'; sleep 0.3
    printf "${W}  \""; typ "$v\""; sleep 0.5
  fi
  if [ $((RANDOM%40)) -eq 0 ]; then
    printf "${R}  !! the recording contains a voice that is not yours, %s${X}\n" "$U"; printf '\a'; sleep 1.0
  fi
  sleep 0.15
done
