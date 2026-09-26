// C# からは BrowserFeatures だけが呼ぶ（アーキテクチャー設計書 9.1）

// 要素の内側の大きさを監視し、変わるたびに .NET の NotifyResized を呼ぶ
export function observeSize(element, receiver) {
    const observer = new ResizeObserver(entries => {
        const size = entries[entries.length - 1].contentBoxSize[0];
        receiver.invokeMethodAsync("NotifyResized", size.inlineSize, size.blockSize);
    });
    observer.observe(element);
    return { disconnect: () => observer.disconnect() };
}

// 振動に対応していない端末（iOS の Safari など）では何もしない（仕様書 4.4）。失敗してもゲームは続ける
export function vibrate(milliseconds) {
    try {
        navigator.vibrate?.(milliseconds);
    } catch {
    }
}

// 要素の上で、矢印キーと Space の既定の動作（ページのスクロール）を止める。Tab などほかのキーは止めない
const scrollingKeys = new Set(["ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", " "]);

export function suppressKeyScrolling(element) {
    element.addEventListener("keydown", event => {
        if (scrollingKeys.has(event.key))
            event.preventDefault();
    });
}

// localStorage の読み書き。プライベートブラウズなどで保存が禁止されていても、例外を C# に渡さない（仕様書 3.8）
export function readStorage(key) {
    try {
        return localStorage.getItem(key);
    } catch {
        return null;
    }
}

export function writeStorage(key, value) {
    try {
        localStorage.setItem(key, value);
    } catch {
    }
}

// 効果音（アーキテクチャー設計書 9.4）。C# で合成した波形を名前で覚えておき、利用者の操作で動かした AudioContext で鳴らす
const soundWaveforms = new Map();   // 名前 → { samples: Float32Array, sampleRate }
const soundBuffers = new Map();     // 名前 → AudioBuffer
let audioContext;
let isWatchingUserActivation = false;

export function loadSound(name, samples, sampleRate) {
    // 受け取った Uint8Array の先頭が 4 バイトの境界にあるとは限らないので、複写してから Float32Array として読む
    soundWaveforms.set(name, { samples: new Float32Array(samples.slice().buffer), sampleRate });
    if (audioContext)
        createSoundBuffer(name);
    watchUserActivation();
}

// 前の音を止めずに重ねて鳴らす（仕様書 5.6）。まだ鳴らせないとき、失敗したときは何もしない。失敗してもゲームは続ける
export function playSound(name) {
    try {
        const buffer = soundBuffers.get(name);
        if (!audioContext || !buffer)
            return;
        const source = audioContext.createBufferSource();
        source.buffer = buffer;
        source.connect(audioContext.destination);
        source.start();
    } catch {
    }
}

// ブラウザーは、利用者の操作のイベントの中で動かした AudioContext でなければ音を出さない。HTML の仕様で操作と見なされるのは、
// キーを押したとき、マウスのボタンを押したとき、指やペンを離したときで、指やペンで触れたときは含まれない。
// Blazor の処理より前に受けるため、document で捕捉の段階に受ける
function watchUserActivation() {
    if (isWatchingUserActivation)
        return;
    isWatchingUserActivation = true;
    const options = { capture: true, passive: true };
    document.addEventListener("keydown", activateAudio, options);
    document.addEventListener("pointerdown", event => { if (event.pointerType === "mouse") activateAudio(); }, options);
    document.addEventListener("pointerup", event => { if (event.pointerType !== "mouse") activateAudio(); }, options);
    document.addEventListener("touchend", activateAudio, options);
}

// 初めてなら AudioContext を作り、止まっていれば（iOS では電話などの後に interrupted になる）動かす。
// iOS の消音スイッチに従うため、navigator.audioSession は設定しない（仕様書 5.6）
function activateAudio() {
    try {
        if (!audioContext) {
            if (typeof AudioContext === "undefined")
                return;
            audioContext = new AudioContext();
            for (const name of soundWaveforms.keys())
                createSoundBuffer(name);
        }
        if (audioContext.state !== "running")
            audioContext.resume().catch(() => { });
    } catch {
    }
}

// AudioBuffer は AudioContext と違うサンプリング周波数でも作れ、鳴らすときにブラウザーが変換する
function createSoundBuffer(name) {
    const { samples, sampleRate } = soundWaveforms.get(name);
    const buffer = audioContext.createBuffer(1, samples.length, sampleRate);
    buffer.copyToChannel(samples, 0);
    soundBuffers.set(name, buffer);
}
