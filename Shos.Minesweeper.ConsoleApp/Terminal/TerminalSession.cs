using System.Text;
using Shos.Minesweeper.ConsoleApp.Rendering;

namespace Shos.Minesweeper.ConsoleApp.Terminal;

/// <summary>
/// 端末を使う間の準備と後始末、キーと大きさの読み取り（アーキテクチャー設計書 8.4、クラス設計書 5.3）。
/// using で使う。例外で終わるときも、Dispose が端末を元に戻してから、例外が元の画面に出る。
/// </summary>
public sealed class TerminalSession : IDisposable
{
    static readonly Encoding Utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    readonly uint? originalConsoleMode;
    readonly Encoding originalOutputEncoding;
    readonly bool originalTreatControlCAsInput;

    TerminalSession()
    {
        // Ctrl+C をキーとして受け、ほかのキーと同じ道で終わらせる。後始末を Dispose の 1 か所にするため（アーキテクチャー設計書 8.4）。
        // 入力がリダイレクトされているとここで例外になるので、最初に行う。端末の設定を変えてから失敗すると、
        // Dispose が呼ばれず、変えた設定（コンソールのコード ページなど）が呼び出し元のシェルに残るため
        originalTreatControlCAsInput = Console.TreatControlCAsInput;
        Console.TreatControlCAsInput = true;
        originalOutputEncoding = Console.OutputEncoding;
        Console.OutputEncoding = Utf8WithoutBom;
        originalConsoleMode = WindowsConsoleMode.EnableVirtualTerminalOutput();
        // 1 画面分をまとめて書くため、自動では Flush しない。シーケンスもすべてここに書く（Console.Out と混ぜると順が入れ替わりうる）
        Output = new StreamWriter(Console.OpenStandardOutput(), Utf8WithoutBom) { AutoFlush = false };
        Output.Write(VirtualTerminalSequences.EnterAlternateScreen + VirtualTerminalSequences.HideCursor);
        Output.Flush();
    }

    /// <summary>端末を準備する（VT の解釈、UTF-8、Ctrl+C をキーに、代替画面、カーソルを隠す）。</summary>
    public static TerminalSession Open() => new();

    /// <summary>端末に書く口。書いたら Flush する。</summary>
    public TextWriter Output { get; }

    public TerminalSize Size => new(Console.WindowWidth, Console.WindowHeight);

    /// <summary>来ているキーをすべて読む。キーがなければ待たずに空を返す。読んだキーは画面に出さない。</summary>
    public IReadOnlyList<ConsoleKeyInfo> ReadAvailableKeys()
    {
        var keys = new List<ConsoleKeyInfo>();
        while (Console.KeyAvailable)
            keys.Add(Console.ReadKey(intercept: true));
        return keys;
    }

    /// <summary>準備を元に戻す。Output は閉じない（標準出力を閉じると、後の例外の内容が出せなくなる）。</summary>
    public void Dispose()
    {
        Output.Write(VirtualTerminalSequences.ResetStyle + VirtualTerminalSequences.ShowCursor + VirtualTerminalSequences.LeaveAlternateScreen);
        Output.Flush();
        Console.TreatControlCAsInput = originalTreatControlCAsInput;
        Console.OutputEncoding = originalOutputEncoding;
        if (originalConsoleMode is { } mode)
            WindowsConsoleMode.Restore(mode);
    }
}
