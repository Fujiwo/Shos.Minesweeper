namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 効果音の波形を合成する（UI デザイン 10.6）。音のファイルは使わない（仕様書 5.6）。
/// 音の上げ下げは、試聴のページ（docs/sounds-preview.html）の Web Audio の指定と同じ形にしてあり、同じ音に聞こえる。
/// </summary>
public static class SoundEffectSynthesizer
{
    /// <summary>1 秒あたりのサンプル数。</summary>
    public const int SampleRate = 44100;

    const double MasterVolume = 0.8;
    const double AttackSeconds = 0.004;
    // 指数関数で音量を上げ下げするときの、始まりと終わりの音量。指数関数では 0 にできないので、聞こえない小ささにする
    const double SilentVolume = 0.0001;
    // 雑音は決まった種の乱数で作る。同じ効果音は、いつも同じ波形になる
    const int NoiseSeed = 1;

    // ド・ミ・ソ・ド（C5、E5、G5、C6）
    const double C5 = 523.25, E5 = 659.25, G5 = 783.99, C6 = 1046.5;

    enum Waveform { Sine, Triangle, Noise }

    /// <summary>音の部品。Noise では、周波数は低域通過の境の周波数を表す。周波数は始まりから終わりへ指数関数で変える。</summary>
    sealed record Part(Waveform Waveform, double StartFrequency, double EndFrequency, double Start, double Length, double Peak)
    {
        public double End => Start + Length;
    }

    /// <summary>-1〜1 の波形（モノラル）。呼ぶたびに新しい配列を返す。</summary>
    public static float[] Synthesize(SoundEffect effect)
    {
        var parts = PartsOf(effect);
        var samples = new float[SampleIndexOf(parts.Max(part => part.End))];
        foreach (var part in parts)
            AddPart(samples, part);
        return samples;
    }

    // UI デザイン 10.6 の表。1 つの効果音が 1 行に当たる
    static Part[] PartsOf(SoundEffect effect)
        => effect switch {
            SoundEffect.Open        => [new(Waveform.Triangle, 900, 600, Start: 0, Length: 0.05, Peak: 0.2)],
            SoundEffect.Chain       => [new(Waveform.Triangle, C5, C5, Start: 0, Length: 0.08, Peak: 0.18),
                                        new(Waveform.Triangle, E5, E5, Start: 0.035, Length: 0.08, Peak: 0.18),
                                        new(Waveform.Triangle, G5, G5, Start: 0.07, Length: 0.08, Peak: 0.18)],
            SoundEffect.FlagPlaced  => [new(Waveform.Sine, 440, 660, Start: 0, Length: 0.09, Peak: 0.25)],
            SoundEffect.FlagRemoved => [new(Waveform.Sine, 660, 440, Start: 0, Length: 0.09, Peak: 0.25)],
            SoundEffect.Lost        => [new(Waveform.Noise, 1200, 200, Start: 0, Length: 0.32, Peak: 0.3),
                                        new(Waveform.Sine, 110, 40, Start: 0, Length: 0.32, Peak: 0.35)],
            SoundEffect.Won         => [new(Waveform.Triangle, C5, C5, Start: 0, Length: 0.1, Peak: 0.2),
                                        new(Waveform.Triangle, E5, E5, Start: 0.1, Length: 0.1, Peak: 0.2),
                                        new(Waveform.Triangle, G5, G5, Start: 0.2, Length: 0.1, Peak: 0.2),
                                        new(Waveform.Triangle, C6, C6, Start: 0.3, Length: 0.35, Peak: 0.2)],
            _                       => throw new ArgumentOutOfRangeException(nameof(effect), effect, null)
        };

    static void AddPart(float[] samples, Part part)
    {
        var source = SourceOf(part);
        var first = SampleIndexOf(part.Start);
        var count = SampleIndexOf(part.End) - first;
        for (var index = 0; index < count; index++) {
            var time = (double)index / SampleRate;
            samples[first + index] += (float)(MasterVolume * VolumeAt(part, time) * source(time));
        }
    }

    // 部品の音源。時刻（部品の始まりからの秒）を順に渡すと、その時刻の値（-1〜1）を返す。位相やフィルターの状態を持つ
    static Func<double, double> SourceOf(Part part)
    {
        if (part.Waveform == Waveform.Noise) {
            var random = new Random(NoiseSeed);
            var lowpass = new LowpassFilter();
            return time => lowpass.Process(random.NextDouble() * 2 - 1, FrequencyAt(part, time));
        }
        var phase = 0.0;   // 1 周期のうちの位置（0〜1）
        return time => {
            var value = part.Waveform == Waveform.Sine ? Math.Sin(2 * Math.PI * phase) : TriangleAt(phase);
            phase = (phase + FrequencyAt(part, time) / SampleRate) % 1;
            return value;
        };
    }

    // 0 から上がり始める三角波
    static double TriangleAt(double phase)
        => phase < 0.25 ? 4 * phase : phase < 0.75 ? 2 - 4 * phase : 4 * phase - 4;

    static double FrequencyAt(Part part, double time)
        => ExponentialRamp(part.StartFrequency, part.EndFrequency, time / part.Length);

    // 4 ミリ秒で最大振幅まで上げ、部品の終わりに向けて下げる。途中で切れたときの「プツッ」という雑音を出さないため
    static double VolumeAt(Part part, double time)
        => time < AttackSeconds
           ? ExponentialRamp(SilentVolume, part.Peak, time / AttackSeconds)
           : ExponentialRamp(part.Peak, SilentVolume, (time - AttackSeconds) / (part.Length - AttackSeconds));

    // Web Audio の exponentialRampToValueAtTime と同じ変え方
    static double ExponentialRamp(double from, double to, double progress)
        => from * Math.Pow(to / from, progress);

    static int SampleIndexOf(double seconds) => (int)Math.Round(seconds * SampleRate);

    /// <summary>
    /// 低域通過のフィルター。Web Audio の BiquadFilterNode（lowpass、Q は既定の 1）と同じ式で、境の周波数をサンプルごとに変えられる。
    /// Web Audio の lowpass は Q をデシベルとして読む（Web Audio API の仕様の BiquadFilterNode の係数の式）。
    /// </summary>
    sealed class LowpassFilter
    {
        const double QInDecibels = 1;

        double input1, input2, output1, output2;

        public double Process(double input, double cutoffFrequency)
        {
            var omega = 2 * Math.PI * cutoffFrequency / SampleRate;
            var alpha = Math.Sin(omega) / (2 * Math.Pow(10, QInDecibels / 20));
            var cosine = Math.Cos(omega);
            var a0 = 1 + alpha;
            var b0 = (1 - cosine) / 2 / a0;
            var b1 = (1 - cosine) / a0;
            var a1 = -2 * cosine / a0;
            var a2 = (1 - alpha) / a0;
            var output = b0 * input + b1 * input1 + b0 * input2 - a1 * output1 - a2 * output2;
            (input2, input1, output2, output1) = (input1, input, output1, output);
            return output;
        }
    }
}
