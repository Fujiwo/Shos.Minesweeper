using Shos.Minesweeper.ConsoleApp.Rendering;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// 画面の単位（アーキテクチャー設計書 8.1、クラス設計書 5.4）。キーを受けて次に出す画面を返し、今の状態を Frame にする。端末には触らない。
/// 実装は、ゲーム、ヘルプ、難易度の選択の 3 つで、どこへ移るかは、移る元の画面が自分で決める。
/// </summary>
public interface IScreen
{
    Frame Render();

    /// <summary>次に出す画面。そのままなら this。アプリを終えるなら null。</summary>
    IScreen? HandleKey(ConsoleKeyInfo key);
}
