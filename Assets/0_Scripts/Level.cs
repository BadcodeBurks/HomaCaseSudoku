using System;
using System.Collections.Generic;

namespace Sudoku
{
    [Serializable]
    public class Level
    {
        public string name;
        public string levelContents;
        public int[] givenCellIndices;

        public uint[] GetLevelAsArray()
        {
            uint[] level = new uint[81];
            for (int i = 0; i < 81; i++)
            {
                level[i] = (uint)char.GetNumericValue(levelContents[i]);
            }

            return level;
        }
    }
}