# Codex Token Monitor v1.0.2

A custom application icon and system-tray-only taskbar behavior.

## Changes

- Added a recognizable teal quota gauge icon, shared by the executable, application window and system tray.
- Embedded a multi-resolution ICO with 16, 20, 24, 32, 40, 48, 64, 128 and 256px frames for crisp small icons and high-DPI displays.
- Removed the widget's taskbar button. The widget remains visible on the desktop and is managed through its system-tray icon.
- Kept **Show**, **Hide**, **Refresh** and **Exit** in the tray menu and double-click to restore the widget.
- Added editable SVG artwork, a reproducible icon-generation script using standard Windows/.NET APIs, and a packaged-resource decoding test.
- Existing preferences and English-default UI are preserved.

## Requirements and installation

Windows 11 x64 and an installed, authenticated Codex CLI with a compatible account. The build is self-contained; no separately installed .NET runtime or API key is needed.

Exit the running version from the tray menu, download `CodexTokenMonitor-v1.0.2-win-x64.zip`, extract it to a permanent folder and run `CodexTokenMonitor.App.exe`. If you change the executable location, disable and enable **Start with Windows** from the new location. Preferences remain in `%LOCALAPPDATA%\CodexTokenMonitor`.

## Validation

44 xUnit tests, including WPF decoding of all nine embedded ICO frames and native decoding of notification-area/high-DPI sizes. The user manually verified the icon and tray behavior. The automated desktop helper did not expose the widget after its taskbar button was removed, so direct tray interaction is not claimed as an automated visual check.

## Known limitations

- Windows controls whether a notification icon appears directly beside the clock or under the hidden-icons arrow. Pin or move it using the taskbar's own controls if needed; this application does not change Windows preferences.
- Existing limitations remain: experimental Codex app-server protocol, no physical mixed-DPI monitor validation or fresh Windows sign-in test, no installer/code signature/updater, and no second-machine validation.
