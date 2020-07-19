using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Logic
{
    /// <summary>
    /// This is the O tetromino (http://en.wikipedia.org/wiki/Tetromino)
    /// </summary>
    [DataContract]
    public class DropPiece : IEquatable<DropPiece>
    {
        public const int NumColumns = DropPieceSimple.NumColumns;
        public const int NumRows = DropPieceSimple.NumRows;

        public DropPiece(DropPieceSimple simple = null)
        {
            Positions = new PlayfieldPoint[NumColumns];
            PositionLerp = new float[NumColumns];
            // create player block:
            for (var x = 0; x < NumColumns; x++)
            {
                Positions[x] = new PlayfieldPoint();
            }

            CurrentState = State.Whole;
            DropPieceSimple = simple;
        }

        public delegate bool CanMoveColumn(int column, int row, int? y);

        public void Reset(PlayfieldPoint pos, bool allowJewel, DropPieceSimple.GetCellState del = null)
        {
            DropPieceSimple = new DropPieceSimple();

            if (del == null)
            {
                del = DropPieceSimple.RandomCell;
            }

            for (var x = 0; x < NumColumns; x++)
            {
                Positions[x].Column = pos.Column + x;
                Positions[x].Row = pos.Row;
            }

            DropPieceSimple.Reset(allowJewel, del);

            CurrentState = State.Whole;
            HoldTimer = HoldTime;
            FastDrop = false;
        }

        public void Reset(PlayfieldPoint pos, DropPieceSimple simple)
        {
            DropPieceSimple = simple;

            for (var x = 0; x < NumColumns; x++)
            {
                Positions[x].Column = pos.Column + x;
                Positions[x].Row = pos.Row;
            }

            CurrentState = State.Whole;
            HoldTimer = HoldTime;
            FastDrop = false;
        }

        public enum MoveDirection
        {
            /// <summary>
            /// Moves to drop piece to the left
            /// </summary>
            Left,
            /// <summary>
            /// moves to drop piece to the right
            /// </summary>
            Right,
            /// <summary>
            /// Rotates the drop piece left or right
            /// </summary>
            Rotate,
            /// <summary>
 			/// Moves the drop piece down
 			/// </summary>
			Down,
            /// <summary>
            /// No movement
            /// </summary>
            None
        };

        public delegate bool DropShouldStop(PlayfieldPoint p);
        public delegate void Populate(PlayfieldPoint p, Cell.States state);

        public event EventHandler<DroppedDownEventArgs> DroppedDown;
        public event EventHandler<RotateEventArgs> Rotated;
        public event EventHandler NewColumnOrRow;

        /// <summary>
        /// Updates the timer for the drop block.
        /// </summary>
        /// <param name="span">The time span that is used to update the drop.</param>
        /// <returns>return true if the block should be dropped down to the next square.</returns>
        private bool UpdateTimer(TimeSpan span)
        {
            bool dropDown = false;
            DropTimerLerp = 0;

            if (Holding)
            {
                HoldTimer -= span.TotalSeconds;
            }
            else
            {
                DropTimer += span.TotalSeconds;

                var dropRate = DownDropRate ?? DropRate;
                dropDown = DropTimer > dropRate;
                if (dropDown)
                {
                    DropTimer %= dropRate;

                    DroppedDown?.Invoke(this, new DroppedDownEventArgs(FastDrop));

                    // add points for fast dropping:
                    /*s
                                        if (FastDrop)
                                        {
                                            _Score.Current = _Score.Current + 1;
                                        }
                    */
                }

                DropTimerLerp = DropTimer / dropRate;
            }

            return dropDown;
        }

        public bool Update(TimeSpan span,
            MoveDirection dir,
             CanMoveColumn canMoveColumn,
            Action<MoveDirection> movedAction,
            Predicate<PlayfieldPoint> dropShouldStop,
            Populate populate)
        {
            if (dir == MoveDirection.Down)
            {
                FastDrop = true;
                DownDropRate = null;
            }

            bool dropDown = UpdateTimer(span);

            bool moved = false;
            int increment = 0;

            // block must be full 2x2 to allow movement:
            if (CurrentState == State.Whole)
            {
                // process rotate: 
                if (dir == MoveDirection.Rotate)
                {
                    Rotate(RotateDirection.Left);
                    moved = true;
                }

                switch (dir)
                {
                    case MoveDirection.Left: increment = -1; break;
                    case MoveDirection.Right: increment = 1; break;
                    default:
                        increment = 0; break;
                }
            }

            // lerp:
            PositionLerp[0] = PositionLerp[1] = 0.0f + ((1.0f - 0.0f) * (float)DropTimerLerp);

            if (dropDown)
            {
                if (CurrentState == State.Whole || CurrentState == State.SplitLeft)
                {
                    NewColumnOrRow?.Invoke(this, null);
                    Positions[0].Row++;
                }
                if (CurrentState == State.Whole || CurrentState == State.SplitRight)
                {
                    NewColumnOrRow?.Invoke(this, null);
                    Positions[1].Row++;
                }
            }

            int newXLeft = Positions[0].Column + increment;
            int newXRight = Positions[1].Column + increment;

            // move left or right:
            int? downY = !Holding ? (int?)Positions[0].Row + 2 : null;
            if (canMoveColumn(newXLeft, newXRight, downY))
            {
                Positions[0].Column = newXLeft;
                Positions[1].Column = newXRight;
                moved = moved || (dir == MoveDirection.Left || dir == MoveDirection.Right);
                if (increment != 0)
                {
                    movedAction(dir);
                    NewColumnOrRow?.Invoke(this, null);
                }
            }

            if (!Holding)
            {
                try
                {
                    switch (CurrentState)
                    {
                        case State.Whole:
                            {
                                bool leftSide = DropSide(0, dropShouldStop, populate);
                                bool rightSide = DropSide(1, dropShouldStop, populate);
                                if (leftSide)
                                {
                                    CurrentState = rightSide ? State.Complete : State.SplitRight;
                                    DownDropRate = null;
                                }
                                else if (rightSide)
                                {
                                    CurrentState = State.SplitLeft;
                                    DownDropRate = null;
                                }
                            }
                            break;

                        case State.SplitRight:
                            if (DropSide(1, dropShouldStop, populate))
                                CurrentState = State.Complete;
                            break;

                        case State.SplitLeft:
                            if (DropSide(0, dropShouldStop, populate))
                                CurrentState = State.Complete;
                            break;
                    }

                    if (CurrentState == State.SplitRight || CurrentState == State.Complete)
                    {
                        PositionLerp[0] = 0.0f;
                    }
                    if (CurrentState == State.SplitLeft || CurrentState == State.Complete)
                    {
                        PositionLerp[1] = 0.0f;
                    }
                }
                catch (System.IndexOutOfRangeException)
                {
                    HitTop = true;
                }
            }

            return moved;
        }

        public bool DropSide(int x, Predicate<PlayfieldPoint> dropShouldStop, Populate populate)
        {
            if (dropShouldStop(new PlayfieldPoint(Positions[x].Column, Positions[x].Row + 2)))
            {
                populate(new PlayfieldPoint(Positions[x].Column, Positions[x].Row), DropPieceSimple.Cells[x][0]);
                populate(new PlayfieldPoint(Positions[x].Column, Positions[x].Row + 1), DropPieceSimple.Cells[x][1]);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Rotate(RotateDirection dir)
        {
            System.Diagnostics.Debug.Assert(NumColumns == 2 && NumRows == 2);

            var cells = DropPieceSimple.Cells;
            if (dir == RotateDirection.Left)
            {
                var first = cells[0][0];
                cells[0][0] = cells[0][1];
                cells[0][1] = cells[1][1];
                cells[1][1] = cells[1][0];
                cells[1][0] = first;
            }
            else
            {
                var first = cells[0][0];
                cells[0][0] = cells[1][0];
                cells[1][0] = cells[1][1];
                cells[1][1] = cells[0][1];
                cells[0][1] = first;
            }

            Rotated?.Invoke(this, new RotateEventArgs(dir));
        }

        public enum RotateDirection { Left, Right };

        public enum State
        {
            /// <summary>
            /// dropping as a cube, nothing has touched another full cell on the playing field.
            /// </summary>
            Whole,

            /// <summary>
            /// Left side has touched an occupied cell on the playing field.
            /// </summary>
            SplitRight,

            /// <summary>
            /// Right side has touched an occupied cell on the playing field.
            /// </summary>
            SplitLeft,

            /// <summary>
            /// Dropping is complete.
            /// </summary>
            Complete
        };

        #region DataMembers
        [DataMember]
        public State CurrentState { get; set; }

        [DataMember]
        public double DropTimer { get; private set; }

        [DataMember]
        public double DropTimerLerp { get; private set; }

        [DataMember]
        public bool FastDrop { get; private set; }

        [DataMember]
        public bool HitTop { get; private set; }

        public Cell.States[][] Cells { get { return DropPieceSimple.Cells; } }

        [DataMember]
        public DropPieceSimple DropPieceSimple { get; private set; }

        [DataMember]
        public PlayfieldPoint[] Positions { get; private set; }

        /// <summary>
        /// Upper positions of the block columns
        /// </summary>
        [DataMember]
        public float[] PositionLerp { get; private set; }

        /// <summary>
        /// The total time to hold the drop piece at the top.
        /// </summary>
        [DataMember]
        public double HoldTime { get; set; }

        [DataMember]
        public double DropRateWhole { get; set; }
        public double DropRateSplit { get; set; }

        /// <summary>
        /// The current time to hold the drop piece at the top.
        /// </summary>
        [DataMember]
        protected double HoldTimer { get; set; }

        /// <summary>
        /// The current rate to drop the block, seconds.
        /// </summary>
        [DataMember]
        internal double? DownDropRate { get; set; }
        #endregion

        internal bool Holding => HoldTimer > 0.0f;
        internal double DropRate => CurrentState == State.Whole ? DropRateWhole : DropRateSplit;

        public void CancelHolding() => HoldTimer = 0.0f;

        #region IEquatable
        public bool Equals(DropPiece other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Positions.SequenceEqual(other.Positions)
                && PositionLerp.SequenceEqual(other.PositionLerp)
                && CurrentState == other.CurrentState
                && HoldTimer.Equals(other.HoldTimer))
            {
                return this.DropPieceSimple.Equals(other.DropPieceSimple);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DropPiece)obj);
        }

        public override int GetHashCode()
        {
            //unchecked
            {
                int hashCode = DropPieceSimple.GetHashCode();
                hashCode = (hashCode * 397) ^ (Positions?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (PositionLerp?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)CurrentState;
                hashCode = (hashCode * 397) ^ HoldTimer.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DropPiece left, DropPiece right) => Equals(left, right);
        public static bool operator !=(DropPiece left, DropPiece right) => !Equals(left, right);
        #endregion

        public DropPiece DeepClone() => Logic.SerializeHelpers.DeepClone(this);
    }

    #region EventArgs
    public class DroppedDownEventArgs : EventArgs
    {
        public DroppedDownEventArgs(bool fastDrop)
        {
            FastDrop = fastDrop;
        }

        public bool FastDrop { get; }
    }

    public class RotateEventArgs : EventArgs
    {
        public RotateEventArgs(DropPiece.RotateDirection dir)
        {
            Direction = dir;
        }

        public DropPiece.RotateDirection Direction { get; }
    }
    #endregion
}
