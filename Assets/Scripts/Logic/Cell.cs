using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Logic
{
	[DataContract]
	public class Cell : PlayfieldElement
	{
	    /// <summary>
	    /// Constructor for a cell.
	    /// </summary>
	    /// <param name="column">The column of the cell.</param>
	    /// <param name="row">The row of the cell.</param>
	    /// <param name="lockCount">The number of locks needed to unlock before removing the cell entirely.</param>
	    public Cell(int column, int row, int lockCount=1) : base(column, row)
        {
            LockCount = lockCount;
        }

		public void ResetSubscriptions()
		{
			StateChanged = null;
			RemoveStateChanged = null;
			Transferred = null;
		}

		public void Reset()
		{
			RemoveState = RemoveStates.NotRemoved;
			State = States.Empty;
            LockCount = 1;
		}

		public enum States
		{
			Empty,          // nothing in the cell
			Disabled,       // cell is occupied, but cannot be used in play	
			Black,          // cell is filled with black block
			White,          // cell is filled with white block
			BlackAndWhite,  // cells that can be black or white			

			BlackJeweledBoth,   // cell is filled with black block with jewel (vertical and horizontal)
            BlackJeweledHorz,
            BlackJeweledVert,
            WhiteJeweledBoth,    // cell is filled with white block with jewel (vertical and horizontal)
            WhiteJeweledHorz,
            WhiteJeweledVert
        };

		public enum RemoveStates
		{
			NotRemoved,                 // not removed 
			WillBeRemoved,              // part of a block of 4
			Removing,                   // timeline is sweeping over blocks

			JewelWillBeRemoved,         // ready for removal
			JewelRemoving,              // timeline has swept over block (which is connected to jewel)
		};

		public event EventHandler<CellStateChangedEventArgs> StateChanged;
		public event EventHandler<CellRemoveStateChangedEventArgs> RemoveStateChanged;
		public event EventHandler<TransferredEventArgs> Transferred;

        /// <summary>
        /// The amount of times the cell must be unlocked
        /// </summary>
        [DataMember]
        public int LockCount { get; private set; }

        [DataMember]
		public States State
		{
			get { return _state; }
			set
			{
				if (_state != value)
				{
					_state = value;
				    StateChanged?.Invoke(this, new CellStateChangedEventArgs(this));
				}
			}
		}

		/// <summary>
		/// The current remove state
		/// </summary>
		[DataMember]
		public RemoveStates RemoveState
		{
			get { return _removeState; }
			set
			{
				if (_removeState != value)
				{
					_removeState = value;
				    RemoveStateChanged?.Invoke(this, new CellRemoveStateChangedEventArgs(this));
				}
			}
		}

		public int RemoveStateColumnAbs { get; set; }

		public States? Color
		{
			get
			{
				if (IsWhite)
					return States.White;
				else if (IsBlack)
					return States.Black;
				else
					return null;
			}
		}

		public bool IsWhite => IsStateWhite(State);
	    public bool IsBlack => IsStateBlack(State);
	    public bool IsJeweled => IsStateJeweled(State);
	    public bool IsJeweledHorz => IsStateJeweledHorz(State);
	    public bool IsJeweledVert => IsStateJeweledVert(State);
	    public bool IsImmovable => State == States.Disabled;
	    public bool IsOccupied => IsBlack || IsWhite || State == States.Disabled;
	    public bool IsRemove => RemoveState == RemoveStates.Removing || RemoveState == RemoveStates.WillBeRemoved;
	    public bool IsJewelRemove => RemoveState == RemoveStates.JewelRemoving || RemoveState == RemoveStates.JewelWillBeRemoved;

	    public static bool IsStateWhite(States state) => state == States.White
                                                         || state == States.WhiteJeweledBoth
                                                         || state == States.WhiteJeweledHorz
                                                         || state == States.WhiteJeweledVert
                                                         || state == States.BlackAndWhite;

	    public static bool IsStateBlack(States state) => state == States.Black
                                                         || state == States.BlackJeweledBoth
                                                         || state == States.BlackJeweledHorz
                                                         || state == States.BlackJeweledVert
                                                         || state == States.BlackAndWhite;

	    public static bool IsStateJeweled(States state) => state == States.WhiteJeweledBoth
                                                           || state == States.WhiteJeweledHorz
                                                           || state == States.WhiteJeweledVert
                                                           || state == States.BlackJeweledBoth
                                                           || state == States.BlackJeweledHorz
                                                           || state == States.BlackJeweledVert;

	    public static bool IsStateJeweledHorz(States state) => state == States.WhiteJeweledBoth
                                                               || state == States.WhiteJeweledHorz
                                                               || state == States.BlackJeweledBoth
                                                               || state == States.BlackJeweledHorz;

	    public static bool IsStateJeweledVert(States state) => state == States.WhiteJeweledBoth
                                                               || state == States.WhiteJeweledVert
                                                               || state == States.BlackJeweledBoth
                                                               || state == States.BlackJeweledVert;

	    public bool RemoveCell()
        {
            if (--LockCount <= 0)
            {
                Reset();
                return true;
            }
            else
            {
                return false;
            }
        }

		public bool IsSameColor(Cell other) => IsSameColor(other.State);

	    public bool IsSameColor(States other) => (IsWhite && IsStateWhite(other)) || (IsBlack && IsStateBlack(other));

	    public override bool Equals(object obj)
		{
			var other = obj as Cell;
            if (other != null)
            {
                return State == other.State
                       && RemoveState == other.RemoveState
                       && base.Equals(other);
            }
            else
            {
                return false;
            }
        }

		public override int GetHashCode() => _state.GetHashCode() ^ base.GetHashCode();

	    public void TransferTo(Cell dest)
		{
			if (dest == this)
				return;

			dest.RemoveState = Cell.RemoveStates.NotRemoved;
			dest.State = this.State;
            dest.LockCount = this.LockCount;
            this.Reset();
            Transferred?.Invoke(this, new TransferredEventArgs(dest, this.Point));
        }

        private States _state;
		private RemoveStates _removeState;
	}

	public class CellStateChangedEventArgs : EventArgs
	{
		public CellStateChangedEventArgs(Cell cell)
		{
			NewState = cell.State;
			Column = cell.Column;
			Row = cell.Row;
		}

        public Cell.States NewState { get; }
        public int Column { get; }
        public int Row { get; }
    }

	public class CellRemoveStateChangedEventArgs : EventArgs
	{
		public CellRemoveStateChangedEventArgs(Cell cell)
		{
			NewState = cell.RemoveState;
			Column = cell.Column;
			Row = cell.Row;
		}

        public Cell.RemoveStates NewState { get; }
        public int Column { get; }
        public int Row { get; }
    }

	public class TransferredEventArgs : EventArgs
	{
		public TransferredEventArgs(Cell cell,PlayfieldPoint prevPoint)
		{
			Cell = cell;
			PrevPoint = prevPoint;
		}

        public Cell Cell { get; }
        public PlayfieldPoint PrevPoint { get; }
    }
}
