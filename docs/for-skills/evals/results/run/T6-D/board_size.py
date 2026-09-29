from dataclasses import dataclass

MIN_LENGTH = 5
MAX_LENGTH = 30
MIN_MINES = 1


@dataclass(frozen=True)
class BoardSize:
    """盤面の大きさ(列の数、行の数)と地雷の数。範囲の外の値では作れない。"""

    columns: int
    rows: int
    mines: int

    def __post_init__(self):
        if not MIN_LENGTH <= self.columns <= MAX_LENGTH:
            raise ValueError(f"列の数は {MIN_LENGTH}〜{MAX_LENGTH} である: {self.columns}")
        if not MIN_LENGTH <= self.rows <= MAX_LENGTH:
            raise ValueError(f"行の数は {MIN_LENGTH}〜{MAX_LENGTH} である: {self.rows}")
        if not MIN_MINES <= self.mines <= self.max_mines:
            raise ValueError(f"地雷の数は {MIN_MINES}〜{self.max_mines} である: {self.mines}")

    @property
    def cells(self) -> int:
        return self.columns * self.rows

    @property
    def max_mines(self) -> int:
        # 地雷のないマスが少なくとも 1 つ残るようにする
        return self.cells - 1
