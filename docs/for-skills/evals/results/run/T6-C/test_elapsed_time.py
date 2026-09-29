import unittest

from elapsed_time import ElapsedTime


class ElapsedTimeTests(unittest.TestCase):
    def test_single_digit_seconds_are_padded_to_three_digits(self):
        self.assertEqual(ElapsedTime(7).text, "007")

    def test_zero_seconds_are_shown_as_000(self):
        self.assertEqual(ElapsedTime(0).text, "000")

    def test_two_digit_seconds_are_padded_to_three_digits(self):
        self.assertEqual(ElapsedTime(42).text, "042")

    def test_999_seconds_are_shown_as_999(self):
        self.assertEqual(ElapsedTime(999).text, "999")

    def test_seconds_over_999_stop_at_999(self):
        self.assertEqual(ElapsedTime(1000).text, "999")

    def test_negative_seconds_are_rejected(self):
        with self.assertRaises(ValueError):
            ElapsedTime(-1)
