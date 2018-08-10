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
            var removed = false;

            for (int i = 0; i < 9; i++)
            {
                var basisCell = Utility.GetCell(checkNumber, i, checkType, grid);

                var basisSet = basisCell.MightBe;

                if (basisSet.Count == 1)
                {
                    continue;
                }

                var subsets = new List<int> { };
                var intersects = new List<int> { };

                var chattyRemoved = false;
                for (int j = 0; j < 9; j++)
                {
                    var checkCell = Utility.GetCell(checkNumber, j, checkType, grid);

                    if (j != i && basisSet.Intersect(checkCell.MightBe).Count() == checkCell.MightBe.Count)
                    {
                        subsets.Add(j);
                    }

                    else if (j != i && basisSet.Intersect(checkCell.MightBe).Count() > 0)
                    {
                        intersects.Add(j);
                    }
                }

                if (subsets.Count == basisSet.Count + 1)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (k != i && !subsets.Contains(k))
                        {
                            var removeCell = Utility.GetCell(checkNumber, k, checkType, grid);

                            foreach (var digit in basisSet)
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
                    if (chattyRemoved && chatty)
                    {
                        Console.WriteLine("In " + checkType.ToString() + " " + (checkNumber + 1) + ", the set " + string.Join(",", basisSet) + " appears " + basisSet.Count + " times");
                    }
                }

                if (basisSet.Count == 2 && intersects.Count == 2 && checkType == CheckType.Box)
                {
                    var intersect1 = Utility.GetCell(checkNumber, intersects[0], CheckType.Box, grid).MightBe;
                    var intersect2 = Utility.GetCell(checkNumber, intersects[0], CheckType.Box, grid).MightBe;

                    var union = Enumerable.Union(basisSet, Enumerable.Union(intersect1, intersect2));
                    if (union.Count() == intersects.Count + 1)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (k != i && !intersects.Contains(k))
                            {
                                var removeCell = Utility.GetCell(checkNumber, k, checkType, grid);

                                foreach (var digit in union)
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
                        Console.WriteLine("In " + checkType.ToString() + " " + (checkNumber + 1) + ", the set " + string.Join(",", union) + " is covered by 3 cells");
                    }
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
