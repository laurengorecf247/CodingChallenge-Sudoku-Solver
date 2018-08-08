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
                        RuleOut(i, j);
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
                CheckForSets(i, CheckType.Row);
                CheckForSets(i, CheckType.Column);
                CheckForSets(i, CheckType.Box);
            }

            for (int i = 0; i < 9; i++)
            {
                CheckRowForOnlyOption(i);
                CheckColForOnlyOption(i);
                CheckBoxForOnlyOption(i);
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
                    Console.WriteLine((i + 1) + "," + (j + 1) + " has only one option remaining, setting value to " + grid[i][j].Fixed);
                }
                RuleOut(i, j);
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
                                Console.WriteLine("In row " + (row + 1) + ", " + option + " can only go in " + (row + 1) + "," + (i + 1));
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
                                Console.WriteLine("In column " + (col + 1) + ", " + option + " can only go in " + (i + 1) + "," + (col + 1));
                            }
                        }
                    }
                }
            }
        }

        static void CheckBoxForOnlyOption(int box)
        {
            int boxRow = box % 3;
            int boxCol = (int)Math.Floor(box / 3.0);

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
                                Console.WriteLine("In box " + (boxRow + 1) + "-" + (boxCol + 1) + ", " + option + " can only go in " + (boxRow * 3 + i % 3 + 1) + "," + (boxCol * 3 + (int)Math.Floor(i / 3.0) + 1));
                            }
                        }
                    }
                }
            }
        }

        static void CheckForSets(int checkNumber, CheckType checkType)
        {
            var setsChecked = new List<List<int>>();

            for (int i = 0; i < 9; i++)
            {
                var basisCell = new Cell();
                switch (checkType)
                {
                    case CheckType.Row:
                        basisCell = grid[checkNumber][i];
                        break;
                    case CheckType.Column:
                        basisCell = grid[i][checkNumber];
                        break;
                    case CheckType.Box:
                        int boxRow = checkNumber % 3;
                        int boxCol = (int)Math.Floor(checkNumber / 3.0);

                        basisCell = grid[(checkNumber % 3) * 3 + i % 3][((int)Math.Floor(checkNumber / 3.0) * 3) + (int)Math.Floor(i / 3.0)];
                        break;
                }

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
                    var checkCell = new Cell();
                    switch (checkType)
                    {
                        case CheckType.Row:
                            checkCell = grid[checkNumber][j];
                            break;
                        case CheckType.Column:
                            checkCell = grid[j][checkNumber];
                            break;
                        case CheckType.Box:
                            checkCell = grid[(checkNumber % 3) * 3 + j % 3][((int)Math.Floor(checkNumber / 3.0) * 3) + (int)Math.Floor(j / 3.0)];
                            break;
                    }

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
                            var removeCell = new Cell();
                            switch (checkType)
                            {
                                case CheckType.Row:
                                    removeCell = grid[checkNumber][k];
                                    break;
                                case CheckType.Column:
                                    removeCell = grid[k][checkNumber];
                                    break;
                                case CheckType.Box:
                                    removeCell = grid[(checkNumber % 3) * 3 + k % 3][((int)Math.Floor(checkNumber / 3.0) * 3) + (int)Math.Floor(k / 3.0)];
                                    break;
                            }
                            foreach (var digit in set)
                            {
                                removeCell.MightBe.Remove(digit);
                                removed = true;
                            }
                        }
                    }
                }
                if (removed && chatty)
                {
                    switch (checkType)
                    {
                        case CheckType.Row:
                            Console.WriteLine("In row " + (checkNumber + 1) + ", the set " + string.Join(",", set) + " appears " + set.Count + " times");
                            break;
                        case CheckType.Column:
                            Console.WriteLine("In column " + (checkNumber + 1) + ", the set " + string.Join(",", set) + " appears " + set.Count + " times");
                            break;
                        case CheckType.Box:
                            Console.WriteLine("In box " + (checkNumber % 3 + 1) + "-" + ((int)Math.Floor(checkNumber / 3.0) + 1) + ", the set " + string.Join(",", set) + " appears " + set.Count + " times");
                            break;
                    }
                }
            }
        }

        static void RuleOut(int row, int column)
        {
            var ruleOut = (int)grid[row][column].Fixed;

            for (int i = 0; i < 9; i++)
            {
                if (i != column)
                {
                    grid[row][i].MightBe.Remove(ruleOut);
                    if (grid[row][i].MightBe.Count == 0)
                    {
                        throw new Exception("COL: Ruled out all options for " + (row + 1) + "," + (i + 1));
                    }
                }

                if (i != row)
                {
                    grid[i][column].MightBe.Remove(ruleOut);
                    if (grid[i][column].MightBe.Count == 0)
                    {
                        throw new Exception("ROW: Ruled out all options for " + (i + 1) + "," + (column + 1));
                    }
                }

                var usingRow = (3 * (int)Math.Floor(row / 3.0)) + i % 3;
                var usingCol = (3 * (int)Math.Floor(column / 3.0)) + (int)Math.Floor(i / 3.0);
                if (usingRow != row || usingCol != column)
                {
                    grid[usingRow][usingCol].MightBe.Remove(ruleOut);
                    if (grid[usingRow][usingCol].MightBe.Count == 0)
                    {
                        throw new Exception("BOX: Ruled out all options for " + (usingRow + 1) + "," + (usingCol + 1));
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

    enum CheckType
    {
        Row,
        Column,
        Box
    }
}
