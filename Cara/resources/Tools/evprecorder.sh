#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H\033[38;5;196m'
printf 'Electronic Voice Phenomenon Recorder v5.0\n'
while true; do
  line=""
  for i in $(seq 1 80); do line="$line$((RANDOM%2))"; done
  printf '%s\n' "$line"
  sleep 0.02
done
