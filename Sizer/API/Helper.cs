using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Sizer.API
{
    public class Helper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT point);

        public const uint GA_ROOT = 2;

        [DllImport("user32.dll")]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0100;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool QueryFullProcessImageName(
            IntPtr hProcess,
            uint dwFlags,
            StringBuilder lpExeName,
            ref uint lpdwSize);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                left = Left;
                top = Top;
                right = Right;
                bottom = Bottom;
            }
            public Rectangle ToRectangle()
            {
                return new Rectangle(left, top, right - left, bottom - top);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        public const int SWP_DEFERERASE = 0x2000;

        public const int SWP_NOCOPYBITS = 0x0100;

        public const int SWP_NOZORDER = 0x0004;

        public const int SWP_NOACTIVATE = 0x0010;

        public const int SWP_NOMOVE = 0x0002;

        public const int SWP_NOSIZE = 0x0001;

        public const int SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        public const int GWL_STYLE = -16;

        public const long WS_CAPTION = 0x00C00000L;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long GetWindowLongPtr(IntPtr hWnd, long nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);

        public const int WM_SIZE = 0x0005;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        public const uint EVENT_OBJECT_DESTROY = 0x8001;
        public const uint OBJID_WINDOW = 0x0000;
        public const uint CHILDID_SELF = 0;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void WinEventDelegate(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime
        );

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
            uint eventMin,
            uint eventMax,
            IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc,
            uint idProcess,
            uint idThread,
            uint dwFlags
        );

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        public const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
