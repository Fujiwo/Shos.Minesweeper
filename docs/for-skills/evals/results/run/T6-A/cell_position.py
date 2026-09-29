"""盤面の上のマスの位置（列と行）。"""

from dataclasses import dataclass


def _require_non_negative_int(name, value):
    # bool は int の派生だが、位置としては誤りなので弾く
    if isinstance(value, bool) or not isinstance(value, int):
        raise TypeError(f"{name} must be an int: {value!r}")
    if value < 0:
        raise ValueError(f"{name} must be 0 or greater: {value}")


@dataclass(frozen=True)
class CellPosition:
    column: int
    row: int

    def __post_init__(self):
        _require_non_negative_int("column", self.column)
        _require_non_negative_int("row", self.row)

    def neighbors(self, columns, rows):
        """盤面（columns 列、rows 行）の中にある周りのマス（最大 8）を返す。"""
        return [
            CellPosition(column, row)
            for row in range(max(self.row - 1, 0), min(self.row + 2, rows))
            for column in range(max(self.column - 1, 0), min(self.column + 2, columns))
            if (column, row) != (self.column, self.row)
        ]
