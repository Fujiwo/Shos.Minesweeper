using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>操作の結果から、鳴らす効果音を決める（仕様書 5.6、クラス設計書 12.3 の表）。</summary>
public class SoundEffectMappingTests
{
    static readonly CellPosition Position = new(2, 2);
    static readonly CellPosition[] OneCell = [Position];
    static readonly CellPosition[] TwoCells = [Position, new(2, 3)];

    public static TheoryData<MoveOutcome, CellPosition[], GameStatus, SoundEffect?> AllRows => new() {
        { MoveOutcome.Opened,      OneCell,  GameStatus.Won,     SoundEffect.Won },        // 勝敗が決まったら、開いた音は鳴らさない
        { MoveOutcome.Opened,      TwoCells, GameStatus.Won,     SoundEffect.Won },
        { MoveOutcome.Opened,      OneCell,  GameStatus.Lost,    SoundEffect.Lost },
        { MoveOutcome.Opened,      TwoCells, GameStatus.Lost,    SoundEffect.Lost },       // コードで地雷を 2 つ開いた
        { MoveOutcome.Opened,      OneCell,  GameStatus.Playing, SoundEffect.Open },
        { MoveOutcome.Opened,      TwoCells, GameStatus.Playing, SoundEffect.Chain },      // 0 の連鎖、コード
        { MoveOutcome.FlagPlaced,  [],       GameStatus.Playing, SoundEffect.FlagPlaced },
        { MoveOutcome.FlagPlaced,  [],       GameStatus.NotStarted, SoundEffect.FlagPlaced },
        { MoveOutcome.FlagRemoved, [],       GameStatus.Playing, SoundEffect.FlagRemoved },
        { MoveOutcome.NoChange,    [],       GameStatus.Playing, null },                   // 何も起きなかった操作では鳴らさない
        { MoveOutcome.NoChange,    [],       GameStatus.NotStarted, null },
    };

    [Theory]
    [MemberData(nameof(AllRows))]
    public void EffectIsDecidedByWhatTheMoveDid(MoveOutcome outcome, CellPosition[] openedPositions, GameStatus status, SoundEffect? expected)
        => Assert.Equal(expected, SoundEffectMapping.EffectFor(new MoveResult(Position, outcome, openedPositions, status)));
}
