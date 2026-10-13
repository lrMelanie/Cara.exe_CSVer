#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H\033[48;5;231m\033[38;5;16m'
printf 'Firewall Bypass Toolkit\n'
while true; do
  for c in '\' '/' '|' '-'; do
    printf '                %s%s%s%s%s%s%s%s\n' "$c" "$c" "$c" "$c" "$c" "$c" "$c" "$c"
    sleep 0.08
    clear
  done
done
