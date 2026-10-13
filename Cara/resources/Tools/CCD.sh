#!/usr/bin/env bash
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
printf '\033[2J\033[H\033[38;5;46m'
printf 'Cryptic Cypher Decoder\n'
while true; do
  printf 'Decrypting: %d\n' $((RANDOM%999999))
  printf '\a'
  sleep 0.1
  clear
done
