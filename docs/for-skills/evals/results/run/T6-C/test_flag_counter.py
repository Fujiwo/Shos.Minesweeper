import unittest

from flag_counter import FlagCounter


class FlagCounterTests(unittest.TestCase):
    def test_remaining_mines_equal_mines_when_no_flags(self):
        self.assertEqual(FlagCounter(10).remaining_mines(0), 10)

    def test_remaining_mines_decrease_by_flags(self):
        self.assertEqual(FlagCounter(10).remaining_mines(3), 7)

    def test_remaining_mines_are_zero_when_flags_equal_mines(self):
        self.assertEqual(FlagCounter(10).remaining_mines(10), 0)

    def test_remaining_mines_are_negative_when_flags_exceed_mines(self):
        self.assertEqual(FlagCounter(10).remaining_mines(12), -2)

    def test_negative_flags_are_rejected(self):
        with self.assertRaises(ValueError):
            FlagCounter(10).remaining_mines(-1)

    def test_negative_mines_are_rejected(self):
        with self.assertRaises(ValueError):
            FlagCounter(-1)
