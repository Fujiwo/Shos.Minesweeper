"""立てた旗の数を数え、「残りの地雷の数」を返す。"""


class FlagCounter:
    def __init__(self, mines):
        if isinstance(mines, bool) or not isinstance(mines, int):
            raise TypeError(f"mines must be an int: {mines!r}")
        if mines < 1:
            raise ValueError(f"mines must be 1 or greater: {mines}")
        self._mines = mines
        self._flags = 0

    @property
    def remaining(self):
        """地雷の数 - 旗の数。旗が地雷の数を超えると負になる。"""
        return self._mines - self._flags

    def place(self):
        self._flags += 1

    def remove(self):
        if self._flags == 0:
            raise ValueError("no flag to remove")
        self._flags -= 1
