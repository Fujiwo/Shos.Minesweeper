
namespace Shos.Minesweeper.GameLogic.Tests;

public class BestTimesTests
{
    [Fact]
    public void NewBestTimesHaveNoRecords()
        => Assert.All(Difficulty.Presets, difficulty => Assert.Null(new BestTimes().SecondsOf(difficulty.Kind)));

    [Fact]
    public void BestTimesKeepTheGivenRecords()
    {
        var bestTimes = new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Beginner] = 23 });

        Assert.Equal(23, bestTimes.SecondsOf(DifficultyKind.Beginner));
        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Intermediate));
    }

    [Fact]
    public void FirstRecordIsRecorded()
    {
        var bestTimes = new BestTimes();

        var result = bestTimes.Record(DifficultyKind.Beginner, 45);

        Assert.Equal(new BestTimeResult(BestTimeOutcome.FirstRecord, null), result);
        Assert.Equal(45, bestTimes.SecondsOf(DifficultyKind.Beginner));
    }

    [Fact]
    public void ShorterTimeUpdatesTheRecord()
    {
        var bestTimes = WithBeginnerRecord(52);

        var result = bestTimes.Record(DifficultyKind.Beginner, 45);

        Assert.Equal(new BestTimeResult(BestTimeOutcome.Updated, 52), result);
        Assert.Equal(45, bestTimes.SecondsOf(DifficultyKind.Beginner));
    }

    [Theory]
    [InlineData(30)]
    [InlineData(31)]
    public void SameOrLongerTimeDoesNotUpdateTheRecord(int seconds)
    {
        var bestTimes = WithBeginnerRecord(30);

        var result = bestTimes.Record(DifficultyKind.Beginner, seconds);

        Assert.Equal(new BestTimeResult(BestTimeOutcome.NotUpdated, 30), result);
        Assert.Equal(30, bestTimes.SecondsOf(DifficultyKind.Beginner));
    }

    [Fact]
    public void CustomTimeIsNotRecorded()
    {
        var bestTimes = new BestTimes();

        var result = bestTimes.Record(DifficultyKind.Custom, 45);

        Assert.Equal(new BestTimeResult(BestTimeOutcome.NotEligible, null), result);
        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Custom));
    }

    [Theory]
    [InlineData(BestTimeOutcome.FirstRecord, true)]
    [InlineData(BestTimeOutcome.Updated, true)]
    [InlineData(BestTimeOutcome.NotUpdated, false)]
    [InlineData(BestTimeOutcome.NotEligible, false)]
    public void OnlyFirstRecordAndUpdateAreNewBests(BestTimeOutcome outcome, bool isNewBest)
        => Assert.Equal(isNewBest, new BestTimeResult(outcome, null).IsNewBest);

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void RecordingSecondsOutside0To999IsAProgramError(int seconds)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new BestTimes().Record(DifficultyKind.Beginner, seconds));

    [Fact]
    public void GivenRecordsMustNotContainCustom()
        => Assert.Throws<ArgumentOutOfRangeException>(
               () => new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Custom] = 10 }));

    [Fact]
    public void GivenRecordsMustBeFrom0To999()
        => Assert.Throws<ArgumentOutOfRangeException>(
               () => new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Expert] = 1000 }));

    static BestTimes WithBeginnerRecord(int seconds)
        => new(new Dictionary<DifficultyKind, int> { [DifficultyKind.Beginner] = seconds });
}
