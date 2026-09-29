from dataclasses import dataclass


@dataclass(frozen=True)
class CellPosition:
    """盤面の上のマスの位置。列と行が同じなら同じ位置である。"""

    column: int
    row: int

    def __post_init__(self):
        if self.column < 0:
            raise ValueError(f"column must be 0 or more: {self.column}")
        if self.row < 0:
            raise ValueError(f"row must be 0 or more: {self.row}")

    def neighbors(self, columns, rows):
        """盤面（columns 列、rows 行）の中にある、周りのマス（最大 8）を返す。"""
        return [
            CellPosition(column, row)
            for row in range(self.row - 1, self.row + 2)
            for column in range(self.column - 1, self.column + 2)
            if (column, row) != (self.column, self.row)
            and 0 <= column < columns and 0 <= row < rows
        ]
