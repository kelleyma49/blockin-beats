using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Logic
{
    [DataContract]
    public class Stats : IEquatable<Stats>
    {
        public const int SquareMult = 40;

        public const int NumSquaresForBonus = 1;

        public const int BonusSquareMult = 160;
        public const int SingleColorBonus = 1000;
        public const int EmptyColorBonus = 10000;

        [DataMember]
        public int Frame { get; set; }
        [DataMember]
        public int FrameSquaresRemoved { get; set; }
        [DataMember]
        public int NumPrevBonuses { get; set; }
        [DataMember]
        public int TotalSquaresRemoved { get; set; }

        [DataMember]
        public int TotalSquaresBonuses { get; set; }
        [DataMember]
        public int TotalNumSingleColorBonuses { get; set; }
        [DataMember]
        public int TotalNumEmptyColorBonuses { get; set; }

        public delegate void BonusMultiplier(int multiplier);
        public event BonusMultiplier OnBonusMultiplier;

        public delegate void EmptyBonus();
        public event EmptyBonus OnEmptyBonus;

        public delegate void ColorBonus();
        public event ColorBonus OnColorBonus;

        private bool _frameHadEmptyColorBonus;

        #region IEquatable
        public bool Equals(Stats other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Frame == other.Frame && FrameSquaresRemoved == other.FrameSquaresRemoved && NumPrevBonuses == other.NumPrevBonuses && TotalSquaresRemoved == other.TotalSquaresRemoved && TotalSquaresBonuses == other.TotalSquaresBonuses && TotalNumSingleColorBonuses == other.TotalNumSingleColorBonuses && TotalNumEmptyColorBonuses == other.TotalNumEmptyColorBonuses;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Stats)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Frame;
                hashCode = (hashCode * 397) ^ FrameSquaresRemoved;
                hashCode = (hashCode * 397) ^ NumPrevBonuses;
                hashCode = (hashCode * 397) ^ TotalSquaresRemoved;
                hashCode = (hashCode * 397) ^ TotalSquaresBonuses;
                hashCode = (hashCode * 397) ^ TotalNumSingleColorBonuses;
                hashCode = (hashCode * 397) ^ TotalNumEmptyColorBonuses;
                return hashCode;
            }
        }

        public static bool operator ==(Stats left, Stats right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Stats left, Stats right)
        {
            return !Equals(left, right);
        }
        #endregion

        public Stats DeepClone() => Logic.SerializeHelpers.DeepClone(this);

        public void AddCompletedSquare()
        {
            FrameSquaresRemoved++;
            TotalSquaresRemoved++;
        }

        public void IncrementFrame()
        {
            if (FrameSquaresRemoved >= NumSquaresForBonus)
            {
                NumPrevBonuses++;
                TotalSquaresBonuses = TotalSquaresBonuses + (BonusSquareMult * NumPrevBonuses);
                OnBonusMultiplier?.Invoke(NumPrevBonuses);
            }
            else
            {
                NumPrevBonuses = 0;
            }

            _frameHadEmptyColorBonus = false;
            FrameSquaresRemoved = 0;
            Frame = Frame + 1;
        }

        public void AddSingleColorBonus()
        {
            TotalNumSingleColorBonuses++;
            OnColorBonus?.Invoke();
        }

        public void AddEmptyColorBonus()
        {
            if (!_frameHadEmptyColorBonus)
            {
                _frameHadEmptyColorBonus = true;
                TotalNumEmptyColorBonuses = TotalNumEmptyColorBonuses + 1;
                OnEmptyBonus?.Invoke();
            }
        }

        public int TotalScore => TotalSquaresRemoved * 100 + TotalNumEmptyColorBonuses * 1000 + TotalNumSingleColorBonuses * 500;
    }
}
