import unittest

from cell_position import CellPosition


class CellPositionTests(unittest.TestCase):
    def test_holds_column_and_row(self):
        position = CellPosition(2, 3)
        self.assertEqual(position.column, 2)
        self.assertEqual(position.row, 3)

    def test_positions_with_same_column_and_row_are_equal(self):
        self.assertEqual(CellPosition(1, 4), CellPosition(1, 4))
        self.assertEqual(hash(CellPosition(1, 4)), hash(CellPosition(1, 4)))

    def test_positions_with_different_column_or_row_are_not_equal(self):
        self.assertNotEqual(CellPosition(1, 4), CellPosition(4, 1))
        self.assertNotEqual(CellPosition(1, 4), CellPosition(1, 5))

    def test_negative_column_or_row_is_rejected(self):
        with self.assertRaises(ValueError):
            CellPosition(-1, 0)
        with self.assertRaises(ValueError):
            CellPosition(0, -1)

    def test_non_integer_column_or_row_is_rejected(self):
        for column, row in [(1.5, 0), (0, "1"), (True, 0)]:
            with self.subTest(column=column, row=row):
                with self.assertRaises(TypeError):
                    CellPosition(column, row)

    def test_inner_cell_has_eight_neighbors(self):
        neighbors = CellPosition(1, 1).neighbors(3, 3)
        expected = {CellPosition(c, r) for c in range(3) for r in range(3)} - {CellPosition(1, 1)}
        self.assertEqual(set(neighbors), expected)
        self.assertEqual(len(neighbors), 8)

    def test_corner_cell_has_three_neighbors(self):
        neighbors = CellPosition(0, 0).neighbors(5, 5)
        self.assertEqual(set(neighbors), {CellPosition(1, 0), CellPosition(0, 1), CellPosition(1, 1)})

    def test_opposite_corner_cell_has_three_neighbors(self):
        neighbors = CellPosition(4, 2).neighbors(5, 3)
        self.assertEqual(set(neighbors), {CellPosition(3, 2), CellPosition(3, 1), CellPosition(4, 1)})

    def test_edge_cell_has_five_neighbors(self):
        self.assertEqual(len(CellPosition(2, 0).neighbors(5, 5)), 5)

    def test_neighbors_exclude_itself(self):
        self.assertNotIn(CellPosition(2, 2), CellPosition(2, 2).neighbors(5, 5))

    def test_single_cell_board_has_no_neighbors(self):
        self.assertEqual(CellPosition(0, 0).neighbors(1, 1), [])


if __name__ == "__main__":
    unittest.main()
