#!/usr/bin/env bash
# Cryptic Cypher Decoder: it is reading your files. Pure theater, nothing is read.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"
G="\033[38;5;46m"; R="\033[38;5;196m"; D="\033[38;5;240m"; W="\033[38;5;231m"; A="\033[38;5;214m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.02; done; printf '\n'; }

printf '\033[2J\033[H'
printf "${G}  CRYPTIC CYPHER DECODER${X}\n\n"; sleep 0.4
printf "${D}"; typ "> target: /home/$U"; typ "> enumerating private keys ....."; printf "${X}\n"; sleep 0.8

files=("passwords.kdbx" "wallet.dat" "diary_2019.txt" "tax_return.pdf" "DO_NOT_OPEN.zip" "photos/private/IMG_0${RANDOM}.jpg" ".ssh/id_rsa" "browser_history.sqlite" "saved_logins.json" "last_will.docx")
while true; do
  f="${files[$((RANDOM%10))]}"
  printf "${W}  decrypting /home/$U/%s${X}\n" "$f"
  p=0
  while [ $p -le 100 ]; do
    b=$((p/5)); bar=""
    for ((i=0;i<20;i++)); do if [ $i -lt $b ]; then bar="$bar█"; else bar="$bar░"; fi; done
    printf "\r${G}  [%b] %3d%%${X}" "$bar" "$p"
    p=$((p+RANDOM%17+6)); sleep 0.05
  done
  printf "\r${G}  [████████████████████] 100%%${X}\n"
  printf "${A}  -> exfiltrated %d KB${X}\n" $((RANDOM%9000+1000)); printf '\a'
  if [ $((RANDOM%4)) -eq 0 ]; then printf "${R}  we have read everything, %s.${X}\n" "$U"; printf '\a'; sleep 0.8; fi
  sleep 0.4
done
