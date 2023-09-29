using System;
using System.Runtime.InteropServices;
using Rectangle = System.Drawing.Rectangle;
public class WindowPlacementHelper
{
    // Constants for the SetWindowLoc functions
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOZORDER = 0x0004;

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    public static void SetWindowLocation(IntPtr windowHandle, int x, int y)
    {
        // Call SetWindowPos with the SWP_NOSIZE and SWP_NOZORDER flags
        SetWindowPos(windowHandle, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
    }
    public static IntPtr GetWindow(string? lpClassName, string lpWindowName)
    {
        return FindWindow(lpClassName, lpWindowName);
    }
}