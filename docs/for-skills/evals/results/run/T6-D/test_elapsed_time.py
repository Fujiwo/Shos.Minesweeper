import unittest

from elapsed_time import ElapsedTime


class ElapsedTimeTests(unittest.TestCase):
    def test_seven_seconds_are_shown_as_007(self):
        self.assertEqual("007", ElapsedTime(7).text)

    def test_zero_seconds_are_shown_as_000(self):
        self.assertEqual("000", ElapsedTime(0).text)

    def test_two_digit_seconds_are_padded_to_three_digits(self):
        self.assertEqual("042", ElapsedTime(42).text)

    def test_998_seconds_are_shown_as_998(self):
        self.assertEqual("998", ElapsedTime(998).text)

    def test_999_seconds_are_shown_as_999(self):
        self.assertEqual("999", ElapsedTime(999).text)

    def test_1000_seconds_stop_at_999(self):
        self.assertEqual("999", ElapsedTime(1000).text)

    def test_very_long_time_stops_at_999(self):
        self.assertEqual("999", ElapsedTime(123456).text)

    def test_negative_seconds_raise_value_error(self):
        with self.assertRaises(ValueError):
            ElapsedTime(-1)
