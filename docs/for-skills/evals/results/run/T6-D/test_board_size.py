import unittest

from board_size import BoardSize


class BoardSizeTests(unittest.TestCase):
    def test_holds_columns_rows_and_mines(self):
        size = BoardSize(9, 9, 10)
        self.assertEqual(9, size.columns)
        self.assertEqual(9, size.rows)
        self.assertEqual(10, size.mines)

    def test_smallest_size_with_one_mine_is_allowed(self):
        self.assertEqual(BoardSize(5, 5, 1), BoardSize(5, 5, 1))

    def test_largest_size_is_allowed(self):
        self.assertEqual(30, BoardSize(30, 30, 99).columns)

    def test_columns_below_five_raise_value_error(self):
        with self.assertRaises(ValueError):
            BoardSize(4, 9, 10)

    def test_columns_above_thirty_raise_value_error(self):
        with self.assertRaises(ValueError):
            BoardSize(31, 9, 10)

    def test_rows_below_five_raise_value_error(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 4, 10)

    def test_rows_above_thirty_raise_value_error(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 31, 10)

    def test_zero_mines_raise_value_error(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 9, 0)

    def test_mines_one_less_than_cells_are_allowed(self):
        self.assertEqual(24, BoardSize(5, 5, 24).mines)

    def test_mines_equal_to_cells_raise_value_error(self):
        with self.assertRaises(ValueError):
            BoardSize(5, 5, 25)

    def test_mine_limit_uses_columns_times_rows(self):
        # 5×6 = 30 マスなので、29 までは置けて、30 は置けない
        self.assertEqual(29, BoardSize(5, 6, 29).mines)
        with self.assertRaises(ValueError):
            BoardSize(5, 6, 30)
