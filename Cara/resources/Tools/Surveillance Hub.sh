#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H\033[48;5;19m\033[38;5;231m'
printf 'Deep Web Surveillance Hub\n'
while true; do
  printf 'CPU USAGE: %d%%\n' $((RANDOM%100+1))
  sleep 0.2
  clear
done
