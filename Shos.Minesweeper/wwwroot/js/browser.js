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
