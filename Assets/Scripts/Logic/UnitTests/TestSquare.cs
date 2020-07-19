#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;
using Logic;

namespace Logic.Tests
{
	[TestFixture()]
	public class TestSquare
	{
		[Test()]
		public void TestCellAccessors()
		{
			var pf = new Playfield(2, 2);
			var sq = pf.Squares[0][0];
			Assert.AreEqual(pf.Cells[0][0], sq.UpperLeft);
			Assert.AreEqual(pf.Cells[1][0], sq.UpperRight);
			Assert.AreEqual(pf.Cells[0][1], sq.LowerLeft);
			Assert.AreEqual(pf.Cells[1][1], sq.LowerRight);
		}

		[Test()]
		public void TestOtherSquareAccessorsNone()
		{
			var pf = new Playfield(2, 2);
			var sq = pf.Squares[0][0];
			Assert.AreEqual(new Square[3] { null, null, null }, sq.GetOtherSquares(sq.UpperLeft));
			Assert.AreEqual(new Square[3] { null, null, null }, sq.GetOtherSquares(sq.UpperRight));
			Assert.AreEqual(new Square[3] { null, null, null }, sq.GetOtherSquares(sq.LowerLeft));
			Assert.AreEqual(new Square[3] { null, null, null }, sq.GetOtherSquares(sq.LowerRight));
		}

		[Test()]
		public void TestOtherSquareAccessorsExist()
		{
			var pf = new Playfield(4, 4);
			var center = pf.Squares[1][1];
			var ul = pf.Squares[0][0];
			var ur = pf.Squares[2][0];
			var cu = pf.Squares[1][0];
			var cl = pf.Squares[0][1];
			var cr = pf.Squares[2][1];
			var ll = pf.Squares[0][2];
			var lr = pf.Squares[2][2];
			var cb = pf.Squares[1][2];
			Assert.AreEqual(new Square[3] { cl, ul, cu }, center.GetOtherSquares(center.UpperLeft));
			Assert.AreEqual(new Square[3] { cu, ur, cr }, center.GetOtherSquares(center.UpperRight));
			Assert.AreEqual(new Square[3] { cb, ll, cl }, center.GetOtherSquares(center.LowerLeft));
			Assert.AreEqual(new Square[3] { cr, lr, cb }, center.GetOtherSquares(center.LowerRight));
		}


		[Test()]
		public void TestStateChanged()
		{
			var pf = new Playfield(2, 2);
			var sq = new Square(0, 0, pf);
			int eventCalledCount = 0;
			sq.StateChanged += (arg1, arg2) => ++eventCalledCount;
			foreach (var c in pf.GetEnumeratorCells())
			{
				c.StateChanged += sq.CellStateChanged;
				Assert.AreEqual(0,eventCalledCount);
				c.State = Cell.States.White;
			}
			// last state set should trigger state change:
			Assert.AreEqual(1, eventCalledCount);

			foreach (var c in pf.GetEnumeratorCells())
			{
				c.State = Cell.States.Empty;
				c.RemoveState = Cell.RemoveStates.NotRemoved;
			}
			Assert.AreEqual(2, eventCalledCount);
		}

		[Test()]
		public void TestEquality()
		{
			var pf = new Playfield(2, 2);
			var sq1 = new Square(1, 1, pf);
			var sq2 = new Square(1, 1, pf);

			Assert.AreEqual(sq1, sq2);
			Assert.AreNotSame(sq1, sq2);
		}

		[Test()]
		public void TestInequality()
		{
			var pf = new Playfield(3, 3);
			var sq1 = new Square(1, 1, pf);
			var sq2 = new Square(2, 2, pf);

			Assert.AreNotEqual(sq1, sq2);
			Assert.AreNotSame(sq1, sq2);
		}
	}
}
#endif // ENABLE_UNITTESTS
