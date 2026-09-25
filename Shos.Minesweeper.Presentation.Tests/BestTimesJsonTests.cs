using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>ベストタイムの保存の形式（アーキテクチャー設計書 10 章）。どのアプリでも、この形式で記録を残す。</summary>
public class BestTimesJsonTests
{
    [Fact]
    public void NothingStoredMeansNoRecords()
    {
        var bestTimes = BestTimesJson.Parse(null);

        Assert.All(Difficulty.Presets, preset => Assert.Null(bestTimes.SecondsOf(preset.Kind)));
    }

    [Fact]
    public void StoredRecordsAreRead()
    {
        var bestTimes = BestTimesJson.Parse("""{"Beginner":23,"Expert":301}""");

        Assert.Equal(23, bestTimes.SecondsOf(DifficultyKind.Beginner));
        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Intermediate));
        Assert.Equal(301, bestTimes.SecondsOf(DifficultyKind.Expert));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[23]")]
    [InlineData("\"Beginner\"")]
    public void UnreadableTextMeansNoRecords(string json)
        => Assert.Null(BestTimesJson.Parse(json).SecondsOf(DifficultyKind.Beginner));

    [Theory]
    [InlineData("""{"Beginner":-1}""")]
    [InlineData("""{"Beginner":1000}""")]
    [InlineData("""{"Beginner":2.5}""")]
    [InlineData("""{"Beginner":"23"}""")]
    [InlineData("""{"beginner":23}""")]
    [InlineData("""{"0":23}""")]
    public void InvalidValuesAreTreatedAsNoRecord(string json)
        => Assert.Null(BestTimesJson.Parse(json).SecondsOf(DifficultyKind.Beginner));

    [Fact]
    public void CustomIsNeverRead()
    {
        var bestTimes = BestTimesJson.Parse("""{"Custom":10,"Intermediate":98}""");

        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Custom));
        Assert.Equal(98, bestTimes.SecondsOf(DifficultyKind.Intermediate));
    }

    [Fact]
    public void RecordsAreWrittenWithoutMissingDifficulties()
    {
        var bestTimes = new BestTimes(new Dictionary<DifficultyKind, int> {
            [DifficultyKind.Expert] = 301,
            [DifficultyKind.Beginner] = 23
        });

        Assert.Equal("""{"Beginner":23,"Expert":301}""", BestTimesJson.Serialize(bestTimes));
    }

    [Fact]
    public void WrittenRecordsCanBeReadAgain()
    {
        var json = BestTimesJson.Serialize(new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Intermediate] = 98 }));

        Assert.Equal(98, BestTimesJson.Parse(json).SecondsOf(DifficultyKind.Intermediate));
    }
}
