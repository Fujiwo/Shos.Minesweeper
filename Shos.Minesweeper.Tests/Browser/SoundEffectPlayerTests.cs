using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shos.Minesweeper.Browser;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Tests.Browser;

/// <summary>Web 版の音の出口（クラス設計書 12.6）。JavaScript の呼び出しは bUnit の偽物で受ける。聞こえ方は、ブラウザーと実機で確かめる。</summary>
public class SoundEffectPlayerTests : AppTestContext
{
    SoundEffectPlayer Player => Services.GetRequiredService<SoundEffectPlayer>();

    [Fact]
    public void SoundIsOnByDefault()
        => Assert.True(Player.IsEnabled);

    [Fact]
    public async Task PreparingPassesEveryEffectToTheBrowser()
    {
        await Player.PrepareAsync();

        var loads = LoadSoundInvocations();
        Assert.Equal(Enum.GetNames<SoundEffect>(), loads.Select(load => (string)load.Arguments[0]!));
        Assert.All(loads, load => Assert.Equal(SoundEffectSynthesizer.SampleRate, load.Arguments[2]));
    }

    [Fact]
    public async Task EachEffectIsPassedAsItsWaveformBytes()
    {
        await Player.PrepareAsync();

        var won = LoadSoundInvocations().Single(load => (string)load.Arguments[0]! == nameof(SoundEffect.Won));
        Assert.Equal(SoundEffectSynthesizer.Synthesize(SoundEffect.Won).Length * sizeof(float), ((byte[])won.Arguments[1]!).Length);
    }

    // GamePage が作り直されても（見つからないページから戻ったときなど）、合成と受け渡しを繰り返さない
    [Fact]
    public async Task PreparingTwicePassesTheEffectsOnlyOnce()
    {
        await Player.PrepareAsync();
        await Player.PrepareAsync();

        Assert.Equal(Enum.GetValues<SoundEffect>().Length, LoadSoundInvocations().Count);
    }

    [Fact]
    public void PlayingAsksTheBrowserToPlayTheEffect()
    {
        Player.Play(SoundEffect.Chain);

        var play = Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "playSound");
        Assert.Equal("Chain", play.Arguments[0]);
    }

    [Fact]
    public void NothingIsPlayedWhileSoundIsOff()
    {
        Player.IsEnabled = false;

        Player.Play(SoundEffect.Chain);

        Assert.DoesNotContain(JSInterop.Invocations, invocation => invocation.Identifier == "playSound");
    }

    List<JSRuntimeInvocation> LoadSoundInvocations()
        => [.. JSInterop.Invocations.Where(invocation => invocation.Identifier == "loadSound")];
}
