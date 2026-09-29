MAX_SECONDS = 999


class ElapsedTime:
    """経過した秒を、999 で止めた 3 桁の文字列で表す。"""

    def __init__(self, seconds):
        if seconds < 0:
            raise ValueError(f"seconds must be 0 or more: {seconds}")
        self._seconds = seconds

    @property
    def text(self):
        return f"{min(self._seconds, MAX_SECONDS):03d}"
