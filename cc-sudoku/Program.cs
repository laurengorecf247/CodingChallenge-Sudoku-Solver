using System;
using System.Collections.Generic;
using System.IO;

namespace cc_sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            bool chatty = false;

            string[] puzzle = File.ReadAllLines(@"C:\testing\sudoku1.csv");
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
            foreach (Cell[] line in grid)
            {
                foreach (Cell digit in line)
                {
                    Console.Write((digit.Fixed > 0 ? digit.Fixed.ToString() : " ") + "|" );
                }
                Console.WriteLine();
            }
            Console.ReadLine();

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

            bool changed = true;
            int iterated = 1;
            while (changed == true)
            {
                Console.WriteLine("Iteration: " + iterated);
                changed = IterateThroughGrid(grid, chatty);
                iterated++;
            }

            Console.WriteLine("Solution = ");
            foreach (Cell[] line in grid)
            {
                foreach (Cell digit in line)
                {
                    Console.Write((digit.Fixed > 0 ? digit.Fixed.ToString() : " ") + "|");
                }
                Console.WriteLine();
            }
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].MightBe.Count > 1)
                    {
                        {
                            Console.WriteLine("Stuck on " + (i+1) + "," + (j+1) + " - could be " + string.Join(",", grid[i][j].MightBe));
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        static bool IterateThroughGrid(Cell[][] grid, bool chatty)
        {
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
                            Console.WriteLine("Setting " + i + "," + j + " to " + grid[i][j].Fixed);
                        }
                        RuleOutInRow(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        RuleOutInColumn(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        RuleOutInBox(i, j, (int)grid[i][j].Fixed, grid, chatty);
                        changed = true;
                    }
                }
            }
            return changed;
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
                        Console.WriteLine(row + "," + i + " can't be " + ruleOut);
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
                        Console.WriteLine(i + "," + column + " can't be " + ruleOut);
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
                            Console.WriteLine(usingRow + "," + usingCol + " can't be " + ruleOut);
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
