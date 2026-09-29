from dataclasses import dataclass

MIN_SIDE = 5
MAX_SIDE = 30


@dataclass(frozen=True)
class BoardSize:
    """盤面の大きさ(列の数、行の数)と地雷の数。"""

    columns: int
    rows: int
    mines: int

    def __post_init__(self):
        if not MIN_SIDE <= self.columns <= MAX_SIDE:
            raise ValueError(f"列の数は {MIN_SIDE}〜{MAX_SIDE} でなければならない: {self.columns}")
        if not MIN_SIDE <= self.rows <= MAX_SIDE:
            raise ValueError(f"行の数は {MIN_SIDE}〜{MAX_SIDE} でなければならない: {self.rows}")
        # 地雷のないマスが少なくとも 1 つ残るように、上限はマスの数より 1 つ少ない
        max_mines = self.columns * self.rows - 1
        if not 1 <= self.mines <= max_mines:
            raise ValueError(f"地雷の数は 1〜{max_mines} でなければならない: {self.mines}")
