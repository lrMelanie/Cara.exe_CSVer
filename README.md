## ⚠️ Important Notes
Academic Project - Not production-ready code

Requires Caution:
Contains experimental system operations
Some features modify registry/network settings
Always run in a controlled environment

Future Potential:
Machine learning integration
GUI interface development
More minigames and effects

---

# Cara.exe - Experimental Virtual Assistant 🖥️✨

**A C# (.NET) port of the academic prototype - a Windows console virtual
assistant with extended system integration and a "technological horror" theme**
*Originally a university project - now rebuilt on .NET*

---

## 🚀 Features

- **Interactive CLI** with typewriter-style output (`PrintSlowly` effect)
- **System integration** (Airplane mode control, Bluetooth management, Admin operations, registry)
- **Productivity tools**:
  - Smart reminders with Scroll Lock trigger
  - Schedule management (`schedule add/list/remove`)
  - Motivational / sarcastic lines (`vol1.txt`, `vol2.txt`)
  - Network recovery (`fix me`) - restores Wi-Fi/Ethernet/Bluetooth
- **Minigames** ⌨️ (via `minigame` -> HACKER TERMINAL):
  - **Code Runner** - real-time transcription (characters turn red the moment
    you mistype), a shrinking time trace, difficulty levels, highscore
  - **Dice Arena** - dice vs HP combat against escalating processes
  - **Reactor Core** - SIMPLE (classic) and EXTENDED (pressure, coolant,
    random events, meltdown/scram)
- **Experimental modules**:
  - Phantom protocol (HTTP request generator)
  - "Black Mirror" visual effects sequence
  - Spectral broadcast / audio manipulation (MP3 + Beep)
  - Hidden "haunt" effects: secret codes and rare jumpscares

---

## ⚙️ Installation

1. **Requirements**:
   - Windows 10/11
   - .NET 8 SDK, or Visual Studio 2022 with the ".NET desktop development" workload
   - Admin privileges (for full functionality - the app auto-elevates via its manifest)

2. **Build and run**:
```text
git clone https://github.com/lrMelanie/Cara.exe_CSVer
Open Cara.sln in Visual Studio 2022 and press F5
   - or from a terminal:  dotnet run --project Cara
```

3. **Standalone build** (one .exe, no .NET needed on the target PC):
```text
double-click publish.bat
the self-contained Cara.exe appears in the publish\ folder
(ship the whole publish\ folder - it also contains resources\)
```

probably should work

---

## 🕹️ Basic Usage
```text
# Core functionality
> schedule add 2025-12-31 23:59 New Year Countdown
> motto
> say
> help
> fix me          # recovery: re-enables network if an experimental command cut it off

# Minigame activation
> minigame

# Experimental commands
> ???
> ???
> ???
> ???
> ???
```

---

Disclaimer: Contains intentional Easter eggs and prototype-grade code. Not
affiliated with any commercial entities. Use at own risk, or not :)
