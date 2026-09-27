using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// 盤面を、上下の枠を含む行にする（UI デザイン 3.2、3.4）。1 マスは 2 升（区切り 1 升と記号 1 升）で、1 行の幅は 2 × 列数 + 3 升。
/// カーソルのマスは、左右の区切りの升に角かっこを置き、記号を反転表示にする。色がなくても角かっこで分かる。
/// </summary>
public static class BoardLines
{
    public static IReadOnlyList<FrameLine> Of(Game game, CellPosition cursor)
    {
        var border = FrameLine.Of("+" + new string('-', 2 * game.Board.Width + 1) + "+");
        return [border, .. Enumerable.Range(0, game.Board.Height).Select(row => RowOf(game, row, cursor)), border];
    }

    static FrameLine RowOf(Game game, int row, CellPosition cursor)
    {
        var parts = new List<StyledText> { new("|") };
        for (var column = 0; column < game.Board.Width; column++) {
            parts.Add(new(SeparatorBefore(new CellPosition(row, column), cursor)));
            parts.Add(GlyphOf(game, new CellPosition(row, column), cursor));
        }
        // 最後のマスの右の区切りは、枠の前の升である
        parts.Add(new(SeparatorBefore(new CellPosition(row, game.Board.Width), cursor) + "|"));
        return new FrameLine(parts);
    }

    // マスの左の区切りの升。カーソルのマスの左なら「[」、右（次のマスの左）なら「]」
    static string SeparatorBefore(CellPosition position, CellPosition cursor)
        => position.Row != cursor.Row ? " "
           : position.Column == cursor.Column ? "["
           : position.Column == cursor.Column + 1 ? "]"
           : " ";

    static StyledText GlyphOf(Game game, CellPosition position, CellPosition cursor)
    {
        var glyph = CellGlyphs.Of(game.AppearanceOf(position), game.Board.CellAt(position).AdjacentMineCount);
        return position == cursor ? glyph with { Style = glyph.Style with { IsReversed = true } } : glyph;
    }
}
