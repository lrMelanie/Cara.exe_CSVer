#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H'
printf 'Eyes in the Static\n'
colors="46 196 226 33 51"
while true; do
  for c in $colors; do
    printf '\033[38;5;%dm' "$c"
    printf '\u25a0\u2592\u256c\u2584\u00b1\u2261\u25a0\u2592\u256c\u2584\u00b1\u2261\u25a0\u2592\u256c\u2584\u00b1\u2261\n'
    sleep 0.05
  done
  clear
done
