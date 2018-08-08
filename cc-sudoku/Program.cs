using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cc_sudoku
{
    class Program
    {
        internal const bool chatty = false;
        internal static Cell[][] grid;

        static void Main(string[] args)
        {
            string[] puzzle = File.ReadAllLines(@"C:\testing\sudoku2.csv");
            grid = new Cell[9][];

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
            WriteGrid();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].Fixed > 0)
                    {
                        RuleOutInRow(i, j);
                        RuleOutInColumn(i, j);
                        RuleOutInBox(i, j);
                    }
                }
            }
            Console.WriteLine();

            IterateThroughGrid();

            Console.WriteLine();
            Console.WriteLine("Solution = ");
            WriteGrid();

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

        static void WriteGrid()
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

        static void IterateThroughGrid()
        {
            Console.WriteLine("Solving...");
            var changed = false;

            for (int i = 0; i < 9; i++)
            {
                CheckRowForSets(i);
                CheckColForSets(i);
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    CheckBoxForSets(i, j);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                CheckRowForOnlyOption(i);
                CheckColForOnlyOption(i);
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    CheckBoxForOnlyOption(i, j);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (CheckCellForNewFixedValue(i, j))
                    {
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                if (chatty)
                {
                    WriteGrid();
                }

                IterateThroughGrid();
            }
        }

        static bool CheckCellForNewFixedValue(int i, int j)
        {
            var changed = false;
            if (grid[i][j].MightBe.Count == 1 && !(grid[i][j].Fixed > 0))
            {
                grid[i][j].Fixed = grid[i][j].MightBe[0];
                if (chatty)
                {
                    Console.WriteLine("Setting " + (i + 1) + "," + (j + 1) + " to " + grid[i][j].Fixed);
                }
                RuleOutInRow(i, j);
                RuleOutInColumn(i, j);
                RuleOutInBox(i, j);
                changed = true;
            }
            return changed;
        }

        static void CheckRowForOnlyOption(int row)
        {
            var options = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 9; i++)
            {
                foreach (var option in grid[row][i].MightBe)
                {
                    options[(option - 1)]++;
                }
            }
            for (int option = 1; option < 10; option++)
            {
                if (options[(option - 1)] == 1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (grid[row][i].MightBe.IndexOf(option) != -1 && !(grid[row][i].Fixed > 0))
                        {
                            grid[row][i].MightBe = new List<int> { option };
                            if (chatty)
                            {
                                Console.WriteLine("In row " + row + ", " + option + " can only go in " + (row + 1) + "," + (i + 1));
                            }
                        }
                    }
                }
            }
        }

        static void CheckColForOnlyOption(int col)
        {
            var options = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 9; i++)
            {
                foreach (var option in grid[i][col].MightBe)
                {
                    options[(option - 1)]++;
                }
            }
            for (int option = 1; option < 10; option++)
            {
                if (options[(option - 1)] == 1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (grid[i][col].MightBe.IndexOf(option) != -1 && !(grid[i][col].Fixed > 0))
                        {
                            grid[i][col].MightBe = new List<int> { option };
                            if (chatty)
                            {
                                Console.WriteLine("In column " + col + ", " + option + " can only go in " + (i + 1) + "," + (col + 1));
                            }
                        }
                    }
                }
            }
        }

        static void CheckBoxForOnlyOption(int boxRow, int boxCol)
        {
            var options = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 9; i++)
            {
                foreach (var option in grid[boxRow * 3 + i % 3][boxCol * 3 + (int)Math.Floor(i / 3.0)].MightBe)
                {
                    options[(option - 1)]++;
                }
            }
            for (int option = 1; option < 10; option++)
            {
                if (options[(option - 1)] == 1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (grid[boxRow * 3 + i % 3][boxCol * 3 + (int)Math.Floor(i / 3.0)].MightBe.IndexOf(option) != -1 && !(grid[boxRow * 3 + i % 3][boxCol * 3 + (int)Math.Floor(i / 3.0)].Fixed > 0))
                        {
                            grid[boxRow * 3 + i % 3][boxCol * 3 + (int)Math.Floor(i / 3.0)].MightBe = new List<int> { option };
                            if (chatty)
                            {
                                Console.WriteLine("In box " + (boxRow + 1) + "," + (boxCol + 1) + ", " + option + " can only go in " + (boxRow * 3 + i % 3 + 1) + "," + (boxCol * 3 + (int)Math.Floor(i / 3.0) + 1));
                            }
                        }
                    }
                }
            }
        }

        static void CheckBoxForSets(int boxRow, int boxCol)
        {
            for (int i = 0; i < 9; i++)
            {
                var basisCell = grid[boxRow * 3 + i % 3][boxCol * 3 + (int)Math.Floor(i / 3.0)];
                var set = basisCell.MightBe;
                var setsFound = new List<int> { i };

                for (int j = 0; j < 9; j++)
                {
                    var checkCell = grid[boxRow * 3 + j % 3][boxCol * 3 + (int)Math.Floor(j / 3.0)];
                    if (j != i && Enumerable.SequenceEqual(set, checkCell.MightBe))
                    {
                        setsFound.Add(j);
                    }

                    if (setsFound.Count == set.Count && set.Count > 1)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (!setsFound.Contains(k))
                            {
                                var removeCell = grid[boxRow * 3 + k % 3][boxCol * 3 + (int)Math.Floor(k / 3.0)];
                                foreach (var digit in set)
                                {
                                    removeCell.MightBe.Remove(digit);
                                    if (chatty)
                                    {
                                        Console.WriteLine((boxRow * 3 + j % 3 + 1) + "," + (boxCol * 3 + (int)Math.Floor(j / 3.0) + 1) + " can't be " + digit + ": " + set.Count + "-subset in box");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckRowForSets(int row)
        {
            for (int i = 0; i < 9; i++)
            {
                var basisCell = grid[row][i];
                var set = basisCell.MightBe;
                var setsFound = new List<int> { i };

                for (int j = 0; j < 9; j++)
                {
                    var checkCell = grid[row][j];
                    if (j != i && Enumerable.SequenceEqual(set, checkCell.MightBe))
                    {
                        setsFound.Add(j);
                    }
                }

                if (setsFound.Count == set.Count && set.Count > 1)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (!setsFound.Contains(k))
                        {
                            var removeCell = grid[row][k];
                            foreach (var digit in set)
                            {
                                removeCell.MightBe.Remove(digit);
                                if (chatty)
                                {
                                    Console.WriteLine((row + 1) + "," + (k + 1) + " can't be " + digit + ": " + set.Count + "-subset in row");
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckColForSets(int col)
        {
            for (int i = 0; i < 9; i++)
            {
                var basisCell = grid[i][col];
                var set = grid[i][col].MightBe;
                var setsFound = new List<int> { i };

                for (int j = 0; j < 9; j++)
                {
                    var checkCell = grid[j][col];
                    if (j != i && Enumerable.SequenceEqual(set, checkCell.MightBe))
                    {
                        setsFound.Add(j);
                    }
                }

                if (setsFound.Count == set.Count && set.Count > 1)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (!setsFound.Contains(k))
                        {
                            var removeCell = grid[k][col];
                            foreach (var digit in set)
                            {
                                removeCell.MightBe.Remove(digit);
                                if (chatty)
                                {
                                    Console.WriteLine((k + 1) + "," + (col + 1) + " can't be " + digit + ": " + set.Count + "-subset in column");
                                }
                            }
                        }
                    }
                }
            }
        }

        static void RuleOutInColumn(int row, int column)
        {
            var ruleOut = (int)grid[row][column].Fixed;

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

        static void RuleOutInRow(int row, int column)
        {
            var ruleOut = (int)grid[row][column].Fixed;

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

        static void RuleOutInBox(int row, int column)
        {
            var ruleOut = (int)grid[row][column].Fixed;

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
