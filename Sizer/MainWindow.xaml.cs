using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static Sizer.API.Helper;
using Screen = System.Windows.Forms.Screen;
using WinFormCursor = System.Windows.Forms.Cursor;
using WinFormPadding = System.Windows.Forms.Padding;
using WinFormPoint = System.Drawing.Point;
using WinFormRect = System.Drawing.Rectangle;
using WinFormSize = System.Drawing.Size;

namespace Sizer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.TITLE.MouseLeftButtonDown += (s, e) =>
            {
                this.DragMove();
            };
            this.CLOSE.PreviewMouseDown += (s, e) =>
            {
                e.Handled = true;
            };
            this.MINI.PreviewMouseDown += (s, e) =>
            {
                e.Handled = true;
            };
            this.CLOSE.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left) this.Close();
                e.Handled = true;
            };
            this.MINI.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left) this.WindowState = WindowState.Minimized;
                e.Handled = true;
            };
            this.SETFLAG.Checked += (s, e) =>
            {
                this.WindowFlag = true;
                this.SETFLAGSTD.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
                this.SETFLAG.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
            };
            this.SETFLAG.Unchecked += (s, e) =>
            {
                this.WindowFlag = false;
                this.SETFLAGSTD.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
                this.SETFLAG.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
            };
            this.Loaded += (s, e) =>
            {
                Settings.CheckFile(this);
                this.Setting_1.Checked += (ss, ee) =>
                {
                    SettingIcon_1.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
                    Settings.SaveData(
                        Settings.Dataindex.WindowFlag,
                        Settings.GetBytes(typeof(bool), true));
                };
                this.Setting_1.Unchecked += (ss, ee) =>
                {
                    SettingIcon_1.Stroke = new SolidColorBrush(Colors.Transparent);
                    Settings.SaveData(
                        Settings.Dataindex.WindowFlag,
                        Settings.GetBytes(typeof(bool), false));
                };
                this.Setting_2.Checked += (ss, ee) =>
                {
                    SettingIcon_2.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
                    Settings.SaveData(
                        Settings.Dataindex.SettingFlag,
                        Settings.GetBytes(typeof(bool), true));
                    SETWIDTH.Text = CacheList[CacheIndex].width.ToString();
                    SETHEIGHT.Text = CacheList[CacheIndex].height.ToString();
                    SETLEFT.IsHitTestVisible = true;
                    SETRIGHT.IsHitTestVisible = true;
                    SETADD.IsHitTestVisible = true;
                    SETREMOVE.IsHitTestVisible = true;
                };
                this.Setting_2.Unchecked += (ss, ee) =>
                {
                    SettingIcon_2.Stroke = new SolidColorBrush(Colors.Transparent);
                    Settings.SaveData(
                        Settings.Dataindex.SettingFlag,
                        Settings.GetBytes(typeof(bool), false));
                    SETWIDTH.Text = string.Empty;
                    SETHEIGHT.Text = string.Empty;
                    SETLEFT.IsHitTestVisible = false;
                    SETRIGHT.IsHitTestVisible = false;
                    SETADD.IsHitTestVisible = false;
                    SETREMOVE.IsHitTestVisible = false;
                };
                this.Closing += (ss, ee) =>
                {
                    if (this.WindowState == WindowState.Minimized)
                    {
                        ShowWindow(SELF == IntPtr.Zero ? new WindowInteropHelper(this).Handle : SELF, SW_RESTORE);
                    }
                    if (this.Setting_1.IsChecked)
                    {
                        Settings.SaveData(
                            Settings.Dataindex.WindowData,
                            Settings.GetBytes(typeof(Point), new Point(this.Left, this.Top)));
                    }
                    if (this.Setting_2.IsChecked)
                    {
                        Settings.SaveData(
                            Settings.Dataindex.SettingData,
                            Settings.GetBytes(typeof(Size), CacheList));
                    }
                    Settings.Dispose();
                    if (HOOK != IntPtr.Zero)
                    {
                        UnhookWinEvent(HOOK);
                        GC.KeepAlive(EventDelegate_D);
                        HOOK = IntPtr.Zero;
                    }
                };
            };
            this.SETWIDTH.PreviewTextInput += SETWIDTH_PreviewTextInput;
            this.SETWIDTH.TextChanged += SETWIDTH_TextChanged;
            this.SETHEIGHT.PreviewTextInput += SETHEIGHT_PreviewTextInput;
            this.SETHEIGHT.TextChanged += SETHEIGHT_TextChanged;
            this.SETLEFT.MouseLeftButtonUp += SETLEFT_MouseLeftButtonUp;
            this.SETRIGHT.MouseLeftButtonUp += SETRIGHT_MouseLeftButtonUp;
            this.SETADD.MouseLeftButtonUp += SETADD_MouseLeftButtonUp;
            this.SETREMOVE.MouseLeftButtonUp += SETREMOVE_MouseLeftButtonUp;
            this.GETWIN.MouseLeftButtonUp += GETWIN_MouseLeftButtonUp;
            this.CLEARWIN.MouseLeftButtonUp += CLEARWIN_MouseLeftButtonUp;
            this.Button.MouseLeftButtonUp += Button_MouseLeftButtonUp;
        }

        private IntPtr SELF = IntPtr.Zero;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
            SELF = source?.Handle ?? IntPtr.Zero;
        }

        private const int WM_EXITSIZEMOVE = 0x0232;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_EXITSIZEMOVE:
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (this.IsLoaded)
                        {
                            Screen screen = Screen.FromRectangle(
                                new WinFormRect((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height));
                            WinFormRect rect = screen.WorkingArea;
                            rect.X += 1;
                            rect.Y += 1;
                            rect.Width -= (int)this.Width + 2;
                            rect.Height -= (int)this.Height + 2;
                            if (this.Left > rect.Right)
                            {
                                this.Left = rect.Right;
                            }
                            if (this.Left < rect.Left)
                            {
                                this.Left = rect.Left;
                            }
                            if (this.Top > rect.Bottom)
                            {
                                this.Top = rect.Bottom;
                            }
                            if (this.Top < rect.Top)
                            {
                                this.Top = rect.Top;
                            }
                        }
                    }));
                    break;
            }
            return IntPtr.Zero;
        }

        public List<(short width, short height)> CacheList = new List<(short width, short height)>();

        public int CacheIndex = 0;

        private IntPtr WindowHandle = IntPtr.Zero;

        private (int width, int height) _SETINFO = (0, 0);

        private int SETINFO_WIDTH
        {
            get
            {
                return _SETINFO.width < 0 ? 0 : _SETINFO.width;
            }
            set
            {
                _SETINFO.width = value;
            }
        }

        private int SETINFO_HEIGHT
        {
            get
            {
                return _SETINFO.height < 0 ? 0 : _SETINFO.height;
            }
            set
            {
                _SETINFO.height = value;
            }
        }

        private bool WindowFlag = false;

        private void SETWIDTH_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out int Textint))
            {
                int _SETINFO_WIDTH = SETINFO_WIDTH * 10 + Textint;
                if (_SETINFO_WIDTH >= 0 && _SETINFO_WIDTH <= short.MaxValue)
                {
                    SETINFO_WIDTH = _SETINFO_WIDTH;
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void SETHEIGHT_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out int Textint))
            {
                int _SETINFO_HEIGHT = SETINFO_HEIGHT * 10 + Textint;
                if (_SETINFO_HEIGHT >= 0 && _SETINFO_HEIGHT <= short.MaxValue)
                {
                    SETINFO_HEIGHT = _SETINFO_HEIGHT;
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void SETWIDTH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SETWIDTH.Text))
            {
                SETINFO_WIDTH = 0;
            }
            else
            {
                if (SETWIDTH.Text == SETINFO_WIDTH.ToString())
                {
                    return;
                }
                else
                {
                    if (int.TryParse(SETWIDTH.Text, out int Textint) && Textint >= 0 && Textint <= short.MaxValue)
                    {
                        SETWIDTH.Text = Textint.ToString();
                        SETINFO_WIDTH = Textint;
                        SETWIDTH.CaretIndex = SETWIDTH.Text.Length;
                    }
                    else
                    {
                        if (SETINFO_WIDTH >= 0)
                        {
                            SETWIDTH.Text = SETINFO_WIDTH.ToString();
                            SETWIDTH.CaretIndex = SETWIDTH.Text.Length;
                        }
                        else
                        {
                            SETWIDTH.Text = string.Empty;
                            SETINFO_WIDTH = 0;
                        }
                    }
                }
            }
        }

        private void SETHEIGHT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SETHEIGHT.Text))
            {
                SETINFO_HEIGHT = 0;
            }
            else
            {
                if (SETHEIGHT.Text == SETINFO_HEIGHT.ToString())
                {
                    return;
                }
                else
                {
                    if (int.TryParse(SETHEIGHT.Text, out int Textint) && Textint >= 0 && Textint <= short.MaxValue)
                    {
                        SETHEIGHT.Text = Textint.ToString();
                        SETINFO_HEIGHT = Textint;
                        SETHEIGHT.CaretIndex = SETHEIGHT.Text.Length;
                    }
                    else
                    {
                        if (SETINFO_HEIGHT >= 0)
                        {
                            SETHEIGHT.Text = SETINFO_HEIGHT.ToString();
                            SETHEIGHT.CaretIndex = SETHEIGHT.Text.Length;
                        }
                        else
                        {
                            SETHEIGHT.Text = string.Empty;
                            SETINFO_HEIGHT = 0;
                        }
                    }
                }
            }
        }

        private void SETLEFT_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CacheList.Count > 1 && CacheIndex > 0)
            {
                CacheIndex--;
                SETWIDTH.Text = CacheList[CacheIndex].width.ToString();
                SETHEIGHT.Text = CacheList[CacheIndex].height.ToString();
            }
        }

        private void SETRIGHT_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CacheList.Count > 1 && CacheIndex < CacheList.Count - 1)
            {
                CacheIndex++;
                SETWIDTH.Text = CacheList[CacheIndex].width.ToString();
                SETHEIGHT.Text = CacheList[CacheIndex].height.ToString();
            }
        }

        private void SETADD_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (short width, short height) cache = ((short)SETINFO_WIDTH, (short)SETINFO_HEIGHT);
            if (cache.width > 0 && cache.height > 0)
            {
                if (!CacheList.Contains(cache))
                {
                    CacheList.Add(cache);
                    CacheIndex = CacheList.Count - 1;
                }
            }
        }

        private void SETREMOVE_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (short width, short height) cache = ((short)SETINFO_WIDTH, (short)SETINFO_HEIGHT);
            if (CacheList.Count > 1 && CacheList.Contains(cache))
            {
                CacheList.Remove(cache);
                if (CacheIndex > 0)
                {
                    CacheIndex--;
                }
                SETWIDTH.Text = CacheList[CacheIndex].width.ToString();
                SETHEIGHT.Text = CacheList[CacheIndex].height.ToString();
            }
        }

        private void GETWIN_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.GETWIN.MouseLeave += GETWIN_MouseLeave;
            this.GETWIN.IsHitTestVisible = false;
        }

        private void GETWIN_MouseLeave(object sender, MouseEventArgs e)
        {
            this.GETWIN.MouseLeave -= GETWIN_MouseLeave;
            this.GETWIN.BeginAnimation(Border.BackgroundProperty, null, HandoffBehavior.SnapshotAndReplace);
            this.GETWINSTD.BeginAnimation(TextBlock.ForegroundProperty, null, HandoffBehavior.SnapshotAndReplace);
            this.GETWIN.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x06, 0xB0, 0x25));
            this.GETWINSTD.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
            this.Deactivated += MainWindow_Deactivated;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            this.Deactivated -= MainWindow_Deactivated;
            this.GETWIN.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x30, 0x30));
            this.GETWINSTD.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
            if (HOOK != IntPtr.Zero)
            {
                UnhookWinEvent(HOOK);
                GC.KeepAlive(EventDelegate_D);
                HOOK = IntPtr.Zero;
            }
            var point = WinFormCursor.Position;
            WindowHandle = FindWindow(point.X, point.Y, out uint PID);
            if (WindowHandle == IntPtr.Zero)
            {
                ClearPrint();
            }
            else
            {
                EventDelegate_D = WinEventHook_D;
                HOOK = SetWinEventHook(
                    EVENT_OBJECT_DESTROY,
                    EVENT_OBJECT_DESTROY,
                    IntPtr.Zero,
                    EventDelegate_D,
                    PID,
                    0U,
                    WINEVENT_OUTOFCONTEXT);
            }
            this.GETWIN.IsHitTestVisible = true;
        }

        private IntPtr HOOK = IntPtr.Zero;

        private WinEventDelegate EventDelegate_D;

        private void WinEventHook_D(
            IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                if (idObject == OBJID_WINDOW && idChild == CHILDID_SELF && hwnd != IntPtr.Zero)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (WindowHandle == hwnd)
                        {
                            if (HOOK != IntPtr.Zero)
                            {
                                UnhookWinEvent(HOOK);
                                GC.KeepAlive(EventDelegate_D);
                                HOOK = IntPtr.Zero;
                            }
                            ClearPrint();
                        }
                    });
                }
            }
            catch (Exception) { }
        }

        private readonly string SystemRoot = Environment.GetEnvironmentVariable("SystemRoot")
            ?? Environment.GetEnvironmentVariable("WINDIR")
            ?? @"C:\Windows";

        private readonly HashSet<string> _ignoreList_Class = new HashSet<string>()
        {
            "Progman",
            "WorkerW",
            "Shell_TrayWnd",
            "Shell_SecondaryTrayWnd"
        };

        private readonly string[] _ignoreList_Proc = new string[4]
        {
            "System32",
            "SysWOW64",
            "WinSxS",
            "SystemApps"
        };

        private string GetSystemFolder(string name)
        {
            return Path.Combine(SystemRoot, name);
        }

        private IntPtr FindWindow(int x, int y, out uint _PID)
        {
            _PID = 0U;
            var pt = new POINT { X = x, Y = y };
            IntPtr hwnd = WindowFromPoint(pt);
            if (hwnd == IntPtr.Zero)
            {
                return hwnd;
            }
            IntPtr rootHwnd = GetAncestor(hwnd, GA_ROOT);
            if (rootHwnd == IntPtr.Zero)
            {
                return rootHwnd;
            }
            if (rootHwnd == SELF)
            {
                return IntPtr.Zero;
            }
            else
            {
                var classnamesb = new StringBuilder(256);
                GetClassName(rootHwnd, classnamesb, 256);
                string className = classnamesb.Length == 0 ? string.Empty : classnamesb.ToString();
                classnamesb.Clear();
                if (_ignoreList_Class.Contains(className)) return IntPtr.Zero;
            }
            string handle = $"0x{rootHwnd.ToInt32():X8}";
            string title = "Idle";
            int titlelength = GetWindowTextLength(rootHwnd);
            if (titlelength > 0)
            {
                var titlesb = new StringBuilder(titlelength + 1);
                GetWindowText(rootHwnd, titlesb, titlesb.Capacity);
                title = titlesb.ToString();
                titlesb.Clear();
            }
            string proc = "Idle";
            _ = GetWindowThreadProcessId(rootHwnd, out uint PID);
            if (PID != 0)
            {
                _PID = PID;
                IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, PID);
                if (hProcess != IntPtr.Zero)
                {
                    StringBuilder path = new StringBuilder(32768);
                    uint scount = (uint)path.Capacity;
                    bool result = QueryFullProcessImageName(hProcess, 0, path, ref scount);
                    if (result)
                    {
                        string ps = path.ToString();
                        path.Clear();
                        if (_ignoreList_Proc.Any(dir => ps.StartsWith(GetSystemFolder(dir), StringComparison.OrdinalIgnoreCase)))
                        {
                            return IntPtr.Zero;
                        }
                        proc = Path.GetFileName(ps);
                    }
                    CloseHandle(hProcess);
                }
            }
            this.WINHWND.Text = $"句柄：{handle}";
            this.WINTITLE.Text = $"标题：{title}";
            this.WINPROC.Text = $"进程：{proc}";
            return rootHwnd;
        }

        private void CLEARWIN_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (HOOK != IntPtr.Zero)
            {
                UnhookWinEvent(HOOK);
                GC.KeepAlive(EventDelegate_D);
                HOOK = IntPtr.Zero;
            }
            ClearPrint();
        }

        private void ClearPrint()
        {
            WindowHandle = IntPtr.Zero;
            this.WINHWND.Text = "句柄：Idle";
            this.WINTITLE.Text = "标题：Idle";
            this.WINPROC.Text = "进程：Idle";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:不需要赋值", Justification = "<挂起>")]
        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsWindow(WindowHandle))
            {
                ClearPrint();
                return;
            }
            if (WindowHandle == IntPtr.Zero ||
                SETINFO_WIDTH == 0 ||
                SETINFO_HEIGHT == 0)
            {
                return;
            }
            bool report = true;
            WinFormRect window = WinFormRect.Empty;
            WinFormRect client = WinFormRect.Empty;
            WinFormPadding frame = WinFormPadding.Empty;
            POINT _client = new POINT(0, 0);
            report &= GetWindowRect(WindowHandle, out RECT windowRECT);
            report &= GetClientRect(WindowHandle, out RECT clientRECT);
            report &= ClientToScreen(WindowHandle, ref _client);
            if (report)
            {
                window = windowRECT.ToRectangle();
                client = clientRECT.ToRectangle();
                client.Offset(_client.X, _client.Y);
                frame.Left = client.Left - window.Left;
                frame.Top = client.Top - window.Top;
                frame.Right = window.Right - client.Right;
                frame.Bottom = window.Bottom - client.Bottom;
            }
            else
            {
                return;
            }
            WinFormSize size = new WinFormSize(SETINFO_WIDTH, SETINFO_HEIGHT);
            if (!WindowFlag)
            {
                size += new WinFormSize(frame.Horizontal, frame.Vertical);
            }
            Screen winscreen = Screen.FromRectangle(window);
            WinFormRect winscreen_area = winscreen.WorkingArea;
            int[] min = new int[2] { 640, 480 };
            if (winscreen_area.Width < winscreen_area.Height)
            {
                min = min.Reverse().ToArray();
            }
            if (size.Width < min[0])
            {
                size.Width = min[0];
            }
            if (size.Width > winscreen_area.Width)
            {
                size.Width = winscreen_area.Width;
            }
            if (size.Height < min[1])
            {
                size.Height = min[1];
            }
            if (size.Height > winscreen_area.Height)
            {
                size.Height = winscreen_area.Height;
            }
            WinFormPoint location = WinFormPoint.Empty;
            location.X = (winscreen_area.Width - size.Width) / 2;
            location.Y = (winscreen_area.Height - size.Height) / 2;
            bool final = SetWindowPos(WindowHandle, IntPtr.Zero,
                location.X, location.Y, size.Width, size.Height,
                SWP_DEFERERASE | SWP_NOCOPYBITS | SWP_NOZORDER | SWP_NOACTIVATE);
            if (final)
            {
                long style = GetWindowLongPtr(WindowHandle, GWL_STYLE);
                if (style > 0)
                {
                    if ((style & WS_CAPTION) == 0)
                    {
                        SetWindowLongPtr(WindowHandle, GWL_STYLE, style | WS_CAPTION);
                        SetWindowLongPtr(WindowHandle, GWL_STYLE, style);
                    }
                    else
                    {
                        SetWindowLongPtr(WindowHandle, GWL_STYLE, style & ~WS_CAPTION);
                        SetWindowLongPtr(WindowHandle, GWL_STYLE, style);
                    }
                    _ = SetWindowPos(WindowHandle, IntPtr.Zero,
                        0, 0, 0, 0,
                        SWP_FRAMECHANGED | SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);
                }
                SendMessage(WindowHandle, WM_SIZE, IntPtr.Zero, (IntPtr)(size.Height << 16 | size.Width));
            }
        }
    }
}
