class FlagCounter:
    """地雷の数から、立てた旗の数を引いた「残りの地雷の数」を数える。"""

    def __init__(self, mines):
        self._mines = mines

    def remaining_mines(self, flags):
        """旗が地雷の数を超えると負になる(0 で止めない)。"""
        return self._mines - flags
