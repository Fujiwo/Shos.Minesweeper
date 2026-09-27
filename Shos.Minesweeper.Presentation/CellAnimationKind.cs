namespace Shos.Minesweeper.Display;

/// <summary>マスの演出の種類（UI デザイン 10.7）。</summary>
public enum CellAnimationKind
{
    /// <summary>未開放のタイルの見た目から、開いたマスの見た目に変わる。</summary>
    Reveal,
    /// <summary>踏んだ地雷の爆発の形が縮む。</summary>
    Explode,
    /// <summary>地雷が広がりながら現れる（負け）。</summary>
    MineAppear,
    /// <summary>誤った旗の × が現れる（負け）。</summary>
    WrongFlagAppear,
    /// <summary>旗が跳ねる（勝ち）。</summary>
    FlagBounce,
}
