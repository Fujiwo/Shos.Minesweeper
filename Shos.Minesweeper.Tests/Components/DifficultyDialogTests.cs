using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>難易度ダイアログ（仕様書 3.1、UI デザイン 2.3、6.3、クラス設計書 9.1 の決定 1、4）。</summary>
public class DifficultyDialogTests : ComponentTestBase
{
    readonly List<Difficulty> selected = [];
    int closeCount;

    [Fact]
    public void DialogIsAModalDialogNamedDifficulty()
    {
        var dialog = RenderDialog(Difficulty.Beginner).Find("[role=dialog]");

        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("難易度", RenderDialog(Difficulty.Beginner).Find($"#{dialog.GetAttribute("aria-labelledby")}").TextContent);
    }

    [Fact]
    public void PresetRowsShowSizeMinesAndBestTime()
    {
        var bestTimes = new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Beginner] = 23 });

        var rows = RenderDialog(Difficulty.Beginner, bestTimes).FindAll("button.preset");

        Assert.Equal(3, rows.Count);
        Assert.Equal(["初級", "9×9・地雷 10", "ベスト 23 秒"], TextsOf(rows[0]));
        Assert.Equal(["中級", "16×16・地雷 40", "記録なし"], TextsOf(rows[1]));
        Assert.Equal(["上級", "30×16・地雷 99", "記録なし"], TextsOf(rows[2]));
    }

    [Fact]
    public void CurrentDifficultyIsMarkedWithACheck()
    {
        var rows = RenderDialog(Difficulty.Intermediate).FindAll("button.preset");

        Assert.Null(rows[0].QuerySelector("svg[data-kind=Check]"));
        Assert.NotNull(rows[1].QuerySelector("svg[data-kind=Check]"));
        Assert.Equal("true", rows[1].GetAttribute("aria-current"));
    }

    [Fact]
    public void ChoosingARowSelectsThatDifficultyEvenIfItIsCurrent()
    {
        var cut = RenderDialog(Difficulty.Beginner);

        cut.FindAll("button.preset")[0].Click();
        cut.FindAll("button.preset")[2].Click();

        Assert.Equal([Difficulty.Beginner, Difficulty.Expert], selected);
    }

    [Fact]
    public void CustomFieldsStartWithTheCurrentBoard()
    {
        var cut = RenderDialog(Difficulty.Intermediate);

        Assert.Equal(["16", "16", "40"], cut.FindAll("input").Select(input => input.GetAttribute("value")));
        Assert.Equal("numeric", cut.Find("#custom-width").GetAttribute("inputmode"));
    }

    [Fact]
    public void RangesAreShownNextToTheFields()
    {
        var cut = RenderDialog(Difficulty.Intermediate);

        Assert.Equal(["5〜30", "5〜24", "1〜247"], cut.FindAll(".range").Select(range => range.TextContent));
    }

    [Fact]
    public void MineCountRangeFollowsTheTypedSize()
    {
        var cut = RenderDialog(Difficulty.Intermediate);

        cut.Find("#custom-width").Input("10");

        Assert.Equal("1〜151", cut.FindAll(".range")[2].TextContent);
    }

    [Fact]
    public void MineCountRangeIsShownAsAFormulaWhenTheSizeIsInvalid()
    {
        var cut = RenderDialog(Difficulty.Intermediate);

        cut.Find("#custom-height").Input("99");

        Assert.Equal("1〜（幅×高さ − 9）", cut.FindAll(".range")[2].TextContent);
    }

    [Fact]
    public void ValidCustomValuesSelectACustomDifficulty()
    {
        var cut = RenderDialog(Difficulty.Beginner);
        cut.Find("#custom-width").Input("20");
        cut.Find("#custom-height").Input("10");
        cut.Find("#custom-mine-count").Input("30");

        cut.Find("button.start-custom").Click();

        Assert.Equal([Difficulty.Custom(20, 10, 30)], selected);
    }

    [Fact]
    public void InvalidCustomValuesAreShownWithoutStarting()
    {
        var cut = RenderDialog(Difficulty.Beginner);
        cut.Find("#custom-height").Input("4");
        cut.Find("#custom-mine-count").Input("abc");

        cut.Find("button.start-custom").Click();

        Assert.Empty(selected);
        Assert.Null(cut.Find("#custom-width").GetAttribute("aria-invalid"));
        Assert.Equal("true", cut.Find("#custom-height").GetAttribute("aria-invalid"));
        Assert.Equal("5〜24 の整数を入力してください", cut.Find("#custom-height-error").TextContent.Trim());
        // 高さが誤っているので地雷数の上限は決まらず、範囲は式で示す（クラス設計書 9.1 の決定 1）
        Assert.Equal("1〜（幅×高さ − 9） の整数を入力してください", cut.Find("#custom-mine-count-error").TextContent.Trim());
        Assert.NotNull(cut.Find("#custom-height-error svg[data-kind=Warning]"));
    }

    [Fact]
    public void MineCountErrorShowsTheRangeForTheTypedSize()
    {
        var cut = RenderDialog(Difficulty.Beginner);
        cut.Find("#custom-mine-count").Input("73");

        cut.Find("button.start-custom").Click();

        Assert.Equal("1〜72 の整数を入力してください", cut.Find("#custom-mine-count-error").TextContent.Trim());
    }

    [Fact]
    public void FocusMovesToTheFirstInvalidField()
    {
        var cut = RenderDialog(Difficulty.Beginner);
        // bUnit は、描き直した要素の参照の印（blazor:elementreference）を空にするので、最初の描画の印を取っておいて比べる
        var heightInputId = cut.Find("#custom-height").GetAttribute("blazor:elementreference");
        cut.Find("#custom-height").Input("4");
        cut.Find("#custom-mine-count").Input("0");

        cut.Find("button.start-custom").Click();

        var focused = (ElementReference)JSInterop.VerifyFocusAsyncInvoke(calledTimes: 2)[1].Arguments[0]!;
        Assert.Equal(heightInputId, focused.Id);
    }

    [Fact]
    public void ErrorsAreClearedWhenTheValuesBecomeValid()
    {
        var cut = RenderDialog(Difficulty.Beginner);
        cut.Find("#custom-width").Input("4");
        cut.Find("button.start-custom").Click();

        cut.Find("#custom-width").Input("5");
        cut.Find("button.start-custom").Click();

        Assert.Empty(cut.FindAll(".error"));
        Assert.Single(selected);
    }

    [Fact]
    public void OpeningFocusesTheCurrentDifficultyRow()
    {
        var cut = RenderDialog(Difficulty.Intermediate);

        JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.FindAll("button.preset")[1]);
    }

    [Fact]
    public void OpeningDuringACustomGameFocusesTheWidthField()
    {
        var cut = RenderDialog(Difficulty.Custom(20, 10, 30));

        JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("#custom-width"));
    }

    [Fact]
    public void CloseButtonEscapeAndBackdropCloseTheDialog()
    {
        var cut = RenderDialog(Difficulty.Beginner);

        cut.Find("button.close").Click();
        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.Find(".backdrop").Click();

        Assert.Equal(3, closeCount);
        Assert.Equal("閉じる", cut.Find("button.close").GetAttribute("aria-label"));
    }

    IRenderedComponent<DifficultyDialog> RenderDialog(Difficulty current, BestTimes? bestTimes = null)
        => Render<DifficultyDialog>(parameters => parameters
               .Add(dialog => dialog.Current, current)
               .Add(dialog => dialog.BestTimes, bestTimes ?? new BestTimes())
               .Add(dialog => dialog.OnSelect, (Difficulty difficulty) => selected.Add(difficulty))
               .Add(dialog => dialog.OnClose, () => closeCount++));

    static string[] TextsOf(IElement row)
        => [row.QuerySelector(".name")!.TextContent, row.QuerySelector(".size")!.TextContent, row.QuerySelector(".best")!.TextContent];
}
