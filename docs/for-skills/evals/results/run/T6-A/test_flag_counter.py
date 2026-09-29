import unittest

from flag_counter import FlagCounter


class FlagCounterTests(unittest.TestCase):
    def test_remaining_equals_mines_before_any_flag(self):
        self.assertEqual(FlagCounter(10).remaining, 10)

    def test_placing_a_flag_decreases_remaining(self):
        counter = FlagCounter(10)
        counter.place()
        counter.place()
        self.assertEqual(counter.remaining, 8)

    def test_removing_a_flag_increases_remaining(self):
        counter = FlagCounter(10)
        counter.place()
        counter.remove()
        self.assertEqual(counter.remaining, 10)

    def test_remaining_is_zero_when_flags_equal_mines(self):
        counter = FlagCounter(2)
        counter.place()
        counter.place()
        self.assertEqual(counter.remaining, 0)

    def test_remaining_becomes_negative_when_flags_exceed_mines(self):
        counter = FlagCounter(1)
        for _ in range(3):
            counter.place()
        self.assertEqual(counter.remaining, -2)

    def test_removing_without_flags_is_rejected(self):
        counter = FlagCounter(10)
        with self.assertRaises(ValueError):
            counter.remove()
        self.assertEqual(counter.remaining, 10)

    def test_mines_less_than_one_are_rejected(self):
        with self.assertRaises(ValueError):
            FlagCounter(0)

    def test_non_integer_mines_are_rejected(self):
        for mines in [1.0, "1", True]:
            with self.subTest(mines=mines):
                with self.assertRaises(TypeError):
                    FlagCounter(mines)


if __name__ == "__main__":
    unittest.main()
