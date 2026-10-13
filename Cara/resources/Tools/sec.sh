#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H'
printf 'Static Echo Chamber\n'
while true; do
  c=$((RANDOM%94+33))
  printf '\033[38;5;%dm' "$c"
  printf '\u25a0 \u25a0 \u25a0 \u25a0 \u25a0 \u25a0 \u25a0\n'
  sleep 0.05
done
