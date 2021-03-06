﻿using System;
using System.Collections.Generic;

namespace cc_sudoku
{
    static class Utility
    {
        public static Cell[][] InitialiseGrid(string[] puzzle)
        {
            var grid = new Cell[9][];
            for (int i = 0; i < 9; i++)
            {
                grid[i] = new Cell[9];
                var digits = puzzle[i].Split(',');
                for (int j = 0; j < 9; j++)
                {
                    grid[i][j] = new Cell
                    {
                        Row = i + 1,
                        Column = j + 1,
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

        public static bool CheckAllBox(Func<int, CheckType, bool> checkFunction)
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (checkFunction(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        public static bool CheckAllRowColumnBox(Func<int, CheckType, bool> checkFunction)
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (checkFunction(i, CheckType.Row))
                {
                    removed = true;
                }
                if (checkFunction(i, CheckType.Column))
                {
                    removed = true;
                }
                if (checkFunction(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        public static int GetBoxNum(Cell cell)
        {
            return 3 * (int)Math.Floor((cell.Row - 1) / 3.0) + (int)Math.Floor((cell.Column - 1.0) / 3.0);
        }

        public static Cell GetCell(int objectToCheck, int cellNumberWithinObject, CheckType checkType, Cell[][] grid)
        {
            var row = 0;
            var column = 0;

            switch (checkType)
            {
                case CheckType.Row:
                    row = objectToCheck;
                    column = cellNumberWithinObject;
                    break;
                case CheckType.Column:
                    column = objectToCheck;
                    row = cellNumberWithinObject;
                    break;
                case CheckType.Box:
                    row = 3 * (int)Math.Floor(objectToCheck / 3.0) + (int)Math.Floor(cellNumberWithinObject / 3.0);
                    column = 3 * (objectToCheck % 3) + cellNumberWithinObject % 3;
                    break;
            }
            return grid[row][column];
        }
    }
}
