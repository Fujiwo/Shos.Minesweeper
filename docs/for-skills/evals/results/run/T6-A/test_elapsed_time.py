import unittest

from elapsed_time import ElapsedTime


class ElapsedTimeTests(unittest.TestCase):
    def test_text_is_padded_to_three_digits(self):
        cases = {0: "000", 7: "007", 42: "042", 123: "123"}
        for seconds, expected in cases.items():
            with self.subTest(seconds=seconds):
                self.assertEqual(ElapsedTime(seconds).text, expected)

    def test_text_stops_at_999(self):
        for seconds in [999, 1000, 12345]:
            with self.subTest(seconds=seconds):
                self.assertEqual(ElapsedTime(seconds).text, "999")

    def test_fractional_seconds_are_truncated(self):
        self.assertEqual(ElapsedTime(7.9).text, "007")
        self.assertEqual(ElapsedTime(999.5).text, "999")

    def test_str_returns_text(self):
        self.assertEqual(str(ElapsedTime(7)), "007")

    def test_negative_seconds_are_rejected(self):
        with self.assertRaises(ValueError):
            ElapsedTime(-1)

    def test_non_finite_seconds_are_rejected(self):
        for seconds in [float("nan"), float("inf")]:
            with self.subTest(seconds=seconds):
                with self.assertRaises(ValueError):
                    ElapsedTime(seconds)

    def test_non_number_seconds_are_rejected(self):
        for seconds in ["7", None, True]:
            with self.subTest(seconds=seconds):
                with self.assertRaises(TypeError):
                    ElapsedTime(seconds)


if __name__ == "__main__":
    unittest.main()
