namespace Shos.Minesweeper.ConsoleApp.Rendering;

/// <summary>
/// 1 画面分の行（アーキテクチャー設計書 8.1）。画面の単位が作り、FrameWriter が端末に書く。
/// CursorRow は、文字のカーソル（点滅する棒）を出す行で、その行の末尾に置く（カスタムの値の入力）。null なら隠す。
/// </summary>
public sealed class Frame(IReadOnlyList<FrameLine> lines, int? cursorRow = null)
{
    public IReadOnlyList<FrameLine> Lines { get; } = lines;

    public int? CursorRow { get; } = cursorRow;
}
