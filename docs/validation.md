# Validation

## Scope

This report summarizes stable-version validation without real quota values, account identity, profile paths or development transcripts. The detailed development history is kept only as an ignored local artifact.

## v1.0.0 results

- Release builds for phases 2-5 completed with no warnings or errors, with the initial 13 tests passing after each stage.
- The integration and preferences suite increased coverage to 33 tests.
- The final suite contained **43 passing tests**, including control rendering and the gauge-gradient regression.
- The self-contained Windows x64 single-file distribution was generated successfully without trimming.
- Before publication, a new Release build completed with **0 warnings and 0 errors**, and `scripts/publish.ps1` passed **43 tests with 0 failures and 0 skips**.
- File metadata was FileVersion `1.0.0.0`, ProductVersion `1.0.0`.
- ZIP integrity and executable equality were verified. The archive contained the executable, README and release notes.
- The initial 42 source/documentation files were reviewed: no private keys, tokens, personal email addresses, profile paths or sensitive assignments were detected. Build output, caches, logs, preferences and account data were excluded. Git used a GitHub noreply identity.

## Automated coverage

Consumed-to-remaining conversion and out-of-range inputs; window identification by duration and bucket; missing quotas; incomplete or malformed JSON-RPC; RPC and simulated authentication errors; disconnect/reconnect; timeout, cancellation and owned-process cleanup; concurrent updates; stale data; preference persistence, atomic replacement, corrupt files and normalization; negative, removed and small monitor geometry with simulated scale factors; STA rendering at color boundaries and unavailable data.

## Manual checks actually performed for v1.0.0

- Live Codex readings and consumed/remaining conversion were checked without retaining account values in the repository.
- Digital, gauge and RPG views were inspected with real data, including in the self-contained build.
- Double-click switching, wheel selection in both directions and dragging were checked.
- Visualization and geometry were restored after reopening.
- Hiding from the context menu and restoring by launching again reused the existing window.
- Automatic refresh after 120 seconds was observed without manual input.
- Startup shortcut creation and removal were checked from the menu, including target and hidden-start argument. Startup was left disabled.
- Normal close before rebuilding and launch from a folder containing only the EXE were checked.

## Issues corrected during validation

ScrollViewer intercepted wheel input; PreviewMouseWheel fixed quota selection and was checked in both directions. The gauge gradient was anchored to the full arc to prevent green in a low-quota segment, with a regression test. Corrupt preference handling was improved. A publication blocked by a running executable was retried after closing the app normally.

## Not executed and known limits

- Physical monitor changes, real DPI changes and physical display disconnection.
- Direct automation of the notification-area tray icon.
- A new Windows sign-in.
- Every manual combination of size, opacity and refresh interval.
- Remote-service failures or actual sign-out: error cases used FakeServer.
- Execution on a second machine without .NET installed.

These checks are not claimed to have passed. No installer, code signature or automatic updater is included.

## v1.0.1 English-default update

The interface, app-owned errors, scripts and documentation have been translated into English. UI culture defaults to en-US while regional date and number formatting is preserved. No preference migration is required.

- `scripts/publish.ps1` completed with exit code 0: Release compilation succeeded, all **43 tests passed**, with **0 failed and 0 skipped**. The v1.0.1 self-contained EXE and ZIP were generated.
- The published executable was opened and inspected through Computer Use. Its header, live-data state, five-hour label, reset label, last-update label and Refresh button were English.
- The context menu was inspected: Visualization, Quota, Opacity, Transparent background, Size, Refresh interval, Start with Windows, Refresh now, Show, Hide and Exit were English.
- Existing visualization, size, opacity and background preferences were retained. Date formatting remained regional.
- A source and documentation scan found no remaining Spanish UI strings. This check does not claim manual interaction with every menu option or the tray icon.
- The known hardware and Windows sign-in limitations above still apply.

## v1.0.2 custom icon and tray-only taskbar behavior

- Added a custom teal gauge icon, embedded in the executable and loaded for the notification area from a WPF resource.
- Disabled the widget's taskbar button while preserving tray controls and restore behavior.
- Added a multi-resolution ICO (16-256px), editable SVG and a deterministic Windows/.NET icon-generation script.
- The existing 43 tests remained green. A new packaged-resource test verifies all nine frames through WPF and the 16-128px sizes through the native icon decoder: **44 tests passed** before packaging.
- The initial test used an absolute pack URI before WPF registered the URI scheme; a component-relative resource URI resolves correctly. The test also distinguishes the 256px Explorer frame from the native small-icon best-fit selection.
- The user explicitly confirmed manual verification of the icon and tray result. This is user-verified, not a claim that the automation inspected the tray directly.
- The automated desktop helper no longer enumerated the widget after ShowInTaskbar was disabled. A diagnostic process check confirmed the launched test process was running; that process alone was then closed for rebuilding.
- Windows notification-area overflow remains under user control; no taskbar registry or system preferences were changed.

Final v1.0.2 packaging: `scripts/publish.ps1` completed with exit code 0, all **44 tests passed** with no failures or skips, and the self-contained EXE and ZIP were generated. The executable's own icon was extracted for artifact inspection, separately from the loose source artwork.

## Reset availability coins

The status line now renders the bundled transparent `MONEDA.png` once for each quota window that includes a valid reset time. Hovering over the coin group displays an English tooltip such as `2 resets available`. The implementation was compiled in Release and the full **45-test** suite passed with no failures or skips after this change.
