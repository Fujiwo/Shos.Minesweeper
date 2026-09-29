import unittest

from cell_position import CellPosition


class CellPositionTests(unittest.TestCase):
    def test_holds_column_and_row(self):
        position = CellPosition(3, 5)
        self.assertEqual((position.column, position.row), (3, 5))

    def test_positions_with_same_column_and_row_are_equal(self):
        self.assertEqual(CellPosition(2, 4), CellPosition(2, 4))

    def test_positions_with_different_column_are_not_equal(self):
        self.assertNotEqual(CellPosition(2, 4), CellPosition(3, 4))

    def test_positions_with_different_row_are_not_equal(self):
        self.assertNotEqual(CellPosition(2, 4), CellPosition(2, 5))

    def test_equal_positions_are_the_same_key_in_a_set(self):
        self.assertEqual(len({CellPosition(1, 1), CellPosition(1, 1)}), 1)

    def test_negative_column_is_rejected(self):
        with self.assertRaises(ValueError):
            CellPosition(-1, 0)

    def test_negative_row_is_rejected(self):
        with self.assertRaises(ValueError):
            CellPosition(0, -1)

    def test_zero_column_and_row_is_accepted(self):
        self.assertEqual(CellPosition(0, 0).column, 0)

    def test_inner_cell_has_eight_neighbors(self):
        neighbors = CellPosition(2, 2).neighbors(5, 5)
        expected = {CellPosition(c, r) for c in (1, 2, 3) for r in (1, 2, 3)} - {CellPosition(2, 2)}
        self.assertEqual(set(neighbors), expected)
        self.assertEqual(len(neighbors), 8)

    def test_top_left_corner_has_three_neighbors(self):
        neighbors = CellPosition(0, 0).neighbors(5, 5)
        self.assertEqual(set(neighbors), {CellPosition(1, 0), CellPosition(0, 1), CellPosition(1, 1)})

    def test_bottom_right_corner_has_three_neighbors(self):
        neighbors = CellPosition(4, 3).neighbors(5, 4)
        self.assertEqual(set(neighbors), {CellPosition(3, 2), CellPosition(4, 2), CellPosition(3, 3)})

    def test_cell_on_top_edge_has_five_neighbors(self):
        self.assertEqual(len(CellPosition(2, 0).neighbors(5, 5)), 5)

    def test_cell_on_right_edge_has_five_neighbors(self):
        self.assertEqual(len(CellPosition(4, 2).neighbors(5, 5)), 5)

    def test_neighbors_do_not_repeat(self):
        neighbors = CellPosition(2, 2).neighbors(5, 5)
        self.assertEqual(len(neighbors), len(set(neighbors)))
