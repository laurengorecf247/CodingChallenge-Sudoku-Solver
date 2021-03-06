﻿using System;
using System.IO;

namespace cc_sudoku
{
    static class Program
    {
        internal const bool chatty = false;

        static void Main(string[] args)
        {
            string[] puzzle = File.ReadAllLines(@"C:\testing\sudokua1.csv");
            var grid = Utility.InitialiseGrid(puzzle);
            var solver = new Solver(grid, chatty);

            Console.WriteLine("Problem = ");
            Writer.WriteGrid(grid);
            Console.WriteLine();
            
            solver.IterateThroughGrid();

            Console.WriteLine();
            Console.WriteLine("Solution = ");
            Writer.WriteGrid(grid);
            Writer.WriteStuckCells(grid);

            Console.ReadLine();
        }
    }
}
