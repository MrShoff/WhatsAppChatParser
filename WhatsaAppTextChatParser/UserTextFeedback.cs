using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class UserTextFeedback
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
    const uint SW_HIDE = 0;
    const uint SW_SHOWNORMAL = 1;
    const uint SW_SHOWNOACTIVATE = 4; // Show without activating
    public static bool ConsoleVisible { get; private set; }
    private static IntPtr handle = GetConsoleWindow();

    public static void ConsoleOut(string msg, bool showTimestamp = true)
    {
        string[] curTime = DateTime.Now.GetDateTimeFormats();
        string timestamp = "[" + curTime[120] + "] ";
        ShowConsole();
        Console.WriteLine((showTimestamp ? timestamp : "") + msg);
    }

    #region Console Window Commands
    public static void HideConsole()
    {
        IntPtr handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
        ConsoleVisible = false;
    }
    public static void ShowConsole(bool active = true)
    {
        IntPtr handle = GetConsoleWindow();
        if (active) { ShowWindow(handle, SW_SHOWNORMAL); }
        else { ShowWindow(handle, SW_SHOWNOACTIVATE); }
        ConsoleVisible = true;
    }
    #endregion
}

