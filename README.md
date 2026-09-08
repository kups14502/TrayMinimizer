# Tray Minimizer

Right-click any window's minimize button and that window goes to the system tray instead of the
taskbar. Left-click its tray icon to bring it back.

[![build](https://github.com/kups14502/TrayMinimizer/actions/workflows/build.yml/badge.svg)](https://github.com/kups14502/TrayMinimizer/actions/workflows/build.yml)
[![license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

One executable. No installer, no DLL injection, no runtime to download, no telemetry.

## Why

Windows has no way to park a window in the tray. The tools that add one, RBTray among them, ask
the window where its minimize button is, and apps that paint their own title bar (Windows Terminal,
WinUI apps, some Office windows) answer that they have none, so the right-click falls through.
Tray Minimizer handles those by working out where the button must be from the window's frame and
its DPI, so the modern frames work as well as the classic ones.

## Install

1. Download `TrayMinimizer.zip` from [Releases](https://github.com/kups14502/TrayMinimizer/releases).
2. Right-click the zip, choose **Properties**, tick **Unblock**, then extract it anywhere.
3. Run `TrayMinimizer.exe`. Its icon (a blue down arrow) lands under the **`^`** arrow next to the clock.
4. Right-click that icon and tick **Start with Windows** if you want it at every logon.

It is portable. Outside its own folder it writes one setting under `HKCU\Software\TrayMinimizer`
and, only if you tick it, the startup entry. It needs .NET Framework 4, which every supported
version of Windows already has, and it is built for 64-bit Windows.

## Usage

- Right-click a window's minimize button: the window is hidden and a tray icon appears with the
  window's own icon and title.
- Left-click that tray icon: the window comes back (maximized state is preserved) and gets focus.
- Right-click that tray icon: Restore, or Close window.
- The Tray Minimizer icon itself has: Restore all windows, Custom title bar mode, Start with
  Windows, How it works, Exit.

Exiting restores every window it is holding, so nothing can get stranded. It also restores on
logoff, shutdown, and unhandled errors.

## Startup

**Start with Windows** on the tray menu writes `HKCU:\Software\Microsoft\Windows\CurrentVersion\Run`
under the value `TrayMinimizer`. Untick it there, or remove it by hand with:

```powershell
Remove-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'TrayMinimizer'
```

## How it works

A `WH_MOUSE_LL` low-level mouse hook watches for right-button down/up. A low-level hook runs in
this process, so nothing is loaded into anyone else's.

On button-down it resolves the top-level window under the cursor and sends it `WM_NCHITTEST`
(100 ms `SMTO_ABORTIFHUNG` timeout, so a hung app can never stall the mouse), then decides by two
paths:

1. **Standard frames** answer `HTMINBUTTON` and are handled directly.
2. **Custom title bars** (see below) answer plain `HTCAPTION` across the entire strip, buttons
   included. For those, the button is located geometrically: third slot in from the right edge of
   the DWM extended frame bounds, using standard Windows 11 caption metrics (46x32 DIP) scaled by
   that window's own DPI, and only if the window has `WS_MINIMIZEBOX`.
   `DWMWA_EXTENDED_FRAME_BOUNDS` is used rather than `GetWindowRect` because the latter includes
   the invisible resize border and is wrong when maximized.

Button-up confirms by proximity (same window, cursor within 12 px of the press) rather than
hit-testing a second time. An earlier version did re-test, and a timed-out round trip to a busy
app silently ate the click. Both clicks are swallowed so no system menu appears.

The window is hidden immediately; the tray icon is built afterward, since fetching a packaged
app's icon can take a few hundred ms and the click should feel instant. Real work is deferred off
the hook callback with `BeginInvoke`.

A 2-second sweep drops tray icons for windows that were closed or reappeared on their own. The
hook is reinstalled every 10 minutes, since Windows can silently drop a low-level hook whose
callback ever ran long.

## Custom title bar mode

On by default, toggle in the tray menu, stored at `HKCU:\Software\TrayMinimizer\CustomTitleBarFallback`.

Needed for **Windows Terminal**, WinUI apps, and some Office windows. Verified on Windows
Terminal: it reports `HTCAPTION` (2) over minimize, maximize, and close alike, and exposes no
caption buttons through UI Automation at all (only its tab close button and scrollbar), so
position is the only way to find the button.

Turn it off if a right-click near the top-right corner of a window ever does something you did
not expect.

## Known limits

- Windows owned by elevated (admin) processes are skipped. UIPI blocks the hit-test message from a
  non-elevated process, so the right-click behaves normally there. Run Tray Minimizer elevated if
  you need it to cover admin windows.
- Apps whose custom caption buttons are not standard-sized and not exposed to `WM_NCHITTEST` are
  not detected. Discord is one example: it draws narrower buttons than the Windows metric, so the
  geometric slot does not line up.
- Chrome, Edge, Explorer, Notepad, Office, and standard Win32/UWP frames all use path 1 and work
  directly.

## Build from source

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

`build.ps1` compiles with the C# compiler that ships inside `C:\Windows\Microsoft.NET`, so no SDK
or Visual Studio is needed. If an instance from this folder is running it is asked to quit first
(every parked window is restored), then relaunched.

## License

MIT, see [LICENSE](LICENSE).
