using System.Runtime.Serialization;

namespace Logic
{
	[DataContract]
	public struct PlayfieldPoint
	{
		public PlayfieldPoint(int c, int r) : this() { Column = c; Row = r; }

		[DataMember]
		public int Column { get; set; }
		[DataMember]
		public int Row { get; set; }

		public override string ToString() => Column  + "," + Row;
	}
}
