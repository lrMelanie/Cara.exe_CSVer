#!/usr/bin/env bash
# Eyes in the Static: something watches from the noise. Pure theater.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"
W="\033[38;5;231m"; R="\033[38;5;196m"; D="\033[38;5;240m"; X="\033[0m"
COLS=46; ROWS=14
glyph=('░' '▒' '▓' '█' ' ' '▒' '░')

printf '\033[2J\033[H'
printf "${D}  EYES IN THE STATIC${X}\n"; sleep 0.4

count=0
while true; do
  count=$((count+1))
  printf '\033[2;1H'
  eye=$((RANDOM%9))
  er=$((RANDOM%ROWS)); ec=$((RANDOM%(COLS-6)+2))
  for ((y=0;y<ROWS;y++)); do
    line=""
    for ((x=0;x<COLS;x++)); do
      if [ $eye -eq 0 ] && [ $y -eq $er ] && { [ $x -eq $ec ] || [ $x -eq $((ec+3)) ]; }; then
        line="$line${W}●${D}"
      else
        line="$line${glyph[$((RANDOM%7))]}"
      fi
    done
    printf "${D}%b${X}\n" "$line"
  done
  if [ $eye -eq 0 ]; then printf '\a'; fi
  if [ $((count%17)) -eq 0 ]; then
    printf '\033[8;14H'; printf "${R}  I SEE YOU, %s  ${X}" "$U"; printf '\a'; sleep 0.7
  fi
  sleep 0.09
done
