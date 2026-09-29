import unittest

from board_size import BoardSize


class BoardSizeTests(unittest.TestCase):
    def test_holds_columns_rows_and_mines(self):
        size = BoardSize(9, 8, 10)
        self.assertEqual((size.columns, size.rows, size.mines), (9, 8, 10))

    def test_cell_count_is_columns_times_rows(self):
        self.assertEqual(BoardSize(9, 8, 10).cell_count, 72)

    def test_columns_and_rows_at_the_limits_are_accepted(self):
        BoardSize(5, 5, 1)
        BoardSize(30, 30, 1)

    def test_columns_outside_5_to_30_are_rejected(self):
        for columns in [4, 31]:
            with self.subTest(columns=columns):
                with self.assertRaises(ValueError):
                    BoardSize(columns, 10, 1)

    def test_rows_outside_5_to_30_are_rejected(self):
        for rows in [4, 31]:
            with self.subTest(rows=rows):
                with self.assertRaises(ValueError):
                    BoardSize(10, rows, 1)

    def test_mines_from_one_to_cells_minus_one_are_accepted(self):
        self.assertEqual(BoardSize(5, 5, 1).mines, 1)
        self.assertEqual(BoardSize(5, 5, 24).mines, 24)

    def test_zero_mines_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(5, 5, 0)

    def test_mines_equal_to_cell_count_are_rejected(self):
        with self.assertRaises(ValueError):
            BoardSize(5, 5, 25)

    def test_non_integer_values_are_rejected(self):
        for args in [(9.0, 9, 10), (9, "9", 10), (9, 9, True)]:
            with self.subTest(args=args):
                with self.assertRaises(TypeError):
                    BoardSize(*args)

    def test_sizes_with_same_values_are_equal(self):
        self.assertEqual(BoardSize(9, 9, 10), BoardSize(9, 9, 10))


if __name__ == "__main__":
    unittest.main()
