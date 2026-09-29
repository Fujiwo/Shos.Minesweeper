import unittest

from flag_counter import FlagCounter


class FlagCounterTests(unittest.TestCase):
    def test_remaining_mines_equal_mines_before_any_flag(self):
        self.assertEqual(10, FlagCounter(10).remaining_mines)

    def test_placing_a_flag_decreases_remaining_mines_by_one(self):
        counter = FlagCounter(10)
        counter.place_flag()
        self.assertEqual(9, counter.remaining_mines)

    def test_removing_a_flag_increases_remaining_mines_by_one(self):
        counter = FlagCounter(10)
        counter.place_flag()
        counter.place_flag()
        counter.remove_flag()
        self.assertEqual(9, counter.remaining_mines)

    def test_remaining_mines_are_zero_when_flags_equal_mines(self):
        counter = FlagCounter(2)
        counter.place_flag()
        counter.place_flag()
        self.assertEqual(0, counter.remaining_mines)

    def test_remaining_mines_become_negative_when_flags_exceed_mines(self):
        counter = FlagCounter(2)
        for _ in range(3):
            counter.place_flag()
        self.assertEqual(-1, counter.remaining_mines)

    def test_removing_a_flag_when_none_is_placed_raises_value_error(self):
        counter = FlagCounter(10)
        with self.assertRaises(ValueError):
            counter.remove_flag()
