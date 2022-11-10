using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShareArea.UI
{
  internal class NativeFunctions
  {
    [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    public static extern int RegisterHotKey(IntPtr Hwnd, int ID, int Modifiers, int Key);

    [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    public static extern int UnregisterHotKey(IntPtr Hwnd, int ID);

    [DllImport("kernel32", EntryPoint = "GlobalAddAtomA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    public static extern short GlobalAddAtom(string IDString);

    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    public static extern short GlobalDeleteAtom(short Atom);


    public const int WM_HOTKEY = 0x0312;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);


    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    public static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(System.Drawing.Point p);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
    public static extern IntPtr GetWindowLong32(HandleRef hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
    public static extern IntPtr GetWindowLongPtr64(HandleRef hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
        IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
    {
      var handles = new List<IntPtr>();

      foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
        EnumThreadWindows(thread.Id,
            (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

      return handles;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
    {
      WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
      placement.length = Marshal.SizeOf(placement);
      GetWindowPlacement(hwnd, ref placement);
      return placement;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPLACEMENT
    {
      public int length;
      public int flags;
      public ShowWindowCommands showCmd;
      public Point ptMinPosition;
      public Point ptMaxPosition;
      public Rectangle rcNormalPosition;
    }

    internal enum ShowWindowCommands : int
    {
      Hide = 0,
      Normal = 1,
      Minimized = 2,
      Maximized = 3,
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int Left;        // x position of upper-left corner
      public int Top;         // y position of upper-left corner
      public int Right;       // x position of lower-right corner
      public int Bottom;      // y position of lower-right corner
    }

    public static IntPtr GetWindowLong(HandleRef hWnd, int nIndex)
    {
      if (IntPtr.Size == 4) return GetWindowLong32(hWnd, nIndex);
      else return GetWindowLongPtr64(hWnd, nIndex);
    }

  }
}
