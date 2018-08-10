using System;
using System.Collections.Generic;

namespace cc_sudoku
{
    class Utility
    {
        public static Cell[][] InitialiseGrid(string[] puzzle)
        {
            var grid = new Cell[9][];
            for (int i = 0; i < 9; i++)
            {
                grid[i] = new Cell[9];
                var digits = puzzle[i].Split(",");
                for (int j = 0; j < 9; j++)
                {
                    grid[i][j] = new Cell
                    {
                        X = i + 1,
                        Y = j + 1,
                        MightBe = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                    };
                    if (!string.IsNullOrWhiteSpace(digits[j]))
                    {
                        grid[i][j].MightBe = new List<int> { Int32.Parse(digits[j]) };
                    }
                }
            }
            return grid;
        }

        public static Cell GetCell(int checkNumber, int i, CheckType checkType, Cell[][] grid)
        {
            var cell = new Cell();
            switch (checkType)
            {
                case CheckType.Row:
                    cell = grid[checkNumber][i];
                    break;
                case CheckType.Column:
                    cell = grid[i][checkNumber];
                    break;
                case CheckType.Box:
                    cell = grid[3 * (int)Math.Floor(checkNumber / 3.0) + (int)Math.Floor(i / 3.0)][3 * (checkNumber % 3) + i % 3];
                    break;
            }
            return cell;
        }
    }
}
