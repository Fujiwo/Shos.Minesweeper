import unittest

from board_size import BoardSize


class BoardSizeTests(unittest.TestCase):
    def test_holds_columns_rows_and_mines(self):
        size = BoardSize(9, 9, 10)
        self.assertEqual((size.columns, size.rows, size.mines), (9, 9, 10))

    def test_four_columns_is_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(4, 9, 10)

    def test_thirty_one_columns_is_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(31, 9, 10)

    def test_five_columns_is_accepted(self):
        self.assertEqual(BoardSize(5, 9, 10).columns, 5)

    def test_thirty_columns_is_accepted(self):
        self.assertEqual(BoardSize(30, 9, 10).columns, 30)

    def test_four_rows_is_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 4, 10)

    def test_thirty_one_rows_is_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 31, 10)

    def test_five_rows_is_accepted(self):
        self.assertEqual(BoardSize(9, 5, 10).rows, 5)

    def test_thirty_rows_is_accepted(self):
        self.assertEqual(BoardSize(9, 30, 10).rows, 30)

    def test_zero_mines_is_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(9, 9, 0)

    def test_one_mine_is_accepted(self):
        self.assertEqual(BoardSize(9, 9, 1).mines, 1)

    def test_mines_equal_to_cell_count_is_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(5, 6, 30)

    def test_mines_one_less_than_cell_count_is_accepted(self):
        self.assertEqual(BoardSize(5, 6, 29).mines, 29)
