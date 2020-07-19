using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Logic
{
    public class DropPieceSimple
    {
        public const int NumColumns = 2;
        public const int NumRows = 2;

        public DropPieceSimple()
        {
            for (var x = 0; x < NumColumns; x++)
            {
                Cells[x] = new Cell.States[NumRows];
            }
        }

        [DataMember]
        public Cell.States[][] Cells { get; } = new Cell.States[NumColumns][];

        public delegate Cell.States GetCellState(int column, int row, bool allowJewel);

        public void Reset(bool allowJewel, GetCellState del)
        {
            for (var x = 0; x < NumColumns; x++)
            {
                for (var y = 0; y < NumRows; y++)
                {
                    Cells[x][y] = del(x, y, allowJewel);
                }
            }
        }

        public bool Equals(DropPieceSimple other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            for (var i = 0; i < Cells.Length; i++)
            {
                var row = Cells[i];
                var rowOther = other.Cells[i];
                for (var j = 0; j < row.Length; j++)
                {
                    if (row[j] != rowOther[j])
                        return false;
                }
            }

            return true;
        }

        private static readonly Random _random = new Random();

        public static Cell.States RandomCell(int column, int row, bool allowJewel)
        {
            // check if we should do jewel:
            int rand = _random.Next(1, 40);
            //rand = 1;
            bool isJewel = allowJewel && rand == 1 && column == 0 && row == 0;
            bool isBlack = _random.Next(0, 2) == 0;
            if (isJewel)
            {
                int randJewel = _random.Next(0, 3);
                if (randJewel == 0)
                    return isBlack ? Cell.States.BlackJeweledBoth : Cell.States.WhiteJeweledBoth;
                else if (randJewel == 1)
                    return isBlack ? Cell.States.BlackJeweledHorz : Cell.States.WhiteJeweledHorz;
                else
                    return isBlack ? Cell.States.BlackJeweledVert : Cell.States.WhiteJeweledVert;
            }
            else
            {
                return isBlack ? Cell.States.Black : Cell.States.White;
            }
        }
    }
}