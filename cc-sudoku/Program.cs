using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cc_sudoku
{
    class Program
    {
        internal const bool chatty = true;
        internal static Cell[][] grid;

        static void Main(string[] args)
        {
            string[] puzzle = File.ReadAllLines(@"C:\testing\sudoku5.csv");
            grid = new Cell[9][];

            InitialiseGrid(puzzle);

            Console.WriteLine("Problem = ");
            WriteGrid();
            Console.WriteLine();

            RuleOutAll();

            IterateThroughGrid();

            Console.WriteLine();
            Console.WriteLine("Solution = ");
            WriteGrid();
            WriteStuckCells();

            Console.ReadLine();
        }

        static void InitialiseGrid(string[] puzzle)
        {
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
        }

        static bool RuleOutAll()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].MightBe.Count == 1)
                    {
                        if (RuleOut(i, j))
                        {
                            removed = true;
                        }
                    }
                }
            }
            return removed;
        }

        static void WriteGrid()
        {
            foreach (Cell[] line in grid)
            {
                foreach (Cell digit in line)
                {
                    Console.Write((digit.MightBe.Count == 1 ? digit.MightBe.First().ToString() : " ") + "|");
                }
                Console.WriteLine();
            }
        }

        static void WriteStuckCells()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var stuckCell = grid[i][j];
                    if (stuckCell.MightBe.Count > 1)
                    {
                        {
                            Console.WriteLine("Stuck on " + stuckCell.X + "," + stuckCell.Y + " - could be " + string.Join(",", stuckCell.MightBe));
                        }
                    }
                }
            }
        }

        static void IterateThroughGrid()
        {
            Console.WriteLine("Solving...");

            if (CheckForSets() || RuleOutAll())
            {
                if (chatty)
                {
                    WriteGrid();
                }

                IterateThroughGrid();
            }
        }

        static bool CheckForSets()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (CheckForSets(i, CheckType.Row) || CheckForSets(i, CheckType.Column) || CheckForSets(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        static bool CheckForSets(int checkNumber, CheckType checkType)
        {
            var setsChecked = new List<List<int>>();
            var removed = false;

            for (int i = 0; i < 9; i++)
            {
                var basisCell = GetCell(checkNumber, i, checkType);

                var set = basisCell.MightBe;

                if (set.Count == 1)
                {
                    continue;
                }

                setsChecked.Add(set);

                var setsFound = new List<int> { i };

                var chattyRemoved = false;
                for (int j = 0; j < 9; j++)
                {
                    var checkCell = GetCell(checkNumber, j, checkType);

                    if (j != i && set.Intersect(checkCell.MightBe).Count() == checkCell.MightBe.Count)
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
                            var removeCell = GetCell(checkNumber, k, checkType);

                            foreach (var digit in set)
                            {
                                if (removeCell.MightBe.Contains(digit))
                                {
                                    removeCell.MightBe.Remove(digit);
                                    if (removeCell.MightBe.Count == 0)
                                    {
                                        throw new Exception(checkType.ToString() + " " + (checkNumber + 1) + ": Set check ruled out all options for " + removeCell.X + "," + removeCell.Y);
                                    }
                                    chattyRemoved = true;
                                    removed = true;
                                }
                            }
                        }
                    }
                }
                if (chattyRemoved && chatty)
                {
                    Console.WriteLine("In " + checkType.ToString() + " " + (checkNumber + 1) + ", the set " + string.Join(",", set) + " appears " + set.Count + " times");
                }
            }
            return removed;
        }

        static bool RuleOut(int row, int column)
        {
            return RuleOutInType(row, CheckType.Row, grid[row][column].MightBe.First()) || RuleOutInType(column, CheckType.Column, grid[row][column].MightBe.First()) || RuleOutInType(3 * (int)Math.Floor(row / 3.0) + (int)Math.Floor(column / 3.0), CheckType.Box, grid[row][column].MightBe.First());
        }

        static bool RuleOutInType(int checkNum, CheckType checkType, int ruleOut)
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                var removeCell = GetCell(checkNum, i, checkType);

                if (removeCell.MightBe.Count > 1 && removeCell.MightBe.Contains(ruleOut))
                {
                    removeCell.MightBe.Remove(ruleOut);
                    removed = true;
                }
            }
            return removed;
        }

        static Cell GetCell(int checkNumber, int i, CheckType checkType)
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

    class Cell
    {
        public List<int> MightBe { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }

    enum CheckType
    {
        Row,
        Column,
        Box
    }
}
