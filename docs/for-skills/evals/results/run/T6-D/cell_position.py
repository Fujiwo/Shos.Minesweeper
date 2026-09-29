from dataclasses import dataclass


@dataclass(frozen=True)
class CellPosition:
    """盤面の上のマスの位置(列と行)。"""

    column: int
    row: int

    def __post_init__(self):
        if self.column < 0:
            raise ValueError(f"列は 0 以上である: {self.column}")
        if self.row < 0:
            raise ValueError(f"行は 0 以上である: {self.row}")

    def neighbors(self, columns: int, rows: int) -> list["CellPosition"]:
        """列の数 columns、行の数 rows の盤面の中にある、周りのマス(最大 8)を返す。"""
        return [
            CellPosition(column, row)
            for row in range(self.row - 1, self.row + 2)
            for column in range(self.column - 1, self.column + 2)
            if (column, row) != (self.column, self.row)
            and 0 <= column < columns
            and 0 <= row < rows
        ]
