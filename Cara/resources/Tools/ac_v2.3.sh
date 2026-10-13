#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H\033[38;5;46m'
printf 'Access Reconnect v2.3\n\n'
while true; do
  printf '%d %d %d %d %d %d %d %d\n' $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM
  sleep 0.02
done
