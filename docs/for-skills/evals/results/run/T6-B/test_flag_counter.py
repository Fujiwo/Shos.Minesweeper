import unittest

from flag_counter import FlagCounter


class FlagCounterTests(unittest.TestCase):
    def test_no_flags_leaves_all_mines(self):
        self.assertEqual(FlagCounter(10).remaining_mines(0), 10)

    def test_flags_are_subtracted_from_mines(self):
        self.assertEqual(FlagCounter(10).remaining_mines(3), 7)

    def test_flags_equal_to_mines_leave_zero(self):
        self.assertEqual(FlagCounter(10).remaining_mines(10), 0)

    def test_more_flags_than_mines_become_negative(self):
        self.assertEqual(FlagCounter(10).remaining_mines(12), -2)
