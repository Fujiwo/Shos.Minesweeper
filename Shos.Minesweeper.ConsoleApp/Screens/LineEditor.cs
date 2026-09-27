using System.Text;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// 1 行の文字の入力（カスタムの値。UI デザイン 3.7）。Console.ReadLine を使わないのは、Esc を受けられず、
/// 入力の間に経過時間の表示と進行のループが止まるからである（アーキテクチャー設計書 8.3）。
/// </summary>
public sealed class LineEditor
{
    readonly StringBuilder text = new();

    public string Text => text.ToString();

    /// <summary>制御文字でない文字を足し、Backspace で最後の 1 文字を消す。全角の文字もそのまま受け取る（整えるのは読むとき）。</summary>
    public void HandleKey(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Backspace) {
            if (text.Length > 0)
                text.Length--;
            return;
        }
        if (!char.IsControl(key.KeyChar))
            text.Append(key.KeyChar);
    }

    public void Clear() => text.Clear();
}
