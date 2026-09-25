using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.GameLogic;

/// <summary>
/// 盤面を文字の絵で与え、結果も絵で比べるための補助。
/// 与える絵: * 地雷、. 地雷なし。
/// 結果の絵: # 未開放、F 旗、. 空白、1〜8 数字、* 地雷、X 踏んだ地雷、x 誤った旗。
/// </summary>
static class TestGames
{
    /// <summary>真ん中の列が地雷の壁になっていて、左側から開いても右側には広がらない盤面。</summary>
    public const string WallPicture = """
        ..*..
        ..*..
        ..*..
        ..*..
        ..*..
        """;

    /// <summary>右下から開くと、(0, 1) だけが地雷でない未開放のマスとして残る盤面。(1, 1) は「2」になる。</summary>
    public const string OneCellLeftPicture = """
        *.*..
        .....
        .....
        .....
        .....
        """;

    public static Game FromPicture(string picture, TimeProvider? timeProvider = null)
    {
        var rows = RowsOf(picture);
        var mines = MinePositionsOf(rows);
        var difficulty = Difficulty.Custom(width: rows[0].Length, height: rows.Length, mineCount: mines.Length);
        return new Game(difficulty, timeProvider ?? new FakeTimeProvider(), (_, _) => mines);
    }

    public static void AssertPicture(string expected, Game game)
        => Assert.Equal(RowsOf(expected), RowsOf(PictureOf(game)));

    static string PictureOf(Game game)
        => string.Join('\n', Enumerable.Range(0, game.Board.Height).Select(row => RowPictureOf(game, row)));

    static string RowPictureOf(Game game, int row)
        => string.Concat(Enumerable.Range(0, game.Board.Width).Select(column => CharOf(game, new CellPosition(row, column))));

    static char CharOf(Game game, CellPosition position)
        => game.AppearanceOf(position) switch {
            CellAppearance.Closed       => '#',
            CellAppearance.Flagged      => 'F',
            CellAppearance.Opened       => NumberCharOf(game.Board.CellAt(position).AdjacentMineCount),
            CellAppearance.Mine         => '*',
            CellAppearance.ExplodedMine => 'X',
            CellAppearance.WrongFlag    => 'x',
            var appearance              => throw new ArgumentOutOfRangeException(nameof(position), appearance, null)
        };

    static char NumberCharOf(int adjacentMineCount) => adjacentMineCount == 0 ? '.' : (char)('0' + adjacentMineCount);

    static CellPosition[] MinePositionsOf(string[] rows)
        => [.. rows.SelectMany((line, row) => line.Select((character, column) => (character, position: new CellPosition(row, column))))
                   .Where(cell => cell.character == '*')
                   .Select(cell => cell.position)];

    // ソースの改行が CRLF でも LF でも同じ絵になるように、行に分けて比べる
    static string[] RowsOf(string picture)
        => picture.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
