#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H\033[38;5;51m'
printf 'Blockchain Integrity Check\n\n'
while true; do
  printf '%06d%06d%06d\n' $((RANDOM%1000000)) $((RANDOM%1000000)) $((RANDOM%1000000))
  sleep 0.05
done
