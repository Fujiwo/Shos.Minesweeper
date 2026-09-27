using System.Globalization;
using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// 難易度の選択（仕様書 5.7、UI デザイン 3.7）。初級・中級・上級・カスタムの行から選ぶ。カスタムは、同じ画面の下で値を尋ねる。
/// 開いている間も、ゲームの経過時間は進む（仕様書 5.7）。
/// </summary>
public sealed class DifficultySelectionScreen : IScreen
{
    const string Title = "難易度を選んでください";
    const string Guide = "1〜4 のキー、または矢印と Enter で選びます。Esc で戻ります。";
    const string CurrentMark = "   （今の難易度）";

    // 行は、初級・中級・上級（Difficulty.Presets の順）と、最後のカスタム
    static readonly int CustomRowIndex = Difficulty.Presets.Count;
    static readonly int RowCount = Difficulty.Presets.Count + 1;

    readonly GameScreen game;
    int selectedIndex;
    CustomDifficultyInput? customInput;

    public DifficultySelectionScreen(GameScreen game)
    {
        this.game = game;
        // 開いたときは、今の難易度の行を選んでいる（UI デザイン 3.7）
        selectedIndex = CurrentRowIndex;
    }

    int CurrentRowIndex
        => Difficulty.Presets.ToList().FindIndex(preset => preset.Kind == game.Difficulty.Kind) is var index and >= 0 ? index : CustomRowIndex;

    public IScreen? HandleKey(ConsoleKeyInfo key)
        => customInput is { } input ? HandleCustomKey(input, key) : HandleSelectionKey(key);

    public Frame Render()
    {
        var lines = new List<FrameLine> { FrameLine.Of(Title), FrameLine.Of("") };
        lines.AddRange(Enumerable.Range(0, RowCount).Select(RowOf));
        lines.Add(FrameLine.Of(""));
        lines.Add(FrameLine.Of(Guide));
        if (customInput is not { } input)
            return new Frame(lines);
        lines.Add(FrameLine.Of(""));
        // 文字のカーソルは、今尋ねている行の末尾に出す
        var promptRow = lines.Count + input.PromptLineIndex;
        lines.AddRange(input.Lines);
        return new Frame(lines, promptRow);
    }

    IScreen HandleCustomKey(CustomDifficultyInput input, ConsoleKeyInfo key)
    {
        input.HandleKey(key);
        if (input.IsCancelled)
            return game;
        if (input.Entered is not { } difficulty)
            return this;
        game.StartNewGame(difficulty);
        return game;
    }

    IScreen HandleSelectionKey(ConsoleKeyInfo key)
    {
        if (RowIndexOf(key) is { } index)
            return Choose(index);
        switch (key.Key) {
            case ConsoleKey.UpArrow:
                selectedIndex = Math.Max(0, selectedIndex - 1);
                return this;
            case ConsoleKey.DownArrow:
                selectedIndex = Math.Min(RowCount - 1, selectedIndex + 1);
                return this;
            case ConsoleKey.Enter:
                return Choose(selectedIndex);
            case ConsoleKey.Escape:
                return game;
            default:
                return this;
        }
    }

    // 1〜4 のキー。IME がオンのままの全角の数字も受けるように、キーの文字を整えてから読む（仕様書 3 章）
    static int? RowIndexOf(ConsoleKeyInfo key)
        => int.TryParse(InputText.Normalize(key.KeyChar.ToString()), NumberStyles.None, CultureInfo.InvariantCulture, out var number)
           && 1 <= number && number <= RowCount
           ? number - 1
           : null;

    // 初級・中級・上級は、今と同じ難易度でも新しいゲームを始める（Web 版の UI デザイン 8 章の決定 7）
    IScreen Choose(int index)
    {
        if (index < CustomRowIndex) {
            game.StartNewGame(Difficulty.Presets[index]);
            return game;
        }
        selectedIndex = CustomRowIndex;
        customInput = new CustomDifficultyInput(game.Difficulty);
        return this;
    }

    // 「> 1  初級        9x9   地雷 10   ベスト  23 秒   （今の難易度）」。列をそろえるため、大きさとベストタイムの数は右にそろえる
    FrameLine RowOf(int index)
    {
        var head = $"{(index == selectedIndex ? ">" : " ")} {index + 1}  ";
        var currentMark = index == CurrentRowIndex ? CurrentMark : "";
        if (index == CustomRowIndex)
            return FrameLine.Of(head + DifficultyNames.Of(DifficultyKind.Custom) + currentMark);
        var preset = Difficulty.Presets[index];
        return FrameLine.Of(string.Create(CultureInfo.InvariantCulture,
            $"{head}{DifficultyNames.Of(preset.Kind)}{$"{preset.Width}x{preset.Height}",11}   地雷 {preset.MineCount,2}   {BestTimeOf(preset.Kind)}{currentMark}"));
    }

    string BestTimeOf(DifficultyKind kind)
        => game.BestTimes.SecondsOf(kind) is { } seconds ? string.Create(CultureInfo.InvariantCulture, $"ベスト {seconds,3} 秒") : "記録なし";
}
