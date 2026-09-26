using System.Diagnostics.CodeAnalysis;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 1 回のゲームの進め方（クラス設計書 12.4）。盤面の操作を Game に伝え、何かが起きた操作を直前の操作として覚え、
/// その効果音を音の出口に渡す。どの版で動いているかは知らない。音の出口を渡さなければ、鳴らないだけである。
/// UI は、Game の Open と ToggleFlag を直接呼ばずに、このクラスを通す（直接呼ぶと、効果音と直前の操作が抜ける）。
/// </summary>
public sealed class GameSession
{
    readonly TimeProvider timeProvider;
    readonly SoundEffectOutput playSoundEffect;
    readonly MineChooser? chooseMines;

    /// <summary>現在のゲーム。</summary>
    public Game Game { get; private set; }

    /// <summary>直前の操作（演出に使う）。何も起きなかった操作では変わらない。新しいゲームを始めると null になる。</summary>
    public MoveResult? LastMove { get; private set; }

    public GameSession(Difficulty difficulty, TimeProvider timeProvider, SoundEffectOutput? playSoundEffect = null, MineChooser? chooseMines = null)
    {
        this.timeProvider = timeProvider;
        // 出口を渡されなければ、何もしない出口を使う。共通の側は、出口が本物かどうかを見分けない
        this.playSoundEffect = playSoundEffect ?? (_ => { });
        this.chooseMines = chooseMines;
        StartNewGame(difficulty);
    }

    [MemberNotNull(nameof(Game))]
    public void StartNewGame(Difficulty difficulty)
    {
        Game = new Game(difficulty, timeProvider, chooseMines);
        LastMove = null;
    }

    public MoveResult Open(CellPosition position) => ReactTo(Game.Open(position));

    public MoveResult ToggleFlag(CellPosition position) => ReactTo(Game.ToggleFlag(position));

    MoveResult ReactTo(MoveResult move)
    {
        // 何も起きなかった操作では、前の操作の演出を止めず、音も鳴らさない（アーキテクチャー設計書 14 章の決定 9）
        if (move.Outcome == MoveOutcome.NoChange)
            return move;
        LastMove = move;
        if (SoundEffectMapping.EffectFor(move) is { } effect)
            playSoundEffect(effect);
        return move;
    }
}
