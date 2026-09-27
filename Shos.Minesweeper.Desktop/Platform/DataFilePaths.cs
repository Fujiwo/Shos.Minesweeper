namespace Shos.Minesweeper.Desktop.Platform;

/// <summary>
/// 保存するファイルのパス（アーキテクチャー設計書 9 章）。版ごとのフォルダーに置き、コンソール版とベストタイムを共有しない（仕様書 6.2）。
/// </summary>
public static class DataFilePaths
{
    static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Shos.Minesweeper", "Desktop");

    public static string BestTimes { get; } = Path.Combine(Folder, "best-times.json");
}
