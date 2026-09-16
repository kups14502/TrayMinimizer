# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-09-16

### Fixed

- Right-clicks were dropped on a window that was busy drawing, so the button often took several
  tries in a terminal printing a lot of output. The hit test that asks the window what is under
  the cursor cannot be answered while that window is blocked, and a timeout threw the click away.
  Geometry is now worked out first and stands on its own when the hit test does not answer, and
  the wait is cut to 30 ms once geometry has identified the slot, which also keeps the mouse hook
  from running long.

## [1.0.0] - 2026-09-08

First release.

### Added

- Right-click a window's minimize button to hide it to the system tray. Left-click the tray
  icon to bring it back with its maximized state intact.
- Per-window tray icons carry the window's own icon and title, with Restore and Close on
  their menu.
- Custom title bar mode, on by default, finds the minimize button geometrically for apps that
  paint their own caption: Windows Terminal, WinUI apps, and some Office windows.
- Restore all windows, Start with Windows, and How it works on the app's own tray menu.
- Every parked window is restored on exit, logoff, shutdown, and unhandled errors, so nothing
  is left hidden.
- The low-level mouse hook is reinstalled every 10 minutes, since Windows can silently drop a
  hook whose callback ever ran long.
