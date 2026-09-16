// TrayMinimizer
// Right-click any window's minimize button to send that window to the system tray.
// Left-click the tray icon to bring it back. Exiting the app restores every hidden window.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TrayMinimizer
{
    internal static class Native
    {
        public const int WH_MOUSE_LL = 14;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_GETICON = 0x007F;
        public const int WM_CLOSE = 0x0010;

        public const int HTCAPTION = 2;
        public const int HTMINBUTTON = 8; // same value as HTREDUCE

        public const int GA_ROOT = 2;

        public const int GWL_STYLE = -16;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        // Standard Win11 caption button footprint in device independent pixels.
        public const double CAPTION_BUTTON_W = 46.0;
        public const double CAPTION_BUTTON_H = 32.0;

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int GCLP_HICON = -14;
        public const int GCLP_HICONSM = -34;

        public const uint SMTO_ABORTIFHUNG = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern int GetDpiForWindow(IntPtr hWnd);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hWnd, int attr, out RECT value, int size);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT p);

        [DllImport("user32.dll")]
        public static extern IntPtr GetAncestor(IntPtr hwnd, int gaFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam,
            uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtrW")]
        public static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr h);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool QueryFullProcessImageName(IntPtr hProcess, uint flags, StringBuilder text, ref int size);
    }

    /// <summary>One window that is currently parked in the tray.</summary>
    internal sealed class HiddenWindow
    {
        public IntPtr Handle;
        public NotifyIcon Icon;
        public bool WasMaximized;
    }

    internal sealed class TrayMinimizerContext : ApplicationContext
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValue = "TrayMinimizer";
        private const string SettingsKey = @"Software\TrayMinimizer";
        private const string FallbackValue = "CustomTitleBarFallback";

        private readonly Dictionary<IntPtr, HiddenWindow> _hidden = new Dictionary<IntPtr, HiddenWindow>();
        private readonly Form _sink;             // hidden window that owns the message loop
        private readonly NotifyIcon _appIcon;
        private readonly ToolStripMenuItem _startupItem;
        private readonly ToolStripMenuItem _restoreAllItem;
        private readonly ToolStripMenuItem _fallbackItem;
        private bool _customTitleBarFallback = true;
        private readonly System.Windows.Forms.Timer _janitor;
        private readonly System.Windows.Forms.Timer _hookRefresh;
        private readonly Icon _ownIcon;

        private static readonly uint OwnProcessId = (uint)Process.GetCurrentProcess().Id;

        private readonly EventWaitHandle _quitSignal;
        private readonly RegisteredWaitHandle _quitWait;

        private IntPtr _hook = IntPtr.Zero;
        private IntPtr _armedWindow = IntPtr.Zero;    // min button that received the right-button-down
        private Native.POINT _armedPoint;
        private bool _shuttingDown;

        public TrayMinimizerContext()
        {
            _sink = new Form
            {
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                Opacity = 0
            };
            _sink.Load += (s, e) => _sink.Hide();
            _sink.Show();   // forces handle creation so BeginInvoke works

            _ownIcon = BuildAppIcon();

            _restoreAllItem = new ToolStripMenuItem("Restore all windows", null, (s, e) => RestoreAll());
            _startupItem = new ToolStripMenuItem("Start with Windows", null, (s, e) => ToggleStartup())
            {
                Checked = IsStartupEnabled(),
                CheckOnClick = false
            };

            _customTitleBarFallback = LoadFallbackSetting();
            _fallbackItem = new ToolStripMenuItem("Custom title bar mode", null, (s, e) => ToggleFallback())
            {
                Checked = _customTitleBarFallback,
                CheckOnClick = false,
                ToolTipText = "Needed for apps that draw their own title bar, such as Windows Terminal."
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add(_restoreAllItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_startupItem);
            menu.Items.Add(_fallbackItem);
            menu.Items.Add(new ToolStripMenuItem("How it works", null, (s, e) => ShowHelp()));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => Shutdown()));
            menu.Opening += (s, e) =>
            {
                _startupItem.Checked = IsStartupEnabled();
                _restoreAllItem.Enabled = _hidden.Count > 0;
                _restoreAllItem.Text = _hidden.Count > 0
                    ? "Restore all windows (" + _hidden.Count + ")"
                    : "Restore all windows";
            };

            _appIcon = new NotifyIcon
            {
                Icon = _ownIcon,
                Text = "Tray Minimizer",
                ContextMenuStrip = menu,
                Visible = true
            };

            InstallHook();

            _janitor = new System.Windows.Forms.Timer { Interval = 2000 };
            _janitor.Tick += (s, e) => Sweep();
            _janitor.Start();

            // A low-level hook can be dropped silently by Windows if a callback ever runs long.
            // Reinstalling periodically guarantees the app never goes quietly dead.
            _hookRefresh = new System.Windows.Forms.Timer { Interval = 10 * 60 * 1000 };
            _hookRefresh.Tick += (s, e) => { RemoveHook(); InstallHook(); };
            _hookRefresh.Start();

            SystemEvents.SessionEnding += (s, e) => RestoreAll();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => RestoreAll();

            // Lets a script (build.ps1) ask for a clean exit instead of killing the process,
            // which would leave parked windows hidden with no way back.
            _quitSignal = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\TrayMinimizer.Quit");
            _quitWait = ThreadPool.RegisterWaitForSingleObject(_quitSignal, (state, timedOut) =>
            {
                if (_shuttingDown || !_sink.IsHandleCreated) return;
                try { _sink.BeginInvoke((Action)Shutdown); } catch { }
            }, null, Timeout.Infinite, true);
        }

        // ---------------- hook plumbing ----------------

        private void InstallHook()
        {
            if (_hook != IntPtr.Zero) return;
            _hook = Native.SetWindowsHookEx(Native.WH_MOUSE_LL, _hookProcCached ?? (_hookProcCached = HookCallback),
                Native.GetModuleHandle(null), 0);
            if (_hook == IntPtr.Zero)
            {
                MessageBox.Show("Tray Minimizer could not install the mouse hook (error " +
                    Marshal.GetLastWin32Error() + ").", "Tray Minimizer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Native.HookProc _hookProcCached;

        private void RemoveHook()
        {
            if (_hook == IntPtr.Zero) return;
            Native.UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && !_shuttingDown)
            {
                int msg = wParam.ToInt32();
                if (msg == Native.WM_RBUTTONDOWN || msg == Native.WM_RBUTTONUP)
                {
                    var data = (Native.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Native.MSLLHOOKSTRUCT));

                    if (msg == Native.WM_RBUTTONDOWN)
                    {
                        IntPtr target = MinimizeButtonUnderCursor(data.pt);
                        _armedWindow = target;
                        _armedPoint = data.pt;
                        if (target != IntPtr.Zero) return new IntPtr(1);   // swallow so no system menu pops
                    }
                    else
                    {
                        IntPtr armed = _armedWindow;
                        _armedWindow = IntPtr.Zero;

                        // Confirm on release the way a button does: same window, cursor did not travel.
                        // Deliberately no second hit test here. That was a cross-process round trip that
                        // could time out on a busy app and silently swallow the click.
                        if (armed != IntPtr.Zero && Native.IsWindow(armed) &&
                            Math.Abs(data.pt.x - _armedPoint.x) <= 12 &&
                            Math.Abs(data.pt.y - _armedPoint.y) <= 12)
                        {
                            // Do the real work after the hook returns; low-level hooks must be fast.
                            _sink.BeginInvoke((Action)(() => SendToTray(armed)));
                            return new IntPtr(1);
                        }
                    }
                }
            }
            return Native.CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        /// <summary>Returns the top-level window whose minimize button is under the cursor, or zero.</summary>
        private IntPtr MinimizeButtonUnderCursor(Native.POINT pt)
        {
            IntPtr hwnd = Native.WindowFromPoint(pt);
            if (hwnd == IntPtr.Zero) return IntPtr.Zero;

            IntPtr root = Native.GetAncestor(hwnd, Native.GA_ROOT);
            if (root == IntPtr.Zero) return IntPtr.Zero;

            uint pid;
            Native.GetWindowThreadProcessId(root, out pid);
            if (pid == OwnProcessId) return IntPtr.Zero;

            string cls = GetClassNameOf(root);
            if (cls == "Progman" || cls == "WorkerW" || cls == "Shell_TrayWnd" ||
                cls == "Shell_SecondaryTrayWnd" || cls == "Windows.UI.Core.CoreWindow") return IntPtr.Zero;

            // Where the minimize button has to be: third slot in from the right edge of the real (DWM)
            // frame, at that window's own DPI. All local calls, so it costs the hook nothing.
            bool inSlot = _customTitleBarFallback && IsStandardMinimizeSlot(root, pt);

            IntPtr lp = (IntPtr)(((pt.y & 0xFFFF) << 16) | (pt.x & 0xFFFF));
            IntPtr result;
            // This call blocks the low-level hook, so keep the wait short when geometry already answered.
            // A window busy drawing does not pump messages: a terminal printing a lot of output can miss
            // 100 ms easily, and every miss used to drop the click.
            uint timeout = inSlot ? 30u : 100u;
            IntPtr ok = Native.SendMessageTimeout(root, Native.WM_NCHITTEST, IntPtr.Zero, lp,
                Native.SMTO_ABORTIFHUNG, timeout, out result);

            // No answer: the window is busy, or it is elevated and will never answer this process.
            // Trust the geometry on its own rather than throw the click away.
            if (ok == IntPtr.Zero) return inSlot ? root : IntPtr.Zero;

            int hit = result.ToInt32();

            // Standard frames name the button outright.
            if (hit == Native.HTMINBUTTON) return root;

            // Apps that draw their own title bar (Windows Terminal, some Office and WinUI windows) report
            // the whole strip as HTCAPTION, buttons included, and expose nothing through UI Automation.
            if (inSlot && hit == Native.HTCAPTION) return root;

            return IntPtr.Zero;
        }

        private static bool IsStandardMinimizeSlot(IntPtr hwnd, Native.POINT pt)
        {
            if ((Native.GetWindowLong(hwnd, Native.GWL_STYLE) & Native.WS_MINIMIZEBOX) == 0) return false;

            Native.RECT f;
            // GetWindowRect includes the invisible resize border, which shifts the right edge by several
            // pixels and is wrong when maximized. The DWM frame bounds are what is actually drawn.
            if (Native.DwmGetWindowAttribute(hwnd, Native.DWMWA_EXTENDED_FRAME_BOUNDS, out f,
                    Marshal.SizeOf(typeof(Native.RECT))) != 0)
            {
                if (!Native.GetWindowRect(hwnd, out f)) return false;
            }

            int dpi = 96;
            try { int d = Native.GetDpiForWindow(hwnd); if (d > 0) dpi = d; }
            catch { }

            int bw = (int)Math.Round(Native.CAPTION_BUTTON_W * dpi / 96.0);
            int bh = (int)Math.Round(Native.CAPTION_BUTTON_H * dpi / 96.0);

            if (pt.y < f.Top || pt.y >= f.Top + bh) return false;
            return pt.x >= f.Right - 3 * bw && pt.x < f.Right - 2 * bw;   // close, maximize, minimize
        }

        // ---------------- tray parking ----------------

        private void SendToTray(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd) || _hidden.ContainsKey(hwnd)) return;

            var entry = new HiddenWindow { Handle = hwnd, WasMaximized = Native.IsZoomed(hwnd) };

            string title = GetWindowTitle(hwnd);
            if (string.IsNullOrEmpty(title)) title = "(untitled window)";

            // Hide first so the click feels instant. Fetching the window's icon can take a moment for
            // packaged apps, and the user should not be staring at the window while that happens.
            _hidden[hwnd] = entry;
            Native.ShowWindow(hwnd, Native.SW_HIDE);

            try
            {
                var menu = new ContextMenuStrip();
                menu.Items.Add(new ToolStripMenuItem("Restore", null, (s, e) => Restore(hwnd)));
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.Add(new ToolStripMenuItem("Close window", null, (s, e) => CloseWindow(hwnd)));

                entry.Icon = new NotifyIcon
                {
                    Icon = GetWindowIcon(hwnd),
                    Text = title.Length > 62 ? title.Substring(0, 62) : title,
                    ContextMenuStrip = menu,
                    Visible = true
                };
                entry.Icon.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) Restore(hwnd); };
                entry.Icon.DoubleClick += (s, e) => Restore(hwnd);
            }
            catch
            {
                // Never strand a window because its tray icon could not be built.
                _hidden.Remove(hwnd);
                Native.ShowWindow(hwnd, entry.WasMaximized ? Native.SW_SHOWMAXIMIZED : Native.SW_SHOW);
                throw;
            }
        }

        private void Restore(IntPtr hwnd)
        {
            HiddenWindow entry;
            if (!_hidden.TryGetValue(hwnd, out entry)) return;
            _hidden.Remove(hwnd);

            if (Native.IsWindow(hwnd))
            {
                Native.ShowWindow(hwnd, entry.WasMaximized ? Native.SW_SHOWMAXIMIZED : Native.SW_SHOW);
                if (Native.IsIconic(hwnd)) Native.ShowWindow(hwnd, Native.SW_RESTORE);
                ForceForeground(hwnd);
            }

            DisposeIcon(entry);
        }

        private void RestoreAll()
        {
            var handles = new List<IntPtr>(_hidden.Keys);
            foreach (IntPtr h in handles)
            {
                HiddenWindow entry;
                if (!_hidden.TryGetValue(h, out entry)) continue;
                _hidden.Remove(h);
                if (Native.IsWindow(h))
                {
                    Native.ShowWindow(h, entry.WasMaximized ? Native.SW_SHOWMAXIMIZED : Native.SW_SHOW);
                    if (Native.IsIconic(h)) Native.ShowWindow(h, Native.SW_RESTORE);
                }
                DisposeIcon(entry);
            }
        }

        private void CloseWindow(IntPtr hwnd)
        {
            if (!Native.IsWindow(hwnd)) { Sweep(); return; }
            IntPtr result;
            Native.SendMessageTimeout(hwnd, Native.WM_CLOSE, IntPtr.Zero, IntPtr.Zero,
                Native.SMTO_ABORTIFHUNG, 2000, out result);
            Sweep();
        }

        /// <summary>Drops tray icons for windows that died or came back on their own.</summary>
        private void Sweep()
        {
            var stale = new List<IntPtr>();
            foreach (var kv in _hidden)
            {
                if (!Native.IsWindow(kv.Key) || Native.IsWindowVisible(kv.Key)) stale.Add(kv.Key);
            }
            foreach (IntPtr h in stale)
            {
                HiddenWindow entry = _hidden[h];
                _hidden.Remove(h);
                DisposeIcon(entry);
            }
        }

        private static void DisposeIcon(HiddenWindow entry)
        {
            if (entry.Icon == null) return;
            entry.Icon.Visible = false;
            if (entry.Icon.ContextMenuStrip != null) entry.Icon.ContextMenuStrip.Dispose();
            Icon ico = entry.Icon.Icon;
            entry.Icon.Dispose();
            if (ico != null) ico.Dispose();
            entry.Icon = null;
        }

        private static void ForceForeground(IntPtr hwnd)
        {
            uint dummy;
            uint fgThread = Native.GetWindowThreadProcessId(Native.GetForegroundWindow(), out dummy);
            uint thisThread = Native.GetCurrentThreadId();
            bool attached = fgThread != 0 && fgThread != thisThread &&
                            Native.AttachThreadInput(thisThread, fgThread, true);
            Native.SetForegroundWindow(hwnd);
            Native.BringWindowToTop(hwnd);
            if (attached) Native.AttachThreadInput(thisThread, fgThread, false);
        }

        // ---------------- window metadata ----------------

        private static string GetWindowTitle(IntPtr hwnd)
        {
            var sb = new StringBuilder(512);
            Native.GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString().Trim();
        }

        private static string GetClassNameOf(IntPtr hwnd)
        {
            var sb = new StringBuilder(256);
            Native.GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private Icon GetWindowIcon(IntPtr hwnd)
        {
            IntPtr h = IntPtr.Zero;
            foreach (int which in new[] { Native.ICON_SMALL2, Native.ICON_SMALL, Native.ICON_BIG })
            {
                IntPtr result;
                if (Native.SendMessageTimeout(hwnd, Native.WM_GETICON, (IntPtr)which, IntPtr.Zero,
                        Native.SMTO_ABORTIFHUNG, 80, out result) != IntPtr.Zero && result != IntPtr.Zero)
                {
                    h = result;
                    break;
                }
            }
            if (h == IntPtr.Zero) h = Native.GetClassLongPtr(hwnd, Native.GCLP_HICONSM);
            if (h == IntPtr.Zero) h = Native.GetClassLongPtr(hwnd, Native.GCLP_HICON);

            if (h != IntPtr.Zero)
            {
                try
                {
                    using (Icon borrowed = Icon.FromHandle(h))
                        return (Icon)borrowed.Clone();
                }
                catch { }
            }

            try
            {
                string exe = GetProcessPath(hwnd);
                if (!string.IsNullOrEmpty(exe))
                {
                    Icon fromExe = Icon.ExtractAssociatedIcon(exe);
                    if (fromExe != null) return fromExe;
                }
            }
            catch { }

            return (Icon)_ownIcon.Clone();
        }

        private static string GetProcessPath(IntPtr hwnd)
        {
            uint pid;
            Native.GetWindowThreadProcessId(hwnd, out pid);
            if (pid == 0) return null;
            IntPtr hProc = Native.OpenProcess(0x1000 /* QUERY_LIMITED_INFORMATION */, false, pid);
            if (hProc == IntPtr.Zero) return null;
            try
            {
                var sb = new StringBuilder(1024);
                int size = sb.Capacity;
                return Native.QueryFullProcessImageName(hProc, 0, sb, ref size) ? sb.ToString() : null;
            }
            finally { Native.CloseHandle(hProc); }
        }

        private static Icon BuildAppIcon()
        {
            try
            {
                using (var bmp = new Bitmap(32, 32))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.Clear(Color.Transparent);
                        var body = new[]
                        {
                            new Point(16, 24), new Point(5, 12), new Point(11, 12),
                            new Point(11, 3),  new Point(21, 3), new Point(21, 12),
                            new Point(27, 12)
                        };
                        using (var fill = new SolidBrush(Color.FromArgb(96, 150, 214)))
                            g.FillPolygon(fill, body);
                        using (var edge = new Pen(Color.FromArgb(40, 70, 110), 1.4f))
                            g.DrawPolygon(edge, body);
                        using (var bar = new Pen(Color.FromArgb(96, 150, 214), 3.5f))
                            g.DrawLine(bar, 5, 28, 27, 28);
                    }
                    IntPtr h = bmp.GetHicon();
                    try
                    {
                        using (Icon tmp = Icon.FromHandle(h))
                            return (Icon)tmp.Clone();
                    }
                    finally { Native.DestroyIcon(h); }
                }
            }
            catch { return SystemIcons.Application; }
        }

        // ---------------- startup registration ----------------

        private static string ExePath { get { return Application.ExecutablePath; } }

        private static bool IsStartupEnabled()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RunKey, false))
                {
                    if (k == null) return false;
                    var v = k.GetValue(RunValue) as string;
                    return !string.IsNullOrEmpty(v) &&
                           v.Trim('"').Equals(ExePath, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { return false; }
        }

        private static bool LoadFallbackSetting()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(SettingsKey, false))
                {
                    if (k == null) return true;
                    object v = k.GetValue(FallbackValue);
                    return v == null || Convert.ToInt32(v) != 0;
                }
            }
            catch { return true; }
        }

        private void ToggleFallback()
        {
            _customTitleBarFallback = !_customTitleBarFallback;
            _fallbackItem.Checked = _customTitleBarFallback;
            try
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(SettingsKey))
                {
                    if (k != null) k.SetValue(FallbackValue, _customTitleBarFallback ? 1 : 0, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        private void ToggleStartup()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (k == null) return;
                    if (IsStartupEnabled()) k.DeleteValue(RunValue, false);
                    else k.SetValue(RunValue, "\"" + ExePath + "\"");
                }
                _startupItem.Checked = IsStartupEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not change the startup setting: " + ex.Message,
                    "Tray Minimizer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void ShowHelp()
        {
            MessageBox.Show(
                "Right-click a window's minimize button to park that window in the system tray.\r\n\r\n" +
                "Left-click its tray icon to bring the window back.\r\n" +
                "Right-click its tray icon for Restore and Close.\r\n\r\n" +
                "Exiting Tray Minimizer restores everything it is holding, so no window is ever lost.\r\n\r\n" +
                "Custom title bar mode covers apps that paint their own caption (Windows Terminal, WinUI, " +
                "some Office windows). Those report no minimize button to Windows, so the button is located " +
                "by position instead. Turn it off if a right-click near the top right ever surprises you.\r\n\r\n" +
                "Note: windows owned by programs running as administrator are skipped unless " +
                "Tray Minimizer is also running as administrator. The right-click behaves normally there.",
                "Tray Minimizer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---------------- shutdown ----------------

        private void Shutdown()
        {
            _shuttingDown = true;
            _janitor.Stop();
            _hookRefresh.Stop();
            RemoveHook();
            RestoreAll();
            if (_quitWait != null) _quitWait.Unregister(null);
            if (_quitSignal != null) _quitSignal.Close();
            _appIcon.Visible = false;
            _appIcon.Dispose();
            _sink.Close();
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _shuttingDown = true;
                RemoveHook();
                RestoreAll();
                if (_appIcon != null) { _appIcon.Visible = false; _appIcon.Dispose(); }
            }
            base.Dispose(disposing);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool isNew;
            using (var mutex = new Mutex(true, @"Local\TrayMinimizer.SingleInstance", out isNew))
            {
                if (!isNew)
                {
                    MessageBox.Show("Tray Minimizer is already running.", "Tray Minimizer",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try { Native.SetProcessDpiAwarenessContext(new IntPtr(-4)); } catch { } // per monitor v2

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var ctx = new TrayMinimizerContext();
                Application.ThreadException += (s, e) =>
                {
                    ctx.Dispose();
                    MessageBox.Show("Tray Minimizer hit an error and shut down:\r\n\r\n" + e.Exception.Message,
                        "Tray Minimizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                };

                Application.Run(ctx);
                GC.KeepAlive(mutex);
            }
        }
    }
}
