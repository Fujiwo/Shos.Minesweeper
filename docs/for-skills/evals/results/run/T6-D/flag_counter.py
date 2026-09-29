class FlagCounter:
    """立てた旗を数え、地雷の数から旗の数を引いた「残りの地雷の数」を返す。"""

    def __init__(self, mines: int):
        self._mines = mines
        self._flags = 0

    @property
    def remaining_mines(self) -> int:
        # 旗が地雷の数を超えたら負になる(プレイヤーに旗の立てすぎを知らせるため)
        return self._mines - self._flags

    def place_flag(self) -> None:
        self._flags += 1

    def remove_flag(self) -> None:
        if self._flags == 0:
            raise ValueError("取り除く旗がない")
        self._flags -= 1
