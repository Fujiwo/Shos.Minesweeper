MAX_SHOWN_SECONDS = 999


class ElapsedTime:
    """経過した秒を、999 で止めた 3 桁の文字列として表す。"""

    def __init__(self, seconds: int):
        if seconds < 0:
            raise ValueError(f"経過した秒は 0 以上である: {seconds}")
        self._seconds = seconds

    @property
    def text(self) -> str:
        return f"{min(self._seconds, MAX_SHOWN_SECONDS):03d}"
