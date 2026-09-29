MAX_DISPLAY_SECONDS = 999


class ElapsedTime:
    """経過した秒を、999 で止めた 3 桁の文字列にする。"""

    def __init__(self, seconds):
        if seconds < 0:
            raise ValueError(f"経過した秒は 0 以上でなければならない: {seconds}")
        self._seconds = seconds

    def display_text(self):
        return f"{min(self._seconds, MAX_DISPLAY_SECONDS):03d}"
