using System.Globalization;
using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// カスタムの値（幅、高さ、地雷数）を 1 行ずつ尋ねる（仕様書 5.7、UI デザイン 3.7）。
/// 何も入れずに Enter を押すと、今の盤面の値（[ ] の中）にする。誤ったときは、誤りの行を 1 行だけ出して、同じ行で尋ね直す。
/// </summary>
public sealed class CustomDifficultyInput(Difficulty current)
{
    const string Heading = "カスタム（Enter だけで [ ] の中の値。Esc でやめる）";
    const int FieldCount = 3;
    static readonly TextStyle ErrorStyle = new(Foreground: ConsoleColor.Red);

    /// <summary>尋ねる値。範囲は、地雷数なら答えた幅と高さから決まる。</summary>
    sealed record Field(string Label, int CurrentValue, AllowedRange Range);

    readonly LineEditor editor = new();
    readonly List<int> answers = [];   // 幅、高さ、地雷数の順
    string? error;

    /// <summary>Esc でやめた。</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>3 つの値がそろったときの難易度。そろうまでは null。</summary>
    public Difficulty? Entered { get; private set; }

    /// <summary>Lines の中の、今尋ねている行。</summary>
    public int PromptLineIndex => 1 + answers.Count;

    /// <summary>見出し、答えた行、今尋ねている行、誤りの行（あるときだけ）。</summary>
    public IReadOnlyList<FrameLine> Lines
    {
        get {
            var lines = new List<FrameLine> { FrameLine.Of(Heading) };
            lines.AddRange(answers.Select((answer, index) => FrameLine.Of(PromptOf(FieldAt(index), answer.ToString(CultureInfo.InvariantCulture)))));
            if (answers.Count < FieldCount)
                lines.Add(FrameLine.Of(PromptOf(FieldAt(answers.Count), editor.Text)));
            if (error is not null)
                lines.Add(FrameLine.Of("  " + error, ErrorStyle));
            return lines;
        }
    }

    public void HandleKey(ConsoleKeyInfo key)
    {
        switch (key.Key) {
            case ConsoleKey.Escape:
                IsCancelled = true;
                break;
            case ConsoleKey.Enter:
                Submit();
                break;
            default:
                editor.HandleKey(key);
                break;
        }
    }

    // 入力は、解釈する前に整える（前後の空白を除き、全角の数字を半角にする。仕様書 3 章。ユーザーの指示、2026-09-27）
    void Submit()
    {
        var field = FieldAt(answers.Count);
        var text = InputText.Normalize(editor.Text);
        editor.Clear();
        if (ValueOf(text, field) is not { } value || !field.Range.Contains(value)) {
            error = CustomDifficultyTexts.InvalidValueOf(CustomDifficultyTexts.RangeOf(field.Range));
            return;
        }
        error = null;
        answers.Add(value);
        if (answers.Count == FieldCount)
            Entered = Difficulty.Custom(width: answers[0], height: answers[1], mineCount: answers[2]);
    }

    static int? ValueOf(string text, Field field)
        => text.Length == 0 ? field.CurrentValue
           : int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value
           : null;

    Field FieldAt(int index)
        => index switch {
            0 => new(CustomDifficultyTexts.Width, current.Width, Difficulty.WidthRange),
            1 => new(CustomDifficultyTexts.Height, current.Height, Difficulty.HeightRange),
            _ => new(CustomDifficultyTexts.MineCount, current.MineCount, Difficulty.MineCountRange(answers[0], answers[1]))
        };

    // 「  幅（5〜30）[16]: 20」
    static string PromptOf(Field field, string text)
        => $"  {field.Label}（{CustomDifficultyTexts.RangeOf(field.Range)}）[{field.CurrentValue}]: {text}";
}
