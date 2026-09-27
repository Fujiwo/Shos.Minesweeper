using Shos.Minesweeper.Desktop.Platform;

namespace Shos.Minesweeper.Desktop.Tests.Platform;

/// <summary>
/// 効果音のオンとオフのファイル（クラス設計書 4.9、9.1 の決定 8）。"off" だけをオフとし、それ以外はオン（Web 版の localStorage と同じ規則）。
/// テストごとに一時フォルダーを作って消す。
/// </summary>
public sealed class SoundSettingFileTests : IDisposable
{
    readonly string folder = Path.Combine(Path.GetTempPath(), "Shos.Minesweeper.Tests", Guid.NewGuid().ToString("N"));

    public SoundSettingFileTests() => Directory.CreateDirectory(folder);

    public void Dispose() => Directory.Delete(folder, recursive: true);

    string PathOf(string name) => Path.Combine(folder, name);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SavedSettingIsLoadedAgain(bool isEnabled)
    {
        var file = new SoundSettingFile(PathOf("sound.json"));

        file.Save(isEnabled);

        Assert.Equal(isEnabled, new SoundSettingFile(PathOf("sound.json")).Load());
    }

    [Fact]
    public void SavedFileIsTheDocumentedJson()
    {
        new SoundSettingFile(PathOf("sound.json")).Save(false);

        Assert.Equal("""{"SoundEffects":"off"}""", File.ReadAllText(PathOf("sound.json")));
    }

    [Fact]
    public void MissingFileIsOn()
        => Assert.True(new SoundSettingFile(PathOf("missing.json")).Load());

    [Theory]
    [InlineData("{ not json")]
    [InlineData("""{"SoundEffects":"OFF"}""")]
    [InlineData("""{"SoundEffects":false}""")]
    [InlineData("""{"Other":"off"}""")]
    [InlineData("""["off"]""")]
    [InlineData("")]
    public void AnythingButOffIsOn(string content)
    {
        File.WriteAllText(PathOf("sound.json"), content);

        Assert.True(new SoundSettingFile(PathOf("sound.json")).Load());
    }

    [Fact]
    public void SavingCreatesTheFolder()
    {
        var path = Path.Combine(folder, "missing", "sound.json");

        new SoundSettingFile(path).Save(false);

        Assert.False(new SoundSettingFile(path).Load());
    }

    // フォルダーの場所にファイルがあると、フォルダーを作れず書けない。例外は出さない
    [Fact]
    public void SavingWhereItCannotWriteDoesNothing()
    {
        File.WriteAllText(PathOf("blocker"), "");
        var file = new SoundSettingFile(Path.Combine(PathOf("blocker"), "sound.json"));

        file.Save(false);

        Assert.True(file.Load());
    }
}
