"""盤面の大きさ（列の数、行の数、地雷の数）。"""

from dataclasses import dataclass

MIN_LENGTH = 5
MAX_LENGTH = 30
MIN_MINES = 1


def _require_int(name, value):
    if isinstance(value, bool) or not isinstance(value, int):
        raise TypeError(f"{name} must be an int: {value!r}")


@dataclass(frozen=True)
class BoardSize:
    columns: int
    rows: int
    mines: int

    def __post_init__(self):
        for name in ("columns", "rows", "mines"):
            _require_int(name, getattr(self, name))
        for name in ("columns", "rows"):
            value = getattr(self, name)
            if not MIN_LENGTH <= value <= MAX_LENGTH:
                raise ValueError(f"{name} must be {MIN_LENGTH} to {MAX_LENGTH}: {value}")
        # 地雷のないマスが最低 1 つ要る（最初に開くマスのため）
        max_mines = self.cell_count - 1
        if not MIN_MINES <= self.mines <= max_mines:
            raise ValueError(f"mines must be {MIN_MINES} to {max_mines}: {self.mines}")

    @property
    def cell_count(self):
        return self.columns * self.rows
