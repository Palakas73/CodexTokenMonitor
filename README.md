# Codex Token Monitor v1.0.1

A lightweight native Windows 11 widget built with C#, .NET 10, WPF and MVVM. It reads real usage limits from the official local Codex app-server without generating model turns or reading credentials directly.

## Download and run

Download `CodexTokenMonitor-v1.0.1-win-x64.zip` from [Release v1.0.1](https://github.com/Palakas73/CodexTokenMonitor/releases/tag/v1.0.1), extract it to a permanent folder, and run `CodexTokenMonitor.App.exe`. This repository and its downloads are private and require authorized GitHub access.

The Windows x64 build is self-contained: you do not need to install .NET to run it. Codex CLI must be installed and signed in with an account that supports ChatGPT usage limits. No API key is required. Building from source creates the executable under `artifacts\win-x64`.

```powershell
.\CodexTokenMonitor.App.exe
.\CodexTokenMonitor.App.exe --simulate
.\CodexTokenMonitor.App.exe --codex "C:\path\codex.exe" --timeout-seconds 45
```

`--simulate` explicitly labels demo data and never activates as an automatic fallback. `--start-hidden` starts in the system tray. Only one instance runs per Windows session: launching the app again restores the existing widget. Exit the current instance before changing command-line arguments.

## Controls

- Drag with the left mouse button. Double-click to cycle through the seven-segment digital display, semicircular gauge and RPG bar.
- Scroll up for the five-hour quota or down for the weekly quota. Select **Quota > Both** in the context menu to display both.
- Right-click for visualization, quota, opacity, transparent background, size, refresh interval, startup, refresh, show, hide and exit options.
- System tray: double-click to restore; right-click for **Show**, **Hide**, **Refresh** and **Exit**. If Windows hides the icon, check the tray overflow. Launching the app again also restores a hidden widget.
- The widget stays on top and does not activate when refreshing data. Drag the bottom-right grip to resize it.
- Automatic refresh defaults to 120 seconds. The menu offers 15, 60, 120, 300 and 600 seconds. Manual refresh is available from the button and menus. Queries never run concurrently.
- Values always show **remaining** quota: green at 70% or higher, yellow at 30%, orange at 10% and red below 10%. Calculation: `clamp(100 - usedPercent, 0, 100)`.
- Missing quotas display `--`. Five hours (300 minutes) and weekly (10080 minutes) are identified by duration, never by response position. Account buckets remain separate.
- Failed reads preserve the last valid values and display **STALE**. Last successful update and reset times use local time. An unavailable quota does not mean 0%.

The default interface language is English, including menus, status and app-owned error messages. Dates and numbers retain the user's regional formats; there is no language selector.

## Preferences

Settings are stored in `%LOCALAPPDATA%\CodexTokenMonitor\preferences.json` through a temporary file and atomic replacement. They include position, size, visualization, quota, opacity, background, interval, timeout and startup preference. Off-screen positions are fitted to available monitor working areas. A PerMonitorV2 manifest enables per-monitor DPI awareness.

Automatic startup is initially disabled. Enabling it creates only `CodexTokenMonitor.lnk` in the current user's Startup folder; disabling it removes that app-owned shortcut. The registry is not modified. Enable startup from the executable's permanent location. If you move it, disable and enable startup from the new location.

## Build, test and publish

Use the .NET 10 x64 SDK; Visual Studio 2026 can open `CodexTokenMonitor.slnx`. The UI has no external dependencies. xUnit and its adapter are used only for tests.

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' build CodexTokenMonitor.slnx -c Release
& 'C:\Program Files\dotnet\dotnet.exe' test CodexTokenMonitor.slnx -c Release
powershell -NoProfile -File scripts/publish.ps1
```

Close running copies from `bin` and the published directory before building or publishing to avoid locked files. The script uses the x64 SDK, stores caches and CLI home under `artifacts`, runs tests, and publishes a self-contained single-file `win-x64` executable without trimming. It also generates the versioned ZIP with the executable, README and release notes. Test results are saved to `artifacts/TestResults/publish.trx`.

The .NET single-file runtime may extract native libraries into its temporary cache on startup. This is normal runtime behavior, not an installer.

Optional independent protocol check (requires an existing Python installation): `python scripts/probe-codex.py`. It only reads quotas and never prints account identity or credentials. Codex needs access to its normal state directory; an agent sandbox may block that access.

## Validation and limitations

See [validation](docs/validation.md) for actual results and unexecuted checks, and [architecture](docs/architecture.md) for implementation details. Geometry tests simulate monitors and scale factors; they do not replace moving a window between physical displays with different DPI settings. A new Windows sign-in has not been tested. No installer, code signature or automatic updater is included.

## Official sources

Protocol checked on October 17, 2026:

- https://developers.openai.com/codex/app-server
- https://github.com/openai/codex/tree/main/codex-rs/app-server
- https://github.com/openai/codex/blob/main/codex-rs/app-server-protocol/schema/typescript/v2/RateLimitWindow.ts
- https://github.com/openai/codex/blob/main/codex-rs/app-server-protocol/schema/typescript/v2/GetAccountRateLimitsResponse.ts

JSONL over stdio, without the `jsonrpc` field: `initialize` > `initialized` > `account/rateLimits/read`. The client prefers `rateLimitsByLimitId` and supports the legacy `rateLimits` field. `resetsAt` uses Unix seconds. The protocol is experimental; errors and incompatible changes are reported without fabricating percentages.
