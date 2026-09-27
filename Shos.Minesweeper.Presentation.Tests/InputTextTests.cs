namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>利用者が入力した文字列の正規化（前後の空白を除き、Unicode の互換正規化 NFKC をする。ユーザーの指示、2026-09-27）。</summary>
public class InputTextTests
{
    [Theory]
    [InlineData("16",           "16")]
    [InlineData("１６",         "16")]     // 全角の数字
    [InlineData("  12 ",        "12")]     // 前後の半角の空白
    [InlineData("\u3000８\u3000", "8")]    // 前後の全角の空白
    [InlineData("－５",         "-5")]     // 全角のマイナス
    [InlineData("ｶｽﾀﾑ",         "カスタム")] // 半角のカタカナ
    [InlineData("1 2",          "1 2")]    // 間の空白は残す
    [InlineData("",             "")]
    public void TextIsTrimmedAndNormalizedByCompatibility(string text, string normalized)
        => Assert.Equal(normalized, InputText.Normalize(text));
}
