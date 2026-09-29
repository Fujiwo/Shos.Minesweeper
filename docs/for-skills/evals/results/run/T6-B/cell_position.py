from dataclasses import dataclass

_NEIGHBOR_OFFSETS = [
    (column_offset, row_offset)
    for row_offset in (-1, 0, 1)
    for column_offset in (-1, 0, 1)
    if (column_offset, row_offset) != (0, 0)
]


@dataclass(frozen=True)
class CellPosition:
    """盤面の上のマスの位置(列と行)。等しさは列と行で決まる。"""

    column: int
    row: int

    def __post_init__(self):
        if self.column < 0:
            raise ValueError(f"列は 0 以上でなければならない: {self.column}")
        if self.row < 0:
            raise ValueError(f"行は 0 以上でなければならない: {self.row}")

    def neighbors(self, columns, rows):
        """columns 列 rows 行の盤面の中にある、周りのマス(最大 8)を返す。"""
        return [
            CellPosition(column, row)
            for column, row in self._surrounding_coordinates()
            if 0 <= column < columns and 0 <= row < rows
        ]

    def _surrounding_coordinates(self):
        return [(self.column + dc, self.row + dr) for dc, dr in _NEIGHBOR_OFFSETS]
