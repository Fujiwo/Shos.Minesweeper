namespace Shos.Minesweeper.Desktop.Views;

/// <summary>アイコンの種類（Web 版の UI デザイン 4.4 の IconKind のうち、デスクトップ版で使うもの）。形は Assets/Icons.axaml にある。</summary>
public enum IconKind
{
    Mine,
    ExplodedMine,
    Flag,
    WrongFlag,
    FaceNormal,
    FaceSurprised,
    FaceWon,
    FaceLost,
    Clock,
    Chevron,
    Close,
    Check,
    Warning,
    Star
}
