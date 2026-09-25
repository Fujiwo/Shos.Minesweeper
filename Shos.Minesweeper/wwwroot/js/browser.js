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
