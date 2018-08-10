using System.Collections.Generic;

namespace cc_sudoku
{
    class Cell
    {
        public List<int> MightBe { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
