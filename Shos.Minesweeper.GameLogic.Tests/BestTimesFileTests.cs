namespace Shos.Minesweeper.GameLogic.Tests;

/// <summary>
/// ベストタイムのファイルの読み書き（デスクトップ版・コンソール版のアーキテクチャー設計書 9 章、仕様書 6.2）。
/// 読めなければ記録なし、書けなくても例外を出さない。テストごとに一時フォルダーを作って消す。
/// </summary>
public sealed class BestTimesFileTests : IDisposable
{
    readonly string folder = Path.Combine(Path.GetTempPath(), "Shos.Minesweeper.Tests", Guid.NewGuid().ToString("N"));

    public BestTimesFileTests() => Directory.CreateDirectory(folder);

    public void Dispose() => Directory.Delete(folder, recursive: true);

    [Fact]
    public void SavedRecordsAreLoadedAgain()
    {
        var path = Path.Combine(folder, "best-times.json");
        var bestTimes = new BestTimes();
        bestTimes.Record(DifficultyKind.Beginner, 23);
        bestTimes.Record(DifficultyKind.Expert, 301);

        new BestTimesFile(path).Save(bestTimes);
        var loaded = new BestTimesFile(path).Load();

        Assert.Equal(23, loaded.SecondsOf(DifficultyKind.Beginner));
        Assert.Null(loaded.SecondsOf(DifficultyKind.Intermediate));
        Assert.Equal(301, loaded.SecondsOf(DifficultyKind.Expert));
    }

    [Fact]
    public void MissingFileLoadsNoRecords()
        => AssertNoRecords(new BestTimesFile(Path.Combine(folder, "missing.json")).Load());

    [Fact]
    public void BrokenFileLoadsNoRecords()
    {
        var path = Path.Combine(folder, "best-times.json");
        File.WriteAllText(path, "{ not json");

        AssertNoRecords(new BestTimesFile(path).Load());
    }

    [Fact]
    public void FileInAMissingFolderLoadsNoRecords()
        => AssertNoRecords(new BestTimesFile(Path.Combine(folder, "missing", "best-times.json")).Load());

    // パスがフォルダーを指していると、読めない（Windows と Linux で例外の種類が違いうる）
    [Fact]
    public void FolderPathLoadsNoRecords()
        => AssertNoRecords(new BestTimesFile(folder).Load());

    [Fact]
    public void SaveCreatesTheMissingFolders()
    {
        var path = Path.Combine(folder, "Shos.Minesweeper", "ConsoleApp", "best-times.json");

        new BestTimesFile(path).Save(RecordOfBeginner(40));

        Assert.Equal(40, new BestTimesFile(path).Load().SecondsOf(DifficultyKind.Beginner));
    }

    // フォルダーを作るはずの場所にファイルがあると、書けない。それでも例外を出さない（遊び続けられる）
    [Fact]
    public void SaveDoesNothingWhenTheFileCannotBeWritten()
    {
        var blocking = Path.Combine(folder, "blocking");
        File.WriteAllText(blocking, "");
        var path = Path.Combine(blocking, "best-times.json");

        new BestTimesFile(path).Save(RecordOfBeginner(40));

        AssertNoRecords(new BestTimesFile(path).Load());
    }

    static BestTimes RecordOfBeginner(int seconds)
    {
        var bestTimes = new BestTimes();
        bestTimes.Record(DifficultyKind.Beginner, seconds);
        return bestTimes;
    }

    static void AssertNoRecords(BestTimes bestTimes)
        => Assert.All(Difficulty.Presets, preset => Assert.Null(bestTimes.SecondsOf(preset.Kind)));
}
