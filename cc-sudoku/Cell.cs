using System.Collections.Generic;

namespace cc_sudoku
{
    class Cell
    {
        public List<int> MightBe { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }
    }
}
