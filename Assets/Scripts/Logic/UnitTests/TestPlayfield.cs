#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;
using System.Linq;
using Logic;

namespace Logic.Tests
{
    [TestFixture()]
    public class TestPlayfield
    {
        [Test()]
        public void TestConstructorBadSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var pf = new Playfield(1, 1);
                Assert.IsNotNull(pf);
            });
        }

        [Test(), Sequential]
        public void TestConstructor(
            [Values(2, 4)] int columns,
            [Values(2, 6)] int rows)
        {
            var pf = new Playfield(columns, rows);

            Assert.AreEqual(columns, pf.NumCellColumns);
            Assert.AreEqual(rows, pf.NumCellRows);
            Assert.IsTrue(pf.GetEnumeratorCells().All(c => c.State == Cell.States.Empty));

            Assert.AreEqual(columns - 1, pf.NumSquareColumns);
            Assert.AreEqual(rows - 1, pf.NumSquareRows);

            for (int c = 0; c < pf.NumSquareColumns; c++)
            {
                for (int r = 0; r < pf.NumSquareRows; r++)
                {
                    Assert.AreEqual(c, pf.Squares[c][r].Column);
                    Assert.AreEqual(r, pf.Squares[c][r].Row);
                }
            }
        }

        /*[Test()]
		public void TestDeepClone()
		{
			var pf1 = new Playfield(2, 2);

			var pf2 = pf1.DeepClone();
			Assert.AreNotSame(pf1, pf2);
			Assert.AreEqual(pf1, pf2);
			Assert.AreNotSame(pf1.Cells, pf2.Cells);
			Assert.AreNotSame(pf1.Squares, pf2.Squares);
		}
        */

        [Test()]
        public void TestColumnRowAccessors()
        {
            var pf = new Playfield(4, 6);
            Assert.AreEqual(4, pf.NumCellColumns);
            Assert.AreEqual(6, pf.NumCellRows);

            Assert.AreEqual(3, pf.NumSquareColumns);
            Assert.AreEqual(5, pf.NumSquareRows);
        }

        [Test()]
        public void TestIsValid()
        {
            var pf = new Playfield(2, 2);
            Assert.IsTrue(pf.IsValid(0, 0));
            Assert.IsTrue(pf.IsValid(1, 0));
            Assert.IsTrue(pf.IsValid(0, 1));
            Assert.IsTrue(pf.IsValid(1, 1));

            Assert.IsFalse(pf.IsValid(-1, 0));
            Assert.IsFalse(pf.IsValid(0, -1));
            Assert.IsFalse(pf.IsValid(-1, -1));
            Assert.IsFalse(pf.IsValid(2, 0));
            Assert.IsFalse(pf.IsValid(0, 2));
            Assert.IsFalse(pf.IsValid(2, 2));
        }

        [Test(), Sequential]
        public void TestCheckConnectedNotConnected(
            [Values(2, 4, 10)] int columns,
            [Values(2, 4, 10)] int rows)
        {
            var pf = new Playfield(columns, rows);
            for (int c = 0; c < columns - 1; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    pf.GetCell(c, r).State = Cell.States.Black;
                }
            }

            for (int r = 0; r < rows; r++)
            {
                pf.GetCell(columns - 1, r).State = Cell.States.White;
            }

            var cs = new Playfield.CheckState(columns, rows, c => c.IsBlack);

            for (int c = 0; c < columns - 1; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    Assert.IsFalse(pf.CheckConnected(pf.GetCell(c, r), columns - 1, cs));
                }
            }
        }

        [Test(), Sequential]
        public void TestCheckConnectedIsConnected(
            [Values(2, 4)] int columns,
            [Values(2, 4)] int rows)
        {
            var pf = new Playfield(columns, rows);
            pf.GetCell(0, 1).State = Cell.States.White;
            pf.GetCell(1, 1).State = Cell.States.White;

            for (int i = 0; i < columns; i++)
            {
                pf.GetCell(i, 0).State = Cell.States.Black;
            }

            var cs = new Playfield.CheckState(columns, rows, c => c.IsBlack);
            Assert.IsTrue(pf.CheckConnected(pf.GetCell(0, 0), columns - 1, cs));
        }

        [Test(), Sequential]
        public void TestCheckConnectedIsConnectedDown(
            [Values(2, 4)] int columns,
            [Values(2, 4)] int rows)
        {
            var pf = new Playfield(columns, rows);
            for (int r = 0; r < rows; r++)
            {
                pf.GetCell(0, r).State = Cell.States.Black;
            }
            for (int c = 1; c < columns; c++)
            {
                pf.GetCell(c, 0).State = Cell.States.Black;
            }
            var cs = new Playfield.CheckState(columns, rows, c => c.IsBlack);
            Assert.IsTrue(pf.CheckConnected(pf.GetCell(0, rows - 1), columns - 1, cs));
        }

        [Test(), Sequential]
        public void TestCheckConnectedIsConnectedUp(
            [Values(2, 4)] int columns,
            [Values(2, 4)] int rows)
        {
            var pf = new Playfield(columns, rows);
            for (int r = 0; r < rows; r++)
            {
                pf.GetCell(0, r).State = Cell.States.Black;
            }
            for (int c = 1; c < columns; c++)
            {
                pf.GetCell(c, rows - 1).State = Cell.States.Black;
            }
            var cs = new Playfield.CheckState(columns, rows, c => c.State == pf.GetCell(0, 0).State);
            Assert.IsTrue(pf.CheckConnected(pf.GetCell(0, 0), columns - 1, cs));
        }

        [Test(), Sequential]
        public void TestCheckConnectedBackwardsConnected(
                [Values(4)] int columns,
                [Values(4)] int rows)
        {
            var pf = new Playfield(columns, rows);
            for (int r = 0; r < rows; r++)
            {
                pf.GetCell(0, r).State = Cell.States.Black;
            }
            for (int c = 1; c < columns; c++)
            {
                pf.GetCell(c, rows - 1).State = Cell.States.Black;
            }
            var cellToCheck = pf.GetCell(2, 1);
            cellToCheck.State = Cell.States.Black;
            var cs = new Playfield.CheckState(columns, rows, c => c.State == cellToCheck.State);
            Assert.IsFalse(pf.CheckConnected(cellToCheck, columns - 1, cs));
        }

        [Test(), Sequential]
        public void TestCheckConnectedForceState(
                [Values(2, 4)] int columns,
                [Values(2, 4)] int rows)
        {
            var pf = new Playfield(columns, rows);
            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    pf.GetCell(c, r).State = Cell.States.White;
                }
            }
            pf.GetCell(columns - 1, rows - 1).State = Cell.States.WhiteJeweledBoth;

            var cs = new Playfield.CheckState(columns, rows);
            Assert.IsFalse(pf.CheckConnected(pf.GetCell(0, 0), 1, cs));
            cs.Predicate = c => { if (c.State == Cell.States.WhiteJeweledBoth) cs.FoundJewel = true; return c.IsWhite; };
            Assert.IsTrue(pf.CheckConnected(pf.GetCell(0, 0), columns - 1, cs));
            Assert.IsTrue(cs.FoundJewel);
        }


        [Test(), Sequential]
        public void TestGetContainingSquare(
            [Values(2, 3, 2, 4)] int columns,
            [Values(2, 2, 3, 4)] int rows)
        {
            var pf = new Playfield(columns, rows);

            for (int c = 0; c < pf.NumCellColumns; c++)
            {
                for (int r = 0; r < pf.NumCellRows; r++)
                {
                    var cell = pf.Cells[c][r];
                    var ul = pf.GetContainingSquare(cell, Playfield.SquarePosition.UpperLeft);
                    Assert.AreEqual(ul, c >= pf.NumSquareColumns || r >= pf.NumSquareRows ? null : pf.Squares[c][r]);
                    var ll = pf.GetContainingSquare(cell, Playfield.SquarePosition.LowerLeft);
                    Assert.AreEqual(ll, c >= pf.NumSquareColumns || r <= 0 ? null : pf.Squares[c][r - 1]);
                    var ur = pf.GetContainingSquare(cell, Playfield.SquarePosition.UpperRight);
                    Assert.AreEqual(ur, c <= 0 || r >= pf.NumSquareRows ? null : pf.Squares[c - 1][r]);
                    var lr = pf.GetContainingSquare(cell, Playfield.SquarePosition.LowerRight);
                    Assert.AreEqual(lr, c <= 0 || r <= 0 ? null : pf.Squares[c - 1][r - 1]);
                }
            }
        }

        [Test()]
        public void TestAdjacentCell()
        {
            var pf = new Playfield(3, 3);

            for (int c = 0; c < pf.NumCellColumns; c++)
            {
                for (int r = 0; r < pf.NumCellRows; r++)
                {
                    var currCell = pf.Cells[c][r];
                    Cell expected = pf.IsValid(c - 1, r) ? pf.Cells[c - 1][r] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.Left));
                    expected = pf.IsValid(c + 1, r) ? pf.Cells[c + 1][r] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.Right));
                    expected = pf.IsValid(c, r - 1) ? pf.Cells[c][r - 1] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.Up));
                    expected = pf.IsValid(c, r + 1) ? pf.Cells[c][r + 1] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.Down));
                    expected = pf.IsValid(c - 1, r - 1) ? pf.Cells[c - 1][r - 1] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.UpLeft));
                    expected = pf.IsValid(c + 1, r - 1) ? pf.Cells[c + 1][r - 1] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.UpRight));
                    expected = pf.IsValid(c - 1, r + 1) ? pf.Cells[c - 1][r + 1] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.DownLeft));
                    expected = pf.IsValid(c + 1, r + 1) ? pf.Cells[c + 1][r + 1] : null;
                    Assert.AreEqual(expected, pf.AdjacentCell(currCell, Playfield.Position.DownRight));
                }
            }
        }

        [Test()]
        public void TestCompletedSquare()
        {
            var pf = new Playfield(4, 6);
            pf.Cells[0][0].State = Cell.States.Black;
            Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);
            pf.Cells[0][1].State = Cell.States.Black;
            Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);
            pf.Cells[1][0].State = Cell.States.Black;
            Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Squares[0][0].UpperLeft.RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Squares[0][0].UpperRight.RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Squares[0][0].LowerLeft.RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Squares[0][0].LowerRight.RemoveState);

            pf.Cells[1][1].State = Cell.States.Black;
            Assert.AreEqual(Square.States.Completing, pf.Squares[0][0].State);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Squares[0][0].UpperLeft.RemoveState);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Squares[0][0].UpperRight.RemoveState);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Squares[0][0].LowerLeft.RemoveState);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Squares[0][0].LowerRight.RemoveState);
        }

#if CRAP
		[Test()]
		public void TestRemovedSquare()
		{
			var pf = new Playfield(3, 2);
			Assert.IsFalse(pf.Squares[0][0].CanRemove(0));
			Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);
			pf.Cells[0][0].State = pf.Cells[0][1].State = pf.Cells[1][0].State = pf.Cells[1][1].State = Cell.States.Black;
			pf.Cells[2][0].State = pf.Cells[2][1].State = Cell.States.Black;

			Assert.AreEqual(Square.States.Completing, pf.Squares[0][0].State);
			Assert.AreEqual(Square.States.Completing, pf.Squares[1][0].State);

			// make sure connected squares are not removed:
			Assert.IsFalse(pf.Squares[0][0].CanRemove(0));
			Assert.AreEqual(Square.States.Completing, pf.Squares[0][0].State);
			Assert.IsTrue(pf.Squares[1][0].CanRemove(0));
			pf.Squares[1][0].Remove();
			Assert.AreEqual(Square.States.None, pf.Squares[1][0].State);
			Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);

			// should already be removed:
			Assert.IsFalse(pf.Squares[0][0].CanRemove(0));
			Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);
		}

		[Test()]
		public void TestRemovedSquareInMiddle()
		{
			var pf = new Playfield(3, 2);
			Assert.IsFalse(pf.Squares[0][0].CanRemove(0));
			Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);
			pf.Cells[0][0].State = pf.Cells[0][1].State = pf.Cells[1][0].State = pf.Cells[1][1].State = Cell.States.Black;
			pf.Cells[2][0].State = pf.Cells[2][1].State = Cell.States.Black;

			Assert.AreEqual(Square.States.Completing, pf.Squares[0][0].State);
			Assert.AreEqual(Square.States.Completing, pf.Squares[1][0].State);

			// make sure connected squares are not removed:
			Assert.IsFalse(pf.Squares[0][0].CanRemove(0));
			Assert.AreEqual(Square.States.Completing, pf.Squares[0][0].State);
			Assert.IsTrue(pf.Squares[1][0].CanRemove(0));
			pf.Squares[1][0].Remove();
			Assert.AreEqual(Square.States.None, pf.Squares[1][0].State);
			Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);

			// should already be removed:
			Assert.IsFalse(pf.Squares[0][0].CanRemove(0));
			Assert.AreEqual(Square.States.None, pf.Squares[0][0].State);
		}
#endif

        [Test()]
        public void TestDisabledDoesNotFall()
        {
            // ---------
            // | W | D | 
            // ---------
            // | D | W | 
            // +---+---+
            // | B | B | 
            // +---+---+
            // | B | B | 
            // +---+---+
            const int numColumns = 2;
            var pf = new Playfield(numColumns, 4);
            pf.Cells[0][2].State = pf.Cells[1][2].State = Cell.States.Black;
            pf.Cells[0][3].State = pf.Cells[1][3].State = Cell.States.Black;

            pf.Cells[0][0].State = Cell.States.White;
            pf.Cells[0][1].State = Cell.States.Disabled;
            pf.Cells[1][0].State = Cell.States.Disabled;
            pf.Cells[1][1].State = Cell.States.White;

            // start it:
            pf.Timeline.IncrementPosition(0.01f);
            Assert.AreEqual(0, pf.Timeline.Column);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][3].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Cells[1][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Cells[1][3].RemoveState);

            double columnIncr = 1.0 / numColumns;
            pf.Timeline.IncrementPosition(columnIncr);
            Assert.AreEqual(1, pf.Timeline.Column);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][3].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[1][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[1][3].RemoveState);

            pf.Timeline.IncrementPosition(columnIncr);
            Assert.AreEqual(0, pf.Timeline.Column);

            // ---------
            // | W | D | 
            // ---------
            // | D | E | 
            // +---+---+
            // | E | E | 
            // +---+---+
            // | E | W | 
            // +---+---+

            // expect cells to be empty:
            Assert.AreEqual(Cell.States.Empty, pf.Cells[1][1].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[1][2].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[0][2].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[0][3].State);

            // filled cells:
            Assert.AreEqual(Cell.States.White, pf.Cells[0][0].State);
            Assert.AreEqual(Cell.States.Disabled, pf.Cells[1][0].State);
            Assert.AreEqual(Cell.States.Disabled, pf.Cells[0][1].State);
            Assert.AreEqual(Cell.States.White, pf.Cells[1][3].State);
        }

        [Test()]
        public void TestSetJewelNotInSquare()
        {
            // ---------
            // | B | W | 
            // +---+---+
            // | W | J | 
            // +---+---+
            const int numColumns = 2;
            var pf = new Playfield(numColumns, 2);
            pf.Cells[0][0].State = Cell.States.Black;
            pf.Cells[0][1].State = pf.Cells[1][0].State = Cell.States.White;
            pf.Cells[1][1].State = Cell.States.WhiteJeweledBoth;
            pf.JeweledCells.Add(pf.Cells[1][1]);

            pf.ProcessJewel(true);

            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[0][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[0][1].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[1][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[1][1].RemoveState);
        }

        [Test(), Sequential]
        public void TestSingleeColorTrigger(
            [Values(2, 3)] int columns,
            [Values(3, 3)] int rows)
        {
            // 3x2
            // ---------
            // | E | B | 
            // +---+---+
            // | W | W | 
            // +---+---+
            // | W | W | 
            // +---+---+

            // 4x4
            // -------------
            // | E | B | E | 
            // +---+---+---+
            // | W | W | E | 
            // +---+---+---+
            // | W | W | E | 
            // +---+---+---+
            var pf = new Playfield(columns, rows);
            pf.Cells[0][1].State =
                pf.Cells[1][1].State =
                pf.Cells[0][2].State =
                pf.Cells[1][2].State =
                Cell.States.White;
            pf.Cells[1][0].State = Cell.States.Black;

            bool eventTriggered = false;
            pf.Stats.OnColorBonus += () => eventTriggered = true;
            pf.Timeline.IncrementPosition(0.001f); //HACK: need to start the timeline
            pf.Timeline.IncrementPosition(3.0f / columns - 0.5f / columns);
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(1, pf.Stats.TotalNumSingleColorBonuses);
        }

        [Test(), Sequential]
        public void TestEmptyTrigger(
            [Values(2, 3)] int columns,
            [Values(2, 3)] int rows)
        {
            // 2x2
            // +---+---+
            // | W | W | 
            // +---+---+
            // | W | W | 
            // +---+---+

            // 3x3
            // -------------
            // | E | E | E | 
            // +---+---+---+
            // | W | W | E | 
            // +---+---+---+
            // | W | W | E | 
            // +---+---+---+
            const int numColumns = 3;
            var pf = new Playfield(numColumns, 3);
            pf.Cells[0][rows - 2].State =
                pf.Cells[1][rows - 2].State =
                pf.Cells[0][rows - 1].State =
                pf.Cells[1][rows - 1].State =
                Cell.States.White;


            bool eventTriggered = false;
            pf.Stats.OnEmptyBonus += () => eventTriggered = true;
            pf.Timeline.IncrementPosition(0.001f); //HACK: need to start the timeline
            pf.Timeline.IncrementPosition(3.0f / columns - 0.5f / columns);
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(1, pf.Stats.TotalNumEmptyColorBonuses);
        }

        //TODO: fix test [Test()]
        public void TestSetJewelRemoveState()
        {
            // -------------
            // | B | W | B | 
            // +---+---+---+
            // | W | J | W | 
            // +---+---+---+
            // | W | W | B | 
            // +---+---+---+
            const int numColumns = 3;
            var pf = new Playfield(numColumns, 3);
            pf.Cells[0][0].State = pf.Cells[2][0].State = Cell.States.Black;
            pf.Cells[2][2].State = Cell.States.Black;

            pf.Cells[1][1].State = Cell.States.WhiteJeweledBoth;
            pf.JeweledCells.Add(pf.Cells[1][1]);

            pf.Cells[0][1].State = pf.Cells[0][2].State = Cell.States.White;
            pf.Cells[1][0].State = pf.Cells[1][2].State = Cell.States.White;
            pf.Cells[2][1].State = Cell.States.White;

            pf.Timeline.IncrementPosition(0.0001f);
            Assert.AreEqual(0, pf.Timeline.Column);

            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[0][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[2][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[2][2].RemoveState);

            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][1].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelWillBeRemoved, pf.Cells[1][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Cells[1][1].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.WillBeRemoved, pf.Cells[1][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelWillBeRemoved, pf.Cells[2][1].RemoveState);

            pf.Timeline.IncrementPosition(1.0 / numColumns);
            Assert.AreEqual(1, pf.Timeline.Column);

            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[0][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[2][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[2][2].RemoveState);

            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][1].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[0][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelRemoving, pf.Cells[1][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[1][1].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.Removing, pf.Cells[1][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelWillBeRemoved, pf.Cells[2][1].RemoveState);

            pf.Timeline.IncrementPosition(1.0 / numColumns);
            Assert.AreEqual(2, pf.Timeline.Column);

            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[0][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[2][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, pf.Cells[2][2].RemoveState);

            Assert.AreEqual(Cell.RemoveStates.JewelRemoving, pf.Cells[0][1].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelRemoving, pf.Cells[0][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelRemoving, pf.Cells[1][0].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelRemoving, pf.Cells[1][1].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelRemoving, pf.Cells[1][2].RemoveState);
            Assert.AreEqual(Cell.RemoveStates.JewelRemoving, pf.Cells[2][1].RemoveState);


            pf.Timeline.IncrementPosition(1.0 / numColumns);
            Assert.AreEqual(0, pf.Timeline.Column);

            // -------------
            // | E | E | E | 
            // +---+---+---+
            // | E | E | B | 
            // +---+---+---+
            // | B | E | B | 
            // +---+---+---+
            Assert.IsTrue(pf.GetEnumeratorCells().All(c => c.RemoveState == Cell.RemoveStates.NotRemoved));

            // black cells:
            Assert.AreEqual(Cell.States.Black, pf.Cells[0][2].State);
            Assert.AreEqual(Cell.States.Black, pf.Cells[2][1].State);
            Assert.AreEqual(Cell.States.Black, pf.Cells[2][2].State);

            // expect cells to be empty:
            Assert.AreEqual(Cell.States.Empty, pf.Cells[0][0].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[0][1].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[1][0].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[1][1].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[1][2].State);
            Assert.AreEqual(Cell.States.Empty, pf.Cells[2][0].State);
        }
    }
}
#endif // ENABLE_UNITTESTS

