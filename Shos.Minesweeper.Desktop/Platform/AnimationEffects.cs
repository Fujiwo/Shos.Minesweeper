using System.Runtime.InteropServices;

namespace Shos.Minesweeper.Desktop.Platform;

/// <summary>
/// OS の「アニメーション効果」の設定（設定 → アクセシビリティ → 視覚効果。クラス設計書 4.9）。Web 版の prefers-reduced-motion に当たる。
/// 設定は開いたまま変えられるので、呼ぶたびに読む。ビューモデルには AreEnabled をデリゲートとして渡し、テストは Windows の API を呼ばない。
/// </summary>
public static partial class AnimationEffects
{
    const uint GetClientAreaAnimation = 0x1042;   // SPI_GETCLIENTAREAANIMATION

    /// <summary>アニメーション効果がオンか。読めなければ true（演出をする。アーキテクチャー設計書 10 章）。</summary>
    public static bool AreEnabled()
        => !SystemParametersInfo(GetClientAreaAnimation, 0, out var isEnabled, 0) || isEnabled != 0;

    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint action, uint parameter, out int value, uint winIni);
}
