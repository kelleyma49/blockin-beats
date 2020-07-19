#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;
using Logic;

namespace Logic.Tests
{
	[TestFixture()]
	public class TestTimeline
	{
		[Test()]
		public void TestConstructor()
		{
			var t = new Timeline(5);
			Assert.AreEqual(0, t.Column);
			Assert.AreEqual(0.0, t.Position);
			Assert.AreEqual(0.0, t.PositionAbs);
		}

		[Test()]
		public void TestDeepClone()
		{
			var t1 = new Timeline(5);
			t1.IncrementPosition(0.5f);
			var t2 = t1.DeepClone();
			Assert.AreNotSame(t1, t2);
			Assert.AreEqual(t1, t2);
		}

		[Test()]
		public void TestColumn()
		{
			var t = new Timeline(10);
			int newColumnTriggeredNumTimes = 0;
			int wrapColumnTriggeredNumTimes = 0;
			t.NewColumn += (s, e) => ++newColumnTriggeredNumTimes;
			t.WrapColumn += (s, e) => ++wrapColumnTriggeredNumTimes;
			Assert.AreEqual(0, t.Column);

			t.IncrementPosition(0.5f);
			Assert.AreEqual(6, newColumnTriggeredNumTimes);
			Assert.AreEqual(5, t.Column);
			newColumnTriggeredNumTimes = 0;
			wrapColumnTriggeredNumTimes = 0;

			// wrap:
			t.IncrementPosition(0.75f);
			Assert.AreEqual(7, newColumnTriggeredNumTimes);
			Assert.AreEqual(1, wrapColumnTriggeredNumTimes);
			Assert.AreEqual(2, t.Column);
			newColumnTriggeredNumTimes = 0;
			wrapColumnTriggeredNumTimes = 0;
		}
	}
}
#endif // ENABLE_UNITTESTS
