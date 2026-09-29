import unittest

from board_size import BoardSize


class BoardSizeTests(unittest.TestCase):
    def test_holds_columns_rows_and_mines(self):
        size = BoardSize(9, 9, 10)
        self.assertEqual((size.columns, size.rows, size.mines), (9, 9, 10))

    def test_columns_below_5_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(4, 9, 10)

    def test_columns_above_30_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(31, 9, 10)

    def test_rows_below_5_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 4, 10)

    def test_rows_above_30_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 31, 10)

    def test_smallest_board_is_accepted(self):
        self.assertEqual(BoardSize(5, 5, 1).columns, 5)

    def test_largest_board_is_accepted(self):
        self.assertEqual(BoardSize(30, 30, 1).rows, 30)

    def test_zero_mines_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 9, 0)

    def test_mines_equal_to_cell_count_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(5, 6, 30)

    def test_mines_one_less_than_cell_count_are_accepted(self):
        self.assertEqual(BoardSize(5, 6, 29).mines, 29)
