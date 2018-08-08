using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cc_sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            bool chatty = false;

            string[] puzzle = File.ReadAllLines(@"C:\testing\sudoku2.csv");
            Cell[][] grid = new Cell[9][];
            for (int i = 0; i < 9; i++)
            {
                grid[i] = new Cell[9];
                var digits = puzzle[i].Split(",");
                for (int j = 0; j < 9; j++)
                {
                    grid[i][j] = new Cell
                    {
                        MightBe = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                    };
                    if (!string.IsNullOrWhiteSpace(digits[j]))
                    {
                        grid[i][j].Fixed = Int32.Parse(digits[j]);
                        grid[i][j].MightBe = new List<int> { (int)grid[i][j].Fixed };
                    }
                }
            }

            Console.WriteLine("Problem = ");
            WriteGrid(grid);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].Fixed > 0)
                    {
                        RuleOutInRow(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        RuleOutInColumn(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        RuleOutInBox(i, j, (int)grid[i][j].Fixed, grid, chatty);
                    }
                }
            }

            IterateThroughGrid(grid, chatty);

            Console.WriteLine();
            Console.WriteLine("Solution = ");
            WriteGrid(grid);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].MightBe.Count > 1)
                    {
                        {
                            Console.WriteLine("Stuck on " + (i + 1) + "," + (j + 1) + " - could be " + string.Join(",", grid[i][j].MightBe));
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        static void WriteGrid(Cell[][] grid)
        {
            foreach (Cell[] line in grid)
            {
                foreach (Cell digit in line)
                {
                    Console.Write((digit.Fixed > 0 ? digit.Fixed.ToString() : " ") + "|");
                }
                Console.WriteLine();
            }
        }

        static void IterateThroughGrid(Cell[][] grid, bool chatty)
        {
            Console.WriteLine("Solving...");
            var changed = false;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].MightBe.Count == 1 && !(grid[i][j].Fixed > 0))
                    {
                        grid[i][j].Fixed = grid[i][j].MightBe[0];
                        if (chatty)
                        {
                            Console.WriteLine("Setting " + (i + 1) + "," + (j + 1) + " to " + grid[i][j].Fixed);
                        }
                        RuleOutInRow(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        RuleOutInColumn(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        RuleOutInBox(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        changed = true;
                    }
                }
            }

            for (int i = 0; i < 9; i++)
            {
                CheckRowForTwoSets(i, grid, chatty);
            }
            for (int i = 0; i < 9; i++)
            {
                CheckColForTwoSets(i, grid, chatty);
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    CheckBoxForTwoSets(i, j, grid, chatty);
                }
            }

            if (changed)
            {
                if (chatty)
                {
                    WriteGrid(grid);
                }

                IterateThroughGrid(grid, chatty);
            }
        }

        static void CheckBoxForTwoSets(int boxRow, int boxCol, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var firstToCheck = grid[boxRow * 3 + i % 3][boxCol * 3 + (int)Math.Floor(i / 3.0)];
                    var secondToCheck = grid[boxRow * 3 + j % 3][boxCol * 3 + (int)Math.Floor(j / 3.0)];

                    if (firstToCheck.MightBe.Count == 2 && j != i && Enumerable.SequenceEqual(secondToCheck.MightBe, firstToCheck.MightBe))
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            var thirdToCheck = grid[boxRow * 3 + k % 3][boxCol * 3 + (int)Math.Floor(k / 3.0)];
                            if (k != i && k != j)
                            {
                                foreach (var digit in firstToCheck.MightBe)
                                {
                                    thirdToCheck.MightBe.Remove(digit);
                                    if (chatty)
                                    {
                                        Console.WriteLine((boxRow * 3 + k % 3 + 1) + "," + (boxCol * 3 + (int)Math.Floor(k / 3.0) + 1) + " can't be " + digit + ": 2-subset in box");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckRowForTwoSets(int row, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[row][i].MightBe.Count == 2 && j != i && Enumerable.SequenceEqual(grid[row][j].MightBe, grid[row][i].MightBe))
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (k != i && k != j)
                            {
                                foreach (var digit in grid[row][i].MightBe)
                                {
                                    grid[row][k].MightBe.Remove(digit);
                                    if (chatty)
                                    {
                                        Console.WriteLine((row + 1) + "," + (k + 1) + " can't be " + digit + ": 2-subset in row");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckColForTwoSets(int column, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][column].MightBe.Count == 2 && j != i && Enumerable.SequenceEqual(grid[j][column].MightBe, grid[i][column].MightBe))
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (k != i && k != j)
                            {
                                foreach (var digit in grid[i][column].MightBe)
                                {
                                    grid[k][column].MightBe.Remove(digit);
                                    if (chatty)
                                    {
                                        Console.WriteLine((k + 1) + "," + (column + 1) + " can't be " + digit + ": 2-subset in column");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void RuleOutInColumn(int row, int column, int ruleOut, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                if (i != column)
                {
                    grid[row][i].MightBe.Remove(ruleOut);
                    if (chatty)
                    {
                        Console.WriteLine((row + 1) + "," + (i + 1) + " can't be " + ruleOut + ": exists in column");
                    }
                    if (grid[row][i].MightBe.Count == 0)
                    {
                        throw new Exception("COL: Ruled out all options for " + row + "," + i);
                    }
                }
            }
        }

        static void RuleOutInRow(int row, int column, int ruleOut, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                if (i != row)
                {
                    grid[i][column].MightBe.Remove(ruleOut);
                    if (chatty)
                    {
                        Console.WriteLine((i + 1) + "," + (column + 1) + " can't be " + ruleOut + ": exists in row");
                    }
                    if (grid[i][column].MightBe.Count == 0)
                    {
                        throw new Exception("ROW: Ruled out all options for " + i + "," + column);
                    }
                }
            }
        }

        static void RuleOutInBox(int row, int column, int ruleOut, Cell[][] grid, bool chatty)
        {
            var boxRow = (int)Math.Floor(row / 3.0);
            var boxCol = (int)Math.Floor(column / 3.0);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var usingRow = 3 * boxRow + i;
                    var usingCol = 3 * boxCol + j;
                    if (usingRow != row || usingCol != column)
                    {
                        grid[usingRow][usingCol].MightBe.Remove(ruleOut);
                        if (chatty)
                        {
                            Console.WriteLine((usingRow + 1) + "," + (usingCol + 1) + " can't be " + ruleOut + ": exists in box");
                        }
                        if (grid[usingRow][usingCol].MightBe.Count == 0)
                        {
                            throw new Exception("BOX: Ruled out all options for " + usingRow + "," + usingCol);
                        }
                    }
                }
            }
        }
    }

    class Cell
    {
        public int? Fixed { get; set; }

        public List<int> MightBe { get; set; }
    }
}
