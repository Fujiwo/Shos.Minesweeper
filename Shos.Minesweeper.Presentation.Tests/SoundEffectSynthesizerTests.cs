namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>効果音の波形の合成（UI デザイン 10.6、クラス設計書 12.3）。聞こえ方は、試聴のページと聞き比べて確かめる。</summary>
public class SoundEffectSynthesizerTests
{
    // UI デザイン 10.6 の長さ（秒）
    public static TheoryData<SoundEffect, double> Lengths => new() {
        { SoundEffect.Open,        0.05 },
        { SoundEffect.Chain,       0.15 },   // 80 ミリ秒の 3 音を 35 ミリ秒ずつずらす
        { SoundEffect.FlagPlaced,  0.09 },
        { SoundEffect.FlagRemoved, 0.09 },
        { SoundEffect.Lost,        0.32 },
        { SoundEffect.Won,         0.65 },   // 100 ミリ秒の 3 音と 350 ミリ秒の 1 音
    };

    // UI デザイン 10.6 の最大振幅。同時に鳴る部品があれば、その和（負けは雑音 0.3 と正弦波 0.35）
    public static TheoryData<SoundEffect, double> DesignedPeaks => new() {
        { SoundEffect.Open,        0.2 },
        { SoundEffect.Chain,       0.18 },
        { SoundEffect.FlagPlaced,  0.25 },
        { SoundEffect.FlagRemoved, 0.25 },
        { SoundEffect.Lost,        0.65 },
        { SoundEffect.Won,         0.2 },
    };

    public static TheoryData<SoundEffect> AllEffects => [.. Enum.GetValues<SoundEffect>()];

    [Theory]
    [MemberData(nameof(Lengths))]
    public void WaveformLastsAsLongAsTheDesign(SoundEffect effect, double seconds)
        => Assert.Equal((int)Math.Round(seconds * SoundEffectSynthesizer.SampleRate), SoundEffectSynthesizer.Synthesize(effect).Length);

    // 全体の音量は、最大振幅に 0.8 を掛けた値（UI デザイン 10.6）。音はすぐに減衰するので、いちばん大きいところでも設計の値を超えない
    [Theory]
    [MemberData(nameof(DesignedPeaks))]
    public void LoudestSampleIsAtMostTheDesignedPeak(SoundEffect effect, double designedPeak)
    {
        var loudest = SoundEffectSynthesizer.Synthesize(effect).Max(Math.Abs);

        Assert.InRange(loudest, designedPeak * 0.8 / 2, designedPeak * 0.8);
    }

    [Theory]
    [MemberData(nameof(AllEffects))]
    public void WaveformStartsAndEndsNearSilenceToAvoidClicks(SoundEffect effect)
    {
        var samples = SoundEffectSynthesizer.Synthesize(effect);

        Assert.InRange(samples[0], -0.001f, 0.001f);
        Assert.InRange(samples[^1], -0.001f, 0.001f);
    }

    [Theory]
    [MemberData(nameof(AllEffects))]
    public void SameEffectAlwaysHasTheSameWaveform(SoundEffect effect)
        => Assert.Equal(SoundEffectSynthesizer.Synthesize(effect), SoundEffectSynthesizer.Synthesize(effect));
}
