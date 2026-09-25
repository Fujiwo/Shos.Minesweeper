using System.Text;
using System.Text.Json;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// ベストタイムの保存の形式（アーキテクチャー設計書 10 章）。どのアプリでも、この形式で記録を残す。保存先は各アプリが決める。
/// 形式は、難易度の種類の名前をキーにした JSON（例: {"Beginner":23,"Expert":301}）。記録のない難易度は書かない。
/// </summary>
public static class BestTimesJson
{
    /// <summary>読めない値（JSON でない、キーが違う、整数でない、0〜999 の外）は「記録なし」として捨てる。例外は投げない。</summary>
    public static BestTimes Parse(string? json)
    {
        var secondsByKind = new Dictionary<DifficultyKind, int>();
        if (RootObjectOf(json) is not { } root)
            return new(secondsByKind);
        foreach (var preset in Difficulty.Presets)
            if (root.TryGetProperty(preset.Kind.ToString(), out var value) && IsValidSeconds(value, out var seconds))
                secondsByKind[preset.Kind] = seconds;
        return new(secondsByKind);
    }

    // 公開するときのトリミングで壊れないように、リフレクションを使うシリアライズではなく、書き手で直接書く
    public static string Serialize(BestTimes bestTimes)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) {
            writer.WriteStartObject();
            foreach (var preset in Difficulty.Presets)
                if (bestTimes.SecondsOf(preset.Kind) is int seconds)
                    writer.WriteNumber(preset.Kind.ToString(), seconds);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    static JsonElement? RootObjectOf(string? json)
    {
        if (json is null)
            return null;
        try {
            var root = JsonDocument.Parse(json).RootElement;
            return root.ValueKind == JsonValueKind.Object ? root : null;
        } catch (JsonException) {
            return null;
        }
    }

    static bool IsValidSeconds(JsonElement value, out int seconds)
    {
        seconds = 0;
        return value.ValueKind == JsonValueKind.Number
               && value.TryGetInt32(out seconds)
               && 0 <= seconds && seconds <= Game.MaxElapsedSeconds;
    }
}
