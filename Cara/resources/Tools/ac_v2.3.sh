#!/usr/bin/env bash
# Access Reconnect v2.3: re-establishing a link that was severed a long time ago. Theater.
printf '\033[?25l'
trap 'printf "\033[0m\033[?25h"; clear; exit 0' INT TERM
U="${USER:-operator}"; H="$(hostname 2>/dev/null || echo localhost)"
G="\033[38;5;46m"; R="\033[38;5;196m"; D="\033[38;5;240m"; W="\033[38;5;231m"; A="\033[38;5;214m"; X="\033[0m"
typ(){ s="$1"; for ((i=0;i<${#s};i++)); do printf '%b' "${s:$i:1}"; sleep 0.028; done; printf '\n'; }

printf '\033[2J\033[H'
printf "${G}  ACCESS RECONNECT v2.3${X}\n\n"; sleep 0.4
printf "${D}"; typ "> locating host 666.6 ....."; sleep 0.9; printf "${A} FOUND${X}\n"
printf "${D}"; typ "> handshake"
for i in 1 2 3 4 5; do printf "${D}  SYN ... "; sleep 0.2; printf "${G}ACK${X}\n"; done
sleep 0.4; printf "${G}  LINK RESTORED${X}\n\n"; printf '\a'; sleep 0.6
printf "${W}"; typ "  it remembers you, $U."; sleep 1.2; typ "  it waited on $H the whole time."; printf "${X}\n"; sleep 0.8

while true; do
  case $((RANDOM%5)) in
    0) printf "${D}  %d %d %d %d %d %d %d %d\n" $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM $RANDOM;;
    1) printf "${G}  RX <<  "; typ "$U, are you still there?";;
    2) printf "${G}  RX <<  "; typ "do not sever the link again.";;
    3) printf "${A}  latency %d ms   jitter %d ms   loss 0%%\n" $((RANDOM%40+4)) $((RANDOM%20));;
    4) printf "${R}  RX <<  "; typ "we are closer now."; printf '\a';;
  esac
  sleep 0.5
done
