# optimizOR

optimizOR is a Windows Forms utility for Windows 10 and Windows 11 that applies selected gaming latency tweaks and optional debloat actions. It is designed as a lightweight admin tool with Thai/English UI, light/dark themes, per-tweak info dialogs, and a clear confirmation step before system changes.

## Features

- Gaming latency and scheduling tweaks through Registry values
- Legacy INI tweaks for `SYSTEM.INI` and `WIN.INI`
- Conservative Windows 10/11 debloat options
- Thai and English language selector
- Light-first UI with optional dark theme
- Per-tweak info buttons
- Thread-safe operation log
- Administrator guard through app manifest and runtime check

## Important Safety Notes

This tool changes real Windows settings. Some actions may require a restart.

Debloat actions can remove Appx packages or disable Windows features. Removed apps may not be restored automatically by `Restore defaults`; some apps must be reinstalled from Microsoft Store or PowerShell.

Before using debloat or registry tweaks, create a Windows restore point and back up important data. Use this tool only if you understand and accept responsibility for the changes made to your PC.

## Requirements

- Windows 10 or Windows 11
- Administrator privileges
- .NET Framework 4.8 runtime to run
- .NET Framework 4.8 Developer Pack or Visual Studio Build Tools to build

Recommended fonts:

- English: Roboto
- Thai: Anuphan

If these fonts are not installed, Windows will use a fallback font.

Download fonts from the official Google Fonts pages:

- Anuphan: https://fonts.google.com/specimen/Anuphan
- Roboto: https://fonts.google.com/specimen/Roboto

To bundle fonts with optimizOR, place the `.ttf` files in:

```text
optimizOR\Assets\Fonts
```

The app checks bundled fonts first. If no bundled or installed font is found, it falls back to `Leelawadee UI` for Thai and `Segoe UI` for English.

## Build

Open `optimizOR.sln` in Visual Studio 2022, or build from PowerShell:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe optimizOR.sln /p:Configuration=Release /p:Platform="Any CPU"
```

Output:

```text
optimizOR\bin\Release\optimizOR.exe
```

For local testing with Debug:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe optimizOR.sln /p:Configuration=Debug /p:Platform="Any CPU"
```

Output:

```text
optimizOR\bin\Debug\optimizOR.exe
```

## Run

Run the executable as Administrator:

1. Right-click `optimizOR.exe`
2. Choose `Run as administrator`
3. Review enabled toggles
4. Use each `i` button to read what a tweak does
5. Click `Apply selected`

## Debloat Scope

The debloat module is intentionally conservative. It does not remove critical Windows components such as Microsoft Store, Windows Security, Edge, Terminal, Photos, Calculator, codecs, or framework packages.

Available debloat actions:

- Remove bundled consumer apps
- Remove Xbox companion apps
- Remove consumer Teams Chat
- Disable Widgets and News
- Disable ads and suggestions
- Disable OneDrive startup without uninstalling OneDrive

## Project Layout

```text
optimizOR.sln
optimizOR/
  optimizOR.csproj
  Program.cs
  Properties/
  Core/
  Models/
  UI/
```

## Disclaimer

optimizOR is provided as-is. You are responsible for reviewing selected tweaks and debloat actions before applying them. The author and contributors are not responsible for data loss, missing apps, broken workflows, reduced security, or system instability caused by applying changes.
