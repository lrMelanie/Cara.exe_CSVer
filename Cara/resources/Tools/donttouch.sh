#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H\033[48;5;52m\033[38;5;231m'
printf 'Waiting Room: Overwatch Console\n'
printf 'Initializing neural interface...\n'
sleep 2
resp=("ACCESS DENIED" "SYSTEM BREACH" "OVERRIDE ACCEPTED" "CRYPTO LOCKED" "DATA STREAM CORRUPT")
while true; do
  printf '[AI] %s\n' "${resp[$((RANDOM%5))]}"
  sleep 1
done
