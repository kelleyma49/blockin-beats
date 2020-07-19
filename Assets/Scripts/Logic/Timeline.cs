using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Logic
{
	[DataContract]
	public class Timeline : IEquatable<Timeline>
	{
		/// <summary>
		/// Constructor for the Timeline.
		/// </summary>
		/// <param name="numColumns">The number of columns in the playfield.</param>
		public Timeline(int numColumns)
		{
			NumColumns = numColumns;
		}

		/// <summary>
		/// Absolute position along the Playfield. 
		/// </summary>
		[DataMember]
		public double PositionAbs { get; set; }

		/// <summary>
		/// Total number of columns in timeline. 
		/// </summary>
		[DataMember]
		public int NumColumns { get; internal set; }

		/// <summary>
		/// The number of total columns passed
		/// </summary>
		[DataMember]
		public int TotalColumnAbs { get; internal set; }

		/// <summary>
		/// The column in the Playfield
		/// </summary>
		public int Column => (int) PositionAbs;


	    /// <summary>
		/// Normalized position along the Playfield. 
		/// </summary>
		public double Position => PositionAbs/NumColumns;

	    /// <summary>
	    /// Check to see if we just wrapped back to the beginning of the timeline.
	    /// </summary>
	    /// <param name="prevColumn">The previous column.</param>
	    /// <returns>True if we just wrapped ; false if not.</returns>
	    public bool Wrapped(int prevColumn) => Column < prevColumn;

		/// <summary>
		/// Increments the normalized position by the specified amount.
		/// </summary>
		/// <param name="amount">the amount to increment</param>
		public void IncrementPosition(double amount)
		{
			// make sure we trigger all new column hits:
			var incrPerColumn = 1.0/NumColumns;

			bool doFirstTime = Column == 0 && TotalColumnAbs == 0 && PositionAbs == 0.0;
            do
            {
                double inc = Math.Min(amount, incrPerColumn);

                int prevColumn = Column;
                PositionAbs += inc * NumColumns;
                if (PositionAbs >= NumColumns)
                {
                    PositionAbs %= NumColumns;
                    OnNewColumn(NumColumns - 1, Column);
                    OnWrapColumn();
                }
                else if (Column != prevColumn)
                {
                    TotalColumnAbs += 1;
                    OnNewColumn(prevColumn, Column);
                }
                else if (doFirstTime && PositionAbs > 0.0f)
                {
                    doFirstTime = false;
                    OnNewColumn(prevColumn, Column);
                }

                amount -= inc;
            }
            while (amount > 0.0);
        }

		#region IEquatable
		public bool Equals(Timeline other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return PositionAbs.Equals(other.PositionAbs) && NumColumns == other.NumColumns;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Timeline) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (PositionAbs.GetHashCode()*397) ^ NumColumns;
			}
		}

		public static bool operator ==(Timeline left, Timeline right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Timeline left, Timeline right)
		{
			return !Equals(left, right);
		}
		#endregion

		public Timeline DeepClone() => Logic.SerializeHelpers.DeepClone(this);

		/// <summary>
		/// Event that gets triggered when the timeline moves to a new column.
		/// </summary>
		public event EventHandler<NewColumnEventArgs> NewColumn;

		private void OnNewColumn(int prevColumn,int currColumn)
		{
            TotalColumnAbs++;
			NewColumn?.Invoke(this, new NewColumnEventArgs(prevColumn,currColumn));
		}

		/// <summary>
		/// Event that gets triggered when the timeline wraps to the beginning.
		/// </summary>
		public event EventHandler WrapColumn;

		private void OnWrapColumn() => WrapColumn?.Invoke(this, EventArgs.Empty);
	}

	public class NewColumnEventArgs : EventArgs
	{
		public NewColumnEventArgs(int prevCellColumn,int cellColumn)
		{
			PrevCellColumn = prevCellColumn;
			CellColumn = cellColumn;
		}

		public readonly int PrevCellColumn;
		public readonly int CellColumn;
	}
}
