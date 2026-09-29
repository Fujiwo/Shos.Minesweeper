class FlagCounter:
    """地雷の数から立てた旗の数を引いて、残りの地雷の数を数える。"""

    def __init__(self, mines):
        if mines < 0:
            raise ValueError(f"mines must be 0 or more: {mines}")
        self._mines = mines

    def remaining_mines(self, flags):
        """残りの地雷の数。旗が地雷の数を超えると負になる。"""
        if flags < 0:
            raise ValueError(f"flags must be 0 or more: {flags}")
        return self._mines - flags
