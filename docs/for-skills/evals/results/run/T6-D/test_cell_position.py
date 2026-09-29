import unittest

from cell_position import CellPosition


class CellPositionTests(unittest.TestCase):
    def test_holds_column_and_row(self):
        position = CellPosition(2, 3)
        self.assertEqual(2, position.column)
        self.assertEqual(3, position.row)

    def test_positions_with_same_column_and_row_are_equal(self):
        self.assertEqual(CellPosition(2, 3), CellPosition(2, 3))

    def test_positions_with_different_column_are_not_equal(self):
        self.assertNotEqual(CellPosition(2, 3), CellPosition(1, 3))

    def test_positions_with_different_row_are_not_equal(self):
        self.assertNotEqual(CellPosition(2, 3), CellPosition(2, 4))

    def test_equal_positions_can_be_used_as_the_same_key(self):
        self.assertEqual(1, len({CellPosition(2, 3), CellPosition(2, 3)}))

    def test_negative_column_raises_value_error(self):
        with self.assertRaises(ValueError):
            CellPosition(-1, 0)

    def test_negative_row_raises_value_error(self):
        with self.assertRaises(ValueError):
            CellPosition(0, -1)

    def test_column_and_row_zero_are_allowed(self):
        self.assertEqual(CellPosition(0, 0), CellPosition(0, 0))


class CellPositionNeighborsTests(unittest.TestCase):
    def test_inner_cell_has_eight_neighbors(self):
        neighbors = CellPosition(2, 2).neighbors(5, 5)
        expected = {CellPosition(c, r) for c in (1, 2, 3) for r in (1, 2, 3)} - {CellPosition(2, 2)}
        self.assertEqual(expected, set(neighbors))
        self.assertEqual(8, len(neighbors))

    def test_neighbors_do_not_include_the_cell_itself(self):
        self.assertNotIn(CellPosition(2, 2), CellPosition(2, 2).neighbors(5, 5))

    def test_top_left_corner_has_three_neighbors(self):
        self.assertEqual(
            {CellPosition(1, 0), CellPosition(0, 1), CellPosition(1, 1)},
            set(CellPosition(0, 0).neighbors(5, 5)))

    def test_bottom_right_corner_has_three_neighbors(self):
        self.assertEqual(
            {CellPosition(3, 4), CellPosition(4, 3), CellPosition(3, 3)},
            set(CellPosition(4, 4).neighbors(5, 5)))

    def test_top_edge_cell_has_five_neighbors(self):
        self.assertEqual(5, len(CellPosition(2, 0).neighbors(5, 5)))

    def test_right_edge_cell_on_non_square_board_has_five_neighbors(self):
        # 列の数と行の数を取り違えると、この数が変わる
        self.assertEqual(5, len(CellPosition(7, 1).neighbors(8, 3)))

    def test_bottom_edge_cell_on_non_square_board_has_five_neighbors(self):
        self.assertEqual(5, len(CellPosition(1, 2).neighbors(8, 3)))
