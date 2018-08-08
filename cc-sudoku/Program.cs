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
                CheckRowForThreeSets(i, grid, chatty);
                CheckColForThreeSets(i, grid, chatty);
                CheckRowForTwoSets(i, grid, chatty);
                CheckColForTwoSets(i, grid, chatty);
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    CheckBoxForTwoSets(i, j, grid, chatty);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                CheckRowForOnlyOption(i, grid, chatty);
                CheckColForOnlyOption(i, grid, chatty);
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    CheckBoxForOnlyOption(i, j, grid, chatty);
                }
            }

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

            var notSolvedYet = false;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (!(grid[i][j].Fixed > 0))
                    {
                        notSolvedYet = true;
                        break;
                    }
                }
                if (notSolvedYet)
                {
                    break;
                }
            }

            if (changed && notSolvedYet)
            {
                if (chatty)
                {
                    WriteGrid(grid);
                }

                IterateThroughGrid(grid, chatty);
            }
        }

        static void CheckRowForOnlyOption(int row, Cell[][] grid, bool chatty)
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
                                Console.WriteLine("In row "+row+", "+option+" can only go in "+(row+1)+","+(i+1));
                            }
                        }
                    }
                }
            }
        }

        static void CheckColForOnlyOption(int col, Cell[][] grid, bool chatty)
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

        static void CheckBoxForOnlyOption(int boxRow, int boxCol, Cell[][] grid, bool chatty)
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
                                Console.WriteLine("In box " + (boxRow+1) + "," + (boxCol+1) +", " + option + " can only go in " + (boxRow * 3 + i % 3 + 1) + "," + (boxCol * 3 + (int)Math.Floor(i / 3.0) + 1));
                            }
                        }
                    }
                }
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

        static void CheckBoxForThreeSets(int boxRow, int boxCol, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        var firstToCheck = grid[boxRow * 3 + i % 3][boxCol * 3 + (int)Math.Floor(i / 3.0)];
                        var secondToCheck = grid[boxRow * 3 + j % 3][boxCol * 3 + (int)Math.Floor(j / 3.0)];
                        var thirdToCheck = grid[boxRow * 3 + j % 3][boxCol * 3 + (int)Math.Floor(j / 3.0)];

                        if (firstToCheck.MightBe.Count == 2 && j != i && Enumerable.SequenceEqual(secondToCheck.MightBe, firstToCheck.MightBe))
                        {
                            for (int m = 0; m < 9; m++)
                            {
                                var fourthToCheck = grid[boxRow * 3 + m % 3][boxCol * 3 + (int)Math.Floor(m / 3.0)];
                                if (m != i && m != j && m != k)
                                {
                                    foreach (var digit in firstToCheck.MightBe)
                                    {
                                        fourthToCheck.MightBe.Remove(digit);
                                        if (chatty)
                                        {
                                            Console.WriteLine((boxRow * 3 + m % 3 + 1) + "," + (boxCol * 3 + (int)Math.Floor(m / 3.0) + 1) + " can't be " + digit + ": 3-subset in box");
                                        }
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

        static void CheckRowForThreeSets(int row, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (grid[row][i].MightBe.Count == 3 && j != i && k != i && j != k && Enumerable.SequenceEqual(grid[row][i].MightBe, grid[row][j].MightBe) && Enumerable.SequenceEqual(grid[row][i].MightBe, grid[row][k].MightBe))
                        {
                            for (int m = 0; m < 9; m++)
                            {
                                if (m != i && m != j && m != k)
                                {
                                    foreach (var digit in grid[row][i].MightBe)
                                    {
                                        grid[row][m].MightBe.Remove(digit);
                                        if (chatty)
                                        {
                                            Console.WriteLine((row + 1) + "," + (m + 1) + " can't be " + digit + ": 3-subset in row");
                                        }
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

        static void CheckColForThreeSets(int column, Cell[][] grid, bool chatty)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (grid[i][column].MightBe.Count == 3 && j != i && j != k && i != k && Enumerable.SequenceEqual(grid[j][column].MightBe, grid[i][column].MightBe) && Enumerable.SequenceEqual(grid[k][column].MightBe, grid[i][column].MightBe))
                        {
                            for (int m = 0; m < 9; m++)
                            {
                                if (m != i && m != j && m != k)
                                {
                                    foreach (var digit in grid[i][column].MightBe)
                                    {
                                        grid[m][column].MightBe.Remove(digit);
                                        if (chatty)
                                        {
                                            Console.WriteLine((m + 1) + "," + (column + 1) + " can't be " + digit + ": 3-subset in column");
                                        }
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
