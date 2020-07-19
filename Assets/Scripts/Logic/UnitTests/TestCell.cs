#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;
using Logic;

namespace Logic.Tests
{
    [TestFixture()]
    public class TestCell
    {
        [Test()]
        public void TestConstructor()
        {
            var cell = new Cell(0, 0);
            Assert.AreEqual(0, cell.Row);
            Assert.AreEqual(0, cell.Column);
            Assert.AreEqual(1, cell.LockCount);
            Assert.False(cell.IsOccupied);
            Assert.AreEqual(Cell.RemoveStates.NotRemoved, cell.RemoveState);
        }

        [Test()]
        public void TestConstructorBadIndices()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var cell = new Cell(-1, -1);
                Assert.IsNotNull(cell);
            });
        }

        [Test(), Sequential]
        public void TestWhiteState(
            [Values(Cell.States.White, Cell.States.WhiteJeweledBoth)] Cell.States state)
        {
            var cell = new Cell(0, 0)
            {
                State = state
            };

            Assert.IsTrue(cell.IsWhite);
            Assert.IsTrue(Cell.IsStateWhite(cell.State));
            Assert.IsTrue(cell.IsSameColor(Cell.States.White));
            Assert.IsFalse(cell.IsBlack);
            Assert.IsFalse(Cell.IsStateBlack(cell.State));
            Assert.IsFalse(cell.IsSameColor(Cell.States.Black));
            if (state == Cell.States.WhiteJeweledBoth)
                Assert.IsTrue(cell.IsJeweled);
            else
                Assert.IsFalse(cell.IsJeweled);
        }

        [Test(), Sequential]
        public void TestBlackState(
            [Values(Cell.States.Black, Cell.States.BlackJeweledBoth)] Cell.States state)
        {
            var cell = new Cell(0, 0)
            {
                State = state
            };

            Assert.IsTrue(cell.IsBlack);
            Assert.IsTrue(Cell.IsStateBlack(cell.State));
            Assert.IsTrue(cell.IsSameColor(Cell.States.Black));
            Assert.IsFalse(cell.IsWhite);
            Assert.IsFalse(Cell.IsStateWhite(cell.State));
            Assert.IsFalse(cell.IsSameColor(Cell.States.White));
            if (state == Cell.States.BlackJeweledBoth)
                Assert.IsTrue(cell.IsJeweled);
            else
                Assert.IsFalse(cell.IsJeweled);
        }

        [Test()]
        public void TestBlackAndWhiteState()
        {
            {
                var cell = new Cell(0, 0)
                {
                    State = Cell.States.BlackAndWhite
                };

                Assert.IsTrue(cell.IsBlack);
                Assert.IsTrue(cell.IsWhite);
                Assert.IsFalse(cell.IsJeweled);
            }
        }

        [Test()]
        public void TestRemoveSuccess()
        {
            var cell = new Cell(0, 0)
            {
                State = Cell.States.White
            };
            Assert.IsTrue(cell.RemoveCell());
            Assert.AreEqual(1, cell.LockCount);
            Assert.AreEqual(Cell.States.Empty, cell.State);
        }

        [Test()]
        public void TestRemoveFail()
        {
            var cell = new Cell(0, 0, 3)
            {
                State = Cell.States.White
            };
            Assert.IsFalse(cell.RemoveCell());
            Assert.AreEqual(2, cell.LockCount);
            Assert.AreEqual(Cell.States.White, cell.State);
        }

        [Test()]
        public void TestEquality()
        {
            var cell1 = new Cell(0, 0)
            {
                State = Cell.States.Black
            };
            var cell2 = new Cell(0, 0)
            {
                State = Cell.States.BlackAndWhite
            };
            var cell3 = new Cell(0, 0)
            {
                State = Cell.States.White
            };

            Assert.AreNotEqual(cell1, cell2);
            Assert.AreNotSame(cell1, cell2);

            Assert.AreNotEqual(cell2, cell3);
            Assert.AreNotSame(cell2, cell3);
        }

        [Test()]
        public void TestInequalityWithNull()
        {
            Assert.AreNotEqual(new Cell(0, 0), null);
        }


        [Test()]
        public void TestInequality()
        {
            {
                var cell1 = new Cell(0, 0)
                {
                    State = Cell.States.Black
                };
                var cell2 = new Cell(1, 1)
                {
                    State = Cell.States.Black
                };

                Assert.AreNotEqual(cell1, cell2);
                Assert.AreNotEqual(cell1, cell2);
            }

            {
                var cell1 = new Cell(0, 0)
                {
                    State = Cell.States.Black
                };
                var cell2 = new Cell(0, 0)
                {
                    State = Cell.States.White
                };

                Assert.AreNotEqual(cell1, cell2);
            }
        }

        [Test()]
        public void TestRemoveStateEventGetsCalled()
        {
            bool eventCalled = false;
            var cell = new Cell(0, 0);
            cell.RemoveStateChanged += (sender, e) => eventCalled = true;
            cell.RemoveState = Cell.RemoveStates.Removing;
            Assert.IsTrue(eventCalled);
            eventCalled = false;
            cell.RemoveState = Cell.RemoveStates.Removing;
            Assert.IsFalse(eventCalled);
        }

        [Test()]
        public void TestTranferredEventGetsCalled()
        {
            bool eventCalled = false;
            var cell1 = new Cell(0, 0);
            var cell2 = new Cell(1, 1);
            cell1.Transferred += (sender, e) => eventCalled = true;
            cell1.TransferTo(cell2);
            Assert.IsTrue(eventCalled);
        }

        [Test()]
        public void TestHashCode()
        {
            var cell1 = new Cell(0, 0);
            var cell2 = new Cell(1, 2);
            Assert.AreNotEqual(cell1.GetHashCode(), cell2.GetHashCode());
        }
    }
}
#endif // ENABLE_UNITTESTS
