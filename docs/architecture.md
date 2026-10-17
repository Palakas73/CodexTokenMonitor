# Architecture

## Components

- **Core**: `IRateLimitProvider`, snapshots, quota windows identified by duration and bucket, remaining-percentage calculation, stale state, normalized preferences and pure window-placement calculations.
- **Infrastructure**: CLI discovery, a private process per query, JSONL with request IDs, sanitized RPC errors, strict parsing, simulated provider and atomic JSON storage.
- **App**: WPF/MVVM. The ViewModel manages data and selection. Three independent controls render through DrawingContext. The window composes desktop services for tray, monitors, menus and lifecycle without an external MVVM or DI framework.
- **Tests**: xUnit, STA rendering and a fake app-server running in a real local process. No credentials or production endpoints are needed.

## Reading and recovery

Each query starts its own hidden process, initializes the protocol and reads usage limits. Unsolicited notifications are ignored; unsupported server requests receive -32601. stderr is drained and discarded. No transcripts or authentication data are stored. Responses are correlated by ID and quota fields are validated.

Timeout defaults to 30 seconds and is configurable. Provider and UI semaphores prevent overlapping queries. Closing cancels pending work. Cleanup closes stdin, waits two seconds and terminates only the owned process tree if necessary. Diagnostic draining is also bounded. After a failure, the next query starts a fresh process. The UI keeps the last valid data, marks it stale and clears the error after a successful reading. Age checks also mark expired data stale.

Authentication classification uses recognized server-message terms. The RPC code is retained, but remote text is never displayed or logged. Unrecognized RPC errors use a generic message.

## UI and resources

The Topmost borderless window supports configurable opacity and background transparency. Physical HWND coordinates are fitted to monitor working areas using WM_DISPLAYCHANGE, WM_DPICHANGED and PerMonitorV2. Refresh never calls Activate. Dragging ends with a monitor-fit check.

Digital, gauge and RPG controls receive remaining quota or NaN for missing data. Changes animate for 350 ms without a continuous rendering loop and respect ClientAreaAnimation. Rows and tooltips expose percentage values to accessibility. The RPG bar is left-anchored so consumption empties it from the right.

The application explicitly defaults CurrentUICulture and DefaultThreadCurrentUICulture to en-US. CurrentCulture is preserved for regional number and date formats. App-owned strings and scripts are English. System-dependent failures shown to users use English summaries rather than localized native exception text.

Geometry saves are debounced for 400 ms and flushed on close. A session-local mutex prevents duplicate instances. A named event restores the existing instance, checked by a lightweight UI timer.

The tray icon remains active while the window is hidden and is disposed on exit. Closing waits for cancelled queries to finish. Startup uses only an app-owned shortcut in the user's Startup folder, modified when the option is explicitly toggled; the registry is untouched.

## Distribution

The Windows x64 build is self-contained and single-file, without trimming to preserve XAML and COM compatibility. The publish script runs tests, builds the executable and packages it with the README and versioned release notes. It does not create installers or change security settings.
