namespace Shos.Minesweeper.ConsoleApp.Tests;

/// <summary>テストで渡すキー。文字を指定しなければ、キーから決まる文字（英字は小文字）にする。</summary>
static class Keys
{
    public static ConsoleKeyInfo Of(ConsoleKey key, char? keyChar = null, bool shift = false, bool control = false)
        => new(keyChar ?? CharOf(key), key, shift, alt: false, control);

    static char CharOf(ConsoleKey key)
        => key switch {
            >= ConsoleKey.A and <= ConsoleKey.Z => (char)('a' + (key - ConsoleKey.A)),
            >= ConsoleKey.D0 and <= ConsoleKey.D9 => (char)('0' + (key - ConsoleKey.D0)),
            ConsoleKey.Spacebar => ' ',
            ConsoleKey.Enter => '\r',
            ConsoleKey.Escape => '\e',
            _ => '\0'
        };
}
