# Codex Token Monitor v1.0.1

English is now the default language throughout the application and documentation.

## Changes

- Translated widget labels, visualization/quota menus, tray actions, startup settings, refresh states, reset labels and error messages into English.
- Defaulted application UI resources to en-US while preserving regional formats for dates and numbers.
- Added English summaries for native errors so localized system messages do not appear in app error states.
- Translated the README, architecture, validation report, release notes and script messages.
- Existing preferences and startup settings continue to work without migration.

## Requirements and installation

Windows 11 x64 and an installed, authenticated Codex CLI with a compatible ChatGPT account. No API key or separately installed .NET runtime is required.

Download `CodexTokenMonitor-v1.0.1-win-x64.zip`, exit any running copy, extract to a permanent folder and run `CodexTokenMonitor.App.exe`. If you move the executable, disable and enable **Start with Windows** from the new location. Preferences remain in `%LOCALAPPDATA%\CodexTokenMonitor`. Use `--simulate` for labeled demo data.

## Validation

The release is checked with a Release build, all 43 xUnit tests and the reproducible publish script. The validation report distinguishes actual UI checks from unexecuted checks.

## Known limitations

- The official Codex app-server protocol is experimental.
- Physical mixed-DPI monitor changes, direct tray-icon interaction and a fresh Windows sign-in have not been manually validated.
- Not every size, opacity and interval combination has been manually checked.
- No installer, code signature or automatic updater is included; a second machine without .NET has not been tested.
- Native Windows shell UI, such as OS-provided dialog buttons, follows Windows language settings. Application-owned text defaults to English.
- Dates and numbers intentionally follow your regional settings.
