using System.Runtime.InteropServices;

namespace BlufiPrinter;

internal static partial class NativeMethod
{
    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;

    [LibraryImport("kernel32.dll")]
    private static partial nint GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    public static bool DisableQuickEditMode()
    {
        nint hConsole = GetStdHandle(STD_INPUT_HANDLE);

        if (GetConsoleMode(hConsole, out uint mode))
        {
            mode &= ~ENABLE_QUICK_EDIT_MODE;
            return SetConsoleMode(hConsole, mode);
        }
        return false;
    }
}
