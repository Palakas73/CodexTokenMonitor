# Codex Token Monitor v1.0.3

Gold reset availability coins.

## Changes

- Added the transparent `MONEDA.png` coin artwork to the widget status line.
- Displays one coin for each quota window with a valid reset time.
- Added the English tooltip `X resets available`.
- Updated the README and architecture documentation in English.

## Requirements and installation

Windows 11 x64 and an installed, authenticated Codex CLI with a compatible account. The build is self-contained; no separately installed .NET runtime or API key is needed.

Download `CodexTokenMonitor-v1.0.3-win-x64.zip`, extract it to a permanent folder and run `CodexTokenMonitor.App.exe`. Preferences remain in `%LOCALAPPDATA%\CodexTokenMonitor`.

## Validation

45 xUnit tests passed with no failures or skips. Release packaging is produced by `scripts/publish.ps1`.

## Known limitations

Windows controls whether a notification icon appears directly beside the clock or under the hidden-icons arrow. The application does not change Windows taskbar preferences.
