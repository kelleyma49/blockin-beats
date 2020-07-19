using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Logic
{
    [DataContract]
    public class Square : PlayfieldElement
    {
        /// <summary>
        /// # of cells in each dimension the square.
        /// </summary>
        public const int NumCellsPerAxis = 2;

        /// <summary>
        /// Delegate for finding a cell based on column/row info.
        /// </summary>
        /// <param name="column">The column of the cell</param>
        /// <param name="row">the row of the cell</param>
        /// <returns>the found cell, or null if the cell was outside the bounds</returns>
        public delegate Cell FindCellDel(int column, int row);

        /// <summary>
        /// Delegate for finding a square based on column/row info.
        /// </summary>
        /// <param name="column">The column of the cell</param>
        /// <param name="row">The row of the cell</param>
        /// <returns>The found square, or null if the square was outside the bounds</returns>
        public delegate Square FindSquareDel(int column, int row);

        public event EventHandler SquareCompleted;

        /// <summary>
        /// Constructor for completed squares.
        /// </summary>
        /// <param name="column">The column of the upper left cell.</param>
        /// <param name="row">The row of the upper left cell.</param>
        /// <param name="playfield">playfield that the squares are part of</param>
        public Square(int column, int row, Playfield playfield) : base(column, row)
        {
            Playfield = playfield;
        }

        public void ResetSubscriptions()
        {
            SquareCompleted = delegate { };
        }

        public enum States
        {
            /// <summary>
            /// No square exists.
            /// </summary>
            None,

            /// <summary>
            /// Square was created but not removed.
            /// </summary>
            Completing,

        }

        public enum ChainStates
        {
            Empty,
            Removeable,
            ChainPrevents,
            NotActivated
        }

        /// <summary>
        /// Checks to see if this square can be removed this frame.
        /// </summary>
        /// <returns>true if the square can be removed; false if not</returns>
        public ChainStates CanRemove(int prevCellColumn, int currCellColumn)
        {
            int nextColumn = Column + 1;
            if (State != States.Completing)
                return ChainStates.Empty;
            else if (nextColumn >= currCellColumn && prevCellColumn <= currCellColumn)
                return ChainStates.ChainPrevents;

            // check if other squares are dependent on this square; if they are, the square cannot 
            // be removed this frame:
            for (int i = -1; i <= 1; i++)
            {
                Square sq = Playfield.FindSquare(nextColumn, Row + i);
                if (sq?.CanRemove(prevCellColumn, currCellColumn) == ChainStates.ChainPrevents)
                    return ChainStates.ChainPrevents;
            }

            // check if square is connected to a jeweled cell:
            var checkState = new Playfield.CheckState(Playfield.NumCellColumns, Playfield.NumCellRows);
            if (Playfield.CheckConnected(UpperLeft, currCellColumn, checkState) && checkState.FoundJewel)
                return ChainStates.ChainPrevents;

            // check if squares are part of a jeweled combo - if so,
            // we can't remove the square yet:
            if (UpperLeft.RemoveState == Cell.RemoveStates.Removing
                && LowerLeft.RemoveState == Cell.RemoveStates.Removing
                && UpperRight.RemoveState == Cell.RemoveStates.Removing
                && LowerRight.RemoveState == Cell.RemoveStates.Removing)
            {
                return ChainStates.Removeable;
            }
            else
            {
                return ChainStates.NotActivated;
            }
        }

        /// <summary>
        /// Remove the square from the playing field.
        /// </summary>
        public void Remove()
        {
            // if we get here we can remove the square:
            SquareCompleted?.Invoke(this, EventArgs.Empty);

            UpperRight.RemoveCell();
            LowerRight.RemoveCell();
            UpperLeft.RemoveCell();
            LowerLeft.RemoveCell();
        }

        public void CellStateChanged(object sender, CellStateChangedEventArgs e)
        {
            Cell firstCell = null;
            bool completed = true;
            for (int c = 0; c < NumCellsPerAxis; c++)
            {
                for (int r = 0; r < NumCellsPerAxis; r++)
                {
                    Cell cell = Playfield.GetCell(c + Column, r + Row);
                    if (firstCell == null)
                        firstCell = cell;
                    if (!cell.IsOccupied
                        || !firstCell.IsSameColor(cell))
                    {
                        completed = false;
                        break;
                    }
                }
            }

            if (completed && State != States.Completing)
            {
                foreach (var c in new Cell[] { UpperLeft, UpperRight, LowerLeft, LowerRight })
                {
                    if (!c.IsRemove)
                        c.RemoveState = Cell.RemoveStates.WillBeRemoved;
                }
            }
            else if (!completed)
            {
                foreach (var c in new Cell[] { UpperLeft, UpperRight, LowerLeft, LowerRight })
                {
                    bool clearRemoveState = true;
                    foreach (var s in GetOtherSquares(c))
                    {
                        if (s?.State == States.Completing)
                        {
                            clearRemoveState = false;
                            break;
                        }
                    }
                    if (clearRemoveState && !c.IsJewelRemove)
                    {
                        c.RemoveState = Cell.RemoveStates.NotRemoved;
                    }
                }
            }

            if (_prevState != State)
            {
                _prevState = State;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler StateChanged;

        public States State => UpperLeft.IsRemove && LowerLeft.IsRemove && UpperRight.IsRemove && LowerRight.IsRemove ?
            States.Completing : States.None;

        public Cell UpperLeft => Playfield.GetCell(Column, Row);
        public Cell UpperRight => Playfield.GetCell(Column + 1, Row);
        public Cell LowerLeft => Playfield.GetCell(Column, Row + 1);
        public Cell LowerRight => Playfield.GetCell(Column + 1, Row + 1);

        public Playfield Playfield { private get; set; }

        public override bool Equals(object obj)
        {
            Square other = obj as Square;
            if (other != null)
            {
                return State == other.State &&
                       base.Equals(other);
            }
            else
                return false;
        }

        public override int GetHashCode() => State.GetHashCode() ^ base.GetHashCode();

        public Square[] GetOtherSquares(Cell c)
        {
            if (c == UpperLeft)
            {
                return new Square[3] {
                    Playfield.FindSquare(Column-1,Row), // c is UR
					Playfield.FindSquare(Column-1,Row-1), // c is LR
					Playfield.FindSquare(Column,Row-1)  // c is LL
				};
            }
            else if (c == UpperRight)
            {
                return new Square[3] {
                    Playfield.FindSquare(Column,Row-1), // c is LR
					Playfield.FindSquare(Column+1,Row-1),  // c is LL
					Playfield.FindSquare(Column+1,Row) // c is UL
				};
            }
            else if (c == LowerLeft)
            {
                return new Square[3] {
                    Playfield.FindSquare(Column,Row+1), // c is UL
					Playfield.FindSquare(Column-1,Row+1), // c is UR
					Playfield.FindSquare(Column-1,Row)  // c is LR
				};
            }
            else if (c == LowerRight)
            {
                return new Square[3] {
                    Playfield.FindSquare(Column+1,Row),  // c is LL
					Playfield.FindSquare(Column+1,Row+1), // c is UL
					Playfield.FindSquare(Column,Row+1) // c is UR
				};
            }

            return null;
        }

        private States _prevState;
    }
}
