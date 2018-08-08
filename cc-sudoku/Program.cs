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

            DoInitialRuleOuts();

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
                        grid[i][j].Fixed = Int32.Parse(digits[j]);
                        grid[i][j].MightBe = new List<int> { (int)grid[i][j].Fixed };
                    }
                }
            }
        }

        static void DoInitialRuleOuts()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].Fixed > 0)
                    {
                        RuleOut(i, j);
                    }
                }
            }
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
            var changed = false;

            CheckForSets();

            CheckForLocatedDigits();

            changed = CheckForFixedCells();

            if (changed)
            {
                if (chatty)
                {
                    WriteGrid();
                }

                IterateThroughGrid();
            }
        }

        static bool CheckForFixedCells()
        {
            var changed = false;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (CheckForFixedCell(i, j))
                    {
                        changed = true;
                    }
                }
            }
            return changed;
        }

        static bool CheckForFixedCell(int i, int j)
        {
            var changed = false;
            var checkCell = grid[i][j];
            if (checkCell.MightBe.Count == 1 && !(checkCell.Fixed > 0))
            {
                checkCell.Fixed = checkCell.MightBe[0];
                if (chatty)
                {
                    Console.WriteLine(checkCell.X + "," + checkCell.Y + " is now " + checkCell.Fixed);
                }
                RuleOut(i, j);
                changed = true;
            }
            return changed;
        }

        static void CheckForLocatedDigits()
        {
            for (int i = 0; i < 9; i++)
            {
                CheckForLocatedDigits(i, CheckType.Row);
                CheckForLocatedDigits(i, CheckType.Column);
                CheckForLocatedDigits(i, CheckType.Box);
            }
        }

        static void CheckForLocatedDigits(int checkNumber, CheckType checkType)
        {
            var digits = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 9; i++)
            {
                var checkCell = GetCell(checkNumber, i, checkType);

                foreach (var digit in checkCell.MightBe)
                {
                    digits[(digit - 1)]++;
                }
            }
            for (int digit = 1; digit < 10; digit++)
            {
                if (digits[(digit - 1)] == 1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        var setCell = GetCell(checkNumber, i, checkType);

                        if (setCell.MightBe.IndexOf(digit) != -1 && !(setCell.Fixed > 0))
                        {
                            setCell.MightBe = new List<int> { digit };
                            if (chatty)
                            {
                                Console.WriteLine("In " + checkType.ToString() + " " + (checkNumber + 1) + ", " + digit + " can only go in " + setCell.X + "," + setCell.Y);
                            }
                        }
                    }
                }
            }
        }

        static void CheckForSets()
        {
            for (int i = 0; i < 9; i++)
            {
                CheckForSets(i, CheckType.Row);
                CheckForSets(i, CheckType.Column);
                CheckForSets(i, CheckType.Box);
            }
        }

        static void CheckForSets(int checkNumber, CheckType checkType)
        {
            var setsChecked = new List<List<int>>();

            for (int i = 0; i < 9; i++)
            {
                var basisCell = GetCell(checkNumber, i, checkType);

                var set = basisCell.MightBe;

                if (set.Count == 1)
                {
                    continue;
                }
                var redundant = false;
                foreach (var setChecked in setsChecked)
                {
                    if (Enumerable.SequenceEqual(set, setChecked))
                    {
                        redundant = true;
                        break;
                    }
                }
                if (redundant)
                {
                    continue;
                }
                setsChecked.Add(set);

                var setsFound = new List<int> { i };

                var removed = false;
                for (int j = 0; j < 9; j++)
                {
                    var checkCell = GetCell(checkNumber, j, checkType);

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
                            var removeCell = GetCell(checkNumber, k, checkType);

                            foreach (var digit in set)
                            {
                                removeCell.MightBe.Remove(digit);
                                if (removeCell.MightBe.Count == 0)
                                {
                                    throw new Exception(checkType.ToString() + " " + (checkNumber + 1) + ": Set check ruled out all options for " + removeCell.X + "," + removeCell.Y);
                                }
                                removed = true;
                            }
                        }
                    }
                }
                if (removed && chatty)
                {
                    Console.WriteLine("In " + checkType.ToString() + " " + (checkNumber + 1) + ", the set " + string.Join(",", set) + " appears " + set.Count + " times");
                }
            }
        }

        static void RuleOut(int row, int column)
        {
            RuleOutInType(row, column, CheckType.Row, (int)grid[row][column].Fixed);
            RuleOutInType(row, column, CheckType.Column, (int)grid[row][column].Fixed);
            RuleOutInType(row, column, CheckType.Box, (int)grid[row][column].Fixed);
        }

        static void RuleOutInType(int row, int column, CheckType checkType, int ruleOut)
        {
            for (int i = 0; i < 9; i++)
            {
                var removeCell = new Cell();
                switch (checkType)
                {
                    case CheckType.Row:
                        removeCell = grid[row][i];
                        break;
                    case CheckType.Column:
                        removeCell = grid[i][column];
                        break;
                    case CheckType.Box:
                        var usingRow = (3 * (int)Math.Floor(row / 3.0)) + i % 3;
                        var usingCol = (3 * (int)Math.Floor(column / 3.0)) + (int)Math.Floor(i / 3.0);
                        removeCell = grid[usingRow][usingCol];
                        break;
                }
                if (!(removeCell.Fixed > 0))
                {
                    removeCell.MightBe.Remove(ruleOut);
                }
                if (removeCell.MightBe.Count == 0)
                {
                    throw new Exception(checkType.ToString() + ": Ruled out all options for " + removeCell.X + "," + removeCell.Y);
                }
            }
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
                    cell = grid[(checkNumber % 3) * 3 + i % 3][((int)Math.Floor(checkNumber / 3.0) * 3) + (int)Math.Floor(i / 3.0)];
                    break;
            }
            return cell;
        }
    }

    class Cell
    {
        public int? Fixed { get; set; }

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
