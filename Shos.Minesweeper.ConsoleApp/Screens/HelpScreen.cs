using Shos.Minesweeper.ConsoleApp.Rendering;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// キーの一覧（仕様書 5.3、UI デザイン 3.6）。何かキーを押すとゲームに戻る（Ctrl+C は ScreenNavigator が先に受けて終わる）。
/// 開いている間も、ゲームの経過時間は進む（仕様書 5.7）。ヘルプには経過時間を出さない。
/// </summary>
public sealed class HelpScreen(GameScreen game) : IScreen
{
    static readonly Frame Help = new([
        FrameLine.Of("キーの一覧"),
        FrameLine.Of(""),
        FrameLine.Of("  矢印キー、H J K L   カーソルを動かす（H 左、J 下、K 上、L 右）"),
        FrameLine.Of("  Space、Enter        開く（数字のマスではコード）"),
        FrameLine.Of("  F                   旗を立てる・外す"),
        FrameLine.Of("  N                   新しいゲーム"),
        FrameLine.Of("  D                   難易度を選ぶ"),
        FrameLine.Of("  ?                   このキーの一覧"),
        FrameLine.Of("  Q、Ctrl+C           終わる"),
        FrameLine.Of(""),
        FrameLine.Of("何かキーを押すと戻ります。"),
    ]);

    public Frame Render() => Help;

    public IScreen? HandleKey(ConsoleKeyInfo key) => game;
}
