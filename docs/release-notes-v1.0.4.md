# Codex Token Monitor v1.0.4

Correct reset availability counting.

## Changes

- Reset coins now count only recognized five-hour and weekly quota windows.
- Expired reset timestamps and unknown quota windows are excluded.
- The last valid count is retained when a refresh fails, together with the existing stale-data indication.

## Requirements and installation

Windows 11 x64 and an installed, authenticated Codex CLI with a compatible account. The build is self-contained; no separately installed .NET runtime or API key is needed.

Download `CodexTokenMonitor-v1.0.4-win-x64.zip`, extract it to a permanent folder and run `CodexTokenMonitor.App.exe`.

## Validation

45 xUnit tests passed with no failures or skips. The test suite covers unknown quota windows and expired reset timestamps.
