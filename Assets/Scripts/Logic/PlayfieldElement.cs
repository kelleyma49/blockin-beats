using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Logic
{
    [DataContract]
    public abstract class PlayfieldElement
    {
        /// <summary>
        /// Constructor for playfield element.
        /// </summary>
        /// <param name="column">The column of the upper left cell.</param>
        /// <param name="row">The row of the upper left cell.</param>
        protected PlayfieldElement(int column, int row)
        {
            if (column < 0 || row < 0)
                throw new ArgumentOutOfRangeException();

            Column = column;
            Row = row;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PlayfieldElement;
            if (other != null)
            {
                return this.Column == other.Column && this.Row == other.Row;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => Column.GetHashCode() ^ Row.GetHashCode();

        [DataMember]
        public int Column { get; set; }
        [DataMember]
        public int Row { get; set; }

        public PlayfieldPoint Point => new PlayfieldPoint(Column, Row);
    }
}
