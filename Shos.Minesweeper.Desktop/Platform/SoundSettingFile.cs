using System.Text.Json;

namespace Shos.Minesweeper.Desktop.Platform;

/// <summary>
/// 効果音のオンとオフを、ファイルに読み書きする（クラス設計書 4.9）。形式は {"SoundEffects":"on"} か {"SoundEffects":"off"}。
/// 値と規則（"off" だけをオフとし、それ以外はオン）は、Web 版の localStorage と同じにする（Web 版のクラス設計書 12.11 の決定 14）。
/// 読めなければオン、書けなくても何もしない（BestTimesFile と同じ受け止め方）。
/// </summary>
public sealed class SoundSettingFile(string path)
{
    const string PropertyName = "SoundEffects";
    const string On = "on";
    const string Off = "off";

    /// <summary>"off" ならオフ。それ以外（ない、読めない、壊れている、ほかの値）はオン。</summary>
    public bool Load()
    {
        try {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            return !IsOff(json.RootElement);
        } catch (Exception exception) when (IsReadFailure(exception)) {
            return true;
        }
    }

    /// <summary>フォルダーがなければ作って書く。書けなければ何もしない（アプリを開いている間は、切り替えた値が効く）。</summary>
    public void Save(bool isEnabled)
    {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, $$"""{"{{PropertyName}}":"{{(isEnabled ? On : Off)}}"}""");
        } catch (Exception exception) when (IsFileAccessFailure(exception)) {
            // 書けないときは、次に起動したときに既定のオンに戻る
        }
    }

    static bool IsOff(JsonElement root)
        => root.ValueKind == JsonValueKind.Object
           && root.TryGetProperty(PropertyName, out var value)
           && value.ValueKind == JsonValueKind.String
           && value.GetString() == Off;

    static bool IsReadFailure(Exception exception) => IsFileAccessFailure(exception) || exception is JsonException;

    static bool IsFileAccessFailure(Exception exception) => exception is IOException or UnauthorizedAccessException;
}
