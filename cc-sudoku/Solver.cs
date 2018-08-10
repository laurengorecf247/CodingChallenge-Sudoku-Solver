using System;
using System.Collections.Generic;
using System.Linq;

namespace cc_sudoku
{
    class Solver
    {
        private readonly Cell[][] grid;
        private readonly bool chatty;

        public Solver(Cell[][] grid, bool chatty)
        {
            this.grid = grid;
            this.chatty = chatty;
        }

        public void IterateThroughGrid()
        {
            Console.WriteLine("Solving...");

            if (CheckForLocatedDigits() ||
                CheckForSets() ||
                RuleOutAll())
            {
                if (chatty)
                {
                    Writer.WriteGrid(grid);
                }

                IterateThroughGrid();
            }
        }

        public bool RuleOutAll()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i][j].MightBe.Count == 1)
                    {
                        if (RuleOutCell(i, j))
                        {
                            removed = true;
                        }
                    }
                }
            }
            return removed;
        }

        private bool CheckForLocatedDigits()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (CheckForLocatedDigits(i, CheckType.Row) ||
                    CheckForLocatedDigits(i, CheckType.Column) ||
                    CheckForLocatedDigits(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        private bool CheckForLocatedDigits(int checkNumber, CheckType checkType)
        {
            var removed = false;
            var digits = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 9; i++)
            {
                var checkCell = Utility.GetCell(checkNumber, i, checkType, grid);

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
                        var setCell = Utility.GetCell(checkNumber, i, checkType, grid);

                        if (setCell.MightBe.IndexOf(digit) != -1 && setCell.MightBe.Count > 1)
                        {
                            setCell.MightBe = new List<int> { digit };
                            removed = true;
                            if (chatty)
                            {
                                Console.WriteLine("In " + checkType.ToString() + " " + (checkNumber + 1) + ", " + digit + " can only go in " + setCell.X + "," + setCell.Y);
                            }
                            break;
                        }
                    }
                }
            }
            return removed;
        }

        private bool CheckForSets()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (CheckForSets(i, CheckType.Row) ||
                    CheckForSets(i, CheckType.Column) ||
                    CheckForSets(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        private bool CheckForSets(int checkNumber, CheckType checkType)
        {
            var setsChecked = new List<List<int>>();
            var removed = false;

            for (int i = 0; i < 9; i++)
            {
                var basisCell = Utility.GetCell(checkNumber, i, checkType, grid);

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
                    var checkCell = Utility.GetCell(checkNumber, j, checkType, grid);

                    if (j != i && set.Intersect(checkCell.MightBe).Count() == checkCell.MightBe.Count)
                    {
                        setsFound.Add(j);
                    }
                }

                if (setsFound.Count == set.Count)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (!setsFound.Contains(k))
                        {
                            var removeCell = Utility.GetCell(checkNumber, k, checkType, grid);

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

        private bool RuleOutCell(int row, int column)
        {
            var digit = grid[row][column].MightBe.First();
            var boxNumber = 3 * (int)Math.Floor(row / 3.0) + (int)Math.Floor(column / 3.0);

            return RuleOutInType(row, CheckType.Row, digit) ||
                   RuleOutInType(column, CheckType.Column, digit) ||
                   RuleOutInType(boxNumber, CheckType.Box, digit);
        }

        private bool RuleOutInType(int checkNum, CheckType checkType, int ruleOut)
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                var removeCell = Utility.GetCell(checkNum, i, checkType, grid);

                if (removeCell.MightBe.Count > 1 && removeCell.MightBe.Contains(ruleOut))
                {
                    removeCell.MightBe.Remove(ruleOut);
                    removed = true;
                }
            }
            return removed;
        }
    }
}
