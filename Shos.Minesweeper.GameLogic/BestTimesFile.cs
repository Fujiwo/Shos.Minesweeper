namespace Shos.Minesweeper.GameLogic;

/// <summary>
/// ベストタイムを、保存の形式（BestTimesJson）のままファイルに読み書きする（デスクトップ版・コンソール版のアーキテクチャー設計書 6.1、9 章）。
/// ファイルの場所は各アプリが決めて渡す。読めなければ記録なし、書けなくても何もしない（遊び続けられる。仕様書 6.2）。
/// 受け止めるのはファイルの読み書きの失敗だけで、パスの誤りなどのプログラムの誤りは受け止めない（同 10 章）。
/// </summary>
public sealed class BestTimesFile(string path)
{
    /// <summary>ない、読めない、壊れているファイルは、記録なしとして読む。</summary>
    public BestTimes Load()
    {
        try {
            return BestTimesJson.Parse(File.ReadAllText(path));
        } catch (Exception exception) when (IsFileAccessFailure(exception)) {
            return new BestTimes();
        }
    }

    /// <summary>フォルダーがなければ作って書く。書けなければ何もしない（メモリーの記録だけが残る）。</summary>
    public void Save(BestTimes bestTimes)
    {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, BestTimesJson.Serialize(bestTimes));
        } catch (Exception exception) when (IsFileAccessFailure(exception)) {
            // 書けないときは、アプリを開いている間だけ記録を保つ（仕様書 6.2）
        }
    }

    // ファイルやフォルダーがない、権限がない、ほかのプロセスが使っている、パスがフォルダーを指している、など
    static bool IsFileAccessFailure(Exception exception)
        => exception is IOException or UnauthorizedAccessException;
}
