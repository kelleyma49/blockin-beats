#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;
using Logic;

namespace Logic.Tests
{
	[TestFixture()]
	public class TestDropPiece
	{
		#region Helpers 
		private Logic.DropPiece CreateCheckeredDropPiece(double holdTimeSecs = 0.0)
		{
			var dp = new Logic.DropPiece();
			dp.HoldTime = holdTimeSecs;
			dp.DownDropRate = 1.0;
			dp.DropRateWhole = 0.5;
			dp.DropRateSplit = 0.25;
            dp.Reset(new PlayfieldPoint(0, 0), false, (c, r, aj) => c % 2 == 0 ? Cell.States.Black : Cell.States.White);
			return dp;
		}

		private void TestPosition(Logic.DropPiece dp, int expectedColumn, int expectedRow)
		{
			Assert.AreEqual(expectedColumn, dp.Positions[0].Column);
			Assert.AreEqual(expectedRow, dp.Positions[0].Row);
			Assert.AreEqual(expectedColumn + 1, dp.Positions[1].Column);
			Assert.AreEqual(expectedRow, dp.Positions[1].Row);
		}

		static readonly TimeSpan OneMs = new TimeSpan(0, 0, 0, 0, 1);
		static readonly TimeSpan OneSec = new TimeSpan(0, 0, 0, 1, 0);
		#endregion

		[Test()]
		public void TestConstructor()
		{
			var dp = new DropPiece();
			Assert.AreEqual(DropPiece.State.Whole, dp.CurrentState);
		}

		[Test()]
		public void TestReset()
		{
			var dp = new Logic.DropPiece();
            var dps = new Logic.DropPieceSimple();
			dp.Reset(new PlayfieldPoint(0, 0), false);
			Assert.AreEqual(DropPiece.State.Whole, dp.CurrentState);
			foreach (var row in dp.Cells)
			{
				foreach (var s in row)
				{
					Assert.AreNotEqual(Cell.States.Disabled, s);
					Assert.AreNotEqual(Cell.States.Empty, s);
				}
			}
		}

		[Test()]
		public void TestResetDelegate()
		{
			var dp = CreateCheckeredDropPiece();
			Assert.AreEqual(DropPiece.State.Whole, dp.CurrentState);
			int column = 0;
			foreach (var row in dp.Cells)
			{
				bool isBlack = (column++) % 2 == 0;
				foreach (var s in row)
				{
					var expected = isBlack ? Cell.States.Black : Cell.States.White;
					Assert.AreEqual(expected, s);
				}
			}

		}

		[Test()]
		public void TestPopulate()
		{
			{
				var dp = CreateCheckeredDropPiece(OneSec.TotalSeconds);
				var populated = false;
				Assert.IsFalse(dp.Update(OneSec, DropPiece.MoveDirection.None, (_, __, ___) => false, (_) => { },
				                         (_) => true, (_, __) => populated = true));
				Assert.IsTrue(populated);
			}
		}

		[Test()]
		public void TestHolding()
		{
			{
				var dp = CreateCheckeredDropPiece(OneSec.TotalSeconds);
				Assert.IsTrue(dp.Holding);
				Assert.IsFalse(dp.Update(OneMs, DropPiece.MoveDirection.None, (_, __, ___) => true,
				                         (_) => { }, (_) => false, (_, __) => { }));
				Assert.IsTrue(dp.Holding);
				Assert.IsFalse(dp.Update(OneSec, DropPiece.MoveDirection.None, (_, __, ___) => true,
                                         (_) => { }, (_) => false, (_, __) => { }));
                Assert.IsFalse(dp.Holding);

				dp.Reset(new PlayfieldPoint(0, 0), false);
				Assert.IsTrue(dp.Holding);
			}
		}

		[Test()]
		public void TestUpdateCanMoveColumn()
		{
			{
				var dp = CreateCheckeredDropPiece(OneSec.TotalSeconds);
				Assert.IsFalse(dp.Update(OneMs, DropPiece.MoveDirection.Left, (c, _, __) => c >= 0 && c <= 2,
                                         (_) => { }, (_) => false, (_, __) => { }));
                TestPosition(dp, 0, 0);
				Assert.IsTrue(dp.Update(OneMs, DropPiece.MoveDirection.Right, (c, _, __) => c >= 0 && c < 2,
                                        (_) => { }, (_) => false, (_, __) => { }));
                TestPosition(dp, 1, 0);
				Assert.IsFalse(dp.Update(OneMs, DropPiece.MoveDirection.Right, (c, _, __) => c >= 0 && c < 2,
                                         (_) => { }, (_) => false, (_, __) => { }));
                TestPosition(dp, 1, 0);

				dp.Reset(new PlayfieldPoint(0, 0), false);
				Assert.IsTrue(dp.Holding);
			}
		}


		[Test()]
		public void TestRotateRight()
		{
			var dp = CreateCheckeredDropPiece();

			dp.Rotate(DropPiece.RotateDirection.Right);
			Assert.IsFalse(dp.FastDrop);
			Assert.AreEqual(Cell.States.White, dp.Cells[0][0]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][0]);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][1]);
			Assert.AreEqual(Cell.States.Black, dp.Cells[1][1]);
		}

		[Test()]
		public void TestRotateEvent()
		{
			var dp = CreateCheckeredDropPiece();

			DropPiece.RotateDirection? dir = null;
			dp.Rotated += (o, ea) => dir = ea.Direction;
			dp.Rotate(DropPiece.RotateDirection.Right);
			Assert.IsNotNull(dir);
			Assert.AreEqual((DropPiece.RotateDirection)dir, DropPiece.RotateDirection.Right);
		}

		[Test()]
		public void TestUpdateRotate()
		{
			var dp = CreateCheckeredDropPiece();

			Assert.IsTrue(dp.Update(OneMs, DropPiece.MoveDirection.Rotate, (_, __, ___) => true,
			                        (_) => { }, (_) => false, (_, __) => { }));
			Assert.IsFalse(dp.FastDrop);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][0]);
			Assert.AreEqual(Cell.States.Black, dp.Cells[1][0]);
			Assert.AreEqual(Cell.States.White, dp.Cells[0][1]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][1]);
		}

		[Test()]
		public void TestUpdateMoveLeft()
		{
			var dp = CreateCheckeredDropPiece();

			Assert.IsTrue(dp.Update(OneMs, DropPiece.MoveDirection.Left, (_, __, ___) => true,
			                        (_) => { }, (_) => false, (_, __) => { }));
			Assert.IsFalse(dp.FastDrop);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][0]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][0]);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][1]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][1]);

			TestPosition(dp, -1, 0);
		}

		[Test()]
		public void TestUpdateMoveRight()
		{
			var dp = CreateCheckeredDropPiece();

			Assert.IsTrue(dp.Update(OneMs, DropPiece.MoveDirection.Right, (_, __, ___) => true,
                                    (_) => { }, (_) => false, (_, __) => { }));
			Assert.IsFalse(dp.FastDrop);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][0]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][0]);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][1]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][1]);

			TestPosition(dp, 1, 0);
		}

		[Test()]
		public void TestUpdateMoveDown()
		{
			var dp = CreateCheckeredDropPiece();

			Assert.IsFalse(dp.Update(OneMs, DropPiece.MoveDirection.Down, (c, r, i) => true,
			                         (_) => { }, (_) => false, (_, __) => { }));
			Assert.IsTrue(dp.FastDrop);
			Assert.AreEqual(null, dp.DownDropRate);
			Assert.IsFalse(dp.Holding);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][0]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][0]);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][1]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][1]);
			//Assert.AreNotEqual(0.0, dp.PositionLerp[0], 0.0001);
			//Assert.AreNotEqual(0.0, dp.PositionLerp[1], 0.0001);

			TestPosition(dp, 0, 0);
		}

		[Test()]
		public void TestUpdateMoveNone()
		{
			var dp = CreateCheckeredDropPiece();

			Assert.IsFalse(dp.Update(OneMs, DropPiece.MoveDirection.None, (c, r, i) => true,
                                     (_) => { }, (_) => false, (_, __) => { }));
            Assert.IsFalse(dp.FastDrop);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][0]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][0]);
			Assert.AreEqual(Cell.States.Black, dp.Cells[0][1]);
			Assert.AreEqual(Cell.States.White, dp.Cells[1][1]);


			TestPosition(dp, 0, 0);
		}

		[Test()]
		public void TestDeepClone()
		{
			var dp1 = new DropPiece(new DropPieceSimple());
			var dp2 = dp1.DeepClone();
			Assert.AreNotSame(dp1, dp2);
			Assert.AreEqual(dp1, dp2);
		}

		[Test()]
		public void TestEquality()
		{
			var dp1 = new DropPiece(new DropPieceSimple());
			var dp2 = new DropPiece(new DropPieceSimple());
			Assert.AreEqual(dp1, dp2);
			Assert.AreEqual(dp1, dp1);
			Assert.AreNotEqual(null, dp1);
		}

		[Test()]
		public void TestHashCode()
		{
			var dp1 = new DropPiece(new DropPieceSimple());
			var dp2 = new DropPiece(new DropPieceSimple());
			Assert.AreNotEqual(dp1.GetHashCode(), dp2.GetHashCode());
		}
	}
}
#endif // ENABLE_UNITTESTS
