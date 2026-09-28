# Shos.Minesweeper

[日本語](README.jp.md)

A Minesweeper game written in C# on .NET 10, available in three editions that share the same game logic:

| Edition | Runs on | Technology | Latest version |
|---------|---------|------------|----------------|
| **Web** | Browsers on smartphones, tablets, and PCs | Blazor WebAssembly (standalone, static site) | 1.1.0 |
| **Desktop** | Windows 11 (x64) | Avalonia UI 12 | 1.0.0 |
| **Console** | Windows 11 (x64) and Linux (x64) terminals | `System.Console` and VT sequences | 1.0.0 |

The user interface of every edition is in Japanese only.

## Play

- **Web**: open <https://fujiwo.github.io/Shos.Minesweeper/>. No installation is needed; an internet connection is required to load the page.
- **Desktop / Console**: download the archive from [GitHub Releases](https://github.com/Fujiwo/Shos.Minesweeper/releases) (tags `desktop-v1.0.0` and `console-v1.0.0`), extract it, and run the executable. They are self-contained single-file apps, so .NET does not need to be installed.
  - The executables are not signed. If Windows shows "Windows protected your PC", choose "More info" and then "Run anyway".
  - Linux (console): extract with `tar -xzf Shos.Minesweeper.ConsoleApp-1.0.0-linux-x64.tar.gz` and run `./Shos.Minesweeper.ConsoleApp` (run `chmod +x Shos.Minesweeper.ConsoleApp` first if permission is denied). If it fails to start, install the ICU libraries (packages starting with `libicu` on Ubuntu and Debian).
  - Console: the terminal must be at least 80 columns × 24 rows.

## Features

- Difficulty levels: Beginner (9×9, 10 mines), Intermediate (16×16, 40 mines), Expert (30×16, 99 mines), and Custom (width 5–30, height 5–24, mines 1 to width × height − 9).
- The first cell you open and its neighbors never contain a mine.
- Chording: opening a revealed number whose adjacent flags equal the number opens the remaining neighbors.
- Best times for Beginner, Intermediate, and Expert are saved locally. Nothing is sent over the internet.
- Web and Desktop: sound effects and animations (cascading reveal, mines appearing on a loss, bouncing flags on a win). Both honor the OS setting that reduces motion, and the sound can be muted from the toolbar.
- Web: responsive layout for portrait and landscape screens, mouse and touch input, light and dark color schemes, and screen reader announcements.

## Controls

### Web

| Action | Mouse | Touch | Keyboard |
|--------|-------|-------|----------|
| Open a cell | Left click | Tap | Arrow keys to select, then Space or Enter |
| Toggle a flag | Right click | Long press (400 ms) | F |
| Chord | Left click a revealed number | Tap a revealed number | Space or Enter on a revealed number |

The flag mode button in the toolbar swaps tap (left click) and long press, so taps place flags. The face button starts a new game, and the difficulty button changes the difficulty.

### Desktop

| Action | Mouse | Keyboard |
|--------|-------|----------|
| Open a cell (chord on a number) | Left click | Arrow keys to select, then Space or Enter |
| Toggle a flag | Right click | F |
| New game | Face button | F2 |
| Change difficulty | Difficulty button (top left) | Tab to the button, then Enter |
| Mute / unmute | Speaker button (top right) | Tab to the button, then Space |

### Console

| Key | Action |
|-----|--------|
| Arrow keys, or H / J / K / L | Select a cell (H left, J down, K up, L right) |
| Space or Enter | Open the selected cell (chord on a number) |
| F | Toggle a flag |
| N | New game |
| D | Choose difficulty (keys 1–4, or arrows and Enter) |
| ? | Show the key list |
| Q, Ctrl+C | Quit |

The board uses symbols: `#` unopened, `F` flag, digits, `*` mine, `@` the mine you hit, `X` wrong flag. Set the `NO_COLOR` environment variable to disable colors.

## Where data is saved

| Edition | Location |
|---------|----------|
| Web | The browser's local storage (not shared across browsers or devices) |
| Desktop | `%LOCALAPPDATA%\Shos.Minesweeper\Desktop` |
| Console (Windows) | `%LOCALAPPDATA%\Shos.Minesweeper\ConsoleApp` |
| Console (Linux) | `~/.local/share/Shos.Minesweeper/ConsoleApp` (or `$XDG_DATA_HOME/Shos.Minesweeper/ConsoleApp`) |

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). The desktop projects target `net10.0-windows` with `EnableWindowsTargeting`, so the whole solution also builds and tests on Linux.

```bash
dotnet build Shos.Minesweeper.slnx
dotnet test                                                # all tests (run at the repository root)

dotnet run --project Shos.Minesweeper                      # Web: http://localhost:5268
dotnet run --project Shos.Minesweeper.Desktop              # Desktop
dotnet run --project Shos.Minesweeper.ConsoleApp           # Console (run inside a terminal)

dotnet publish Shos.Minesweeper -c Release                 # static files in bin/Release/net10.0/publish/wwwroot
```

Tests run on Microsoft.Testing.Platform (selected in `global.json`), so filter with xUnit options such as `--filter-class` and `--filter-method` together with `--project`, not with `--filter`.

## Project structure

| Project | Description |
|---------|-------------|
| `Shos.Minesweeper.GameLogic` | Game rules and best-time storage format. Independent of any UI |
| `Shos.Minesweeper.Presentation` | UI-independent parts shared by the apps: display texts, input mapping, the game session, and sound effect definitions and synthesis |
| `Shos.Minesweeper` | Web edition (Blazor WebAssembly) |
| `Shos.Minesweeper.Desktop` | Desktop edition (Avalonia UI; sound via NAudio) |
| `Shos.Minesweeper.ConsoleApp` | Console edition |
| `Shos.Minesweeper.TestSupport` | Shared test helpers |
| `*.Tests` | Tests for each project (xUnit v3; bUnit for Razor components) |

## Documentation

The project was developed through a documented process (research, specification, UI design, architecture, class design, implementation, refactoring, integration testing, and release), with a review at each step. The documents are in Japanese.

- Web edition: [`docs/`](docs/) — [release notes](docs/release-notes.md)
- Desktop and console editions: [`docs/desktop-console/`](docs/desktop-console/) — [release notes](docs/desktop-console/release-notes.md)
- Reviews: [`docs/reviews/`](docs/reviews/) and [`docs/desktop-console/reviews/`](docs/desktop-console/reviews/)
