from dataclasses import dataclass

MIN_LENGTH = 5
MAX_LENGTH = 30
MIN_MINES = 1


@dataclass(frozen=True)
class BoardSize:
    """盤面の大きさ（列の数、行の数）と地雷の数。作った時点で範囲の内にある。"""

    columns: int
    rows: int
    mines: int

    def __post_init__(self):
        if not MIN_LENGTH <= self.columns <= MAX_LENGTH:
            raise ValueError(f"columns must be {MIN_LENGTH} to {MAX_LENGTH}: {self.columns}")
        if not MIN_LENGTH <= self.rows <= MAX_LENGTH:
            raise ValueError(f"rows must be {MIN_LENGTH} to {MAX_LENGTH}: {self.rows}")
        # 少なくとも 1 マスは地雷のないマスを残す
        max_mines = self.columns * self.rows - 1
        if not MIN_MINES <= self.mines <= max_mines:
            raise ValueError(f"mines must be {MIN_MINES} to {max_mines}: {self.mines}")
