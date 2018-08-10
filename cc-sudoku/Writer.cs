using System;
using System.Linq;

namespace cc_sudoku
{
    static class Writer
    {
        public static void WriteGrid(Cell[][] grid)
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

        public static void WriteStuckCells(Cell[][] grid)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var stuckCell = grid[i][j];
                    if (stuckCell.MightBe.Count > 1)
                    {
                        {
                            Console.WriteLine("Stuck on " + stuckCell.Row + "," + stuckCell.Column + " - could be " + string.Join(",", stuckCell.MightBe));
                        }
                    }
                }
            }
        }
    }
}
