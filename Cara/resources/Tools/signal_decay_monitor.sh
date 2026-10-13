#!/usr/bin/env bash
# Dead Signal Monitor: a transmission that will not stop decaying. Pure theater.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"
R="\033[38;5;196m"; G="\033[38;5;46m"; D="\033[38;5;240m"; W="\033[38;5;231m"; A="\033[38;5;214m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.025; done; printf '\n'; }

printf '\033[2J\033[H'
printf "${G}  DEAD SIGNAL MONITOR  [carrier 666.6 MHz]${X}\n\n"; sleep 0.5
last=$((RANDOM%48+2))
printf "${D}"; typ "> last confirmed contact: ${last}h ago"
typ "> the station went dark. the signal did not."; printf "${X}\n"; sleep 0.8

snr=92
whisper=("...is anyone there..." "...$U...can you hear us..." "...do not answer..." "...it followed the signal home..." "...we never left...")
while true; do
  snr=$((snr-RANDOM%4))
  if [ $snr -lt 6 ]; then snr=$((RANDOM%30+70)); printf "${R}  !! CARRIER LOST - REACQUIRING !!${X}\n"; printf '\a'; sleep 0.5; fi
  bar=""; f=$((snr/5))
  for ((i=0;i<20;i++)); do if [ $i -lt $f ]; then bar="$bar█"; else bar="$bar░"; fi; done
  col="$G"; [ $snr -lt 50 ] && col="$A"; [ $snr -lt 25 ] && col="$R"
  printf "${col}  SNR [%b] %3d%%${X}\n" "$bar" "$snr"
  if [ $((RANDOM%5)) -eq 0 ]; then
    printf "${W}  EVP> %s${X}\n" "${whisper[$((RANDOM%5))]}"; printf '\a'
  else
    line=""; for ((i=0;i<46;i++)); do if [ $((RANDOM%100)) -lt $snr ]; then line="$line▒"; else line="$line "; fi; done
    printf "${D}  %b${X}\n" "$line"
  fi
  sleep 0.22
done
