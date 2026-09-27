namespace Shos.Minesweeper.ConsoleApp.Terminal;

/// <summary>端末の大きさ（列と行。1 升を 1 列と数える）。</summary>
public readonly record struct TerminalSize(int Columns, int Rows)
{
    /// <summary>列も行も、必要な大きさ以上か。</summary>
    public bool IsAtLeast(TerminalSize required) => Columns >= required.Columns && Rows >= required.Rows;
}
