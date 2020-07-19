using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Logic
{
	[DataContract]
	public class Playfield : IEquatable<Playfield>
	{
		/// <summary>
		/// Constructor for Playfield. 
		/// Column 0 is the top of the playfield.
		/// Row 0 is the left of the playfield.
		/// </summary>
		/// <param name="numColumns">The number of cell columns.</param>
		/// <param name="numRows">The number of cell rows.</param>
		public Playfield(int numColumns, int numRows)
		{
            JeweledCells = new List<Cell>();

            // score:
			Stats = new Stats();

			const int min = 2;
			if (numColumns< 0 || numColumns < min || numRows < 0 || numRows < min)
				throw new ArgumentOutOfRangeException("Columns and rows (" + numColumns + "x" + numRows + ") must be greater than " + min);

			Cells = new Cell[numColumns][];
			for (int x = 0; x < NumCellColumns; x++)
			{
				Cells[x] = new Cell[numRows];
				for (int y = 0; y < NumCellRows; y++)
				{
					Cells[x][y] = new Cell(x,y);
				}
			}

			Squares = new Square[numColumns-1][];
			for (int x = 0; x < NumSquareColumns; x++)
			{
				Squares[x] = new Square[numRows-1];
				for (int y = 0; y < NumSquareRows; y++)
				{
					var s = new Square(x, y, this);
				    s.SquareCompleted += (sender, args) => Stats.AddCompletedSquare();
				    Squares[x][y] = s;
					for (int cellX = 0; cellX < Square.NumCellsPerAxis; cellX++)
					{
						for (int cellY = 0; cellY < Square.NumCellsPerAxis; cellY++)
						{
						    var cell = Cells[x + cellX][y + cellY];
							int x1 = x;
							int y1 = y;
							cell.StateChanged += (sender, args) =>
						        {
                                    Squares[x1][y1].CellStateChanged(sender,args);
                                    if (cell.IsJeweled)
                                    {
                                        JeweledCells.Add(cell);
                                    }
                                    else
                                    {
                                        JeweledCells.Remove(cell);
                                    }
                                };
						}
					}
				}
			}

         	// timeline:
			Timeline = new Timeline(numColumns);
			Timeline.NewColumn += NewColumn;
			Timeline.WrapColumn += (sender, args) => Stats.IncrementFrame();
		}

		/// <summary>
		/// Resets all event handlers.  Mainly used by testing code.
		/// </summary>
		public void ResetSubscriptions()
		{
			foreach (var c in GetEnumeratorCells())
			{
				c.ResetSubscriptions();
			}

			foreach (var s in GetEnumeratorSquares())
			{
				s.ResetSubscriptions();
			}
		}

		public enum Position
		{
			// horizontal
			Left,
			Right,

			// vertical
			Up,
			Down,

			// diagonal 
			UpLeft,
			UpRight,
			DownLeft,
			DownRight
		}

        /// <summary>
        /// Position of cell within square.
        /// </summary>
        public enum SquarePosition
        {
            UpperLeft,
            LowerLeft,
            UpperRight,
            LowerRight
        }

		public bool IsValid(int column, int row)
		{
			return column >= 0 && column < NumCellColumns &&
				   row >= 0 && row < NumCellRows;
		}

        public class CheckState
        {
            public CheckState(int columns, int rows, Predicate<Cell> pred = null)
            {
                Predicate = pred;
                VisitedRight = new bool[columns,rows];
            }

            public bool[,] VisitedRight { get; }
            public bool FoundJewel { get; set; }
            public Predicate<Cell> Predicate { get; set; }
            public bool WasVisitedRight(Cell c) { return VisitedRight[c.Column, c.Row]; }
            public void SetVisitedRight(Cell c) { VisitedRight[c.Column, c.Row] = true; }

            public bool Check(Cell c)
            {
                return Predicate?.Invoke(c) ?? false;
            }
        }

        public bool CheckConnected(Cell cell,int currColumn,CheckState condition)
        {
            if (cell.Column == currColumn)
            {
                return condition.Check(cell);
            }

            var start = cell;
            bool result = false;
            foreach (var dir in new Position[] { Position.Up, Position.Down } )
            {
                for (var dirCell = start; dirCell != null; dirCell = AdjacentCell(dirCell, dir))
                {
                    if (condition.WasVisitedRight(dirCell) || !condition.Check(dirCell))
                        break;

                    var right = AdjacentCell(dirCell, Position.Right);
                    if (right!=null && !condition.WasVisitedRight(right) && condition.Check(right) && CheckConnected(right, currColumn, condition))
                    {
                        result = true;
                    }

                    condition.SetVisitedRight(dirCell);
                }

                // move start cell to unchecked cell:
                start = AdjacentCell(start, Position.Down);
            }

            var left = AdjacentCell(cell, Position.Left);
            if (left != null && condition.Check(left))
            {
                return CheckConnected(left, currColumn, condition) || result;
            }
            else
            {
                return result;
            }
        }

		public Cell AdjacentCell(Cell cell, Position position)
		{
            if (cell == null)
                return null;

			int column = cell.Column, row = cell.Row;

			switch (position)
			{
				case Position.Down:
                    row++;
					break;

				case Position.DownLeft:
                    row++;
                    column--;
					break;

				case Position.DownRight:
                    row++;
                    column++;
					break;

				case Position.Up:
                    row--;
					break;

				case Position.UpLeft:
                    row--;
                    column--;
					break;

				case Position.UpRight:
                    row--;
                    column++;
					break;

				case Position.Right:
                    column++;
					break;

				case Position.Left:
                    column--;
					break;
			}

			return IsValid(column,row)?Cells[column][row]:null;
		}

        public Square GetContainingSquare(Cell cell, SquarePosition position)
        {
            int col = cell.Column, row = cell.Row;
            switch (position)
            {
                case SquarePosition.UpperLeft:
                    break;
                case SquarePosition.LowerLeft:
                    row--;
                    break;
                case SquarePosition.UpperRight:
                    col--;
                    break;
                case SquarePosition.LowerRight:
                    row--;
                    col--;
                    break;
                default:
                    break;
            }

            if (col >= NumSquareColumns)
                return null;
            else if (col < 0)
                return null;
            else if (row >= NumSquareRows)
                return null;
            else if (row < 0)
                return null;

            return Squares[col][row];
        }

		internal void ProcessJewel(bool removingSpecialJewelCells)
		{
            var processed = new bool[NumCellColumns, NumCellRows];
            foreach (var jc in JeweledCells)
            {
                if (jc.IsRemove || jc.IsJewelRemove)
		    	{
                    ProcessJewel(jc, jc, removingSpecialJewelCells, processed);
                }
            }
		}

		internal void ProcessJewel(Cell cell,Cell jeweled,bool removingSpecialJewelCells,bool[,] marker)
		{
			// prevent stack overflow
			if (marker[cell.Column, cell.Row])
				return;

			marker[cell.Column, cell.Row] = true;
			if (cell==jeweled || cell.IsSameColor(jeweled))
			{
				InProcessOfRemovingJewelCells = true;
				bool isJewelRemoving = cell.RemoveState == Cell.RemoveStates.JewelWillBeRemoved
                                           || cell.RemoveState == Cell.RemoveStates.JewelRemoving;

				if (!isJewelRemoving && !cell.IsRemove && !cell.IsJewelRemove)
				{
					cell.RemoveState = Cell.RemoveStates.JewelWillBeRemoved;
					cell.RemoveStateColumnAbs = Timeline.TotalColumnAbs;
				}

				if (Timeline.Column == cell.Column
                    && cell.RemoveState == Cell.RemoveStates.JewelWillBeRemoved && removingSpecialJewelCells)
				{
					cell.RemoveState = Cell.RemoveStates.JewelRemoving;
					cell.RemoveStateColumnAbs = Timeline.TotalColumnAbs;
				}

				Cell um, ml, mr, lm;
                if (jeweled.IsJeweledVert)
                {
                    if ((um = AdjacentCell(cell, Position.Up)) != null)
                        ProcessJewel(um, jeweled, removingSpecialJewelCells, marker);
                    if ((lm = AdjacentCell(cell, Position.Down)) != null)
                        ProcessJewel(lm, jeweled, removingSpecialJewelCells, marker);
                }
                if (jeweled.IsJeweledHorz)
                {
                    if ((ml = AdjacentCell(cell, Position.Left)) != null)
                        ProcessJewel(ml, jeweled, removingSpecialJewelCells, marker);
                    if ((mr = AdjacentCell(cell, Position.Right)) != null)
                        ProcessJewel(mr, jeweled, removingSpecialJewelCells, marker);
                }
            }
		}

		private void NewColumn(object sender, NewColumnEventArgs args)
		{
			ProcessJewel(true);

			int prevColumn = args.PrevCellColumn;

			bool hadRemoval = false;

			if (prevColumn >= 0)
			{
				// loop over rows to see if squares should be removed:
				int numSquareRows = Squares[0].Length;
				var squaresToRemove = new List<Square>();
				for (int r = 0; r < numSquareRows; r++)
				{
					for (int c = prevColumn; c >= 0; c--)
					{
						var s = FindSquare(c,r);
						if (s?.CanRemove(args.PrevCellColumn, args.CellColumn) == Square.ChainStates.Removeable)
						{
							squaresToRemove.Add(s);
						}
					}
				}

				// execute removal after collecting all squares:
				hadRemoval = hadRemoval || squaresToRemove.Count > 0;
				foreach (var s in squaresToRemove)
				{
					s.Remove();
				}

				// see if we've reached a column that doesn't have any jeweled cells:
				if (InProcessOfRemovingJewelCells)
				{
                    // check if we just wrapped our playfield:
                    int currColumn = Timeline.Column;
                    bool checkWrapped = Timeline.Wrapped(args.PrevCellColumn) &&
                        GetCellColumn(NumCellColumns - 1).Any(c => c.RemoveState == Cell.RemoveStates.JewelRemoving);
                    bool checkNoJewelCellsInColumn = Cells[currColumn].All(c => c.RemoveState != Cell.RemoveStates.JewelRemoving
                        && c.RemoveState != Cell.RemoveStates.JewelWillBeRemoved);

                    if (checkNoJewelCellsInColumn || checkWrapped)
					{
                        for (int c = 0; c <= args.PrevCellColumn; c++)
						{
							for (int r = 0; r < Cells[currColumn].Length; r++)
							{
								var cell = Cells[c][r];
								if (cell.RemoveState == Cell.RemoveStates.JewelRemoving)
								{
									cell.Reset();
									hadRemoval = true;
								}
							}
						}

						InProcessOfRemovingJewelCells = false;
					}
				}
			}

			for (int c = args.CellColumn; c <= args.CellColumn; c++)
			{
				for (int r = NumCellRows - 1; r >= 0; r--)
				{
					var cell = Cells[c][r];

					// advance cell states:
					if (cell.RemoveState == Cell.RemoveStates.WillBeRemoved)
					{
						cell.RemoveState = Cell.RemoveStates.Removing;
					}
				}
			}

			if (!hadRemoval)
				return;

			bool emptyField = true;
			bool hasOneColor = true;
			Cell.States? oneColor = null;

			// fall cells based on removed squares:
			for (int c = 0; c < NumCellColumns; c++)
			{
                // collect move counts based on empty cells:
                var moveCounts = new int[NumCellRows];
                int currMoveCount = 0;
                for (int r = NumCellRows - 1; r >= 0; r--)
                {
					var cell = Cells[c][r];

					if (cell.IsImmovable)
					{
						currMoveCount = 0;
					}

                    moveCounts[r] = cell.IsOccupied? currMoveCount : 0;
                    currMoveCount += cell.IsOccupied? 0 : 1;

					if (cell.IsOccupied)
					{
						emptyField = false;
					}

					// check to see if we have only one color:
					if (oneColor == null)
					{
						oneColor = cell.Color;
					}
					else if ((cell.IsWhite || cell.IsBlack)
                        && !cell.IsSameColor((Cell.States)oneColor))
					{
						hasOneColor = false;
					}
                }

				// transfer cells:
				for (int i = NumCellRows - 1; i >= 0; i--)
				{
					if (moveCounts[i] > 0)
					{
						Cells[c][i].TransferTo(Cells[c][i + moveCounts[i]]);
					}
				}
			}

			// add status:
            if (emptyField)
                Stats.AddEmptyColorBonus();
			else if (hasOneColor)
				Stats.AddSingleColorBonus();
        }

        public string PrintCells(string linePrefix)
		{
			var str = new StringBuilder();

			str.Append(linePrefix);
			int totalCellCount = (NumCellColumns * 3 * 2) + 1; 
			for (int c = 0; c < totalCellCount; c++)
				str.Append("-");
			str.Append(Environment.NewLine);

			for (int r = 0; r < NumCellRows; r++)
			{
				str.Append(linePrefix);
				str.Append("|");
				for (int c = 0; c < NumCellColumns; c++)
				{
					switch (Cells[c][r].State)
					{
						case Cell.States.Empty:
							str.Append("--");
							break;
						case Cell.States.Disabled:
							str.Append(" D");
							break;
						case Cell.States.Black:
							str.Append(" B");
							break;
						case Cell.States.White:
							str.Append(" W");
							break;
						case Cell.States.BlackAndWhite:
							str.Append("BW");
							break;
						case Cell.States.BlackJeweledBoth:
							str.Append("JB");
							break;
						case Cell.States.WhiteJeweledBoth:
							str.Append("JW");
							break;
					}

					switch (Cells[c][r].RemoveState)
					{
						case Cell.RemoveStates.Removing:
							str.Append("_RM|");
							break;

						case Cell.RemoveStates.WillBeRemoved:
							str.Append("_WR|");
							break;

						case Cell.RemoveStates.NotRemoved:
							str.Append("---|");
							break;

						case Cell.RemoveStates.JewelRemoving:
							str.Append("_JR|");
							break;

						case Cell.RemoveStates.JewelWillBeRemoved:
							str.Append("_JW|");
							break;
					}
				}

				str.Append(Environment.NewLine);

				str.Append(linePrefix);
				for (int c = 0; c < totalCellCount; c++)
					str.Append("-");
				str.Append(Environment.NewLine);
			}

			return str.ToString();
		}

		#region IEquatable
		public bool Equals(Playfield other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
            return GetEnumeratorCells().SequenceEqual(other.GetEnumeratorCells()) &&
                GetEnumeratorSquares().SequenceEqual(other.GetEnumeratorSquares()) &&
                Equals(Timeline, other.Timeline) && Equals(Stats, other.Stats) &&
                JeweledCells.SequenceEqual(other.JeweledCells);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Playfield) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (Cells != null ? Cells.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (Squares?.GetHashCode() ?? 0);
				hashCode = (hashCode*397) ^ (Timeline?.GetHashCode() ?? 0);
				hashCode = (hashCode*397) ^ (Stats?.GetHashCode() ?? 0);
				hashCode = (hashCode*397) ^ (JeweledCells?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		public static bool operator ==(Playfield left, Playfield right) => Equals(left, right);
		public static bool operator !=(Playfield left, Playfield right) => !Equals(left, right);
		#endregion

		public Playfield DeepClone()
		{
			var p = Logic.SerializeHelpers.DeepClone(this);
			foreach (var s in p.GetEnumeratorSquares())
			{
				s.Playfield = this;
			}
            //TODO: jeweled cells
            return p;
		}

		public int NumCellColumns => Cells.Length;
	    public int NumCellRows => NumCellColumns>0?Cells[0].Length:0;
	    public int NumSquareColumns => Squares.Length;
	    public int NumSquareRows => NumSquareColumns>0?Squares[0].Length:0;

	    public IEnumerable<Cell> GetEnumeratorCells()
		{
			for (int c = 0; c < Cells.Length; c++)
			{
				for (int r = 0; r < Cells[c].Length; r++)
				{
					yield return Cells[c][r];
				}
			}
		}

		public IEnumerable<Square> GetEnumeratorSquares()
		{
			for (int c = 0; c < Squares.Length; c++)
			{
				for (int r = 0; r < Squares[c].Length; r++)
				{
					yield return Squares[c][r];
				}
			}
		}

		public Cell GetCell(PlayfieldPoint p) => Cells[p.Column][p.Row];
	    public Cell GetCell(int column, int row) => Cells[column][row];
	    public Cell[] GetCellColumn(int column) => Cells[column];

	    /// <summary>
        /// Delegate for finding a square based on column/row info.
        /// </summary>
        /// <param name="column">The column of the square</param>
        /// <param name="row">The row of the square</param>
        /// <returns>The found square, or null if the square was outside the bounds</returns>
        public Square FindSquare(int column, int row)
		{
			if (column < 0 || column >= NumSquareColumns)
				return null;
			else if (row < 0 || row >= NumSquareRows)
				return null;
			else
				return Squares[column][row];
		}

		[DataMember]
		public Cell[][] Cells { get; private set; }
		[DataMember]
		public Square[][] Squares { get; private set; }
		[DataMember]
		public Timeline Timeline { get; private set; }
		[DataMember]
        public Stats Stats { get; private set; }
		[DataMember]
		public bool InProcessOfRemovingJewelCells { get; private set; }

		public IList<Cell> JeweledCells { get; internal set; }
    }
}

	
