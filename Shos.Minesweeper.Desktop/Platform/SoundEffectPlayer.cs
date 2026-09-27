using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.Platform;

/// <summary>
/// 効果音を重ねて鳴らす（クラス設計書 4.9。音の出口 SoundEffectOutput の中身）。オンとオフは持たず、鳴らすだけにする（同 9.2 の A1）。
/// 音の出力を 1 つ開き、ミキサーをつないで鳴らし続ける。効果音はミキサーの入力として足すので、前の音を止めずに重なる（仕様書 4.6）。
/// 音の出力の機器がないなどで開けないときは、鳴らさないだけにする（アーキテクチャー設計書 7.6、10 章）。
/// </summary>
public sealed class SoundEffectPlayer : IDisposable
{
    static readonly WaveFormat Format = WaveFormat.CreateIeeeFloatWaveFormat(SoundEffectSynthesizer.SampleRate, channels: 1);

    // 最初の音の遅れは、およそ「バッファーの数 × バッファーの長さ」。短いほど早く鳴るが、短すぎると途切れる（アーキ 11 章 #4）
    const int BufferMilliseconds = 20;
    const int NumberOfBuffers = 3;

    IReadOnlyDictionary<SoundEffect, float[]>? waveforms;
    WaveOut? output;
    MixingSampleProvider? mixer;
    volatile bool hasOutputStopped;

    /// <summary>6 つの波形を合成し、音の出力を開く。2 回目からは何もしない。</summary>
    public void Prepare()
    {
        if (waveforms is not null)
            return;
        waveforms = Enum.GetValues<SoundEffect>().ToDictionary(effect => effect, SoundEffectSynthesizer.Synthesize);
        Open();
    }

    /// <summary>鳴らす（呼ばれたらすぐに返す）。準備の前と、出力を開けないときは何もしない。出力が止まっていたら、開き直す。</summary>
    public void Play(SoundEffect effect)
    {
        if (waveforms is null)
            return;
        if (output is null || hasOutputStopped)
            Open();
        mixer?.AddMixerInput(new WaveformReader(waveforms[effect]));
    }

    public void Dispose() => Close();

    void Open()
    {
        Close();
        try {
            // 入力がなくても無音を出し続け、出力を止めない（鳴らすたびに開かない）
            mixer = new MixingSampleProvider(Format) { ReadFully = true };
            output = new WaveOut { BufferMilliseconds = BufferMilliseconds, NumberOfBuffers = NumberOfBuffers };
            // 再生の途中の失敗は、NAudio が受け止めて知らせる。止まったと覚えるだけにし、次の Play で開き直す（機器が戻れば、また鳴る）。
            // 開き直すときに閉じた前の出力も、止まったと後から知らせてくるので、今の出力の知らせだけを受ける
            output.PlaybackStopped += (sender, _) => {
                if (sender == output)
                    hasOutputStopped = true;
            };
            output.Init(mixer);
            output.Play();
            hasOutputStopped = false;
        } catch (MmException) {
            // 音の出力の機器がないなど。鳴らさずに遊び続けられる
            Close();
        }
    }

    void Close()
    {
        output?.Dispose();
        output = null;
        mixer = null;
    }

    /// <summary>合成しておいた波形を、先頭から 1 回だけ読む。読み終わると、ミキサーが入力から外す。</summary>
    sealed class WaveformReader(float[] samples) : ISampleProvider
    {
        int position;

        public WaveFormat WaveFormat => Format;

        public int Read(Span<float> buffer)
        {
            var count = Math.Min(buffer.Length, samples.Length - position);
            samples.AsSpan(position, count).CopyTo(buffer);
            position += count;
            return count;
        }
    }
}
