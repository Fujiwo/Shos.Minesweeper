"""経過時間の表示（3 桁、999 で止める）。"""

import math

MAX_SECONDS = 999


class ElapsedTime:
    def __init__(self, seconds):
        if isinstance(seconds, bool) or not isinstance(seconds, (int, float)):
            raise TypeError(f"seconds must be a number: {seconds!r}")
        if not math.isfinite(seconds) or seconds < 0:
            raise ValueError(f"seconds must be a finite number, 0 or greater: {seconds}")
        self._seconds = seconds

    @property
    def text(self):
        """経過した秒（端数は切り捨て）を 999 で止め、3 桁の文字列にする。"""
        return f"{min(math.floor(self._seconds), MAX_SECONDS):03d}"

    def __str__(self):
        return self.text
