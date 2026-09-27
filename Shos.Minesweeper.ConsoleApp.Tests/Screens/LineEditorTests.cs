using Shos.Minesweeper.ConsoleApp.Screens;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>1 行の文字の入力（カスタムの値。UI デザイン 3.7）。Console.ReadLine を使わず、キーを 1 つずつ受けて編集する。</summary>
public class LineEditorTests
{
    readonly LineEditor editor = new();

    [Fact]
    public void TypedCharactersAreAppended()
    {
        editor.HandleKey(Keys.Of(ConsoleKey.D2));
        editor.HandleKey(Keys.Of(ConsoleKey.D0));

        Assert.Equal("20", editor.Text);
    }

    // 全角の数字もそのまま受け取る。整えるのは Enter を押して読むとき（仕様書 3 章）
    [Fact]
    public void FullWidthCharactersAreAppendedAsTheyAre()
    {
        editor.HandleKey(new ConsoleKeyInfo('２', default, shift: false, alt: false, control: false));

        Assert.Equal("２", editor.Text);
    }

    [Fact]
    public void BackspaceRemovesTheLastCharacter()
    {
        editor.HandleKey(Keys.Of(ConsoleKey.D2));
        editor.HandleKey(Keys.Of(ConsoleKey.D0));

        editor.HandleKey(Keys.Of(ConsoleKey.Backspace, '\b'));

        Assert.Equal("2", editor.Text);
    }

    [Fact]
    public void BackspaceOnAnEmptyLineDoesNothing()
    {
        editor.HandleKey(Keys.Of(ConsoleKey.Backspace, '\b'));

        Assert.Equal("", editor.Text);
    }

    [Theory]
    [InlineData(ConsoleKey.UpArrow)]
    [InlineData(ConsoleKey.Tab)]
    public void ControlKeysAreNotAppended(ConsoleKey key)
    {
        editor.HandleKey(Keys.Of(key, key == ConsoleKey.Tab ? '\t' : '\0'));

        Assert.Equal("", editor.Text);
    }

    [Fact]
    public void ClearEmptiesTheLine()
    {
        editor.HandleKey(Keys.Of(ConsoleKey.D5));

        editor.Clear();

        Assert.Equal("", editor.Text);
    }
}
