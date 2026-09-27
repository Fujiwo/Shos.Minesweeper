using System.Runtime.InteropServices;

namespace Shos.Minesweeper.ConsoleApp.Terminal;

/// <summary>
/// Windows のコンソールで、出力の VT の解釈を有効にし、元に戻す（アーキテクチャー設計書 8.4）。
/// 従来のコンソール ホストでは、有効にしないと VT のシーケンスがそのまま文字として出る。
/// </summary>
internal static partial class WindowsConsoleMode
{
    const int StandardOutputHandle = -11;
    const uint EnableVirtualTerminalProcessing = 0x0004;

    /// <summary>元のモード。Windows でないとき、出力がコンソールでないとき、失敗したときは null。</summary>
    public static uint? EnableVirtualTerminalOutput()
    {
        if (!OperatingSystem.IsWindows())
            return null;
        var handle = GetStdHandle(StandardOutputHandle);
        if (!GetConsoleMode(handle, out var originalMode) || !SetConsoleMode(handle, originalMode | EnableVirtualTerminalProcessing))
            return null;
        return originalMode;
    }

    public static void Restore(uint originalMode) => SetConsoleMode(GetStdHandle(StandardOutputHandle), originalMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint GetStdHandle(int standardHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(nint consoleHandle, out uint mode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(nint consoleHandle, uint mode);
}
