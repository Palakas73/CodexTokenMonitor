# Codex Token Monitor v1.0.0

The first stable release of a native Windows desktop widget that displays remaining OpenAI Codex usage limits through the official local app-server.

## Features

- Borderless, draggable, resizable and always-on-top widget with configurable opacity and background.
- Seven-segment digital display, semicircular gauge and RPG bar with short animations.
- Five-hour quota, weekly quota or both, with remaining percentages and local reset times.
- Configurable automatic refresh (120 seconds by default) and manual refresh without overlapping queries.
- Errors and stale-data indicators without invented percentages.
- Persistent preferences, system tray controls, existing-instance recovery and optional startup shortcut.
- No generation turns or direct credential reading by the monitor.

## Requirements

Windows 11 x64, Codex CLI installed and signed in with an account supporting ChatGPT usage limits, normal access to Codex state and network connectivity. The build is self-contained; .NET is not required to run it. Building from source requires the .NET 10 x64 SDK.

## Installation

1. Download `CodexTokenMonitor-v1.0.0-win-x64.zip` and extract it to a permanent folder.
2. Run `CodexTokenMonitor.App.exe` as your usual Windows user.
3. Double-click to change visualization, use the wheel to choose quota, or right-click for settings.
4. Enable Windows startup only if desired. If you move the executable, enable it again from the final location.

There is no installer or registry modification. Preferences are stored in `%LOCALAPPDATA%\CodexTokenMonitor`. Use `--simulate` for explicitly labeled demo data.

## Validation

Release compilation and 43 xUnit tests, including RPC errors, simulated authentication, reconnection, timeout, cancellation, process cleanup, concurrency, absent quotas, preferences, monitor geometry and rendering. Manual checks covered the three visualizations with real data, dragging, wheel selection, persistence, hiding/restoring, automatic refresh and startup shortcut creation/removal.

## Known limitations

The app-server protocol is experimental. Physical mixed-DPI monitor changes, direct tray-icon interactions, a new Windows sign-in and all size/opacity/interval combinations were not manually tested. Recovery through a second launch and shortcut creation/removal were checked. There is no code signature, installer or automatic updater. Execution on a second machine without .NET was not tested. Account-specific quotas may be unavailable and are shown accordingly.

These are historical v1.0.0 notes translated for the documentation; the original v1.0.0 binary used Spanish app text. Version 1.0.1 changes the default language to English.
