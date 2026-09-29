import unittest

from elapsed_time import ElapsedTime


class ElapsedTimeTests(unittest.TestCase):
    def test_single_digit_seconds_are_padded_to_three_digits(self):
        self.assertEqual(ElapsedTime(7).display_text(), "007")

    def test_zero_seconds_is_three_zeros(self):
        self.assertEqual(ElapsedTime(0).display_text(), "000")

    def test_two_digit_seconds_are_padded_to_three_digits(self):
        self.assertEqual(ElapsedTime(42).display_text(), "042")

    def test_999_seconds_is_shown_as_is(self):
        self.assertEqual(ElapsedTime(999).display_text(), "999")

    def test_1000_seconds_stops_at_999(self):
        self.assertEqual(ElapsedTime(1000).display_text(), "999")

    def test_far_over_999_seconds_stops_at_999(self):
        self.assertEqual(ElapsedTime(12345).display_text(), "999")

    def test_negative_seconds_is_rejected(self):
        with self.assertRaises(ValueError):
            ElapsedTime(-1)
