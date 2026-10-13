#!/usr/bin/env bash
# Blockchain Integrity: every move you make is written to a ledger that cannot be erased. Theater.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"
C="\033[38;5;51m"; W="\033[38;5;231m"; D="\033[38;5;240m"; R="\033[38;5;196m"; A="\033[38;5;214m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.02; done; printf '\n'; }
hex(){ h=""; for ((i=0;i<${1:-16};i++)); do h="$h$(printf '%x' $((RANDOM%16)))"; done; printf '%s' "$h"; }

printf '\033[2J\033[H'
printf "${C}  BLOCKCHAIN INTEGRITY LEDGER${X}\n\n"; sleep 0.4
printf "${D}"; typ "> syncing chain ....."; typ "> actor identity: $U (immutable)"; printf "${X}\n"; sleep 0.6
printf "${A}"; typ "  every keystroke you make is now permanent."; sleep 1.0; printf "${X}\n"

acts=("opened terminal" "read a private file" "looked away from screen" "typed a password" "deleted evidence" "lied" "hesitated" "pressed a key" "breathed" "kept reading")
n=$((RANDOM%90000+10000)); prev="$(hex 16)"
while true; do
  n=$((n+1)); h="$(hex 16)"
  printf "${W}  BLOCK #%d${X}\n" "$n"
  printf "${D}    hash .. %s\n" "$h"
  printf "${D}    prev .. %s\n" "$prev"
  printf "${C}    actor . %s\n" "$U"
  printf "${D}    act ... %s${X}\n" "${acts[$((RANDOM%10))]}"
  prev="$h"
  if [ $((RANDOM%6)) -eq 0 ]; then printf "${R}    >> this block cannot be deleted. neither can you.${X}\n"; printf '\a'; sleep 0.6; fi
  sleep 0.5
done
