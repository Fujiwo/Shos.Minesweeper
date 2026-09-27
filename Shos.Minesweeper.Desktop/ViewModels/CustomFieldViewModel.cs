using System.ComponentModel;
using System.Globalization;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.ViewModels;

/// <summary>
/// 難易度ダイアログのカスタムの入力欄 1 つ（クラス設計書 4.5）。
/// 範囲の文は外から受け取る関数で求める。地雷数の範囲は、入力中の幅と高さで変わるからである。
/// </summary>
public sealed class CustomFieldViewModel : INotifyPropertyChanged
{
    readonly Func<string> rangeText;
    string text;
    bool isInvalid;

    public CustomFieldViewModel(string label, int initialValue, Func<string> rangeText)
    {
        Label = label;
        text = initialValue.ToString(CultureInfo.InvariantCulture);
        this.rangeText = rangeText;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Label { get; }

    /// <summary>入力欄の文字列（双方向のバインディング）。</summary>
    public string Text
    {
        get => text;
        set {
            text = value;
            Notify(nameof(Text), nameof(Value));
        }
    }

    /// <summary>
    /// 入力した値。解釈する前に整える（前後の空白を除き、全角の数字も受け付ける。ユーザーの指示、2026-09-27）。
    /// 整数でなければ null（Difficulty.ValidateCustom が、範囲の外と同じ誤りとして扱う）。
    /// </summary>
    public int? Value
        => int.TryParse(InputText.Normalize(Text), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;

    /// <summary>入力できる範囲（「5〜30」）。</summary>
    public string RangeText => rangeText();

    /// <summary>誤りがあるか。「カスタムで始める」を押したときに決め、直しても次に押すまでは残す（Web 版と同じ）。</summary>
    public bool IsInvalid
    {
        get => isInvalid;
        internal set {
            isInvalid = value;
            Notify(nameof(IsInvalid), nameof(ErrorText), nameof(HelpText));
        }
    }

    /// <summary>誤りの文（「5〜30 の整数を入力してください」）。誤りがなければ null。</summary>
    public string? ErrorText => IsInvalid ? CustomDifficultyTexts.InvalidValueOf(RangeText) : null;

    /// <summary>読み上げの補足。誤りの文は範囲を含むので、誤りがあれば誤りの文だけにする。</summary>
    public string HelpText => ErrorText ?? RangeText;

    /// <summary>範囲が変わったことを知らせる（幅か高さが変わって、地雷数の範囲が変わった）。</summary>
    internal void RefreshRange() => Notify(nameof(RangeText), nameof(ErrorText), nameof(HelpText));

    void Notify(params string[] propertyNames)
    {
        foreach (var name in propertyNames)
            PropertyChanged?.Invoke(this, new(name));
    }
}
