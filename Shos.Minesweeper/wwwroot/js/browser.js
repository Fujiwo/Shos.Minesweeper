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
