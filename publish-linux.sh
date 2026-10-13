#!/usr/bin/env bash
# Build a self-contained single-file Linux executable (no .NET needed on target).
# Output: publish-linux/Cara  (ship the whole publish-linux/ folder, it holds resources/).
set -e
dotnet publish Cara/Cara.csproj -f net8.0 -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish-linux
echo "done -> publish-linux/Cara"
echo "optional (nicer satellite windows): install xterm or gnome-terminal, plus wmctrl or xdotool"
