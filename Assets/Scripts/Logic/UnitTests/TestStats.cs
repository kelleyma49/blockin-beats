#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;

namespace UnitTests
{
	[TestFixture()]
	public class TestStats
	{
		[Test()]
		public void TestAddCompletedSquare()
		{
			var s = new Logic.Stats();
			Assert.AreEqual(0, s.FrameSquaresRemoved);
			Assert.AreEqual(0, s.TotalSquaresRemoved);
			s.AddCompletedSquare();
			Assert.AreEqual(1, s.FrameSquaresRemoved);
			Assert.AreEqual(1, s.TotalSquaresRemoved);
			s.IncrementFrame();
			Assert.AreEqual(0, s.FrameSquaresRemoved);
			Assert.AreEqual(1, s.TotalSquaresRemoved);
		}

		[Test()]
		public void TestSquareBonus()
		{
            var s = new Logic.Stats
            {
                FrameSquaresRemoved = Logic.Stats.NumSquaresForBonus + 1
            };
            s.IncrementFrame();
			Assert.AreEqual(1, s.NumPrevBonuses);
			Assert.AreEqual(160, s.TotalSquaresBonuses);
		}

        [Test()]
        public void TestSingleColorBonus()
        {
            var s = new Logic.Stats();
            Assert.AreEqual(0, s.TotalNumSingleColorBonuses);
            s.AddSingleColorBonus();
            Assert.AreEqual(1,s.TotalNumSingleColorBonuses);
        }

        [Test()]
        public void TestSingleColorBonusEventTriggered()
        {
            var s = new Logic.Stats();
            bool triggered = false;
            s.OnColorBonus += () => triggered = true;
            s.AddSingleColorBonus();
            Assert.IsTrue(triggered);
        }

        [Test()]
        public void TestEmptyColorBonus()
        {
            var s = new Logic.Stats();
            Assert.AreEqual(0, s.TotalNumEmptyColorBonuses);
            s.AddEmptyColorBonus();
            Assert.AreEqual(1,s.TotalNumEmptyColorBonuses);
        }

        [Test()]
        public void TestEmptyColorBonusEventTriggered()
        {
            var s = new Logic.Stats();
            bool triggered = false;
            s.OnEmptyBonus += () => triggered = true;
            s.AddEmptyColorBonus();
            Assert.IsTrue(triggered);
        }

        [Test()]
		public void TestEquality()
		{
			var s1 = new Logic.Stats();
			var s2 = new Logic.Stats();
			Assert.AreEqual(s1, s2);
			Assert.AreEqual(s1, s1);
			Assert.AreNotEqual(null, s2);
			Assert.AreNotEqual(1, s2);
		}

		[Test()]
		public void TestHasCode()
		{
            var s1 = new Logic.Stats
            {
                TotalSquaresBonuses = 100
            };
            var s2 = new Logic.Stats
            {
                TotalSquaresBonuses = 200
            };
            Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode());
		}

		[Test()]
		public void TestDeepClone()
		{
			var s1 = new Logic.Stats
			{
				Frame = 1,
				FrameSquaresRemoved = 2,
				NumPrevBonuses = 3,
				TotalSquaresRemoved = 4,
				TotalSquaresBonuses = 5,
				TotalNumSingleColorBonuses = 6,
				TotalNumEmptyColorBonuses = 7
			};

			var s2 = s1.DeepClone();
			Assert.AreNotSame(s1, s2);
			Assert.AreEqual(s1, s2);
		}
	}
}
#endif // ENABLE_UNITTESTS
