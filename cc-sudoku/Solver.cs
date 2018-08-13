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

            var removed = false;

            if (RuleOutAll())
            {
                removed = true;
            }
            if (CheckForSubsets())
            {
                removed = true;
            }
            if (CheckForIntersects())
            {
                removed = true;
            }
            if (CheckForLocatedDigits())
            {
                removed = true;
            }
            if (CheckForNearlyLocatedDigits())
            {
                removed = true;
            }

            if (removed)
            {
                if (chatty)
                {
                    Writer.WriteGrid(grid);
                }

                IterateThroughGrid();
            }
        }

        private bool RuleOutAll()
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
                if (CheckForLocatedDigits(i, CheckType.Row))
                {
                    removed = true;
                }
                if (CheckForLocatedDigits(i, CheckType.Column))
                {
                    removed = true;
                }
                if (CheckForLocatedDigits(i, CheckType.Box))
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
                                Console.WriteLine("In " + checkType.ToString() + " " + (checkNumber + 1) + ", " + digit + " can only go in " + setCell.Row + "," + setCell.Column);
                            }
                            break;
                        }
                    }
                }
            }
            return removed;
        }

        private bool CheckForNearlyLocatedDigits()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (CheckForNearlyLocatedDigits(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        private bool CheckForNearlyLocatedDigits(int checkNumber, CheckType checkType)
        {
            var removed = false;
            var digits = new List<List<int>>();
            for (int i = 0; i < 9; i++)
            {
                digits.Add(new List<int>());
            }
            for (int i = 0; i < 9; i++)
            {
                var checkCell = Utility.GetCell(checkNumber, i, checkType, grid);

                foreach (var digit in checkCell.MightBe)
                {
                    digits[digit - 1].Add(i);
                }
            }
            for (var i = 0; i < 9; i++)
            {
                var digit = digits[i];
                var digitName = i + 1;
                if (digit.Count > 1 && digit.Count <= 3)
                {
                    var firstLocation = digit[0];
                    var firstRow = (int)Math.Floor(firstLocation / 3.0);
                    var globalRow = 3 * (int)Math.Floor(checkNumber / 3.0) + firstRow;
                    var firstCol = firstLocation % 3;
                    var globalCol = 3 * (checkNumber % 3) + firstCol;
                    var rowLocated = true;
                    var colLocated = true;
                    foreach (var location in digit)
                    {
                        var row = (int)Math.Floor(location / 3.0);
                        if (row != firstRow)
                        {
                            rowLocated = false;
                        }
                        var col = location % 3;
                        if (col != firstCol)
                        {
                            colLocated = false;
                        }
                    }
                    if (rowLocated)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            var removeCell = Utility.GetCell(globalRow, j, CheckType.Row, grid);
                            var boxNum = Utility.GetBoxNum(removeCell);
                            if (boxNum != checkNumber && removeCell.MightBe.Contains(digitName) && removeCell.MightBe.Count > 1) {
                                removeCell.MightBe.Remove(digitName);
                                removed = true;
                            }
                        }
                        if (chatty && removed)
                        {
                            Console.WriteLine("In Box " + (checkNumber + 1) + ", " + digitName + " must be in Row " + (globalRow + 1));
                        }
                    }
                    if (colLocated)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            var removeCell = Utility.GetCell(globalCol, j, CheckType.Column, grid);
                            var boxNum = Utility.GetBoxNum(removeCell);
                            if (boxNum != checkNumber && removeCell.MightBe.Contains(digitName) && removeCell.MightBe.Count > 1) {
                                removeCell.MightBe.Remove(digitName);
                                removed = true;
                            }
                        }
                        if (chatty && removed)
                        {
                            Console.WriteLine("In Box " + (checkNumber + 1) + ", " + digitName + " must be in Column " + (globalCol + 1));
                        }
                    }
                }
            }
            return removed;
        }

        private bool CheckForSubsets()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (CheckForSubsets(i, CheckType.Row))
                {
                    removed = true;
                }
                if (CheckForSubsets(i, CheckType.Column))
                {
                    removed = true;
                }
                if (CheckForSubsets(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        private bool CheckForSubsets(int checkNumber, CheckType checkType)
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

                var chattyRemoved = false;
                for (int j = 0; j < 9; j++)
                {
                    var checkCell = Utility.GetCell(checkNumber, j, checkType, grid);

                    if (j != i && basisSet.Intersect(checkCell.MightBe).Count() == checkCell.MightBe.Count)
                    {
                        subsets.Add(j);
                    }
                }

                if (subsets.Count == basisSet.Count - 1)
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
                                        throw new Exception(checkType.ToString() + " " + (checkNumber + 1) + ": Subset check ruled out all options for " + removeCell.Row + "," + removeCell.Column);
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

            }
            return removed;
        }

        private bool CheckForIntersects()
        {
            var removed = false;
            for (int i = 0; i < 9; i++)
            {
                if (CheckForIntersects(i, CheckType.Row))
                {
                    removed = true;
                }
                if (CheckForIntersects(i, CheckType.Column))
                {
                    removed = true;
                }
                if (CheckForIntersects(i, CheckType.Box))
                {
                    removed = true;
                }
            }
            return removed;
        }

        private bool CheckForIntersects(int checkNumber, CheckType checkType)
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
                
                var intersects = new List<int> { };

                var chattyRemoved = false;
                for (int j = 0; j < 9; j++)
                {
                    var checkCell = Utility.GetCell(checkNumber, j, checkType, grid);

                    if (j != i && basisSet.Intersect(checkCell.MightBe).Count() < checkCell.MightBe.Count && basisSet.Intersect(checkCell.MightBe).Count() > 0)
                    {
                        intersects.Add(j);
                    }
                }

                if (basisSet.Count == 2 && intersects.Count == 2 && checkType == CheckType.Box)
                {
                    var intersect1 = Utility.GetCell(checkNumber, intersects[0], CheckType.Box, grid).MightBe;
                    var intersect2 = Utility.GetCell(checkNumber, intersects[1], CheckType.Box, grid).MightBe;

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
                                            throw new Exception(checkType.ToString() + " " + (checkNumber + 1) + ": Intersect check ruled out all options for " + removeCell.Row + "," + removeCell.Column);
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

            var removed = false;
            if (RuleOutInType(row, CheckType.Row, digit))
            {
                removed = true;
            }
            if (RuleOutInType(column, CheckType.Column, digit))
            {
                removed = true;
            }
            if (RuleOutInType(boxNumber, CheckType.Box, digit))
            {
                removed = true;
            }

            return removed;
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
