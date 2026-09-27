using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.ConsoleApp.Terminal;

// 区切り 1 の骨組み（docs/desktop-console/05-class-design.md の 8 章）。端末の準備と後始末、キーの届き方を実機で確かめるため、
// 押したキーの中身を画面に出す。Q か Ctrl+C で終わる。ゲームの画面は区切り 3 で作り、この中身は置き換える
using var terminal = TerminalSession.Open();

WriteLine(0, "端末の確認（区切り 1）。キーを押すと、その中身を出します。Q か Ctrl+C で終わります。");
var row = 2;
while (true) {
    foreach (var key in terminal.ReadAvailableKeys()) {
        if (key.Key == ConsoleKey.Q || IsInterrupt(key))
            return;
        WriteLine(row, $"Key={key.Key} KeyChar=U+{(int)key.KeyChar:X4} Modifiers={key.Modifiers}");
        row = row + 1 < terminal.Size.Rows ? row + 1 : 2;
    }
    Thread.Sleep(30);
}

void WriteLine(int row, string text)
{
    terminal.Output.Write(VirtualTerminalSequences.MoveCursorTo(row, column: 0) + text + VirtualTerminalSequences.ClearToEndOfLine);
    terminal.Output.Flush();
}

// Ctrl+C の届き方は Windows と Linux で違いうるので、両方を見る（クラス設計書 5.5）
static bool IsInterrupt(ConsoleKeyInfo key)
    => (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control)) || key.KeyChar == '\u0003';
